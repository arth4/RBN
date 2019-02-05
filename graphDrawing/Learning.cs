using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Math;

namespace graphDrawing {
    static class Learning {

        class Neat {

        }

    }
    class EA {
        static readonly object[] DEFAULT_NET = new object[] { 3,6,10,2.0,1,true,0,false }; //(int inDegree,int gridX,int gridY, double sigma, int delayScaler = 1,bool refactoryScheme = true, int randomSeed = 0, bool randInit = false)
        public int Runtime;
        public List<Network> Population;
        public double MutRate;
        double CxRate;
        public int TournSize;
        private int PopSize;
        public int printGen = 0;
        static readonly Random Rand = new Random();
        private StringBuilder log = new StringBuilder();
        public EA(int popSize,int genLength,object[] netProps = null, double mutRate=0.1, double cxRate=0.2, int tournSize=8){
            Population = new List<Network>(popSize);
            if (ReferenceEquals(netProps, null))
                netProps = DEFAULT_NET;

            Parallel.For(0, popSize, i =>
                  Population.Add( new Network((int)netProps[0], (int)netProps[1], (int)netProps[2], (double)netProps[3], (int)netProps[4], (bool)netProps[5], (int)netProps[6], (bool)netProps[7]))
                  );
                 
            Runtime = genLength;
            this.MutRate = mutRate;
            this.CxRate = cxRate;
            this.TournSize = tournSize;
            this.PopSize = popSize;

        }
        public Network HOF = null;


        public void Run(string logName = "log") {
            try {


                for (int gen = 0; gen < Runtime; gen++) { //good to Parralel, would need threadsafe RNG
                    Mutate();
                    //Cx();
                    Selection();//HoF
                    if (printGen != 0 && gen % printGen == 0)
                        Console.WriteLine(gen); Console.WriteLine(Population.Min(ele => ele.Fitness));
                    log.AppendLine(string.Join(",", Population.Select(net => net.Fitness)));
                }
            }
            catch (Exception e) {
                log.AppendLine("ERROR" + e.ToString());                
            }
            System.IO.File.WriteAllText(logName+".txt", log.ToString());
        }

        private void Selection() {
            List<Network> newPop = new List<Network>(PopSize);
            //TODO: Add HOF
            for (int i = 0; i < PopSize; i++) {
                Network net = Tournement();
                newPop.Add(net);
            }
            
            Population = newPop;
            
        }
        private Network Tournement() {
            double fit = double.PositiveInfinity;
            Network winner = null;
            for (int i = 0; i < TournSize; i++) {
                var net = Rbn.PickRand(Population, Rand);
                if (Fitness(net) < fit) {
                    fit = net.Fitness;
                    winner = net;
                }
            }
            if (fit == double.PositiveInfinity) throw new Exception("No Individual found with fit < inf");
            return winner;
        }
        private double Fitness(Network net) {
            if (double.IsNaN(net.Fitness))
                net.Fitness = HammingFitn(net);            
            return net.Fitness;
        }
        private double HammingFitn(Network net) { //Conversions slow and unecessary //TODO: RUN length as param
            var target = Rbn.bTarget;
            int testNum = net.UseRandInit ? 10 : 1;
            var distance = .0;
            for (int netRun = 0; netRun < testNum; netRun++) {
                net.Reset();
                net.Run(100, "sync");
                var runOut = net.GetRunOut(target.Length);
                //var runOut = net.MakeSpikeTrain(target[0].Length);
                
                for (int i = 0; i < Rbn.target.Length; i++) {
                    bool[] r = runOut[i].Take(target[i].Length).ToArray(); //TODO: get rid, seperate timestamp from state 
                    distance += Accord.Math.Distance.Hamming(new BitArray(target[i]), new BitArray(r));
                }
            }
            return distance/testNum;
            
        }
        public void Mutate() {
            for (int net = 0; net < PopSize; net++) {
                //if (Rand.NextDouble() < MutRate)
                    Population.Add(basicMutate(Population[net].Copy()));
                Population[net].Fitness = double.NaN;
            }
        }

        private Network basicMutate(Network net) { //mutate net in place 
            if (Rand.NextDouble() < 0.5)
                MutateArcs(net,MutRate);
            else
                MutateFunc(net,MutRate);
            return net;
        }

        private void MutateArcs(Network net, double affectPercent) {
            foreach (var node in net.Nodes) {
                foreach (var arc in node.InArcs) {
                    if (Rand.NextDouble() < affectPercent) {
                        Node newIn = Rbn.PickRand(net.Nodes, Rand);
                        arc.ChangeInNode(newIn, newIn.NeighborDistance(node) * net.DelayScaler);
                    }                        
                }                
            }
        }
        private void MutateFunc(Network net,double affectPercent) {
            foreach (var node in net.Nodes) {
                for (int i = 0; i < node.mapArray.Length; i++) {
                    if (Rand.NextDouble() < affectPercent)
                        node.mapArray[i] = !node.mapArray[i];
                }
            }
        }

    } 
}
