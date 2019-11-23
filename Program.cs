﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace TspApp
{
    class Program
    {
        static void Main(string[] args)
        {            
            //load and parse data
            string data = TSPData.ReadData();
            TSPData.AdjacencyMatrix distancesMatrix = TSPData.ParseData(data);
            Console.WriteLine(distancesMatrix.ToString);

            //run the algorithm
            int runs = 5;
            var runResults = new List<string>(runs * 2);
            var p = new Program();

            for (int r = 0; r < runs; r++)
            {
                var (circuit, cost) = p.Run(distancesMatrix);
                runResults.Add(string.Join(' ', circuit));
                runResults.Add(cost.ToString());
            }

            //save the results
            TSPData.SaveResults(runResults);                        
        }

        private (LinkedList<uint>, uint) Run(TSPData.AdjacencyMatrix matrix)
        {
            //distances matrix
            uint[,] distances = matrix.distances;
            int rowsCount = (int) Math.Sqrt(distances.Length);

            //select starting node
            var random = new Random();
            uint startNode = (uint) random.Next(maxValue: (int) rowsCount);

            //init circuit
            LinkedList<uint> circuitList = new LinkedList<uint>();
            circuitList.AddFirst(startNode);

            //init frontier
            var range = Enumerable.Range(0, rowsCount).ToList().ConvertAll(x => (uint) x);
            List<uint> frontierList = new List<uint>(range);
            frontierList.Remove(startNode);

            //DEBUG
            Console.WriteLine(string.Join(' ', circuitList));
            Console.WriteLine(string.Join(' ', frontierList));            


            for (int i = 0; i < rowsCount; i++)
            {
                Console.WriteLine($"Loop #{i}");
                //select next node to add to the circuit
                var (circuitNodeId, frontierNodeId) = SelectNextNode(distances, circuitList, frontierList);

                //add new node to circuit
                circuitList.AddAfter(circuitList.Find(circuitNodeId), frontierNodeId);

                //remove node from frontier
                frontierList.Remove(frontierNodeId);

                //DEBUG
                Console.WriteLine(string.Join(' ', circuitList));
                Console.WriteLine(string.Join(' ', frontierList));                
            }

            //return circuit and cost
            return (circuitList, CircuitCost(distances, circuitList));
        }
        
        private List<uint> FilterFrontier(uint[,] matrix, uint currentNodId, List<uint> frontierList)
        {
            var filteredFrontier = new List<uint>(frontierList);
            double[] costs = new double[filteredFrontier.Count];
            double costsSum = 0;

            //compute frontier nodes costs
            for (int i = 0; i < filteredFrontier.Count; i++)
            {
                costs[i] = matrix[currentNodId, i];
                costsSum += costs[i];
            }
            
            //normalize costs
            for (int i = 0; i < costs.Length; i++)
            {
                costs[i] = costs[i] / costsSum; //FIX divide by the max value
            }

            //DEBUG
            Console.WriteLine(string.Join(' ', costs));

            var random = new Random();
            var k = random.NextDouble(); //FIX k values to big in respect to node costs

            //filter normalized costs
            for (int i = 0; i < costs.Length; i++)
            {
                if (k >= costs[i])
                    filteredFrontier.Remove(frontierList[i]); //TODO check coherence between costs and frontier
            }

            //DEBUG
            Console.WriteLine(k);
            Console.WriteLine("Filtered frontier");
            Console.WriteLine(string.Join(' ', filteredFrontier));

            return filteredFrontier;
        }


        private (uint,uint) SelectNextNode(uint[,] matrix, LinkedList<uint> circuit, List<uint> frontier)
        {
            uint circuitNodeId = 0;
            uint frontierNodeId = 0;
            uint minimumAddCost = uint.MaxValue;
            
            //select a node from the circuit
            foreach (uint circuitNode in circuit) 
            {
                uint circuitNextNodeId;
                if (circuitNode == circuit.Last.Value)
                    circuitNextNodeId = circuit.First.Value; //simulate a circular linked list
                else
                    circuitNextNodeId = circuit.Find(circuitNode).Next.Value;

                //filter frontier
                List<uint> filteredFrontier = FilterFrontier(matrix, circuitNodeId, frontier);

                //select a node from the frontier
                foreach (uint ext in filteredFrontier) 
                {                    
                    uint extAddCost = matrix[circuitNode, ext] + matrix[ext, circuitNextNodeId] - matrix[circuitNode, circuitNextNodeId];
                    if (extAddCost <= minimumAddCost)
                    {
                        //update best node so far
                        minimumAddCost = extAddCost;
                        circuitNodeId = circuitNode;
                        frontierNodeId = ext;
                    }
                }                
            }

            return (circuitNodeId, frontierNodeId);
        }

        private uint CircuitCost(uint[,] distanceMatrix, LinkedList<uint> circuitList)
        {
            uint cost = 0;

            foreach (uint node in circuitList)
            {
                //get next node
                uint next;
                if (node == circuitList.Last.Value)
                    next = circuitList.First.Value;
                else
                    next = circuitList.Find(node).Next.Value;

                //compute segment cost
                var c = distanceMatrix[node, next];

                //update total circuit cost
                cost += c;
            }

            return cost;
        }

    }
}
