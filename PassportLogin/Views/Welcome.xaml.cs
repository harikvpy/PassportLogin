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

using PassportLogin.Utils;
using System.Diagnostics;

using PassportLogin.AuthService;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PassportLogin.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Welcome : Page
    {
        private UserAccount _activeAccount;

        public Welcome()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _activeAccount = (UserAccount)e.Parameter;
            if (_activeAccount != null)
            {
                UserAccount account = AuthService.AuthService.Instance.GetUserAccount(_activeAccount.UserId);
                if (account != null)
                {
                    UserListView.ItemsSource = account.PassportDevices;
                    UserNameText.Text = account.Username;
                }
            }
        }

        private void Button_Restart_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(UserSelection));
        }

        private void Button_Forget_User_Click(object sender, RoutedEventArgs e)
        {
            // Remove it from Microsoft Passport
            MicrosoftPassportHelper.RemovePassportAccountAsync(_activeAccount);

            Debug.WriteLine("User " + _activeAccount.Username + " deleted.");

            // Navigate back to UserSelection page.
            Frame.Navigate(typeof(UserSelection));
        }

        private void Button_Forget_Device_Click(object sender, RoutedEventArgs e)
        {
            PassportDevice selectedDevice = UserListView.SelectedItem as PassportDevice;
            if (selectedDevice != null)
            {
                //Remove it from Windows Hello
                MicrosoftPassportHelper.RemovePassportDevice(_activeAccount, selectedDevice.DeviceId);

                Debug.WriteLine("User " + _activeAccount.Username + " deleted.");

                if (!UserListView.Items.Any())
                {
                    //Navigate back to UserSelection page.
                    Frame.Navigate(typeof(UserSelection));
                }
            }
            else
            {
                ForgetDeviceErrorTextBlock.Visibility = Visibility.Visible;
            }
        }
    }
}
