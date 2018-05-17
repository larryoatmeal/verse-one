import csv
from collections import defaultdict
with open('task1.csv', 'rb') as csvfile:
	reader = csv.reader(csvfile, delimiter=',', quotechar='|')
	counts = defaultdict(int)
	for row in reader:
		gesture = row[0]
		result = row[1]

		if result == 'y':
			counts[gesture] += 1/15.0

print counts