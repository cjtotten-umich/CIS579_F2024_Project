using System.Collections;
using System.Collections.Generic;

namespace StreetViewImageRetrieve
{
    public class PanoInfo : EqualityComparer<PanoInfo>
    {
        public string PanoId { get; set; }

        public float Latitude { get; set; }

        public float Longitude { get; set; }


        public PanoInfo(string panoID, float latitude, float longitude) 
        {
            PanoId = panoID;
            Latitude = latitude;
            Longitude = longitude;
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
            return PanoId + " - " + Latitude + "," + Longitude;
        }
    }
}
