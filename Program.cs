using System;
using System.Collections.Generic;

namespace TspApp
{
    class Program
    {
        static void Main(string[] args)
        {            
            string data = Data.ReadData();
            Data.AdjacencyMatrix distancesMatrix = Data.ParseData(data);
            Console.WriteLine(distancesMatrix.ToString);


            LinkedList<uint> cList = new LinkedList<uint>();
            cList.AddLast(0);
            cList.AddLast(1);

            List<uint> fList = new List<uint>() { 2, 3 };

            var newNode = new Program().SelectNextNode(distancesMatrix.distances, cList, fList);
            Console.WriteLine(newNode);

            //compute circuit cost
            cList.AddAfter(cList.Find(newNode.Item1), newNode.Item2);
            var cost = new Program().CircuitCost(distancesMatrix.distances, cList);
            Console.WriteLine(cost);
            
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


        private (uint,uint) SelectNextNode(uint[,] distanceMatrix, LinkedList<uint> circuitList, List<uint> frontierList)
        {
            uint circuitNodeId = 0;
            uint frontierNodeId = 0;
            uint minimumAddCost = uint.MaxValue;


            //select a node from the circuit
            foreach (uint circuitNode in circuitList) 
            {
                uint circuitNextNodeId;
                if (circuitNode == circuitList.Last.Value)
                    circuitNextNodeId = circuitList.First.Value; //simulate a circular linked list
                else
                    circuitNextNodeId = circuitList.Find(circuitNode).Next.Value;

                //select a node from the frontier
                foreach (uint ext in frontierList) 
                {                    
                    uint extAddCost = distanceMatrix[circuitNode, ext] + distanceMatrix[ext, circuitNextNodeId] - distanceMatrix[circuitNode, circuitNextNodeId];
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


    }
}
