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

using System.Diagnostics;
using PassportLogin.Utils;
using PassportLogin.AuthService;

namespace PassportLogin.Views
{
    public sealed partial class UserSelection : Page
    {
        public UserSelection()
        {
            InitializeComponent();
            Loaded += UserSelection_Loaded;
        }

        private void UserSelection_Loaded(object sender, RoutedEventArgs e)
        {
            List<UserAccount> accounts = AuthService.AuthService.Instance.GetUserAccountsForDevice(Helpers.GetDeviceId());

            if (accounts.Any())
            {
                UserListView.ItemsSource = accounts;
                UserListView.SelectionChanged += UserSelectionChanged;
            }
            else
            {
                //If there are no accounts navigate to the LoginPage
                Frame.Navigate(typeof(Login));
            }
        }

        /// <summary>
        /// Function called when an account is selected in the list of accounts
        /// Navigates to the Login page and passes the chosen account
        /// </summary>
        private void UserSelectionChanged(object sender, RoutedEventArgs e)
        {
            if (((ListView)sender).SelectedValue != null)
            {
                UserAccount account = (UserAccount)((ListView)sender).SelectedValue;
                if (account != null)
                {
                    Debug.WriteLine("Account " + account.Username + " selected!");
                }
                Frame.Navigate(typeof(Login), account);
            }
        }

        /// <summary>
        /// Function called when the "+" button is clicked to add a new user.
        /// Navigates to the Login page with nothing filled out
        /// </summary>
        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PassportRegister));
        }
    }
}