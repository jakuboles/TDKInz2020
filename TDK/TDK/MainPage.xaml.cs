using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using TDK.Model;
using SQLite;
using TDK.MapsCustoms;

namespace TDK
{
    public partial class MainPage : ContentPage
    {
        public bool hasLocationPermission = false;
        private List<Place> globalUndiscoveredPlaces = null;

        public MainPage()
        {
            InitializeComponent();
            GetPlaces();
            GetPermissions();
        }

        private async void GetPermissions()
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
                    hasLocationPermission = true; 
                    locationsMap.IsShowingUser = true;
                    GetLocation();
                }
                else
                {
                    await DisplayAlert("BŁĄD", "Brak zezwolenia na korzystanie z lokalizacji urządzenia.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("BŁĄD", ex.Message, "OK");
            }
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

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (hasLocationPermission)
            {
                var locator = CrossGeolocator.Current;
                locator.PositionChanged += Locator_PositionChanged;
                await locator.StartListeningAsync(TimeSpan.Zero, 100);
            }

            GetLocation();
        }

        private void SeparateDiscoveredAndNotDiscoveredPlaces(List<Place> allPlaces, List<UsersPlaces> discoveredPlacesIds)
        {
            List<Place> discovered = null;
            List<Place> undiscovered = null;

            foreach (var place in allPlaces)
            {
                foreach (var id in discoveredPlacesIds)
                {
                    if(place.Id == id.PlaceId)
                    {
                        discovered.Add(place);
                    }
                    else
                    {
                        undiscovered.Add(place);
                    }
                }
            }
            globalUndiscoveredPlaces = undiscovered;
            DisplayInMap(discovered, undiscovered);
        }

        private void DisplayInMap(List<Place> discovered, List<Place> undiscovered)
        {
            
            foreach (var place in discovered)
            {
                try
                {
                    var position = new Xamarin.Forms.Maps.Position(place.Latitude, place.Longitude);

                    var pin = new Xamarin.Forms.Maps.Pin()
                    {
                        Type = Xamarin.Forms.Maps.PinType.Place,
                        Position = position,
                        Label = place.PlaceName,
                        Address = place.PlaceAddess
                    };

                    locationsMap.Pins.Add(pin);
                        
                }
                catch (Exception ex)
                {
                }
            }

            List<CustomCircle> circleList = new List<CustomCircle>();

            foreach (var place in undiscovered)
            {
                try
                {
                    var position = new Xamarin.Forms.Maps.Position(place.Latitude, place.Longitude);
                    var circle = new MapsCustoms.CustomCircle
                    {
                        Position = position,
                        Radius = 200
                    };
                    circleList.Add(circle);
                }
                catch (Exception ex)
                {
                }
            }
            locationsMap.CircleList = circleList;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            CrossGeolocator.Current.StopListeningAsync();
            CrossGeolocator.Current.PositionChanged -= Locator_PositionChanged;
        }

        private void Locator_PositionChanged(object sender, PositionEventArgs e)
        {
            MoveMap(e.Position);
        }

        private async void GetLocation()
        {
            if (hasLocationPermission)
            {
                var locator = CrossGeolocator.Current;
                var position = await locator.GetPositionAsync();
                MoveMap(position);
            }
        }

        private void MoveMap(Position position)
        {
            var center = new Xamarin.Forms.Maps.Position(position.Latitude, position.Longitude);
            var span = new Xamarin.Forms.Maps.MapSpan(center, 0.01, 0.01);
            locationsMap.MoveToRegion(span);
        }
    }
}
