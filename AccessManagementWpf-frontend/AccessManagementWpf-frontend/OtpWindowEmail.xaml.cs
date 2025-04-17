﻿using Newtonsoft.Json;
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
    /// Interaction logic for OtpWindowEmail.xaml
    /// </summary>
    public partial class OtpWindowEmail : Window
    {
        private readonly string _username;
        public OtpWindowEmail(string username)
        {
            _username = username;
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            this.WindowStyle = WindowStyle.SingleBorderWindow;
            this.ResizeMode = ResizeMode.NoResize;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        
        private async void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            string otp = txtOTP.Text.Trim();

            if (string.IsNullOrEmpty(otp))
            {
                MessageBox.Show("Please enter the OTP.");
                return;
            }

            try
            {
                var data = new { _username, otp };
                var jsonContent = JsonConvert.SerializeObject(data);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                string apiUrl = $"https://localhost:7080/api/User/check-email-otp?username={_username}&otp={otp}";

                HttpResponseMessage response = await HttpClientManager.Client.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("OTP verification successful. You can reset your password.");
                    ChangeForgotPassword changePasswordWindow = new ChangeForgotPassword(_username);
                    this.Hide();
                    changePasswordWindow.ShowDialog();
                    this.Close();
                }
                else
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Error: {responseBody}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }
    }
}
