Running Verse-One is quite an involved process given the amount of components needed. First we list the requisite hardware and software packages.

HARDWARE 
1 Windows Computer (Server)
2 Mac Computer (Client, could possibly run on Windows, but untested)
3 Kinect
4 A MIDI Keyboard Controller
5 Wireless Nextwork

SOFTWARE
For Windows Computer:
1 Visual Studio
2 Kinect SDK installed

For Mac Computer:
1 Node JS
2 Google Chrome
3 MainStage, or some other MIDI supported musical instrument

PRELIMINARY NETWORK SETUP
In order to make server and client communicate, they must be connected on the same local network. On the windows machine, find the local IP. Then in the file UI/index.html, find the line

<input type="text" id="hostIp" value="http://192.168.0.101:8080/test"/>

and replace the 192.168.0.101 with the correct local IP address of the Widows machine. (Keep the :8080/test part)

You might also have to unblock the Windows port 8080.

PRELIMINARY MIDI SETUP
Make sure the MIDI keyboard is properly connected to your Mac. These instructions are for MainStage although similar steps can be made for other software.

In MainStage create three patches. In Edit View, find the Attributes tab under Patch Settings and set the Program Change number to 1,2,3 for each channel respectively.

Now in Layout view, click the foot pedal icon and set the MIDI port to "BUS 1 IAC Driver". Now in UI/js/main.js, find the line at the top

let midiName = "MK-449C USB MIDI Keyboard"

and replace the name with the name of the MIDI keyboard found in the Audio MIDI Setup app on Mac.

RUNNING BACKEND
To run the backend, open ShapeGame.sln with Visual Studio (in ADMINSTRATOR mode), and compile/run it. Check the url localhost:8080/test on the Windows computer to verify the backend is running. A screen should also appear that will show the skeleton once the user is detected.

RUNNING FRONTEND
cd into the folder "WebBackend". You will have to install some node packages:

npm install express
npm install glob

Now run 

node index.js

To start the web app. Open localhost:3000 in Google Chrome (other browsers do not have support for WebMIDI)







