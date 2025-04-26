using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Management;
using System.Net.NetworkInformation;
using System.Windows.Forms;
using System.Windows.Media;

namespace IRCTC_APP
{
    public partial class HOME : Form
    {
        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;
       // private static Point lastPosition = new Point(370, 835); // Track the last position for form placement
        public HOME()
        {
            InitializeComponent();
            this.panel1.MouseDown += new MouseEventHandler(Panel_MouseDown);
            this.panel1.MouseMove += new MouseEventHandler(Panel_MouseMove);
            this.panel1.MouseUp += new MouseEventHandler(Panel_MouseUp);
            this.usernamelebel.MouseDown += new MouseEventHandler(Panel_MouseDown);
            this.usernamelebel.MouseMove += new MouseEventHandler(Panel_MouseMove);
            this.usernamelebel.MouseUp += new MouseEventHandler(Panel_MouseUp);
            this.datelebel.MouseDown += new MouseEventHandler(Panel_MouseDown);
            this.datelebel.MouseMove += new MouseEventHandler(Panel_MouseMove);
            this.datelebel.MouseUp += new MouseEventHandler(Panel_MouseUp);
            this.paidlebel.MouseDown += new MouseEventHandler(Panel_MouseDown);
            this.paidlebel.MouseMove += new MouseEventHandler(Panel_MouseMove);
            this.paidlebel.MouseUp += new MouseEventHandler(Panel_MouseUp);
            this.validlebel.MouseDown += new MouseEventHandler(Panel_MouseDown);
            this.validlebel.MouseMove += new MouseEventHandler(Panel_MouseMove);
            this.validlebel.MouseUp += new MouseEventHandler(Panel_MouseUp);

            string usernamelebelid = Properties.Settings.Default.SavedUsername;
            usernamelebel.Text = "Your Id: " + usernamelebelid;
            datelebel.Text = DateTime.Now.ToString("dd-MMM-yyyy");
            // this.StartPosition = FormStartPosition.Manual;
            // this.Location = lastPosition;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Kya aap exit krna chahte hain?", "Confirm Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                try
                {
                    Application.Exit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred: " + ex.Message);
                    Environment.Exit(0);
                }

            }
            
        }

        private void nEWTICKETToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PASSENGER mform = new PASSENGER();
            mform.ShowDialog();

        }

        private void pAYMENTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ADDPayment mform = new ADDPayment();
            mform.ShowDialog();
        }

        private void aDDIRCTCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Addirctcid mform = new Addirctcid();
            mform.ShowDialog();
        }

        private void tICKETDATAToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
        }

        private void autoFillCaptchaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            History mform = new History();
            mform.ShowDialog();
        }

        private void totalTicket1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string savedUsername = Properties.Settings.Default.SavedUsername;
            string macAddress = GetMacAddress();

            //string connectionString = "Server=tcp:gadarprouserlogin.database.windows.net,1433;Initial Catalog=login;Persist Security Info=False;User ID=ganesh2993;Password=Ganesh@2993;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            string connectionString = "Server=tcp:gadarlogin.database.windows.net,1433;Initial Catalog=gadarprologinuserdatabase;Persist Security Info=False;User ID=GANESH2993;Password=Ganesh@2993;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT MACAddress FROM Users WHERE Username = @username ";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", savedUsername);
                        //command.Parameters.AddWithValue("@password", password);

                        SqlDataReader reader = command.ExecuteReader();

                        if (reader.Read())
                        {
                            string dbMacAddress = reader["MACAddress"] as string;

                            if (dbMacAddress == macAddress)
                            {
                                totakticket mform = new totakticket();
                                mform.ShowDialog();
                            }
                            else if (dbMacAddress == null)
                            {
                               // MessageBox.Show("Already registered in another system.");
                              // Environment.Exit(0);
                                Application.Exit();

                            }
                            else
                            {
                                reader.Close();  // Close the reader before executing another command

                                string updateQuery = "UPDATE Users SET Username = @newUsername WHERE Username = @username";
                                using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                                {
                                    string newUsername = savedUsername + "fr@ud";
                                    updateCommand.Parameters.AddWithValue("@newUsername", newUsername);
                                    updateCommand.Parameters.AddWithValue("@username", savedUsername);
                                    updateCommand.ExecuteNonQuery();
                                }
                                //MessageBox.Show("Already registered in another system.");
                                MessageBox.Show("You are attempting fraud. Your username is logged into two systems simultaneously, so your data is being deleted from the database. For more information,please contact your supplier.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                Application.Exit();

                            }
                        }
                        else
                        {

                            Console.WriteLine("Invalid username or password.\n    Contact Your Developer....!", "Error");
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Database error: " + ex.Message);
                MessageBox.Show($"Internet Not Available. Please Connect Internet & Try Again....!",
                               "Macaddress Matching Error",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                MessageBox.Show($"Internet Not Available. Please Connect Internet & Try Again....!",
                               "Macaddress Matching Error",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Error);
            }


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
        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
        private void Panel_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = this.Location;
        }

        private void Panel_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point diff = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(diff));
            }
        }

        private void Panel_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void rESETKEYGADARPROToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string connectionString = "Server=tcp:gadarlogin.database.windows.net,1433;Initial Catalog=gadarprologinuserdatabase;Persist Security Info=False;User ID=GANESH2993;Password=Ganesh@2993;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
           // string username = "GANESH001"; // Assuming you want to clear the MAC for this username
            string username = Properties.Settings.Default.SavedUsername;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string updateQuery = "UPDATE Users SET MACAddress = NULL WHERE Username = @username";
                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("       RESET successfully.\nYou Can Use Another System..!","Reset", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Environment.Exit(0);
                        }
                        else
                        {
                            MessageBox.Show("     RESET Failed....! \n User not found...!","ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    } 
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Database error: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        private void datelebel_Click(object sender, EventArgs e)
        {

        }

        private void autoSubmitCaptchaAfterFillToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void proxyIPSettingToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void HOME_Load(object sender, EventArgs e)
        {
            string connectionString = "Server=tcp:gadarlogin.database.windows.net,1433;Initial Catalog=gadarprologinuserdatabase;Persist Security Info=False;User ID=GANESH2993;Password=Ganesh@2993;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            // string username = "GANESH001"; // Assuming you want to clear the MAC for this username
            string username = Properties.Settings.Default.SavedUsername;

            try
            {
                // SQL Query to fetch validity
                string query = "SELECT Validity FROM Users WHERE Username = @Username";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);

                        // Execute the query and fetch the validity
                        object result = command.ExecuteScalar();

                        if (result != null)
                        {
                            // Set the fetched validity value to the label
                            validlebel.Text = $"Valid: {result} days";
                        }
                        else
                        {
                            validlebel.Text = "User not found!";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
