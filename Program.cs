using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ShellProgressBar;

namespace TspApp
{
    class Program
    {
        /// <summary>
        /// att48_mat.dat
        /// bayg29_mat.dat
        /// bays29_mat.dat        
        /// burma14_mat.dat
        /// fri26_mat.dat
        /// gr21_mat.dat
        /// gr24_mat.dat
        /// pr76_mat.dat
        /// st70_mat.dat
        /// </summary>
        /// <param name="args"></param>
        /// 

        private static uint[,] distanceMatrix;
        private static readonly Random RNG = new Random();
        static void Main()
        {
            var data = new TSPData("./data/source", "./data/instances", "./data/results");
            var p = new Program();
            var instances = new DirectoryInfo("./data/instances")
                .GetFiles("*_mat.dat")
                .Select(x => x.Name)
                .ToArray();            

            //// prepare instance data
            //data.PrepareInstanceData(null);
            
            // run on all the instances
            var runsCount = 100;
            var results = new Dictionary<string, List<string>>(instances.Length);
            
            foreach (var instanceName in instances)
            {
                Console.WriteLine("Solving instance: " + instanceName);
                // load and parse data            
                distanceMatrix = data.LoadMatrix(instanceName);

                // run the algorithm                
                var instanceResult = new List<string>(runsCount * 2);                

                using (var pbar = new ProgressBar(runsCount, instanceName))
                {     
                    for (int r = 0; r < runsCount; r++)
                    {
                        var (circuit, cost) = p.RunHeuristic(grasp: true);

                        // save run result
                        instanceResult.Add(string.Join(' ', circuit));
                        instanceResult.Add(cost.ToString());
                        
                        pbar.Tick($"Task {r + 1} out of {runsCount}");                                                                
                    }
                }
                
                // add instance results to the overall results dictionary
                results.Add(instanceName, instanceResult);                
            }            
            
            // save overall results
            string folderName = $"{DateTime.Now.Day}{DateTime.Now.Hour}{DateTime.Now.Minute}";
            data.SaveResults(results, folderName);            
        }

        private (LinkedList<uint>, uint) RunHeuristic(bool grasp = true)
        {
            // distances matrix            
            int rowsCount = (int) Math.Sqrt(distanceMatrix.Length);

            // select starting node            
            uint startNode = (uint) RNG.Next(maxValue: (int) rowsCount);

            // init circuit
            LinkedList<uint> circuitList = new LinkedList<uint>();
            circuitList.AddFirst(startNode);

            // init frontier
            var range = Enumerable.Range(0, rowsCount).ToList().ConvertAll(x => (uint) x);
            List<uint> frontierList = new List<uint>(range);
            frontierList.Remove(startNode);

            for (int i = 0; i < rowsCount - 1; i++)
            {                
                // select next node to add to the circuit
                var (circuitNodeId, frontierNodeId) = SelectNextNode(circuitList, frontierList);

                // add new node to circuit
                circuitList.AddAfter(circuitList.Find(circuitNodeId), frontierNodeId);

                // remove node from frontier
                frontierList.Remove(frontierNodeId);
            }

            // 2-opt exchange heuristic
            if (grasp)
            {                
                circuitList = Opt2Swap(circuitList);
            }

            // return circuit and cost
            return (circuitList, CircuitCost(circuitList));
        }
        
       // 2optSwap(route, i, k)
       // {
       //   1.take route[0] to route[i - 1] and add them in order to new_route
       //   2.take route[i] to route[k] and add them in reverse order to new_route
       //   3.take route[k + 1] to end and add them in order to new_route
       //   return new_route;
       // }
        private static LinkedList<uint> Opt2Swap(in LinkedList<uint> circuitList)
        {
            uint[] circuit = circuitList.ToArray();
            uint[] swapped = new uint[circuit.Length];            
            LinkedList<uint> bestCircuit = circuitList;

            for (int i = 1; i < circuit.Length - 1; i++)
            {
                for (int j = i + 1; j < circuit.Length; j++)
                {                    
                    // head
                    Array.Copy(circuit, 0, swapped, 0, i);

                    // reversed mid
                    var mid = circuit.Skip(i).Take(j - i + 1).Reverse().ToArray();
                    Array.Copy(mid, 0, swapped, i, j - i + 1);

                    // tail
                    Array.Copy(circuit, j + 1, swapped, j + 1, circuit.Length - j - 1);

                    // check cost
                    var nc = new LinkedList<uint>(swapped);                    
                    if (CircuitCost(nc) < CircuitCost(bestCircuit))
                    {                        
                        bestCircuit = nc;
                    }
                }
            }

            return bestCircuit;
        }

        private static List<uint> FilterFrontier(uint currentNode, uint nextCircuitNode, List<uint> frontier)
        {
            // check for single node frontier
            if (frontier.Count <= 1)
            {
                return frontier;
            }
            
            double[] costs = new double[frontier.Count];

            // compute frontier nodes insertion cost            
            for (int i = 0; i < frontier.Count; i++)
            {
                costs[i] = distanceMatrix[currentNode, frontier[i]] + distanceMatrix[frontier[i], nextCircuitNode] - distanceMatrix[currentNode, nextCircuitNode];                
            }

            // make RCL
            var mu = 0.40;
            var rclMin = costs.Min();
            var rclMax = costs.Max(); 
            var rcl = new List<uint>();

            for (int i = 0; i < costs.Length; i++)
            {
                if (rclMin <= costs[i] && costs[i] <= (rclMin + mu * (rclMax - rclMin)) )
                    rcl.Add(frontier[i]);
            }

            return rcl;
        }

        private static (uint, uint) SelectNextNode(LinkedList<uint> circuit, List<uint> frontier)
        {
            uint circuitNodeId = uint.MaxValue;
            uint frontierNodeId = uint.MaxValue;
            uint minimumAddCost = uint.MaxValue;
            
            // select a node from the circuit
            foreach (uint circuitNode in circuit) 
            {
                uint nextCircuitNode;
                if (circuitNode == circuit.Last.Value)
                    nextCircuitNode = circuit.First.Value; //simulate a circular linked list
                else
                    nextCircuitNode = circuit.Find(circuitNode).Next.Value;

                // filter frontier
                List<uint> filteredFrontier = FilterFrontier(circuitNode, nextCircuitNode, frontier);

                // select a node from the frontier
                foreach (uint ext in filteredFrontier) 
                {                    
                    uint extAddCost = distanceMatrix[circuitNode, ext] + distanceMatrix[ext, nextCircuitNode] - distanceMatrix[circuitNode, nextCircuitNode];
                    if (extAddCost <= minimumAddCost)
                    {
                        // update best node so far
                        minimumAddCost = extAddCost;
                        circuitNodeId = circuitNode;
                        frontierNodeId = ext;
                    }
                }                
            }

            return (circuitNodeId, frontierNodeId);
        }

        private static uint CircuitCost(LinkedList<uint> circuit)
        {
            uint cost = 0;

            foreach (uint node in circuit)
            {
                // get next node
                uint next;
                if (node == circuit.Last.Value)
                    next = circuit.First.Value;
                else
                    next = circuit.Find(node).Next.Value;

                // compute segment cost
                var c = distanceMatrix[node, next];

                // update total circuit cost
                cost += c;
            }

            return cost;
        }
    }
}
