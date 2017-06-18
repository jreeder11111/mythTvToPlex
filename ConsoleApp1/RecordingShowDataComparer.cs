using System.Collections.Generic;

namespace ConsoleApp1
{
    public class RecordingShowDataComparer : IEqualityComparer<Recording>
    {
        public bool Equals(Recording x, Recording y)
        {
            if (object.ReferenceEquals(x, y))
            {
                return true;
            }
            if (object.ReferenceEquals(x, null) ||
                object.ReferenceEquals(y, null))
            {
                return false;
            }
            return (x.Title == y.Title &&
                x.SubTitle == y.SubTitle &&
                x.Season == y.Season &&
                x.Episode == y.Episode);
        }

        public int GetHashCode(Recording obj)
        {
            if (obj == null)
            {
                return 0;
            }
            return new { obj.Title, obj.SubTitle, obj.Season, obj.Episode }.GetHashCode();
        }
    }
}
