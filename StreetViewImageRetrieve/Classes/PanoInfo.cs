using System.Collections;
using System.Collections.Generic;

namespace StreetViewImageRetrieve
{
    public class PanoInfo : EqualityComparer<PanoInfo>
    {
        public string PanoId { get; set; }

        public PanoPosition Position { get; set; }

        public PanoInfo(string panoID, PanoPosition position) 
        {
            PanoId = panoID;
            Position = position;
        }

        public override bool Equals(PanoInfo x, PanoInfo y)
        {
            return x.PanoId.Equals(y.PanoId);
        }

        public override int GetHashCode(PanoInfo obj)
        {
            return obj.PanoId.GetHashCode();
        }

        public override string ToString()
        {
            return PanoId + " - " + Position;
        }
    }
}
