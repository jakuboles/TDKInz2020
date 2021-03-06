﻿using System;
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

        public MainPage()
        {
            StartMainPage();
        }

        private async void StartMainPage()
        {
            InitializeComponent();
            await GetPlaces();
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

        protected async Task GetPlaces()
        {
            var places = await App.MobileService.GetTable<Place>().ToListAsync();
            List<UsersPlaces> discoveredPlacesIds;
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                conn.CreateTable<UsersPlaces>();
                discoveredPlacesIds = conn.Table<UsersPlaces>().ToList();
            }
            await SeparateDiscoveredAndNotDiscoveredPlaces(places, discoveredPlacesIds);
        }

        private async Task SeparateDiscoveredAndNotDiscoveredPlaces(List<Place> allPlaces, List<UsersPlaces> discoveredPlacesIds)
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
        }

        private async Task DisplayInMap(List<Place> discovered, List<Place> undiscovered)
        {
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

            foreach (var place in globalUndiscoveredPlaces)
            {
                dist = DistanceBetweenPositions(position.Latitude, position.Longitude, place.Latitude, place.Longitude);
                if(dist <= 10)
                {
                    PlaceDiscovered(place);
                }
            }
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
            Navigation.PushAsync(new DiscoveredNewPlace(place));
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
    }
}
