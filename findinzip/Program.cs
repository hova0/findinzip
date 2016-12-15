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
            string zipfilename = args[0];
            string zipfilemaskregex = convertGlobtoRegex(args[1]);
            bool searchFilenamesOnly = true;
            string searchText = null;
            if (args.Length == 4 && args[2] == "-text")
            {
                //text search in files
                searchFilenamesOnly = false;
                searchText = args[3];
            }

            Console.WriteLine("Beginning search in filename {0} matching only files {1} and searching for text \"{2}\"", zipfilename, zipfilemaskregex, searchText);
            string[] foundfiles = System.IO.Directory.GetFiles(Environment.CurrentDirectory, zipfilename, System.IO.SearchOption.TopDirectoryOnly);
            foreach (string zipfile in foundfiles)
            {
                if (String.IsNullOrEmpty(searchText))
                    Iteratezipfile(zipfile, zipfilemaskregex);
                else
                    IterateZipFileSearchInFiles(zipfile, zipfilemaskregex, searchText);
            }
            if (foundfiles.Length == 0)
                Console.WriteLine("No files found that match {0}", zipfilename);

            return 0;
        }

        private static void Iteratezipfile(string zipfilename, string filemask)
        {
            Unzipper u = new Unzipper(zipfilename);
            zipfilename = System.IO.Path.GetFileName(zipfilename);
            foreach (ZipEntry ze in u.GetNextEntry())
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(ze.Name, filemask))
                {
                    Console.WriteLine("{0} - {1}", zipfilename, ze.Name);
                }

            }
            u.Dispose();
        }
        private static void IterateZipFileSearchInFiles(string zipfilename, string filemask, string searchtext)
        {
            Unzipper u = new Unzipper(zipfilename);
            zipfilename = System.IO.Path.GetFileName(zipfilename);
            foreach (ZipEntry ze in u.GetNextEntry())
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(ze.Name, filemask))
                {
                    //Console.WriteLine("{0} - {1}", zipfilename, ze.Name);
                    SearchinZipEntry(ze, u.fz, searchtext, zipfilename);
                }

            }
            u.Dispose();
        }


        private static void SearchinZipEntry(ZipEntry ze, ZipFile zfx, string searchtext, string zipfilename)
        {
            System.IO.MemoryStream unextractstream = new System.IO.MemoryStream();
            if (ze.IsFile)
            {
                System.IO.Stream zipstream = zfx.GetInputStream(ze);
                zipstream.CopyTo(unextractstream);
                zipstream.Dispose();
            }
            int lineposition = 0;
            unextractstream.Position = 0;
            System.IO.StreamReader sr = new System.IO.StreamReader(unextractstream);
            while (true)
            {
                string line = sr.ReadLine();
                if (line == null)
                    break;
                bool searchtextfound = line.Contains(searchtext);
                lineposition++;
                if (searchtextfound && line.Length < 512)
                {
                    Console.WriteLine("{0}({1}) Line {2}:\t{3}", zipfilename, ze.Name, lineposition, line);
                }else if(searchtextfound && line.Length >= 512)
                {
                    Console.WriteLine("{0}({1}) Line {2}:\t<Line too long>", zipfilename, ze.Name, lineposition);
                }
            }

            unextractstream.Close();

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

        public ICSharpCode.SharpZipLib.Zip.ZipFile fz;

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
