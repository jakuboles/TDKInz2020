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
	public partial class DiscoveredNewPlace : ContentPage
	{
        private Place place;

        public DiscoveredNewPlace ()
		{
			InitializeComponent();
		}

        public DiscoveredNewPlace(Place place)
        {
            InitializeComponent();
            this.place = place;
            placeName.Text = place.PlaceName;
            placeAddress.Text = place.PlaceAddess;
            PlaceDescription.Text = place.PlaceDescription;
        }

        private async void OkButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MainPage());
        }
    }
}