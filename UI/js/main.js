/**
 * Created by larryw on 4/7/18.
 */

window.addEventListener('load', function() {
    //console.log('All assets are loaded')
    let wavesurfer = WaveSurfer.create({
        container: '#waveform',
        waveColor: 'violet',
        progressColor: 'purple'
    });


    function loadAudio(name){

        wavesurfer.load('audio/' + name + '.wav');
        isLooping = false;
        loopStart = 0;
        loopEnd = 1000;
    }

    let beatData = null;

    let loadReg = document.getElementById("loadingRegular");
    let loadBeat = document.getElementById("loadingBeatDetect");

    loadReg.style.display = 'none';
    loadBeat.style.display = 'none';

    getRequest('songlist', (songs) => {
        console.log(songs);
        let select = document.getElementById('songList');
        songs.forEach(song => {
            let opt = document.createElement('option');
            opt.value = song;
            opt.innerHTML = song;
            select.appendChild(opt)
        });
        select.addEventListener("change", () =>{
            //var selectedText = select.options[select.selectedIndex].innerHTML;
            var selectedValue = select.value;
            loadSong(selectedValue);
        });

        loadSong(songs[0]);
    });

    function loadSong(name){
        loadBeat.style.display = '';
        //loadReg.style.display = 'none';

        getRequest('beatDetect/' + name, (data) => {
            loadBeat.style.display = 'none';

            console.log(data);
            beatData = data;
            loadAudio(name);
        })
    }

    function argMin(arr) {
        if (arr.length === 0) {
            return -1;
        }
        var max = arr[0];//these variables should be min haha
        var maxIndex = 0;

        for (var i = 1; i < arr.length; i++) {
            if (arr[i] < max) {
                maxIndex = i;
                max = arr[i];
            }
        }

        return maxIndex;
    }
    function findNearestBeat(time){

        let diffs = beatData.beats.map(beat => {
            return Math.abs(beat - time);
        });
        let closest = beatData.beats[argMin(diffs)];
        return closest;
    }





    let playBtn = document.getElementById("playBtn");
    let pauseBtn = document.getElementById("pauseBtn");
    let loopOnBtn = document.getElementById("loopOnBtn");
    let loopOffBtn = document.getElementById("loopOffBtn");
    let loopStartSetBtn = document.getElementById("loopStartSetBtn");
    let loopEndSetBtn = document.getElementById("loopEndSetBtn");
    let hostIpInput = document.getElementById("hostIp");

    const processedMessages = new Set();

    //let hostIp = hostIpInput.value;
    //console.log("Pinging", hostIp);

    let isLooping = false;

    let loopMs = 500;

    let loopStart = 0;
    let loopEnd = 1000;
    let beatAlign = true;
    //API
    //loadAudio('snow');

    function play(){

        console.log("PLAY");
        wavesurfer.play();
    }
    function pause(){
        console.log("PAUSE");
        wavesurfer.pause();
    }
    function reset(){
        isLooping = false;

    }
    function setLoopStart(ts){
        loopStart = wavesurfer.getCurrentTime();

        let aligned = findNearestBeat(loopStart);
        console.log("User", loopStart);

        console.log(aligned);
        if(beatAlign){
            loopStart = aligned;
        }
    }
    function setLoopEnd(ts){
        loopEnd = wavesurfer.getCurrentTime();

        let aligned = findNearestBeat(loopEnd);

        if(beatAlign){
            loopEnd = aligned;
        }
        isLooping = true;
    }
    function loopOn(){
        isLooping = true;
    }
    function loopOff(){
        isLooping = false;
    }

    let deleteMe = [];

    function placeBeatMarkers(wave){

        console.log(beatData);

        var totalWidth = wave.scrollWidth;
        console.log(totalWidth);

        deleteMe.forEach(div => {
            div.remove();
        });
        deleteMe = [];

        beatData.beats.forEach((beat) => {
            let div = document.createElement("div");
            div.style.width = "2px";
            div.style.height = "100%";
            div.style.background = "red";
            div.style.color = "white";
            //div.innerHTML = "Hello";
            div.style.position = "absolute";
            div.style.left = totalWidth * beat/wavesurfer.getDuration() + "px";
            console.log(div.style.left);

            wave.appendChild(div);
            deleteMe.push(div);
        });

    }


    wavesurfer.on('ready', function () {
        console.log("AUDIO READY");

        let waveForm = document.getElementById("waveform");

        let wave = waveForm.getElementsByTagName("wave")[0];
        console.log(wave);

        placeBeatMarkers(wave);

        let duration = wavesurfer.getDuration();
        console.log(duration);

        //console.log(waveForm.getElementsByTagName("wave"));


        //wavesurfer.zoom(20);
    });
    wavesurfer.on('audioprocess', function () {
        if(wavesurfer.isPlaying()){
            let t = wavesurfer.getCurrentTime();

            if(isLooping){
                //console.log(t);
                if(t > loopEnd){
                    console.log("seek back");
                    //wavesurfer.pause();
                    wavesurfer.seekTo(loopStart/wavesurfer.getDuration());
                }
            }
        }
    });

    playBtn.addEventListener("click", () => {
        play();
    });
    pauseBtn.addEventListener("click", () => {
        pause();
    });
    loopOnBtn.addEventListener("click", () => {
        //console.log("PAUSE");
        loopOn();
        //wavesurfer.pause();
    });
    loopOffBtn.addEventListener("click", () => {
        //console.log("PAUSE");
        loopOff();
        //wavesurfer.pause();
    });
    loopStartSetBtn.addEventListener("click", ()=>{
        setLoopStart();
    });
    loopEndSetBtn.addEventListener("click", ()=>{
        setLoopEnd();
    });


    function processJson(data){
        data.queue.forEach(message => {
            let id = message.ID;

            //{
            //    "queue": [
            //    {
            //        "Timestamp": 3.937,
            //        "Data": {
            //            "Command": "Pause"
            //        }
            //    }
            //]
            //}

            let msgDat = message.Data;
            let timestamp = message.Timestamp;
            let command = msgDat.Command;

            if(!processedMessages.has(id)){
                //let command = message.command;
                if(command === 'play'){
                    play();
                }
                else if(command === 'pause'){
                    pause();
                }
                else if(command === 'loopOn'){
                    loopOn();
                }else if(command === 'loopOff'){
                    loopOff();
                }
                else if(command === 'setLoopStart'){
                    setLoopStart();
                }else if(command === 'setLoopEnd'){
                    setLoopEnd();
                }
                processedMessages.add(id);
            }
        });
    }

    setInterval(() => {
        //console.log("PING");

        let xhttp = new XMLHttpRequest();
        xhttp.onreadystatechange = function() {

            if (xhttp.readyState == 4) {
                if (xhttp.status == 200) {
                    //console.log(this.responseText);
                    let text = this.responseText;
                    let json = JSON.parse(text);
                    processJson(json);
                } else {
                    //console.log("Error", xhttp.statusText);
                }
            }
        };
        xhttp.open("GET", hostIpInput.value, true);
        xhttp.send();
    }, loopMs);


    function getRequest(url, cb){
        let xhttp = new XMLHttpRequest();
        xhttp.onreadystatechange = function() {

            if (xhttp.readyState == 4) {
                if (xhttp.status == 200) {
                    //console.log(this.responseText);
                    //let text = this.responseText;
                    //let json = JSON.parse(text);
                    //processJson(json);
                    cb(JSON.parse(this.responseText));
                } else {
                    //console.log("Error", xhttp.statusText);
                }
            }
        };
        xhttp.open("GET", url, true);
        xhttp.send();
    }


});

