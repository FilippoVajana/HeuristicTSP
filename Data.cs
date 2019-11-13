using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace TspApp
{
    public class Data
    {
        public String ReadData()
        {
            using (StreamReader sr = new StreamReader("./data/bayg29.dat"))
            {
                var str = sr.ReadToEnd();
                return str;
            }
        }

        public AdjacencyMatrix ParseData(string rawData)
        {
            return new AdjacencyMatrix(rawData);
        }

        public struct AdjacencyMatrix
        {
            public uint[,] distances;

            public AdjacencyMatrix(String rawData)
            {                
                int rowCount = 1; // starts from 1 because the matrix miss the main diagonal
                using (StringReader sr = new StringReader(rawData))
                {
                    // count matrix rows
                    while (sr.ReadLine() != null)
                    {
                        rowCount += 1;
                    }

                    // init the distances matrix
                    distances = new uint[rowCount, rowCount];
                }                

                // fill the matrix
                using (StringReader sr = new StringReader(rawData))
                {  
                    for (int rowIdx = 0; rowIdx < rowCount - 1; rowIdx++)
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
        }
    }
}
