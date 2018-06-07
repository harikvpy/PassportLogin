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
using Windows.UI.Popups;

using PassportLogin.Utils;
using PassportLogin.AuthService;
using System.Diagnostics;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PassportLogin.Views
{
    public sealed partial class Login : Page
    {
        private UserAccount _account;
        private bool _isExistingAccount;

        public Login()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // Check Microsoft Passport is setup and available on this machine
            if (await MicrosoftPassportHelper.MicrosoftPassportAvailableCheckAsync())
            {
                if (e.Parameter != null)
                {
                    _isExistingAccount = true;
                    // Set the account to the existing account being passed in
                    _account = (UserAccount)e.Parameter;
                    UsernameTextBox.Text = _account.Username;
                    SignInPassport();
                }
            }
            else
            {
                // Microsoft Passport is not setup so inform the user
                PassportStatus.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 50, 170, 207));
                PassportStatusText.Text = "Microsoft Passport is not setup!\n" +
                    "Please go to Windows Settings and set up a PIN to use it.";
                PassportSignInButton.IsEnabled = false;
            }
        }

        private void PassportSignInButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorMessage.Text = "";
            SignInPassport();
        }
        private void RegisterButtonTextBlock_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            ErrorMessage.Text = "";
        }

        private async void SignInPassport()
        {
            if (_isExistingAccount)
            {
                if (await MicrosoftPassportHelper.GetPassportAuthenticationMessageAsync(_account))
                {
                    Frame.Navigate(typeof(Welcome), _account);
                }
            }
            else if (AuthService.AuthService.Instance.ValidateCredentials(UsernameTextBox.Text, PasswordBox.Password))
            {
                Guid userId = AuthService.AuthService.Instance.GetUserId(UsernameTextBox.Text);

                if (userId != Guid.Empty)
                {
                    //Now that the account exists on server try and create the necessary passport details and add them to the account
                    bool isSuccessful = await MicrosoftPassportHelper.CreatePassportKeyAsync(userId, UsernameTextBox.Text);
                    if (isSuccessful)
                    {
                        Debug.WriteLine("Successfully signed in with Windows Hello!");
                        //Navigate to the Welcome Screen. 
                        _account = AuthService.AuthService.Instance.GetUserAccount(userId);
                        Frame.Navigate(typeof(Welcome), _account);
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
                ErrorMessage.Text = "Invalid Credentials";
            }
        }
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            //var dialog = new MessageDialog("Exit button clicked!", "Passport Login Sample");
            //await dialog.ShowAsync();
            Frame.Navigate(typeof(PassportRegister));
        }
    }
}
