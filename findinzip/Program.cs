using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;

namespace findinzip
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args == null || args.Length < 2)
            {
                Console.WriteLine("findinzip <filename.zip> <file_mask> -text <string>");
                return 1;
            }
            try
            {
                if (args.Length > 0 && !System.IO.File.Exists(args[0]))
                {
                    Console.WriteLine("Zip file does not exist or access denied");
                    return 1;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Invalid filename or access denied.");
                return 1;
            }
            string zipfilename = args[0];
            string zipfilemaskregex = convertGlobtoRegex(args[1]);
            bool searchFilenamesOnly = true;
            string searchText = null;
            if(args.Length == 4 && args[2] == "-text")
            {
                //text search in files
                searchFilenamesOnly = false;
                searchText = args[3];
            }

            Console.WriteLine("Beginning search in filename {0} matching only files {1} and searching for text \"{2}\"", zipfilename, zipfilemaskregex, searchText);

            Iteratezipfile(zipfilename, zipfilemaskregex, searchText);




            return 0;
        }

        private static void Iteratezipfile(string zipfilename, string filemask, string searchtext)
        {
            Unzipper u = new Unzipper(zipfilename);
            foreach(ZipEntry ze in u.GetNextEntry())
            {
                if(System.Text.RegularExpressions.Regex.IsMatch(ze.Name, filemask)) {
                    Console.WriteLine("{0} - {1}", zipfilename,  ze.Name);
                }

            }
            u.Dispose();
        }

        public static string convertGlobtoRegex(string glob)
        {
            string regexstr = "";
            for (int i = 0; i < glob.Length; i++)
            {
                switch (glob[i])
                {
                    case '?':
                        regexstr += ".";
                        break;
                    case '*':
                        regexstr += ".*?";
                        break;
                    case '.':
                        regexstr += @"\.";
                        break;
                    case '\\':
                        regexstr += @"\\";
                        break;
                    case '+':
                        regexstr += @"\+";
                        break;
                    case '(':
                        regexstr += @"\(";
                        break;
                    case ')':
                        regexstr += @"\)";
                        break;
                    default:
                        regexstr += glob[i];
                        break;
                }
            }
            return regexstr;
        }

    }

  

    public class Unzipper : IDisposable
    {

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        private string _zipfile;

        private ICSharpCode.SharpZipLib.Zip.ZipFile fz;

        public Unzipper(string zipfile)
        {
            _zipfile = zipfile;
            fz = new ICSharpCode.SharpZipLib.Zip.ZipFile(_zipfile);
            
        }

        public IEnumerable<ZipEntry> GetNextEntry()
        {
            ZipEntry e = null;

            //System.Collections.IEnumerator fzen = fz.GetEnumerator();
            foreach (object x in fz)
            {
                if (x is ZipEntry)
                {
                    e = x as ZipEntry;
                    yield return e;
                }
            }

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    fz.Close();

                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Unzipper() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }


}
