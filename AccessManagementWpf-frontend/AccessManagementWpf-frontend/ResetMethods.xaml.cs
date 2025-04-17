using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace AccessManagementWpf_frontend
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : Window
    {
        private readonly string _username;
        private List<string> _resetMethods;
        private readonly HttpClient _httpClient = new HttpClient();

        public Dashboard(string username)
        {
            InitializeComponent();
            _username = username;
            LoadResetMethods();
            this.WindowState = WindowState.Maximized;
            this.WindowStyle = WindowStyle.SingleBorderWindow;
            this.ResizeMode = ResizeMode.NoResize;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async Task LoadResetMethods()
        {
            try
            {
                string apiUrl = $"https://localhost:7080/api/User/reset-options?username={_username}";
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ResetOptionsResponse>(json);

                    if (result?.ResetMethods == null || result.ResetMethods.Count == 0)
                    {
                        MessageBox.Show("No reset methods available.");
                        return;
                    }

                    _resetMethods = result.ResetMethods;
                    ResetMethodsListBox.Items.Clear();

                    foreach (var method in _resetMethods)
                    {
                        ResetMethodsListBox.Items.Add(method);
                    }
                }
                else
                {
                    MessageBox.Show($"Failed to load reset options. Status Code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
        private void ResetMethodsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ResetMethodsListBox.SelectedItem != null)
            {
                ProceedButton.IsEnabled = true;
            }
            else
            {
                ProceedButton.IsEnabled = false;
            }
        }

        private async void ProceedButton_Click(object sender, RoutedEventArgs e)
        {
            ProceedButton.IsEnabled = false;
            if (ResetMethodsListBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a reset method.");
                ProceedButton.IsEnabled = true;
                return;
            }

            string selectedMethod = ResetMethodsListBox.SelectedItem.ToString();
            string apiurl = "";
            var data = new { _username };
            var jsonContent = JsonConvert.SerializeObject(data);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                if (selectedMethod == "Security Question")
                {
                    //Z_dynamic ss = new Z_dynamic(txtUsername.Text);
                    //this.Hide();
                    //ss.Show();
                    SecurityQuestionWindow sqWindow = new SecurityQuestionWindow(_username);
                    this.Hide();
                    sqWindow.ShowDialog();
                    this.Close();
                    return;
                }
                else if (selectedMethod == "Email OTP")
                {
                    apiurl = $"https://localhost:7080/api/User/send-email-otp?username={_username}";
                }
                else if (selectedMethod == "Send OTP to Phone Number")
                {
                    apiurl = $"https://localhost:7080/api/User/sendSMS?username={_username}";
                }

                if (!string.IsNullOrEmpty(apiurl))
                {
                    HttpResponseMessage response = await _httpClient.PostAsync(apiurl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show(selectedMethod == "Email OTP"
                            ? "Email has been sent to your email address."
                            : "OTP has been sent to your phone number.");

                        if(selectedMethod=="Email OTP")
                        {
                            OtpWindowEmail otpWindowemail = new OtpWindowEmail(_username);
                            this.Hide();
                            otpWindowemail.ShowDialog();
                        }
                        else if(selectedMethod== "Send OTP to Phone Number")
                        {
                            OtpWindow otpWindow = new OtpWindow(_username);
                            this.Hide();
                            otpWindow.ShowDialog();
                        }
                    }
                    else
                    {
                        string errorMessage = await response.Content.ReadAsStringAsync();
                        ProceedButton.IsEnabled = true;
                        MessageBox.Show($"Failed to send OTP: {errorMessage}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }

            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class ResetOptionsResponse
    {
        public string Message { get; set; }
        public string Username { get; set; }

        [JsonPropertyName("resetoption")]
        public List<string> ResetMethods { get; set; }
    }

}
