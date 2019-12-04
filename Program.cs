﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace TspApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var data = new TSPData("./data/source", "./data/instances", "./data/results");

            var rawData = data.ReadSourceData("st70.tsp");

            var matrix = data.MatrixFrom2DPos(int.Parse(rawData[0]), rawData[2]);

            TSPData.PrintMatrix(matrix);

            data.SaveMatrix(matrix, "st70_mat.dat");


            ////load and parse data
            //string data = new TSPData(null, null, null).ReadData(null);
            //TSPData.AdjacencyMatrix distancesMatrix = TSPData.ParseData(data);
            //Console.WriteLine(distancesMatrix.ToString);

            ////run the algorithm
            //int runs = 200;
            //var results = new List<string>(runs * 2);
            //var p = new Program();

            //for (int r = 0; r < runs; r++)
            //{
            //    var (circuit, cost) = p.Run(distancesMatrix.distances);
            //    results.Add(string.Join(' ', circuit));
            //    results.Add(cost.ToString());
            //}

            ////save the results
            //TSPData.SaveResults(results);                        
        }

        private (LinkedList<uint>, uint) Run(uint[,] distances)
        {
            //distances matrix            
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


            for (int i = 0; i < rowsCount - 1; i++)
            {
                Console.WriteLine($"Loop #{i}");
                //select next node to add to the circuit
                var (circuitNodeId, frontierNodeId) = SelectNextNode(distances, circuitList, frontierList);

                //add new node to circuit
                circuitList.AddAfter(circuitList.Find(circuitNodeId), frontierNodeId);

                //remove node from frontier
                frontierList.Remove(frontierNodeId);

                //DEBUG
                Console.WriteLine("Circuit");
                Console.WriteLine(string.Join(' ', circuitList));

                Console.WriteLine("Frontier");
                Console.WriteLine(string.Join(' ', frontierList));                
            }

            //return circuit and cost
            return (circuitList, CircuitCost(distances, circuitList));
        }
        
        private static List<uint> FilterFrontier(uint[,] distances, uint currentNode, List<uint> frontier)
        {
            var filteredFrontier = new List<uint>(frontier);
            double[] costs = new double[filteredFrontier.Count];
            double costsSum = 0;

            //compute frontier nodes costs
            for (int i = 0; i < filteredFrontier.Count; i++)
            {
                costs[i] = distances[currentNode, i];
                costsSum += costs[i];
            }

            //rescale costs
            var minCost = costs.Min();
            var maxCost = costs.Max();
            for (int i = 0; i < costs.Length; i++)
            {
                costs[i] = Math.Round((costs[i] - minCost) / (maxCost - minCost),3);
            }

            //filter costs
            var random = new Random();
            var k = random.NextDouble();
            for (int i = 0; i < costs.Length; i++)
            {
                if (k >= costs[i])
                    filteredFrontier.Remove(frontier[i]);
            }

            return filteredFrontier;
        }
        
        private (uint,uint) SelectNextNode(uint[,] distances, LinkedList<uint> circuit, List<uint> frontier)
        {
            uint circuitNodeId = 0;
            uint frontierNodeId = 0;
            uint minimumAddCost = uint.MaxValue;
            
            //select a node from the circuit
            foreach (uint circuitNode in circuit) 
            {
                uint nextNodeId;
                if (circuitNode == circuit.Last.Value)
                    nextNodeId = circuit.First.Value; //simulate a circular linked list
                else
                    nextNodeId = circuit.Find(circuitNode).Next.Value;

                //filter frontier
                List<uint> filteredFrontier = FilterFrontier(distances, circuitNodeId, frontier);

                //select a node from the frontier
                foreach (uint ext in filteredFrontier) 
                {                    
                    uint extAddCost = distances[circuitNode, ext] + distances[ext, nextNodeId] - distances[circuitNode, nextNodeId];
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

        private static uint CircuitCost(uint[,] distances, LinkedList<uint> circuit)
        {
            uint cost = 0;

            foreach (uint node in circuit)
            {
                //get next node
                uint next;
                if (node == circuit.Last.Value)
                    next = circuit.First.Value;
                else
                    next = circuit.Find(node).Next.Value;

                //compute segment cost
                var c = distances[node, next];

                //update total circuit cost
                cost += c;
            }

            return cost;
        }

    }
}
