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
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace AccessManagementWpf_frontend
{
    /// <summary>
    /// Interaction logic for SecurityQuestionWindow.xaml
    /// </summary>
    public partial class SecurityQuestionWindow : Window
    {
        private readonly string _username;
        private List<string> _questions = new List<string>();

        public SecurityQuestionWindow(string username)
        {
            InitializeComponent();
            _username = username;
            this.WindowState = WindowState.Maximized;
            this.WindowStyle = WindowStyle.SingleBorderWindow;
            this.ResizeMode = ResizeMode.NoResize;

            txtUsername.Text = "Hello " + _username + "! Provide security question answers.";
            txtUsername.IsReadOnly = true;
            LoadSecurityQuestions();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        


        private async void LoadSecurityQuestions()
        {
            try
            {
                string apiUrl = $"https://localhost:7080/api/User/get-security-questions?username={_username}";
                HttpResponseMessage response = await HttpClientManager.Client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    _questions = JsonConvert.DeserializeObject<List<string>>(json);
                    GenerateQuestionFields();
                }
                else
                {
                    MessageBox.Show("Failed to load security questions.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }
        private void GenerateQuestionFields()
        {
            QuestionPanel.Children.Clear();
            for (int i = 0; i < _questions.Count; i++)
            {
                TextBlock questionBlock = new TextBlock
                {
                    Text = _questions[i],
                    FontSize = 17,
                    FontWeight = System.Windows.FontWeights.Bold,
                    Margin = new Thickness(0, 10, 0, 5)
                };

                TextBox answerBox = new TextBox
                {
                    Name = $"txtAnswer{i}",
                    FontSize = 17,
                    Height = 50,
                    Width = 350,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 5, 0, 10)
                };

                QuestionPanel.Children.Add(questionBlock);
                QuestionPanel.Children.Add(answerBox);
            }
        }
        private async void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            List<KeyValuePair<string, string>> securityQnA = new List<KeyValuePair<string, string>>();

            // Extract questions and answers dynamically from UI
            foreach (var child in QuestionPanel.Children)
            {
                if (child is StackPanel stack)
                {
                    TextBlock questionText = stack.Children[0] as TextBlock;
                    TextBox answerBox = stack.Children[1] as TextBox;

                    string question = questionText.Text.Trim();
                    string answer = answerBox.Text.Trim();

                    securityQnA.Add(new KeyValuePair<string, string>(question, string.IsNullOrWhiteSpace(answer) ? "-1" : answer));
                }
            }

            string apiUrl = "https://localhost:7080/api/User/validate-answers";

            var requestData = new
            {
                username = _username,
                securityQnA = securityQnA
            };

            try
            {
                using HttpClient client = new HttpClient();
                var jsonContent = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(apiUrl, jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Verification successful. You can now reset your password.");
                    ChangeForgotPassword resetWindow = new ChangeForgotPassword(_username);
                    this.Hide();
                    resetWindow.ShowDialog();
                    this.Close();
                }
                else
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Verification failed: {responseBody}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private void txtUsername_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }

}
