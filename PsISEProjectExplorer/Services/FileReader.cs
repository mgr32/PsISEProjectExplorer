using NLog;
using PsISEProjectExplorer.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.Services
{
    public class FileReader
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static string ReadFileAsString(string path) {
            try
            {
                using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
                {
                    using (BufferedStream bs = new BufferedStream(fs))
                    {
                        using (StreamReader sr = new StreamReader(bs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
            catch (IOException e)
            {
                logger.Error("Cannot read file '" + path + "'");
                return null;
            }
        }

        public static IEnumerable<LineInfo> ReadFileAsEnumerableWithWrap(string path, int startLine)
        {
            FileStream fs = null;
            BufferedStream bs = null;
            StreamReader sr = null;
            try
            {
                fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite);
                bs = new BufferedStream(fs);
                sr = new StreamReader(bs);
            }
            catch (IOException e)
            {
                logger.Error("Cannot open file '" + path + "'");
                if (sr != null)
                    sr.Dispose();
                if (bs != null)
                    bs.Dispose();
                if (fs != null)
                    fs.Dispose();
                yield break;
            }
            
            string line = null;
            IList<string> wrappedLines = new List<string>(startLine - 1);
            int lineNum = 0;
            do 
            {
                try {
                    line = sr.ReadLine();
                    lineNum++;
                } catch {
                    logger.Error("Cannot read from file '" + path + "'");
                    if (sr != null)
                        sr.Dispose();
                    if (bs != null)
                        bs.Dispose();
                    if (fs != null)
                        fs.Dispose();
                    yield break;
                }
                if (line != null)
                {
                    if (lineNum >= startLine)
                    {
                        yield return new LineInfo(line, lineNum);
                    }
                    else
                    {
                        wrappedLines.Add(line);
                    }
                }
            } while (line != null);
            if (sr != null)
                sr.Dispose();
            if (bs != null)
                bs.Dispose();
            if (fs != null)
                fs.Dispose();

            int i = 1;
            foreach (string wrappedLine in wrappedLines)
            {
                yield return new LineInfo(wrappedLine, i++);
            }

        }
    }
}
