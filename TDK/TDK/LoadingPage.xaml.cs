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
	public partial class LoadingPage : ContentPage
	{
		public LoadingPage ()
		{
			InitializeComponent ();
            GetPlaces();
        }

        protected async void GetPlaces()
        {
            var places = await App.MobileService.GetTable<Place>().ToListAsync();
            List<UsersPlaces> discoveredPlacesIds;
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                conn.CreateTable<UsersPlaces>();
                discoveredPlacesIds = conn.Table<UsersPlaces>().ToList();
            }
            SeparateDiscoveredAndNotDiscoveredPlaces(places, discoveredPlacesIds);
        }

        private async void SeparateDiscoveredAndNotDiscoveredPlaces(List<Place> allPlaces, List<UsersPlaces> discoveredPlacesIds)
        {
            List<Place> discovered = new List<Place>();
            List<Place> undiscovered = new List<Place>();

            foreach (var place in allPlaces)
            {
                if (discoveredPlacesIds.Find(p => p.PlaceId == place.Id) != null)
                {
                    discovered.Add(place);
                }
                else
                {
                    undiscovered.Add(place);
                }
            }
            var globalUndiscoveredPlaces = undiscovered;
            DisplayInMap(discovered, undiscovered);
        }

        private void DisplayInMap(List<Place> discovered, List<Place> undiscovered)
        {
            List<CustomCircle> circleList = new List<CustomCircle>();
            foreach (var place in undiscovered)
            {
                try
                {
                    var circle = createCircle(place);
                    circleList.Add(circle);
                }
                catch (Exception ex)
                {
                }
            }

            List<Xamarin.Forms.Maps.Pin> pinList = new List<Xamarin.Forms.Maps.Pin>();
            foreach (var place in discovered)
            {
                
                try
                {
                    var pin = createPin(place);
                    pinList.Add(pin);

                }
                catch (Exception ex)
                {
                }
            }
            Navigation.PushModalAsync(new MainPage(circleList, pinList, undiscovered));
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

        private MapsCustoms.CustomCircle createCircle(Place place)
        {
            var position = new Xamarin.Forms.Maps.Position(place.Latitude, place.Longitude);
            var circle = new MapsCustoms.CustomCircle
            {
                Position = position,
                Radius = 250
            };
            return circle;
        }
    }
}