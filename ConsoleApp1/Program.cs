using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{

    class Options
    {
        [Option('t', "title", HelpText = "Title of show")]
        public string Title { get; set; }

        [Option('n', "maxnumber", DefaultValue = 1, HelpText = "Max number of shows to copy")]
        public int MaxNumberToCopy { get; set; }

        [Option("dry-run", HelpText = "Dry-run")]
        public bool DryRun { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText
            {
                Heading = new HeadingInfo("Sync recordings from MythTv to Plex")
            };

            help.AddOptions(this);

            return help;
        }
    }

    partial class Program
    {
        public const string TEMP_DIR = @"c:\temp";

        static void Main(string[] args)
        {
            string step = null;
            try
            {
                var options = new Options();
                if (CommandLine.Parser.Default.ParseArguments(args, options))
                {
                    // Load recordings from MythTv
                    step = "Loading recordings from MythTv";
                    Console.Out.WriteLine(step);
                    MythTv mythtv = new MythTv("fattony", 6544);
                    mythtv.LoadRecordings(options.Title);
                    mythtv.RemoveRecordingsWithoutSeasonOrEpisode();
                    mythtv.SortRecordingsBySeasonAndEpisode();

                    // Load recordings from database
                    step = "Loading recordings from database";
                    Console.Out.WriteLine(step);
                    Database database = new Database();
                    database.LoadRecordings(@"recordings.txt");

                    // Look for new recordings in MythTv
                    step = "Looking for new recordings";
                    Console.Out.WriteLine(step);
                    var newRecordings = mythtv.Recordings.Except(database.Recordings, new RecordingShowDataComparer()).ToList();
                    Console.Out.WriteLine("Found " + newRecordings.Count + " new recordings");

                    int numberToCopy = (options.MaxNumberToCopy > 1 ? options.MaxNumberToCopy : 1);

                    while (numberToCopy > 0 && newRecordings.Count > 0)
                    {
                        numberToCopy--;

                        // Copy recordings to Plex
                        /// Copy the file and mark it copied in the database
                        /// Transcode the file and mark it transcoded in the database
                        var newRecording = newRecordings[0];
                        newRecordings.RemoveAt(0);

                        step = "Copying 1 recording: " + newRecording.Title + ": " + newRecording.SubTitle;
                        Console.Out.WriteLine(step);

                        step = "Downloading file";
                        Console.Out.WriteLine(step);
                        string tempFile = Path.Combine(TEMP_DIR, newRecording.FileName);
                        if (!options.DryRun) mythtv.DownloadRecording(newRecording, tempFile);

                        step = "Transcoding";
                        Console.Out.WriteLine(step);
                        Plex plex = new Plex(@"\\CRABAPPLE\Recorded TV");
                        string transcodedFile = Path.Combine(TEMP_DIR, plex.GetPlexFileName(newRecording));
                        if (!options.DryRun) plex.Transcode(newRecording, tempFile, transcodedFile);

                        step = "Copying to plex";
                        Console.Out.WriteLine(step);
                        if (!options.DryRun) plex.CopyToPlex(newRecording, transcodedFile);
                        database.Recordings.Add(newRecording);

                        step = "Saving database";
                        Console.Out.WriteLine(step);
                        if (!options.DryRun) database.SaveRecordings();

                        step = "Deleting temporary files";
                        if (!options.DryRun)
                        {
                            File.Delete(tempFile);
                            File.Delete(transcodedFile);
                        }
                    }
                }
                else
                {
                    // Help text is automatically printed (I'm guessing from the parse code)
                }
            }
            catch (Exception e)
            {
                Console.Out.WriteLine("Got exception in step " + step + ": " + e.Message);
            }

            //Console.ReadKey();
            return;

            //{
            //    FattonyContent.ContentClient client = new FattonyContent.ContentClient();
            //    var aos = client.GetFileList("default");
            //    Console.Out.WriteLine(aos.ToString());
            //}

            //    {
            //        FattonyDvr.DvrClient client = new FattonyDvr.DvrClient();
            //    var aos = client.GetTitleList();
            //    Console.Out.WriteLine(aos.ToString());
            //}

            //{
            //    FattonyDvr.DvrClient client = new FattonyDvr.DvrClient();
            //    var til = client.GetTitleInfoList();
            //    var tils = til.TitleInfos;
            //    Console.Out.WriteLine(til.ToString());
            //}

            //{
            //    FattonyDvr.DvrClient client = new FattonyDvr.DvrClient();
            //    var program = client.GetRecorded(1, DateTime.Now);
            //    Console.Out.WriteLine(program);
            //}

            //{
            //    FattonyChannel.ChannelClient client = new FattonyChannel.ChannelClient();
            //    var videoSourceList = client.GetVideoSourceList();
            //    var videoSource = videoSourceList.VideoSources.ElementAt(0);

            //    var channelInfoList = client.GetChannelInfoList(videoSource.Id, 0, 1000);

            //    var channelInfo = channelInfoList.ChannelInfos.ElementAt(0);

            //    FattonyDvr.DvrClient dvrClient = new FattonyDvr.DvrClient();
            //    var program = dvrClient.GetRecorded((int)channelInfo.ChanId, DateTime.MinValue);

            //}

            {
                FattonyDvr.DvrClient client = new FattonyDvr.DvrClient();
                string titleSearch = "squidbillies";
                bool keepGoing = true;
                int startIdx = 0;
                int count = 10;
                List<Recording> recordings = new List<Recording>();

                do
                {
                    var result = client.GetRecordedList(true, startIdx, count, titleSearch, null, null);
                    startIdx += count;

                    if (result.Count == 0)
                    {
                        keepGoing = false;
                    }

                    foreach (var program in result.Programs)
                    {
                        recordings.Add(new Recording() {
                            Title = program.Title,
                            SubTitle = program.SubTitle,
                            FileName = program.FileName,
                            Season = program.Season,
                            Episode = program.Episode,
                            RecGroup = program.Recording.RecGroup
                        });
                    }

                } while (keepGoing);

#if false
                try
                {
                    WebClient wc = new WebClient();
                    wc.DownloadFile(recordings[0].GetMythTvDownloadUrl("fattony.reederhome.com", 6544), @"c:\temp\" + recordings[0].GetPlexFileName());
                }
                catch (Exception e)
                {
                    string stack = e.StackTrace;
                    Console.Out.WriteLine(stack);

                    bool breakfordebugger = true;
                }

                Console.Out.WriteLine(recordings.Count);
#endif

#if false
                foreach (var r in recordings)
                {
                    if (r.Episode <= 0 || r.Season <= 0)
                        continue;

                    Console.Out.WriteLine(r.GetPlexFileName());
                }
                Console.ReadKey();
#endif
                //string s = Newtonsoft.Json.JsonConvert.SerializeObject(recordings);
                //Console.ReadKey();
            }
            //Console.ReadKey();
        }
    }
}
