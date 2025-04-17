using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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
    /// Interaction logic for ForgotPass.xaml
    /// </summary>
    public partial class ForgotPass : Window
    {
        private string _username;
        private readonly HttpClient _httpClient = new HttpClient();

        public ForgotPass(string username)
        {
            InitializeComponent();
            _username = username;
            this.WindowStyle = WindowStyle.SingleBorderWindow;
            this.WindowState = WindowState.Maximized;
            this.ResizeMode = ResizeMode.NoResize;
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            string enteredUsername = txtUsername.Text.Trim();

            if (string.IsNullOrEmpty(enteredUsername))
            {
                MessageBox.Show("Enter a valid username.");
                return;
            }

            try
            {
                Dashboard ds = new Dashboard(enteredUsername);
                this.Hide();
                ds.ShowDialog();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }
    }

}
