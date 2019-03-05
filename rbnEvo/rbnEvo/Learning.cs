using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Math;

namespace rbnEvo {
    static class Learning {

        class Neat {

        }

    }
    class EA {
        static readonly object[] DEFAULT_NET = new object[] { 3, 6, 10, 2.0, 1, true, 0, false }; //(int inDegree,int gridX,int gridY, double sigma, int delayScaler = 1,bool refactoryScheme = true, int randomSeed = 0, bool randInit = false)
        public int Runtime;
        public List<Network> Population;
        public double MutRate;
        double CxRate;
        public int TournSize;
        private int PopSize;
        public int printGen = 0;
        static readonly Random Rand = new Random();
        public bool[][] target;
        public int testNum = 10;
        private StringBuilder log = new StringBuilder();
        public int netRunTime = 100;
        public string hamMethod = "all";
        public double hamScale = 1;

        public EA(int popSize, int genLength, object[] netProps = null, double mutRate = 0.1, double cxRate = 0.2, int tournSize = 8) {
            Population = new List<Network>(popSize);
            if (ReferenceEquals(netProps, null))
                netProps = DEFAULT_NET;

            Parallel.For(0, popSize, i =>
                  Population.Add(new Network((int)netProps[0], (int)netProps[1], (int)netProps[2], (double)netProps[3], (int)netProps[4], (bool)netProps[5], (int)netProps[6], (bool)netProps[7]))
                  );

            Runtime = genLength;
            this.MutRate = mutRate;
            this.CxRate = cxRate;
            this.TournSize = tournSize;
            this.PopSize = popSize;

        }
        public Network HOF = null;


        public void Run(string logName = "log") {           
            

                for (int gen = 0; gen < Runtime; gen++) { //good to Parralel, would need threadsafe RNG
                    Mutate();
                    //Cx();
                    Selection();//HoF               
                    if (printGen != 0 && gen % printGen == 0)
                        Console.Write(gen); Console.Write(": "); //Console.WriteLine(Population.Min(ele => ele.Fitness));
                
                Console.WriteLine(HOF.Fitness); 
                    log.AppendLine(string.Join(",", Population.FindAll(n=> !double.IsNaN(n.Fitness)).Select(net => net.Fitness)));
                }
            log.AppendLine(Population.OrderBy(net => net.Fitness).First().strReprs());


            System.IO.File.WriteAllText(logName + ".txt", log.ToString());
        }

        private void Selection() {
            List<Network> newPop = new List<Network>(PopSize);
            Network best = null;
            for (int i = 0; i < PopSize; i++) {
                Network net = Tournement();
                newPop.Add(net);
                Population.Remove(net);
                if (object.ReferenceEquals(best, null) || net.Fitness < best.Fitness)
                    best = net;                
            }
            if (object.ReferenceEquals(HOF, null) || best.Fitness <= HOF.Fitness) {
                HOF = best.Copy();
                Fitness(HOF);
            }
            else
                newPop.Add(HOF);

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
            if (double.IsNaN(net.Fitness)) {
                if (hamMethod == "all")
                    net.Fitness = HammingFitn(net);
                else
                    net.Fitness = FullStateFitn(net);

            }
            return net.Fitness;
        }
        private double HammingFitn(Network net) { //Conversions slow and unecessary //TODO: RUN length as param
            
            var distance = .0;
            for (int netRun = 0; netRun < testNum; netRun++) {
                net.Reset();
                net.Run(netRunTime, "sync");
                var runOut = net.GetRunOut(target.Length);
                //var runOut = net.MakeSpikeTrain(target[0].Length);

                for (int i = 0; i < Rbn.target.Length; i++) {
                    bool[] r = runOut[i].Take(target[i].Length).ToArray(); //TODO: get rid, seperate timestamp from state 
                    distance += Accord.Math.Distance.Hamming(new BitArray(target[i]), new BitArray(r));
                }
            }
            return distance / testNum;

        }
        private double FullStateFitn(Network net) { //Conversions slow and unecessary //TODO: RUN length as param

            var distance = .0;
            for (int netRun = 0; netRun < testNum; netRun++) {
                net.Reset();
                net.Run(netRunTime, "sync");
                var runOut = net.GetRunOut(target.Length);
                //var runOut = net.MakeSpikeTrain(target[0].Length);

                for (int i = 0; i < Rbn.target.Length; i++) {
                    bool[] r = runOut[i].Take(target[i].Length).ToArray(); //TODO: get rid, seperate timestamp from state 
                    distance += new BitArray(target[i]).Xor(new BitArray(r)).OfType<bool>().All(e => !e) ? 0 : 1;
                }
            }
            return distance / testNum;

        }
        public void Mutate() {
            for (int net = 0; net < PopSize; net++) {
                //if (Rand.NextDouble() < MutRate)
                Population.Add(basicMutate(Population[net].Copy()));
                //Population[net].Fitness = double.NaN; //not needed, copying creates new net thus fit = NaN in new one
            }
        }

        private Network basicMutate(Network net) { //mutate net in place 
            if (Rand.NextDouble() < 0.5)
                MutateArcs(net, MutRate);
            else
                MutateFunc(net, MutRate);
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
        private void MutateFunc(Network net, double affectPercent) {
            foreach (var node in net.Nodes) {
                for (int i = 0; i < node.mapArray.Length; i++) {
                    if (Rand.NextDouble() < affectPercent)
                        node.mapArray[i] = !node.mapArray[i];
                }
            }
        }

    }
}
