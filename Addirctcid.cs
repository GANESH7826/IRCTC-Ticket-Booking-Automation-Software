using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using System.IO;


namespace IRCTC_APP
{
    public partial class Addirctcid : Form
    {
        public Addirctcid()
        {
            InitializeComponent();
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
           string dbPath = Path.Combine(baseDir, "sqllitedb.db");
           connectionString = $"Data Source={dbPath};Version=3;Journal Mode=WAL;";

            if (!File.Exists(dbPath))
            {
                MessageBox.Show("Database file not found: " + dbPath);
            }


        }
        private readonly string connectionString = "Data Source=|DataDirectory|\\sqllitedb.db;Version=3;";


        private void LoadData()
        {
            string query = "SELECT UserID, Password, TotalPnr, MobileNum FROM irctcid";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                using (SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(query, connection))
                {
                    DataTable dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);
                    dataGridView1.DataSource = dataTable;
                    
                    total_count.Text = " " + dataTable.Rows.Count.ToString();
                }
            }
          
        }
       



        private void label2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string dataToSave = userid_txt.Text;
            string dataToSave1 = password_txt.Text;

            // Check if username or password is blank
            if (string.IsNullOrWhiteSpace(dataToSave) || string.IsNullOrWhiteSpace(dataToSave1))
            {
                MessageBox.Show("Username or password cannot be blank.", "UserID & Pass Blank", MessageBoxButtons.OK, MessageBoxIcon.Error);

                
            }
            else
            {
                // Check if the UserID already exists in the database
                string checkQuery = "SELECT COUNT(*) FROM irctcid WHERE UserID = @UserID";

                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    using (SQLiteCommand checkCommand = new SQLiteCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@UserID", dataToSave);

                        try
                        {
                            connection.Open();
                            int userCount = Convert.ToInt32(checkCommand.ExecuteScalar());

                            if (userCount > 0)
                            {
                                // If user already exists, show message
                                MessageBox.Show("This UserID already exists...!", "UserID already", MessageBoxButtons.OK, MessageBoxIcon.Error);

                            }
                            else
                            {
                                // If user doesn't exist, insert data
                                string insertQuery = "INSERT INTO irctcid (UserID, Password) VALUES (@UserID, @Password)";
                                using (SQLiteCommand insertCommand = new SQLiteCommand(insertQuery, connection))
                                {
                                    insertCommand.Parameters.AddWithValue("@UserID", dataToSave);
                                    insertCommand.Parameters.AddWithValue("@Password", dataToSave1);

                                    int rowsAffected = insertCommand.ExecuteNonQuery();

                                    if (rowsAffected > 0)
                                    {
                                        // Data saved successfully
                                        MessageBox.Show("Data saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                      //  MessageBox.Show("Data saved successfully.");
                                        userid_txt.Text = "";
                                       // password_txt.Text = "";
                                        LoadData();
                                    }
                                    else
                                    {
                                        MessageBox.Show("Failed to save data...!", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);

                                       
                                    }
                                }
                            }
                            connection.Close();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error: " + ex.Message);
                        }
                    }
                }
            }

        }
        //private bool isDeleting = false;
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {


            if (e.ColumnIndex == dataGridView1.Columns["Delete"].Index && e.RowIndex >= 0)
            {
                // Confirm delete action
                DialogResult result = MessageBox.Show("Are you sure you want to delete this record?", "Confirm Delete", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    // Get the UserID of the selected row
                    string userIdToDelete = dataGridView1.Rows[e.RowIndex].Cells["UserID"].Value.ToString();

                    // Delete the record from the database
                    DeleteRecord(userIdToDelete);

                    // Refresh the DataGridView
                    LoadData();
                }
            }

        }
        private void DeleteRecord(string userId)
        {
            string query = "DELETE FROM irctcid WHERE UserID = @UserID";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", userId);

                    try
                    {
                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            //MessageBox.Show("Record deleted successfully.");
                        }
                        else
                        {
                            MessageBox.Show("Failed to delete record.");
                        }
                        connection.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
        }

        private void Addirctcid_Load(object sender, EventArgs e)
        {
           
            dataGridView1.DefaultCellStyle.Font = new Font("Arial", 10);
            
            LoadData();
        }

        private void total_count_Click(object sender, EventArgs e)
        {

        }

        private void userid_txt_TextChanged(object sender, EventArgs e)
        {
           

            string cleanedText = CleanText(userid_txt.Text);
            userid_txt.Text = cleanedText;
            userid_txt.SelectionStart = userid_txt.Text.Length;  // Cursor ko end me rakhein
        }
        private string CleanText(string input)
        {
            // Invisible characters aur extra spaces ko remove karte hain
            input = input.Replace("\r", "");  // Carriage return remove
            input = input.Replace("\n", "");  // Newline remove
            input = string.Join(" ", input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));  // Extra spaces remove
            return input;
        }

        private void password_txt_TextChanged(object sender, EventArgs e)
        {
            string cleanedText = CleanText(password_txt.Text);
            password_txt.Text = cleanedText;
            password_txt.SelectionStart = password_txt.Text.Length;  // Cursor ko end me rakhein
        }
    }
}
