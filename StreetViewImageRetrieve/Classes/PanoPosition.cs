namespace StreetViewImageRetrieve
{
    public class PanoPosition
    {
        public float Latitude { get; set; }

        public float Longitude { get; set; }

        public PanoPosition(float latitude, float longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }   
    }
}
