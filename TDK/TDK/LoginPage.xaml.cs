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
	public partial class LoginPage : ContentPage
	{
		public LoginPage ()
		{
			InitializeComponent ();
		}

        private async void LoginButton_Clicked(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(emailEntry.Text) || string.IsNullOrEmpty(passwordEntry.Text))
            {
                await DisplayAlert("BŁĄD", "Wypełnij pola Email i Hasło", "OK");
            }
            else
            {
                var user = (await App.MobileService.GetTable<AppUser>().Where(u => u.Email == emailEntry.Text).ToListAsync()).FirstOrDefault();

                if(user != null)
                {
                    if(user.Password == passwordEntry.Text)
                    {
                        await Navigation.PushModalAsync(new LoadingPage());
                    }
                    else
                    {
                        await DisplayAlert("BŁĄD", "Błędny login lub hasło", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("BŁĄD", "Błędny login lub hasło", "OK");
                }
            }   
        }

        private async void RegisterButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegisterPage());
        }
    }
}