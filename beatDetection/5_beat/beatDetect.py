import pset5
import json

# print pset5.beatDetect('audio/snow.wav')
import sys

def performBeatDetection(path):
	[beats, bpm] = pset5.beatDetect(path)

	obj = {
		'beats': beats.tolist(),
		'bpm': bpm
	}

	with open(path[:-3] + 'json', 'w') as outfile:
		# print(path[:-3] + 'json')
		json.dump(obj, outfile)
    	# json.dump(obj, outfile, sort_keys=True, indent=4, separators=(',', ': '))
	return obj


name = 'snow'
if len(sys.argv) > 1:
	name = sys.argv[1]

performBeatDetection('audio/' + name + '.wav')
print("DONE")


