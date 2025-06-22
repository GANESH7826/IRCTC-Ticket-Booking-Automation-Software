using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.IO;
using System.Net;
using System.Reflection;

namespace IRCTC_APP
{
    public partial class Form1 : Form
    {
      
        private string currentVersion;
        private string latestVersion;
        private readonly string versionUrl = "https://raw.githubusercontent.com/GANESH7826/gadarproinstaller/main/version.txt.txt";
        private readonly string versionFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "version.txt");
        
        public Form1()
        {
            InitializeComponent();
           
            
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            

            // MessageBox.Show(appVersion);
            try
            {
                

                loadingpicturebox.Invoke((MethodInvoker)(() => loadingpicturebox.BringToFront()));
                loadingpicturebox.Invoke((MethodInvoker)(() => loadingpicturebox.Visible = true));

                Properties.Settings.Default.SavedUsername = username_txt.Text;
                Properties.Settings.Default.Save();

                // Perform the database operations on a separate thread
               



               
                string username = username_txt.Text;
                string password = password_txt.Text;
                string macAddress = GetMacAddress();
                string appVersion = GetAppVersion();





                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT MACAddress, LastLoginDate, LoginCount, Status FROM Users WHERE Username = @username AND Password = @password";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", password);

                        using (SqlDataReader reader = command.ExecuteReader()) // Wrap the reader in a using block
                        {
                            if (reader.Read())
                            {
                                string dbMacAddress = reader["MACAddress"] as string;
                                DateTime lastLoginDate = reader["LastLoginDate"] != DBNull.Value ? (DateTime)reader["LastLoginDate"] : DateTime.MinValue;
                                int loginCount = reader["LoginCount"] != DBNull.Value ? (int)reader["LoginCount"] : 0;
                                DateTime currentDate = DateTime.Now.Date; // Get current date (only date part)
                                string userStatus = reader["Status"] != DBNull.Value ? reader["Status"].ToString() : "Enable";


                                if (userStatus == "Disable")
                                {
                                    MessageBox.Show("You are disabled. Please contact the administrator.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    loadingpicturebox.Invoke((MethodInvoker)(() => loadingpicturebox.Visible = false));
                                    return; // Exit the login process
                                }
                                // Logic to handle login count and reset
                                if (lastLoginDate.Date == currentDate) // Same day
                                {
                                    loginCount++; // Increment login count
                                }
                                else // New day, reset login count
                                {
                                    loginCount = 1; // Start count from 1 for the new day
                                }
                                reader.Close();
                                string updateQuery = "UPDATE Users SET currentversion = @currentversion, LastLoginDate = @lastLoginDate, LoginCount = @loginCount WHERE Username = @username AND Password = @password";
                                using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                                {
                                   // updateCommand.Parameters.AddWithValue("@logintime", DateTime.Now);
                                    updateCommand.Parameters.AddWithValue("@currentversion", appVersion);
                                    updateCommand.Parameters.AddWithValue("@lastLoginDate", DateTime.Now);
                                    updateCommand.Parameters.AddWithValue("@loginCount", loginCount);
                                    updateCommand.Parameters.AddWithValue("@username", username);
                                    updateCommand.Parameters.AddWithValue("@password", password);
                                    updateCommand.ExecuteNonQuery();
                                }

                                // Handle version and MAC address logic
                                if (dbMacAddress == null && latestVersion != appVersion)
                                {
                                    await Task.Delay(4000);
                                    loadingpicturebox.Invoke((MethodInvoker)(() => loadingpicturebox.Visible = false));
                                    await Task.Delay(1000);
                                    // Logic for version mismatch
                                    MessageBox.Show(
                                        $"Your current app version is: {appVersion}.\nPlease update to the latest version: {latestVersion}.",
                                        "Version Check",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error
                                    );
                                    Application.Exit();
                                }
                                else if (dbMacAddress == null && latestVersion == appVersion)
                                {
                                    // First-time login, register MAC address
                                    reader.Close();
                                    string registerMacQuery = "UPDATE Users SET MACAddress = @macAddress WHERE Username = @username AND Password = @password";
                                    using (SqlCommand registerMacCommand = new SqlCommand(registerMacQuery, connection))
                                    {
                                        registerMacCommand.Parameters.AddWithValue("@macAddress", macAddress);
                                        registerMacCommand.Parameters.AddWithValue("@username", username);
                                        registerMacCommand.Parameters.AddWithValue("@password", password);
                                        registerMacCommand.ExecuteNonQuery();
                                    }
                                    await Task.Delay(4000);
                                    loadingpicturebox.Invoke((MethodInvoker)(() => loadingpicturebox.Visible = false));
                                    await Task.Delay(1000);
                                    MessageBox.Show("Login successful and MAC address registered.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    this.Hide();
                                    Form2 mform = new Form2();
                                    mform.Show();
                                }
                                else if (dbMacAddress != macAddress && latestVersion != appVersion)
                                {
                                    await Task.Delay(4000);
                                    loadingpicturebox.Invoke((MethodInvoker)(() => loadingpicturebox.Visible = false));
                                    await Task.Delay(1000);
                                    // Handle version or MAC address mismatch
                                    string message = $"Your current app version is: {appVersion}.\nPlease update to the latest version: {latestVersion}.\n\nYour current system UUID No. is: {macAddress}.\nRegistered system UUID No. is: {dbMacAddress}.\n\nUse only the registered system and current version, or contact your supplier.";
                                    MessageBox.Show(message, "Version or System UUID N0 Mismatch", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    Application.Exit();
                                }
                                else if (dbMacAddress == macAddress && latestVersion != appVersion)
                                {
                                    await Task.Delay(4000);
                                    loadingpicturebox.Invoke((MethodInvoker)(() => loadingpicturebox.Visible = false));
                                    await Task.Delay(1000);
                                    // Version mismatch but same MAC address
                                    MessageBox.Show(
                                        $"Your current app version is: {appVersion}.\nPlease update to the latest version: {latestVersion}.",
                                        "Version Check",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error
                                    );
                                    Application.Exit();
                                }
                                else if (dbMacAddress != macAddress && latestVersion == appVersion)
                                {
                                    await Task.Delay(4000);
                                    loadingpicturebox.Invoke((MethodInvoker)(() => loadingpicturebox.Visible = false));
                                    await Task.Delay(1000);
                                    // MAC address mismatch but same version
                                    MessageBox.Show(
                                        $"Your Current System UUID No. is: {macAddress}.\nRegistered System UUID NO is: {dbMacAddress}.\nUse Only Registered System Else Contact Your Supplier....!",
                                        "System UUID N0 Check",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error
                                    );
                                    Application.Exit();
                                }
                                else if (dbMacAddress == macAddress && latestVersion == appVersion)
                                {
                                    await Task.Delay(4000);
                                    loadingpicturebox.Invoke((MethodInvoker)(() => loadingpicturebox.Visible = false));
                                    await Task.Delay(1000);
                                    // Successful login with same MAC address and version
                                    MessageBox.Show("Login successful.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    this.Hide();
                                    Form2 mform = new Form2();
                                    mform.Show();
                                }
                            }
                            else
                            {
                                await Task.Delay(4000);
                                loadingpicturebox.Invoke((MethodInvoker)(() => loadingpicturebox.Visible = false));
                                await Task.Delay(1000);
                                MessageBox.Show("Invalid username or password.Contact Your Supplier....!", "Invalid User", MessageBoxButtons.OK,MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Database error: " + ex.Message);
                MessageBox.Show($"Internet Not Available. Please Connect Internet & Try Again....!",
                                 "Login Check Error",
                                 MessageBoxButtons.OK,
                                 MessageBoxIcon.Error);
                loadingpicturebox.Invoke((MethodInvoker)(() => loadingpicturebox.Visible = false));
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                MessageBox.Show($"Internet Not Available. Please Connect Internet & Try Again....!",
                                "Login Check Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                loadingpicturebox.Invoke((MethodInvoker)(() => loadingpicturebox.Visible = false));
            }



        }

        public static string GetAppVersion()
        {
            // Get the current application version from the Assembly
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return version.ToString(); // returns version in "major.minor.build.revision" format
        }
        private string GetMacAddress()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT UUID FROM Win32_ComputerSystemProduct");
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    string uuid = queryObj["UUID"]?.ToString();
                    if (!string.IsNullOrEmpty(uuid) && uuid != "00000000-0000-0000-0000-000000000000")
                    {
                        return uuid; // Valid UUID found
                    }
                }
                Console.WriteLine("UUID not found or invalid.");
                System.Environment.Exit(1); // Exit application if UUID is not valid
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching hardware profile GUID: " + ex.Message);
                System.Environment.Exit(1); // Exit application on exception
            }
            return "UUID No"; // This line won't be reached
        }
        private async void Form1_Load(object sender, EventArgs e)
        {
           
            
            username_txt.Text = Properties.Settings.Default.SavedUsername;

            if (!File.Exists(versionFilePath))
            {
                File.WriteAllText(versionFilePath, "5.0.1.1");
            }

            await Task.Delay(5000);
            if (await CheckForUpdatesAsync())
            {
                DialogResult result = MessageBox.Show(
                    "Gadar Pro new version is available.\n Do you want to proceed?",
                    "Update Available.......!",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                   loadingpicturebox.Visible = false;
                    groupBox1.BringToFront();
                  
                    await UpdateCurrentVersionFile();
                   
                    await UpdateApplicationAsync();

                    MessageBox.Show("Update completed successfully.");
                   
                   // await UpdateCurrentVersionFile();
                    //Application.Exit(); // Uncomment this if you want to exit the application after the update.
                }
                else
                {

                    loadingpicturebox.Visible = false;
                    // Application.Exit();
                    // MessageBox.Show("Update cancelled.");
                }
            }

        }

        private async Task UpdateCurrentVersionFile()
        {
            LogMessage("Fetching latest version...");
            try
            {
                string versionString = await new HttpClient().GetStringAsync(versionUrl);
                LogMessage($"Fetched version string: {versionString}");

                // Trim the version string and write to file
                versionString = versionString.Trim();
                File.WriteAllText(versionFilePath, versionString);
                LogMessage($"Updated version file to: {versionString}");
            }
            catch (HttpRequestException ex)
            {
                LogMessage($"HTTP request failed: {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                LogMessage($"Request timed out: {ex.Message}");
            }
            catch (Exception ex)
            {
                LogMessage($"Error: {ex.Message}");
            }
        }
        private async Task<bool> CheckForUpdatesAsync()
        {
            try
            {
                string versionUrl = "https://raw.githubusercontent.com/GANESH7826/gadarproinstaller/main/version.txt.txt";
                currentVersion = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "version.txt")).Trim();


                var httpClientHandler = new HttpClientHandler()
                {
                    Proxy = null, // Bypass the proxy
                    UseProxy = false // Ensure the proxy is not used
                };




                using (HttpClient client = new HttpClient(httpClientHandler))
                {

                    latestVersion = await client.GetStringAsync(versionUrl);
                    latestVersion = latestVersion.Trim();

                    if (latestVersion != currentVersion)
                    {
                        return true;
                    }
                }
                loadingpicturebox.Visible = false;
                return false;
            }                     
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                // Show error message with an icon
                MessageBox.Show($"Internet Not Available. Please Connect Internet & Try Again....!",
                                "Update Check Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                Environment.Exit(0);
                return false; // Ensure the method always returns a value
            }

        }

       

        private string GetInstalledProductCode(string applicationName)
        {
            UpdateProgressBar(progressBar2, 50);
            string productCode = string.Empty;
            string query = "SELECT * FROM Win32_Product WHERE Name LIKE '%" + applicationName + "%'";
           
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject mo in searcher.Get())
                {
                    productCode = mo["IdentifyingNumber"].ToString();
                    UpdateProgressBar(progressBar2, 60);
                    break;
                }
            }

            return productCode;
        }
        private void UpdateProgressBar(ProgressBar progressBar, int value)
        {
            if (progressBar.InvokeRequired)
            {
                progressBar.Invoke(new Action(() => progressBar.Value = value));
            }
            else
            {
                progressBar.Value = value;
            }
        }
        private async Task UpdateApplicationAsync()
        {
           
           string installerUrl = "https://raw.githubusercontent.com/GANESH7826/gadarproinstaller/main/Gadarpro.msi";
          


            LogMessage("Downloading installer...");
            UpdateProgressBar(progressBar1, 15);
            UpdateProgressBar(progressBar2, 10);
            UpdateProgressBar(progressBar3, 5);

            string installerPath = Path.Combine(Path.GetTempPath(), "Gadarpro.msi");
            string installationPath = @"C:\GadarPro"; // Path where your software is installed
            string databaseFileName = "sqllitedb.db"; // SQLite database file name
            string databaseBackupPath = Path.Combine(Path.GetTempPath(), databaseFileName);
            string softwareExeName = "IRCTC APP.exe"; // Your software's executable file name



            var httpClientHandler = new HttpClientHandler()
            {
                Proxy = null, // Bypass the proxy
                UseProxy = false // Ensure the proxy is not used
            };




            using (HttpClient client = new HttpClient(httpClientHandler))
            {
                try
                {

                    using (HttpResponseMessage response = await client.GetAsync(installerUrl, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();
                        UpdateProgressBar(progressBar2, 25);
                        UpdateProgressBar(progressBar3, 20);
                        long totalBytes = response.Content.Headers.ContentLength ?? -1L;
                        byte[] buffer = new byte[8192];
                        using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                                      fileStream = new FileStream(installerPath, FileMode.Create, FileAccess.Write, FileShare.None, buffer.Length, true))
                        {
                            long totalBytesRead = 0L;
                            int bytesRead;
                            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                UpdateProgressBar(progressBar2, 35);
                                UpdateProgressBar(progressBar3, 30);
                                await fileStream.WriteAsync(buffer, 0, bytesRead);
                                totalBytesRead += bytesRead;
                                if (totalBytes != -1)
                                {
                                    int progress = (int)((totalBytesRead * 100L) / totalBytes);
                                    UpdateProgressBar(progressBar2, 45);
                                    UpdateProgressBar(progressBar1, progress);
                                }
                            }
                        }
                    }
                    LogMessage("Installer downloaded successfully.");
                    UpdateProgressBar(progressBar1, 100);

                    // Backup database before uninstall
                    string databasePath = Path.Combine(installationPath, databaseFileName);
                    if (File.Exists(databasePath))
                    {
                        File.Copy(databasePath, databaseBackupPath, true);
                        LogMessage("Database backup created.");
                    }



                    LogMessage("Checking for existing installation...");
                    string productCode = GetInstalledProductCode("Gadarpro");
                    if (!string.IsNullOrEmpty(productCode))
                    {
                        LogMessage("Uninstalling existing version...");
                        UpdateProgressBar(progressBar2, 80);
                        UninstallCurrentVersion(productCode);
                        LogMessage("Uninstall process completed.");
                        UpdateProgressBar(progressBar2, 100);
                    }

                    LogMessage("Installing new version...");
                    UpdateProgressBar(progressBar3, 50);
                    Process installProcess = new Process();
                    installProcess.StartInfo.FileName = "msiexec.exe";
                    UpdateProgressBar(progressBar3, 60);
                    installProcess.StartInfo.Arguments = $"/i \"{installerPath}\" /quiet /qn"; // Silent install arguments
                    installProcess.StartInfo.UseShellExecute = false;
                    installProcess.StartInfo.RedirectStandardOutput = true;
                    installProcess.StartInfo.RedirectStandardError = true;
                    UpdateProgressBar(progressBar3, 80);
                    installProcess.Start();
                    installProcess.WaitForExit();
                    LogMessage("Install process completed.");
                    UpdateProgressBar(progressBar3, 100);

                    // Restore database after install
                    string newDatabasePath = Path.Combine(installationPath, databaseFileName);
                    if (File.Exists(databaseBackupPath))
                    {
                        File.Copy(databaseBackupPath, newDatabasePath, true);
                        LogMessage("Database restored.");
                    }
                    // Launch software after installation
                    string softwareExePath = Path.Combine(installationPath, softwareExeName);
                    if (File.Exists(softwareExePath))
                    {
                        LogMessage("Launching application...");
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = softwareExePath,
                            UseShellExecute = true
                        });
                    }
                    else
                    {
                        LogMessage("Failed to launch application: Executable not found.");
                        MessageBox.Show("Installation completed, Please start it manually.");
                    }


                }
                catch (Exception ex)
                {
                    LogMessage($"Error: {ex.Message}");
                    MessageBox.Show($"Update failed: {ex.Message}");
                }
            }
        }

        
        private void UninstallCurrentVersion(string productCode)
        {
            Process uninstallProcess = new Process();
            uninstallProcess.StartInfo.FileName = "msiexec.exe";
            uninstallProcess.StartInfo.Arguments = $"/x {productCode} /quiet /qn";
            uninstallProcess.Start();
            uninstallProcess.WaitForExit();

            LogMessage($"Uninstall process completed with exit code: {uninstallProcess.ExitCode}");
        }
        private void LogMessage(string message)
        {
            string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update_log.txt");
            try
            {
                File.AppendAllText(logFilePath, $"{DateTime.Now}: {message}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to write to log file: {ex.Message}");
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {
           
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void username_txt_TextChanged(object sender, EventArgs e)
        {
            username_txt.Text = username_txt.Text.ToUpper();

            // Cursor ko text ke end pe set karne ke liye
            username_txt.SelectionStart = username_txt.Text.Length;
            username_txt.SelectionLength = 0;
        }

        private void password_txt_TextChanged(object sender, EventArgs e)
        {
            password_txt.Text = password_txt.Text.ToUpper();

            // Cursor ko text ke end pe set karne ke liye
            password_txt.SelectionStart = password_txt.Text.Length;
            password_txt.SelectionLength = 0;
        }

        private void loadingpicturebox_Click(object sender, EventArgs e)
        {

        }
    }
}
