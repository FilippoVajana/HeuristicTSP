using System;

namespace TspApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var reader = new Data();
            var data = reader.ReadData();
            var distancesMatrix = reader.ParseData(data);
            Console.WriteLine("");
        }
    }
}
