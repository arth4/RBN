using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace graphDrawing {
    public partial class Form1 : Form {
        public Form1() {    
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            //for (int i = 0; i < 2; i++) {
            //    EA ea = new EA(200, 100);
            //    ea.TournSize = 8;
            //    ea.MutRate = 0.5;
                
            //    ea.Run($"mut5tourn8logDelay1run{i}");
            //}
            //for (int i = 0; i < 2; i++) {
            //    EA ea = new EA(200, 100, new object[] { 3, 6, 10, 2.0, 0, true, 0, false });
            //    ea.MutRate = 0.5;
            //    ea.TournSize = 8;
            //    ea.Run($"mut5tourn8logDelay0run{i}");
            //}
            int i = 0;
            foreach (var isRand in new bool[] { true, false }) {
                foreach (var tSize in new int[] { 2, 4, 8, 16 }) {
                    foreach (var mut in new double[] {0.1,0.3,0.5,0.7}) {
                        foreach (var popSize in new int[] { 50, 100, 200, 400 }) {
                            foreach (var dScale in new int[] { 0, 1 }) {
                                EA ea = new EA(popSize, 400, new object[] { 3, 6, 10, 2.0, dScale, true, 0, isRand });
                                ea.MutRate = mut;
                                ea.TournSize = tSize;
                                ea.Run($"logs1/log{i}_{isRand}_{tSize}_{mut}_{popSize}_{dScale}");
                                i++;
                            }
                        }
                    }
                }
            }

            this.Close();
            ////Rbn.DoStuff(new Network(3, 2, 5, 2,1,false));
            //var net = new Network(3, 6, 10, 2);
            //net.Run(200, "sync", true);
            //var train = net.MakeSpikeTrain(100);
            //var sb = new StringBuilder();
            //foreach (var state in train) {
            //    sb.Append(Rbn.ArrayToString(state));
            //    sb.AppendLine();                          
            //}
            //MessageBox.Show(sb.ToString());
            //chart1.Series.RemoveAt(0);
            //for (int i = 0; i < net.Nodes.Length; i++) {
            //    chart1.Series.Add(i.ToString()).ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            //    System.Windows.Forms.DataVisualization.Charting.Series s = chart1.Series[i];
            //    for (int j = 0; j < train.Length; j++) {

            //        int px = s.Points.AddXY(j, i);
            //        s.Points[px].Color = s.Color;
            //        s.Points[px].BorderWidth = 10;
            //        if (px > 0) s.Points[px - 1].Color = train[j][i]  ? s.Color : Color.Transparent;
            //    }
            //}
            //chart1.ChartAreas[0].AxisX.Interval=1;
            //chart1.ChartAreas[0].AxisY.Interval = 1;

            //chart1.ChartAreas[0].AxisX.Maximum = train.Length - 2;
            //chart1.ChartAreas[0].AxisX.Minimum = 0;
            //chart1.ChartAreas[0].AxisY.Maximum = net.Nodes.Length - 1;
            //chart1.ChartAreas[0].AxisY.Minimum = 0;
            //chart1.ChartAreas[0].AxisX.Title = "Attractor starts at: "+net.Tail.Count().ToString() +", with length: " +net.Attractor.Count().ToString();
            //net.DrawNet(null);

        }
    }

    class Rbn {
        public static  Random rand = new Random();

        static public void DoStuff(Network net) {

            //Network net = new Network(10, 3);
            int runtime = 500       ;
            net.runToMax = true;
            var output = net.Run(runtime, "sync",true);
            //net.DrawNet("null");
            var sOut = output.Select((o, index) => ArrayToString(o) + (index % net.lcm).ToString()).ToArray();
            Network.DrawGeneric(sOut,true,net.Nodes.Length);
        }
        
        //public static List<string[][]> RepeatedRun(int numRuns,int runtime,int inDegree=3,int gridx = 2, int gridy = 5, double sigma = 3, int delayScale =1) {
        //    var attractors = new List<Dictionary<string, string[]>>();

        //    Network net = new Network(inDegree, gridx, gridy, sigma,delayScale); 
        //    int x = 0;
        //    for (int i = 0; i < numRuns; i++) {
        //        net.Reset(); net.RandomizeState();
        //        net.Run(runtime, "sync", true);

        //        attractors.Add(new Dictionary<string, string[]> { { "attr", net.Attractor.ToArray() }, { "tail", net.Tail.ToArray() } });
        //        x++;
        //    }

        //    int numUniqueAttr = FindUniqueAttractors(attractors).Count;
        //    string[][] tails = attractors.Select(attr => attr["tail"]).ToArray();
        //    string[][] attrs = attractors.Select(attr => attr["attr"]).ToArray();
        //    return new List<string[][]> { new string[][] { new string[] { numUniqueAttr.ToString() } }, tails, attrs };
        //}
        
        public static List<Dictionary<string, string[]>> FindUniqueAttractors(List<Dictionary<string, string[]>> attractors) {
            
            var uniqueAttr = new List<Dictionary<string, string[]>>();
            
            for (int i = 0; i < attractors.Count -1; i++) {
                var unique = true;
                for (int j = i+1; j < attractors.Count; j++) {
                    if (CompareAttractors(attractors[i]["attr"], attractors[j]["attr"])) {
                        unique = false;
                        break;
                    }
                }
                if (unique)
                    uniqueAttr.Add(attractors[i]);
            }
            
            return uniqueAttr;
        }


        public static int BitArrayToInt(BitArray bA) {
            if (bA.Length > 32)
                throw new ArgumentException("Argument length shall be at most 32 bits.");
            int[] array = new int[1];
            bA.CopyTo(array, 0);
            return array[0];
        }

        /// <summary>
        /// Box-Muller transform to simulated normal distr
        /// </summary>
        public static double SampleNormal(double mean, double std, Random r) {
            
            double u1 = 1.0 - r.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0 - r.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                         Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            return(mean + std * randStdNormal);
        }

        public static T PickRand<T>(List<T> list,Random r) {
            return list[r.Next(list.Count())];

        }
        public static T PickRand<T>(T[] arr, Random r) {
            return arr[r.Next(arr.Count())];

        }
        public static string[] SimplifyStateNames(string[] states) {
            var dict = new Dictionary<string, string>();
            int currentMax = 0;
            for (int i = 0; i < states.Length; i++) {
                if (dict.Keys.Contains(states[i])) {
                    states[i] = dict[states[i]];
                }
                else {
                    dict[states[i]] = currentMax.ToString();
                    states[i] = dict[states[i]];
                    currentMax++;
                }
            }
            return states;
        }

        public static Dictionary<string,string[]> FindAttractor(string[] states) {
            var tail = new List<string>();
            int i = 0;
            while (i<states.Length &&!tail.Contains(states[i])) {
                tail.Add(states[i]);
                i++;
            }
            string[] attractor;
            if (i < states.Length) {
                var start = tail.FindIndex(x => x.Equals(states[i]));
                attractor = new string[tail.Count - start];
                tail.CopyTo(start, attractor, 0, attractor.Length);
                tail.RemoveRange(start, attractor.Length);
            }
            else {
                attractor = new string[0];
            }


            return new Dictionary<string, string[]>() { { "tail", tail.ToArray() }, { "attractor", attractor } };
        }

        public static bool CompareAttractors(string[] a1, string[] a2) {
            if (a1.Length != a2.Length || !a2.Contains(a1[0]))
                return false;            
            var y = Array.FindIndex(a2, x => x.Equals(a1[0])) + 1;
            for (int x = 1; x < a1.Length; x++) {

                if (y == a2.Length)
                    y = 0;
                if (a1[x] != a2[y])
                    return false;
                y++;
            }
            return true;
            
        }
        
        public static string ArrayToString<T>(T[] arr) {
            var sb = new StringBuilder();
            if(typeof(T) == typeof(bool)) {
                foreach (var item in arr) {
                    sb.Append((bool)(object)item? 1 : 0 );
                }
                return sb.ToString();
            }
            foreach (var item in arr) {
                sb.Append(item.ToString());
            }
            return sb.ToString();
        }

        #region spiketrain measures 

        public static float TanimotoCoeff(bool[] a, bool[] b) {
            if (a.Length != b.Length) throw new Exception("Compared arrays must be of same size");
            float andSum=0, orSum=0;
            for (int i = 0; i < a.Length; i++) {
                if (a[i] || b[i]) orSum++;
                if (a[i] && b[i]) andSum++;
            }
            return (andSum / orSum);
        }

        public static double SpikeSync(List<int>[] trains) {
            int sum=0, count =0;
            for (int i = 0; i < trains.Length; i++) {
                int spikeIndx = -1;
                foreach (int spike in trains[i]) {
                    spikeIndx++;
                    for (int otherTrain = i+1; otherTrain < trains.Length; otherTrain++) {
                        //Find closest spike
                        int compareResult = 0, nextResult = 0, compIndx = 0;
                        foreach (int oSpike in trains[otherTrain]) {
                            nextResult = Math.Abs(spike - oSpike);
                            if (compareResult < nextResult && compareResult != 0)
                                break;
                            compareResult = nextResult;
                            compIndx++;
                        }
                        //Calc threshold
                        int sThresh = spike, oThresh = trains[otherTrain][compIndx];
                        if (spikeIndx != 0)
                            sThresh = spike - trains[i][spikeIndx - 1]; //distance between spike and previous
                        if (compIndx != 0)
                            oThresh = trains[otherTrain][compIndx] - trains[otherTrain][compIndx-1];

                        if (spikeIndx != trains[i].Count() - 1)
                            sThresh = Math.Min(sThresh, trains[i][spikeIndx + 1] - spike);
                        else
                            sThresh = Math.Min(sThresh, trains[i].Count()-1 - spike);

                        if (compIndx!= trains[otherTrain].Count() - 1)
                            oThresh = Math.Min(oThresh, trains[otherTrain][compIndx + 1] - trains[otherTrain][compIndx]);
                        else
                            sThresh = Math.Min(sThresh, trains[otherTrain].Count() - 1 - trains[otherTrain][compIndx]);

                        double threshold = Math.Min(sThresh, oThresh) / 2.0;

                        if (compareResult < threshold)
                            sum++;
                        count++;
                    }       

                    
                }
            }
            return (((double)sum) / count);
        }


        #endregion
        #region target
        public static string[] target = new string[] { "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111", "000001111100000111110000011111000001111100000111110000011111" };
        //TODO: clean this mess
        public static bool[][] bTarget = BoolTarget(0);
        public static bool[][] BoolTarget(int l = 0) {
            if (l == 0)
                l = target.Length;
            var res = new bool[l][];
            
            for (int i = 0; i < l; i++) {
                var a = new bool[target[i].Length];
                for (int chr = 0; chr < target[i].Length; chr++) {
                    a[chr] = target[i][chr] == '1';
                }
                res[i] = a;
            }
            return res;
        }
        #endregion

    }





    class Network {
        public Node[] Nodes { get; set; }
        public bool[][] RunOutput { get; private set; }
        public int[][] DelayStates { get; private set; }
        public int GlobalTime { get; private set; } = -1;
        public bool RefactoryScheme { get; private set; }
        public int DelayScaler { get; private set; }

        public List<bool[]> Tail { get; private set; } = new List<bool[]>();
        public List<bool[]> Attractor { get; private set; } = new List<bool[]>();

        private List<Arc> arcs= new List<Arc>(); 
        public List<Arc> Arcs { get { return arcs; } } //populated by the Arc class constructor
        public int lcm;
        private int RandSeed;
        private Random Rand;
        public bool UseRandInit;
        private bool outputReady = false;
        public bool runToMax = false;
        public double Fitness = double.NaN;
        public void AddArc(Arc a) {
            arcs.Add(a);
                }

        public void Reset() {
            GlobalTime = -1;
            if (!UseRandInit)
                Parallel.ForEach(Nodes, n => n.Reset());
            else
                RandomizeState();

            outputReady = false;
        }

        public Network Copy() {
            Node[] nodes = new Node[Nodes.Length];
            for (int node = 0; node < nodes.Length; node++) {
                nodes[node] = new Node(node, Nodes[node].initValue, null);
            }
            Network net = new Network(nodes, DelayScaler, RefactoryScheme, RandSeed, UseRandInit);
            for (int node = 0; node < nodes.Length; node++) {
                nodes[node].SetNet(net);
            }
            for (int node = 0; node < nodes.Length; node++) {
                //adding the nodes auto-updates the In-arcs of the network and nodes
                nodes[node].AddInNodes(Nodes[node].InArcs.Select(arc => nodes[arc.inNode.Id]).ToArray(),false); //find in-Nodes of original node and then find the corrisponding new node copies

                bool[] ma = new bool[Nodes[node].mapArray.Length]; //copy bool function into copy node
                Nodes[node].mapArray.CopyTo(ma,0);
                nodes[node].mapArray = ma;
            }
            net.lcm = (int)Utils.LCM(Arcs
                .Select(a => (long)(a.Delay + 1))
                .Distinct()
                .ToArray());
            return net;
        }
        private Network(Node[] nodes, int delayScaler,bool refactoryScheme, int randomSeed, bool randInit) { //for copying only
            Nodes = nodes; DelayScaler = delayScaler; RefactoryScheme = refactoryScheme;
            RandSeed = randomSeed; UseRandInit = randInit;
            Rand = RandSeed == 0 ? new Random() : new Random(RandSeed);
            //lcm = (int)Utils.LCM(Arcs
            //    .Select(a => (long)(a.Delay + 1))
            //    .Distinct()
            //    .ToArray());
        }

        /// <summary>
        /// For network with spatial parameter
        /// </summary>        
        public Network(int inDegree,int gridX,int gridY, double sigma, int delayScaler = 1,bool refactoryScheme = true, int randomSeed = 0, bool randInit = false) {
            var numNodes = gridX * gridY;             
            var nodes = new Node[numNodes];
            RandSeed = randomSeed;
            Rand = RandSeed==0? new Random() : new Random(RandSeed);
            
            for (int y = 0; y < gridY; y++) {
                for (int x = 0; x < gridX; x++) {
                    var nodeVal = UseRandInit ? (Rand.NextDouble() < 0.5 ? false : true) : false; 
                    nodes[x+y*gridX] = new Node(x + y*gridX, Rand.NextDouble()<0.5? false:true, x, y,this);
                }                
            }
            RefactoryScheme = refactoryScheme;
            DelayScaler = delayScaler;
            for (int i = 0; i < numNodes; i++) {
                var tempNodes = new Node[inDegree];
                var neighborhoodDists = nodes
                        .GroupBy(node => node.NeighborDistance(nodes[i]))
                        .ToDictionary(g => g.Key, g => g.ToList()); //puts nodes into lists by distance to node i 
                var maxDist = neighborhoodDists.Keys.Max(); 

                for (int j = 0; j < inDegree; j++) {
                    var randN = Rbn.SampleNormal(0, sigma,Rand);
                    var selectedDist = (int)(Math.Abs(randN));                    
                    tempNodes[j] = Rbn.PickRand(neighborhoodDists[Math.Min(selectedDist,maxDist)],Rand);
                }
                nodes[i].AddInNodes(tempNodes);
            }
            Nodes = nodes;
            lcm = (int)Utils.LCM(Arcs
                .Select(a => (long)(a.Delay+1))
                .Distinct()
                .ToArray());
        }
        
        
        public void Update(string updateScheme) {
            //update arcs first 

            Parallel.ForEach(Nodes, n =>
                Parallel.ForEach(n.InArcs, a => a.Update())
            );
            
            if (updateScheme.Equals("sync")) {
                Parallel.ForEach(Nodes, n => n.Update());                
            }
            else throw new Exception("update scheme not recognised");
            GlobalTime++;
        }
        public bool[] getState(bool getArcs = false) {
            bool[] state;
            if (!getArcs) {
                state = new bool[Nodes.Length];
                Parallel.For(0, Nodes.Length, i => state[i] = Nodes[i].Value);
            }
            else {
                state = new bool[Nodes.Length + Arcs.Count];
                Parallel.For(0, Nodes.Length, i => state[i] = Nodes[i].Value);
                Parallel.For(Nodes.Length,state.Length, i => state[i] = Arcs[i-Nodes.Length].Value);
            }
            return state;
        }
        public int[] getDelayState() {
            var state = new int[Arcs.Count];
            Parallel.For(0, state.Length, i => state[i] = Arcs[i].TimeUntilUpdate());
            return state;
        }
        private int TestIfAttractor() {
            int index = Tail.FindIndex(s => s.SequenceEqual(Tail.Last()));
            if (index == Tail.Count - 1) //check if first occurance of state is 'this' occurence
                return -1;
            else
                return index;

        }

        public bool[][] Run(int timesteps,string updateScheme,bool getArcs = false) {
            GlobalTime = 0;
            RunOutput =new bool[timesteps][];
            DelayStates = new int[timesteps][];
            int sizeOfTimestamp = ((int)Math.Log(lcm, 2)) + 1;
            for (int t = 0; t < timesteps; t++) {
                RunOutput[t] = getState(getArcs);
                //DelayStates[t] = getDelayState();
                DelayStates[t] = new int[] { GlobalTime % lcm };
                //Tail.Add(Rbn.ArrayToString(RunOutput[t]) + Rbn.ArrayToString(DelayStates[t]));
                bool[] state = new bool[RunOutput[t].Length + sizeOfTimestamp];
                RunOutput[t].CopyTo(state, 0);
                new BitArray(GlobalTime % lcm).Cast<bool>().SkipWhile(x => !x).ToArray()
                    .CopyTo(state, RunOutput[t].Length);
                Tail.Add(state);

                if (!runToMax) {//for debug (remove)
                    var attrStart = TestIfAttractor();
                    if (attrStart != -1) {
                        Tail.RemoveAt(Tail.Count - 1);
                        Attractor = Tail.GetRange(attrStart, Tail.Count - attrStart);
                        Tail.RemoveRange(attrStart, Tail.Count - attrStart);
                        outputReady = true;
                        return RunOutput;
                    }
                }
                Update(updateScheme); //increments GlobalTime
            }
            outputReady = true;
            return RunOutput;

        }
        public bool[][] GetRunOut(int len) {
            if (!outputReady) {
                throw new Exception("No Run Output");
                return new bool[0][];
            }
            var result = new bool[len][];
            Tail.GetRange(0, Math.Min(len, Tail.Count)).ToArray().CopyTo(result, 0);

            if (Tail.Count >= len)
                return result;

            var stopCond = (len - Tail.Count) / Attractor.Count;
            for (int i = 0; i < stopCond; i++) {
                Attractor.ToArray().CopyTo(result, Tail.Count + i * Attractor.Count);
            }
            var remainder = (len - Tail.Count) % Attractor.Count;
            if (remainder != 0) {
                Attractor.GetRange(0, remainder).ToArray().CopyTo(result, len - remainder);
            }
            return result;
        }
        /// <summary>
        /// if states empty bool[] then initialises all to false
        /// </summary>        
        public void SetStates(bool[] states) {
            if (states.Length == 0) {
                foreach (var n in Nodes) {
                    n.Value = false;
                }
                return;
            }
            if (states.Length != Nodes.Length) throw new Exception("states.Length!= Nodes.Length()");
            for (int i = 0; i < states.Length; i++) {
                Nodes[i].Value = states[i];
            }
        }
        public void RandomizeState() {
            foreach (var n in Nodes) {
                n.Value = Rand.NextDouble() < 0.5 ? false : true;
            }
        }

        public bool[][] MakeSpikeTrain(int runtime =-1) {
            if (runtime < 0)
                runtime = Attractor.Count() + Tail.Count();
            if (Attractor.Count() < 1 && runtime > Tail.Count())
                throw new Exception("Runtime cannot exceed tail length when there is no attractor");

            int t = 1;
            bool[][] train = new bool[runtime][];
            train[0] = new bool[Nodes.Length]; //first all 0 as no spike on first timestep (result of how spikes encoded)
            while (t < runtime && t< Tail.Count()) { //TODO: optimise loop (for loop better)
                train[t] = SpikeCheck(Tail[t - 1], Tail[t]);
                t++;
            }

            if (t < runtime) {
                train[t] = SpikeCheck(Tail[t - 1], Attractor[0]);
                t++;
            }
            int AttrI = 1;
            while (t < runtime) {
                if(AttrI == Attractor.Count()) {
                    train[t] = SpikeCheck(Attractor[AttrI - 1], Attractor[0]);
                    AttrI = 1;
                    continue; 
                }
                train[t] = SpikeCheck(Attractor[AttrI - 1], Attractor[AttrI]);
                t++; AttrI++;
            }
            return train;

        }
        private bool[] SpikeCheck(bool[] state1, bool[] state2) {
            bool[] spikes = new bool[Nodes.Length];
            for(int node= 0; node< spikes.Length; node++) {
                spikes[node] = !state1[node] && state2[node];
            }
            return spikes;
        }

        public void DrawNet(string saveLocation) {
            Microsoft.Msagl.Drawing.Graph graph = new Microsoft.Msagl.Drawing.Graph("");
            foreach (var node in Nodes) {
                foreach (var arc in node.InArcs) {
                    graph.AddEdge(arc.inNode.Id.ToString(), node.Id.ToString());
                }
            }


            Form form = new System.Windows.Forms.Form();
            Microsoft.Msagl.GraphViewerGdi.GViewer viewer = new Microsoft.Msagl.GraphViewerGdi.GViewer();
            viewer.Graph = graph;            
            form.SuspendLayout();
            viewer.Dock = DockStyle.Fill;
            form.Controls.Add(viewer);
            form.ResumeLayout();
            form.ShowDialog();
        }
        public static void DrawGeneric(string[] states,bool shrinkState,int numNodes, string[] delayStates = null) {
            Microsoft.Msagl.Drawing.Graph graph = new Microsoft.Msagl.Drawing.Graph("");

            if (delayStates != null) {
                if (states.Length != delayStates.Length)
                    throw new Exception("states.Length != delayStates.Length");
                Parallel.For(0, states.Length, i => states[i] = states[i] + delayStates[i]);
            }

            states = Rbn.SimplifyStateNames(states);
            for (int i = 1; i < states.Length; i++) {
                graph.AddEdge(states[i-1], states[i]);
            }


            Form form = new System.Windows.Forms.Form();
            Microsoft.Msagl.GraphViewerGdi.GViewer viewer = new Microsoft.Msagl.GraphViewerGdi.GViewer();
            viewer.Graph = graph;
            form.SuspendLayout();
            viewer.Dock = DockStyle.Fill;
            form.Controls.Add(viewer);
            form.ResumeLayout();
            form.ShowDialog();
        }
        



    }



    class Node {
        public Arc[] InArcs { get; private set; }
        private Network net;
        public int Id { get; }
        public bool[] mapArray;
        public bool Value { get; set; }
        public bool initValue;
        
        
        public int XPos { get; set; } public int YPos{get;set;}


        public Node(int id, Node[] inNodes, bool value,Network net) {
            Id = id;
            initValue = value;
            InArcs = new Arc[inNodes.Length];
            for (int i = 0; i < inNodes.Length; i++) {
                InArcs[i] = new Arc(inNodes[i],
                    NeighborDistance(inNodes[i]) * net.DelayScaler,
                    net.RefactoryScheme,
                    net);

            }
            
            this.Value = value;
            
            this.net = net;
            XPos = 0; YPos = 0;
            NewRandomFunction(inNodes.Length);
        }

        public Node(int id, bool value,Network net) {
            Id = id;
            initValue = value;
            Value = value;
            InArcs = new Arc[0];
            this.net = net;
            XPos = 0; YPos = 0;

        }
        public Node(int id, bool value,int xPos,int yPos,Network net) {
            Id = id;
            initValue = value;
            Value = value;
            InArcs = new Arc[0];
            this.net = net;
            XPos = xPos; YPos = yPos;

        }

        public void NewRandomFunction(int arity) {
            var len = (int)Math.Pow(2, arity);
            mapArray = new bool[len];
            for (int i = 0; i < len; i++) {
                mapArray[i] = Rbn.rand.NextDouble() >= 0.5;
            }
        }

        public void Update() {
            var input = new bool[InArcs.Length];

            Parallel.For(0, input.Length, i => {
                input[i] = InArcs[i].Value;
            });

            this.Value = ApplyMyFunc(new BitArray(input));

        }

        public bool ApplyMyFunc(BitArray x) {
            int input = Rbn.BitArrayToInt(x);
            return this.mapArray[input];
        }
        public void Reset() {
            Value = initValue;
        }
        public void SetNet(Network net) {
            this.net = net;
        }
        
        public void AddInNodes(Node[] nodes,bool makeNewFunc = true) {
            var newArcs = new Arc[InArcs.Length + nodes.Length];
            InArcs.CopyTo(newArcs, 0);

            for (int i = InArcs.Length; i < newArcs.Length; i++) {
                newArcs[i] = new Arc(nodes[i],
                    NeighborDistance(nodes[i]) * net.DelayScaler,
                    net.RefactoryScheme,
                    net);
            }
            InArcs= newArcs;
            if(makeNewFunc)
                NewRandomFunction(InArcs.Length);
        }
        public int NeighborDistance(Node node) {
            var x = Math.Abs(this.XPos - node.XPos);
            var y = Math.Abs(this.YPos - node.YPos);
            return Math.Max(x, y);
        }
        public double AbsDistance(Node node) {
            var x = this.XPos - node.XPos;
            var y = this.YPos - node.YPos;
            return (Math.Sqrt(Math.Pow(x, 2) * Math.Pow(y, 2)));
        }
    }
    class Arc {
        private readonly bool RefactoryScheme;
        public Node inNode { get;private set; }
        private readonly Network net;
        public int Delay { get; set; }
        public bool Value { get; private set; }

        public Arc( Node inNode, int delay,bool refactoryScheme,Network net) {
            RefactoryScheme = refactoryScheme;
            this.inNode = inNode;
            Delay = delay;
            this.net = net;
            this.net.AddArc(this);
        }
        public int TimeUntilUpdate() {
            return Delay==0? 0 : (Delay - (net.GlobalTime % Delay));
        }
        public void ChangeInNode(Node node,int delay) {
            inNode = node;
            Delay = delay;
        }

        public void Update() {
            if (RefactoryScheme) {
                if (net.GlobalTime % (Delay+1) == 0) //0 nice as all update on initialise
                    Value = inNode.Value;
            }
            else {
                if (net.GlobalTime > Delay)
                    Value = net.RunOutput[net.GlobalTime - Delay][inNode.Id];
                else
                    Value = net.RunOutput[0][inNode.Id];
            }
        }
    }

    class Utils {
        public static long LCM(long[] numbers) {
            return numbers.Aggregate(lcm);
        }
        private static long lcm(long a, long b) {
            return Math.Abs(a * b) / GCD(a, b);
        }
        static long GCD(long a, long b) {
            return b == 0 ? a : GCD(b, a % b);
        }

    }

}
