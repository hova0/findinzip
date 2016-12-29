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
        static int Main(string[] args)
        {



            SevenZip.SevenZipBase.SetLibraryPath(Program.Get7zLocation());
            if (args == null || args.Length < 2)
            {
                Console.WriteLine("findinzip <filename.zip> <file_mask> -text <string>");
                return 1;
            }
            string zipfilename = args[0];
            string zipfilemaskregex = convertGlobtoRegex(args[1]);
            string searchText = null;
            if (args.Length == 4 && args[2] == "-text")
            {
                //text search in files
                searchText = args[3];
            }

            Console.WriteLine("Beginning search in filename {0} matching only files {1} and searching for text \"{2}\"", zipfilename, zipfilemaskregex, searchText);

            string[] foundfiles;
            if (!System.IO.File.Exists(zipfilename))
                foundfiles = System.IO.Directory.GetFiles(Environment.CurrentDirectory, zipfilename, System.IO.SearchOption.TopDirectoryOnly);
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
            //Unzipper u = new Unzipper(zipfilename);
            SevenZip.SevenZipExtractor se = new SevenZip.SevenZipExtractor(zipfilename);
            string[] allfiles = new string[se.FilesCount];
            se.ArchiveFileNames.CopyTo(allfiles, 0);
            se.Dispose();
            int maxfiles = allfiles.Length;   // more than 2b files?

            //zipfilename = System.IO.Path.GetFileName(zipfilename);
            //System.Threading.Tasks.Parallel.ForEach<string>(allfiles/*, new System.Threading.Tasks.ParallelOptions { MaxDegreeOfParallelism = 4 }*/, ze =>
            System.Threading.Tasks.Parallel.For(0, maxfiles, ze => 
            {
                //foreach (string ze in u.GetNextEntry())
                //{
                if (System.Text.RegularExpressions.Regex.IsMatch(allfiles[ze], filemask))
                {
                    try
                    {
                        SearchinZipEntry(ze, searchtext, zipfilename);
                    }catch(Exception f)
                    {
                        Console.WriteLine("Failed to extract {0} from {1}", ze, zipfilename);
                        throw;
                    }
                }

            });
            
            
        }

        //Searches for text inside a ZipEntry (file inside zip)
        private static void SearchinZipEntry(string ze, string searchtext, string zipfilename)
        {
            System.IO.MemoryStream unextractstream = new System.IO.MemoryStream();

            //System.IO.Stream zipstream = new //ze.Open();
            //zipstream.CopyTo(unextractstream);
            //zipstream.Dispose();
            using (SevenZip.SevenZipExtractor se = new SevenZip.SevenZipExtractor(zipfilename))
            {
                se.ExtractFile(ze, unextractstream);
                //fz.fz.ExtractFile(ze, unextractstream);
                string shortzipfilename = System.IO.Path.GetFileName(zipfilename);


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
                        Console.WriteLine("{0}({1}) Line {2}:\t{3}", shortzipfilename, ze, lineposition, line);
                    }
                    else if (searchtextfound && line.Length >= 512)
                    {
                        Console.WriteLine("{0}({1}) Line {2}:\t<Line too long>", shortzipfilename, ze, lineposition);
                    }
                }
                sr.Dispose();

                unextractstream.Close();
                unextractstream.Dispose();
                se.Dispose();
            }
        }
        //Searches for text inside a ZipEntry (file inside zip)
        private static void SearchinZipEntry(int ze, string searchtext, string zipfilename)
        {
            System.IO.MemoryStream unextractstream = new System.IO.MemoryStream();

            //System.IO.Stream zipstream = new //ze.Open();
            //zipstream.CopyTo(unextractstream);
            //zipstream.Dispose();
            using (SevenZip.SevenZipExtractor se = new SevenZip.SevenZipExtractor(zipfilename))
            {

                se.ExtractFile(ze, unextractstream);
                //fz.fz.ExtractFile(ze, unextractstream);
                string shortzipfilename = System.IO.Path.GetFileName(zipfilename);


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
                        Console.WriteLine("{0}({1}) Line {2}:\t{3}", shortzipfilename, ze, lineposition, line);
                    }
                    else if (searchtextfound && line.Length >= 512)
                    {
                        Console.WriteLine("{0}({1}) Line {2}:\t<Line too long>", shortzipfilename, ze, lineposition);
                    }
                }
                sr.Dispose();

                unextractstream.Close();
                unextractstream.Dispose();
                se.Dispose();
            }
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

        public static string Get7zLocation()
        {
            //locate 7z.dll!
            string s7zlocation = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\7-zip\\7z.dll";
            if (!System.IO.File.Exists(s7zlocation))
            {
                //Console.WriteLine("7z dll not found in " + s7zlocation);
                s7zlocation = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + "\\7-zip\\7z.dll";
                if (!System.IO.File.Exists(s7zlocation))
                    throw new Exception("7z dll not found in " + s7zlocation);
            }
            //Console.WriteLine("7z DLL loaded from " + s7zlocation);
            return s7zlocation;
        }

    }


    /// <summary>
    /// Class that will lazily enumerate each zip entry in a zipfile
    /// </summary>
    public class Unzipper : IDisposable
    {

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        private string _zipfile;

        public SevenZip.SevenZipExtractor fz;

        public Unzipper(string zipfile)
        {
            _zipfile = zipfile;
            //fz = ZipFile.OpenRead(_zipfile);
            //fz.UseZip64 = UseZip64.On;

           
            SevenZip.SevenZipBase.SetLibraryPath(Program.Get7zLocation());
            fz = new SevenZip.SevenZipExtractor(zipfile);
        }

        public IEnumerable<string> GetNextEntry()
        {
            int zipi = 0;
            
            //System.Collections.IEnumerator fzen = fz.GetEnumerator();
            foreach (string x in fz.ArchiveFileNames)
            {
                zipi++;
                
                if (String.IsNullOrEmpty(x) )
                    continue;
                yield return x;
                
            }
            //Console.WriteLine("Zip index : {0}", zipi);

        }

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
