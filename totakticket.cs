using System;
using System.IO;
using System.Data;
using System.Data.SQLite;
using System.Drawing;

using System.Windows.Forms;
using System.Collections.Generic;


namespace IRCTC_APP
{
    public partial class totakticket : Form
    {
        
        public totakticket()
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
             string query = "SELECT ticketname, fromstation, tostation,dateofjourney, quota, class,ticketslot FROM stationdb2";

           using (SQLiteConnection connection = new SQLiteConnection(connectionString))
           {
                connection.Open();
                using (SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(query, connection))
                {
                 DataTable dataTable = new DataTable();
                 dataAdapter.Fill(dataTable);
                   
                    dataGridView1.DataSource = dataTable;
                    connection.Close();
                    //total_count.Text = " " + dataTable.Rows.Count.ToString();
                }
               
            }

         }
       

        private void label3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
           
        }

        

        private void totakticket_Load(object sender, EventArgs e)
        {
            dataGridView1.DefaultCellStyle.Font = new Font("Arial", 10);

           LoadData();
           
            
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // SQL Query जो टेबल बनाएगी
                    string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS History (
                        TicketName TEXT,
                        ID TEXT,
                        From_Station TEXT,
                        To_Station TEXT,
                        Journey_Date TEXT,
                        Bank_Name TEXT,
                        PNR TEXT,
                        Fare TEXT,
                        Date TEXT,
                        Remarks TEXT
                    );";

                    // Command को Execute करें
                    using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    Console.WriteLine("Table created successfully!", "Success");
                }
                catch (Exception ex)
                {
                    // एरर दिखाने के लिए
                    Console.WriteLine($"Error: {ex.Message}", "Error");
                }
            }



        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to delete this record?", "Confirm Delete", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                string queryStationDB = "DELETE FROM stationdb2";
                string queryPassengerDB = "DELETE FROM passengerdatab2";

                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    using (SQLiteCommand command = new SQLiteCommand(queryStationDB, connection))
                    {

                        try
                        {
                            connection.Open();
                            int rowsAffected = command.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                // Now delete from the passengerdatab2 table
                                command.CommandText = queryPassengerDB;
                                rowsAffected = command.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("All Record deleted successfully.");
                                    LoadData();
                                }
                                else
                                {
                                    MessageBox.Show("Failed to delete record from passengerdatab2.");
                                }

                            }
                            else
                            {
                                MessageBox.Show("Failed to delete record from stationdb2.");
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

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            LoadData();
            if (e.ColumnIndex == dataGridView1.Columns["deletecol"].Index && e.RowIndex >= 0)
            {
                // Confirm delete action
                DialogResult result = MessageBox.Show("Are you sure you want to delete this record?", "Confirm Delete", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    // Get the UserID of the selected row
                    string ticketnameToDelete = dataGridView1.Rows[e.RowIndex].Cells["ticketnamecol"].Value.ToString();

                    // Delete the record from the database
                    DeleteRecord(ticketnameToDelete);

                    // Refresh the DataGridView
                    LoadData();
                }
            }
            if (e.ColumnIndex == dataGridView1.Columns["editcol"].Index && e.RowIndex >= 0)
            {
                // Retrieve the ticket name from the selected row
                string ticketname = dataGridView1.Rows[e.RowIndex].Cells["ticketnamecol"].Value.ToString();

                // Create and show the PassengerForm
                PASSENGER passengerForm = new PASSENGER();
               
                passengerForm.LoadData(ticketname, true, false);
                this.Hide();
                passengerForm.ShowDialog();
               
                //LoadData();
            }
        }
        private void DeleteRecord(string ticketname)
        {
            string queryStationDB = "DELETE FROM stationdb2 WHERE ticketname = @ticketname";
            string queryPassengerDB = "DELETE FROM passengerdatab2 WHERE ticketname = @ticketname";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                using (SQLiteCommand command = new SQLiteCommand(queryStationDB, connection))
                {
                    command.Parameters.AddWithValue("@ticketname", ticketname);

                    try
                    {
                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // Now delete from the passengerdatab2 table
                            command.CommandText = queryPassengerDB;
                            rowsAffected = command.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Record deleted successfully.");
                            }
                            else
                            {
                                MessageBox.Show("Failed to delete record from passengerdatab2.");
                            }
                           
                        }
                        else
                        {
                            MessageBox.Show("Failed to delete record from stationdb2.");
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

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Check if a valid row and the "Login" button column is clicked
            if (e.RowIndex >= 0 && e.ColumnIndex == dataGridView1.Columns["Login"].Index)
            {
                DataGridViewRow selectedRow = dataGridView1.Rows[e.RowIndex];
                string ticketName = selectedRow.Cells["ticketnamecol"].Value.ToString();

                slotpair form = new slotpair();
                form.TicketName = ticketName; // Ticket name ko slotpair form mein pass karna
                this.Hide();
                form.Show(); // Slotpair form ko open karna
                LoadData();
            }
            if (e.RowIndex >= 0 && e.ColumnIndex == dataGridView1.Columns["opencol"].Index)
            {
                DataGridViewRow selectedRow = dataGridView1.Rows[e.RowIndex];
                string ticketName = selectedRow.Cells["ticketnamecol"].Value.ToString();

                slotpair form = new slotpair();
                form.TicketName = ticketName; // Ticket name ko slotpair form mein pass karna
                this.Hide();
                form.Show(); // Slotpair form ko open karna
                LoadData();
            }
        }

    }
}
