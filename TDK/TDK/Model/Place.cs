using System;
using System.Collections.Generic;
using System.Text;

namespace TDK.Model
{
    public class Place
    {
        public string Id { get; set; }

        public string PlaceName { get; set; }

        public string PlaceDescription { get; set; }

        public string PlaceAddess { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }
}
