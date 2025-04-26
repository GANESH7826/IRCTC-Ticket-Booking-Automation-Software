using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace IRCTC_APP
{
    public partial class ADDPayment : Form
    {

        public ADDPayment()
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
        private string cellValueToDelete = null;
        private string cellValueToEdit = null;
        private void LoadData()
        {
            string query = "SELECT nametosave FROM paymentid1";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    try
                    {
                        connection.Open();
                        SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        dataAdapter.Fill(dataTable);
                        dataGridView1.DataSource = dataTable;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
        }


        private void label10_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string bankName = bank_namebox.Text.Trim();
            string paymentId = upi_txt.Text.Trim();
            string mobileNo = mobile_txt.Text.Trim();
            string nameToSave = namesave_txt.Text.Trim();

            if (string.IsNullOrEmpty(bankName) || string.IsNullOrEmpty(paymentId) || string.IsNullOrEmpty(mobileNo) || string.IsNullOrEmpty(nameToSave))
            {
                MessageBox.Show("Fields cannot be empty.");
                return;
            }

            string query;

            if (!string.IsNullOrEmpty(cellValueToEdit)) // Check if in edit mode
            {
                query = "UPDATE paymentid1 SET bankname = @bankname, paymentid = @paymentid, mobileno = @mobileno, nametosave = @nametosave WHERE nametosave = @oldvalue";
            }
            else
            {
                query = "INSERT INTO paymentid1 (bankname, paymentid, mobileno, nametosave) VALUES (@bankname, @paymentid, @mobileno, @nametosave)";
            }

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@bankname", bankName);
                    command.Parameters.AddWithValue("@paymentid", paymentId);
                    command.Parameters.AddWithValue("@mobileno", mobileNo);
                    command.Parameters.AddWithValue("@nametosave", nameToSave);

                    if (!string.IsNullOrEmpty(cellValueToEdit)) // If in edit mode
                    {
                        command.Parameters.AddWithValue("@oldvalue", cellValueToEdit);
                    }

                    try
                    {
                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show(!string.IsNullOrEmpty(cellValueToEdit) ? "Data updated successfully." : "Data saved successfully.");

                            // Clear TextBoxes and reset edit mode
                            bank_namebox.Text = "";
                            upi_txt.Text = "";
                            mobile_txt.Text = "";
                            namesave_txt.Text = "";
                            cellValueToEdit = null;

                            LoadData();
                        }
                        else
                        {
                            MessageBox.Show("Failed to save data.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
        }

        private void name_txt_TextChanged(object sender, EventArgs e)
        {

        }
        private void UpdateTicketName()
        {
            string banknamebox = bank_namebox.Text;
            string upiid = upi_txt.Text;

            namesave_txt.Text = banknamebox + "_" + upiid;
        }

        private void upi_txt_TextChanged(object sender, EventArgs e)
        {
            UpdateTicketName();
        }

        private void bank_namebox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateTicketName();
        }

        private void button3_Click(object sender, EventArgs e)
        {

            if (!string.IsNullOrEmpty(cellValueToDelete))
            {
                string query = "DELETE FROM paymentid1 WHERE nametosave = @nametosave";

                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@nametosave", cellValueToDelete);

                        try
                        {
                            connection.Open();
                            int rowsAffected = command.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Bank Data deleted successfully.");
                                cellValueToDelete = null; // Reset the variable
                                LoadData(); // Reload the DataGridView after deleting data
                            }
                            else
                            {
                                MessageBox.Show("Failed to delete data.");
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error: " + ex.Message);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please click a cell to delete its corresponding row.");
            }

        }

        private void ADDPayment_Load(object sender, EventArgs e)
        {
            LoadData();
            //dataGridView1.CellClick += dataGridView1_CellClick;
            //dataGridView.CellClick += new DataGridViewCellEventHandler(dataGridView_CellClick);
            dataGridView1.DefaultCellStyle.Font = new Font("Arial", 10);
            //dataGridView1.RowTemplate.Height = 40;
            //DataGridViewCell cell = dataGridView1.Rows[0].Cells[4]; // Row index 0, Column index 0

            // Set the height and width of the cell
            //cell.Style.Padding = new Padding(10);
            comboBox1.SelectedItem = "IRCTC";
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // Get the value of the cell clicked
                cellValueToDelete = dataGridView1.Rows[e.RowIndex].Cells["nametosave"].Value.ToString();
                cellValueToEdit = dataGridView1.Rows[e.RowIndex].Cells["nametosave"].Value.ToString();
                dataGridView1.DefaultCellStyle.Font = new Font("Arial", 10);
                
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                string selectedNameToSave = dataGridView1.CurrentRow.Cells["nametosave"].Value.ToString();

                // Database se data fetch karna
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    string query = "SELECT bankname, paymentid, mobileno, nametosave FROM paymentid1 WHERE nametosave = @nametosave";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@nametosave", selectedNameToSave);

                        try
                        {
                            connection.Open();
                            using (SQLiteDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    bank_namebox.Text = reader["bankname"].ToString();
                                    upi_txt.Text = reader["paymentid"].ToString();
                                    mobile_txt.Text = reader["mobileno"].ToString();
                                    namesave_txt.Text = reader["nametosave"].ToString();

                                    // Edit mode ke liye unique identifier store karna
                                    cellValueToEdit = reader["nametosave"].ToString();
                                }
                                else
                                {
                                    MessageBox.Show("No data found.");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error: " + ex.Message);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a row to edit.");
            }
        }

    }
}

