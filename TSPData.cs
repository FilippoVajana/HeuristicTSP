using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace TspApp
{
    public class TSPData
    {
        private string sourceDirPath, instancesDirPath, resultsDirPath;

        public TSPData(string sourceDirPath, string instancesDirPath, string resultsDirPath)
        {
            this.sourceDirPath = sourceDirPath;
            this.instancesDirPath = instancesDirPath;
            this.resultsDirPath = resultsDirPath;
        }

        public static string ReadData()
        {
            using (StreamReader sr = new StreamReader("./data/bayg29.dat"))
            {
                var str = sr.ReadToEnd();                
                return str;
            }
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
                var matches = System.Text.RegularExpressions.Regex.Matches(data, pattern);

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
