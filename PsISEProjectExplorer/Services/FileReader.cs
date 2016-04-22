using NLog;
using PsISEProjectExplorer.Model;
using System;
using System.Collections.Generic;
using System.IO;

namespace PsISEProjectExplorer.Services
{
    [Component]
    public class FileReader
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // note: exceptions not handled
        public string ReadFileAsString(string path) 
        {
            using (var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {
                using (var bs = new BufferedStream(fs))
                {
                    using (var sr = new StreamReader(bs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }

        // note: exceptions handled and ignored (logged only)
        public IEnumerable<LineInfo> ReadFileAsEnumerableWithWrap(string path, int startLine)
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
                Logger.Error(e, "Cannot open file '" + path + "'");
                if (sr != null)
                    sr.Dispose();
                if (bs != null)
                    bs.Dispose();
                if (fs != null)
                    fs.Dispose();
                yield break;
            }

            string line;
            IList<string> wrappedLines = new List<string>(startLine - 1);
            int lineNum = 0;
            String startLineString = string.Empty;
            do 
            {
                try 
                {
                    line = sr.ReadLine();
                    lineNum++;
                } 
                catch (Exception e)
                {
                    Logger.Error(e, "Cannot read from file '" + path + "'");
                    sr.Dispose();
                    bs.Dispose();
                    fs.Dispose();
                    yield break;
                }
                if (line != null)
                {
                    if (lineNum >= startLine)
                    {
                        if (lineNum == startLine)
                        {
                            startLineString = line;
                        }
                        yield return new LineInfo(line, lineNum);
                    }
                    else
                    {
                        wrappedLines.Add(line);
                    }
                }
            } 
            while (line != null);
            sr.Dispose();
            bs.Dispose();
            fs.Dispose();

            int i = 1;
            foreach (string wrappedLine in wrappedLines)
            {
                yield return new LineInfo(wrappedLine, i++);
            }
            yield return new LineInfo(startLineString, i);

        }
    }
}
