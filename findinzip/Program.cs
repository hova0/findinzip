using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using ICSharpCode.SharpZipLib.Zip;
//using System.IO.Compression;

namespace findinzip
{
    class Program
    {
        public static string s7zlocation;

        static int Main(string[] args)
        {
            //locate 7z.dll!
            s7zlocation = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\7-zip\\7z.dll";
            if (!System.IO.File.Exists(s7zlocation))
            {
                Console.WriteLine("7z dll not found in " + s7zlocation);
                s7zlocation = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + "\\7-zip\\7z.dll";
                if (!System.IO.File.Exists(s7zlocation))
                    throw new Exception("7z dll not found in " + s7zlocation + ".  Program cannont continue \r\n Install 7-Zip!");
            }
            Console.WriteLine("7z DLL loaded from " + s7zlocation);
            SevenZip.SevenZipBase.SetLibraryPath(s7zlocation);

            // Only accept 2 or 4 arguments
            if (args == null || args.Length > 4 || args.Length < 2 || args.Length == 3)
            {
                PrintHelp();
                return 1;
            }

            if(args.Length >= 3 && args[2] != "-text")
            { PrintHelp(); return 1; }
            string zipfilename = args[0];
            string zipfilemaskregex = convertGlobtoRegex(args[1]);
            string searchText = null;
            if (args.Length == 4 && args[2] == "-text")
            {
                //text search in files
                searchText = args[3];
                if(String.IsNullOrEmpty(searchText))
                {
                    PrintHelp();
                    return 1;
                }
            } 

            Console.WriteLine("Beginning search in filename {0} matching only files {1} and searching for text \"{2}\"", zipfilename, args[1], searchText);
            string[] foundfiles;
            if (!System.IO.File.Exists(zipfilename))
            {
                if(String.IsNullOrWhiteSpace(System.IO.Path.GetDirectoryName(zipfilename))) 
                    foundfiles = System.IO.Directory.GetFiles(Environment.CurrentDirectory, zipfilename, System.IO.SearchOption.TopDirectoryOnly);
                else 
                    foundfiles = System.IO.Directory.GetFiles(System.IO.Path.GetDirectoryName(zipfilename), System.IO.Path.GetFileName(zipfilename), System.IO.SearchOption.TopDirectoryOnly);
            }
            else
                foundfiles = new string[1] { zipfilename };

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

        private static void PrintHelp()
        {
            Console.WriteLine("findinzip.exe <zip file(s)> <filenames inside zip> [-text \"Search string inside files inside zip\"");
        }

        //Only searches the zip file for filenames
        private static void Iteratezipfile(string zipfilename, string filemask)
        {
            Unzipper u = new Unzipper(zipfilename);
            zipfilename = System.IO.Path.GetFileName(zipfilename);
            foreach (string ze in u.GetNextEntry())
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(ze, filemask))
                {
                    Console.WriteLine("{0} - {1}", zipfilename, ze);
                }

            }
            u.Dispose();
        }
        //Searches for text inside files inside zip files
        private static void IterateZipFileSearchInFiles(string zipfilename, string filemask, string searchtext)
        {
            Unzipper u = new Unzipper(zipfilename);
            zipfilename = System.IO.Path.GetFileName(zipfilename);
            foreach (string ze in u.GetNextEntry())
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(ze, filemask))
                {
                    SearchinZipEntry(ze,  searchtext, zipfilename, u);
                }

            }
            
            u.Dispose();
        }

        //Searches for text inside a ZipEntry (file inside zip)
        private static void SearchinZipEntry(string ze, string searchtext, string zipfilename, Unzipper fz)
        {
            System.IO.MemoryStream unextractstream = new System.IO.MemoryStream();

            //System.IO.Stream zipstream = new //ze.Open();
            //zipstream.CopyTo(unextractstream);
            //zipstream.Dispose();
            fz.fz.ExtractFile(ze, unextractstream);

            

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
                    Console.WriteLine("{0}({1}) Line {2}:\t{3}", zipfilename, ze, lineposition, line);
                }
                else if (searchtextfound && line.Length >= 512)
                {
                    Console.WriteLine("{0}({1}) Line {2}:\t<Line too long>", zipfilename, ze, lineposition);
                }
            }
            sr.Dispose();

            unextractstream.Close();
            unextractstream.Dispose();
        }
        //Converts globs to regex so that we can match on files inside zips
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


    /// <summary>
    /// Class that will lazily enumerate each zip entry in a zipfile
    /// </summary>
    public class Unzipper : IDisposable
    {

        private bool disposedValue = false; // To detect redundant calls
        private string _zipfile;

        public SevenZip.SevenZipExtractor fz;

        public Unzipper(string zipfile)
        {
            _zipfile = zipfile;
            fz = new SevenZip.SevenZipExtractor(zipfile);
        }

        public IEnumerable<string> GetNextEntry()
        {
            int zipi = 0;
            foreach (string x in fz.ArchiveFileNames)
            {
                zipi++;
                if (String.IsNullOrEmpty(x) )
                    continue;
                yield return x;
            }
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    fz.Dispose();

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
