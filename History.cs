using System;
using System.IO;
using System.Data;
using System.Data.SQLite;
using System.Drawing;

using System.Windows.Forms;
using System.Collections.Generic;

namespace IRCTC_APP
{
    public partial class History : Form
    {
        public History()
        {
            InitializeComponent();
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string dbPath = Path.Combine(baseDir, "sqllitedb.db");
            connectionString = $"Data Source={dbPath};Version=3;";

            if (!File.Exists(dbPath))
            {
                MessageBox.Show("Database file not found: " + dbPath);
            }
        }
        private readonly string connectionString = "Data Source=|DataDirectory|\\sqllitedb.db;Version=3;";
        private void label2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void LoadData()
        {
            string query = "SELECT TicketName, ID, From_Station,To_Station, Journey_Date, Bank_Name,PNR,Fare,Date,Remarks FROM History";

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

        private void History_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to Clear this record?", "Confirm Clear", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    try
                    {
                        connection.Open();

                        // DELETE Query
                        string deleteQuery = "DELETE FROM History";

                        using (SQLiteCommand deleteCommand = new SQLiteCommand(deleteQuery, connection))
                        {
                            int rowsAffected = deleteCommand.ExecuteNonQuery(); // Execute the delete command

                            // Check if rows were deleted
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("All data Clear successfully!", "Success");
                                LoadData();
                            }
                            else
                            {
                                MessageBox.Show("No data to delete!", "Info");
                                LoadData();
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



           
        }
    }
}
