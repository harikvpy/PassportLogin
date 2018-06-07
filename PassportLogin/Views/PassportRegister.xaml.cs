using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

using PassportLogin.Utils;
using PassportLogin.AuthService;

namespace PassportLogin.Views
{
    public sealed partial class PassportRegister : Page
    {
        public PassportRegister()
        {
            InitializeComponent();
        }

        private async void RegisterButton_Click_Async(object sender, RoutedEventArgs e)
        {
            ErrorMessage.Text = "";

            //Validate entered credentials are acceptable
            if (!string.IsNullOrEmpty(UsernameTextBox.Text))
            {
                //Register an Account on the AuthService so that we can get back a userId
                AuthService.AuthService.Instance.Register(UsernameTextBox.Text);
                Guid userId = AuthService.AuthService.Instance.GetUserId(UsernameTextBox.Text);

                if (userId != Guid.Empty)
                {
                    //Now that the account exists on server try and create the necessary passport details and add them to the account
                    bool isSuccessful = await MicrosoftPassportHelper.CreatePassportKeyAsync(userId, UsernameTextBox.Text);
                    if (isSuccessful)
                    {
                        //Navigate to the Welcome Screen. 
                        Frame.Navigate(typeof(Welcome), AuthService.AuthService.Instance.GetUserAccount(userId));
                    }
                    else
                    {
                        //The passport account creation failed.
                        //Remove the account from the server as passport details were not configured
                        AuthService.AuthService.Instance.PassportRemoveUser(userId);

                        ErrorMessage.Text = "Account Creation Failed";
                    }
                }
            }
            else
            {
                ErrorMessage.Text = "Please enter a username";
            }
        }
    }
}
