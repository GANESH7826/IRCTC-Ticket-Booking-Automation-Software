using System;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading;
using System.Net;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net.Http;
using HtmlAgilityPack;
using System.Text.Json;
using System.Net.Http.Headers;
using Tesseract;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace IRCTC_APP
{
    
    public partial class slotpair : Form
    {
        bool tokenReceived = false;
        bool tokenReceived1 = false;
        bool tokenReceived2 = false;
        bool paybtnClicked = false;
        private string selectedItem;
        private string bankgatewayid;
        private string food = null;
       public string TicketName { get; set; }
        public string ticketunsername;
        public string hisfrom;
        public string histo;
        public string hisjourdate;
        


        private System.Windows.Forms.Timer timer;
        private bool isStopped = false;
        private bool isFilling = false;
        private static readonly HttpClient client = new HttpClient();
        private System.Windows.Forms.Timer updateTimer;
        private bool dragging = false;
        private System.Drawing.Point dragCursorPoint;
        private System.Drawing.Point dragFormPoint;
        private static int pairCount = 0;
        private List<string> userIDs = new List<string>();
        //private ComboBox ;
        private System.Windows.Forms.ComboBox payment1box;
       
        private Stopwatch stopwatch;
        private Thread countdownThread;
        private bool stopCountdown = false; // Flag to stop countdown
        private Thread countdownThread1;
        private bool stopCountdown1 = false;
        private Stopwatch stopwatch1;
        //string ticketName = TicketName;
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(3);
        //private List<ChromeDriver> activeDrivers = new List<ChromeDriver>();
        

        private bool isFirstLoad = true;

        private string classcheck;
        private string quotacheck;
        private string quota;
        private string PTfairticket;
        private string amount ;
        private string availablity;
        private string captchalink= "https://www.irctc.co.in/eticketing/protected/mapps1/loginCaptcha";

        //private readonly HttpClient httpClient;
        private string firstCsrf= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        private string accessToken;
        private string captchaAnswer;
        private string userData;
        private string csrfToken;
       
        // Each form has its own HttpClientHandler, CookieContainer, and HttpClient
        private readonly CookieContainer cookieContainer = new CookieContainer();
        private readonly HttpClientHandler handler;
        private readonly HttpClient httpClient;
        // Console.WriteLine("Current Public IP: " + ip);

        // Static variables to track forms and positions
        private static int formCount = 0; // Static counter to track form instances
        private int formID; // Unique identifier for each form instance
        private static Point lastPosition = new Point(0, 0); // Track the last position for form placement
        private static string lastTicketName = ""; // Store the last TicketName for comparison
        private int startX = 0;
        private int startY = 0;
        private int offsetX = 227; // Horizontal distance between pairs
        private int offsetY = 140; // Vertical distance between rows

        //public string TicketName { get; set; }
        

        private bool isLoopRunning = true;
        private CheckBox ticketCheckbox;
        public slotpair(string ticketName = null)
        {
            InitializeComponent();
            // Assign ticketName to the property
           
            blinkpicturebox.Visible = false;
            qrupipicturebox.Visible = false;
            successpanel.Visible = false;
            finalhitpnrblink.Visible = false;


            label1.Click += new EventHandler(label1_Click);
            paybtn.Click += (sender, e) =>
            {
                paybtnClicked = true; // Set paybtnClicked to true when button is clicked
            };

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string dbPath = Path.Combine(baseDir, "sqllitedb.db");
            connectionString = $"Data Source={dbPath};Version=3;Journal Mode=WAL;";

            if (!File.Exists(dbPath))
            {
                MessageBox.Show("Database file not found: " + dbPath);
            }

            InitializeCountdownTimer();
            InitializeCountdownTimer1();
            
             
            //SetDefaultPaymentOption();
           // this.Load += slotpair_Load;

            this.panel3.MouseDown += new MouseEventHandler(Panel_MouseDown);
            this.panel3.MouseMove += new MouseEventHandler(Panel_MouseMove);
            this.panel3.MouseUp += new MouseEventHandler(Panel_MouseUp);
            this.ticketnamelebel.MouseDown += new MouseEventHandler(Panel_MouseDown);
            this.ticketnamelebel.MouseMove += new MouseEventHandler(Panel_MouseMove);
            this.ticketnamelebel.MouseUp += new MouseEventHandler(Panel_MouseUp);



            pairCount++; // Increment pair count when form is created
            LoadUsernames(); // Load UserIDs when form is created
           
            AutoSelectUsername(); // Auto-select username when form is created

            Load += async (sender, e) => await UpdateTimeAsync();
            InitializeTimer();

            var currentTime = DateTime.Now.TimeOfDay;
            // Proxy configuration
            HttpClientHandler handler;
            if ((currentTime >= new TimeSpan(7, 50, 0) && currentTime <= new TimeSpan(10, 10, 0)) ||
                (currentTime >= new TimeSpan(10, 50, 0) && currentTime <= new TimeSpan(11, 10, 0)))
            {
                handler = new HttpClientHandler
                {
                    UseProxy = true,
                    Proxy = new WebProxy("http://in.smartproxy.com:10000")
                    {
                        Credentials = new NetworkCredential(
                            "spp8trjsx9",
                            "EZha8nv4X=akomS67r")
                    },
                    CookieContainer = cookieContainer
                };

                Console.WriteLine("Using proxy between 9:50 and 10:17.");
            }
            else
            {
                handler = new HttpClientHandler
                {
                    UseProxy = true, // No proxy
                                     //Proxy = WebRequest.DefaultWebProxy, // System ke proxy settings use karega
                                     //  DefaultProxyCredentials = CredentialCache.DefaultCredentials // System default credentials ka use karega
                   
                  
                    CookieContainer = cookieContainer,
                };

                Console.WriteLine("Using direct connection outside 9:50 and 10:17.");
            }

            this.httpClient = new HttpClient(handler); // Form ka dedicated HttpClient

            // Assign a unique ID to the form
            formID = ++formCount;

            // Set the location of this form
            this.StartPosition = FormStartPosition.Manual;
            this.Location = lastPosition;

            // Update the position for the next form instance
            lastPosition.X += offsetX;

            // If the next form goes off the screen horizontally, reset X and move down by offsetY
            if (lastPosition.X + offsetX > Screen.PrimaryScreen.WorkingArea.Width)
            {
                lastPosition.X = startX;
                lastPosition.Y += offsetY;
            }
            if (lastPosition.Y + this.Height > Screen.PrimaryScreen.WorkingArea.Height)
            {
                lastPosition.X = offsetX; // Reset X to starting point
                lastPosition.Y = offsetY; // Reset Y to starting point
            }

        }
       
        private readonly string connectionString = "Data Source=|DataDirectory|\\sqllitedb.db;Version=3;";
       
        private async void slotpair_Load(object sender, EventArgs e)
        {
           
            string ticketName = TicketName;
            FillStationDetails1(ticketName);
           
            LoadPaymentOptions();
        //  availabilitylebel1.Hide();               
            qrpictureBox1.Hide();
           
            richTextBox1.Visible = false;           
            paybtn.Visible = false;
            // Wait until a slot is available
            // await LoadIRCTC();
         
            await FetchAndShowCurrentIPAsync(); // Verify the IP
           // Register the form with the manager
            this.TicketName = TicketName;
            TicketFormManager.RegisterForm(this, TicketName);
           
            await Task.Delay(100);
        }

        // Method to create a new HttpClient with VPN settings
        private async Task FetchAndShowCurrentIPAsync()
        {
            try
            {
                // Har naye form mein ye IP fetch karega
                var response = await this.httpClient.GetAsync("https://api.ipify.org");
                string ip = await response.Content.ReadAsStringAsync();
                if (statusLabel != null && !statusLabel.IsDisposed)
                {
                    statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = $"Current IP =  {ip}"));
                }

                //  MessageBox.Show($"Current IP for this form: {ip}");
                Console.WriteLine($"Current IP for this form: {ip}");
            }
            catch (Exception ex)
            {
                // MessageBox.Show($"Error fetching IP: {ex.Message}");
                Console.WriteLine($"Error fetching IP: {ex.Message}");
            }
        }
        public void LoadPaymentOptions()
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Step 1: Populate the ComboBox
                string query1 = "SELECT nametosave FROM paymentid1";
                using (SQLiteCommand command = new SQLiteCommand(query1, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            comboBox1.Items.Add(reader["nametosave"].ToString());
                        }
                    }
                }

                // Step 2: Fetch `priorbank` and set ComboBox selection
                string query2 = "SELECT priorbank FROM stationdb2 WHERE ticketName = @ticketName";
                using (SQLiteCommand command = new SQLiteCommand(query2, connection))
                {
                    command.Parameters.AddWithValue("@ticketName", TicketName);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string priorbank = reader["priorbank"].ToString();

                            // Set ComboBox selected item to `priorbank` if it exists in items
                            if (comboBox1.Items.Contains(priorbank))
                            {
                                comboBox1.SelectedItem = priorbank;
                            }
                            else
                            {
                                Console.WriteLine("priorbank value not found in ComboBox items.");
                            }
                        }
                    }
                }

                connection.Close();
            }

        }


        public void LoadUsernames()
        {
            // Load UserIDs into usernameComboBox
            string connectionString = "Data Source=sqllitedb.db;Version=3;";
            //string connectionString = Data Source =| DataDirectory |\\sqllitedb.db; Version = 3;
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT UserID FROM irctcid"; // Fetch complete IRCTC ID
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            usernameComboBox.Items.Add(reader["UserID"].ToString()); // Add complete IRCTC ID to combo box
                        }
                    }
                }
                connection.Close();
            }
        }

        private void AutoSelectUsername()
        {
            // Auto-select username based on pair count
            if (pairCount > 0 && pairCount <= usernameComboBox.Items.Count)
            {
                usernameComboBox.SelectedIndex = pairCount - 1;
            }
        }
        private int GetRandomDelay()
        {
            int minDelayMs = 200; // Minimum delay (in milliseconds)
            int maxDelayMs = 700; // Maximum delay (in milliseconds)
            Random random = new Random();
            return random.Next(minDelayMs, maxDelayMs);
        }
        private int GetRandomDelay1()
        {
            int minDelayMs = 3000; // Minimum delay (in milliseconds)
            int maxDelayMs = 20000; // Maximum delay (in milliseconds)
            Random random = new Random();
             int delay = random.Next(minDelayMs, maxDelayMs);
            // MessageBox.Show($"Random Delay Generated: {delay} ms", "Random Delay Test");
            //  return random.Next(minDelayMs, maxDelayMs);
            return delay;

        }
        private async void button1_Click(object sender, EventArgs e)
        {
            if (usernameComboBox.SelectedItem != null)
            {
                loginbtn.Hide();
                webloginbtn.Hide();
                string selectedUsername = usernameComboBox.SelectedItem.ToString();
               
               
                try
                {
                    Invoke(new Action(() =>
                    {
                        availabilitylebel1.Show();
                        availabilitylebel1.BringToFront();
                    }));

                    await Task.Run(async () =>
                    {                      
                       await AuthenticateAndFill(selectedUsername);
                    });
                }
                finally
                {
                    semaphore.Release(); // Release the semaphore after login attempt is complete
                }

               // await Task.Run(async () =>
                //{
                 //   availabilitylebel1.Show();
                  // availabilitylebel1.BringToFront();
               // });

                

            }
            else
            {
                statusLabel.Text = "Please select a username.";
            }

        }
        private async Task AuthenticateAndFill(string username)
        {
            //string connectionString = Data Source=|DataDirectory|\\sqllitedb.db;Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT Password FROM irctcid WHERE UserID = @UserID";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", username);
                    string Password = command.ExecuteScalar()?.ToString();
                    if (!string.IsNullOrEmpty(Password))
                    {
                        usernameComboBox.Invoke((MethodInvoker)(() => usernameComboBox.Enabled = false));
                        ticketunsername = username;

                        // if (statusLabel != null && !statusLabel.IsDisposed)
                        // {
                        //Invoke(new Action(() => statusLabel.Text = "Login Check!"));
                        //}
                        // var irctcClient = new IRCTCClient();
                       // await LoadIRCTC();
                        // Invoke(new Action(() => statusLabel.Text = "IRCTC Loaded Successfully"));

                        DateTime currentTimeIst = DateTime.Now.AddSeconds(2);
                        // int delayInSeconds = 0;
                        connection.Close();
                        if ((quotacheck.ToLower() == "tatkal" || quotacheck.ToLower() == "premium tatkal") && classcheck.ToLower() == "sleeper (sl)")
                        {
                            if (statusLabel != null && !statusLabel.IsDisposed)
                            {
                                statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Wait For VPN Start 57am"));
                            }
                            DateTime targetTime = currentTimeIst.Date.Add(new TimeSpan(10, 57, 00));
                            // Calculate the difference in milliseconds
                            TimeSpan timeToWait = targetTime - currentTimeIst;

                            if (timeToWait.TotalMilliseconds > 0)
                            {
                                // Wait until the target time
                                await Task.Delay(timeToWait);

                                int delay = GetRandomDelay1();
                                await Task.Delay(delay);

                                if (statusLabel != null && !statusLabel.IsDisposed)
                                {
                                    statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Loading Captcha...!"));
                                }

                                await SignIn(username, Password);
                            }
                            else
                            {
                                if (statusLabel != null && !statusLabel.IsDisposed)
                                {
                                    statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Loading Captcha...!"));
                                }

                                await SignIn(username, Password);
                            }
                           
                           


                        }
                        else if ((quotacheck.ToLower() == "tatkal" || quotacheck.ToLower() == "premium tatkal") &&
                                 (classcheck.ToLower() == "ac 3 economy (3e)" || classcheck.ToLower() == "ac 3 tier (3a)" ||
                                  classcheck.ToLower() == "ac 2 tier (2a)" || classcheck.ToLower() == "ac first class (1a)"))
                        {

                            if (statusLabel != null && !statusLabel.IsDisposed)
                            {
                                statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Wait For VPN Start 57am"));
                            }
                            DateTime targetTime = currentTimeIst.Date.Add(new TimeSpan(09, 57, 00));

                            TimeSpan timeToWait = targetTime - currentTimeIst;

                            if (timeToWait.TotalMilliseconds > 0)
                            {
                                // Wait until the target time
                                await Task.Delay(timeToWait);
                                
                                int delay = GetRandomDelay1();
                                await Task.Delay(delay);

                                if (statusLabel != null && !statusLabel.IsDisposed)
                                {
                                    statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Loading Captcha...!"));
                                }

                                await SignIn(username, Password);
                            }
                            else
                            {
                                if (statusLabel != null && !statusLabel.IsDisposed)
                                {
                                    statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Loading Captcha...!"));
                                }

                                await SignIn(username, Password);
                            }
                           

                        }
                        else
                        {
                            await Task.Delay(GetRandomDelay());
                            if (statusLabel != null && !statusLabel.IsDisposed)
                            {
                                statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Loading Captcha...!"));
                            }

                            await SignIn(username, Password);
                        }


                        // statusLabel.Text = "IRCTC Loaded Successfully";
                    }
                    else
                    {
                        Invoke(new Action(() => statusLabel.Text = "Login failed. Invalid credentials."));
                    }
                }
                connection.Close();
            }
        }

        public async Task<string> LoadIRCTC()
        {
           
           
            // First request to load IRCTC homepage
            var headers1 = new HttpRequestMessage(HttpMethod.Get, "https://www.irctc.co.in/nget/train-search");

            // Set headers for the first request
            headers1.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            headers1.Headers.Add("Accept-Language", "en-US,en;q=0.9");
            headers1.Headers.Add("Connection", "keep-alive");
            headers1.Headers.Add("DNT", "1");
            headers1.Headers.Add("Host", "www.irctc.co.in");
            headers1.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36 Edg/123.0.0.0");
            headers1.Headers.Add("sec-ch-ua", "\"Chromium\";v=\"130\", \"Google Chrome\";v=\"130\", \"Not?A_Brand\";v=\"99\"");
            headers1.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
            headers1.Headers.Add("Upgrade-Insecure-Requests", "1");
            headers1.Headers.Add("sec-ch-ua-mobile", "?0");
            headers1.Headers.Add("Sec-Fetch-Site", "same-origin");
            headers1.Headers.Add("Sec-Fetch-Mode", "navigate");
            headers1.Headers.Add("Sec-Fetch-Dest", "empty");
           // headers1.Headers.Add("Accept-Encoding", "gzip, deflate, br, zstd");
           

            // Send the initial request
            HttpResponseMessage response = await httpClient.SendAsync(headers1);
            if (!response.IsSuccessStatusCode)
            {
                return "Failed to load IRCTC.";
            }
            await Task.Delay(GetRandomDelay());
            // Second request to load profile
            var headers2 = new HttpRequestMessage(HttpMethod.Get, $"https://www.irctc.co.in/eticketing/protected/profile/textToNumber/{firstCsrf}");

            // Set headers for the second request
            headers2.Headers.Add("Host", "www.irctc.co.in");
            headers2.Headers.Add("Connection", "keep-alive");
            headers2.Headers.Add("Accept-Language", "en-US,en;q=0.0");
            headers2.Headers.Add("Accept", "application/json, text/plain, */*");
            headers2.Headers.Add("DNT", "1");
            headers2.Headers.Add("Referer", "https://www.irctc.co.in/nget/train-search");
            headers2.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36 Edg/123.0.0.0");
            headers2.Headers.Add("sec-ch-ua", "\"Chromium\";v=\"130\", \"Google Chrome\";v=\"130\", \"Not?A_Brand\";v=\"99\"");
            headers2.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
            headers2.Headers.Add("greq", firstCsrf);
            headers2.Headers.Add("bmirak", "webbm");
            headers2.Headers.Add("bmiyek", "");
            headers2.Headers.Add("sec-ch-ua-mobile", "?0");
          //  headers2.Headers.Add("Content-Language", "en");
           // headers2.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            headers2.Headers.Add("Sec-Fetch-Site", "same-origin");
            headers2.Headers.Add("Sec-Fetch-Mode", "cors");
            headers2.Headers.Add("Sec-Fetch-Dest", "empty");
           // headers2.Headers.Add("Accept-Encoding", "gzip, deflate, br, zstd");


            // Send the profile request
            response = await httpClient.SendAsync(headers2);
            if (response.IsSuccessStatusCode)
            {

                Invoke(new Action(() => statusLabel.Text = "IRCTC Load Successfully...!"));
               // return "IRCTC Loaded Successfully";
            }

            return "Failed to load IRCTC profile";
        }

        public async Task SignIn(string username, string Password)
        {
            captchapanel.Invoke((MethodInvoker)(() => captchapanel.Visible = false));
            await Task.Delay(GetRandomDelay());
            await ClickingSignButton(username, Password);
           
            //await GettingToken();
           // return "Sign In Successful";
        }
       
        public void ShowCaptchaImage(string captchaImageString)
        {
            // Check if we need to invoke the method on the UI thread
            if (InvokeRequired)
            {
                Invoke(new Action<string>(ShowCaptchaImage), captchaImageString);
                return;
            }

            try
            {
                // Step 1: Convert the base64 string to a byte array
                byte[] imageBytes = Convert.FromBase64String(captchaImageString);

                // Step 2: Create a memory stream from the byte array
                using (MemoryStream ms = new MemoryStream(imageBytes))
                {
                    // Step 3: Convert the memory stream to an image
                    //Image captchaImage = Image.FromStream(ms);
                    System.Drawing.Image captchaImage = System.Drawing.Image.FromStream(ms);

                    // Step 4: Display the image in the PictureBox
                    captchaimagepicturebox.Image = captchaImage;

                    // Step 5: Show the panel if it's not visible
                    if (!captchapanel.Visible)
                    {
                        captchapanel.Visible = true; // Show the panel
                    }

                    // Bring the panel to the front
                    captchapanel.BringToFront();
                }
            }
            catch (FormatException ex)
            {
                Console.WriteLine("Invalid base64 string for the image: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while displaying the captcha image: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void solveocrcaptcha(string captchaImageString)
        {
            // Check if we need to invoke the method on the UI thread
            if (InvokeRequired)
            {
                Invoke(new Action<string>(solveocrcaptcha), captchaImageString);
                return;
            }

            try
            {
                // Step 1: Convert the base64 string to a byte array
                byte[] imageBytes = Convert.FromBase64String(captchaImageString);

                // Step 2: Create a memory stream from the byte array
                using (MemoryStream ms = new MemoryStream(imageBytes))
                {
                    // Step 3: Convert the memory stream to an image
                    //Image captchaImage = Image.FromStream(ms);
                    System.Drawing.Image captchaImage = System.Drawing.Image.FromStream(ms);

                    try
                    {
                        // Step 1: Ensure the image is a Bitmap
                        Bitmap captchaBitmap = captchaImage as Bitmap;

                        if (captchaBitmap == null)
                        {
                            // If the image is not a Bitmap, explicitly convert it
                            captchaBitmap = new Bitmap(captchaImage);
                        }

                        // Step 2: Specify the tessdata path
                        string tessdataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");

                        // Step 3: Initialize Tesseract Engine
                        using (var engine = new Tesseract.TesseractEngine(tessdataPath, "cap+eng", Tesseract.EngineMode.LstmOnly))
                        {
                            // Step 3a: Set the whitelist for valid characters (0-9, A-Z, a-z, @, =)
                            engine.SetVariable("tessedit_char_whitelist", "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz@=");
                            engine.SetVariable("tessedit_pageseg_mode", "6"); // Set PSM mode to single block text
                            engine.SetVariable("tessedit_oem", "1"); // Set OCR Engine Mode (LSTM only)
                            // Step 4: Convert the Bitmap to Pix format
                            using (var img = PixConverter.ToPix(captchaBitmap))
                            {
                                // Optional: Preprocess the image for better accuracy (e.g., convert to grayscale)
                                // This step can help with noisy captchas and poor image quality.
                                var processedImg = PreprocessImage(captchaBitmap);
                                // captchaimagepicturebox.Image = processedImg;
                                using (var pix = PixConverter.ToPix(processedImg)) // Use preprocessed image
                                {
                                    // Step 5: Process the Pix image to extract text
                                    using (var page = engine.Process(pix))
                                    {
                                        captchaAnswer = page.GetText().Trim(); // Remove any extra spaces/newlines
                                       // MessageBox.Show($"Extracted Captcha Text: {captchaText}");
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error during Tesseract processing: {ex.Message}");
                    }

                }
            }
            catch (FormatException ex)
            {
                Console.WriteLine("Invalid base64 string for the image: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while displaying the captcha image: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private static Bitmap PreprocessImage(Bitmap original)
        {
            // Convert to grayscale (optional, depending on captcha)
            Bitmap grayBitmap = new Bitmap(original.Width, original.Height);
            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    Color pixelColor = original.GetPixel(x, y);
                    int grayValue = (int)(pixelColor.R * 0.3 + pixelColor.G * 0.59 + pixelColor.B * 0.11);
                    grayBitmap.SetPixel(x, y, Color.FromArgb(grayValue, grayValue, grayValue));
                }
            }

            // Optional: Apply thresholding to create a binary image (helps with noisy captchas)
            Bitmap thresholdedBitmap = ApplyThreshold(grayBitmap, 150); // Adjust threshold as necessary

            return thresholdedBitmap;
        }

        // Thresholding method to convert to black & white (binary) image
        private static Bitmap ApplyThreshold(Bitmap original, int threshold)
        {
            Bitmap thresholdedBitmap = new Bitmap(original.Width, original.Height);
            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    Color pixelColor = original.GetPixel(x, y);
                    int grayValue = (int)(pixelColor.R * 0.3 + pixelColor.G * 0.59 + pixelColor.B * 0.11);
                    Color newColor = (grayValue < threshold) ? Color.Black : Color.White;
                    thresholdedBitmap.SetPixel(x, y, newColor);
                }
            }
            return thresholdedBitmap;
        }

        private async Task ClickingSignButton(string username, string Password)
        {
           
            int attempts = 0;
            int maxRetries = 50;
            

            while (attempts < maxRetries)
            {
                attempts++;
                try
                {
                    if (!isLoopRunning)
                    {
                        Console.WriteLine("Loop has been stopped.");
                        break; // Break the loop if the flag is false
                    }
                    // var headers3 = new HttpRequestMessage(HttpMethod.Get, "https://www.irctc.co.in/eticketing/protected/mapps1/loginCaptcha?nlpCaptchaException=true");
                    var headers3 = new HttpRequestMessage(HttpMethod.Get, captchalink);
                    // Set all required headers in a single statement
                    headers3.Headers.Add("Host", "www.irctc.co.in");
                    headers3.Headers.Add("Connection", "keep-alive");
                    headers3.Headers.Add("greq", firstCsrf);
                    headers3.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
                    headers3.Headers.Add("bmirak", "webbm");
                    headers3.Headers.Add("Accept-Language", "en-US,en;q=0.0");
                    headers3.Headers.Add("sec-ch-ua", "\"Chromium\";v=\"133\", \"Google Chrome\";v=\"133\", \"Not?A_Brand\";v=\"133\"");
                    headers3.Headers.Add("bmiyek", "");
                    headers3.Headers.Add("sec-ch-ua-mobile", "?0");
                    headers3.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/133.0.0.0 Safari/537.36 Edg/133.0.0.0");                 
                    headers3.Headers.Add("Accept", "application/json, text/plain, */*");
                    headers3.Headers.Add("DNT", "1");
                      //headers3.Headers.Add("Content-Language", "en");
                    // headers3.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    headers3.Headers.Add("Accept-Encoding", " deflate, br, zstd");
                    headers3.Headers.Add("Sec-Fetch-Site", "same-origin");
                    headers3.Headers.Add("Sec-Fetch-Mode", "cors");
                    headers3.Headers.Add("Sec-Fetch-Dest", "empty");
                    headers3.Headers.Add("Referer", "https://www.irctc.co.in/nget/train-search");
                   

                    // Add Content-Type header (Note: this might not be standard for GET requests)
                    headers3.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");

                    // Send the request
                    var response = await httpClient.SendAsync(headers3);

                    if (response.IsSuccessStatusCode)
                    {
                        var contentType = response.Content.Headers.ContentType.MediaType;
                        if (contentType == "application/json")
                        {
                            
                            var jsonResponse = await response.Content.ReadAsStringAsync();

                            // Deserialize the JSON response
                            var captchaResponse = JsonConvert.DeserializeObject<CaptchaResponse>(jsonResponse);

                            string captchaImageString = captchaResponse.captchaQuestion;
                           
                            string uidstatus = captchaResponse.status;
                           
                            ShowCaptchaImage(captchaImageString);

                           
                            DateTime currentTime = DateTime.Now;
                            // Define the time ranges
                            TimeSpan range1Start = new TimeSpan(7, 52, 0); // 9:50 AM
                            TimeSpan range1End = new TimeSpan(10, 17, 0); // 10:10 AM
                            TimeSpan range2Start = new TimeSpan(10, 52, 0); // 10:50 AM
                            TimeSpan range2End = new TimeSpan(11, 17, 0); // 11:10 AM

                            // Check if current time is within either range
                            if ((currentTime.TimeOfDay >= range1Start && currentTime.TimeOfDay <= range1End) ||
                                (currentTime.TimeOfDay >= range2Start && currentTime.TimeOfDay <= range2End))
                            {
                                // Call SolveCaptchaAsync
                                captchaAnswer =  SolveCaptchaAsync(captchaImageString);
                               // MessageBox.Show("Captcha solved using SolveCaptchaAsync: " + captchaAnswer);
                            }
                            else
                            {
                                solveocrcaptcha(captchaImageString);
                                // Call solveocrcaptcha
                                // string captchaAnswer = solveocrcaptcha(captchaImageString);
                               // MessageBox.Show("Captcha solved using solveocrcaptcha: " + captchaAnswer);
                            }
                           
                            captchatxtbox.Invoke((MethodInvoker)(() => captchatxtbox.Text = captchaAnswer));


                            if (statusLabel != null && !statusLabel.IsDisposed)
                            {
                                statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Validating Captcha...!"));
                            }
                            await Task.Delay(GetRandomDelay());
                            await SendLogin(uidstatus, username, Password);
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Unexpected response format. Expected JSON but received " + contentType);
                           // MessageBox.Show("Unexpected response format. Expected JSON but received " + contentType);
                        }
                    }
                    else
                    {
                        if (statusLabel != null && !statusLabel.IsDisposed)
                        {
                            statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Failed captcha data: " + response.StatusCode));
                        }

                        //MessageBox.Show("Failed to retrieve captcha data: " + response.StatusCode);
                    }
                }
                catch (Exception ex)
                {

                    Console.WriteLine($"Attempt {attempts}: Error occurred - {ex.Message}");
                    //MessageBox.Show($"Attempt {attempts}: Error occurred - {ex.Message}");
                  
                    
                    if (attempts >= maxRetries)
                    {
                        if (statusLabel != null && !statusLabel.IsDisposed)
                        {
                            statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Max retry attempts exceeded."));
                        }

                        //MessageBox.Show("Max retry attempts exceeded.");
                    }
                    await Task.Delay(1000); // Wait before retrying
                }
            }
        }

        private async Task SendLogin(string uidstatus, string username, string Password)
        {

          
            string base64Password = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Password));

            var headers4 = new HttpRequestMessage(HttpMethod.Post, "https://www.irctc.co.in/authprovider/webtoken");

            // Set the general request headers
            headers4.Headers.Add("Host", "www.irctc.co.in");
            headers4.Headers.Add("Connection", "keep-alive");
            // headers4.Headers.Add("Content-Length", "");
            headers4.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
            headers4.Headers.Add("bmirak", "webbm");
            headers4.Headers.Add("Accept-Language", "en-US,en;q=0.11");
            headers4.Headers.Add("sec-ch-ua", "\"Chromium\";v=\"130\", \"Google Chrome\";v=\"130\", \"Not?A_Brand\";v=\"99\"");
            headers4.Headers.Add("bmiyek", "");
            headers4.Headers.Add("sec-ch-ua-mobile", "?0");
            headers4.Headers.Add("user-agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36");
            headers4.Headers.Add("Accept", "application/json, text/plain, */*");
            headers4.Headers.Add("DNT", "1");
            // headers4.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            headers4.Headers.Add("Origin", "https://www.irctc.co.in");
            headers4.Headers.Add("Sec-Fetch-Site", "same-origin");
            headers4.Headers.Add("Sec-Fetch-Mode", "cors");
            headers4.Headers.Add("Sec-Fetch-Dest", "empty");
            headers4.Headers.Add("referer", "https://www.irctc.co.in/nget/train-search");
           headers4.Headers.Add("Accept-Encoding", "deflate, br, zstd");

            // Prepare the form data as StringContent
            var data = new StringContent($"grant_type=password&username={username}&password={base64Password}&captcha={captchaAnswer}&uid={uidstatus}&otpLogin=false&nlpIdentifier=&nlpAnswer=&nlpToken=&lso=&encodedPwd=true");

            // Set content-specific headers
            data.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

            // Assign the content to the HttpRequestMessage
            headers4.Content = data;
           
            // Send the request
            var response = await httpClient.SendAsync(headers4);
            captchapanel.Invoke((MethodInvoker)(() => captchapanel.Visible = false));
            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                
                statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Validating User...!"));
                // Parse responseData (assuming it is in JSON format)
                var act = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseData);

                while (act.ContainsKey("error"))
                {
                    if (!isLoopRunning)
                    {
                        Console.WriteLine("Loop has been stopped.");
                        break; // Break the loop if the flag is false
                    }



                    if (act["error"].ToString() == "unauthorized" && act["error_description"].ToString() == "Invalid Captcha....")
                    {
                        captchalink = "https://www.irctc.co.in/eticketing/protected/mapps1/loginCaptcha?nlpCaptchaException=true";
                        await SignIn(username, Password);
                    }
                    else if (act["error"].ToString() == "unauthorized" && act["error_description"].ToString() == "Bad credentials")
                    {
                        string errorDescription = act["error_description"].ToString();
                        if (statusLabel != null && !statusLabel.IsDisposed)
                        {
                            statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = errorDescription));
                        }
                        loginbtn.Invoke((MethodInvoker)(() => loginbtn.Visible = true));
                        webloginbtn.Invoke((MethodInvoker)(() => webloginbtn.Visible = true));
                        webloginbtn.Invoke((MethodInvoker)(() => webloginbtn.BringToFront()));
                        break;

                    }
                    else if (act["error"].ToString() == "unauthorized" && act["error_description"].ToString() == "Invalid User")
                    {
                        string errorDescription = act["error_description"].ToString();
                        if (statusLabel != null && !statusLabel.IsDisposed)
                        {
                            statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = errorDescription));
                        }
                        loginbtn.Invoke((MethodInvoker)(() => loginbtn.Visible = true));
                        webloginbtn.Invoke((MethodInvoker)(() => webloginbtn.Visible = true));
                        webloginbtn.Invoke((MethodInvoker)(() => webloginbtn.BringToFront()));
                        break;

                    }
                  
                    else if (act["error"].ToString() == "unauthorized")
                    {
                        string errorDescription = act["error_description"].ToString();
                        if (statusLabel != null && !statusLabel.IsDisposed)
                        {
                            statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = errorDescription));
                        }

                        loginbtn.Invoke((MethodInvoker)(() => loginbtn.Visible = true));
                        webloginbtn.Invoke((MethodInvoker)(() => webloginbtn.Visible = true));
                        webloginbtn.Invoke((MethodInvoker)(() => webloginbtn.BringToFront ()));
                        // loginbtn.Show();
                       // webloginbtn.BringToFront();
                        break;
                        //captchalink = "https://www.irctc.co.in/eticketing/protected/mapps1/loginCaptcha?nlpCaptchaException=true";
                        // await SignIn(username, Password);

                    }
                    break;
                }

                if (act.ContainsKey("access_token"))
                {
                    
                    string accessToken = "Bearer " + act["access_token"].ToString();

                    // Safely update the label on the UI thread

                    await Task.Delay(GetRandomDelay());
                    await ValidateUser(accessToken, uidstatus, username,  Password);
                   
                }
               // else
               // {
                   // if (statusLabel != null && !statusLabel.IsDisposed)
                   // {
                       // statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "error occurred in the access token "));
                   // }
                   // break;
                    //captchalink = "https://www.irctc.co.in/eticketing/protected/mapps1/loginCaptcha?nlpCaptchaException=true";

                    // await SignIn(username, Password);
                    // throw new Exception(" error occurred in the token  process");

                //}

               
            }
        }
        private async Task ValidateUser(string accessToken, string uidstatus, string username, string Password)
        {
           

            var request = new HttpRequestMessage(HttpMethod.Get, "https://www.irctc.co.in/eticketing/protected/mapps1/validateUser?source=3");

            // Add request headers to the HttpRequestMessage
            request.Headers.Add("Host", "www.irctc.co.in");
            request.Headers.Add("Connection", "keep-alive");
            request.Headers.Add("greq", uidstatus); // Use the provided uidstatus   
            request.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
            request.Headers.Add("Authorization", accessToken); // Use the provided access token   
            request.Headers.Add("bmirak", "webbm");
            request.Headers.Add("spa-csrf-token", firstCsrf); // Add CSRF token header
            request.Headers.Add("Accept-Language", "en-US,en;q=0.0");
            request.Headers.Add("sec-ch-ua", "\"Chromium\";v=\"130\", \"Google Chrome\";v=\"130\", \"Not?A_Brand\";v=\"99\"");
            request.Headers.Add("bmiyek", "");
            request.Headers.Add("sec-ch-ua-mobile", "?0");
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36 Edg/123.0.0.0");
            request.Headers.Add("Accept", "application/json, text/plain, */*");
            request.Headers.Add("DNT", "1");
            // request.Headers.Add("Content-Language", "en");
            // request.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            request.Headers.Add("Sec-Fetch-Site", "same-origin");
            request.Headers.Add("Sec-Fetch-Mode", "cors");
            request.Headers.Add("Sec-Fetch-Dest", "empty");
            request.Headers.Add("Referer", "https://www.irctc.co.in/nget/train-search");
            //request.Headers.Add("Accept-Encoding", "gzip, deflate, br, zstd");


            var response = await httpClient.SendAsync(request);

            // Check if the response is successful
            if (response.IsSuccessStatusCode)
            {
                // Read user data from the response
                string userDataJson = await response.Content.ReadAsStringAsync();

                // Deserialize the user data into an object
                UserData userData = JsonConvert.DeserializeObject<UserData>(userDataJson);

                // Extract user hash from the user data
                string userHash = userData.userIdHash; // Assuming userIdHash is a property in your JSON
                
                 csrfToken = response.Headers.GetValues("csrf-token").FirstOrDefault();

                string ticketName = TicketName;
                if (statusLabel != null && !statusLabel.IsDisposed)
                {
                    statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Login Successfully"));
                }

                await Task.Delay(GetRandomDelay());
                await GetTrainsAsync( userHash,uidstatus, accessToken, ticketName, username,  Password);

            }
            else
            {
                // Handle unsuccessful response
               // MessageBox.Show($"Failed to validate user: {response.ReasonPhrase}");
                statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = $"Failed validate user: {response.ReasonPhrase}"));
                await SignIn(username, Password);

            }


        }
        public async Task GetTrainsAsync( string userHash, string uidstatus, string accessToken, string ticketName,string username, string Password)
        {

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {

                connection.Open();
                string query = "SELECT fromstation, tostation,dateofjourney,quota,trainno,class FROM stationdb2 WHERE ticketName = @ticketName";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ticketName", ticketName);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string fromStation = reader["fromstation"].ToString();
                            string toStation = reader["tostation"].ToString();
                            string formattedDate = reader["dateofjourney"].ToString();
                            string quota = reader["quota"].ToString();
                            string trainno = reader["trainno"].ToString();
                            string GetTrainClass = reader["class"].ToString();

                           


                            if (DateTime.TryParse(formattedDate, out DateTime parsedDate))
                            {
                                // Format the parsed date to dd-MMM-yyyy
                                formattedDate = parsedDate.ToString("yyyyMMdd"); // Example: 16-Nov-2024
                            }
                           
                            Dictionary<string, string> quotaMap = new Dictionary<string, string>
                        {
                            { "General", "GN" },
                            { "Tatkal", "TQ" },
                            { "Premium Tatkal", "PT" },
                            { "Ladies", "LD" }
                        };
                            string quotaMapvalue = quotaMap.ContainsKey(quota) ? quotaMap[quota] : string.Empty;
                            // MessageBox.Show(quotaMapvalue);
                            int maxRetryAttempts = 50;
                            int retryCount = 0;
                           // bool tokenReceived = false;
                            //string newCsrfToken = null;

                            while (retryCount < maxRetryAttempts && !tokenReceived)
                            {
                                if (!isLoopRunning)
                                {
                                    Console.WriteLine("Loop has been stopped.");
                                    break; // Break the loop if the flag is false
                                }


                                var postData = new
                                {
                                    concessionBooking = false,
                                    srcStn = fromStation,
                                    destStn = toStation,
                                    jrnyClass = "",
                                    jrnyDate = formattedDate,
                                    quotaCode = quotaMapvalue,
                                    currentBooking = "false",
                                    flexiFlag = false,
                                    handicapFlag = false,
                                    ticketType = "E",
                                    loyaltyRedemptionBooking = false,
                                    ftBooking = false
                                };

                                string postDataString = JsonConvert.SerializeObject(postData);
                                var content = new StringContent(postDataString, Encoding.UTF8, "application/json");

                                using (HttpClient client = new HttpClient())
                                using (var request = new HttpRequestMessage(HttpMethod.Post, "https://www.irctc.co.in/eticketing/protected/mapps1/altAvlEnq/TC"))
                                {
                                    // Assign content to the request
                                    request.Content = content;

                                    // Essential headers
                                    request.Headers.Add("Host", "www.irctc.co.in");
                                    request.Headers.Add("Connection", "keep-alive");
                                    //request.Headers.Add("Content-Length", "");
                                    request.Headers.Add("greq", uidstatus);
                                    request.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
                                    request.Headers.Add("Authorization", accessToken);
                                    request.Headers.Add("bmirak", "webbm");
                                    request.Headers.Add("spa-csrf-token", csrfToken);
                                    request.Headers.Add("Accept-Language", "en-US,en;q=0.0");
                                    request.Headers.Add("sec-ch-ua", "\"Chromium\";v=\"130\", \"Google Chrome\";v=\"130\", \"Not?A_Brand\";v=\"99\"");
                                    request.Headers.Add("bmiyek", userHash);
                                    request.Headers.Add("sec-ch-ua-mobile", "?0");
                                    request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36 Edg/123.0.0.0");
                                    request.Headers.Add("Accept", "application/json, text/plain, */*");
                                    request.Headers.Add("DNT", "1");
                                    // request.Headers.Add("Content-Language", "en");
                                    //request.Headers.Add("Content-Type", "application/json; charset=UTF-8");
                                    request.Headers.Add("Origin", "https://www.irctc.co.in");
                                    request.Headers.Add("Sec-Fetch-Site", "same-origin");
                                    request.Headers.Add("Sec-Fetch-Mode", "cors");
                                    request.Headers.Add("Sec-Fetch-Dest", "empty");
                                    request.Headers.Add("Referer", "https://www.irctc.co.in/nget/train-search");
                                    // request.Headers.Add("Accept-Encoding", "gzip, deflate, br, zstd");

                                    if (statusLabel != null && !statusLabel.IsDisposed)
                                    {
                                        statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Loading Train List...!"));
                                    }
                                    try
                                    {
                                        // Making the POST request
                                        // HttpResponseMessage response = await client.SendAsync(request);
                                        var response = await httpClient.SendAsync(request);
                                        // If token was received, proceed with parsing the response data

                                        string responseData = await response.Content.ReadAsStringAsync();
                                        JObject data = JObject.Parse(responseData);

                                        if (data.ContainsKey("errorMessage"))
                                        {
                                            statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = data["errorMessage"].ToString()));

                                            await SignIn(username, Password);

                                        }
                                        else if (data.ContainsKey("error_description"))
                                        {
                                            statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = data["error_description"].ToString()));

                                            await SignIn(username, Password);

                                        }

                                        else
                                        {
                                            // Check if the csrf-token header is present
                                            if (response.Headers.Contains("csrf-token"))
                                            {
                                                csrfToken = response.Headers.GetValues("csrf-token").FirstOrDefault();
                                                tokenReceived = true;
                                            }
                                            else
                                            {
                                                // Increment the retry count if token is not found
                                                retryCount++;
                                                if (retryCount < maxRetryAttempts)
                                                {
                                                    await Task.Delay(100); // Wait before retrying
                                                    continue;
                                                }
                                                else
                                                {
                                                    await SignIn(username, Password);
                                                    // throw new Exception("Failed to retrieve csrf-token after multiple attempts.");
                                                }
                                            }

                                            if (statusLabel != null && !statusLabel.IsDisposed)
                                            {
                                                statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Get Train List...!"));
                                            }
                                            await Task.Delay(GetRandomDelay());
                                            connection.Close();
                                            await FetchClassAvailabilityAsync( userHash, uidstatus, accessToken, ticketName, username, Password);
                                               
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        if (retryCount >= maxRetryAttempts)
                                        {
                                            Console.WriteLine($"Error gettrain after {maxRetryAttempts} attempts: {ex.Message}");
                                            if (statusLabel != null && !statusLabel.IsDisposed)
                                            {
                                                statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = $"Errortd: {ex.Message}"));
                                            }
                                             await SignIn(username, Password);
                                           // continue;
                                        }
                                      
                                        retryCount++;
                                        continue;
                                    }

                                }
                            }
                           


                        }
                        else
                        {
                            //Invoke(new Action(() => statusLabel.Text = "No station details found!"));
                        }
                    }
                }
                connection.Close();
            }

        }
        private int GetRandomDelayavai()
        {
            int minDelayMs = 100; // Minimum delay (in milliseconds)
            int maxDelayMs = 1500; // Maximum delay (in milliseconds)
            Random random = new Random();
            return random.Next(minDelayMs, maxDelayMs);
        }
        public async Task FetchClassAvailabilityAsync( string userHash, string uidstatus, string accessToken, string ticketName, string username, string Password)
        {
            
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {

                connection.Open();
                string query = "SELECT fromstation, tostation,dateofjourney,quota,trainno,class,ptfair FROM stationdb2 WHERE ticketName = @ticketName";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ticketName", ticketName);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string fromStation = reader["fromstation"].ToString();
                            string toStation = reader["tostation"].ToString();
                            string formattedDate = reader["dateofjourney"].ToString();
                            quota = reader["quota"].ToString();
                            string trainno = reader["trainno"].ToString();
                            string GetTrainClass = reader["class"].ToString();
                            PTfairticket = reader["ptfair"].ToString();

                            if (DateTime.TryParse(formattedDate, out DateTime parsedDate))
                            {
                                // Format the parsed date to dd-MMM-yyyy
                                formattedDate = parsedDate.ToString("yyyyMMdd"); // Example: 16-Nov-2024
                            }
                           
                           

                            Dictionary<string, string> quotaMap = new Dictionary<string, string>
                        {
                            { "General", "GN" },
                            { "Tatkal", "TQ" },
                            { "Premium Tatkal", "PT" },
                            { "Ladies", "LD" }
                        };
                            string quotaMapvalue = quotaMap.ContainsKey(quota) ? quotaMap[quota] : string.Empty;
                            // MessageBox.Show(quotaMapvalue);
                            Dictionary<string, string> classMap = new Dictionary<string, string>
                        {
                            { "Second Sitting (2S)", "2S" },
                            { "Sleeper (SL)", "SL" },
                            { "AC 3 Economy (3E)", "3E" },
                            { "AC 3 Tier (3A)", "3A" },
                            {"AC 2 Tier (2A)", "2A" },
                            {"AC First Class (1A)", "1A" }
                        };
                            string classMapvalue = classMap.ContainsKey(GetTrainClass) ? classMap[GetTrainClass] : string.Empty;

                           
                            DateTime currentTimeIst = DateTime.Now.AddSeconds(10);                           
                          //  int delayInSeconds = 0;

                            if ((quota.ToLower() == "tatkal" || quota.ToLower() == "premium tatkal") && GetTrainClass.ToLower() == "sleeper (sl)")
                            {
                                if (statusLabel != null && !statusLabel.IsDisposed)
                                {
                                    statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Wait For PG Open...!"));
                                }
                                DateTime targetTime = currentTimeIst.Date.Add(new TimeSpan(10, 59, 40));

                                TimeSpan timeToWait = targetTime - currentTimeIst;

                                if (timeToWait.TotalMilliseconds > 0)
                                {
                                    // Wait until the target time
                                    await Task.Delay(timeToWait);
                                }

                            }
                            else if ((quota.ToLower() == "tatkal" || quota.ToLower() == "premium tatkal") &&
                                     (GetTrainClass.ToLower() == "ac 3 economy (3e)" || GetTrainClass.ToLower() == "ac 3 tier (3a)" ||
                                      GetTrainClass.ToLower() == "ac 2 tier (2a)" || GetTrainClass.ToLower() == "ac first class (1a)"))
                            {

                                if (statusLabel != null && !statusLabel.IsDisposed)
                                {
                                    statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Wait For PG Open...!"));
                                }
                                DateTime targetTime = currentTimeIst.Date.Add(new TimeSpan(09, 59, 40));

                                TimeSpan timeToWait = targetTime - currentTimeIst;

                                if (timeToWait.TotalMilliseconds > 0)
                                {
                                    // Wait until the target time
                                    await Task.Delay(timeToWait);
                                }

                            }
                            else
                            {
                                if (statusLabel != null && !statusLabel.IsDisposed)
                                {
                                    statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Wait For PG Open...!"));
                                }
                                DateTime targetTime = currentTimeIst.Date.Add(new TimeSpan(08, 00, 00));

                                TimeSpan timeToWait = targetTime - currentTimeIst;

                                if (timeToWait.TotalMilliseconds > 0)
                                {
                                    // Wait until the target time
                                    await Task.Delay(timeToWait);
                                }
                            }
                          
                            DateTime currentTimeIst1 = DateTime.Now.AddSeconds(10);

                            if ((quota.ToLower() == "tatkal" || quota.ToLower() == "premium tatkal") && GetTrainClass.ToLower() == "sleeper (sl)")
                            {
                                if (statusLabel != null && !statusLabel.IsDisposed)
                                {
                                    statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Wait For Open Availability...!"));
                                }
                                DateTime targetTime = currentTimeIst1.Date.Add(new TimeSpan(11, 00, 00));
                                TimeSpan timeToWait = targetTime - currentTimeIst1;

                                if (timeToWait.TotalMilliseconds > 0)
                                {
                                    // Wait until the target time
                                    await Task.Delay(timeToWait);
                                }


                            }
                            else if ((quota.ToLower() == "tatkal" || quota.ToLower() == "premium tatkal") &&
                                     (GetTrainClass.ToLower() == "ac 3 economy (3e)" || GetTrainClass.ToLower() == "ac 3 tier (3a)" ||
                                      GetTrainClass.ToLower() == "ac 2 tier (2a)" || GetTrainClass.ToLower() == "ac first class (1a)"))
                            {

                                if (statusLabel != null && !statusLabel.IsDisposed)
                                {
                                    statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Wait For Open Availability...!"));
                                }
                                DateTime targetTime = currentTimeIst1.Date.Add(new TimeSpan(10, 00, 00));

                                TimeSpan timeToWait = targetTime - currentTimeIst1;

                                if (timeToWait.TotalMilliseconds > 0)
                                {
                                    // Wait until the target time
                                    await Task.Delay(timeToWait);
                                }

                            }
                            else
                            {
                                // No delay needed for General quota or other cases
                                // delayInSeconds = 0;
                            }

                            
                            await Task.Delay(GetRandomDelayavai());
                            if (statusLabel != null && !statusLabel.IsDisposed)
                            {
                                statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Getting Availability...!"));
                            }


                            int maxRetries = 50;
                            int retryCount = 0;
                          //  bool tokenReceived1 = false;

                            using (HttpClient client = new HttpClient())
                            {
                                while (retryCount < maxRetries)
                                {
                                    if (!isLoopRunning)
                                    {
                                        Console.WriteLine("Loop has been stopped.");
                                        break; // Break the loop if the flag is false
                                    }



                                    var postData = new
                                    {
                                        paymentFlag = "N",
                                        concessionBooking = false,
                                        ftBooking = false,
                                        loyaltyRedemptionBooking = false,
                                        ticketType = "E",
                                        quotaCode = quotaMapvalue,
                                        moreThanOneDay = false,
                                        trainNumber = trainno,
                                        fromStnCode = fromStation,
                                        toStnCode = toStation,
                                        isLogedinReq = true,
                                        journeyDate = formattedDate,
                                        classCode = classMapvalue
                                    };

                                    string postDataString = JsonConvert.SerializeObject(postData);
                                    var content = new StringContent(postDataString, Encoding.UTF8, "application/json");


                                    using (var request = new HttpRequestMessage(HttpMethod.Post, $"https://www.irctc.co.in/eticketing/protected/mapps1/avlFarenquiry/{trainno}/{formattedDate}/{fromStation}/{toStation}/{classMapvalue}/{quotaMapvalue}/N"))
                                    {
                                        // Assigning the JSON payload to request
                                        request.Content = content;

                                        // Adding headers

                                        request.Headers.Add("Host", "www.irctc.co.in");
                                        request.Headers.Add("Connection", "keep-alive");
                                        // request.Headers.Add("Content-Length", "");
                                        request.Headers.Add("greq", uidstatus);
                                        request.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
                                        request.Headers.Add("Authorization", accessToken);
                                        request.Headers.Add("bmirak", "webbm");
                                        request.Headers.Add("spa-csrf-token", csrfToken);
                                        request.Headers.Add("Accept-Language", "en-US,en;q=0.0");
                                        request.Headers.Add("sec-ch-ua", "\"Chromium\";v=\"130\", \"Google Chrome\";v=\"130\", \"Not?A_Brand\";v=\"99\"");
                                        request.Headers.Add("bmiyek", userHash);
                                        request.Headers.Add("sec-ch-ua-mobile", "?0");
                                        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36 Edg/123.0.0.0");
                                        request.Headers.Add("Accept", "application/json, text/plain, */*");
                                        request.Headers.Add("DNT", "1");
                                        //request.Headers.Add("Content-Language", "en");
                                       // request.Headers.Add("Content-Type", "application/json; charset=UTF-8");
                                        request.Headers.Add("Origin", "https://www.irctc.co.in");
                                        request.Headers.Add("Sec-Fetch-Site", "same-origin");
                                        request.Headers.Add("Sec-Fetch-Mode", "cors");
                                        request.Headers.Add("Sec-Fetch-Dest", "empty");
                                       // request.Headers.Add("Accept-Encoding", "gzip, deflate, br, zstd");
                                        request.Headers.Add("Referer", "https://www.irctc.co.in/nget/booking/train-list");

                                        try
                                        {
                                            // Send the request and get response
                                          //  HttpResponseMessage response = await client.SendAsync(request);
                                            var response = await httpClient.SendAsync(request);
                                            if (response.StatusCode == System.Net.HttpStatusCode.BadGateway)
                                            {
                                                retryCount++;
                                                //MessageBox.Show($"Received 502 Bad Gateway, retrying attempt {retryCount}...");
                                               
                                               // if (statusLabel != null && !statusLabel.IsDisposed)
                                               // {
                                                  //  statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = $"502 Bad Gateway,retrying attempt {retryCount}..."));
                                               // }
                                                continue;
                                            }
                                            if (response.Headers.Contains("csrf-token"))
                                            {
                                                csrfToken = response.Headers.GetValues("csrf-token").FirstOrDefault();
                                                tokenReceived1 = true;
                                            }
                                            else
                                            {
                                                // Increment the retry count if token is not found
                                                retryCount++;
                                                if (retryCount < maxRetries)
                                                {
                                                    await Task.Delay(100); // Wait before retrying
                                                    continue;
                                                }
                                                else
                                                {
                                                    await SignIn(username, Password);
                                                    // throw new Exception("Failed to retrieve csrf-token after multiple attempts.");
                                                }
                                            }
                                           // csrfToken = response.Headers.GetValues("csrf-token").FirstOrDefault();
                                            string responseData = await response.Content.ReadAsStringAsync();
                                            JObject data = JObject.Parse(responseData);

                                            if (data.ContainsKey("errorMessage"))
                                            {
                                                // throw new Exception(data["errorMessage"].ToString());
                                                if (statusLabel != null && !statusLabel.IsDisposed)
                                                {
                                                    statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = data["errorMessage"].ToString()));
                                                }
                                                retryCount++;
                                                if (retryCount >= maxRetries)
                                                {
                                                    await SignIn(username, Password);
                                                }
                                                // Optionally add delay before retry
                                               // await Task.Delay(500);
                                                continue;
                                            }
                                            else
                                            {
                                                //MessageBox.Show(responseData);
                                               
                                                if (statusLabel != null && !statusLabel.IsDisposed)
                                                {
                                                    statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Get Availability...!"));
                                                }
                                                await Task.Delay(GetRandomDelay());
                                                connection.Close();
                                                await GetBoardingStationsAsync( userHash, uidstatus, accessToken, ticketName, username,  Password);
                                              
                                                break;
                                            }
                                        }
                                        catch (TaskCanceledException ex) when (ex.CancellationToken == CancellationToken.None)
                                        {
                                            retryCount++;
                                            if (retryCount >= maxRetries)
                                            {
                                               // if (statusLabel != null && !statusLabel.IsDisposed)
                                               // {
                                                   // statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = $"Error ava: {ex.CancellationToken}"));
                                              // }
                                                await SignIn(username, Password);
                                            }
                                            // Optionally add delay before retry
                                            await Task.Delay(500);
                                            continue;
                                        }
                                        catch (Exception ex)
                                        {
                                           Console.WriteLine($"Error ava: {ex.Message}");
                                            //MessageBox.Show($"Error ava: {ex.Message}");
                                          
                                            retryCount++;
                                            if (retryCount >= maxRetries)
                                            {
                                               // if (statusLabel != null && !statusLabel.IsDisposed)
                                               // {
                                                  //  statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = $"Error ava1: {ex.Message}"));
                                              //  }
                                                 await SignIn(username, Password);
                                            }
                                            // Optionally add delay before retry
                                            await Task.Delay(500);
                                            continue;
                                        }
                                    }
                                }

                               
                            }


                        }
                        else
                        {
                            //Invoke(new Action(() => statusLabel.Text = "No station details found!"));
                        }
                    }
                }
                connection.Close();
            }


        }
        private int GetRandomDelayboarding()
        {
            int minDelayMs = 100; // Minimum delay (in milliseconds)
            int maxDelayMs = 1000; // Maximum delay (in milliseconds)
            Random random = new Random();
            return random.Next(minDelayMs, maxDelayMs);
        }
        public async Task GetBoardingStationsAsync( string userHash, string uidstatus, string accessToken, string ticketName, string username, string Password)
        {

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {

                connection.Open();
                string query = "SELECT fromstation, tostation,dateofjourney,quota,trainno,class FROM stationdb2 WHERE ticketName = @ticketName";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ticketName", ticketName);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string fromStation = reader["fromstation"].ToString();
                            string toStation = reader["tostation"].ToString();
                            string formattedDate = reader["dateofjourney"].ToString();
                            string quota = reader["quota"].ToString();
                            string trainno = reader["trainno"].ToString();
                            string GetTrainClass = reader["class"].ToString();
                           
                            
                            if (DateTime.TryParse(formattedDate, out DateTime parsedDate))
                            {
                                // Format the parsed date to dd-MMM-yyyy
                                formattedDate = parsedDate.ToString("yyyyMMdd"); // Example: 16-Nov-2024
                            }
                           


                            Dictionary<string, string> quotaMap = new Dictionary<string, string>
                        {
                            { "General", "GN" },
                            { "Tatkal", "TQ" },
                            { "Premium Tatkal", "PT" },
                            { "Ladies", "LD" }
                        };
                            string quotaMapvalue = quotaMap.ContainsKey(quota) ? quotaMap[quota] : string.Empty;
                            // MessageBox.Show(quotaMapvalue);
                            Dictionary<string, string> classMap = new Dictionary<string, string>
                        {
                            { "Second Sitting (2S)", "2S" },
                            { "Sleeper (SL)", "SL" },
                            { "AC 3 Economy (3E)", "3E" },
                            { "AC 3 Tier (3A)", "3A" },
                            {"AC 2 Tier (2A)", "2A" },
                            {"AC First Class (1A)", "1A" }
                        };
                            string classMapvalue = classMap.ContainsKey(GetTrainClass) ? classMap[GetTrainClass] : string.Empty;

                            DateTime currentTimeIst = DateTime.Now.AddSeconds(10);
                            // int delayInSeconds = 0;
                           
                            if ((quota.ToLower() == "tatkal" || quota.ToLower() == "premium tatkal") && GetTrainClass.ToLower() == "sleeper (sl)")
                            {
                                if (statusLabel != null && !statusLabel.IsDisposed)
                                {
                                    statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Get Availability1...!"));
                                }
                                DateTime targetTime = currentTimeIst.Date.Add(new TimeSpan(11, 00, 00));
                                TimeSpan timeToWait = targetTime - currentTimeIst;

                                if (timeToWait.TotalMilliseconds > 0)
                                {
                                    // Wait until the target time
                                    await Task.Delay(timeToWait);
                                }


                            }
                            else if ((quota.ToLower() == "tatkal" || quota.ToLower() == "premium tatkal") &&
                                     (GetTrainClass.ToLower() == "ac 3 economy (3e)" || GetTrainClass.ToLower() == "ac 3 tier (3a)" ||
                                      GetTrainClass.ToLower() == "ac 2 tier (2a)" || GetTrainClass.ToLower() == "ac first class (1a)"))
                            {

                                if (statusLabel != null && !statusLabel.IsDisposed)
                                {
                                    statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Get Availability1...!"));
                                }
                                DateTime targetTime = currentTimeIst.Date.Add(new TimeSpan(10, 00, 00));

                                TimeSpan timeToWait = targetTime - currentTimeIst;

                                if (timeToWait.TotalMilliseconds > 0)
                                {
                                    // Wait until the target time
                                    await Task.Delay(timeToWait);
                                }

                            }
                            else
                            {
                                // No delay needed for General quota or other cases
                               // delayInSeconds = 0;
                            }
                            
                            if (statusLabel != null && !statusLabel.IsDisposed)
                            {
                                statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Get Availability2...!"));
                            }
                            await Task.Delay(GetRandomDelayboarding());
                           // await Task.Delay(8000); // Wait before retrying            


                            int maxRetries = 50;
                            int retryCount = 0;
                            using (HttpClient client = new HttpClient())
                            {
                                while (retryCount < maxRetries)
                                {
                                    if (!isLoopRunning)
                                    {
                                        Console.WriteLine("Loop has been stopped.");
                                        break; // Break the loop if the flag is false
                                    }


                                    var postData = new
                                    {
                                        clusterFlag = "N",
                                        onwardFlag = "N",
                                        cod = "false",
                                        reservationMode = "WS_TA_B2C",
                                        autoUpgradationSelected = false,
                                        gnToCkOpted = false,
                                        paymentType = 1,
                                        twoPhaseAuthRequired = false,
                                        captureAddress = 0,
                                        alternateAvlInputDTO = new[]
                {
            new
            {
                trainNo = trainno,
                destStn = toStation,
                srcStn = fromStation,
                jrnyDate = formattedDate,
                quotaCode = quotaMapvalue,
                jrnyClass = classMapvalue,
                concessionPassengers = false
            }
        },
                                        passBooking = false,
                                        journalistBooking = false
                                    };

                                    string postDataString = JsonConvert.SerializeObject(postData);
                                    var content = new StringContent(postDataString, Encoding.UTF8, "application/json");
                                    if (statusLabel != null && !statusLabel.IsDisposed)
                                    {
                                        statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Wait For ## Remove...!"));
                                    }

                                    using (var request = new HttpRequestMessage(HttpMethod.Post, "https://www.irctc.co.in/eticketing/protected/mapps1/boardingStationEnq"))
                                    {
                                       // var requestContent = new StringContent(postDataString, Encoding.UTF8, "application/json");
                                        //request.Content = requestContent;
                                        request.Content = content;
                                        request.Headers.Add("Host", "www.irctc.co.in");
                                        request.Headers.Add("Connection", "keep-alive");
                                        // request.Headers.Add("Content-Length", "");
                                        request.Headers.Add("greq", uidstatus);
                                        request.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
                                        request.Headers.Add("Authorization", accessToken);
                                        request.Headers.Add("bmirak", "webbm");
                                        request.Headers.Add("spa-csrf-token", csrfToken);
                                        request.Headers.Add("Accept-Language", "en-US,en;q=0.0");
                                        request.Headers.Add("sec-ch-ua", "\"Chromium\";v=\"130\", \"Google Chrome\";v=\"130\", \"Not?A_Brand\";v=\"99\"");
                                        request.Headers.Add("bmiyek", userHash);
                                        request.Headers.Add("sec-ch-ua-mobile", "?0");
                                        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36 Edg/123.0.0.0");
                                        request.Headers.Add("Accept", "application/json, text/plain, */*");
                                        request.Headers.Add("DNT", "1");
                                        //  request.Headers.Add("Content-Language", "en");
                                        //  request.Headers.Add("Content-Type", "application/json; charset=UTF-8");
                                        request.Headers.Add("Origin", "https://www.irctc.co.in");
                                        request.Headers.Add("Sec-Fetch-Site", "same-origin");
                                        request.Headers.Add("Sec-Fetch-Mode", "cors");
                                        request.Headers.Add("Sec-Fetch-Dest", "empty");
                                        request.Headers.Add("Referer", "https://www.irctc.co.in/nget/booking/train-list");
                                       // request.Headers.Add("Accept-Encoding", "gzip, deflate, br, zstd");

                                        //request.Headers.Add("Content-Length", postDataString.Length.ToString());

                                        try
                                        {
                                           
                                            // HttpResponseMessage response = await client.SendAsync(request);

                                            var response = await httpClient.SendAsync(request);
                                            string responseData = await response.Content.ReadAsStringAsync();
                                            JObject data = JObject.Parse(responseData);

                                            if (response.StatusCode == System.Net.HttpStatusCode.BadGateway)
                                            {
                                                retryCount++;
                                                Console.WriteLine($"502 Bad Gateway,retrying attempt {retryCount}...");
                                              //  if (statusLabel != null && !statusLabel.IsDisposed)
                                              //  {
                                                 //   statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = $"502 Bad Gateway, retrying attempt {retryCount}..."));
                                               // }
                                               
                                                continue;
                                            }
                                            if (response.Headers.Contains("csrf-token"))
                                            {
                                                csrfToken = response.Headers.GetValues("csrf-token").FirstOrDefault();
                                                tokenReceived2 = true;
                                            }
                                            else
                                            {
                                                // Increment the retry count if token is not found
                                                retryCount++;
                                                if (retryCount < maxRetries)
                                                {
                                                    await Task.Delay(100); // Wait before retrying
                                                    continue;
                                                }
                                                else
                                                {
                                                    await SignIn(username, Password);
                                                    // throw new Exception("Failed to retrieve csrf-token after multiple attempts.");
                                                }
                                            }
                                           // csrfToken = response.Headers.GetValues("csrf-token").FirstOrDefault();
                                            // Parse response data
                                           // string responseData = await response.Content.ReadAsStringAsync();
                                           // JObject data = JObject.Parse(responseData);

                                            if (data.ContainsKey("errorMessage"))
                                            {
                                                if (statusLabel != null && !statusLabel.IsDisposed)
                                                {
                                                    statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = data["errorMessage"].ToString()));
                                                }

                                                retryCount++;
                                                if (retryCount >= maxRetries)
                                                {
                                                   
                                                    await SignIn(username, Password);
                                                }

                                                //await Task.Delay(500);                                               
                                                continue;
                                            }
                                            else
                                            {
                                                // MessageBox.Show(responseData);
                                                if (data["bkgCfgs"] != null && data["bkgCfgs"].Any(cfg => cfg["foodChoiceEnabled"]?.ToString() == "true"))
                                                {
                                                    food = "V";  // Assign "V" to food if trainno is 20503 or 20504
                                                }

                                                if (statusLabel != null && !statusLabel.IsDisposed)
                                                {
                                                    statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Getting Booking Details...!"));
                                                }
                                                await Task.Delay(GetRandomDelay());
                                                connection.Close();
                                                await FillBookingDetailsAsync( userHash, uidstatus, accessToken, ticketName, fromStation, toStation, trainno, quotaMapvalue, classMapvalue, formattedDate, username,  Password);
                                               
                                                break;
                                                
                                            }
                                        }
                                        catch (TaskCanceledException ex) when (ex.CancellationToken == CancellationToken.None)
                                        {
                                            retryCount++;
                                            if (retryCount >= maxRetries)
                                            {
                                               // if (statusLabel != null && !statusLabel.IsDisposed)
                                               // {
                                                 //   statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = $"Errorbs: {ex.CancellationToken}"));
                                               // }
                                                await SignIn(username, Password);
                                            }
                                            // Optionally add delay before retry
                                            await Task.Delay(500);
                                            continue;
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"Error: {ex.Message}");
                                            retryCount++;
                                            if (retryCount >= maxRetries)
                                            {
                                                //if (statusLabel != null && !statusLabel.IsDisposed)
                                               // {
                                                 //   statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = $"Errorbs1: {ex.Message}"));
                                               // }
                                                await SignIn(username, Password);
                                            }
                                           
                                            await Task.Delay(500);
                                           // await SignIn(username, Password);
                                            continue;
                                        }
                                    }
                                }
                            }

                           
                        }
                        else
                        {
                            //Invoke(new Action(() => statusLabel.Text = "No station details found!"));
                        }
                    }
                }
                connection.Close();
            }
        }
       
        public async Task FillBookingDetailsAsync( string userHash, string uidstatus, string accessToken, string ticketName, string fromStation,string toStation, string trainno, string quotaMapvalue, string classMapvalue, string formattedDate,string username, string Password)
        {

            string fromstation = fromStation;
            string toostation = toStation;
          //  string newCsrfToken3 = newCsrfToken2;

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT passengername, age, sex, berth FROM passengerdatab2 WHERE ticketName = @ticketName";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ticketName", ticketName);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        // Initialize the passenger JSON array
                        List<JObject> passengersList = new List<JObject>();

                        // Gender and berth mapping
                        Dictionary<string, string> genderMap = new Dictionary<string, string>
            {
                { "Male", "M" },
                { "Female", "F" },
                { "Transgender", "T" }
            };
                        Dictionary<string, string> berthMap = new Dictionary<string, string>
            {
                 
                { "Lower", "LB" },
                { "Middle", "MB" },
                { "Upper", "UB" },
                { "Side Lower", "SL" },
                { "Side Upper", "SU" }
            };

                        int passengerSerialNumber = 0;
                        while (reader.Read())
                        {
                            passengerSerialNumber++;
                            string passengerName = reader["passengername"].ToString();
                            string age = reader["age"].ToString();
                            string gender = reader["sex"].ToString();
                            string berthChoice = reader["berth"].ToString();

                            string genderValue = genderMap.ContainsKey(gender) ? genderMap[gender] : string.Empty;
                            string berthValue = berthMap.ContainsKey(berthChoice) ? berthMap[berthChoice] : string.Empty;

                            
                            var passengerJson = new JObject
{
    { "passengerName", passengerName.Substring(0, Math.Min(16, passengerName.Length)) },
    { "passengerAge", int.Parse(age) },
    { "passengerGender", genderValue },
    { "passengerBerthChoice", berthValue },
    { "passengerFoodChoice", food },
    { "passengerBedrollChoice", null },
    { "passengerNationality", "IN" },
    { "passengerCardTypeMaster", null },
    { "passengerCardNumberMaster", null },
    { "psgnConcType", null },
    { "psgnConcCardId", null },
    { "psgnConcDOB", null },
    { "psgnConcCardExpiryDate", null },
    { "psgnConcDOBP", null },
    { "softMemberId", null },
    { "softMemberFlag", null },
    { "psgnConcCardExpiryDateP", null },
    { "passengerVerified", false },
    { "masterPsgnId", null },
    { "mpMemberFlag", null },
    { "passengerForceNumber", null },
    { "passConcessionType", "0" },
    { "passUPN", null },
    { "passBookingCode", null },
    { "passengerSerialNumber", passengerSerialNumber },
    { "childBerthFlag", true },
    { "passengerCardType", "NULL_IDCARD" },
    { "passengerIcardFlag", false },
    { "passengerCardNumber", null }
};

                            passengersList.Add(passengerJson);
                        }

                        string passengersJsonArray = JsonConvert.SerializeObject(passengersList);

                        Console.WriteLine(passengersJsonArray);

                        stopCountdown1 = false;
                        if (countdownThread1 == null || !countdownThread1.IsAlive)
                        {
                            countdownThread1 = new Thread(StartCountdown1);
                            countdownThread1.Start();
                        }

                        Dictionary<int, int> psgInputAwait = new Dictionary<int, int>
                {
                    { 1, 20000 },
                    { 2, 20000 },
                    { 3, 25000 },
                    { 4, 25000 },
                    { 5, 30000 },
                    { 6, 30000 }
                };

                        // Wait before sending the request
                        if (psgInputAwait.TryGetValue(passengerSerialNumber, out int sleepTime))
                        {
                            Console.WriteLine($"Sleeping for {sleepTime / 1000} seconds for passenger details input.");
                            await Task.Delay(sleepTime); // Wait for the specified time
                        }


                        // var content = new StringContent(postDataString, Encoding.UTF8, "application/json");

                        // MessageBox.Show(postDataString);


                        using (HttpClient client = new HttpClient())
                        {

                            int maxRetries = 50;
                            int retryCount = 0;
                            while (retryCount < maxRetries)
                            {
                                if (!isLoopRunning)
                                {
                                    Console.WriteLine("Loop has been stopped.");
                                    break; // Break the loop if the flag is false
                                }



                                string tid = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString("x");

                                var postData = new
                                {
                                    clusterFlag = "N",
                                    onwardFlag = "N",
                                    cod = "false",
                                    reservationMode = "WS_TA_B2C",
                                    autoUpgradationSelected = true,
                                    gnToCkOpted = false,
                                    paymentType = "2",
                                    twoPhaseAuthRequired = false,
                                    captureAddress = 0,
                                    wsUserLogin = username,
                                    moreThanOneDay = false,
                                    clientTransactionId = tid,
                                    boardingStation = fromstation,
                                    reservationUptoStation = toostation,
                                    mobileNumber = "8757061243",
                                    ticketType = "E",
                                    mainJourneyTxnId = (string)null,
                                    mainJourneyPnr = "",
                                    captcha = "",
                                    generalistChildConfirm = false,
                                    ftBooking = false,
                                    loyaltyRedemptionBooking = false,
                                    loyaltyBankId = (object)null,
                                    loyaltyNumber = (object)null,
                                    nosbBooking = false,
                                    warrentType = 0,
                                    ftTnCAgree = false,
                                    bookingChoice = 1,
                                    bookingConfirmChoice = 1,
                                    bookOnlyIfCnf = true,
                                    returnJourney = (object)null,
                                    connectingJourney = false,
                                    lapAvlRequestDTO = new[]
                       {
            new
            {
                trainNo = trainno,
                journeyDate = formattedDate,
                fromStation = fromstation,
                toStation = toostation,
                journeyClass = classMapvalue,
                quota = quotaMapvalue,
                coachId = (object)null,
                reservationChoice = 4,
                ignoreChoiceIfWl = false,
                travelInsuranceOpted = true,
                warrentType = 0,
                coachPreferred = false,
                autoUpgradation = false,
                bookOnlyIfCnf = true,
                addMealInput = (object)null,
                concessionBooking = false,
                passengerList = passengersList,
                ssQuotaSplitCoach = "N"
            }
        },
                                    gstDetails = new
                                    {
                                        gstIn = "",
                                        error = (object)null
                                    }
                                };

                                string postDataString = JsonConvert.SerializeObject(postData);


                                var content = new StringContent(postDataString, Encoding.UTF8, "application/json");


                                using (var request = new HttpRequestMessage(HttpMethod.Post, "https://www.irctc.co.in/eticketing/protected/mapps1/allLapAvlFareEnq/Y"))
                                {
                                    request.Content = content;
                                    request.Headers.Add("Host", "www.irctc.co.in");
                                    request.Headers.Add("Connection", "keep-alive");
                                    //request.Headers.Add("Content-Length", "");
                                    request.Headers.Add("greq", uidstatus);
                                    request.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
                                    request.Headers.Add("Authorization", accessToken);
                                    request.Headers.Add("bmirak", "webbm");
                                    request.Headers.Add("spa-csrf-token", csrfToken);
                                    request.Headers.Add("Accept-Language", "en-US,en;q=0.0");
                                    request.Headers.Add("sec-ch-ua", "\"Chromium\";v=\"130\", \"Google Chrome\";v=\"130\", \"Not?A_Brand\";v=\"99\"");
                                    request.Headers.Add("bmiyek", userHash);
                                    request.Headers.Add("sec-ch-ua-mobile", "?0");
                                    request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36 Edg/123.0.0.0");
                                    request.Headers.Add("Accept", "application/json, text/plain, */*");
                                    request.Headers.Add("DNT", "1");
                                    // request.Headers.Add("Content-Language", "en");
                                    // request.Headers.Add("Content-Type", "application/json; charset=UTF-8");
                                    request.Headers.Add("Origin", "https://www.irctc.co.in");
                                    request.Headers.Add("Sec-Fetch-Site", "same-origin");
                                    request.Headers.Add("Sec-Fetch-Mode", "cors");
                                    request.Headers.Add("Sec-Fetch-Dest", "empty");
                                    request.Headers.Add("Referer", "https://www.irctc.co.in/nget/booking/psgninput");
                                    //request.Headers.Add("Accept-Encoding", "gzip, deflate, br, zstd");

                                    //request.Headers.Add("Content-Length", postDataString.Length.ToString());


                                    try
                                    {
                                      //  HttpResponseMessage response = await client.SendAsync(request);
                                        var response = await httpClient.SendAsync(request);
                                        if (response.StatusCode == System.Net.HttpStatusCode.BadGateway)
                                        {
                                            retryCount++;
                                            Console.WriteLine($"502 Bad Gateway,retrying attempt {retryCount}...");
                                            continue;
                                        }

                                        if (response.Headers.Contains("csrf-token"))
                                        {
                                            csrfToken = response.Headers.GetValues("csrf-token").FirstOrDefault();
                                        }
                                        else
                                        {
                                            //newCsrfToken3 = null; // or any default value you'd prefer
                                            continue;
                                        }

                                        // string newCsrfToken3 = response.Headers.GetValues("csrf-token").FirstOrDefault();
                                        // Parse response data
                                        string responseData = await response.Content.ReadAsStringAsync();
                                        JObject data = JObject.Parse(responseData);

                                        if (data.ContainsKey("errorMessage"))
                                        {
                                            //throw new Exception(data["errorMessage"].ToString());
                                            if (statusLabel != null && !statusLabel.IsDisposed)
                                            {
                                                statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = data["errorMessage"].ToString()));
                                                // statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "1111"));
                                              
                                            }

                                            retryCount++;
                                            if (retryCount >= maxRetries)
                                            {
                                               
                                                await SignIn(username, Password);
                                            }
                                           
                                            //await Task.Delay(500);
                                            // MessageBox.Show($"Error fillbook: {ex.Message}");
                                            continue;
                                        }
                                        else if (data.ContainsKey("confirmation"))
                                        {
                                            // throw new Exception("Tickets are unavailable, Exiting Booking Process");
                                            if (statusLabel != null && !statusLabel.IsDisposed)
                                            {
                                                statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Tickets No Seat Available...!"));
                                            }
                                            break;
                                        }
                                        else
                                        {
                                            if (data["captchaDto"]?["nlpcaptchEnabled"]?.Value<bool>() == true)
                                            {
                                                // throw new Exception("NLP Captcha is enabled, it is not supported yet");
                                                if (statusLabel != null && !statusLabel.IsDisposed)
                                                {
                                                    statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "NLP Cp enabled,it's not supported..!"));
                                                }
                                                
                                            }
                                            else
                                            {
                                                stopCountdown1 = false;

                                                if (statusLabel != null && !statusLabel.IsDisposed)
                                                {
                                                    statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Verify Final Captcha...!"));
                                                }
                                                
                                                // MessageBox.Show(responseData);
                                                string captchaQuestion = null;

                                                if (data["captchaDto"] != null && data["captchaDto"]["captchaQuestion"] != null)
                                                {
                                                    captchaQuestion = data["captchaDto"]["captchaQuestion"].ToString();
                                                }
                                                else
                                                {
                                                    // Handle the absence of captchaDto or captchaQuestion here
                                                    Console.WriteLine("captchaDto or captchaQuestion not found in the response.");
                                                    continue;
                                                }

                                                string amount1 = data.ContainsKey("totalCollectibleAmount") && data["totalCollectibleAmount"] != null
                                                ? data["totalCollectibleAmount"].ToString()
                                                 : "N/A"; // or provide a default value if "totalCollectibleAmount" is missing
                                                          // string captchaQuestion = data["captchaDto"]["captchaQuestion"].ToString();
                                                amount = amount1;
                                                
                                                availablity = data["avlFareResponseDTO"]?[0]?["avlDayList"]?[0]?["availablityStatus"]?.ToString() ?? "N/A";
                                                Invoke(new Action(() =>
                                                {
                                                    availabilitylebel1.Text = $"{amount} || {availablity}";
                                                    availabilitylebel1.BringToFront();
                                                }));


                                                // string captchaQuestion = data["captchaDto"]["captchaQuestion"].ToString();

                                                await Task.Delay(GetRandomDelay());
                                                connection.Close();
                                                await ConfirmBookingAsync( userHash, uidstatus, accessToken, ticketName, fromStation, toStation, trainno, quotaMapvalue, classMapvalue, formattedDate, username,tid, captchaQuestion, amount1,  Password);
                                                
                                                break;
                                                // Process captchaQuestion as needed
                                            }
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Error: {ex.Message}");
                                       
                                        retryCount++;
                                        if (retryCount >= maxRetries)
                                        {
                                           // if (statusLabel != null && !statusLabel.IsDisposed)
                                          //  {
                                              //  statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = $"Errorflp: {ex.Message}"));
                                           // }
                                            await SignIn(username, Password);
                                        }
                                       // if (statusLabel != null && !statusLabel.IsDisposed)
                                       // {
                                         //   statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = $"Errorflp1: {ex.Message}"));
                                      //  }
                                        await Task.Delay(500);                                      
                                       // MessageBox.Show($"Error fillbook: {ex.Message}");
                                        continue;
                                    }
                                }
                            }
                        }


                    }
                }
                connection.Close();
            }

        }

        public async Task ConfirmBookingAsync( string userHash, string uidstatus, string accessToken, string ticketName, string fromStation, string toStation, string trainno, string quotaMapvalue, string classMapvalue, string formattedDate, string username,string tid,string captchaQuestion,string amount1, string Password)
        {
          //  string newCsrfToken4 = newCsrfToken3;
            string bookingCaptchaResponse = "FAIL";
            string captchaQuestion1 = captchaQuestion;

            var headers = new Dictionary<string, string>();
            headers["Host"] = "www.irctc.co.in";
            headers["Connection"] = "keep-alive";
            headers["greq"] = uidstatus;
            headers["sec-ch-ua-platform"] = "\"Windows\"";
            headers["Authorization"] = accessToken;
            headers["bmirak"] = "webbm";
            headers["spa-csrf-token"] = csrfToken;
            headers["Accept-Language"] = "en-US,en;q=0.7";
            headers["sec-ch-ua"] = "\"Chromium\";v=\"130\", \"Google Chrome\";v=\"130\", \"Not?A_Brand\";v=\"99\"";
            headers["bmiyek"] = userHash;
            headers["sec-ch-ua-mobile"] = "?0";
            headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36 Edg/123.0.0.0";
            headers["Accept"] = "application/json, text/plain, */*";
            headers["DNT"] = "1";
            // headers["Content-Language"] = "en";
            //   headers["Content-Type"] = "application/x-www-form-urlencoded";
            headers["Sec-Fetch-Site"] = "same-origin";
            headers["Sec-Fetch-Mode"] = "cors";
            headers["Sec-Fetch-Dest"] = "empty";
            headers["Referer"] = "https://www.irctc.co.in/nget/booking/reviewBooking";
           // headers["Accept-Encoding"] = "gzip, deflate, br, zstd";

            while (bookingCaptchaResponse != "SUCCESS")
            {

                if (!isLoopRunning)
                {
                    Console.WriteLine("Loop has been stopped.");
                    break; // Break the loop if the flag is false
                }
                captchaAnswer = SolveCaptchaAsync(captchaQuestion1);

               // MessageBox.Show(captchaAnswer);

                headers["spa-csrf-token"] = csrfToken;
                Console.WriteLine("Sending an HTTP request to IRCTC for self-confirmation of already sent booking data using captcha");

                using (var request = new HttpRequestMessage(HttpMethod.Get, $"https://www.irctc.co.in/eticketing/protected/mapps1/captchaverify/{tid}/BOOKINGWS/{captchaAnswer}"))
                {
                    // Add headers to the request
                    foreach (var header in headers)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }

                    try
                    {
                       // HttpResponseMessage response = await client.SendAsync(request);
                        var response = await httpClient.SendAsync(request);

                        response.EnsureSuccessStatusCode(); // Throw if not a success code.

                        // Get CSRF token from response headers
                        csrfToken = response.Headers.GetValues("csrf-token").FirstOrDefault();

                        // Parse response data
                        string responseData = await response.Content.ReadAsStringAsync();
                        JObject data = JObject.Parse(responseData);

                        bookingCaptchaResponse = data["status"].ToString();

                        if (bookingCaptchaResponse == "FAIL")
                        {                           
                             captchaQuestion1 = data["captchaQuestion"].ToString();
                        }
                        else
                        {
                            if (statusLabel != null && !statusLabel.IsDisposed)
                            {
                                statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Opening Bank Server...!"));
                            }

                            // MessageBox.Show(responseData);
                            await Task.Delay(GetRandomDelay());
                            await SelectPaytmUpiGateway( userHash, uidstatus, accessToken, ticketName, fromStation, toStation, trainno, quotaMapvalue, classMapvalue, formattedDate, username, tid, captchaQuestion, amount1,  Password);
                            

                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                       // MessageBox.Show($"Error final cp: {ex.Message}");

                      //  if (statusLabel != null && !statusLabel.IsDisposed)
                        //{
                          //  statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = $"Error fcp: {ex.Message}"));
                       // }
                        continue;
                    }
                }
            }
          
        }

        public async Task SelectPaytmUpiGateway( string userHash, string uidstatus, string accessToken, string ticketName, string fromStation, string toStation, string trainno, string quotaMapvalue, string classMapvalue, string formattedDate, string username, string tid, string captchaQuestion,string amount1, string Password)
        {
            comboBox1.Invoke((MethodInvoker)(() => comboBox1.Enabled = false));
           
            if (selectedItem.Contains("BHIM UPI"))
            {
                bankgatewayid = "117";
            }
            else if (selectedItem.Contains("Pay QR Code"))
            {
                bankgatewayid = "113";
            }
           
            // string newCsrfToken5 = newCsrfToken4;
            int maxRetries = 50;
            int retryCount = 0;
            while (retryCount < maxRetries)
            {
                try
                {
                    if (!isLoopRunning)
                    {
                        Console.WriteLine("Loop has been stopped.");
                        break; // Break the loop if the flag is false
                    }


                    var data = new
                    {
                        bankId = bankgatewayid,
                        txnType = 1,
                        paramList = new List<object>(),
                        amount = amount1,
                        transationId = 0,
                        txnStatus = 1
                    };

                    string dataString = JsonConvert.SerializeObject(data);


                    var headers = new Dictionary<string, string>();


                    headers["Host"] = "www.irctc.co.in";
                    headers["Connection"] = "keep-alive";
                    // headers["Content-Length"] = "";
                    headers["greq"] = uidstatus;
                    headers["sec-ch-ua-platform"] = "\"Windows\"";
                    headers["Authorization"] = accessToken;
                    headers["bmirak"] = "webbm";
                    headers["spa-csrf-token"] = csrfToken;
                    headers["Accept-Language"] = "en-US,en;q=0.0";
                    headers["sec-ch-ua"] = "\"Chromium\";v=\"130\", \"Google Chrome\";v=\"130\", \"Not?A_Brand\";v=\"99\"";
                    headers["bmiyek"] = userHash;
                    headers["sec-ch-ua-mobile"] = "?0";
                    headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36 Edg/123.0.0.0";
                    headers["Accept"] = "application/json, text/plain, */*";
                    headers["DNT"] = "1";
                    // headers["Content-Language"] = "en";
                    // headers["Content-Type"] = "application/json; charset=UTF-8";
                    headers["Origin"] = "https://www.irctc.co.in";
                    headers["Sec-Fetch-Site"] = "same-origin";
                    headers["Sec-Fetch-Mode"] = "cors";
                    headers["Sec-Fetch-Dest"] = "empty";
                    headers["Referer"] = "https://www.irctc.co.in/nget/booking/reviewBooking";
                   // headers["Accept-Encoding"] = "gzip, deflate, br, zstd";






                    var request = new HttpRequestMessage(HttpMethod.Post, $"https://www.irctc.co.in/eticketing/protected/mapps1/bookingInitPayment/{tid}?insurenceApplicable=");
                    request.Content = new StringContent(dataString, Encoding.UTF8, "application/json");


                    foreach (var header in headers)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }

                    var response = await httpClient.SendAsync(request);

                    var csrfTokenValues = response.Headers.GetValues("csrf-token");
                    csrfToken = csrfTokenValues?.FirstOrDefault() ?? "Token not found";

                    string responseBody = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JObject.Parse(responseBody);

                    if (jsonResponse.ContainsKey("errorMsg"))
                    {
                        //throw new Exception(data["errorMessage"].ToString());
                        if (statusLabel != null && !statusLabel.IsDisposed)
                        {
                            statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = jsonResponse["errorMsg"].ToString()));

                        }
                        continue;
                    }
                    else
                    {
                        string oid = jsonResponse["paramList"].First["value"].ToString();
                        // MessageBox.Show(responseBody);
                        await Task.Delay(GetRandomDelay());
                        await GetGatewayRedirectParams( userHash, uidstatus, accessToken, ticketName, fromStation, toStation, trainno, quotaMapvalue, classMapvalue, formattedDate, username, tid, captchaQuestion, amount1, oid);
                        //return "Paytm UPI Payment Gateway Selected Successfully";
                        break;
                    }


                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Errorspg: {ex.Message}");
                    // MessageBox.Show($"Error final cp: {ex.Message}");
                   // if (statusLabel != null && !statusLabel.IsDisposed)
                   // {
                       // statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = $"Errorspg: {ex.Message}"));
                   // }
                    continue;
                }
               
            }

           
        }

        public async Task GetGatewayRedirectParams( string userHash, string uidstatus, string accessToken, string ticketName, string fromStation, string toStation, string trainno, string quotaMapvalue, string classMapvalue, string formattedDate, string username, string tid, string captchaQuestion, string amount1,string oid)
        {

            try
            {
                if (statusLabel != null && !statusLabel.IsDisposed)
                {
                    statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Redirect Bank Gateway...!"));
                }
                // Generate the payload string with unique timestamps
                double timestamp1 = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / (1e5 * new Random().NextDouble());
                double timestamp2 = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / (1e6 * new Random().NextDouble());
                string xx = $"{timestamp1}{csrfToken}{timestamp2}";
                string rawToken = accessToken.Replace("Bearer ", "");
                string payload = $"token={rawToken}&txn={username}%3A{tid}&{username}%3A{tid}={xx}";
               

                // Create the request with headers
                var request = new HttpRequestMessage(HttpMethod.Post, "https://www.wps.irctc.co.in/eticketing/PaymentRedirect")
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/x-www-form-urlencoded")
                };

                request.Headers.Add("Host", "www.wps.irctc.co.in");
                request.Headers.Add("Connection", "keep-alive");
                //  request.Headers.Add("Content-Length", "");
                request.Headers.Add("Cache-Control", "max-age=0");
                request.Headers.Add("sec-ch-ua", "\"Chromium\";v=\"130\", \"Google Chrome\";v=\"130\", \"Not?A_Brand\";v=\"99\"");
                request.Headers.Add("sec-ch-ua-mobile", "?0");
                request.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
                request.Headers.Add("Origin", "https://www.irctc.co.in");
                request.Headers.Add("DNT", "1");
                request.Headers.Add("Upgrade-Insecure-Requests", "1");
                // request.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36 Edg/123.0.0.0");
                request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                request.Headers.Add("Sec-Fetch-Site", "same-site");
                request.Headers.Add("Sec-Fetch-Mode", "navigate");
                request.Headers.Add("Sec-Fetch-User", "?1");
                request.Headers.Add("Sec-Fetch-Dest", "document");
                request.Headers.Add("Referer", "https://www.irctc.co.in/");
                // request.Headers.Add("Accept-Encoding", "gzip, deflate, br, zstd");
                request.Headers.Add("Accept-Language", "en-US,en;q=0.9");

               
                Console.WriteLine("Fetching Paytm UPI Gateway Redirect Parameters");


                // Send the request and get the response
              //  HttpResponseMessage response = await client.SendAsync(request);
                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    string paytmHtml = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Received Paytm UPI Gateway Redirect Parameters from IRCTC Successfully");

                    // Show the response content in a message box
                    // MessageBox.Show(paytmHtml, "Response Content1");
                    await Task.Delay(GetRandomDelay());

                    if (bankgatewayid == "113")
                    {

                        await qrupipayment(userHash, uidstatus, accessToken, ticketName, fromStation, toStation, trainno, quotaMapvalue, classMapvalue, formattedDate, username, tid, captchaQuestion, amount1, paytmHtml, oid);

                    }
                    else if (bankgatewayid == "117")
                    {
                        await GoToPaytm( userHash, uidstatus, accessToken, ticketName, fromStation, toStation, trainno, quotaMapvalue, classMapvalue, formattedDate, username, tid, captchaQuestion, amount1, paytmHtml,oid);

                    }


                    // return paytmHtml;
                }
                else
                {
                    string errorMessage = $"Errorgtwr: {response.StatusCode}";
                    // MessageBox.Show(errorMessage, "Request Failed");

                    if (statusLabel != null && !statusLabel.IsDisposed)
                    {
                        statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = errorMessage));
                    }
                    // return errorMessage;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errorgtwr: {ex.Message}");
               // MessageBox.Show($"Errorgtwr: {ex.Message}");
               // if (statusLabel != null && !statusLabel.IsDisposed)
               // {
                  //  statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = $"Errorgtwr: {ex.Message}"));
              //  }
                //break;
                // throw; // Rethrow the exception or handle it as needed.
            }

           
        }



        public async Task qrupipayment(string userHash, string uidstatus, string accessToken, string ticketName, string fromStation, string toStation, string trainno, string quotaMapvalue, string classMapvalue, string formattedDate, string username, string tid, string captchaQuestion, string amount1, string paytmHtml, string oid)
        {
            try
            {
                // Load HTML to parse
                // Instead of using 'HtmlDocument', specify 'HtmlAgilityPack.HtmlDocument'
                HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.LoadHtml(paytmHtml);


                var form = htmlDoc.DocumentNode.SelectSingleNode("//form");

                if (form == null)
                {
                    Console.WriteLine("Form element not found in the HTML document.");
                    return; // Or handle the case appropriately
                }

                string actionUrl = form.GetAttributeValue("action", string.Empty);

                // Extract input elements and their names/values
                Dictionary<string, string> bodyData = new Dictionary<string, string>();
                foreach (HtmlNode input in form.SelectNodes("//input"))
                {
                    string name = input.GetAttributeValue("name", string.Empty);
                    string value = input.GetAttributeValue("value", string.Empty);
                    if (!string.IsNullOrEmpty(name))
                    {
                        bodyData[name] = value;
                    }
                }

                // Create request headers
                var headers = new Dictionary<string, string>
        {
            {"Method", "POST"},
            {"Host", "www.irctcipay.com"},
            {"Connection", "keep-alive"},
            //{"Content-Length", ""},
            {"Cache-Control", "max-age=0"},
            {"sec-ch-ua", "\"Chromium\";v=\"130\", \"Google Chrome\";v=\"130\", \"Not?A_Brand\";v=\"99\""},
            {"sec-ch-ua-mobile", "?0"},
            {"Sec-Ch-Ua-Platform", "\"Windows\""},
            {"Origin", "https://www.wps.irctc.co.in"},
            {"DNT", "1"},
            {"Upgrade-Insecure-Requests", "1"},
            //{"Content-Type", "application/x-www-form-urlencoded"},
            {"User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36 Edg/123.0.0.0"},
            {"Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7"},
            {"Sec-Fetch-Site", "cross-site"},
            {"Sec-Fetch-Mode", "navigate"},
            {"Sec-Fetch-Dest", "document"},
            {"Referer", "https://www.wps.irctc.co.in/"},
            //{"Accept-Encoding", "gzip, deflate, br, zstd"},
            {"Accept-Language", "en-US,en;q=0.9"}
        };

                // Build POST body content
                var bodyContent = new FormUrlEncodedContent(bodyData);



                // Prepare HTTP request
                using (HttpClient client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, actionUrl)
                    {
                        Content = bodyContent
                    };

                    foreach (var header in headers)
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }

                    // Send the request
                    var response = await httpClient.SendAsync(request);



                    if (response.IsSuccessStatusCode)
                    {
                        string scriptData = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Reached Paytm UPI Gateway for Payment");
                        // MessageBox.Show(scriptData);
                        if (statusLabel != null && !statusLabel.IsDisposed)
                        {
                            statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Collecting Payment Info...!"));
                        }

                        await Task.Delay(GetRandomDelay());
                        await clickingqrcode(ticketName, tid, accessToken, userHash, uidstatus);
                       
                    }
                    else
                    {
                        string errorMessage = $"qrupipayment: {response.StatusCode}";
                        Console.WriteLine(errorMessage);
                        if (statusLabel != null && !statusLabel.IsDisposed)
                        {
                            statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = errorMessage));
                        }

                        //return errorMessage;
                        // MessageBox.Show(errorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"qrupupayment: {ex.Message}");
               // if (statusLabel != null && !statusLabel.IsDisposed)
                //{
                  //  statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = $"qrupipayment: {ex.Message}"));
                //}

            }
        }
        static string GenerateRandomToken()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            StringBuilder token = new StringBuilder(32);

            for (int i = 0; i < 32; i++) // Generate a 32-character string
            {
                token.Append(chars[random.Next(chars.Length)]);
            }

            return token.ToString();
        }
        public async Task clickingqrcode(string ticketName,string tid,string accessToken,string userHash,string uidstatus)
        {
            while (isFilling == true) // Added paybtnClicked condition
            {
                await Task.Delay(200);

            }

            if (quota.ToLower() == "premium tatkal")
            {
                Invoke(new Action(() =>
                {
                    string available = availablity; // Replace with your actual available count

                    // Example of formatting parts of the text
                    richTextBox1.Clear();
                    richTextBox1.AppendText("\n     PT Fair increased...!\n Do You Want to Procced.\n");

                    // Format the amount text
                    richTextBox1.SelectionColor = Color.Blue; // Change color
                    richTextBox1.AppendText($"    Rs: {amount}  - ");
                    richTextBox1.SelectionColor = Color.Black; // Reset color

                    // Format the availability text
                    richTextBox1.SelectionColor = Color.Green; // Change color
                    richTextBox1.AppendText($"  {available}\n");
                    richTextBox1.SelectionColor = Color.Black; // Reset color


                }));

                decimal ptFareAmountValue;
                decimal amountValueDecimal;

                // Try parsing to ensure valid numeric values
                if (decimal.TryParse(PTfairticket, out ptFareAmountValue) && decimal.TryParse(amount, out amountValueDecimal))
                {
                    // Check if ptFareAmount is less than amountValue
                    if (ptFareAmountValue < amountValueDecimal)
                    {
                        Invoke(new Action(() =>
                        {
                            richTextBox1.Visible = true;
                            richTextBox1.BringToFront();
                            paybtn.Visible = true;
                            paybtn.BringToFront();
                            // statusLabel.Text = "Do you want to continue?";
                        }));

                        while (paybtnClicked == false) // Added paybtnClicked condition
                        {
                            await Task.Delay(200);

                        }
                    }
                }
            }




            Invoke(new Action(() =>
            {
                string available = availablity; // Replace with your actual available count

                // Example of formatting parts of the text
                richTextBox1.Clear();
                richTextBox1.AppendText("\n Another Ticket Already Procced.\n");

                // Format the amount text
                richTextBox1.SelectionColor = Color.Blue; // Change color
                richTextBox1.AppendText($"    Rs: {amount}  - ");
                richTextBox1.SelectionColor = Color.Black; // Reset color

                // Format the availability text
                richTextBox1.SelectionColor = Color.Green; // Change color
                richTextBox1.AppendText($"  {available}\n");
                richTextBox1.SelectionColor = Color.Black; // Reset color
            }));


            bool isPaymentMode = PaymentStatusTracker.IsAnyFormInPaymentMode(ticketName);

            if (isPaymentMode)
            {
                // Show the button and update status label
                Invoke(new Action(() =>
                {
                    richTextBox1.Visible = true;
                    richTextBox1.BringToFront();
                    paybtn.Visible = true;
                    paybtn.BringToFront();
                    // statusLabel.Text = "Do you want to continue?";
                }));
            }

            while (isPaymentMode && paybtnClicked == false) // Added paybtnClicked condition
            {
                await Task.Delay(100);

                // Check if payment mode is still true
                isPaymentMode = PaymentStatusTracker.IsAnyFormInPaymentMode(ticketName);
            }
            // Set the payment mode for this form to true
            PaymentStatusTracker.SetPaymentMode(ticketName, true);





            string url = "https://www.irctcipay.com/pgui/jsp/upiPay";
            string token = GenerateRandomToken();
           // MessageBox.Show(token);
            
            
            // Create HttpClient
            using (HttpClient client = new HttpClient())
            {
                // Set headers
                client.DefaultRequestHeaders.Add("Connection", "keep-alive");
                client.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.Add("sec-ch-ua", "\"Chromium\";v=\"130\", \"Google Chrome\";v=\"130\", \"Not?A_Brand\";v=\"99\"");
                client.DefaultRequestHeaders.Add("DNT", "1");
                client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
                client.DefaultRequestHeaders.Add("Origin", "https://www.irctcipay.com");
                client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
                client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
                client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
                client.DefaultRequestHeaders.Add("Referer", "https://www.irctcipay.com/pgui/jsp/surchargePaymentPage.jsp");
               // client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br, zstd");
                client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");

                // Create the multipart form data
                var content = new MultipartFormDataContent("----WebKitFormBoundarylWMBI3tabV3nD4tC");
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data; boundary=----WebKitFormBoundarylWMBI3tabV3nD4tC");

                content.Add(new StringContent(token), "token");
                content.Add(new StringContent("dummy"), "upiCustName");
                content.Add(new StringContent("UP"), "paymentType");
                content.Add(new StringContent("UP"), "mopType");
                content.Add(new StringContent(amount), "amount");
                content.Add(new StringContent("356"), "currencyCode");
                content.Add(new StringContent("Y"), "IsQr");
                content.Add(new StringContent("Chrome"), "browserName");
                content.Add(new StringContent("130"), "browserVersion");

                try
                {
                    // Send the POST request
                    HttpResponseMessage response = await httpClient.PostAsync(url, content);

                    // Read the response
                    string responseString = await response.Content.ReadAsStringAsync();
                    // Parse JSON response
                    var parsedJson = JObject.Parse(responseString);

                    // Extract the required fields
                    string imageData = parsedJson["imageData"]?.ToString();
                    string base64Image = imageData?.Substring(imageData.IndexOf(",") + 1);

                    DisplayImageInPictureBox(base64Image);
                    // MessageBox.Show(base64Image);

                    qrupipicturebox.Invoke((MethodInvoker)(() => qrupipicturebox.Visible = true));
                    qrupipicturebox.Invoke((MethodInvoker)(() => qrupipicturebox.BringToFront()));

                    string pgRefNum = parsedJson["pgRefNum"]?.ToString();
                    string pgRefHash = parsedJson["pgRefHash"]?.ToString();
                    string encdata = parsedJson["responseFields"]?["encdata"]?.ToString();

                    await verifyupirsponse(tid, accessToken, userHash, uidstatus,pgRefNum, pgRefHash, encdata,token);
                    // Print the response
                    Console.WriteLine("Response:");
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"clickingqrcode: {ex.Message}");
                }
            }
        }
        public async Task verifyupirsponse(string tid, string accessToken, string userHash,string uidstatus,string pgRefNum, string pgRefHash, string encdata,string token)
        {
            // URL to send the request
            string url = "https://www.irctcipay.com/pgui/jsp/verifyUpiResponse";
            // MessageBox.Show(pgRefNum);
            // Create HttpClient

            while (true)
            {
                using (HttpClient client = new HttpClient())
                {
                    // Set headers
                    client.DefaultRequestHeaders.Add("Connection", "keep-alive");
                    client.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36");
                    client.DefaultRequestHeaders.Add("sec-ch-ua", "\"Chromium\";v=\"130\", \"Google Chrome\";v=\"130\", \"Not?A_Brand\";v=\"99\"");
                    client.DefaultRequestHeaders.Add("DNT", "1");
                    client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
                    client.DefaultRequestHeaders.Add("Origin", "https://www.irctcipay.com");
                    client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
                    client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
                    client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
                    client.DefaultRequestHeaders.Add("Referer", "https://www.irctcipay.com/pgui/jsp/surchargePaymentPage.jsp");
                    // client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br, zstd");
                    client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");

                    // Create the multipart form data
                    var content = new MultipartFormDataContent("----WebKitFormBoundarylWMBI3tabV3nD4tC");
                    content.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data; boundary=----WebKitFormBoundarylWMBI3tabV3nD4tC");

                    content.Add(new StringContent(token), "token");
                    content.Add(new StringContent(pgRefNum), "pgRefNum");
                    content.Add(new StringContent(pgRefHash), "pgRefHash");


                    try
                    {
                        // Send the POST request
                        HttpResponseMessage response = await httpClient.PostAsync(url, content);

                        // Read the response
                        string responseString = await response.Content.ReadAsStringAsync();

                        if (!string.IsNullOrEmpty(responseString))
                        {

                            JObject parsedJson = JObject.Parse(responseString);
                            // Get transactionStatus
                            string transactionStatus = parsedJson["transactionStatus"]?.ToString();
                           
                            
                            if (transactionStatus == "Failed")
                            {
                                // Get PG_TXN_MESSAGE and trim "ZM::"
                                qrpictureBox1.Invoke((MethodInvoker)(() => qrpictureBox1.Visible = false));
                                qrupipicturebox.Invoke((MethodInvoker)(() => qrupipicturebox.Visible = false));
                                string pgTxnMessage = parsedJson["responseFields"]?["PG_TXN_MESSAGE"]?.ToString();
                                string trimmedMessage = pgTxnMessage?.Replace("ZM::", "").Trim();
                                if (statusLabel != null && !statusLabel.IsDisposed)
                                {
                                    statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = trimmedMessage));
                                  
                                    InsertDummyDatafail();
                                   
                                }
                               
                                string encDataqr = parsedJson["responseFields"]?["encdata"]?.ToString();
                                // await CallbackToIrctcforqrupi(tid, accessToken, userHash, uidstatus, encDataqr);

                                // Display trimmed message on status label

                                
                            }
                            else
                            {
                                // MessageBox.Show(encDataqr);
                                qrpictureBox1.Invoke((MethodInvoker)(() => qrpictureBox1.Visible = false));
                                
                                if (statusLabel != null && !statusLabel.IsDisposed)
                                {
                                    statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Final Hit PNR...!"));
                                }

                                qrupipicturebox.Invoke((MethodInvoker)(() => qrupipicturebox.Visible = false));
                                finalhitpnrblink.Invoke((MethodInvoker)(() => finalhitpnrblink.Visible = true));
                                finalhitpnrblink.Invoke((MethodInvoker)(() => finalhitpnrblink.BringToFront()));


                                string encDataqr = parsedJson["responseFields"]?["encdata"]?.ToString();
                                await CallbackToIrctcforqrupi(tid, accessToken, userHash, uidstatus, encDataqr);

                                // Else, move forward with other logic
                                Console.WriteLine("Transaction Successful or other action required.");
                            }

                            
                            break; // Exit the loop when data is received
                        }
                        else
                        {

                            // Optional: Add a delay to avoid spamming the server
                            await Task.Delay(5000); // 1-second delay
                            continue;
                        }

                        


                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
            }

        }
        public async Task CallbackToIrctcforqrupi(string tid,string accessToken,string userHash,string uidstatus,string encDataqr)
        {

            try
            {
                


              //  var form = htmlDoc.DocumentNode.SelectSingleNode("//form[@name='TESTFORM']");


                string actionUrl = "https://www.wps.irctc.co.in/eticketing/BankResponse";

                // Create a dictionary with the encdata parameter
                Dictionary<string, string> bodyData = new Dictionary<string, string>
                {
                   { "encdata", encDataqr }
                  
                
                };

                // Convert dictionary to form URL-encoded content
                var data = new FormUrlEncodedContent(bodyData);




                // Clear existing headers
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("METHOD", "POST");
                // Set the headers as specified
                httpClient.DefaultRequestHeaders.Add("Host", "www.wps.irctc.co.in");
                httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
                //  httpClient.DefaultRequestHeaders.Add("Content-Length", "570");
                httpClient.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
                httpClient.DefaultRequestHeaders.Add("sec-ch-ua", "\"Chromium\";v=\"130\", \"Google Chrome\";v=\"130\", \"Not?A_Brand\";v=\"99\"");
                httpClient.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
                httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
                httpClient.DefaultRequestHeaders.Add("Origin", "https://secure.paytmpayments.com");
                httpClient.DefaultRequestHeaders.Add("DNT", "1");
                httpClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                //httpClient.DefaultRequestHeaders.Add("Content-Type", "application/x-www-form-urlencoded");
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36");
                httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "cross-site");
                httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
                httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
                httpClient.DefaultRequestHeaders.Add("Referer", "https://secure.paytmpayments.com/");
                // httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br, zstd");
                httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");


                Console.WriteLine("Sending Payment Response Callback Data to IRCTC");

                var response = await httpClient.PostAsync(actionUrl, data);
                // string responseData1 = await response.Content.ReadAsStringAsync();
                //MessageBox.Show(responseData1);

                if (response.StatusCode == System.Net.HttpStatusCode.Redirect)
                {
                    Console.WriteLine("Received a redirect Response from IRCTC. Going to that redirect URL");

                    var redirectUrl = response.Headers.Location.ToString();

                    httpClient.DefaultRequestHeaders.Clear();

                    // Set the headers as specified
                    httpClient.DefaultRequestHeaders.Add("Host", "www.irctc.co.in");
                    httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
                    httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36");
                    httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                    httpClient.DefaultRequestHeaders.Add("DNT", "1");
                    httpClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua", "\"Chromium\";v=\"130\", \"Google Chrome\";v=\"130\", \"Not?A_Brand\";v=\"99\"");
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
                    httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
                    httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
                    httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
                    // httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br, zstd");
                    httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");


                    response = await httpClient.GetAsync(redirectUrl);
                    string responseData = await response.Content.ReadAsStringAsync();
                    // MessageBox.Show(responseData);
                }
                await Task.Delay(GetRandomDelay());
                await GetBookingDetailsqr( tid,  accessToken,  userHash,  uidstatus);

                //return "IRCTC Callback Payment Successful";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"errorcallbqr: {ex.Message}");
                //return "IRCTC Callback Payment Failed";
            }


        }
        public async Task GetBookingDetailsqr(string tid, string accessToken, string userHash, string uidstatus)
        {
           
            string actionUrl = $"https://www.wps.irctc.co.in/eticketing/protected/mapps1/bookingData/{tid}";

           
            int maxRetries = 5;
            int retryCount = 0;
            while (retryCount < maxRetries)
            {
                if (!isLoopRunning)
                {
                    Console.WriteLine("Loop has been stopped.");
                    break; // Break the loop if the flag is false
                }
                // Configure HttpClient to ignore SSL certificate errors (only for development)
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true;
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    // Set headers
                    httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");

                    httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.0");
                    httpClient.DefaultRequestHeaders.Add("Authorization", accessToken);


                    httpClient.DefaultRequestHeaders.Add("Origin", "https://www.irctc.co.in");
                    httpClient.DefaultRequestHeaders.Add("Referer", "https://www.irctc.co.in/");

                    httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36 Edg/123.0.0.0");
                    httpClient.DefaultRequestHeaders.Add("bmirak", "webbm");
                    httpClient.DefaultRequestHeaders.Add("bmiyek", userHash);
                    httpClient.DefaultRequestHeaders.Add("greq", uidstatus);

                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
                    httpClient.DefaultRequestHeaders.Add("spa-csrf-token", $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");

                    try
                    {
                        // Send GET request
                        HttpResponseMessage response = await httpClient.GetAsync(actionUrl);
                        response.EnsureSuccessStatusCode();

                        // Read response content
                        string responseData = await response.Content.ReadAsStringAsync();
                        JObject jsonResponse = JObject.Parse(responseData);


                        // Check if lastTxnStatus is "Booked" (case-insensitive, trimmed)
                        string lastTxnStatus = jsonResponse["userDetail"]?["lastTxnStatus"]?.ToString().Trim();
                        var bookingResponseArray = jsonResponse["bookingResponseDTO"] as JArray;
                        var firstBookingResponse = bookingResponseArray[0] as JObject;


                        if (string.Equals(lastTxnStatus, "Booked", StringComparison.OrdinalIgnoreCase))
                        {
                            // Extract required details
                            string pnrNo = jsonResponse["bookingResponseDTO"]?[0]?["pnrNumber"]?.ToString();
                            string dateOfJourney = jsonResponse["bookingResponseDTO"]?[0]?["journeyDate"]?.ToString();
                            string trainNumber = jsonResponse["bookingResponseDTO"]?[0]?["trainNumber"]?.ToString();
                            string fromStation1 = jsonResponse["bookingResponseDTO"]?[0]?["fromStn"]?.ToString();
                            string toStation1 = jsonResponse["bookingResponseDTO"]?[0]?["destStn"]?.ToString();
                            string bookingDate = jsonResponse["bookingDate"]?.ToString();
                            string bookingTime = "";
                            if (DateTime.TryParse(bookingDate, out DateTime parsedDate))
                            {
                                bookingTime = parsedDate.ToString("HH:mm:ss");
                            }
                            DateTime parsedDate1;
                            if (DateTime.TryParse(dateOfJourney, out parsedDate1))
                            {
                                dateOfJourney = parsedDate1.ToString("dd-MMM-yyyy");  // Format as Day-Month-Year
                            }
                            Invoke(new Action(() =>
                            {

                                // Display details in labels
                                pnrlebel.Text = $"PNR: {pnrNo}";
                                datelbl.Text = $"Date: {dateOfJourney}";
                                trainlebel.Text = $"Train No: {trainNumber}";
                                fromtolebel.Text = $"From|To: {fromStation1}_{toStation1}";
                                timelebel.Text = $"Time: {bookingTime}";
                                idlebel.Text = $"ID: {ticketunsername}";


                            }));

                            finalhitpnrblink.Invoke((MethodInvoker)(() => finalhitpnrblink.Visible = false));
                            successpanel.Invoke((MethodInvoker)(() => successpanel.Visible = true));
                            successpanel.Invoke((MethodInvoker)(() => successpanel.BringToFront()));
                            InsertDummyDatasucess(pnrNo);
                            break;
                        }
                        else if (firstBookingResponse != null && firstBookingResponse.ContainsKey("errorMessage"))
                        {
                            string errorMessage = firstBookingResponse["errorMessage"].ToString();
                            finalhitpnrblink.Invoke((MethodInvoker)(() => finalhitpnrblink.Visible = false));
                            if (statusLabel != null && !statusLabel.IsDisposed)
                            {
                                statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = errorMessage));
                            }

                            InsertDummyDatafail();
                            break;
                        }

                        else
                        {

                            finalhitpnrblink.Invoke((MethodInvoker)(() => finalhitpnrblink.Visible = false));
                            if (statusLabel != null && !statusLabel.IsDisposed)
                            {
                                statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Booking Failed Login Check History.!"));
                            }
                            InsertDummyDatafail();
                            Console.WriteLine("Booking status is not 'Booked'.");
                            // MessageBox.Show(responseData, "Booking Details");
                            break;
                        }


                    }
                    catch (HttpRequestException ex)
                    {

                        if (retryCount >= maxRetries)
                        {
                            finalhitpnrblink.Invoke((MethodInvoker)(() => finalhitpnrblink.Visible = false));
                            if (statusLabel != null && !statusLabel.IsDisposed)
                            {
                                statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Booking Failed Login Check History.!"));
                            }
                            InsertDummyDatafail();
                            break;
                        }


                       
                        Console.WriteLine($"Request error: {ex.Message}", "Error");
                        // MessageBox.Show($"Request error: {ex.Message}", "Error");
                        continue;
                    }
                    catch (Exception ex)
                    {
                        if (retryCount >= maxRetries)
                        {
                            finalhitpnrblink.Invoke((MethodInvoker)(() => finalhitpnrblink.Visible = false));
                            if (statusLabel != null && !statusLabel.IsDisposed)
                            {
                                statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Booking Failed Login Check History.!"));
                            }
                            InsertDummyDatafail();
                            break;
                        }
                        Console.WriteLine($"Request error: {ex.Message}", "Error");
                        // MessageBox.Show($"Unexpected error: {ex.Message}", "Error");
                        continue;
                    }
                }


            }




           
        }



        public async Task GoToPaytm( string userHash, string uidstatus, string accessToken, string ticketName, string fromStation, string toStation, string trainno, string quotaMapvalue, string classMapvalue, string formattedDate, string username, string tid, string captchaQuestion, string amount1, string paytmHtml,string oid)
        {
            try
            {
                // Load HTML to parse
                // Instead of using 'HtmlDocument', specify 'HtmlAgilityPack.HtmlDocument'
                HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.LoadHtml(paytmHtml);


                var form = htmlDoc.DocumentNode.SelectSingleNode("//form");

                if (form == null)
                {
                    Console.WriteLine("Form element not found in the HTML document.");
                    return; // Or handle the case appropriately
                }

                string actionUrl = form.GetAttributeValue("action", string.Empty);

                // Extract input elements and their names/values
                Dictionary<string, string> bodyData = new Dictionary<string, string>();
                foreach (HtmlNode input in form.SelectNodes("//input"))
                {
                    string name = input.GetAttributeValue("name", string.Empty);
                    string value = input.GetAttributeValue("value", string.Empty);
                    if (!string.IsNullOrEmpty(name))
                    {
                        bodyData[name] = value;
                    }
                }

                // Create request headers
                var headers = new Dictionary<string, string>
        {
            {"Method", "POST"},
            {"Host", "secure.paytmpayments.com"},
            {"Connection", "keep-alive"},
            //{"Content-Length", ""},
            {"Cache-Control", "max-age=0"},
            {"sec-ch-ua", "\"Chromium\";v=\"130\", \"Google Chrome\";v=\"130\", \"Not?A_Brand\";v=\"99\""},
            {"sec-ch-ua-mobile", "?0"},
            {"Sec-Ch-Ua-Platform", "\"Windows\""},
            {"Origin", "https://www.wps.irctc.co.in"},
            {"DNT", "1"},
            {"Upgrade-Insecure-Requests", "1"},
            //{"Content-Type", "application/x-www-form-urlencoded"},
            {"User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36 Edg/123.0.0.0"},
            {"Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7"},
            {"Sec-Fetch-Site", "cross-site"},
            {"Sec-Fetch-Mode", "navigate"},
            {"Sec-Fetch-Dest", "document"},
            {"Referer", "https://www.wps.irctc.co.in/"},
            //{"Accept-Encoding", "gzip, deflate, br, zstd"},
            {"Accept-Language", "en-US,en;q=0.9"}
        };

                // Build POST body content
                var bodyContent = new FormUrlEncodedContent(bodyData);

                // Prepare HTTP request
                using (HttpClient client = new HttpClient())
                {
                    // Set headers
                    foreach (var header in headers)
                    {
                        client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                    }

                    // Send POST request to action URL
                   // HttpResponseMessage response = await client.PostAsync(actionUrl, bodyContent);
                    //var response = await httpClient.SendAsync(request);
                    var request = new HttpRequestMessage(HttpMethod.Post, actionUrl)
                    {
                        Content = bodyContent
                    };

                    // Send the request
                    var response = await httpClient.SendAsync(request);



                    if (response.IsSuccessStatusCode)
                    {
                        string scriptData = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Reached Paytm UPI Gateway for Payment");
                        // MessageBox.Show(scriptData);
                        if (statusLabel != null && !statusLabel.IsDisposed)
                        {
                            statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Collecting Payment Info...!"));
                        }
                        // Proceed with the next step if needed
                        // Call the method to handle the next page (if applicable)
                        //MessageBox.Show(paytmHtml, "Response Content1");


                         await PaytmPgLandingPage( userHash, uidstatus, accessToken, ticketName, fromStation, toStation, trainno, quotaMapvalue, classMapvalue, formattedDate, username, tid, captchaQuestion, amount1, paytmHtml, scriptData,oid);

                        // return "Gone to Paytm UPI Gateway Successfully";
                    }
                    else
                    {
                        string errorMessage = $"Errorpgtw: {response.StatusCode}";
                        Console.WriteLine(errorMessage);
                        if (statusLabel != null && !statusLabel.IsDisposed)
                        {
                            statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = errorMessage));
                        }

                        //return errorMessage;
                       // MessageBox.Show(errorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ErrorGoToPaytm: {ex.Message}");
               // if (statusLabel != null && !statusLabel.IsDisposed)
               // {
               //     statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = $"ErrorGoToPaytm: {ex.Message}"));
               // }

            }
        }

        public async Task PaytmPgLandingPage( string userHash, string uidstatus, string accessToken, string ticketName, string fromStation, string toStation, string trainno, string quotaMapvalue, string classMapvalue, string formattedDate, string username, string tid, string captchaQuestion, string amount1, string paytmHtml, string scriptData,string oid)
        {
            try
            {
                
                // Extract pushAppData and encodeFlag from script_data
                string pushAppData = scriptData.Split(new[] { "pushAppData=\"" }, StringSplitOptions.None)[1].Split(new[] { "\",encodeFlag=" }, StringSplitOptions.None)[0];
                string encodeFlag = scriptData.Split(new[] { ",encodeFlag=\"" }, StringSplitOptions.None)[1].Split(new[] { "\";" }, StringSplitOptions.None)[0];
                
                // Decode if necessary
                string decodedData = encodeFlag == "true" ? DecodeBase64Unicode(pushAppData) : pushAppData;
               
                JObject data = JObject.Parse(decodedData);
              
                string txnToken = data["txnToken"].ToString();
                string MID1 = data["merchant"]["mid"].ToString();
                
                // Store txnToken and MID for further processing
                string txntkn = txnToken;
                string MID = MID1;

              //  await Task.Delay(100);
                // Proceed with the UPI request
                await Task.Delay(GetRandomDelay());
                await SendUpiCollectRequest( userHash, uidstatus, accessToken, ticketName, fromStation, toStation, trainno, quotaMapvalue, classMapvalue, formattedDate, username, tid, captchaQuestion, amount1, paytmHtml, scriptData, oid,txntkn, MID);
               // await SendUpiCollectRequestAsync(oid, txntkn, MID);
                Console.WriteLine("Paytm PG Landing Page Loaded Successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PaytmPgLandingPage: {ex.Message}");

            }
        }
        
        public async Task SendUpiCollectRequest( string userHash, string uidstatus, string accessToken, string ticketName, string fromStation, string toStation, string trainno, string quotaMapvalue, string classMapvalue, string formattedDate, string username, string tid, string captchaQuestion, string amount1, string paytmHtml, string scriptData, string oid,string txntkn,string MID)
        {
           
            try
            {
                while (isFilling == true) // Added paybtnClicked condition
                {
                    await Task.Delay(200);

                }

                if ( quota.ToLower() == "premium tatkal")
                {
                    Invoke(new Action(() =>
                    {
                        string available = availablity; // Replace with your actual available count
                       
                        // Example of formatting parts of the text
                        richTextBox1.Clear();
                        richTextBox1.AppendText("\n   PT Fair increased...!\n Do You Want to Procced.\n");

                        // Format the amount text
                        richTextBox1.SelectionColor = Color.Blue; // Change color
                        richTextBox1.AppendText($"    Rs: {amount}  - ");
                        richTextBox1.SelectionColor = Color.Black; // Reset color

                        // Format the availability text
                        richTextBox1.SelectionColor = Color.Green; // Change color
                        richTextBox1.AppendText($"  {available}\n");
                        richTextBox1.SelectionColor = Color.Black; // Reset color


                    }));

                    decimal ptFareAmountValue;
                    decimal amountValueDecimal;

                    // Try parsing to ensure valid numeric values
                    if (decimal.TryParse(PTfairticket, out ptFareAmountValue) && decimal.TryParse(amount, out amountValueDecimal))
                    {
                        // Check if ptFareAmount is less than amountValue
                        if (ptFareAmountValue < amountValueDecimal)
                        {
                            Invoke(new Action(() =>
                            {
                                richTextBox1.Visible = true;
                                richTextBox1.BringToFront();
                                paybtn.Visible = true;
                                paybtn.BringToFront();
                                // statusLabel.Text = "Do you want to continue?";
                            }));

                             while (paybtnClicked == false) // Added paybtnClicked condition
                             {
                                    await Task.Delay(200);

                             }
                        }
                    }
                }

               


                Invoke(new Action(() =>
                {
                    string available = availablity; // Replace with your actual available count

                    // Example of formatting parts of the text
                    richTextBox1.Clear();
                    richTextBox1.AppendText("\n Another Ticket Already Procced.\n");

                    // Format the amount text
                    richTextBox1.SelectionColor = Color.Blue; // Change color
                    richTextBox1.AppendText($"    Rs: {amount}  - ");
                    richTextBox1.SelectionColor = Color.Black; // Reset color

                    // Format the availability text
                    richTextBox1.SelectionColor = Color.Green; // Change color
                    richTextBox1.AppendText($"  {available}\n");
                    richTextBox1.SelectionColor = Color.Black; // Reset color
                }));


                bool isPaymentMode = PaymentStatusTracker.IsAnyFormInPaymentMode(ticketName);

                if (isPaymentMode)
                {
                    // Show the button and update status label
                    Invoke(new Action(() =>
                    {
                        richTextBox1.Visible = true;
                        richTextBox1.BringToFront();
                        paybtn.Visible = true;
                        paybtn.BringToFront();
                        // statusLabel.Text = "Do you want to continue?";
                    }));
                }

                while (isPaymentMode && paybtnClicked == false) // Added paybtnClicked condition
                {
                    await Task.Delay(100);

                    // Check if payment mode is still true
                    isPaymentMode = PaymentStatusTracker.IsAnyFormInPaymentMode(ticketName);
                }
                // Set the payment mode for this form to true
                PaymentStatusTracker.SetPaymentMode(ticketName, true);


                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                string upiid = null;

                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {

                    connection.Open();
                    string query = "SELECT paymentid FROM paymentid1 WHERE nametosave = @nametosave";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@nametosave", selectedItem);
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string paymentid = reader["paymentid"].ToString();
                                upiid = paymentid;
                                //comboBox1.Enabled = false;
                                comboBox1.Invoke((MethodInvoker)(() => comboBox1.Enabled = false));
                                connection.Close();
                            }
                        }
                    }
                    connection.Close();
                }


                var data = new JObject
                {
                    ["head"] = new JObject
                    {
                        ["version"] = "v1",
                        ["requestTimestamp"] = timestamp,
                        ["channelId"] = "WEB",
                        ["txnToken"] = txntkn,
                        ["workFlow"] = "enhancedCashierFlow",
                        ["token"] = txntkn,
                        ["tokenType"] = "TXN_TOKEN"
                    },
                    ["body"] = new JObject
                    {
                        ["paymentMode"] = "UPI",
                        ["payerAccount"] = upiid,
                        ["requestType"] = "NATIVE",
                        ["authMode"] = "3D",
                        ["mid"] = MID,
                        ["orderId"] = oid,
                        ["paymentFlow"] = "NONE",
                        ["selectedPaymentModeId"] = "2",
                        ["riskExtendInfo"] = "userAgent:Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36 Edg/123.0.0.0|timeZone:Asia/Calcutta|operationType:PAYMENT|refererURL:https://securegw.paytm.in/|businessFlow:STANDARD|amount:32.25|merchantType:offus|language:en-US|screenResolution:1536X864|networkType:4g|osType:Windows|osVersion:10|platform:WEB|channelId:WEB|deviceType:Desktop|browserType:Edge|browserVersion:123.0.0.0|"
                    },
                    ["showPostFetchLoader"] = false
                };

                string dataString = JObject.FromObject(data).ToString();
                string actionUrl = $"https://secure.paytmpayments.com/theia/api/v1/processTransaction?mid={MID}&orderId={oid}";

                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(actionUrl),
                    Method = HttpMethod.Post,
                    Content = new StringContent(dataString, Encoding.UTF8, "application/json")
                };

                // Add headers from headers_14
                request.Headers.Add("Method", "POST");
                request.Headers.Add("Host", "secure.paytmpayments.com");
                request.Headers.Add("Connection", "keep-alive");
               // request.Headers.Add("Content-Length", "");
                request.Headers.Add("Sec-Ch-Ua-Platform", "\"Windows\"");
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36 Edg/123.0.0.0");
                request.Headers.Add("sec-ch-ua", "\"Chromium\";v=\"130\", \"Google Chrome\";v=\"130\", \"Not?A_Brand\";v=\"99\"");
                request.Headers.Add("Dnt", "1");
               // request.Headers.Add("content-type", "application/json\r\n");
                request.Headers.Add("Sec-Ch-Ua-Mobile", "?0");
                request.Headers.Add("Accept", "*/*");
                request.Headers.Add("Origin", "https://securegw.paytm.in");
                request.Headers.Add("Sec-Fetch-Site", "same-origin");
                request.Headers.Add("Sec-Fetch-Mode", "cors");
                request.Headers.Add("Sec-Fetch-Dest", "empty");
                request.Headers.Add("Referer", "https://secure.paytmpayments.com/theia/processTransaction?orderid={oid}");
                //  request.Headers.Add("Accept-Encoding", "gzip, deflate, br, zstd");
                request.Headers.Add("Accept-Language", "en-US,en;q=0.9");

                request.Content.Headers.ContentLength = dataString.Length;


                Console.WriteLine("Sending UPI Collect Request to Paytm by giving UPI ID for Payment");
                HttpResponseMessage response = await httpClient.SendAsync(request);
                string responseContent = await response.Content.ReadAsStringAsync();
                JObject responseJson = JObject.Parse(responseContent);
              //  MessageBox.Show(responseContent);


                if (responseJson["body"]?["resultInfo"]?["resultStatus"]?.ToString() == "F")
                {
                    if (statusLabel != null && !statusLabel.IsDisposed)
                    {
                        statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = responseJson["body"]?["resultInfo"]?["resultMsg"]?.ToString()));
                    }

                  // throw new Exception(responseJson["body"]?["resultInfo"]?["resultMsg"]?.ToString());
                }
                else
                {
                 
                    var paycon = responseJson["body"]["content"];
                    string paycon1 = responseJson["body"]["content"].ToString();
                   
                    string transId = paycon["transId"]?.ToString();
                    string cashierRequestId = paycon["cashierRequestId"]?.ToString();
                    string upiStatusUrl = paycon["upiStatusUrl"]?.ToString();

                   //  MessageBox.Show(paycon1);

                    var message = $"Please approve the UPI Collect request from the UPI ID {paycon["MERCHANT_VPA"]} for INR {paycon["TXN_AMOUNT"]} with payment remarks as 'Oid{paycon["ORDER_ID"]}@IRCTCWebUPI' in your {paycon["upiHandleInfo"]["upiAppName"]} app";
                    Console.WriteLine(message);
                    
                    blinkpicturebox.Invoke((MethodInvoker)(() => blinkpicturebox.Visible = true));
                    blinkpicturebox.Invoke((MethodInvoker)(() => blinkpicturebox.BringToFront()));
                   

                    // await Task.Delay(5000);
                    // MessageBox.Show(message);
                    //await Task.Delay(5000);
                    await Task.Delay(GetRandomDelay());
                    await GetUpiTransactionStatus( userHash, uidstatus, accessToken, ticketName, fromStation, toStation, trainno, quotaMapvalue, classMapvalue, formattedDate, username, tid, captchaQuestion, amount1, paytmHtml, scriptData, oid, txntkn, MID, transId, cashierRequestId, upiStatusUrl, paycon1);
                    

                }

                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errorupireq: {ex.Message}");
               // throw;
            }

           // return "UPI Collect Request Sent Successfully";
        }

        public async Task GetUpiTransactionStatus( string userHash, string uidstatus, string accessToken, string ticketName, string fromStation, string toStation, string trainno, string quotaMapvalue, string classMapvalue, string formattedDate, string username, string tid, string captchaQuestion, string amount1, string paytmHtml, string scriptData, string oid, string txntkn, string MID,string transId,string cashierRequestId,string upiStatusUrl,string paycon1)
        {
            try
            {
               

                int maxAttempts = 5000; // Maximum attempts
                int currentAttempt = 0; // Current attempt counter

                while (currentAttempt < maxAttempts)
                {
                    if (!isLoopRunning)
                    {
                        Console.WriteLine("Loop has been stopped.");
                        break; // Break the loop if the flag is false
                    }


                    await Task.Delay(5000);



                    var data = new Dictionary<string, string>
        {
            { "merchantId", MID },
            { "orderId", oid },
            { "transId", transId },
            { "paymentMode", "UPI" },
            { "cashierRequestId", cashierRequestId }
        };

                    var dataString = new FormUrlEncodedContent(data);
                    var actionUrl = upiStatusUrl.ToString();
                   
                   
                    // Set headers for the request
                    var headers = new Dictionary<string, string>
        {
           
                        
                        
            { "Method", "POST" },
            { "Host", "secure.paytmpayments.com" },
            { "Connection", "keep-alive" },
           // { "Content-Length", "" },
            { "Sec-Ch-Ua-Platform", "\"Windows\"" },
            { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36 Edg/123.0.0.0" },
            { "sec-ch-ua", "\"Chromium\";v=\"130\", \"Google Chrome\";v=\"130\", \"Not?A_Brand\";v=\"99\""},
            { "DNT", "1" },
           // { "content-type", "application/x-www-form-urlencoded" },
            { "sec-ch-ua-mobile", "?0" },
            { "Accept", "*/*" },
            { "Origin", "https://secure.paytmpayments.com" },
            {"Sec-Fetch-Site", "same-origin"},
            {"Sec-Fetch-Mode", "cors"},
            {"Sec-Fetch-Dest", "empty"},
            { "Referer", "https://secure.paytmpayments.com/theia/processTransaction?orderid={oid}" },
           // { "Accept-Encoding", "gzip, deflate, br, zstd" },
            { "Accept-Language", "en-US,en;q=0.9" },

        };
                    
                    
                    // Clear previous headers to avoid duplicates
                    httpClient.DefaultRequestHeaders.Clear();

                    // Add headers to httpClient
                    foreach (var header in headers)
                    {
                        // Check if the header is already set
                        if (!httpClient.DefaultRequestHeaders.Contains(header.Key))
                        {
                            httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                        }
                        else
                        {
                            // If the header exists, you can choose to set a new value
                            // In this case, you can simply replace it with a new value
                            httpClient.DefaultRequestHeaders.Remove(header.Key);
                            httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                        }
                    }
                   

                    Console.WriteLine("Fetching any update from UPI App for the Payment Transaction Status");
                    var response = await httpClient.PostAsync(actionUrl, dataString);
                    var responseString = await response.Content.ReadAsStringAsync();
                    var responseData = JObject.Parse(responseString);
                   
                    string pollStatus = responseData["POLL_STATUS"]?.ToString();
                   // MessageBox.Show(pollStatus);
                    if (pollStatus != "POLL_AGAIN")
                    {
                        Console.WriteLine("Received an update about the UPI Collect Transaction");
                        if (statusLabel != null && !statusLabel.IsDisposed)
                        {
                            statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Final Hit PNR...!"));
                        }
                        blinkpicturebox.Invoke((MethodInvoker)(() => blinkpicturebox.Visible = false));
                        finalhitpnrblink.Invoke((MethodInvoker)(() => finalhitpnrblink.Visible = true));
                        finalhitpnrblink.Invoke((MethodInvoker)(() => finalhitpnrblink.BringToFront()));



                        // await Task.Delay(100);
                        // await Task.Delay(GetRandomDelay());
                        await GetCompletedPaymentParams( userHash, uidstatus, accessToken, ticketName, fromStation, toStation, trainno, quotaMapvalue, classMapvalue, formattedDate, username, tid, captchaQuestion, amount1, paytmHtml, scriptData, oid, txntkn, MID, transId, cashierRequestId, upiStatusUrl, paycon1);
                        //return "UPI Transaction Status Fetched Successfully";
                        break;
                    }

                    currentAttempt++; // Increment attempt counter
                    // Adjust delay time as needed
                   
                }
              
               
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errorupista: {ex.Message}");
               // MessageBox.Show($"Error upi status: {ex.Message}");
               
            }
           

            
        }

        public async Task GetCompletedPaymentParams(string userHash, string uidstatus, string accessToken, string ticketName, string fromStation, string toStation, string trainno, string quotaMapvalue, string classMapvalue, string formattedDate, string username, string tid, string captchaQuestion, string amount1, string paytmHtml, string scriptData, string oid, string txntkn, string MID, string transId, string cashierRequestId, string upiStatusUrl,string paycon1)
        {
            try
            {
                string actionUrl = $"https://secure.paytmpayments.com/theia/transactionStatus?MID={MID}&ORDER_ID={oid}";
                var url = new Uri(actionUrl);

                var paycon1Data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(paycon1);



                // Extract necessary values and handle nested objects
                var keyValuePairs = new Dictionary<string, string>
    {
        { "cashierRequestId", paycon1Data["cashierRequestId"]?.ToString() },
        { "paymentMode", paycon1Data["paymentMode"]?.ToString() },
        { "vpaID", paycon1Data["vpaID"]?.ToString() },
        { "isSelfPush", paycon1Data["isSelfPush"]?.ToString() },
        { "upiAccepted", paycon1Data["upiAccepted"]?.ToString() },
        { "upiStatusUrl", paycon1Data["upiStatusUrl"]?.ToString() },
        { "selfPush", paycon1Data["selfPush"]?.ToString() },
        { "merchantId", paycon1Data["merchantId"]?.ToString() },
        { "ORDER_ID", paycon1Data["ORDER_ID"]?.ToString() },
        { "transId", paycon1Data["transId"]?.ToString() },
        { "TXN_AMOUNT", paycon1Data["TXN_AMOUNT"]?.ToString() },
        { "STATUS_INTERVAL", paycon1Data["STATUS_INTERVAL"]?.ToString() },
        { "STATUS_TIMEOUT", paycon1Data["STATUS_TIMEOUT"]?.ToString() },
        { "MERCHANT_VPA", paycon1Data["MERCHANT_VPA"]?.ToString() }
    };

                // Handle nested object (upiHandleInfo)
                if (paycon1Data.ContainsKey("upiHandleInfo") && paycon1Data["upiHandleInfo"] is JsonElement upiHandleInfo)
                {
                    if (upiHandleInfo.TryGetProperty("upiAppName", out JsonElement upiAppName))
                    {
                        keyValuePairs["upiAppName"] = upiAppName.GetString();
                    }
                    if (upiHandleInfo.TryGetProperty("upiImageName", out JsonElement upiImageName))
                    {
                        keyValuePairs["upiImageName"] = upiImageName.GetString();
                    }
                }

                // Convert to FormUrlEncodedContent
                var requestContent = new FormUrlEncodedContent(keyValuePairs);

                // Create the request
                var request = new HttpRequestMessage(HttpMethod.Post, actionUrl)
                {
                    Content = requestContent
                };

                request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                request.Headers.Add("Connection", "keep-alive");
                request.Headers.Add("Host", "secure.paytmpayments.com");
                // request.Headers.Add("Accept-Encoding", "gzip, deflate, br, zstd");
                request.Headers.Add("Accept-Language", "en-US,en;q=0.9");
                request.Headers.Add("Cache-Control", "max-age=0");
                request.Headers.Add("Dnt", "1");
                request.Headers.Add("Origin", "https://secure.paytmpayments.com");
                request.Headers.Add("Referer", $"https://secure.paytmpayments.com/theia/processTransaction?orderid={oid}");
                request.Headers.Add("Sec-Ch-Ua", "Chromium\";v=\"130\", \"Google Chrome\";v=\"130\", \"Not?A_Brand\";v=\"99");
                request.Headers.Add("Sec-Ch-Ua-Mobile", "?0");
                request.Headers.Add("Sec-Ch-Ua-Platform", "\"Windows\"");
                request.Headers.Add("Sec-Fetch-Dest", "document");
                request.Headers.Add("Sec-Fetch-Mode", "navigate");
                request.Headers.Add("Sec-Fetch-Site", "same-origin");
                request.Headers.Add("Upgrade-Insecure-Requests", "1");
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36 Edg/123.0.0.0");

                // Send the request
                try
                {
                    // Send the request
                    HttpResponseMessage response = await httpClient.SendAsync(request);
                    string responseContent = await response.Content.ReadAsStringAsync();

                    // Debug response content
                    Console.WriteLine("Response Content: " + responseContent);

                    // Check if transaction was unsuccessful
                    if (responseContent.Contains("pushAppData=\""))
                    {
                        Console.WriteLine("Trying payment again due to unsuccessful transaction");
                        // if (statusLabel != null && !statusLabel.IsDisposed)
                        {
                            statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Failed transaction Try again"));
                        }
                        finalhitpnrblink.Invoke((MethodInvoker)(() => finalhitpnrblink.Visible = false));

                        // await CallbackToIrctc(newCsrfToken5, userHash, uidstatus, accessToken, ticketName, fromStation, toStation, trainno, quotaMapvalue, classMapvalue, formattedDate, username, tid, captchaQuestion, amount1, paytmHtml, scriptData, oid, txntkn, MID, transId, cashierRequestId, upiStatusUrl, paycon1, responseContent);

                        //
                        //  MessageBox.Show(responseContent);
                        // Retry payment logic here
                        InsertDummyDatafail();
                    }
                    else
                    {
                        Console.WriteLine("Received Final Completed Payment Parameters from Paytm UPI Gateway");
                        //  MessageBox.Show("Received Final Completed Payment Parameters from Paytm UPI Gateway");
                        //   MessageBox.Show(responseContent);

                        //  await Task.Delay(5000);
                        // Process the response if successful
                        await Task.Delay(GetRandomDelay());
                        await CallbackToIrctc( userHash, uidstatus, accessToken, ticketName, fromStation, toStation, trainno, quotaMapvalue, classMapvalue, formattedDate, username, tid, captchaQuestion, amount1, paytmHtml, scriptData, oid, txntkn, MID, transId, cashierRequestId, upiStatusUrl, paycon1, responseContent);


                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Errorcmp: " + ex.Message);
                  //  MessageBox.Show("Errorcmp: " + ex.Message);
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errorcmp: {ex.Message}");
                // MessageBox.Show($"Error upi status: {ex.Message}");

            }

           
        }

        public async Task CallbackToIrctc( string userHash, string uidstatus, string accessToken, string ticketName, string fromStation, string toStation, string trainno, string quotaMapvalue, string classMapvalue, string formattedDate, string username, string tid, string captchaQuestion, string amount1, string paytmHtml, string scriptData, string oid, string txntkn, string MID, string transId, string cashierRequestId, string upiStatusUrl, string paycon1,string responseContent)
        {
           
            try
            {
                // MessageBox.Show(responseContent);
                HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.LoadHtml(responseContent);


                var form = htmlDoc.DocumentNode.SelectSingleNode("//form[@name='TESTFORM']");


                string actionUrl = form.GetAttributeValue("action", string.Empty);

                // Extract input elements and their names/values
                Dictionary<string, string> bodyData = new Dictionary<string, string>();
                foreach (HtmlNode input in form.SelectNodes("//input"))
                {
                    string name = input.GetAttributeValue("name", string.Empty);
                    string value = input.GetAttributeValue("value", string.Empty);
                    if (!string.IsNullOrEmpty(name))
                    {
                        bodyData[name] = value;
                    }
                }

                var data = new FormUrlEncodedContent(bodyData);

              

                // Clear existing headers
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("METHOD", "POST");
                // Set the headers as specified
                httpClient.DefaultRequestHeaders.Add("Host", "www.wps.irctc.co.in");
                httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
              //  httpClient.DefaultRequestHeaders.Add("Content-Length", "570");
                httpClient.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
                httpClient.DefaultRequestHeaders.Add("sec-ch-ua", "\"Chromium\";v=\"130\", \"Google Chrome\";v=\"130\", \"Not?A_Brand\";v=\"99\"");
                httpClient.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
                httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
                httpClient.DefaultRequestHeaders.Add("Origin", "https://secure.paytmpayments.com");
                httpClient.DefaultRequestHeaders.Add("DNT", "1");
                httpClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                //httpClient.DefaultRequestHeaders.Add("Content-Type", "application/x-www-form-urlencoded");
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36");
                httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "cross-site");
                httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
                httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
                httpClient.DefaultRequestHeaders.Add("Referer", "https://secure.paytmpayments.com/");
               // httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br, zstd");
                httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");


                Console.WriteLine("Sending Payment Response Callback Data to IRCTC");
               
                var response = await httpClient.PostAsync(actionUrl, data);
                // string responseData1 = await response.Content.ReadAsStringAsync();
                //MessageBox.Show(responseData1);
                
                if (response.StatusCode == System.Net.HttpStatusCode.Redirect)
                {
                    Console.WriteLine("Received a redirect Response from IRCTC. Going to that redirect URL");

                    var redirectUrl = response.Headers.Location.ToString();

                    httpClient.DefaultRequestHeaders.Clear();

                    // Set the headers as specified
                    httpClient.DefaultRequestHeaders.Add("Host", "www.irctc.co.in");
                    httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
                    httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36");
                    httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                    httpClient.DefaultRequestHeaders.Add("DNT", "1");
                    httpClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua", "\"Chromium\";v=\"130\", \"Google Chrome\";v=\"130\", \"Not?A_Brand\";v=\"99\"");
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
                    httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
                    httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
                    httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
                   // httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br, zstd");
                    httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");


                    response = await httpClient.GetAsync(redirectUrl);
                    string responseData = await response.Content.ReadAsStringAsync();
                   // MessageBox.Show(responseData);
                }
                await Task.Delay(GetRandomDelay());
                await GetBookingDetails( userHash, uidstatus, accessToken, ticketName, fromStation, toStation, trainno, quotaMapvalue, classMapvalue, formattedDate, username, tid, captchaQuestion, amount1, paytmHtml, scriptData, oid, txntkn, MID, transId, cashierRequestId, upiStatusUrl, paycon1, responseContent);

                //return "IRCTC Callback Payment Successful";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"errorcallb: {ex.Message}");
                //return "IRCTC Callback Payment Failed";
            }


        }

      
        public async Task GetBookingDetails( string userHash, string uidstatus, string accessToken, string ticketName, string fromStation, string toStation, string trainno, string quotaMapvalue, string classMapvalue, string formattedDate, string username, string tid, string captchaQuestion, string amount1, string paytmHtml, string scriptData, string oid, string txntkn, string MID, string transId, string cashierRequestId, string upiStatusUrl, string paycon1,string responseContent)
        {
            string actionUrl = $"https://www.wps.irctc.co.in/eticketing/protected/mapps1/bookingData/{tid}";


            int maxRetries = 5;
            int retryCount = 0;
            while (retryCount < maxRetries)
            {
                if (!isLoopRunning)
                {
                    Console.WriteLine("Loop has been stopped.");
                    break; // Break the loop if the flag is false
                }
                // Configure HttpClient to ignore SSL certificate errors (only for development)
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true;
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    // Set headers
                    httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");

                    httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.0");
                    httpClient.DefaultRequestHeaders.Add("Authorization", accessToken);


                    httpClient.DefaultRequestHeaders.Add("Origin", "https://www.irctc.co.in");
                    httpClient.DefaultRequestHeaders.Add("Referer", "https://www.irctc.co.in/");

                    httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36 Edg/123.0.0.0");
                    httpClient.DefaultRequestHeaders.Add("bmirak", "webbm");
                    httpClient.DefaultRequestHeaders.Add("bmiyek", userHash);
                    httpClient.DefaultRequestHeaders.Add("greq", uidstatus);

                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
                    httpClient.DefaultRequestHeaders.Add("spa-csrf-token", $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");

                    try
                    {
                        // Send GET request
                        HttpResponseMessage response = await httpClient.GetAsync(actionUrl);
                        response.EnsureSuccessStatusCode();

                        // Read response content
                        string responseData = await response.Content.ReadAsStringAsync();
                        JObject jsonResponse = JObject.Parse(responseData);


                        // Check if lastTxnStatus is "Booked" (case-insensitive, trimmed)
                        string lastTxnStatus = jsonResponse["userDetail"]?["lastTxnStatus"]?.ToString().Trim();
                        var bookingResponseArray = jsonResponse["bookingResponseDTO"] as JArray;
                        var firstBookingResponse = bookingResponseArray[0] as JObject;


                        if (string.Equals(lastTxnStatus, "Booked", StringComparison.OrdinalIgnoreCase))
                        {
                            // Extract required details
                            string pnrNo = jsonResponse["bookingResponseDTO"]?[0]?["pnrNumber"]?.ToString();
                            string dateOfJourney = jsonResponse["bookingResponseDTO"]?[0]?["journeyDate"]?.ToString();
                            string trainNumber = jsonResponse["bookingResponseDTO"]?[0]?["trainNumber"]?.ToString();
                            string fromStation1 = jsonResponse["bookingResponseDTO"]?[0]?["fromStn"]?.ToString();
                            string toStation1 = jsonResponse["bookingResponseDTO"]?[0]?["destStn"]?.ToString();
                            string bookingDate = jsonResponse["bookingDate"]?.ToString();
                            string bookingTime = "";
                            if (DateTime.TryParse(bookingDate, out DateTime parsedDate))
                            {
                                bookingTime = parsedDate.ToString("HH:mm:ss");
                            }
                            DateTime parsedDate1;
                            if (DateTime.TryParse(dateOfJourney, out parsedDate1))
                            {
                                dateOfJourney = parsedDate1.ToString("dd-MMM-yyyy");  // Format as Day-Month-Year
                            }
                            Invoke(new Action(() =>
                            {

                                // Display details in labels
                                pnrlebel.Text = $"PNR: {pnrNo}";
                                datelbl.Text = $"Date: {dateOfJourney}";
                                trainlebel.Text = $"Train No: {trainNumber}";
                                fromtolebel.Text = $"From|To: {fromStation1}_{toStation1}";
                                timelebel.Text = $"Time: {bookingTime}";
                                idlebel.Text = $"ID: {ticketunsername}";


                            }));

                            finalhitpnrblink.Invoke((MethodInvoker)(() => finalhitpnrblink.Visible = false));
                            successpanel.Invoke((MethodInvoker)(() => successpanel.Visible = true));
                            successpanel.Invoke((MethodInvoker)(() => successpanel.BringToFront()));
                            InsertDummyDatasucess(pnrNo);
                            break;
                        }
                        else if (firstBookingResponse != null && firstBookingResponse.ContainsKey("errorMessage"))
                        {
                            string errorMessage = firstBookingResponse["errorMessage"].ToString();
                            finalhitpnrblink.Invoke((MethodInvoker)(() => finalhitpnrblink.Visible = false));
                            if (statusLabel != null && !statusLabel.IsDisposed)
                            {
                                statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = errorMessage));
                            }

                            InsertDummyDatafail();
                            break;
                        }

                        else
                        {

                            finalhitpnrblink.Invoke((MethodInvoker)(() => finalhitpnrblink.Visible = false));
                            if (statusLabel != null && !statusLabel.IsDisposed)
                            {
                                statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Booking Failed Login Check History.!"));
                            }
                            InsertDummyDatafail();
                            Console.WriteLine("Booking status is not 'Booked'.");
                            // MessageBox.Show(responseData, "Booking Details");
                            break;
                        }


                    }
                    catch (HttpRequestException ex)
                    {

                        if (retryCount >= maxRetries)
                        {
                            finalhitpnrblink.Invoke((MethodInvoker)(() => finalhitpnrblink.Visible = false));
                            if (statusLabel != null && !statusLabel.IsDisposed)
                            {
                                statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Booking Failed Login Check History.!"));
                            }
                            InsertDummyDatafail();
                            break;
                        }



                        Console.WriteLine($"Request error: {ex.Message}", "Error");
                        // MessageBox.Show($"Request error: {ex.Message}", "Error");
                        continue;
                    }
                    catch (Exception ex)
                    {
                        if (retryCount >= maxRetries)
                        {
                            finalhitpnrblink.Invoke((MethodInvoker)(() => finalhitpnrblink.Visible = false));
                            if (statusLabel != null && !statusLabel.IsDisposed)
                            {
                                statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Booking Failed Login Check History.!"));
                            }
                            InsertDummyDatafail();
                            break;
                        }
                        Console.WriteLine($"Request error: {ex.Message}", "Error");
                        // MessageBox.Show($"Unexpected error: {ex.Message}", "Error");
                        continue;
                    }
                }


            }
        }

        private void InsertDummyDatasucess(string pnrNo)
        {
            string remarks = "Booking Success...!";


            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();


                    string insertQuery = @"
            INSERT INTO History(TicketName, ID, From_Station, To_Station, Journey_Date, Bank_Name, PNR, Fare, Date, Remarks)
            VALUES (@TicketName, @ID, @From_Station, @To_Station, @Journey_Date, @Bank_Name, @PNR, @Fare, @Date, @Remarks)";

                    using (SQLiteCommand insertCommand = new SQLiteCommand(insertQuery, connection))
                    {

                        insertCommand.Parameters.AddWithValue("@TicketName", TicketName);
                        insertCommand.Parameters.AddWithValue("@ID", ticketunsername);
                        insertCommand.Parameters.AddWithValue("@From_Station", hisfrom);
                        insertCommand.Parameters.AddWithValue("@To_Station", histo);
                        insertCommand.Parameters.AddWithValue("@Journey_Date", hisjourdate);
                        insertCommand.Parameters.AddWithValue("@Bank_Name", selectedItem);
                        insertCommand.Parameters.AddWithValue("@PNR", pnrNo);
                        insertCommand.Parameters.AddWithValue("@Fare", amount);
                        insertCommand.Parameters.AddWithValue("@Date", DateTime.Now.ToString("dd-MMM-yyyy"));
                        insertCommand.Parameters.AddWithValue("@Remarks", remarks);

                        // Execute Query
                        int rowsAffected = insertCommand.ExecuteNonQuery();


                        if (rowsAffected > 0)
                        {
                            Console.WriteLine("Data inserted successfully!", "Success");
                        }
                    }

                    connection.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}", "Error");
                }
            }
        }
        private void InsertDummyDatafail()
        {
            string remarks = "Booking Failed...!";
            string pnrNo = "0000000000";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();


                    string insertQuery = @"
            INSERT INTO History(TicketName, ID, From_Station, To_Station, Journey_Date, Bank_Name, PNR, Fare, Date, Remarks)
            VALUES (@TicketName, @ID, @From_Station, @To_Station, @Journey_Date, @Bank_Name, @PNR, @Fare, @Date, @Remarks)";

                    using (SQLiteCommand insertCommand = new SQLiteCommand(insertQuery, connection))
                    {

                        insertCommand.Parameters.AddWithValue("@TicketName", TicketName);
                        insertCommand.Parameters.AddWithValue("@ID", ticketunsername);
                        insertCommand.Parameters.AddWithValue("@From_Station", hisfrom);
                        insertCommand.Parameters.AddWithValue("@To_Station", histo);
                        insertCommand.Parameters.AddWithValue("@Journey_Date", hisjourdate);
                        insertCommand.Parameters.AddWithValue("@Bank_Name", selectedItem);
                        insertCommand.Parameters.AddWithValue("@PNR", pnrNo);
                        insertCommand.Parameters.AddWithValue("@Fare", amount);
                        insertCommand.Parameters.AddWithValue("@Date", DateTime.Now.ToString("dd-MMM-yyyy"));
                        insertCommand.Parameters.AddWithValue("@Remarks", remarks);

                        // Execute Query
                        int rowsAffected = insertCommand.ExecuteNonQuery();


                        if (rowsAffected > 0)
                        {
                            Console.WriteLine("Data inserted successfully!", "Success");
                        }
                    }

                    connection.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}", "Error");
                }
            }
        }

        public async Task GetBookingDetails112()
        {
            try
            {


                // Read response content
                //  string responseData = await response.Content.ReadAsStringAsync();


                string responseData = "{\"bookingResponseDTO\":[{\"errorMessage\":\"No seats available : Transaction dropped -\",\"distance\":\"1843\",\"boardingStn\":\"ST\",\"boardingDate\":\"2024-11-23T04:43:00.000\",\"journeyDate\":\"2024-11-23T00:00:00.000\",\"trainOwner\":\"0\",\"reservationCharge\":\"20\",\"superfastCharge\":\"0\",\"fuelAmount\":\"0.0\",\"tatkalFare\":\"200\",\"serviceTax\":\"0.0\",\"cateringCharge\":\"0\",\"totalFare\":\"930\",\"wpServiceCharge\":\"10.0\",\"wpServiceTax\":\"1.8\",\"insuredPsgnCount\":\"1\",\"serverId\":\"DM06AP30MS4\",\"timeStamp\":\"2024-11-22T11:02:01.616\",\"otpAuthenticationFlag\":\"0\",\"reservationId\":\"0\",\"lapNumber\":\"0\",\"numberOfpassenger\":\"1\",\"timeTableFlag\":\"0\",\"pnrNumber\":\"8425934645\",\"departureTime\":\"04:43\",\"arrivalTime\":\"15:30\",\"reasonType\":\"S\",\"reasonIndex\":\"1\",\"informationMessage\":[\"N        S\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],\"destArrvDate\":\"2024-11-24T15:30:00.000\",\"bookingDate\":\"2024-11-22T11:02:00.000\",\"numberOfChilds\":\"0\",\"numberOfAdults\":\"1\",\"trainNumber\":\"19483\",\"fromStn\":\"ST\",\"destStn\":\"MFP\",\"resvnUptoStn\":\"MFP\",\"fromStnName\":\"SURAT\",\"boardingStnName\":\"SURAT\",\"resvnUptoStnName\":\"MUZAFFARPUR JN\",\"journeyClass\":\"SL\",\"journeyQuota\":\"TQ\",\"insuranceCharge\":\"0.45\",\"totalCollectibleAmount\":\"942.25\",\"psgnDtlList\":[{\"passengerSerialNumber\":\"1\",\"passengerName\":\"NIRBHAY B JHA\",\"passengerAge\":\"27\",\"passengerGender\":\"M\",\"passengerBerthChoice\":\"MB\",\"passengerBedrollChoice\":\"false\",\"passengerIcardFlag\":\"false\",\"childBerthFlag\":\"true\",\"passengerNationality\":\"IN\",\"fareChargedPercentage\":\"0.0\",\"validationFlag\":\"N\",\"bookingStatusIndex\":\"1\",\"bookingStatus\":\"CNF\",\"bookingCoachId\":\"S5\",\"bookingBerthNo\":\"2\",\"bookingBerthCode\":\"MB\",\"bookingStatusDetails\":\"CNF/S5/2/MB\",\"currentStatusIndex\":\"1\",\"currentStatus\":\"CNF\",\"currentCoachId\":\"S5\",\"currentBerthNo\":\"2\",\"currentBerthCode\":\"MB\",\"passengerNetFare\":\"930\",\"currentBerthChoice\":\"MB\",\"currentStatusDetails\":\"CNF/S5/2/MB\",\"insuranceIssued\":\"Yes\",\"psgnwlType\":\"0\",\"dropWaitlistFlag\":\"false\"}],\"clientTransactionId\":\"1935259f895\",\"scheduleDepartureFlag\":\"true\",\"scheduleArrivalFlag\":\"true\",\"serviceChargeTotal\":\"11.8\",\"ticketType\":\"E-ticket\",\"bookedQuota\":\"TATKAL\",\"ersGovMsg\":\"false\",\"avlForVikalp\":\"false\",\"ersDisplayMessage\":[\"IR recovers only 57% of cost of travel on an average.\",\"This ticket is booked on a personal user ID. Its sale/purchase is an offence u/s 143 of the Railways Act, 1989.\",\"For enquiry and integrated railway helpline, please dial 139.\"],\"gstCharge\":{\"totalPRSGst\":\"0.0\",\"irctcCgstCharge\":\"0.0\",\"irctcSgstCharge\":\"0.0\",\"irctcIgstCharge\":\"1.8\",\"irctcUgstCharge\":\"0.0\",\"totalIrctcGst\":\"1.8\"},\"gstFlag\":\"false\",\"monthBkgTicket\":\"2\",\"sai\":\"false\",\"journeyLap\":\"0\",\"sectorId\":\"false\",\"canSpouseFlag\":\"false\",\"mahakalFlag\":\"false\",\"tourismUrl\":\"https://www.irctctourism.com/tourism/pkgUser/Irctc/pacPage\",\"rrHbFlag\":\"YY\",\"mealChoiceEnable\":\"false\",\"complaintFlag\":\"0\",\"qrCodeText\":\"PNR No.:8425934645,\\nTXN ID:100005399537551,\\nPassenger Name:NIRBHAY B JHA,\\n\\t\\tGender:M,\\n\\t\\tAge:27,\\n\\t\\tStatus:CNFS5/2MB,\\nQuota:TATKAL (TQ),\\nTrain No.:19483,\\nTrain Name:ADI BJU EXP,\\nScheduled Departure:23-Nov-2024 04:43,\\nDate Of Journey:23-Nov-2024,\\nBoarding Station:SURAT - ST,\\nClass:SLEEPER_CLASS (SL),\\nFrom:SURAT - ST,\\nTo:MUZAFFARPUR JN - MFP,\\nTicket Fare: Rs930.0,\\nIRCTC C Fee: Rs11.8+PG Charges Extra\",\"bankNameDis\":\"Credit cards/Debit cards/Netbanking/UPI (Powered by IRCTC)\",\"bankPaymentMode\":\"Multiple Payment Service (Credit & Debit Cards/ Netbanking /Wallets)\",\"travelnsuranceRefundAmount\":\"0.0\",\"addOnOpted\":\"false\",\"metroServiceOpted\":\"false\",\"eligibleForMetro\":\"false\",\"multiLapFlag\":\"false\",\"mlUserId\":\"0\",\"mlReservationStatus\":\"0\",\"mlTransactionStatus\":\"0\",\"mlJourneyType\":\"0\",\"timeDiff\":\"0\",\"mlTimeDiff\":\"0\",\"totalRefundAmount\":\"0.0\",\"travelProtectOpted\":\"false\",\"dmrcRefundStatusId\":\"0\",\"dmrcRefundAmount\":\"0.0\",\"dmrcCancellationCharge\":\"0.0\",\"dmrcCancellationId\":\"0\",\"dmrcBooking\":\"false\",\"postMealRefundStatusId\":\"0\",\"postMealRefundAmount\":\"0.0\",\"postMealCancellationCharge\":\"0.0\",\"postMealComplaintFlag\":\"0\",\"postMealOpt\":\"false\",\"mealCancellationId\":\"0\"}],\"userDetail\":{\"existenceFlag\":\"false\",\"mpOldStatus\":\"0\",\"spouseOldStatus\":\"0\",\"mpCardExpFlag\":\"false\",\"funcType\":\"0\",\"mpSpouseFlag\":\"0\",\"spouseExistenceFlag\":\"false\",\"mpFileExistFlag\":\"false\",\"updatePhoto\":\"false\",\"cardStatus\":\"1\",\"mob_verify_flag\":\"0\",\"email_verify_flag\":\"0\",\"emailVerifyFlag\":\"0\",\"userName\":\"ZN+imm8glUXHcG+DxKCFng==\",\"userId\":\"100000948186173\",\"firstName\":\"Sneha Mathur\",\"gender\":\"F\",\"dob\":\"EgtGkz7/+YAvZv9yC9uwCw==\",\"email\":\"y09RqQt6U20/XlWJ/dNXVNKwJyfEaRwacv1yhLDEaIc=\",\"countryId\":\"94\",\"mobile\":\"R6BfRHIEYLEZc+xkzjzEyA==\",\"isdCode\":\"91\",\"verified\":\"true\",\"emailVerified\":\"true\",\"mobileVerified\":\"true\",\"userEnableState\":\"1\",\"mobileAppConfigDTO\":{\"formFillCheckStartTime\":\"460\",\"formFillCheckEndTime\":\"720\",\"captchaFillCheckStartTime\":\"465\",\"captchaFillCheckEndTime\":\"720\",\"minmPsgnInputTime\":\"20000#20000#25000#25000#30000#30000\",\"minmCaptchaInputTime\":\"0\",\"minmPaymentTime\":\"0\",\"formFillCheckEnable\":\"1\",\"paymentCompletCheckEnable\":\"1\",\"gstEnable\":\"true\"},\"informationMessage\":[{\"message\":\"IR recovers only 57% of cost of travel on an average.\",\"popup\":\"false\",\"paramName\":\"ERS\"},{\"message\":\"0\",\"popup\":\"false\",\"paramName\":\"GOVT_ADS_ENABLE\"},{\"message\":\"0\",\"popup\":\"false\",\"paramName\":\"CAPTCHA_POC_ENABLE\"}],\"lastTxnId\":\"100005399537551\",\"lastTxnStatus\":\"Booked\",\"lastTxnTimeStamp\":\"2024-11-22T11:01:38.745\",\"showLastTxn\":\"false\",\"rdsFlag\":\"0\",\"aadhaarVerifyFlag\":\"0\",\"eWalletExpireFlag\":\"false\",\"eWalletAadhaarRegisterFlag\":\"true\",\"renewFlag\":\"0\",\"otpLogin\":\"0\",\"rolles\":[\"1\",\"1\",\"0\",\"1\",\"0\",\"1\",\"1\",\"1\",\"1\",\"1\",\"1\",\"1\",\"1\",\"1\",\"0\",\"0\",\"1\",\"1\",\"0\",\"0\",\"1\",\"1\",\"0\",\"1\"],\"timeStamp\":\"2024-11-22T11:02:01.600\",\"kycAddressDisplayStat\":\"false\",\"deactivationReason\":\"0\",\"enable\":\"1\",\"userIdHash\":\"3B14B77784EF3E59AE54FD857AC1ADBB\",\"aadhaarReverifyFlag\":\"false\",\"passwordChangeMandatory\":\"false\",\"masterList\":\"1\",\"fevJourney\":\"true\",\"softBankList\":[{\"bankId\":\"101\",\"bankName\":\"IRCTC SBI Co-Brand Loyalty Credit Card\",\"paymentMode\":\"0\",\"enableFlag\":\"false\",\"displaySection\":\"0\",\"cardInputFlag\":\"0\",\"travelAgentFlag\":\"0\",\"txnPasswordMandatory\":\"false\",\"groupId\":\"0\",\"displaySequence\":\"0\",\"juspayEnableFlag\":\"0\"},{\"bankId\":\"102\",\"bankName\":\"IRCTC BOB Co-Brand Loyalty Credit Card\",\"paymentMode\":\"0\",\"enableFlag\":\"false\",\"displaySection\":\"0\",\"cardInputFlag\":\"0\",\"travelAgentFlag\":\"0\",\"txnPasswordMandatory\":\"false\",\"groupId\":\"0\",\"displaySequence\":\"0\",\"juspayEnableFlag\":\"0\"},{\"bankId\":\"103\",\"bankName\":\"IRCTC HDFC Co-Brand Loyalty Credit Card\",\"paymentMode\":\"0\",\"enableFlag\":\"false\",\"displaySection\":\"0\",\"cardInputFlag\":\"0\",\"travelAgentFlag\":\"0\",\"txnPasswordMandatory\":\"false\",\"groupId\":\"0\",\"displaySequence\":\"0\",\"juspayEnableFlag\":\"0\"},{\"bankId\":\"106\",\"bankName\":\"IRCTC RBL Co-brand Loyalty Credit Card\",\"paymentMode\":\"0\",\"enableFlag\":\"false\",\"displaySection\":\"0\",\"cardInputFlag\":\"0\",\"travelAgentFlag\":\"0\",\"txnPasswordMandatory\":\"false\",\"groupId\":\"0\",\"displaySequence\":\"0\",\"juspayEnableFlag\":\"0\"}],\"kycStatus\":\"0\",\"dobChangeFlag\":\"false\",\"genderChangeFlag\":\"false\",\"nameChangeFlag\":\"false\",\"nameUpdateFlag\":\"false\",\"groupId\":\"0\",\"appModeSuperApp\":\"0\",\"kycDocToken\":\"N#N#N\",\"ewalletValidDocListToken\":\"Y#Y#N\",\"kycValidDocListToken\":\"N#Y#Y\",\"kycMode\":\"3\",\"ersTktSendEmailFlag\":\"1\",\"ersTktDownloadFlag\":\"1\",\"ratingOptions\":[{\"v\":\"1\",\"en\":\"Look and feel\",\"hi\":\"उपयोगकर्ता अंतरपृष्ठ\"},{\"v\":\"2\",\"en\":\"Captcha\",\"hi\":\"कॅप्चा\"},{\"v\":\"3\",\"en\":\"Login\",\"hi\":\"लॉगिन\"},{\"v\":\"4\",\"en\":\"OTP\",\"hi\":\"ओटीपी\"},{\"v\":\"5\",\"en\":\"Payment Issues\",\"hi\":\"भुगतान सम्बंधित समस्याएं\"},{\"v\":\"6\",\"en\":\"Registration\",\"hi\":\"पंजीकरण\"},{\"v\":\"7\",\"en\":\"Others\",\"hi\":\"अन्य\"}]},\"qrCodeText\":\"PNR No.:8425934645,\\nTXN ID:100005399537551,\\nPassenger Name:NIRBHAY B JHA,\\n\\t\\tGender:M,\\n\\t\\tAge:27,\\n\\t\\tStatus:CNFS5/2MB,\\nQuota:TATKAL (TQ),\\nTrain No.:19483,\\nTrain Name:ADI BJU EXP,\\nScheduled Departure:23-Nov-2024 04:43,\\nDate Of Journey:23-Nov-2024,\\nBoarding Station:SURAT - ST,\\nClass:SLEEPER_CLASS (SL),\\nFrom:SURAT - ST,\\nTo:MUZAFFARPUR JN - MFP,\\nTicket Fare: Rs930.0,\\nIRCTC C Fee: Rs11.8+PG Charges Extra\\n\",\"transactionId\":\"100005399537551\",\"bankNameDis\":\"Credit cards/Debit cards/Netbanking/UPI (Powered by IRCTC)\",\"bankPaymentMode\":\"Multiple Payment Service (Credit & Debit Cards/ Netbanking /Wallets)\",\"bookingDate\":\"2024-11-22T11:02:01.618\",\"totalCollectibleAmount\":\"942.25\",\"retryBooking\":\"false\",\"timeStamp\":\"2024-11-22T11:02:01.618\",\"bankErrorFlag\":\"false\"}";

               
               // (bookingResponseDTO":[errorMessage":"Choice of Confirm berths not met -
               // (134972130)',


                JObject jsonResponse = JObject.Parse(responseData);

                var bookingResponseArray = jsonResponse["bookingResponseDTO"] as JArray;
                var firstBookingResponse = bookingResponseArray[0] as JObject;


                if (firstBookingResponse != null && firstBookingResponse.ContainsKey("errorMessage"))
                {
                    string errorMessage = firstBookingResponse["errorMessage"].ToString();
                    finalhitpnrblink.Invoke((MethodInvoker)(() => finalhitpnrblink.Visible = false));
                    if (statusLabel != null && !statusLabel.IsDisposed)
                    {
                        statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = errorMessage));
                    }
                }
               
                else
                {

                    finalhitpnrblink.Invoke((MethodInvoker)(() => finalhitpnrblink.Visible = false));

                    Console.WriteLine("Booking status is not 'Booked'.");
                    MessageBox.Show(responseData, "Booking Details");

                }
                await Task.Delay(200);

            }
            catch (HttpRequestException ex)
            {

                Console.WriteLine($"Request error: {ex.Message}", "Error");
                // MessageBox.Show($"Request error: {ex.Message}", "Error");
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Request error: {ex.Message}", "Error");
                // MessageBox.Show($"Unexpected error: {ex.Message}", "Error");
            }

        }



        public string SolveCaptchaAsync(string base64Image)
        {

            try
            {
                
                // TrueCaptcha API URL
                string API_URL = "https://api.apitruecaptcha.org/one/gettext";

                // Prepare the request data
                var requestData = new
                {
                    userid = "GANESH2993",
                    apikey = "LG9ywXgee7qIuA9TPzgc",
                    data = base64Image,
                    mode = "auto"


                };

                // Serialize the request data
                string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);

                // Create a web request
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(API_URL);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                httpWebRequest.Proxy = null; // Bypasses the system-wide proxy


                // Write the request data to the request stream
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(jsonData);
                }

                // Get the response and read the result
                using (var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    

                    string responseContent = streamReader.ReadToEnd();
                    Console.WriteLine("API Response: " + responseContent); // Debugging statement

                    // Deserialize the JSON response
                    var captchaResult = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(responseContent);

                    // Check if CAPTCHA solving was successful
                    if (captchaResult != null && captchaResult["result"] != null)
                    {
                        return captchaResult["result"].ToString();
                    }
                    else
                    {
                        throw new Exception($"Failed to solve CAPTCHA. Empty result.");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to solve CAPTCHA: {ex.Message}");
            }
        }

        private void DisplayImageInPictureBox(string base64Image)
        {
            var imageBytes = Convert.FromBase64String(base64Image);
            using (var ms = new MemoryStream(imageBytes))
            {

                Invoke(new Action(() =>
                {

                  //  qrpictureBox1.Image = Image.FromStream(ms);
                    qrpictureBox1.Image = System.Drawing.Image.FromStream(ms);
                    qrpictureBox1.BringToFront();
                    qrpictureBox1.Show();
                    // Check if the desired HTML element is visible


                }));

            }
        }
       

       
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // Cancel background operations
          //  cancellationTokenSource.Cancel();

            // Dispose of resources and call DoEvents to complete closure
            this.Dispose();
            Application.DoEvents();
        }
        private  void label1_Click(object sender, EventArgs e)
        {
            // await Task.Run(async () =>
            // {

            isLoopRunning = false;
            this.Close();
            Application.DoEvents(); 
             // });
            ///

        }
        private void InitializeTimer()
        {
            updateTimer = new System.Windows.Forms.Timer();
            updateTimer.Interval = 1000; // 1 second
            updateTimer.Tick += async (sender, e) => await OnTimerTick();
            updateTimer.Start();
        }

        private async Task OnTimerTick()
        {
            await UpdateTimeAsync();
        }

        private async Task UpdateTimeAsync()
        {
            try
            {
                // Fetch current IST time from an API
                DateTime currentTimeIst = DateTime.Now.AddSeconds(10);  

                // Update the label with formatted time in IST
                labelCurrentTime.Text = currentTimeIst.ToString("HH:mm:ss");
            }
            catch (Exception ex)
            {

                Console.WriteLine($"error: {ex.Message}", "Error");
                // MessageBox.Show($"Unexpected error: {ex.Message}", "Error");
            }

            await Task.Delay(10);
        }

        private void button2_Click(object sender, EventArgs e)
        {
           
        }

        private  void button2_Click_1(object sender, EventArgs e)
        {
           
            CreateNewPairForm();


        }
        private  void CreateNewPairForm()
        {
            // Using Invoke to create a new form without async
            this.Invoke((MethodInvoker)delegate
            {
                slotpair newPair = new slotpair();
                newPair.TicketName = this.TicketName;

                EventHandler newFormLoadHandler = null;
                newFormLoadHandler =  (s, args) =>
                {
                    newPair.Load -= newFormLoadHandler;
                    newPair.button2.Visible = false;

                    // Call a separate async method to fetch and show IP
                  //await FetchAndShowCurrentIPAsync();
                };

                newPair.Load += newFormLoadHandler;
                newPair.Show();
               
            });
            
        }


       

        public void LogException(Exception ex)
        {
            // Example implementation: Write exception details to console
            Console.WriteLine($"Exception occurred: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            // Add more sophisticated logging logic as needed (e.g., write to a log file)
        }
       


        public void FillStationDetails1(string ticketName)
        {
            try
            {
                

                // MessageBox.Show(ticketName);
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {

                    connection.Open();
                    string query = "SELECT fromstation, tostation,dateofjourney,quota,trainno,class FROM stationdb2 WHERE ticketName = @ticketName";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ticketName", ticketName);
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string fromStation = reader["fromstation"].ToString();
                               string toStation = reader["tostation"].ToString();
                                string dateofjourney1 = reader["dateofjourney"].ToString();

                                 hisfrom = reader["fromstation"].ToString();
                                 histo = reader["tostation"].ToString();
                                 hisjourdate = reader["dateofjourney"].ToString();
                               
                                string quota = reader["quota"].ToString();
                                string trainno = reader["trainno"].ToString();
                                string GetTrainClass = reader["class"].ToString();
                                 classcheck = reader["class"].ToString();
                                 quotacheck = reader["quota"].ToString();

                                Dictionary<string, string> quotaMap = new Dictionary<string, string>
                        {
                            { "General", "GN" },
                            { "Tatkal", "TQ" },
                            { "Premium Tatkal", "PT" }
                        };
                                string quotaMapvalue = quotaMap.ContainsKey(quota) ? quotaMap[quota] : string.Empty;

                                Dictionary<string, string> classMap = new Dictionary<string, string>
                        {
                             { "Second Sitting (2S)", "2S" },
                             { "Sleeper (SL)", "SL" },
                            { "AC 3 Economy (3E)", "3E" },
                            { "AC 3 Tier (3A)", "3AC" },
                            {"AC 2 Tier (2A)", "2AC" },
                            {"AC First Class (1A)", "1AC" }
                        };
                                string classMapvalue = classMap.ContainsKey(GetTrainClass) ? classMap[GetTrainClass] : string.Empty;

                                //  DateTime dateValue = DateTime.ParseExact(dateofjourney1, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                                //string formattedDate = dateValue.ToString("dd/MM/yyyy");
                                //string formattedDate = dateValue.ToString("dd-MMM-yyyy");//.Replace("-", "/");

                                if (DateTime.TryParse(dateofjourney1, out DateTime parsedDate))
                                {
                                    // Format the parsed date to dd-MMM-yyyy
                                    dateofjourney1 = parsedDate.ToString("dd-MMM-yyyy"); // Example: 16-Nov-2024
                                }
                                
                                Invoke(new Action(() => fromlebel.Text = $"{fromStation}_{toStation}"));
                                Invoke(new Action(() => ticketnamelebel.Text = ticketName));
                                Invoke(new Action(() => datelebel.Text = dateofjourney1));
                                Invoke(new Action(() => quotalebel.Text = $"{quotaMapvalue}:{classMapvalue}"));
                                 Invoke(new Action(() => trainnolebel.Text = trainno));
                                //Invoke(new Action(() => classlebel.Text = classMapvalue));
                            }

                        }
                    }
                    connection.Close();
                }
               
            }
            catch (Exception ex)
            {

                Console.WriteLine($"error: {ex.Message}", "Error");
                // MessageBox.Show($"Unexpected error: {ex.Message}", "Error");
            }
        }

        private async void webloginbtn_Click(object sender, EventArgs e)
        {
            if (usernameComboBox.SelectedItem != null)
            {
                loginbtn.Hide();
                webloginbtn.Hide();
                string selectedUsername = usernameComboBox.SelectedItem.ToString();


                try
                {
                    Invoke(new Action(() =>
                    {
                        availabilitylebel1.Show();
                        availabilitylebel1.BringToFront();
                    }));
                    await Task.Run(async () =>
                    {
                        await AuthenticateAndFill(selectedUsername);
                    });
                }
                finally
                {
                    semaphore.Release(); // Release the semaphore after login attempt is complete
                }
               


            }
            else
            {
                statusLabel.Text = "Please select a username.";
            }
        }
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            TicketFormManager.UnregisterForm(this, TicketName);
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            TicketFormManager.UpdateCheckbox(TicketName, checkBox1.Checked);
           
           

        }
        public void UpdateCheckboxState(bool isChecked)
        {
            checkBox1.CheckedChanged -= checkBox1_CheckedChanged;
            checkBox1.Checked = isChecked;
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;
            isStopped = checkBox1.Checked;
            if (isStopped)
            {
                //MessageBox.Show("Execution stopped.");
                isFilling = true;

            }
            else
            {
                isFilling = false;
            }

        }
        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Panel_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
           // dragCursorPoint = Cursor.Position;
            dragCursorPoint = System.Windows.Forms.Cursor.Position;
            dragFormPoint = this.Location;
           
        }

        private void Panel_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
               // System.Drawing.Point diff = System.Drawing.Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                System.Drawing.Point diff = System.Drawing.Point.Subtract(System.Windows.Forms.Cursor.Position, new System.Drawing.Size(dragCursorPoint)); // Corrected to specify namespace explicitly
                this.Location = System.Drawing.Point.Add(dragFormPoint, new Size(diff));
                lastPosition = new System.Drawing.Point(this.Location.X + offsetX, this.Location.Y );
            }

        }


        private void Panel_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
            // Update lastPosition with offsetX and offsetY
          
        }
        private void InitializeCountdownTimer()
        {
            stopwatch = new Stopwatch();

        }

        private void UpdateCountdownLabel(string text)
        {
            try
            {
                if (statusLabel.InvokeRequired)
                {
                    statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = text));
                }
                else
                {
                    statusLabel.Text = text;
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine($"error: {ex.Message}", "Error");
                // MessageBox.Show($"Unexpected error: {ex.Message}", "Error");
            }



        }
        private void InitializeCountdownTimer1()
        {
            stopwatch1 = new Stopwatch();

        }
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Disposes resources of the form to prevent any background activity
            this.Dispose();
        }

        private async void StartCountdown1()
        {
            stopwatch1.Start();
            int countdownMilliseconds1 = 19000; // Set the initial countdown value to 10 seconds (10000 milliseconds)
            string currentMessage1 = "";

            while (countdownMilliseconds1 >= 0)
            {
                if (stopCountdown1) // Check if countdown should be stopped
                {
                    break;
                }

                TimeSpan timeRemaining = TimeSpan.FromMilliseconds(countdownMilliseconds1);
                string newMessage1 = $"Submitting Passenger: {timeRemaining.Seconds}:{timeRemaining.Milliseconds} ";
                try
                {
                    if (currentMessage1 != newMessage1) // Check if the message has changed
                    {
                        UpdateCountdownLabel1(newMessage1);
                        currentMessage1 = newMessage1;
                    }
                }
                catch (Exception ex)
                {

                    Console.WriteLine($"error: {ex.Message}", "Error");
                    // MessageBox.Show($"Unexpected error: {ex.Message}", "Error");
                }


                await Task.Delay(90); // Wait for 10 milliseconds
                countdownMilliseconds1 -= 100; // Reduce the countdown by 10 milliseconds
                try
                {
                    if (countdownMilliseconds1 <= 0) // Check if countdown has reached zero
                    {
                        UpdateCountdownLabel1("Submitting PG Data");
                        break; // Exit the loop when countdown reaches zero
                    }
                }
                catch (Exception ex)
                {

                    Console.WriteLine($"error: {ex.Message}", "Error");
                    // MessageBox.Show($"Unexpected error: {ex.Message}", "Error");
                }

            }
        }

        private void UpdateCountdownLabel1(string text)
        {
            try
            {
                if (statusLabel != null && !statusLabel.IsDisposed)
                {
                    if (statusLabel.InvokeRequired)
                    {
                        statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = text));
                    }
                    else
                    {
                        statusLabel.Text = text;
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // Log the error or handle accordingly
                Console.WriteLine("The statusLabel control was disposed before updating text.");
            }
        }


        private void paybtn_Click(object sender, EventArgs e)
        {
            Invoke(new Action(() => paybtn.Visible = false));
            Invoke(new Action(() => richTextBox1.Visible = false));

        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            
        }

        private  void webBrowser1_DocumentCompleted_1(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
          


        }

        private void webviewpanel_Paint(object sender, PaintEventArgs e)
        {

        }
        private string DecodeBase64Unicode(string base64EncodedData)
        {
            // Decode the base64 string to a byte array
            byte[] data = Convert.FromBase64String(base64EncodedData);

            // Convert each byte to a percent-encoded hex string
            var percentEncodedString = new StringBuilder();
            foreach (byte b in data)
            {
                percentEncodedString.Append($"%{b:X2}");
            }

            // Decode the percent-encoded string back to Unicode
            return Uri.UnescapeDataString(percentEncodedString.ToString());
        }
        private void panel5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null)
            {
                 selectedItem = comboBox1.SelectedItem.ToString();
                Console.WriteLine("Final Selected Item: " + selectedItem);
            }
        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private   void trainnolebel_Click(object sender, EventArgs e)
        {
            
            //  await GetBookingDetails112();
            // Example base64 image string
            //  successpanel.Invoke((MethodInvoker)(() => successpanel.BringToFront()));
            // successpanel.Invoke((MethodInvoker)(() => successpanel.Visible = true));


        }
    }
}
public class Suggestion
{
    public string text { get; set; }
    public int index { get; set; }
}

public class CaptchaResponse
{
    public string captchaType { get; set; }
    public string captchaQuestion { get; set; }
    public string captchaTime { get; set; }
    public string status { get; set; }
    public string timeStamp { get; set; }
    public string nlpcaptchEnabled { get; set; }
    public string captcha { get; set; }
}
public class UserData
{
    public string userIdHash { get; set; }
    
}


