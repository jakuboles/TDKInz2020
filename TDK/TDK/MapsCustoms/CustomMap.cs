using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.Maps;

namespace TDK.MapsCustoms
{
    public class CustomMap : Map
    {
        private CustomMap locationsMap;

        public List<CustomCircle> CircleList { get; set; }
    }
}
