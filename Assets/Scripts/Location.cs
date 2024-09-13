namespace Assets.Scripts
{
    public struct Location
    {
        public string City { get; set; }
        public string Country { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }


        public override readonly string ToString()
        {
            return $"{City}, {Country} \nLatitude: {Latitude}, Longitude: {Longitude}";
        }
    }
}