import random
a = 'play'
b = 'loopStart'
c = 'loopEnd'
d = 'forward'
e = 'backward'
f = 'cycle mode'


gestures = [a, b, c, d, e, f]
gesturesTest = gestures + gestures + gestures
cont = True
while cont:
	random.shuffle(gesturesTest)
	cont = False
	for (a, b) in zip(gesturesTest, gesturesTest[1:]):
		if a == b:
			cont = True

for g in gesturesTest:
	print g