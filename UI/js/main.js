/**
 * Created by larryw on 4/7/18.
 */
let Instrument = {
    PIANO: "Piano",
    ELECTRIC_PIANO: "Electric Piano",
    ORGAN: "Organ",
};

let SkeletonPositions = {
    TOO_FAR: "You're too far! Please move closer to the Kinect.",
    TOO_NEAR: "You're too near! Please move further from the Kinect",
    OKAY: "Skeleton detected.",
    GONE: "Skeleton not detected."
};

window.addEventListener('load', function () {
    //console.log('All assets are loaded')
    let wavesurfer = WaveSurfer.create({
        container: '#waveform',
        waveColor: 'violet',
        progressColor: 'purple'
    });
    let divA = null;
    let divB = null;

    let container = document.getElementById('container');
    let canvas = document.getElementById('canvas');
    let coordinates = document.getElementById('coordinates');

    drawGrid();

    // function updateCoordinates(e) {
    //   let ctx = canvas.getContext('2d');
    //   ctx.clearRect(0, 0, canvas.width, canvas.height);
    //   var x = e.offsetX;
    //   var y = e.offsetY;
    //   var coords = "X coordinates: " + x + ", Y coordinates: " + y;
    //   coordinates.innerHTML = coords;
    //
    //   drawGrid();
    //   ctx.beginPath();
    //   ctx.arc(x,y,10,0,2*Math.PI);
    //   ctx.fillStyle = 'green';
    //   ctx.fill();
    //   ctx.closePath();
    //   var coords = "X coordinates: " + x + ", Y coordinates: " + y;
    //   coordinates.innerHTML = coords;
    // }

    // canvas.addEventListener("click", updateCoordinates);


    function loadAudio(name) {

        loopOff();
        pause();

        wavesurfer.load('audio/' + name + '.wav');
        //isLooping = false;
        //loopStart = 0;
        //loopEnd = 1000;
    };

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
        select.addEventListener("change", () => {
            //var selectedText = select.options[select.selectedIndex].innerHTML;
            var selectedValue = select.value;
            loadSong(selectedValue);
        });

        loadSong(songs[0]);
    });

    function loadSong(name) {
        loadBeat.style.display = '';
        //loadReg.style.display = 'none';

        getRequest('beatDetect/' + name, (data) => {
            loadBeat.style.display = 'none';

            console.log(data);
            beatData = data;
            loadAudio(name);
        })
    };

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




    function findNearestBeat(time) {

        let diffs = beatData.beats.map(beat => {
            return Math.abs(beat - time);
        });

        let best = argMin(diffs);

        let index = Math.floor(best / 4) * 4;

        let closest = beatData.beats[index];
        return closest;
    }


    function findNearestBeatIndex(time) {

        let diffs = beatData.beats.map(beat => {
            return Math.abs(beat - time);
        });
        return argMin(diffs);
    }

    function findAdjacentBeat(time, step) {
        let i = findNearestBeatIndex(time);
        i = Math.min(Math.max(0, i + step), beatData.beats.length - 1);
        return beatData.beats[i];
    }


    let playBtn = document.getElementById("playBtn");
    let pauseBtn = document.getElementById("pauseBtn");
    let loopOnBtn = document.getElementById("loopOnBtn");
    let loopOffBtn = document.getElementById("loopOffBtn");
    let loopStartSetBtn = document.getElementById("loopStartSetBtn");
    let loopEndSetBtn = document.getElementById("loopEndSetBtn");
    let hostIpInput = document.getElementById("hostIp");
    let calibrationText = document.getElementById("calibrationText");

    const processedMessages = new Set();

    //let hostIp = hostIpInput.value;
    //console.log("Pinging", hostIp);

    let isLooping = false;
    let skeletonDetected = false;

    let loopMs = 50;

    let loopStart = 0;
    let loopEnd = 1000;
    let beatAlign = true;
    //API
    //loadAudio('snow');
    let xySmoothSize = 20;
    let xSmoother = new Smoother(xySmoothSize);
    let ySmoother = new Smoother(xySmoothSize);

    //MIDI related
    let midiOut = null;
    let controlLock = true;

    document.getElementById("playStopButton").addEventListener("click", togglePlay)
    var cycleButton = document.getElementById("cycleOnOffButton");
    cycleButton.addEventListener("click", toggleLoop)

    document.getElementById("setStartButton").addEventListener('click', setLoopStart);
    document.getElementById("setEndButton").addEventListener('click', setLoopEnd);


    document.getElementById("rewindButton").addEventListener('click', rewind);
    document.getElementById("fastForwardButton").addEventListener('click', fastforward);

    let playIcon = document.getElementById('playIcon');
    let pauseIcon = document.getElementById('pauseIcon');


    function setupMidi() {
        WebMidi.enable(function () {

            // Viewing available inputs and outputs
            console.log(WebMidi.inputs);
            console.log(WebMidi.outputs);

            // Retrieve an input by name, id or index
            let input = WebMidi.getInputByName("MK-449C USB MIDI Keyboard");
            let output = WebMidi.getOutputByName("IAC Driver Bus 1");
            midiOut = output;
            // OR...
            // input = WebMidi.getInputById("1809568182");
            // input = WebMidi.inputs[0];

            // Listen for a 'note on' message on all channels
            //input.addListener('noteon', 'all',
            //    function (e) {
            //      console.log("Received 'noteon' message (" + e.note.name + e.note.octave + ").");
            //    }
            //);

            // Listen to pitch bend message on channel 3
            //input.addListener('pitchbend', 3,
            //    function (e) {
            //      console.log("Received 'pitchbend' message.", e);
            //    }
            //);

            // Listen to control change message on all channels
            input.addListener('controlchange', "all",
                function (e) {
                    //console.log("Received 'controlchange' message.", e);
                    console.log(e);
                    console.log(e.controller);
                    if (e.controller.number == 64) {
                        //if(e.con)
                        //controlLock = e.value <= 0;
                        pedalDepressVal = e.value;
                        if (e.value > 0) {
                            pedalPressed();
                        } else {
                            controlLock = true;
                            updateControlLockText();
                        }

                        //send default pedal
                        if (!pedalControlMode) {
                            sendCCChange(64, e.value);
                        }
                    }
                }
            );
        })
    };
    //// Remove all listeners for 'noteoff' on all channels
    //input.removeListener('noteoff');
    //
    //// Remove all listeners on the input
    //input.removeListener();

    function init() {
        setCalibrationModeOff();
    }

    init();
    setupMidi();

    let pedalControlMode = true; //false is traditional pedal
    let pedalHitCount = 0;
    let pedalBounceTime = 500;
    let pedalTimeOut = null;

    let pedalDepressVal = 0;
