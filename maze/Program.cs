using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace maze
{
    class Program
    {
        static void Main()
        {
            var args = Environment.GetCommandLineArgs();
            var in_file = (args.Length > 1) ? args.ElementAt(1) : @"maze.txt";
            maze.find_path(in_file);
        }
    }
}
