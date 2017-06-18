using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MythTvToPlex
{
    public class Database
    {
        public Database()
        {
            Recordings = new List<Recording>();
        }

        public List<Recording> Recordings { get; private set; }
        private bool recordingsLoaded = false;

        public string LoadedFilename { get; private set; }

        public void LoadRecordings(string filename)
        {
            try
            {
                using (StreamReader file = File.OpenText(filename))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    List<Recording> recordings = (List<Recording>)serializer.Deserialize(file, typeof(List<Recording>));
                    this.Recordings = recordings;
                    this.LoadedFilename = filename;
                    this.recordingsLoaded = true;
                }
            }
            catch (FileNotFoundException fne)
            {
                this.Recordings = new List<Recording>();
                this.LoadedFilename = filename;
                this.recordingsLoaded = true;
            }
        }

        public void SaveRecordings()
        {
            SaveRecordings(this.LoadedFilename);
        }

        public void SaveRecordings(string filename)
        {
            if (!this.recordingsLoaded)
                throw new InvalidOperationException("Recordings are not loaded");

            using (StreamWriter file = File.CreateText(filename))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, this.Recordings);
            }
        }
    }
}
