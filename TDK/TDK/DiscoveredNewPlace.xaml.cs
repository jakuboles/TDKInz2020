using SQLite;
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
        private List<Place> globalUndiscoveredPlaces = new List<Place>();
        private List<Place> globalDiscoveredPlaces = new List<Place>();
        private bool discoveredPlaceViewed = false;

        public DiscoveredNewPlace()
		{
			InitializeComponent();
		}

        public DiscoveredNewPlace(Place place)
        {
            InitializeComponent();
            this.place = place;
            this.discoveredPlaceViewed = true;

            labelNewPlace.Text = "";
            placeName.Text = place.PlaceName;
            placeAddress.Text = place.PlaceAddess;
            PlaceDescription.Text = place.PlaceDescription;
        }

        public DiscoveredNewPlace(Place place, List<CustomCircle> circleList, List<Xamarin.Forms.Maps.Pin> pinList, List<Place> globalUndiscoveredPlaces, List<Place> globalDiscoveredPlaces)
        {
            InitializeComponent();
            this.place = place;
            this.circleList = circleList;
            this.pinList = pinList;
            this.globalDiscoveredPlaces = globalDiscoveredPlaces;
            this.globalUndiscoveredPlaces = globalUndiscoveredPlaces;

            placeName.Text = place.PlaceName;
            placeAddress.Text = place.PlaceAddess;
            PlaceDescription.Text = place.PlaceDescription;

            globalUndiscoveredPlaces.Remove(place);
            globalDiscoveredPlaces.Add(place);

            var removeCircle = circleList.First(x => x.Position.Latitude == place.Latitude && x.Position.Longitude == place.Longitude);
            circleList.Remove(removeCircle);

            var pin = CreatePin(place);
            pinList.Add(pin);
        }

        private Xamarin.Forms.Maps.Pin CreatePin(Place place)
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
            if(discoveredPlaceViewed)
            {
                await Navigation.PopModalAsync();
            }
            else
            {
                var existingPages = Navigation.NavigationStack.ToList();
                foreach (var page in existingPages)
                {
                    Navigation.RemovePage(page);
                }
                await Navigation.PushModalAsync(new MainPage(circleList, pinList, globalUndiscoveredPlaces, globalDiscoveredPlaces));
            }
        }
    }
}


