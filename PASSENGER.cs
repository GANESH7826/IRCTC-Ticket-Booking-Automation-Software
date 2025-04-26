using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Data.SQLite;
using System.IO;
using System.Data;
using System.Linq;




namespace IRCTC_APP
{
    public partial class PASSENGER : Form
    {
        private List<SQLiteConnection> openConnections = new List<SQLiteConnection>();
        private List<Station> stations;
        private bool isLoading = false;


        public PASSENGER()
        {
            InitializeComponent();
           
            List<string> dataList = RetrieveDataFromDatabase();
            PopulateComboBoxWithData(dataList);
            
            slot_box.SelectedIndex = slot_box.Items.IndexOf("Slot 1");
           
            sex1.SelectedIndex = sex1.Items.IndexOf("Male");
            sex2.SelectedIndex = sex2.Items.IndexOf("Male");
            sex3.SelectedIndex = sex3.Items.IndexOf("Male");
            sex4.SelectedIndex = sex4.Items.IndexOf("Male");

            berth1.SelectedIndex = berth1.Items.IndexOf("No Preference");
            berth2.SelectedIndex = berth2.Items.IndexOf("No Preference");
            berth3.SelectedIndex = berth3.Items.IndexOf("No Preference");
            berth4.SelectedIndex = berth4.Items.IndexOf("No Preference");

            indiancom1.SelectedIndex = sex1.Items.IndexOf("India-IN");
            indiancom2.SelectedIndex = sex2.Items.IndexOf("India-IN");
            indiancom3.SelectedIndex = sex3.Items.IndexOf("India-IN");
            indiancom4.SelectedIndex = sex4.Items.IndexOf("India-IN");


            // Set the minimum date to today
            dateTimePicker1.MinDate = DateTime.Today;

            // Set the default value to tomorrow
            dateTimePicker1.Value = DateTime.Today.AddDays(1);

            // Set the custom date format
            dateTimePicker1.Format = DateTimePickerFormat.Custom;
            dateTimePicker1.CustomFormat = "dd-MMM-yyyy";


            LoadStations();
            from.TextChanged += from_TextChanged;
           
            listboxsugg.Click += listboxsugg_Click;
           
            // from.Leave += (s, e) => HideSuggestions();          
            this.Click += (s, e) =>
            {
                // Check if the click is outside of the suggestion list
                if (!listboxsugg.ClientRectangle.Contains(listboxsugg.PointToClient(MousePosition)) &&
                    !from.ClientRectangle.Contains(from.PointToClient(MousePosition)))
                {
                    HideSuggestions();
                }
            };

            // Only hide suggestions when the focus leaves the textbox, but only if not interacting with the suggestion list
            from.Leave += (s, e) =>
            {
                if (!listboxsugg.Focused) // Only hide if the suggestion list isn't focused
                {
                    HideSuggestions();
                }
            };
            
            listboxsugg1.Click += listboxsugg1_Click;
            to.TextChanged += to_TextChanged;
            this.Click += (s, e) =>
            {
                // Check if the click is outside of the suggestion list
                if (!listboxsugg1.ClientRectangle.Contains(listboxsugg1.PointToClient(MousePosition)) &&
                    !to.ClientRectangle.Contains(to.PointToClient(MousePosition)))
                {
                    HideSuggestions1();
                }
            };

            // Only hide suggestions when the focus leaves the textbox, but only if not interacting with the suggestion list
            to.Leave += (s, e) =>
            {
                if (!listboxsugg1.Focused) // Only hide if the suggestion list isn't focused
                {
                    HideSuggestions1();
                }
            };

            //PopulateComboBoxes();
            from.Multiline = false;
            to.Multiline = false;
            bdgpt.Multiline = false;
            train_no.Multiline = false;
            passenger1.Multiline = false;
            passenger2.Multiline = false;
            passenger3.Multiline = false;
            passenger4.Multiline = false;
            age1.Multiline = false;
            age2.Multiline = false;
            age3.Multiline = false;
            age4.Multiline = false;
            mobile_txt.Multiline = false;
            fair_txt.Multiline = false;
            ticket_name.Multiline = false;

            CreateTables();
            fair_txt.Visible = false;
           
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string dbPath = Path.Combine(baseDir, "sqllitedb.db");
            connectionString = $"Data Source={dbPath};Version=3;Journal Mode=WAL;";
            
            if (!File.Exists(dbPath))
            {
                MessageBox.Show("Database file not found: " + dbPath);
            }

        }
        
        private readonly string connectionString = "Data Source=|DataDirectory|\\sqllitedb.db;Version=3;";

        private void LoadStations()
        {
            // Predefined list of stations
            stations = Station.GetStations();
        }
       
        public void CreateTables()
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

              

               string createPassengerdatab = @"
               CREATE TABLE IF NOT EXISTS passengerdatab2(
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                ticketname TEXT,
                passengername TEXT,
                age INTEGER,
                sex TEXT,
                berth TEXT,
               FOREIGN KEY(ticketname) REFERENCES stationdb2(ticketname)
                );";

                

