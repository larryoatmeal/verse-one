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




    wavesurfer.load('audio/click.mp3');

    let playBtn = document.getElementById("playBtn");
    let pauseBtn = document.getElementById("pauseBtn");
    let loopOnBtn = document.getElementById("loopOnBtn");
    let loopOffBtn = document.getElementById("loopOffBtn");
    let loopStartSetBtn = document.getElementById("loopStartSetBtn");
    let loopEndSetBtn = document.getElementById("loopEndSetBtn");
    let hostIpInput = document.getElementById("hostIp");


    //let hostIp = hostIpInput.value;
    //console.log("Pinging", hostIp);

    let isLooping = false;

    let loopMs = 500;

    let loopStart = 0;
    let loopEnd = 1000;
    //API
    function play(){

    }
    function pause(){

    }
    function reset(){
        isLooping = false;

    }
    function setLoopStart(ts){
        loopStart = wavesurfer.getCurrentTime();
    }
    function setLoopEnd(ts){
        loopEnd = wavesurfer.getCurrentTime();
    }
    function loopOn(){
        isLooping = true;
    }
    function loopOff(){
        isLooping = false;
    }

    wavesurfer.on('ready', function () {
        console.log("AUDIO READY");
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
        console.log("PLAY");
        wavesurfer.play();

    });
    pauseBtn.addEventListener("click", () => {
        console.log("PAUSE");
        wavesurfer.pause();
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

    setInterval(() => {
        console.log("PING");


        let xhttp = new XMLHttpRequest();
        xhttp.onreadystatechange = function() {
            if (xhttp.readyState == 4 && xhttp.status == 200) {
                console.log(this.responseText);
            }
        };
        xhttp.open("GET", hostIpInput.value, true);
        xhttp.send();
    }, loopMs);



});

