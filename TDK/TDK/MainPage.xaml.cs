using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using TDK.Model;
using SQLite;
using TDK.MapsCustoms;
using System.Threading.Tasks;

namespace TDK
{
    public partial class MainPage : ContentPage
    {
        public bool hasLocationPermission = false;
        private List<Place> globalUndiscoveredPlaces = new List<Place>();
        private List<CustomCircle> globalCircleList = new List<CustomCircle>();
        private List<Xamarin.Forms.Maps.Pin> globalPinList = new List<Xamarin.Forms.Maps.Pin>();

        public MainPage(List<CustomCircle> circleList, List<Xamarin.Forms.Maps.Pin> pinList)
        {
            globalCircleList = circleList;
            globalPinList = pinList;
            StartMainPage(circleList, pinList);
        }

        public MainPage(List<CustomCircle> circleList, List<Xamarin.Forms.Maps.Pin> pinList, List<Place> undiscoveredPlaces)
        {
            globalUndiscoveredPlaces = undiscoveredPlaces;
            globalCircleList = circleList;
            globalPinList = pinList;
            StartMainPage(circleList, pinList);
        }

        private async void StartMainPage(List<CustomCircle> circleList, List<Xamarin.Forms.Maps.Pin> pinList)
        {
            InitializeComponent();
            await AssignPins(circleList, pinList);
            await GetPermissions();
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

        /*protected async Task GetPlaces()
        {
            var places = await App.MobileService.GetTable<Place>().ToListAsync();
            List<UsersPlaces> discoveredPlacesIds;
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                conn.CreateTable<UsersPlaces>();
                discoveredPlacesIds = conn.Table<UsersPlaces>().ToList();
            }
            await SeparateDiscoveredAndNotDiscoveredPlaces(places, discoveredPlacesIds);
        }*/

        /*private async Task SeparateDiscoveredAndNotDiscoveredPlaces(List<Place> allPlaces, List<UsersPlaces> discoveredPlacesIds)
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
            globalUndiscoveredPlaces = undiscovered;
            await DisplayInMap(discovered, undiscovered);
        }*/

        private async Task AssignPins(List<CustomCircle> circleList, List<Xamarin.Forms.Maps.Pin> pinList)
        {
            locationsMap.CircleList = circleList;

            foreach (var pin in pinList)
            {
                locationsMap.Pins.Add(pin);
            }              
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
            CheckIfNewDiscovery(position);
            locationsMap.MoveToRegion(span);
        }

        private void CheckIfNewDiscovery(Position position)
        {
            double dist;
            var discoveredPlace = new Place();
            foreach (var place in globalUndiscoveredPlaces)
            {
                dist = DistanceBetweenPositions(position.Latitude, position.Longitude, place.Latitude, place.Longitude);
                if(dist <= 50)
                {
                    discoveredPlace = place;
                    PlaceDiscovered(place);
                }
            }
            globalUndiscoveredPlaces.Remove(discoveredPlace);
        }

        private void PlaceDiscovered(Place place)
        {
            var newPlace = new UsersPlaces();
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                conn.CreateTable<UsersPlaces>();
                newPlace.PlaceId = place.Id;
                conn.Insert(newPlace);
            }
            Navigation.PushModalAsync(new DiscoveredNewPlace(place, globalCircleList, globalPinList));
        }

        private double DistanceBetweenPositions(double userLat, double userLon, double placeLat, double placeLon)
        {//wzór haversine 
            /*
             * R = promień ziemii
             * Δlat = lat2− lat1
             * Δlong = long2− long1
             * a = sin²(Δlat/2) + cos(lat1).cos(lat2).sin²(Δlong/2)
             * c = 2.atan2(√a, √(1−a))
             * d = R.c - d = dystans między obiektami w metrach
             */
            //Δlat = lat2− lat1
            double deltaLat = (placeLat - userLat);
            deltaLat = deltaLat / 180 * Math.PI;

            //Δlong = long2− long1
            double deltaLon = placeLon - userLon;
            deltaLon = deltaLon / 180 * Math.PI;

            //a = sin²(Δlat/2) + cos(lat1).cos(lat2).sin²(Δlong/2)
            double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(userLat) * Math.Cos(placeLat) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

            //c = 2.atan2(√a, √(1−a))
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            //promień ziemii
            double radiusEquatorial = 6378135;
            double radiusPolar = 6356750;

            double numerator = Math.Pow(radiusEquatorial * radiusPolar * Math.Cos(userLat / 180 * Math.PI), 2);
            double denominator = Math.Pow(radiusEquatorial * Math.Cos(userLat / 180 * Math.PI), 2) +
                Math.Pow(radiusPolar * Math.Sin(userLat / 180 * Math.PI), 2);
            double r = Math.Sqrt(numerator / denominator);

            //dystans w metrach między user'em, a place'm
            return r * c;
        }

        private void StatsButton_Clicked(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new MenuStatystyk());
        }
    }
}
