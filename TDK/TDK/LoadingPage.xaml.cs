using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
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
            StartLoadingPage();
        }

        public async void StartLoadingPage()
        {
            InitializeComponent();
            await GetPermissions();
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
                    var circle = CreateCircle(place);
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
                    var pin = CreatePin(place);
                    pinList.Add(pin);

                }
                catch (Exception ex)
                {
                }
            }
            
            Navigation.PushAsync(new MainPage(circleList, pinList, undiscovered, discovered));

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

        private MapsCustoms.CustomCircle CreateCircle(Place place)
        {
            var position = new Xamarin.Forms.Maps.Position(place.Latitude, place.Longitude);
            var circle = new MapsCustoms.CustomCircle
            {
                Position = position,
                Radius = 250
            };
            return circle;
        }

        private async Task GetPermissions()
        {
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.LocationWhenInUse);
                if (status != PermissionStatus.Granted)
                {
                    if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.LocationWhenInUse))
                    {
                        await DisplayAlert("Potrzebna zgoda", "Do poprawnego działania tej aplikacji potrzebna jest zgoda na dostęp do lokalizacji urządzenia.", "OK");
                    }

                    var result = await CrossPermissions.Current.RequestPermissionsAsync(Permission.LocationWhenInUse);
                    if (result.ContainsKey(Permission.LocationWhenInUse))
                    {
                        status = result[Permission.LocationWhenInUse];
                    }
                }

                if (status == PermissionStatus.Granted)
                {
                    GetPlaces();
                }
                else
                {
                    await DisplayAlert("BŁĄD", "Brak zezwolenia na korzystanie z lokalizacji urządzenia.", "OK");
                    labelLoading.Text = "";
                    labelLoading.Text = "By korzystać z tej aplikacji potrzebna jest zgoda na korzystanie z Lokalizacji. Wyłącz aplikację i udziel jej praw w ustawieniach.";
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("BŁĄD", ex.Message, "OK");
            }
        }
    }
}