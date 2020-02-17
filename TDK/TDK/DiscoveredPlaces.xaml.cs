using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDK.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TDK
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DiscoveredPlaces : ContentPage
	{
        private List<Place> discoveredPlaces = new List<Place>();
        private string totalPlacesNumber;

        public DiscoveredPlaces (List<Place> discoveredPlaces, int totalPlacesNumber)
        {
            this.discoveredPlaces = discoveredPlaces;
            this.totalPlacesNumber = "Odkryte miejsca " + discoveredPlaces.Count.ToString() + "/" + totalPlacesNumber;
            InitializeComponent ();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            labelPlacesCounts.Text = totalPlacesNumber;
            listDiscoveredPlaces.ItemsSource = discoveredPlaces;
        }

        private void ListDiscoveredPlaces_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var place = e.Item as Place;
            Navigation.PushAsync(new DiscoveredNewPlace(place));
        }

        private void LabelGoBack_Clicked(object sender, EventArgs e)
        { 
            Navigation.PopAsync();
        }
    }
}
