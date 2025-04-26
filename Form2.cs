using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IRCTC_APP
{
    public partial class Form2 : Form
    {
        private string fileUrl = "https://raw.githubusercontent.com/GANESH7826/gadarproinstaller/main/notification.txt.txt";
        private int buttonClickCount = 0;

        public Form2()
        {
            InitializeComponent();

            // Load notifications when the form initializes
            LoadNotificationsFromGitHub();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            buttonClickCount++; // Increment the counter

            if (buttonClickCount == 5) // Show HOME form on 4th click
            {
                this.Hide();
                HOME mform = new HOME();
                mform.Show();

                // Reset counter for future clicks
                buttonClickCount = 0;
            }
            else
            {
                this.Hide();
                this.Show();
                
            }
        }

        private  void Form2_Load(object sender, EventArgs e)
        {
            
          
        }
        private async void LoadNotificationsFromGitHub()
        {
            string fileUrl = "https://raw.githubusercontent.com/GANESH7826/gadarproinstaller/main/notification.txt.txt";

            try
            {
                // Load the file content from GitHub
                string fileContent = await LoadFileContentFromGitHub(fileUrl);

                // Split the content by lines and add them to the ListBox without trimming spaces
                string[] lines = fileContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                // Clear the ListBox to avoid duplicates if reopened
                notificationlistbox.Items.Clear();

                foreach (var line in lines)
                {
                    notificationlistbox.Items.Add(line); // Add each line exactly as it is
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load notifications: {ex.Message}");
            }
        }

        private async Task<string> LoadFileContentFromGitHub(string fileUrl)
        {

            using (HttpClient client = new HttpClient())
            {
                // Add no-cache headers
                client.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue
                {
                    NoCache = true,
                    NoStore = true
                };

                return await client.GetStringAsync(fileUrl);
            }

        }
    }
}
