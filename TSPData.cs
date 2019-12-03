using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace TspApp
{
    public class TSPData
    {
        private string sourceDirPath, instancesDirPath, resultsDirPath;
        private Func<(int, int), (int, int), uint> EuclideanDistance = (a, b) =>
        {
            var dist = Math.Sqrt(Math.Pow((a.Item1 - b.Item1), 2) + Math.Pow((a.Item2 - b.Item2), 2));
            return (uint)dist;
        };

        public TSPData(string sourceDirPath, string instancesDirPath, string resultsDirPath)
        {
            this.sourceDirPath = sourceDirPath;
            this.instancesDirPath = instancesDirPath;
            this.resultsDirPath = resultsDirPath;
        }

        public string[] ReadData(string filePath)
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                var result = new string[3];

                // parse source file
                result[0] = sr.ReadLine();  //node count
                result[1] = sr.ReadLine();  //data format
                result[2] = sr.ReadToEnd(); //data

                return result;
            }
        }
        public uint[,] MatrixFrom2DPos(int size, string rawData)
        {
            var distanceDict = new Dictionary<int, (int, int)>(size);

            using (StringReader sr = new StringReader(rawData))
            {
                // parse 2D positions
                var rowPattern = @"\d+";
                while (sr.Peek() != -1)
                {
                    var match = Regex.Matches(sr.ReadLine(), rowPattern);
                    distanceDict.Add(
                        int.Parse(match[0].Value),
                        (int.Parse(match[1].Value), int.Parse(match[2].Value))
                        );
                }
            }

            // build distance matrix
            uint[,] matrix = new uint[size, size];
            foreach (var startNode in distanceDict.Keys)
            {
                foreach (var endNode in distanceDict.Keys)
                {
                    var distance = EuclideanDistance(distanceDict[startNode], distanceDict[endNode]);
                    matrix[startNode - 1, endNode - 1] = matrix[endNode - 1, startNode - 1] = distance;                    
                }
            }

            return matrix;
        }

        


        public static void SaveResults(List<string> results)
        {
            string name = $"{DateTime.Now.Day}{DateTime.Now.Hour}{DateTime.Now.Minute}{DateTime.Now.Second}";
            var file = File.CreateText($"./data/results/{name}.txt");
            using (StreamWriter sw = file)
            {
                foreach (var line in results)
                {
                    sw.WriteLine(line);
                }
                Console.WriteLine($"Results saved in folder: {((FileStream)(sw.BaseStream)).Name}");
            }
        }

        public static AdjacencyMatrix ParseData(string rawData)
        {
            return new AdjacencyMatrix(rawData);
        }


        public struct AdjacencyMatrix
        {
            public uint[,] distances;
            private uint matrixSize;

            public AdjacencyMatrix(String rawData)
            {                
                matrixSize = 1; // starts from 1 because the matrix miss the main diagonal
                using (StringReader sr = new StringReader(rawData))
                {
                    // count matrix rows
                    while (sr.ReadLine() != null)
                    {
                        matrixSize += 1;
                    }

                    // init the distances matrix
                    distances = new uint[matrixSize, matrixSize];
                }                

                // fill the matrix
                using (StringReader sr = new StringReader(rawData))
                {  
                    for (int rowIdx = 0; rowIdx < matrixSize - 1; rowIdx++)
                    {
                        var rowData = ParseRowData(sr.ReadLine());
                        for (int colIdx = 0; colIdx < rowData.Length; colIdx++)
                        {
                            distances[rowIdx, colIdx + rowIdx + 1] = rowData[colIdx];
                            distances[colIdx + rowIdx + 1, rowIdx] = rowData[colIdx];
                        }
                    }
                }
            }

            private uint[] ParseRowData(String data)
            {
                // get values using regular expression
                String pattern = @"\d+";
                var matches = Regex.Matches(data, pattern);

                // init data array
                uint[] rowData = new uint[matches.Count];

                // fill the array
                for (int idx = 0; idx < rowData.Length; idx++)
                {
                    rowData[idx] = uint.Parse(matches[idx].Value);
                }
                
                return rowData;
            }

            public new string ToString
            {
                get
                {
                    var sb = new StringBuilder();

                    for (int r = 0; r < matrixSize; r++)
                    {
                        for (int c = 0; c < matrixSize; c++)
                        {
                            sb.Append(string.Format("{0,3} ", distances[r, c]));
                        }
                        // new line
                        sb.Append(Environment.NewLine);
                    }

                    return sb.ToString();
                }
            }
        }
    }
}
