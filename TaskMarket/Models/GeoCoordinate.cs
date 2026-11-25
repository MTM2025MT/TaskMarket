using System.ComponentModel.DataAnnotations.Schema;

namespace TaskMarket.Models
{
    public  class GeoCoordinate
    {
        public  double Latitude { get; set; }   // -90  to +90
        public double Longitude { get; set; }   // -180 to +180
        public GeoCoordinate(double latitude, double longitude)
        {
            if (latitude < -90 || latitude > 90) throw new ArgumentOutOfRangeException(nameof(latitude));
            if (longitude < -180 || longitude > 180) throw new ArgumentOutOfRangeException(nameof(longitude));

            Latitude = latitude;
            Longitude = longitude;
        }

        public override string ToString() => $"{Latitude}°N, {Longitude}°E";
    }
}
