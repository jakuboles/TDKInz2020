using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDK.MapsCustoms;
using TDK.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TDK
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DiscoveredNewPlace : ContentPage
	{
        private Place place;
        private List<CustomCircle> circleList = new List<CustomCircle>();
        private List<Xamarin.Forms.Maps.Pin> pinList = new List<Xamarin.Forms.Maps.Pin>();

        public DiscoveredNewPlace()
		{
			InitializeComponent();
		}

        public DiscoveredNewPlace(Place place, List<CustomCircle> circleList, List<Xamarin.Forms.Maps.Pin> pinList)
        {
            InitializeComponent();
            this.place = place;
            this.circleList = circleList;
            this.pinList = pinList;

            placeName.Text = place.PlaceName;
            placeAddress.Text = place.PlaceAddess;
            PlaceDescription.Text = place.PlaceDescription;

            var removeCircle = circleList.First(x => x.Position.Latitude == place.Latitude && x.Position.Longitude == place.Longitude);
            circleList.Remove(removeCircle);

            var pin = createPin(place);
            pinList.Add(pin);
        }

        private Xamarin.Forms.Maps.Pin createPin(Place place)
        {
            var position = new Xamarin.Forms.Maps.Position(place.Latitude, place.Longitude);
            var pin = new Xamarin.Forms.Maps.Pin()
            {
                Type = Xamarin.Forms.Maps.PinType.Place,
                Position = position,
                Label = place.PlaceName,
                Address = place.PlaceAddess
            };
            return pin;
        }

        private async void OkButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new MainPage(circleList, pinList));
        }
    }
}