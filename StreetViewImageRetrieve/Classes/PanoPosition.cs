namespace StreetViewImageRetrieve
{
    public struct PanoPosition
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public PanoPosition(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public override string ToString()
        {
            return "(" + Latitude + ", " + Longitude + ")";
        }
    }
}
