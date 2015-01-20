using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace findinzip
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                Console.WriteLine("findinzip <filename.zip> <file_mask>");
                return 1;
            }
            if (args.Length > 0 && !System.IO.File.Exists(args[0]))
            {
                Console.WriteLine("Zip file does not exist");
                return 1;
            }

            return 0;
        }
    }
}
