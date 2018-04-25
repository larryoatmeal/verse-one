
dataset = 'FAUST'
iterNumber = 1

cmd = "screen -S {0}{1} -d -m /usr/MATLAB/R2018a/bin/matlab -nodesktop -r 'RUN_{0}({1})'".format(dataset, iterNumber)

print cmd