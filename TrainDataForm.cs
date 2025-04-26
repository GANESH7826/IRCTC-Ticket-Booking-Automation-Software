using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.Threading;
using static IRCTC_APP.TrainDataForm;




namespace IRCTC_APP
{
    public partial class TrainDataForm : Form
    {
        public string From1 { get; set; }
        //string from12 = From1;
        public string To1 { get; set; }
        //public string Date1 { get; set; }
        public DateTime Date1 { get; set; }

        private string uidstatus1;
       // private string accessToken;
        private string userHash1;
        private string newCsrfToken1;
        private string username;
        private string Password;

        string fromStationfill;
        string toStationfill;
        string trainNumberfill;
        string enqClassfill;
        string totalCollectibleAmount;

        private string trainNumber;
        private string selectedQuota;
        private string selectedClass;

        private string captchalink = "https://www.irctc.co.in/eticketing/protected/mapps1/loginCaptcha";

        //private readonly HttpClient httpClient;
        private string firstCsrf = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        private string accessToken1;
        private string captchaAnswer;
        private string userData;

        private readonly CookieContainer cookieContainer = new CookieContainer();
        private readonly HttpClientHandler handler;
        private readonly HttpClient httpClient;

        private bool isLoopRunning = true;
        private string userID = null;
        private string password = null;


