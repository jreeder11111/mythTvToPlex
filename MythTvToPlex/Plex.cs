using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MythTvToPlex
{
    public class Plex
    {
        public const string TRANSCODED_FILE_SUFFIX = ".m4v";
        public const string HANDBRAKECLI_EXE = @"C:\Program Files\Handbrake\HandBrakeCLI.exe";

        public Plex(string sharePath)
        {
            SharePath = sharePath;
        }

        public string SharePath { get; private set; }

        public void Transcode(Recording r, string originalFile, string transcodedFile)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = HANDBRAKECLI_EXE;
            startInfo.Arguments = "-Z \"High Profile\" -i \"" + originalFile + "\" -o \"" + transcodedFile + "\"";

            using (Process p = new Process())
            { 
                p.StartInfo = startInfo;
                p.Start();
                p.PriorityClass = ProcessPriorityClass.BelowNormal;
                p.WaitForExit();
            }
        }

        public void CopyToPlex(Recording r, string filename)
        {
            string destDir = Path.Combine(SharePath, GetPlexShowDirectory(r), GetPlexSeasonDirectory(r));
            Directory.CreateDirectory(destDir);
            string destFile = Path.Combine(destDir, GetPlexFileName(r));
            File.Copy(filename, destFile);
        }

        public string GetPlexFileName(Recording r)
        {
            string rval = String.Format("{0} - s{1:00}e{2:00} - {3}{4}",
                r.Title, r.Season, r.Episode, r.SubTitle, TRANSCODED_FILE_SUFFIX);
            return rval;
        }

        public string GetPlexShowDirectory(Recording r)
        {
            return r.Title;
        }

        public string GetPlexSeasonDirectory(Recording r)
        {
            string rval = String.Format("Season {0:00}", r.Season);
            return rval;
        }
    }
}
