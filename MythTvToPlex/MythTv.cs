using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MythTvToPlex
{
    public class MythTv
    {
        public MythTv(string hostname, int wsdlPort)
        {
            Hostname = hostname;
            WsdlPort = wsdlPort;
            Recordings = new List<Recording>();
        }

        public string Hostname { get; private set; }
        public int WsdlPort { get; private set; }

        public List<Recording> Recordings { get; private set; }

        public void LoadRecordings(string title)
        {
            FattonyDvr.DvrClient client = new FattonyDvr.DvrClient();
            bool keepGoing = true;
            int startIdx = 0;
            int count = 10;
            List<Recording> recordings = new List<Recording>();

            do
            {
                var result = client.GetRecordedList(true, startIdx, count, title, null, null);
                startIdx += count;

                if (result.Count == 0)
                {
                    keepGoing = false;
                }

                foreach (var program in result.Programs)
                {
                    recordings.Add(new Recording()
                    {
                        Title = program.Title,
                        SubTitle = program.SubTitle,
                        FileName = program.FileName,
                        Season = program.Season,
                        Episode = program.Episode,
                        RecGroup = program.Recording.RecGroup
                    });
                }

            } while (keepGoing);

            this.Recordings = recordings;
        }

        public void RemoveRecordingsWithoutSeasonOrEpisode()
        {
            Recordings.RemoveAll(x => (x.Episode <= 0 || x.Season <= 0));
        }

        public void SortRecordingsBySeasonAndEpisode()
        {
            var r = Recordings.OrderBy(s => s.Season).ThenBy(e => e.Episode).ToList();
            Recordings = r;
        }

        public string GetMythTvDownloadUrl(Recording r, string hostname, int port)
        {
            UriBuilder uriBuilder = new UriBuilder();
            uriBuilder.Host = hostname;
            uriBuilder.Port = port;
            uriBuilder.Path = "/Content/GetFile";
            uriBuilder.Query = "StorageGroup=" + r.RecGroup + "&FileName=" + r.FileName;

            return uriBuilder.Uri.ToString();
        }

        public void DownloadRecording(Recording r, string filename)
        {
            using (WebClient wc = new WebClient())
            {
                wc.DownloadFile(GetMythTvDownloadUrl(r, Hostname, WsdlPort), filename);
            }
        }
    }
}
