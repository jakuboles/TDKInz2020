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
	public partial class RegisterPage : ContentPage
	{
		public RegisterPage ()
		{
			InitializeComponent ();
		}

        private async void RegisterUserButton_Clicked(object sender, EventArgs e)
        {
            if(passwordEntry.Text == confirmPasswordEntry.Text)
            {
                AppUser newUser = new AppUser()
                {
                    Email = emailEntry.Text,
                    Password = passwordEntry.Text
                };

                try
                {
                    await App.MobileService.GetTable<AppUser>().InsertAsync(newUser);
                    await DisplayAlert("", "Zarejestrowano pomyślnie", "OK");
                    await Navigation.PushAsync(new LoginPage());
                }
                catch (Exception)
                {
                    await DisplayAlert("BŁĄD", "Wystąpił błąd przy rejestracji", "OK");
                }
            }
            else
            {
                await DisplayAlert("BŁĄD", "Hasła nie są takie same", "OK");
            }
        }
    }
}