        public string FormattedDate1
        {
            get { return Date1.ToString("yyyy/MM/dd").Replace("/", ""); }
            set
            {
                if (DateTime.TryParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                {
                    Date1 = result;
                }
            }
        }


        public TrainDataForm()
        {
            InitializeComponent();
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string dbPath = Path.Combine(baseDir, "sqllitedb.db");
            connectionString = $"Data Source={dbPath};Version=3;";

            if (!File.Exists(dbPath))
            {
                MessageBox.Show("Database file not found: " + dbPath);
            }

            
            var handler = new HttpClientHandler
            {
                UseProxy = true,
                //Proxy = new WebProxy("http://YOUR_VPN_PROXY_IP:PORT"), // Har form ke liye VPN/Proxy ke IP details
                // CookieContainer = new CookieContainer()
                CookieContainer = cookieContainer,
            };

            this.httpClient = new HttpClient(handler); // Form ka dedicated HttpClient
            label1.Click += new EventHandler(label1_Click);
        }
        private readonly string connectionString = "Data Source=|DataDirectory|\\sqllitedb.db;Version=3;";


        public async Task GetFirstUserCredentialsAsync()
        {
            

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string query = "SELECT UserID, Password FROM irctcid LIMIT 1";

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            if (await reader.ReadAsync())
                            {
                                userID = reader["UserID"].ToString();
                                password = reader["Password"].ToString();



                               // MessageBox.Show(userID);
                               // MessageBox.Show(password);

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw; // Re-throw the exception if needed
            }

           // return (userID, password);
        }
        private async void TrainDataForm_Load(object sender, EventArgs e)
    {
            dataGridView1.CellFormatting += DataGridView1_CellFormatting;
            string fromStation = From1;
            string toStation = To1;
            string dateOfJourney = FormattedDate1;
            tatkal.Checked = true;
            pictureBox1.Visible=true;
            await GetFirstUserCredentialsAsync();
            username = userID;
            Password = password;


            try
            {
                await SignIn(username, Password);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error fetching train data: " + ex.Message);
            }
        }
        public async Task<string> SignIn(string username, string Password)
        {
           // captchapanel.Invoke((MethodInvoker)(() => captchapanel.Visible = false));
           // await Task.Delay(GetRandomDelay());
            await ClickingSignButton(username, Password);

            //await GettingToken();
            return "Sign In Successful";
        }

        private async Task ClickingSignButton(string username, string Password)
        {

            int attempts = 0;
            int maxRetries = 500;


            while (attempts < maxRetries)
            {
                attempts++;
                try
                {
                    if (string.IsNullOrWhiteSpace(userID) || string.IsNullOrWhiteSpace(password))
                    {
                        pictureBox1.Visible = false;
                     
                        MessageBox.Show("Add Atleast One IRCTC UserID...! & Password...!", "UserID Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        this.Close();
                        break; // Process ko yahin stop karne ke liye.
                    }


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
                    headers3.Headers.Add("sec-ch-ua", "\"Chromium\";v=\"130\", \"Google Chrome\";v=\"130\", \"Not?A_Brand\";v=\"99\"");
                    headers3.Headers.Add("bmiyek", "");
                    headers3.Headers.Add("sec-ch-ua-mobile", "?0");
                    headers3.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36 Edg/123.0.0.0");
                    headers3.Headers.Add("Accept", "application/json, text/plain, */*");
                    headers3.Headers.Add("DNT", "1");
                    //  headers3.Headers.Add("Content-Language", "en");
                    // headers3.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    headers3.Headers.Add("Accept-Encoding", "deflate, br, zstd");
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

                           // ShowCaptchaImage(captchaImageString);


                            captchaAnswer = SolveCaptchaAsync(captchaImageString);

                          //  captchatxtbox.Invoke((MethodInvoker)(() => captchatxtbox.Text = captchaAnswer));


                           /// if (statusLabel != null && !statusLabel.IsDisposed)
                            ///{
                              //  statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Validating Captcha...!"));
                           // }
                           // await Task.Delay(GetRandomDelay());
                            await SendLogin(uidstatus, username, Password);
                            return; // Exit after successful login
                        }
                        else
                        {
                            Console.WriteLine("Unexpected response format. Expected JSON but received " + contentType);
                            // MessageBox.Show("Unexpected response format. Expected JSON but received " + contentType);
                        }
                    }
                    else
                    {
                       /// if (statusLabel != null && !statusLabel.IsDisposed)
                       // {
                          //  statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Failed captcha data: " + response.StatusCode));
                        //}

                        //MessageBox.Show("Failed to retrieve captcha data: " + response.StatusCode);
                    }
                }
                catch (Exception ex)
                {

                    Console.WriteLine($"Attempt {attempts}: Error occurred - {ex.Message}");
                    //MessageBox.Show($"Attempt {attempts}: Error occurred - {ex.Message}");


                    if (attempts >= maxRetries)
                    {
                        

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
            headers4.Headers.Add("sec-ch-ua", "\"Chromium\";v=\"134\", \"Google Chrome\";v=\"134\", \"Not?A_Brand\";v=\"24\"");
            headers4.Headers.Add("bmiyek", "");
            headers4.Headers.Add("sec-ch-ua-mobile", "?0");
            headers4.Headers.Add("user-agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.0.0 Safari/537.36");
            headers4.Headers.Add("Accept", "application/json, text/plain, */*");
            headers4.Headers.Add("DNT", "1");
            // headers4.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            headers4.Headers.Add("Origin", "https://www.irctc.co.in");
            headers4.Headers.Add("Sec-Fetch-Site", "same-origin");
            headers4.Headers.Add("Sec-Fetch-Mode", "cors");
            headers4.Headers.Add("Sec-Fetch-Dest", "empty");
            headers4.Headers.Add("referer", "https://www.irctc.co.in/nget/train-search");
            headers4.Headers.Add("Accept-Encoding", " deflate, br, zstd");

            // Prepare the form data as StringContent
            var data = new StringContent($"grant_type=password&username={username}&password={base64Password}&captcha={captchaAnswer}&uid={uidstatus}&otpLogin=false&nlpIdentifier=&nlpAnswer=&nlpToken=&lso=&encodedPwd=true");

            // Set content-specific headers
            data.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

            // Assign the content to the HttpRequestMessage
            headers4.Content = data;

            // Send the request
            var response = await httpClient.SendAsync(headers4);
            //captchapanel.Invoke((MethodInvoker)(() => captchapanel.Visible = false));
            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();

               // statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Validating User...!"));
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
                        availibilitylebel.Text = "Bad credentials...Add Valid USER...!";
                        availibilitylebel.ForeColor = Color.Red; // Label text ko red color me set karna.
                        pictureBox1.Visible = false;
                        isLoopRunning = false;
                        break;

                    }
                    else if (act["error"].ToString() == "unauthorized" && act["error_description"].ToString() == "Invalid User")
                    {
                        string errorDescription = act["error_description"].ToString();
                        availibilitylebel.Text = "Invalid User...Add Valid USER...!";
                        availibilitylebel.ForeColor = Color.Red; // Label text ko red color me set karna.
                        pictureBox1.Visible = false;
                        isLoopRunning = false;
                        break;
                    }
                    else if (act["error"].ToString() == "unauthorized")
                    {
                        string errorDescription = act["error_description"].ToString();
                        availibilitylebel.Text = errorDescription;
                        captchalink = "https://www.irctc.co.in/eticketing/protected/mapps1/loginCaptcha?nlpCaptchaException=true";
                        await SignIn(username, Password);

                    }
                }

                if (act.ContainsKey("access_token"))
                {

                    string accessToken = "Bearer " + act["access_token"].ToString();

                    // Safely update the label on the UI thread

                   // await Task.Delay(GetRandomDelay());
                    await ValidateUser(accessToken, uidstatus, username, Password);

                }
                else
                {
                   // if (statusLabel != null && !statusLabel.IsDisposed)
                    //{
                      //  statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "error occurred in the access token "));
                   // }
                    captchalink = "https://www.irctc.co.in/eticketing/protected/mapps1/loginCaptcha?nlpCaptchaException=true";
                    await SignIn(username, Password);
                    // throw new Exception(" error occurred in the token  process");

                }


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

                string csrfToken = response.Headers.GetValues("csrf-token").FirstOrDefault();

                string ticketName = "TicketName";
                ///if (statusLabel != null && !statusLabel.IsDisposed)
               // {
                  //  statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Login Successfully"));
               /// }

               // await Task.Delay(GetRandomDelay());
                await GetTrainsAsync(csrfToken, userHash, uidstatus, accessToken, ticketName, username, Password);

            }
            else
            {
                // Handle unsuccessful response
                // MessageBox.Show($"Failed to validate user: {response.ReasonPhrase}");
                //statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = $"Failed validate user: {response.ReasonPhrase}"));
                await SignIn(username, Password);

            }


        }
        public async Task GetTrainsAsync(string csrfToken, string userHash, string uidstatus, string accessToken, string ticketName, string username, string Password)
        {

            // Fill password field
            string fromStation = From1;
            string toStation = To1;
            string dateofjourney = FormattedDate1.Replace("-", ""); ;
            string selectedQuota = GetSelectedQuota();


            // MessageBox.Show(quotaMapvalue);

            var postData = new
            {
                concessionBooking = false,
                srcStn = fromStation,
                destStn = toStation,
                jrnyClass = "",
                jrnyDate = dateofjourney,
                quotaCode = selectedQuota,
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

                // if (statusLabel != null && !statusLabel.IsDisposed)
                //{
                ///  statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = "Loading Train List...!"));
                // }

                int maxRetryAttempts = 50;
                int retryCount = 0;
                bool tokenReceived = false;
               // string newCsrfToken = null;

                while (retryCount < maxRetryAttempts && !tokenReceived)
                {
                    try
                    {

                        if (!isLoopRunning)
                        {
                            Console.WriteLine("Loop has been stopped.");
                            break; // Break the loop if the flag is false
                        }
                        // Making the POST request
                        // HttpResponseMessage response = await client.SendAsync(request);
                        var response = await httpClient.SendAsync(request);
                        // If token was received, proceed with parsing the response data

                        string responseData = await response.Content.ReadAsStringAsync();
                        JObject data = JObject.Parse(responseData);
                         if (data.ContainsKey("errorMessage") && data["errorMessage"].ToString().Contains("No direct trains available between the inputted stations. Would you like to search the in-direct journey trains? "))
                         {
                              
                            string errorMessage = "No direct trains available between the inputted stations";

                            // Show the error message in a MessageBox with an error icon
                            MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            this.Close();
                            break;
                            // return; // or break, depending on your requirement
                         }
                        else if (data.ContainsKey("errorMessage"))
                        {
                            string errorMessage = data["errorMessage"].ToString();

                            // Show the error message in a MessageBox with an error icon
                            MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            this.Close();
                            // await SignIn(username, Password);
                            break;

                        }
                        else if (data.ContainsKey("error_description"))
                        {
                            string errorMessage = data["error_description"].ToString();

                            // Show the error message in a MessageBox with an error icon
                            MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            this.Close();

                        }

                        else
                        {
                            // Check if the csrf-token header is present
                            if (response.Headers.Contains("csrf-token"))
                            {
                                newCsrfToken1 = response.Headers.GetValues("csrf-token").FirstOrDefault();
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


                           // MessageBox.Show(responseData);

                            string jsonResponse = await response.Content.ReadAsStringAsync();
                            var trainsData = Newtonsoft.Json.JsonConvert.DeserializeObject<TrainDataResponse>(jsonResponse);
                            // var responseString = await response.Content.ReadAsStringAsync();

                            // var availabilityResponse = JsonConvert.DeserializeObject<AvailabilityResponse>(responseString);
                            pictureBox1.Visible = false;
                            foreach (var train in trainsData.TrainBtwnStnsList)
                            {
                                dataGridView1.Rows.Add(
                                    train.TrainNumber,
                                    train.TrainName,
                                    train.FromStnCode,
                                    train.DepartureTime,
                                    train.ToStnCode,                                   
                                    train.ArrivalTime,
                                    train.RunningSun == "Y "? " " : " ",
                                    train.RunningSun == "Y" ? "Y" : "X",
                                    train.RunningMon == "Y" ? "Y" : "X",
                                    train.RunningTue == "Y" ? "Y" : "X",
                                    train.RunningWed == "Y" ? "Y" : "X",
                                    train.RunningThu == "Y" ? "Y" : "X",
                                    train.RunningFri == "Y" ? "Y" : "X",
                                    train.RunningSat == "Y" ? "Y" : "X",
                                     train.RunningSat == "Y" ? " " : " ",
                                     train.AvlClasses.Contains("2S") ? "2S" : "X",
                                    train.AvlClasses.Contains("SL") ? "SL" : "X",
                                    train.AvlClasses.Contains("3E") ? "3E" : "X",
                                    train.AvlClasses.Contains("3A") ? "3A" : "X",
                                    train.AvlClasses.Contains("2A") ? "2A" : "X",
                                    train.AvlClasses.Contains("1A") ? "1A" : "X"
                                );

                            }

                            uidstatus1 = uidstatus;
                            accessToken1 = accessToken;
                            userHash1 = userHash;


                            break;
                        }
                    }    
                    catch (Exception ex)
                    {
                        if (retryCount >= maxRetryAttempts)
                        {
                            Console.WriteLine($"Error gettrain after {maxRetryAttempts} attempts: {ex.Message}");
                            /// if (statusLabel != null && !statusLabel.IsDisposed)
                            // {
                            // statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = $"Errortd: {ex.Message}"));
                            // }
                            await SignIn(username, Password);
                            break;
                        }
                        retryCount++;
                    }
                }
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


        private async void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                DataGridView dgv = sender as DataGridView;
                if (dgv.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
                {
                    string trainNumber = dgv.Rows[e.RowIndex].Cells["TrainNo"].Value.ToString();
                    string tostationrow = dgv.Rows[e.RowIndex].Cells["To"].Value.ToString();
                    string fromstationrow = dgv.Rows[e.RowIndex].Cells["From"].Value.ToString();
                    string selectedQuota = GetSelectedQuota();
                    string selectedClass = dgv.Columns[e.ColumnIndex].HeaderText; // Assuming column header is the class name

                  await FetchClassAvailabilityAsync(trainNumber, selectedQuota, selectedClass, tostationrow, fromstationrow);
                    
                }
            }
        }



        public async Task FetchClassAvailabilityAsync(string trainNumber, string selectedQuota,string selectedClass,string tostationrow,string fromstationrow)
        {
            pictureBox1.Visible = true;
            string fromStation = fromstationrow;
            string toStation = tostationrow;
            string dateofjourney = FormattedDate1.Replace("-", ""); ;
            // string selectedQuota1 = selectedQuota;
            string trainno = trainNumber;
            string classMapvalue = selectedClass;

            Dictionary<string, string> quotaMap = new Dictionary<string, string>
                        {
                            { "General", "GN" },
                            { "Tatkal", "TQ" },
                            { "Premium Tatkal", "PT" },
                            { "Ladies", "LD" }
                        };
            // string quotaMapvalue = quotaMap.ContainsKey(quota) ? quotaMap[quota] : string.Empty;
            // MessageBox.Show(quotaMapvalue);


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
                        paymentFlag = "N",
                        concessionBooking = false,
                        ftBooking = false,
                        loyaltyRedemptionBooking = false,
                        ticketType = "E",
                        quotaCode = selectedQuota,
                        moreThanOneDay = false,
                        trainNumber = trainno,
                        fromStnCode = fromStation,
                        toStnCode = toStation,
                        isLogedinReq = true,
                        journeyDate = dateofjourney,
                        classCode = classMapvalue
                    };

                    string postDataString = JsonConvert.SerializeObject(postData);
                    var content = new StringContent(postDataString, Encoding.UTF8, "application/json");



                    using (var request = new HttpRequestMessage(HttpMethod.Post, $"https://www.irctc.co.in/eticketing/protected/mapps1/avlFarenquiry/{trainno}/{dateofjourney}/{fromStation}/{toStation}/{classMapvalue}/{selectedQuota}/N"))
                    {
                        // Assigning the JSON payload to request
                        request.Content = content;

                        // Adding headers

                        request.Headers.Add("Host", "www.irctc.co.in");
                        request.Headers.Add("Connection", "keep-alive");
                        // request.Headers.Add("Content-Length", "");
                        request.Headers.Add("greq", uidstatus1);
                        request.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
                        request.Headers.Add("Authorization", accessToken1);
                        request.Headers.Add("bmirak", "webbm");
                        request.Headers.Add("spa-csrf-token", newCsrfToken1);
                        request.Headers.Add("Accept-Language", "en-US,en;q=0.0");
                        request.Headers.Add("sec-ch-ua", "\"Chromium\";v=\"130\", \"Google Chrome\";v=\"130\", \"Not?A_Brand\";v=\"99\"");
                        request.Headers.Add("bmiyek", userHash1);
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
                                //  statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = $"Received 502 Bad Gateway, retrying attempt {retryCount}..."));
                                // }
                                continue;
                            }

                             newCsrfToken1 = response.Headers.GetValues("csrf-token").FirstOrDefault();
                            string responseData = await response.Content.ReadAsStringAsync();
                            JObject data = JObject.Parse(responseData);

                            
                            if (data.ContainsKey("errorMessage") && data["errorMessage"].ToString().Contains("Booking Not allowed"))
                            {
                                // Action to take when the error message contains "Booking Not allowed"
                                pictureBox1.Visible = false;
                                availibilitylebel.Text = "Booking Not allowed";

                                Console.WriteLine("Error: Booking Not allowed.");
                                break;
                               // return; // or break, depending on your requirement
                            }
                            else if (data.ContainsKey("errorMessage") && data["errorMessage"].ToString().Contains("Train does not touch this station"))
                            {
                                // Action to take when the error message contains "Booking Not allowed"
                                pictureBox1.Visible = false;
                                availibilitylebel.Text = "Train does not touch this station";

                                Console.WriteLine("Error: Train does not touch this station");
                                break;
                                // return; // or break, depending on your requirement
                            }
                            else if (data.ContainsKey("errorMessage") && data["errorMessage"].ToString().Contains("Station <DNR> not an Intermediate Station of Train"))
                            {
                                // Action to take when the error message contains "Booking Not allowed"
                                pictureBox1.Visible = false;
                                availibilitylebel.Text = "Station <DNR> not Intermediate Station Train";

                                Console.WriteLine("Error: Train does not touch this station");
                                break;
                                // return; // or break, depending on your requirement
                            }
                            else if (data.ContainsKey("errorMessage") && data["errorMessage"].ToString().Contains("This action not allowed as the Date given is Outside Advance Reservation Period"))
                            {
                                // Action to take when the error message contains "Booking Not allowed"
                                pictureBox1.Visible = false;
                                availibilitylebel.Text = "not allowed Date given is Outside";

                                Console.WriteLine("Error: Train does not touch this station");
                                break;
                                // return; // or break, depending on your requirement
                            }
                            else if (data.ContainsKey("errorMessage") && data["errorMessage"].ToString().Contains("Date outside Tatkal ARP"))
                            {
                                // Action to take when the error message contains "Booking Not allowed"
                                pictureBox1.Visible = false;
                                availibilitylebel.Text = "Date outside Tatkal ARP";

                                Console.WriteLine("Error: Train does not touch this station");
                                break;
                                // return; // or break, depending on your requirement
                            }
                            else if (data.ContainsKey("errorMessage"))
                            {
                                // throw new Exception(data["errorMessage"].ToString());
                                //if (statusLabel != null && !statusLabel.IsDisposed)
                                pictureBox1.Visible = false;
                                {
                                    availibilitylebel.Invoke((MethodInvoker)(() => availibilitylebel.Text = data["errorMessage"].ToString()));
                                }
                                break;
                            }
                            else
                            {
                                // MessageBox.Show(responseData);
                                pictureBox1.Visible = false;
                                // Extract totalCollectibleAmount
                                 totalCollectibleAmount = data["totalCollectibleAmount"].ToString();
                                // Extract availablityStatus from avlDayList
                                string availablityStatus = data["avlDayList"][0]["availablityStatus"].ToString();                              
                                 fromStationfill = data["from"].ToString();
                                 toStationfill = data["to"].ToString();
                                 trainNumberfill = data["trainNo"].ToString();
                                 enqClassfill = data["enqClass"].ToString();
                                
                                availibilitylebel.Text = $" {availablityStatus}  ||  {trainNumberfill}  ||  {enqClassfill}";
                                fairlebel.Text = totalCollectibleAmount;


                                break;
                            }
                        }
                        catch (TaskCanceledException ex) when (ex.CancellationToken == CancellationToken.None)
                        {
                            retryCount++;
                            if (retryCount >= maxRetries)
                            {
                                // throw new Exception("Request timed out after multiple attempts.");
                            }
                            // Optionally add delay before retry
                            await Task.Delay(1500);
                            continue;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error ava: {ex.Message}");
                            //MessageBox.Show($"Error ava: {ex.Message}");
                            // if (statusLabel != null && !statusLabel.IsDisposed)
                            //{
                            //  statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = $"Error ava: {ex.Message}"));
                            // }
                            await SignIn(username, Password);
                            continue;
                        }
                    }
                }


            }

        }



        private string GetSelectedQuota()
        {
            if (tatkal.Checked) return "TQ";
            if (premiumtatkal.Checked) return "PT";
            if (general.Checked) return "GN";
            if (ladies.Checked) return "LD";
          
            // Add more quota checks here
            return string.Empty;
        }
       

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            // Column ka header text ko middle center mein set karna
            //dataGridView1.Columns["TrainName"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }
        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Replace "ColumnRunningDay" with the actual name of your columns
            var runningDayColumns = new[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thrusday", "Friday", "Saturday" };

            // Check if the current column is one of the running day columns
            if (runningDayColumns.Contains(dataGridView1.Columns[e.ColumnIndex].Name))
            {
                // Get the cell value
                string cellValue = e.Value?.ToString();

                // Check if the cell value is "Y" or "X" and apply colors accordingly
                if (cellValue == "Y")
                {
                    e.CellStyle.ForeColor = Color.Blue;
                }
                else if (cellValue == "X")
                {
                    e.CellStyle.ForeColor = Color.Red;
                }
                else
                {
                    // Default color if it's neither "Y" nor "X"
                    e.CellStyle.ForeColor = Color.Black;
                }
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            isLoopRunning = false;
            // this.Close();
            this.Dispose();
        }
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Disposes resources of the form to prevent any background activity
            this.Dispose();
        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }
        private string GetSelectedQuota1()
        {
            if (tatkal.Checked) return "Tatkal";
            if (premiumtatkal.Checked) return "Premium Tatkal";
            if (general.Checked) return "General";
            if (ladies.Checked) return "Ladies";

            // Add more quota checks here
            return string.Empty;
        }
       
        private void button1_Click(object sender, EventArgs e)
        {

            string trainNo = trainNumberfill;
            string selectedQuota1 = GetSelectedQuota1();
           
            Dictionary<string, string> classMap = new Dictionary<string, string>
                        {
                            { "2S", "Second Sitting (2S)" },
                            { "SL", "Sleeper (SL)" },
                            { "3E", "AC 3 Economy (3E)" },
                            { "3A", "AC 3 Tier (3A)" },
                            {"2A", "AC 2 Tier (2A)" },
                            {"1A", "AC First Class (1A)" }
                        };
            string classMapvalue = classMap.ContainsKey(enqClassfill) ? classMap[enqClassfill] : string.Empty;
            string selectedClass1 = classMapvalue;

            string message = string.Format(
     "{0,-24} {1}\n" +
     "{2,-27} {3,-27}\n" +
     "{4,-17} {5}\n" +
     "{6,-23} {7}\n" +
     "{8,-25} {9}\n" +
     "{10,-25} {11}\n",
     "From:", fromStationfill,
     "To:", toStationfill,
     "Train Number:", trainNo,
     "Quota:", selectedQuota1,
     "Class:", selectedClass1,
     "Fare:", totalCollectibleAmount
 );

            // Show the message in a message box
            MessageBox.Show(message, "Train Details", MessageBoxButtons.OK, MessageBoxIcon.Information);



            PASSENGER passengerForm = Application.OpenForms.OfType<PASSENGER>().FirstOrDefault();

            if (passengerForm == null)
            {
                // If PassengerForm is not open, create a new instance
                // passengerForm = new passengerForm(trainNo, selectedQuota1, selectedClass1);
                // passengerForm.Show();
            }
            else
            {
                // If PassengerForm is already open, fill the values in the existing instance
                passengerForm.FillValues(trainNo, selectedQuota1, selectedClass1, fromStationfill, toStationfill, totalCollectibleAmount);
                passengerForm.BringToFront(); // Bring the form to front if it's behind other forms
            }

            isLoopRunning = false;
            this.Close();

        }

        private void availibilitylebel_Click(object sender, EventArgs e)
        {
            string trainNo = trainNumberfill;
            string selectedQuota1 = GetSelectedQuota1();

            Dictionary<string, string> classMap = new Dictionary<string, string>
                        {
                            { "2S", "Second Sitting (2S)" },
                            { "SL", "Sleeper (SL)" },
                            { "3E", "AC 3 Economy (3E)" },
                            { "3A", "AC 3 Tier (3A)" },
                            {"2A", "AC 2 Tier (2A)" },
                            {"1A", "AC First Class (1A)" }
                        };
            string classMapvalue = classMap.ContainsKey(enqClassfill) ? classMap[enqClassfill] : string.Empty;
            string selectedClass1 = classMapvalue;

            string message = string.Format(
     "{0,-24} {1}\n" +
     "{2,-27} {3,-27}\n" +
     "{4,-17} {5}\n" +
     "{6,-23} {7}\n" +
     "{8,-25} {9}\n" +
     "{10,-25} {11}\n",
     "From:", fromStationfill,
     "To:", toStationfill,
     "Train Number:", trainNo,
     "Quota:", selectedQuota1,
     "Class:", selectedClass1,
     "Fare:", totalCollectibleAmount
 );

            // Show the message in a message box
            MessageBox.Show(message, "Train Details", MessageBoxButtons.OK, MessageBoxIcon.Information);



            PASSENGER passengerForm = Application.OpenForms.OfType<PASSENGER>().FirstOrDefault();

            if (passengerForm == null)
            {
                // If PassengerForm is not open, create a new instance
                // passengerForm = new passengerForm(trainNo, selectedQuota1, selectedClass1);
                // passengerForm.Show();
            }
            else
            {
                // If PassengerForm is already open, fill the values in the existing instance
                passengerForm.FillValues(trainNo, selectedQuota1, selectedClass1, fromStationfill, toStationfill, totalCollectibleAmount);
                passengerForm.BringToFront(); // Bring the form to front if it's behind other forms
            }

            isLoopRunning = false;
            this.Close();

        }

        private void fairlebel_Click(object sender, EventArgs e)
        {

        }
    }

    public class TrainDataResponse
    {
        public List<Train> TrainBtwnStnsList { get; set; }
    }

    public class Train
    {
        public string TrainNumber { get; set; }
        public string TrainName { get; set; }
        public string FromStnCode { get; set; }
        public string ToStnCode { get; set; }
        public string ArrivalTime { get; set; }
        public string DepartureTime { get; set; }
        public List<string> AvlClasses { get; set; }
        public string RunningMon { get; set; }
        public string RunningTue { get; set; }
        public string RunningWed { get; set; }
        public string RunningThu { get; set; }
        public string RunningFri { get; set; }
        public string RunningSat { get; set; }
        public string RunningSun { get; set; }
    }
}
