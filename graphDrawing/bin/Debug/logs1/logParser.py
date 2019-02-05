# -*- coding: utf-8 -*-
import numpy as np
from matplotlib import pyplot as plt

i=-1
colour = "r"
for tSize in [2,4,8]:
    for mutR in [0.1,0.3,.5,.7]:
        for popSize in [50,100,200,400]:
            for delay in [0,1]:
                i+=1
                with open(f"log{i}_True_{tSize}_{mutR}_{popSize}_{delay}.txt",'r') as f:
                    log = f.readlines();
                log = [line.split(",") for line in log]
                log = [[float(val) for val in line] for line in log]
                mean = [np.mean(line) for line in log]
                if(i==39):#(mean[-1]<800):
                    plt.plot(mean,label=f"{i}")
    colour = "g" if tSize==2 else "b"


#
#with open("log1_True_2_0.1_50_1.txt",'r') as f:
#    log = f.readlines();
#log = [line.split(",") for line in log]
#log = [[float(val) for val in line] for line in log]
#mean = [np.min(line) for line in log]
#plt.plot(mean)










plt.show()