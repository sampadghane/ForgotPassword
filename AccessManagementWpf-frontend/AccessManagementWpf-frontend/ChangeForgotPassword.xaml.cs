using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AccessManagementWpf_frontend
{
    /// <summary>
    /// Interaction logic for ChangeForgotPassword.xaml
    /// </summary>
    public partial class ChangeForgotPassword : Window
    {
        private readonly string _username;

        public ChangeForgotPassword(string username)
        {
            InitializeComponent();
            _username = username;
            this.WindowStyle = WindowStyle.SingleBorderWindow;
            this.WindowState = WindowState.Maximized;
            this.ResizeMode = ResizeMode.NoResize;
        }

        private void txtNewPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            lblNewPasswordPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void txtNewPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtNewPassword.Password))
            {
                lblNewPasswordPlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void txtConfirmPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            lblConfirmPasswordPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void txtConfirmPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtConfirmPassword.Password))
            {
                lblConfirmPasswordPlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void btnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            string newPassword = txtNewPassword.Password;
            string confirmPassword = txtConfirmPassword.Password;

            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                MessageBox.Show("Both password fields must be filled.");
                return;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Passwords do not match.");
                return;
            }

            if (newPassword.Length < 10 ||
                !newPassword.Any(char.IsUpper) ||
                !newPassword.Any(char.IsLower) ||
                !newPassword.Any(char.IsDigit) ||
                !newPassword.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                MessageBox.Show("Password must be at least 10 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character.");
                return;
            }

            try
            {
                var data = new { _username, newPassword };
                var jsonContent = JsonConvert.SerializeObject(data);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                string url = $"https://localhost:7080/api/User/resetpassword?username={_username}&newPassword={newPassword}";

                HttpResponseMessage response = await HttpClientManager.Client.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Your password has changed successfully.");
                    this.Close();
                }
                else
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Error: {responseBody}");
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Network error: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error: {ex.Message}");
            }
        }

        
    }

}
