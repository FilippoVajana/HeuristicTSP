using System;
using System.Collections.Generic;

namespace TspApp
{
    class Program
    {
        static void Main(string[] args)
        {            
            string data = Data.ReadData();
            var distancesMatrix = Data.ParseData(data);
            Console.WriteLine(distancesMatrix.ToString);
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
