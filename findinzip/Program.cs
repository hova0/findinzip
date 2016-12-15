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
            if (args.Length == 4 && args[2] == "-text")
            {
                //text search in files
                searchFilenamesOnly = false;
                searchText = args[3];
            }

            Console.WriteLine("Beginning search in filename {0} matching only files {1} and searching for text \"{2}\"", zipfilename, zipfilemaskregex, searchText);


            if (String.IsNullOrEmpty(searchText))
                Iteratezipfile(zipfilename, zipfilemaskregex);
            else
                IterateZipFileSearchInFiles(zipfilename, zipfilemaskregex, searchText);
            return 0;
        }

        private static void Iteratezipfile(string zipfilename, string filemask)
        {
            Unzipper u = new Unzipper(zipfilename);
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
            //Now search the unextracted stream
            int searchlength = searchtext.Length;
            byte[] buff = new byte[4096];
            int bytesread = 1;
            unextractstream.Position = 0;
            int streamposition = 0;
            while (bytesread > 0)
            {
                bytesread = unextractstream.Read(buff, 0, 4096);
                if (bytesread < searchlength)
                    break;
                streamposition += bytesread - searchlength;
                string chunk = System.Text.Encoding.ASCII.GetString(buff, 0, bytesread);
                int chunki = 0;
                while (chunki >= 0)
                {
                    chunki = chunk.IndexOf(searchtext, chunki);
                    if (chunki >= 0)
                    {
                        //if(chunki > 20 &&  chunk.Length - (chunki) > 20 )
                        //    Console.WriteLine("{0} - {1}: {2} = {3}", zipfilename, ze.Name, streamposition + chunki, chunk.Substring(chunki - 20, chunki + searchlength+20));
                        //else
                        //    Console.WriteLine("{0} - {1}: {2} = {3}", zipfilename, ze.Name, streamposition + chunki, searchtext);
                        int prevcr = -1;
                        int nextcr = -1;
                        for (int i = chunki; i > 0; i--)
                        {
                            if (chunk[i] == '\n')
                            {
                                prevcr = i;
                                break;
                            }
                        }
                        for(int i = chunki; i < chunk.Length; i++)
                        {
                            if (chunk[i] == '\n')
                            {
                                nextcr = i;
                                break;
                            }
                        }
                        if(prevcr > 0 && nextcr > 0)
                            Console.WriteLine("{0} - {1}: {2} = {3}", zipfilename, ze.Name, streamposition + chunki, chunk.Substring(prevcr + 1, ( nextcr - 1) - prevcr));
                        else if(prevcr > 0)
                            Console.WriteLine("{0} - {1}: {2} = {3}", zipfilename, ze.Name, streamposition + chunki, chunk.Substring(prevcr + 1, chunki + searchlength));
                        chunki++;
                    }

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