                using (SQLiteCommand command = new SQLiteCommand(createPassengerdatab, connection))
                {
                   
                    command.ExecuteNonQuery();
                   
                }
                connection.Close();
               
            }
            
        }
        private  void MainForm_Load(object sender, EventArgs e)
        {
           
        }
        public void FillValues(string trainNo, string selectedQuota1, string selectedClass1, string fromStationfill, string toStationfill,string totalCollectibleAmount)
        {

            from.Text = fromStationfill;
            to.Text = toStationfill;
            train_no.Text = trainNo; // Fill train number in TextBox
            fairlebel.Text = totalCollectibleAmount;
            fair_txt.Text = totalCollectibleAmount;
            if (class_box.Items.Contains(selectedClass1))
            {
                class_box.SelectedItem = selectedClass1;
            }
            else
            {
                // Log or show a message indicating that the selected class is not in the ComboBox items
                MessageBox.Show("Selected class not found in ComboBox items: " + selectedClass1);
            }

            // Select quota using RadioButtons based on the selectedQuota value
            if (selectedQuota1 == "Tatkal")
            {
                tatkal.Checked = true; // Assuming radTatkal is the RadioButton for Tatkal quota
            }
            else if (selectedQuota1 == "Premium Tatkal")
            {
                premiumtatkal.Checked = true; // Assuming radPremiumTatkal is the RadioButton for Premium Tatkal quota
            }
            else if (selectedQuota1 == "General")
            {
                general.Checked = true; // Assuming radGeneral is the RadioButton for General quota
            }
            else if (selectedQuota1 == "Ladies")
            {
                ladies.Checked = true; // Assuming radGeneral is the RadioButton for General quota
            }
        }
        public void SetTrainNumber(string trainNumber)
        {
            train_no.Text = trainNumber;
        }
        private void label2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            if (e is KeyPressEventArgs keyPressArgs)
            {
                keyPressArgs.Handled = true;  // Block any keyboard input
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            train_no.Text = train_no.Text.ToUpper();

            // Cursor ko text ke end pe set karne ke liye
            train_no.SelectionStart = train_no.Text.Length;
            train_no.SelectionLength = 0;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void HideSuggestions()
        {
            listboxsugg.Visible = false;
        }
        private void listboxsugg_Click(object sender, EventArgs e)
        {
            if (listboxsugg.SelectedItem != null)
            {
                // Get the selected station
                Station selectedStation = (Station)listboxsugg.SelectedItem;

                // Set only the code of the selected station in the TextBox
                from.Text = selectedStation.Code;
                listboxsugg.Visible = false;
                from.Focus();
            }
        }
        private void from_TextChanged(object sender, EventArgs e)
        {
            if (!isLoading)
            {
                UpdateTicketName();
            }

            from.Text = from.Text.ToUpper();

            // Cursor ko text ke end pe set karne ke liye
            from.SelectionStart = from.Text.Length;
            from.SelectionLength = 0;
           
            
            string searchText = from.Text.ToLower();
            listboxsugg.Items.Clear();

            // Only filter and show suggestions if searchText is not empty
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var filteredStations = stations
               .Where(s => s.Name.ToLower().Contains(searchText) || s.Code.ToLower().Contains(searchText))
               .ToList();

                foreach (var station in filteredStations)
                {
                    listboxsugg.Items.Add(station);
                }

                // Show ListBox only if there are matching items
                listboxsugg.Visible = listboxsugg.Items.Count > 0;
            }
            else
            {
                // Hide ListBox if TextBox is empty
                listboxsugg.Visible = false;
            }
        }
        private  void button1_Click(object sender, EventArgs e)
        {
            string from1 = from.Text.Trim();
            string to1 = to.Text.Trim();
            DateTime selectedDate = dateTimePicker1.Value;
            if (string.IsNullOrEmpty(from1))
            {
                MessageBox.Show("Please Fill From station fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrEmpty(to1))
            {
                MessageBox.Show("Please Fill To station fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            TrainDataForm mform = new TrainDataForm();
            mform.From1 = from1;
            mform.To1 = to1;          
            mform.Date1 = selectedDate;

           

            mform.ShowDialog();
        }
        

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
           
            fair_txt.Text = fair_txt.Text.ToUpper();

            // Cursor ko text ke end pe set karne ke liye
            fair_txt.SelectionStart = fair_txt.Text.Length;
            fair_txt.SelectionLength = 0;
           // this.Hide();
        }

        private void tick_premiumtatkal_CheckedChanged(object sender, EventArgs e)
        {
            
           
        }

        private void label20_Click(object sender, EventArgs e)
        {

        }
        
        private void slot_name_TextChanged(object sender, EventArgs e)
        {
            ticket_name.Text = ticket_name.Text.ToUpper();

            // Cursor ko text ke end pe set karne ke liye
            ticket_name.SelectionStart = ticket_name.Text.Length;
            ticket_name.SelectionLength = 0;
        }
        private void HideSuggestions1()
        {
            listboxsugg1.Visible = false;
        }
        private void listboxsugg1_Click(object sender, EventArgs e)
        {
            if (listboxsugg1.SelectedItem != null)
            {
                // Get the selected station
                Station selectedStation = (Station)listboxsugg1.SelectedItem;

                // Set only the code of the selected station in the TextBox
                to.Text = selectedStation.Code;
                listboxsugg1.Visible = false;
                to.Focus();
            }
        }
        private void to_TextChanged(object sender, EventArgs e)
        {
            if (!isLoading)
            {
                UpdateTicketName();
            }

            to.Text = to.Text.ToUpper();

            // Cursor ko text ke end pe set karne ke liye
            to.SelectionStart = to.Text.Length;
            to.SelectionLength = 0;


            string searchText1 = to.Text.ToLower();
            listboxsugg1.Items.Clear();

            // Only filter and show suggestions if searchText is not empty
            if (!string.IsNullOrWhiteSpace(searchText1))
            {
                var filteredStations1 = stations
               .Where(s => s.Name.ToLower().Contains(searchText1) || s.Code.ToLower().Contains(searchText1))
               .ToList();

                foreach (var station in filteredStations1)
                {
                    listboxsugg1.Items.Add(station);
                }

                // Show ListBox only if there are matching items
                listboxsugg1.Visible = listboxsugg1.Items.Count > 0;
            }
            else
            {
                // Hide ListBox if TextBox is empty
                listboxsugg1.Visible = false;
            }
           // label3.Focus();

        }
        private void UpdateTicketName()
        {
            string fromText = from.Text;
            string toText = to.Text;

            ticket_name.Text = fromText + "_" + toText;
            bdgpt.Text = fromText;
        }

        private void save_btn_Click(object sender, EventArgs e)
        {

            

            string ticketName1 = ticket_name.Text.Trim();
            string passenger11 = passenger1.Text.Trim();
            string trainno1 = train_no.Text.Trim();
            string class1 = class_box.SelectedIndex == -1 ? string.Empty : class_box.SelectedItem.ToString();
            string priorbnk1 = priorbank_box.SelectedIndex == -1 ? string.Empty : priorbank_box.SelectedItem.ToString();

            if (string.IsNullOrEmpty(trainno1))
            {
                MessageBox.Show("Please Fill Trainno station fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrEmpty(class1))
            {
                MessageBox.Show("Please Select Train Class. ", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrEmpty(passenger11))
            {
                MessageBox.Show("Please fill At least One Passenger Detail. ", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrEmpty(priorbnk1))
            {
                MessageBox.Show("Please Select Payment Mode. ", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrEmpty(ticketName1))
            {
                MessageBox.Show("Please enter a Ticket Name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (CheckIfTicketNameExists(ticketName1))
            {
                MessageBox.Show("Ticket Name already exists. Please use a different name.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            else
            {
                if (!ValidateFields())
                {
                    MessageBox.Show("Please fill 1st Passenger. ", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!ValidateFields1())
                {
                    MessageBox.Show("Please fill 2nd Passenger. ", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!ValidateFields2())
                {
                    MessageBox.Show("Please fill 3rd Passenger. ", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!ValidateFields3())
                {
                    MessageBox.Show("Please fill 4th Passenger. ", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }


                string fromStation = from.Text;
                string toStation = to.Text;
               // string dateOfJourney = dateTimePicker1.Value.ToString("yyyy/MM/dd");
                string trainNo = train_no.Text;
                string travelClass = class_box.Text;
                string quota = GetSelectedQuota();
                string ticketName = ticket_name.Text;
                string traintype = traintype_box.Text;
                string mobileno = mobile_txt.Text;
                string ptfair = fair_txt.Text;
                string ticketslot = slot_box.Text;
                string priorbank = priorbank_box.Text;
                string backupbank = backupbank_box.Text;

                DateTime selectedDate = dateTimePicker1.Value;
                string formattedDate = selectedDate.ToString("yyyy-MM-dd");

                string queryStation = "INSERT INTO stationdb2(fromstation, tostation, dateofjourney, trainno, class, quota, ticketname,traintype,mobileno,ptfair,ticketslot,priorbank,backupbank) VALUES (@fromstation, @tostation, @dateofjourney, @trainno, @class, @quota, @ticketname,@traintype,@mobileno,@ptfair,@ticketslot,@priorbank,@backupbank)";
                string queryPassenger = "INSERT INTO passengerdatab2(ticketname, passengername, age, sex, berth) VALUES (@ticketname, @passengername, @age, @sex, @berth)";

                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    using (SQLiteCommand commandStation = new SQLiteCommand(queryStation, connection))
                    {
                        commandStation.Parameters.AddWithValue("@fromstation", fromStation);
                        commandStation.Parameters.AddWithValue("@tostation", toStation);
                        commandStation.Parameters.AddWithValue("@dateofjourney", formattedDate);
                        commandStation.Parameters.AddWithValue("@trainno", trainNo);
                        commandStation.Parameters.AddWithValue("@class", travelClass);
                        commandStation.Parameters.AddWithValue("@quota", quota);
                        commandStation.Parameters.AddWithValue("@ticketname", ticketName);
                        commandStation.Parameters.AddWithValue("@traintype", traintype);
                        commandStation.Parameters.AddWithValue("@mobileno", mobileno);
                        commandStation.Parameters.AddWithValue("@ptfair", ptfair);
                        commandStation.Parameters.AddWithValue("@ticketslot", ticketslot);
                        commandStation.Parameters.AddWithValue("@priorbank", priorbank);
                        commandStation.Parameters.AddWithValue("@backupbank", backupbank);

                        try
                        {
                            int rowsAffected = commandStation.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                // long ticketId = connection.LastInsertRowticketname;

                                List<Passenger> passengers = GetPassengerDetails();

                                foreach (var passenger in passengers)
                                {
                                    using (SQLiteCommand commandPassenger = new SQLiteCommand(queryPassenger, connection))
                                    {
                                        commandPassenger.Parameters.AddWithValue("@ticketname", ticketName);
                                        commandPassenger.Parameters.AddWithValue("@passengername", passenger.Name);
                                        commandPassenger.Parameters.AddWithValue("@age", passenger.Age);
                                        commandPassenger.Parameters.AddWithValue("@sex", passenger.Sex);
                                        commandPassenger.Parameters.AddWithValue("@berth", passenger.Berth);
                                        
                                        commandPassenger.ExecuteNonQuery();
                                    }
                                    
                                }
                               
                                MessageBox.Show("New Ticket Data saved successfully.");
                                this.Close();
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
                        finally
                        {
                            connection.Close();
                        }

                    }
                   
                }
                
            }


           
            
        }

        private List<Passenger> GetPassengerDetails()
        {
            List<Passenger> passengers = new List<Passenger>();

            if (!string.IsNullOrEmpty(passenger1.Text) && !string.IsNullOrEmpty(age1.Text) && !string.IsNullOrEmpty(sex1.Text) && !string.IsNullOrEmpty(berth1.Text))
            {
                passengers.Add(new Passenger { Name = passenger1.Text, Age = int.Parse(age1.Text), Sex = sex1.Text, Berth = berth1.Text });
            }
            if (!string.IsNullOrEmpty(passenger2.Text) && !string.IsNullOrEmpty(age2.Text) && !string.IsNullOrEmpty(sex2.Text) && !string.IsNullOrEmpty(berth2.Text))
            {
                passengers.Add(new Passenger { Name = passenger2.Text, Age = int.Parse(age2.Text), Sex = sex2.Text, Berth = berth2.Text });
            }
            if (!string.IsNullOrEmpty(passenger3.Text) && !string.IsNullOrEmpty(age3.Text) && !string.IsNullOrEmpty(sex3.Text) && !string.IsNullOrEmpty(berth3.Text))
            {
                passengers.Add(new Passenger { Name = passenger3.Text, Age = int.Parse(age3.Text), Sex = sex3.Text, Berth = berth3.Text });
            }
            if (!string.IsNullOrEmpty(passenger4.Text) && !string.IsNullOrEmpty(age4.Text) && !string.IsNullOrEmpty(sex4.Text) && !string.IsNullOrEmpty(berth4.Text))
            {
                passengers.Add(new Passenger { Name = passenger4.Text, Age = int.Parse(age4.Text), Sex = sex4.Text, Berth = berth4.Text });
            }

            return passengers;
        }
        class Passenger
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public string Sex { get; set; }
            public string Berth { get; set; }
        }
        private bool CheckIfTicketNameExists(string ticketName)
        {
            bool exists = false;

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT COUNT(1) FROM stationdb2 WHERE ticketname = @ticketname";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ticketname", ticketName);
                    exists = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
                conn.Close();
            }

            return exists;
        }
        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void PASSENGER_Load(object sender, EventArgs e)
        {

           
           

          //  slot_box.SelectedIndex = slot_box.Items.IndexOf("Slot 1");
            gateway_box.SelectedIndex = gateway_box.Items.IndexOf("Pay through BHIM/UPI");

            traintype_box.SelectedIndex = traintype_box.Items.IndexOf("OTHER");
            

           // List<string> dataList = RetrieveDataFromDatabase();
           // PopulateComboBoxWithData(dataList);
            fairlebel.Visible = false;
            //Invoke(new Action(() => updatebtn.Visible = false));
           
            if (!string.IsNullOrWhiteSpace(from.Text))
            {
                // Hide ListBox if TextBox has data
                listboxsugg.Visible = false;
            }
            if (!string.IsNullOrWhiteSpace(to.Text))
            {
                // Hide ListBox if TextBox has data
                listboxsugg1.Visible = false;
            }
        }

        private void tick_general_CheckedChanged(object sender, EventArgs e)
        {
            
        }

        private void tick_ladies_CheckedChanged(object sender, EventArgs e)
        {
            
        }

        private void tick_tatkal_CheckedChanged(object sender, EventArgs e)
        {
            
        }
        private string GetSelectedQuota()
        {
            if (general.Checked)
            {
                return general.Text;
            }
            if (ladies.Checked)
            {
                return ladies.Text;
            }
            if (tatkal.Checked)
            {
                return tatkal.Text;
            }
            if (premiumtatkal.Checked)
            {
                return premiumtatkal.Text;
            }

            return null;
        }
       

        private void premiumtatkal_CheckedChanged(object sender, EventArgs e)
        {
            if (premiumtatkal.Checked)
            {
                // If checked, show the text box
                fair_txt.Visible = true;
            }
            else
            {
                // If unchecked, hide the text box
                fair_txt.Visible = false;
            }
        }

        private void priorbank_box_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void backupbank_box_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private List<string> RetrieveDataFromDatabase()
        {
            List<string> dataList = new List<string>();

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                string query = "SELECT nametosave FROM paymentid1";

                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    try
                    {
                        connection.Open();
                        SQLiteDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            string data = reader["nametosave"].ToString();
                            dataList.Add(data);
                        }

                        reader.Close();
                        connection.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
                
            }

            return dataList;
        }

        private void PopulateComboBoxWithData(List<string> dataList)
        {
            //priorbank_box.Items.Clear();
            //backupbank_box.Items.Clear();

            foreach (string data in dataList)
            {
                priorbank_box.Items.Add(data);
                backupbank_box.Items.Add(data);
            }
        }

        private void bdgpt_TextChanged(object sender, EventArgs e)
        {
            bdgpt.Text = bdgpt.Text.ToUpper();

            // Cursor ko text ke end pe set karne ke liye
            bdgpt.SelectionStart = bdgpt.Text.Length;
            bdgpt.SelectionLength = 0;
        }

        private void passenger1_TextChanged(object sender, EventArgs e)
        {
            //UpdateTicketName();
           
            
            passenger1.Text = passenger1.Text.ToUpper();

            // Cursor ko text ke end pe set karne ke liye
            passenger1.SelectionStart = passenger1.Text.Length;
            passenger1.SelectionLength = 0;

            bool isNameFilled = !string.IsNullOrEmpty(passenger1.Text.Trim());

            age1.Enabled = isNameFilled;
            sex1.Enabled = isNameFilled;
            berth1.Enabled = isNameFilled;

            // Clear other fields if txtName is empty
            if (!isNameFilled)
            {
                age1.Text = string.Empty;
                sex1.SelectedIndex = -1;
                berth1.SelectedIndex = -1;
            }
        }
        private bool ValidateFields()
        {
            // If txtName is filled, other fields must be filled
            if (!string.IsNullOrEmpty(passenger1.Text.Trim()))
            {
                if (string.IsNullOrEmpty(age1.Text.Trim()) || sex1.SelectedIndex == -1 || berth1.SelectedIndex == -1)
                {
                    return false;
                }
            }

            return true;
        }
        private void passenger2_TextChanged(object sender, EventArgs e)
        {
            passenger2.Text = passenger2.Text.ToUpper();

            // Cursor ko text ke end pe set karne ke liye
            passenger2.SelectionStart = passenger2.Text.Length;
            passenger2.SelectionLength = 0;

            bool isNameFilled = !string.IsNullOrEmpty(passenger2.Text.Trim());

            age2.Enabled = isNameFilled;
            sex2.Enabled = isNameFilled;
            berth2.Enabled = isNameFilled;

            // Clear other fields if txtName is empty
            if (!isNameFilled)
            {
                age2.Text = string.Empty;
                sex2.SelectedIndex = -1;
                berth2.SelectedIndex = -1;
            }
        }
        private bool ValidateFields1()
        {
            // If txtName is filled, other fields must be filled
            if (!string.IsNullOrEmpty(passenger2.Text.Trim()))
            {
                if (string.IsNullOrEmpty(age2.Text.Trim()) || sex2.SelectedIndex == -1 || berth2.SelectedIndex == -1)
                {
                    return false;
                }
            }

            return true;
        }
        private void passenger3_TextChanged(object sender, EventArgs e)
        {
            passenger3.Text = passenger3.Text.ToUpper();

            // Cursor ko text ke end pe set karne ke liye
            passenger3.SelectionStart = passenger3.Text.Length;
            passenger3.SelectionLength = 0;

            bool isNameFilled = !string.IsNullOrEmpty(passenger3.Text.Trim());

            age3.Enabled = isNameFilled;
            sex3.Enabled = isNameFilled;
            berth3.Enabled = isNameFilled;

            // Clear other fields if txtName is empty
            if (!isNameFilled)
            {
                age3.Text = string.Empty;
                sex3.SelectedIndex = -1;
                berth3.SelectedIndex = -1;
            }
        }
        private bool ValidateFields2()
        {
            // If txtName is filled, other fields must be filled
            if (!string.IsNullOrEmpty(passenger3.Text.Trim()))
            {
                if (string.IsNullOrEmpty(age3.Text.Trim()) || sex3.SelectedIndex == -1 || berth3.SelectedIndex == -1)
                {
                    return false;
                }
            }

            return true;
        }
        private void passenger4_TextChanged(object sender, EventArgs e)
        {
            passenger4.Text = passenger4.Text.ToUpper();

            // Cursor ko text ke end pe set karne ke liye
            passenger4.SelectionStart = passenger4.Text.Length;
            passenger4.SelectionLength = 0;
            bool isNameFilled = !string.IsNullOrEmpty(passenger4.Text.Trim());

            age4.Enabled = isNameFilled;
            sex4.Enabled = isNameFilled;
            berth4.Enabled = isNameFilled;

            // Clear other fields if txtName is empty
            if (!isNameFilled)
            {
                age4.Text = string.Empty;
                sex4.SelectedIndex = -1;
                berth4.SelectedIndex = -1;
            }
        }
        private bool ValidateFields3()
        {
            // If txtName is filled, other fields must be filled
            if (!string.IsNullOrEmpty(passenger4.Text.Trim()))
            {
                if (string.IsNullOrEmpty(age4.Text.Trim()) || sex4.SelectedIndex == -1 || berth4.SelectedIndex == -1)
                {
                    return false;
                }
            }

            return true;
        }

        private void age1_TextChanged(object sender, EventArgs e)
        {
            age1.Text = age1.Text.ToUpper();

            // Cursor ko text ke end pe set karne ke liye
            age1.SelectionStart = age1.Text.Length;
            age1.SelectionLength = 0;
        }

        private void age2_TextChanged(object sender, EventArgs e)
        {
            age2.Text = age2.Text.ToUpper();

            // Cursor ko text ke end pe set karne ke liye
            age2.SelectionStart = age2.Text.Length;
            age2.SelectionLength = 0;
        }

        private void age3_TextChanged(object sender, EventArgs e)
        {
            age3.Text = age3.Text.ToUpper();

            // Cursor ko text ke end pe set karne ke liye
            age3.SelectionStart = age3.Text.Length;
            age3.SelectionLength = 0;
        }

        private void age4_TextChanged(object sender, EventArgs e)
        {
            age4.Text = age4.Text.ToUpper();

            // Cursor ko text ke end pe set karne ke liye
            age4.SelectionStart = age4.Text.Length;
            age4.SelectionLength = 0;
        }

        private void mobile_txt_TextChanged(object sender, EventArgs e)
        {
            mobile_txt.Text = mobile_txt.Text.ToUpper();

            // Cursor ko text ke end pe set karne ke liye
            mobile_txt.SelectionStart = mobile_txt.Text.Length;
            mobile_txt.SelectionLength = 0;
        }
        public void LoadData(string ticketname, bool showUpdateButton, bool showSaveButton)
        {
            //List<string> dataList = RetrieveDataFromDatabase();
            //PopulateComboBoxWithData(dataList);
            // Load ticket details
            isLoading = true;

            LoadTicketDetails(ticketname);

            // Reset the flag after loading is done
            isLoading = false;

            // Load passenger details
            LoadPassengerDetails(ticketname);

            save_btn.Visible = showSaveButton;

            updatebtn.Visible = showUpdateButton;
        }
        private void LoadTicketDetails(string ticketname)
        {
            string connectionString = "Data Source=sqllitedb.db;Version=3;";
            string query = "SELECT ticketname,fromStation,tostation,dateofjourney,trainno,class,quota,traintype,mobileno,ptfair,ticketslot,priorbank,backupbank FROM stationdb2 WHERE ticketname = @ticketname";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                SQLiteCommand command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@ticketname", ticketname);
                connection.Open();
                SQLiteDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                   ticket_name.Text = reader["ticketname"].ToString();
                    from.Text = reader["FromStation"].ToString();
                    to.Text = reader["ToStation"].ToString();
                    dateTimePicker1.Text = reader["dateofjourney"].ToString();
                    train_no.Text = reader["trainno"].ToString();
                    class_box.Text = reader["class"].ToString();
                    string quota = reader["quota"].ToString();

                    // Set the appropriate radio button based on quota
                    switch (quota)
                    {
                        case "Tatkal":
                            tatkal.Checked = true;
                            break;
                        case "Premium Tatkal":
                            premiumtatkal.Checked = true;
                            break;
                        case "General":
                            general.Checked = true;
                            break;
                        case "Ladies":
                            ladies.Checked = true;
                            break;
                        default:
                            // Handle default case if needed
                            break;
                    }
                   // GetSelectedQuota.Text = reader["quota"].ToString();
                    traintype_box.Text = reader["traintype"].ToString();
                    mobile_txt.Text = reader["mobileno"].ToString();
                    fair_txt.Text = reader["ptfair"].ToString();
                    slot_box.Text = reader["ticketslot"].ToString();
                    priorbank_box.Text = reader["priorbank"].ToString();
                    backupbank_box.Text = reader["backupbank"].ToString();
                  
                   
                }
                reader.Close();
                connection.Close();
            }
        }

        private void LoadPassengerDetails(string ticketname)
        {
            string connectionString = "Data Source=sqllitedb.db;Version=3;";
            string query = "SELECT passengername,age,sex,berth FROM passengerdatab2 WHERE ticketname = @ticketname";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                SQLiteCommand command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@ticketname", ticketname);
                connection.Open();
                SQLiteDataReader reader = command.ExecuteReader();

                int passengerIndex = 1;

                while (reader.Read() && passengerIndex <= 4)
                {
                    string passengerName = reader["passengername"].ToString();
                    string age = reader["age"].ToString();
                    string sex = reader["sex"].ToString();
                    string berth = reader["berth"].ToString();

                    switch (passengerIndex)
                    {
                        case 1:
                            passenger1.Text = passengerName;
                            age1.Text = age;
                            sex1.Text = sex;
                            berth1.Text = berth;
                            break;
                        case 2:
                            passenger2.Text = passengerName;
                            age2.Text = age;
                            sex2.Text = sex;
                            berth2.Text = berth;
                            break;
                        case 3:
                            passenger3.Text = passengerName;
                            age3.Text = age;
                            sex3.Text = sex;
                            berth3.Text = berth;
                            break;
                        case 4:
                            passenger4.Text = passengerName;
                            age4.Text = age;
                            sex4.Text = sex;
                            berth4.Text = berth;
                            break;
                    }

                    passengerIndex++;
                }
                reader.Close();
                connection.Close();
            }
        }

        private void updatebtn_Click(object sender, EventArgs e)
        {
           

            string fromStation = from.Text;
            string toStation = to.Text;
           // string dateOfJourney = dateTimePicker1.Value.ToString("yyyy/MM/dd");
            string trainNo = train_no.Text;
            string travelClass = class_box.Text;
            string quota = GetSelectedQuota();
            string traintype = traintype_box.Text;
            string mobileno = mobile_txt.Text;
            string ptfair = fair_txt.Text;
            string ticketslot = slot_box.Text;
            string priorbank = priorbank_box.Text;
            string backupbank = backupbank_box.Text;
            string formattedDate = dateTimePicker1.Value.ToString("yyyy-MM-dd");

            string connectionString = "Data Source=sqllitedb.db;Version=3;";
            string queryStation = "UPDATE stationdb2 SET fromstation = @fromstation, tostation = @tostation, dateofjourney = @dateofjourney, trainno = @trainno, class = @class, quota = @quota, traintype = @traintype, mobileno = @mobileno, ptfair = @ptfair, ticketslot = @ticketslot, priorbank = @priorbank, backupbank = @backupbank WHERE ticketname = @ticketname";
            string queryPassengerDelete = "DELETE FROM passengerdatab2 WHERE ticketname = @ticketname";
            string queryPassenger = "INSERT INTO passengerdatab2(ticketname, passengername, age, sex, berth) VALUES (@ticketname, @passengername, @age, @sex, @berth)";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (SQLiteCommand commandStation = new SQLiteCommand(queryStation, connection))
                {
                    commandStation.Parameters.AddWithValue("@ticketname", ticket_name.Text);
                    commandStation.Parameters.AddWithValue("@fromstation", fromStation);
                    commandStation.Parameters.AddWithValue("@tostation", toStation);
                    commandStation.Parameters.AddWithValue("@dateofjourney", formattedDate);
                    commandStation.Parameters.AddWithValue("@trainno", trainNo);
                    commandStation.Parameters.AddWithValue("@class", travelClass);
                    commandStation.Parameters.AddWithValue("@quota", quota);
                    commandStation.Parameters.AddWithValue("@traintype", traintype);
                    commandStation.Parameters.AddWithValue("@mobileno", mobileno);
                    commandStation.Parameters.AddWithValue("@ptfair", ptfair);
                    commandStation.Parameters.AddWithValue("@ticketslot", ticketslot);
                    commandStation.Parameters.AddWithValue("@priorbank", priorbank);
                    commandStation.Parameters.AddWithValue("@backupbank", backupbank);
                   
                    try
                    {
                        int rowsAffected = commandStation.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // Delete existing passengers
                            using (SQLiteCommand commandPassengerDelete = new SQLiteCommand(queryPassengerDelete, connection))
                            {
                                commandPassengerDelete.Parameters.AddWithValue("@ticketname", ticket_name.Text);
                                commandPassengerDelete.ExecuteNonQuery();
                            }

                            // Insert updated passengers
                            List<Passenger> passengers = GetPassengerDetails();
                            foreach (var passenger in passengers)
                            {
                                using (SQLiteCommand commandPassenger = new SQLiteCommand(queryPassenger, connection))
                                {
                                    commandPassenger.Parameters.AddWithValue("@ticketname", ticket_name.Text);
                                    commandPassenger.Parameters.AddWithValue("@passengername", passenger.Name);
                                    commandPassenger.Parameters.AddWithValue("@age", passenger.Age);
                                    commandPassenger.Parameters.AddWithValue("@sex", passenger.Sex);
                                    commandPassenger.Parameters.AddWithValue("@berth", passenger.Berth);
                                    
                                    commandPassenger.ExecuteNonQuery();
                                }
                            }
                            
                            MessageBox.Show("Ticket Data updated successfully.");
                            this.Close();
                            
                        }
                        else
                        {
                            savedataduplicate();
                            //save_btn_Click(sender, e);

                           // MessageBox.Show("Failed to update data.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
                connection.Close();
            }
        }
        public void savedataduplicate()
        {

          //  UpdateTicketName();

            string ticketName1 = ticket_name.Text.Trim();
            string passenger11 = passenger1.Text.Trim();
            string trainno1 = train_no.Text.Trim();
            string class1 = class_box.SelectedIndex == -1 ? string.Empty : class_box.SelectedItem.ToString();
            string priorbnk1 = priorbank_box.SelectedIndex == -1 ? string.Empty : priorbank_box.SelectedItem.ToString();

            if (string.IsNullOrEmpty(trainno1))
            {
                MessageBox.Show("Please Fill Trainno station fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrEmpty(class1))
            {
                MessageBox.Show("Please Select Train Class. ", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrEmpty(passenger11))
            {
                MessageBox.Show("Please fill At least One Passenger Detail. ", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrEmpty(priorbnk1))
            {
                MessageBox.Show("Please Select Payment Mode. ", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrEmpty(ticketName1))
            {
                MessageBox.Show("Please enter a Ticket Name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (CheckIfTicketNameExists(ticketName1))
            {
                MessageBox.Show("Ticket Name already exists. Please use a different name.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            else
            {
                if (!ValidateFields())
                {
                    MessageBox.Show("Please fill 1st Passenger. ", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!ValidateFields1())
                {
                    MessageBox.Show("Please fill 2nd Passenger. ", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!ValidateFields2())
                {
                    MessageBox.Show("Please fill 3rd Passenger. ", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!ValidateFields3())
                {
                    MessageBox.Show("Please fill 4th Passenger. ", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }


                string fromStation = from.Text;
                string toStation = to.Text;
               // string dateOfJourney = dateTimePicker1.Value.ToString("yyyy/MM/dd");
                string trainNo = train_no.Text;
                string travelClass = class_box.Text;
                string quota = GetSelectedQuota();
                string ticketName = ticket_name.Text;
                string traintype = traintype_box.Text;
                string mobileno = mobile_txt.Text;
                string ptfair = fair_txt.Text;
                string ticketslot = slot_box.Text;
                string priorbank = priorbank_box.Text;
                string backupbank = backupbank_box.Text;

                DateTime selectedDate = dateTimePicker1.Value;
                string formattedDate = selectedDate.ToString("yyyy-MM-dd");

                string queryStation = "INSERT INTO stationdb2(fromstation, tostation, dateofjourney, trainno, class, quota, ticketname,traintype,mobileno,ptfair,ticketslot,priorbank,backupbank) VALUES (@fromstation, @tostation, @dateofjourney, @trainno, @class, @quota, @ticketname,@traintype,@mobileno,@ptfair,@ticketslot,@priorbank,@backupbank)";
                string queryPassenger = "INSERT INTO passengerdatab2(ticketname, passengername, age, sex, berth) VALUES (@ticketname, @passengername, @age, @sex, @berth)";

                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    using (SQLiteCommand commandStation = new SQLiteCommand(queryStation, connection))
                    {
                        commandStation.Parameters.AddWithValue("@fromstation", fromStation);
                        commandStation.Parameters.AddWithValue("@tostation", toStation);
                        commandStation.Parameters.AddWithValue("@dateofjourney", formattedDate);
                        commandStation.Parameters.AddWithValue("@trainno", trainNo);
                        commandStation.Parameters.AddWithValue("@class", travelClass);
                        commandStation.Parameters.AddWithValue("@quota", quota);
                        commandStation.Parameters.AddWithValue("@ticketname", ticketName);
                        commandStation.Parameters.AddWithValue("@traintype", traintype);
                        commandStation.Parameters.AddWithValue("@mobileno", mobileno);
                        commandStation.Parameters.AddWithValue("@ptfair", ptfair);
                        commandStation.Parameters.AddWithValue("@ticketslot", ticketslot);
                        commandStation.Parameters.AddWithValue("@priorbank", priorbank);
                        commandStation.Parameters.AddWithValue("@backupbank", backupbank);

                        try
                        {
                            int rowsAffected = commandStation.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                // long ticketId = connection.LastInsertRowticketname;

                                List<Passenger> passengers = GetPassengerDetails();

                                foreach (var passenger in passengers)
                                {
                                    using (SQLiteCommand commandPassenger = new SQLiteCommand(queryPassenger, connection))
                                    {
                                        commandPassenger.Parameters.AddWithValue("@ticketname", ticketName);
                                        commandPassenger.Parameters.AddWithValue("@passengername", passenger.Name);
                                        commandPassenger.Parameters.AddWithValue("@age", passenger.Age);
                                        commandPassenger.Parameters.AddWithValue("@sex", passenger.Sex);
                                        commandPassenger.Parameters.AddWithValue("@berth", passenger.Berth);

                                        commandPassenger.ExecuteNonQuery();
                                    }

                                }

                                MessageBox.Show("New Ticket Data saved successfully.");
                                this.Close();
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
                        finally
                        {
                            connection.Close();
                        }

                    }

                }

            }




        }



        private void PASSENGER_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (var connection in openConnections)
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }
        private SQLiteConnection OpenConnection()
        {
            var connection = new SQLiteConnection("Data Source=sqllitedb.db;Version=3;");
            connection.Open();
            openConnections.Add(connection); // Track this connection
            return connection;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            fairlebel.Visible = true;
        }

        private void listboxsugg_SelectedIndexChanged(object sender, EventArgs e)
        {
           
        }

        private void slot_box_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void class_box_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
