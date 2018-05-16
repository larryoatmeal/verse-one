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
  let calibrationText = document.getElementById("calibrationText");

  const processedMessages = new Set();

  //let hostIp = hostIpInput.value;
  //console.log("Pinging", hostIp);

  let isLooping = false;

  let loopMs = 500;

  let loopStart = 0;
  let loopEnd = 1000;
  //API
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

  function setCalibrationModeOn(){
    calibrationText.classList.add("hidden");
  }

  function setCalibrationModeOff(){
    calibrationText.classList.remove("hidden");
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
        }
        else if(command === 'loopOff'){
          loopOff();
        }
        else if(command === 'setLoopStart'){
          setLoopStart();
        }
        else if(command === 'setLoopEnd'){
          setLoopEnd();
        }
        else if(command === 'calibrateModeOn'){
          setCalibrationModeOn();
        }
        else if(command === 'calibrateModeOff'){
          setCalibrationModeOff();
        }
        processedMessages.add(id);
      }
    });
  }

  setInterval(() => {
    console.log("PING");

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

  let canvas = document.getElementById('canvas');

  function drawGrid() {

    if (canvas.getContext) {
      var context = canvas.getContext('2d');

      for(var x=0.5;x<500;x+=10) {
        context.moveTo(x,0);
        context.lineTo(x,500);
      }

      for(var y=0.5; y<500; y+=10) {
        context.moveTo(0,y);
        context.lineTo(500,y);
      }

      context.strokeStyle='gray';
      context.stroke();
    }
  };

  function showCoords(event) {
    var x = event.clientX - 10;
    var y = event.clientY - 10;
    var coords = "X coordinates: " + x + ", Y coordinates: " + y;
    document.getElementById('showCoords').innerHTML = coords;
  }


  drawGrid();


  canvas.addEventListener("click", showCoords);





});