//looping state
    let CLEAR = 0;
    let LOOP_START_SET = 1;
    let LOOP_END_SET = 2;
    let LOOP_STATE = CLEAR;

    function pedalPressed() {
        if (pedalTimeOut) {
            clearTimeout(pedalTimeOut);
        }

        pedalHitCount += 1;
        pedalTimeOut = setTimeout(() => {
            pedalAction(pedalHitCount);
            pedalHitCount = 0;
            pedalTimeOut = null;
        }, pedalBounceTime);

        //console.log()
        //controlLock = !controlLock;
    }

    function pedalAction(n) {
        //console.log("PEDAL ACTION", n);

        if (n == 3) {
            pedalControlMode = !pedalControlMode;
            updatePedalControlModeText();
        }
        if (n == 2) {
            //set loop points
            if (LOOP_STATE == CLEAR) {
                setLoopStart();
                LOOP_STATE = LOOP_START_SET;
            }
            else if (LOOP_STATE == LOOP_START_SET) {
                setLoopEnd();
                loopOn();
                LOOP_STATE = LOOP_END_SET
            }
            else if (LOOP_STATE == LOOP_END_SET) {
                loopOff();
                loopStart = 0;
                loopEnd = wavesurfer.getDuration();
                LOOP_STATE = CLEAR;
            }
        }
        if (n == 1) {
            if (pedalControlMode && pedalDepressVal == 127) {//make sure user hasn't already released pedal by this point
                controlLock = false;
                updateControlLockText();
            }
        }
    }

    function sendCCChange(cc, val) {
        if (midiOut) {
            midiOut.sendControlChange(cc, val);
        }
    }

    function sendProgramChange(program) {
        if (midiOut) {
            midiOut.sendProgramChange(program);
        }
    }

    wavesurfer.on('finish', function () {
        console.log("FINISHED PLAYING");
        seekToTime(0);
        pause();
    });

    function setCalibrationModeOn() {
        calibrationText.classList.remove("hidden");
    }

    function setCalibrationModeOff() {
        calibrationText.classList.add("hidden");
    }

    function togglePlay() {
        if (wavesurfer.isPlaying()) {
            pause();
        } else {
            play();
        }
    }

    function toggleLoop() {
        if (isLooping) {
            loopOff();
        } else {
            loopOn();
        }
    }

    function play() {

        playIcon.style.display = 'none';
        pauseIcon.style.display = '';
        console.log("PLAY");

        let t = findAdjacentBeat(wavesurfer.getCurrentTime(), -1);
        seekToTime(t);

        wavesurfer.play();
    }

    function pause() {
        console.log("PAUSE");
        wavesurfer.pause();
        playIcon.style.display = '';
        pauseIcon.style.display = 'none';
    }

    function reset() {
        isLooping = false;

    }

    function setLoopStart(ts) {
        loopStart = wavesurfer.getCurrentTime();

        let aligned = findNearestBeat(loopStart);
        console.log("User", loopStart);

        console.log(aligned);
        if (beatAlign) {
            loopStart = aligned;
        }

        if (Math.abs(loopStart - loopEnd) < 0.1) {
            loopEnd = findAdjacentBeat(loopStart, 1)
        }


        setTimePosition(divA, loopStart);


    }

    let currentIndex = 0;
    let songs = ["1_polkadots","2_42", "3_brandy"];
    let select = document.getElementById('songList');

    function nextSong(){
        currentIndex = (currentIndex + 1) % 3;
        select.value = songs[currentIndex];
        loadSong(songs[currentIndex]);
        //loadSong(currentIndex);
    }
    function previousSong(){
        currentIndex = (currentIndex + 3 - 1) % 3;
        select.value = songs[currentIndex];
        loadSong(songs[currentIndex]);

        //loadSong(currentIndex);
    }

    function setLoopEnd(ts, bypassLoopOn) {
        loopEnd = wavesurfer.getCurrentTime();

        let aligned = findNearestBeat(loopEnd);

        if (beatAlign) {
            loopEnd = aligned;
        }
        //isLooping = true;
        //if(loopStart){
        //if(!bypassLoopOn){
        //  loopOn();
        //}
        if (Math.abs(loopStart - loopEnd) < 0.1) {
            loopEnd = findAdjacentBeat(loopStart, 4)
        }

        setTimePosition(divB, loopEnd);

        //}
    }

    function loopOn() {
        isLooping = true;
        cycleButton.classList.remove('notActive');
        updateLoopingText();

    }

    function loopOff() {
        isLooping = false;
        cycleButton.classList.add('notActive');
        updateLoopingText();
    }

    function rewind() {
        seekToTime(findAdjacentBeat(wavesurfer.getCurrentTime(), -8))
    }

    function fastforward() {
        seekToTime(findAdjacentBeat(wavesurfer.getCurrentTime(), 8))
    }

    function seekToTime(t) {
        wavesurfer.seekTo(t / wavesurfer.getDuration());
    }


    let deleteMe = [];

    function setTimePosition(elem, time) {
        let waveForm = document.getElementById("waveform");

        let wave = waveForm.getElementsByTagName("wave")[0];
        let totalWidth = wave.clientWidth;
        elem.style.left = totalWidth * time / wavesurfer.getDuration() + "px";
    }

    function placeMarkers(wave) {

        //console.log(beatData);

        var totalWidth = wave.scrollWidth;
        //console.log(totalWidth);

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
            div.style.left = totalWidth * beat / wavesurfer.getDuration() + "px";
            //console.log(div.style.left);

            //wave.appendChild(div);
            //deleteMe.push(div);
        });


        if (!divA) {
            divA = document.createElement("div");
            divA.classList.add("btn");
            divA.classList.add("front");
            divA.classList.add("btn-primary");
            divA.textContent = "A";
            divA.style.position = 'absolute';
            wave.appendChild(divA);
            setTimePosition(divA, loopStart);

        }

        if (!divB) {
            divB = document.createElement("div");
            divB.classList.add("btn");
            divB.classList.add("front");
            divB.classList.add("btn-info");
            divB.textContent = "B";
            divB.style.position = 'absolute';
            wave.appendChild(divB);
            //console.log(loopEnd);
            setTimePosition(divB, loopEnd);
        }


        //div.style.width = "2px";
        //div.style.height = "100%";
        //div.style.background = "red";
        //div.style.color = "white";
        ////div.innerHTML = "Hello";
        //div.style.position = "absolute";


        //div.style.left = totalWidth * beat/wavesurfer.getDuration() + "px";
        //console.log(div.style.left);

    }


    wavesurfer.on('ready', function () {
        console.log("AUDIO READY");

        let waveForm = document.getElementById("waveform");

        let wave = waveForm.getElementsByTagName("wave")[0];
        console.log(wave);
        //setLoopEnd(wavesurfer.getDuration(), false);

        loopEnd = wavesurfer.getDuration();
        placeMarkers(wave);

        let duration = wavesurfer.getDuration();
        //console.log(duration);

        //console.log(waveForm.getElementsByTagName("wave"));


        //wavesurfer.zoom(20);
    });
    wavesurfer.on('audioprocess', function () {
        if (wavesurfer.isPlaying()) {
            let t = wavesurfer.getCurrentTime();

            if (isLooping) {
                //console.log(t);
                if (t > loopEnd) {
                    console.log("seek back");
                    //wavesurfer.pause();
                    wavesurfer.seekTo(loopStart / wavesurfer.getDuration());
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
    loopStartSetBtn.addEventListener("click", ()=> {
        setLoopStart();
    });
    loopEndSetBtn.addEventListener("click", ()=> {
        setLoopEnd();
    });


    function processJson(data) {
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
            let msgData = message.Data;
            let timestamp = message.Timestamp;
            let command = msgData.Command;


            if (!processedMessages.has(id)) {
                //let command = message.command;
                if (command === 'play') {
                    play();
                }
                else if (command === 'pause') {
                    pause();
                }
                //else if(command === 'loopOn'){
                //  loopOn();
                //}
                else if (command === 'togglePlay') {
                    togglePlay();
                }
                //else if(command === 'toggleLoop'){
                //  toggleLoop();
                //}
                //else if(command === 'loopOff'){
                //  loopOff();
                //}
                //else if(command === 'setLoopStart'){
                //  setLoopStart();
                //}
                //else if(command === 'setLoopEnd'){
                //  setLoopEnd();
                //}
                else if (command === 'forward') {
                    fastforward();
                }
                else if (command === 'reverse') {
                    rewind();
                }
                else if (command === 'calibrateKeyboardHeight') {
                    playSound("audio/calib1.wav")                    
                    setCalibrationModeOn();
                    calibrationText.innerHTML = "Please place your hands on the keyboard. Say NEXT to continue"
                }
                else if (command === 'calibrateHandRaiseHeight') {
                    playSound("audio/calib2.wav")                    
                    setCalibrationModeOn();
                    calibrationText.innerHTML = "Raise both hands as high as possible. Say NEXT to continue"
                }
                else if (command === 'calibrateXY') {
                    playSound("audio/calib3.wav")                    
                    setCalibrationModeOn();
                    calibrationText.innerHTML = "Move your left hand in the xy region you would like to control. Say NEXT to finish"
                }
                else if (command === 'calibrateDone') {
                    playSound("audio/calib4.wav")                    
                    setCalibrationModeOff();
                }
                else if (command == "patchOne") {
                    sendProgramChange(0);
                    updateInstrument(Instrument.PIANO);
                }
                else if (command == "patchTwo") {
                    sendProgramChange(1);
                    updateInstrument(Instrument.ELECTRIC_PIANO);
                }
                else if (command == "patchThree") {
                    sendProgramChange(2);
                    updateInstrument(Instrument.ORGAN);
                }
                else if (command == "Next Song") {
                    nextSong();
                }
                else if (command == "Previous Song") {
                    previousSong();
                }

                processedMessages.add(id);
            }
        });


        let status = data.status;
        let skeletonStatus = status.skeletonStatus;
        if (skeletonStatus == "isTooFar") {
            skeletonDetected = false;
            updateSkeletonText(SkeletonPositions.TOO_FAR);
        }
        else if (skeletonStatus == "isTooNear") {
            skeletonDetected = false;
            updateSkeletonText(SkeletonPositions.TOO_NEAR);
        } else if (skeletonStatus == "skeletonOkay") {
            skeletonDetected = true;
            updateSkeletonText(SkeletonPositions.OKAY);
        } else {
            skeletonDetected = false;
            updateSkeletonText(SkeletonPositions.GONE);
        }

        let xyStatus = status.XY;
        if (xyStatus) {
            let params = xyStatus.split(",");
            let x = parseInt(params[0]);
            let y = parseInt(params[1]);

            xSmoother.feed(x);
            ySmoother.feed(y);

            let xFinal = Math.floor(xSmoother.lowpassValue);
            let yFinal = Math.floor(ySmoother.lowpassValue);

            updateCoordinates(xFinal, yFinal);

            if (!controlLock) {
                sendCCChange(1, xFinal);
                sendCCChange(7, yFinal);
            }
        }

    }


    setInterval(() => {
        //console.log("PING");

        let xhttp = new XMLHttpRequest();
        xhttp.onreadystatechange = function () {

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


    function getRequest(url, cb) {
        let xhttp = new XMLHttpRequest();
        xhttp.onreadystatechange = function () {

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

    function drawGrid() {

        if (canvas.getContext) {

            let context = canvas.getContext('2d');
            context.beginPath();

            for (var x = 0.5; x < 508; x += 10) {
                context.moveTo(x, 0);
                context.lineTo(x, 508);
            }

            for (var y = 0.5; y < 508; y += 10) {
                context.moveTo(0, y);
                context.lineTo(508, y);
            }

            context.strokeStyle = 'grey';
            context.stroke();
            context.closePath();

        }
    }

    function updateCoordinates(x, y) {
        let ctx = canvas.getContext('2d');
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        var coords = "X coordinates: " + x + ", Y coordinates: " + y;
        coordinates.innerHTML = coords;

        drawGrid();
        ctx.beginPath();
        ctx.arc(x * 4, (127 - y) * 4, 10, 0, 2 * Math.PI);
        ctx.fillStyle = 'green';
        ctx.fill();
        ctx.closePath();
        var coords = "X coordinates: " + x + ", Y coordinates: " + y;
        coordinates.innerHTML = coords;
    }


    let instrumentName = document.getElementById('instrumentName');

    function updateInstrument(newInstrument) {
        instrumentName.innerHTML = newInstrument;
        let instrumentImage = document.getElementById('instrumentImage');
        instrumentImage.className = "";
        if (newInstrument == Instrument.PIANO) {
            instrumentImage.classList.add("pianoImage");
        }
        else if (newInstrument == Instrument.ELECTRIC_PIANO) {
            instrumentImage.classList.add("ePianoImage");
        }
        else if (newInstrument == Instrument.ORGAN) {
            instrumentImage.classList.add("organImage");
        }
    }

    let isLoopingText = document.getElementById("isLoopingText");

    function updateLoopingText() {
        if (isLooping) {
            isLoopingText.classList.remove("hidden");
        }
        else {
            isLoopingText.classList.add("hidden");
        }
    }

    function updateSkeletonText(message) {
        let skeletonText = document.getElementById("skeletonText");

        skeletonText.innerHTML = message;
        if (message == SkeletonPositions.OKAY) {
            skeletonText.classList.remove("invalid");
            skeletonText.classList.add("valid");
        } else {
            skeletonText.classList.remove("valid");
            skeletonText.classList.add("invalid");
        }
    }

    let pedalControlModeText = document.getElementById("pedalControlModeText");


    function updatePedalControlModeText() {
        if (pedalControlMode) {
            pedalControlModeText.innerHTML = "Control";
        }
        else {
            pedalControlModeText.innerHTML = "Sustain";
        }
    }

    let controlLockModeText = document.getElementById("controlLockModeText");

    function updateControlLockText() {
        if (controlLock) {
            controlLockModeText.innerHTML = "Locked";
        }
        else {
            controlLockModeText.innerHTML = "Engaged";
        }
    }

    

    updatePedalControlModeText();
    updateControlLockText();
});

function playSound(file){
    var audio = new Audio(file);
    audio.play();
}
