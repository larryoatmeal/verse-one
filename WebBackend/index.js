const express = require('express')
const app = express()
const port = 3000
const path = require('path')
const fs = require('fs');
const { exec } = require('child_process');
const glob = require("glob")

// app.get('/', (request, response) => {
//   response.send('Hello from Express!')
// })

app.listen(port, (err) => {
  if (err) {
    return console.log('something bad happened', err)
  }

  console.log(`server is listening on ${port}`)
})


// app.engine('.hbs', exphbs({
//   defaultLayout: 'main',
//   extname: '.hbs',
//   layoutsDir: path.join(__dirname, 'views/layouts')
// }))

app.use(express.static('../UI'))
audioPath = path.join(__dirname, '../beatDetection/5_beat/audio/')

app.use("/audio", express.static(audioPath));

app.get('/songlist', function(req, res){
	audioSearch = path.join(__dirname, '../beatDetection/5_beat/audio/*.wav')

	glob(audioSearch, {}, function (er, files) {
		names = files.map((file) => {
			return path.basename(file).replace(/\.[^/.]+$/, "")
		});

  		res.send(names)
	})

})
app.get('/beatDetect/:name', function(req, res) {

	beatDetectMain = path.join(__dirname, '../beatDetection/5_beat/')
	audioPath = path.join(beatDetectMain, 'audio/')
	beatDetectScript = path.join(beatDetectMain, 'beatDetect.py')

	var name = req.param("name")
	file = path.join(audioPath, name + '.json')
	// res.send("name " + req.param("name"));

    fs.readFile(file, {encoding: 'utf-8'}, function(err,data){
    if (!err) {
        console.log('received data: ' + data);
    	res.send(data);

    } else {
        console.log('file does not exist');

        // script = 'cd ' + beatDetectMain + ";python beatDetect.py " + name
        script = 'cd ' + beatDetectMain + ';python beatDetect.py ' + name

        console.log(script)

        exec(script, (err, stdout, stderr) => {
		  if (err) {
		    // node couldn't execute the command
		    console.log("ERROR")
		    console.log(stdout)
		    console.log(stderr)
		    res.send(stderr)
		    return;
		  }

	      fs.readFile(file, {encoding: 'utf-8'}, function(err,data){
  	    	res.send(data);
	      })

		  // the *entire* stdout and stderr (buffered)
		  // console.log(`stdout: ${stdout}`);
		  // console.log(`stderr: ${stderr}`);
		});






    }
	});


});

// app.set('view engine', '.hbs')
// app.set('views', path.join(__dirname, '../UI/'))