using System;
using System.IO;

namespace MythTvToPlex
{
    [Serializable]
    public class Recording
    {
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string FileName { get; set; }
        public int Season { get; set; }
        public int Episode { get; set; }
        public string RecGroup { get; set; }
    }
}
