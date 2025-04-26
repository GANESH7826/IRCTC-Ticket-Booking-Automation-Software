namespace IRCTC_APP
{
    partial class totakticket
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(totakticket));
            this.panel1 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.ticketnamecol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fromstationcol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tostationcol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dateofjourneycol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.quotacol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.classcol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.opencol = new System.Windows.Forms.DataGridViewButtonColumn();
            this.Login = new System.Windows.Forms.DataGridViewButtonColumn();
            this.editcol = new System.Windows.Forms.DataGridViewButtonColumn();
            this.deletecol = new System.Windows.Forms.DataGridViewButtonColumn();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(978, 30);
            this.panel1.TabIndex = 0;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            // 
            // label3
            // 
            this.label3.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(945, -8);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(34, 38);
            this.label3.TabIndex = 3;
            this.label3.Text = "x";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(137, 2);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(131, 23);
            this.label2.TabIndex = 2;
            this.label2.Text = "00";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(0, 2);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(131, 23);
            this.label1.TabIndex = 1;
            this.label1.Text = "Total Ticket";
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.Cyan;
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.button1.Location = new System.Drawing.Point(688, 330);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(110, 36);
            this.button1.TabIndex = 2;
            this.button1.Text = "Delete All";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.Cyan;
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.ForeColor = System.Drawing.Color.Black;
            this.button2.Location = new System.Drawing.Point(800, 330);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(174, 36);
            this.button2.TabIndex = 3;
            this.button2.Text = "Open All Ticket";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeColumns = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dataGridView1.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView1.BackgroundColor = System.Drawing.SystemColors.ButtonHighlight;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.Lime;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Palatino Linotype", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Desktop;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ticketnamecol,
            this.fromstationcol,
            this.tostationcol,
            this.dateofjourneycol,
            this.quotacol,
            this.classcol,
            this.opencol,
            this.Login,
            this.editcol,
            this.deletecol});
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView1.DefaultCellStyle = dataGridViewCellStyle6;
            this.dataGridView1.GridColor = System.Drawing.Color.White;
            this.dataGridView1.Location = new System.Drawing.Point(0, 27);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.RowHeadersDefaultCellStyle = dataGridViewCellStyle7;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowHeadersWidth = 82;
            this.dataGridView1.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dataGridView1.RowsDefaultCellStyle = dataGridViewCellStyle8;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(975, 297);
            this.dataGridView1.TabIndex = 4;
            this.dataGridView1.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellClick);
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick_1);
            // 
            // ticketnamecol
            // 
            this.ticketnamecol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.ticketnamecol.DataPropertyName = "ticketname";
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.White;
            this.ticketnamecol.DefaultCellStyle = dataGridViewCellStyle3;
            this.ticketnamecol.HeaderText = "Ticket Name";
            this.ticketnamecol.MinimumWidth = 10;
            this.ticketnamecol.Name = "ticketnamecol";
            this.ticketnamecol.ReadOnly = true;
            this.ticketnamecol.Width = 110;
            // 
            // fromstationcol
            // 
            this.fromstationcol.DataPropertyName = "fromstation";
            this.fromstationcol.FillWeight = 72.72726F;
            this.fromstationcol.HeaderText = "From";
            this.fromstationcol.MinimumWidth = 10;
            this.fromstationcol.Name = "fromstationcol";
            this.fromstationcol.ReadOnly = true;
            this.fromstationcol.Width = 50;
            // 
            // tostationcol
            // 
            this.tostationcol.DataPropertyName = "tostation";
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.tostationcol.DefaultCellStyle = dataGridViewCellStyle4;
            this.tostationcol.FillWeight = 73.70866F;
            this.tostationcol.HeaderText = "To";
            this.tostationcol.MinimumWidth = 10;
            this.tostationcol.Name = "tostationcol";
            this.tostationcol.ReadOnly = true;
            this.tostationcol.Width = 51;
            // 
            // dateofjourneycol
            // 
            this.dateofjourneycol.DataPropertyName = "dateofjourney";
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle5.Format = "dd/MM/yyyy";
            dataGridViewCellStyle5.NullValue = null;
            this.dateofjourneycol.DefaultCellStyle = dataGridViewCellStyle5;
            this.dateofjourneycol.FillWeight = 164.1872F;
            this.dateofjourneycol.HeaderText = "Date";
            this.dateofjourneycol.MinimumWidth = 10;
            this.dateofjourneycol.Name = "dateofjourneycol";
            this.dateofjourneycol.ReadOnly = true;
            this.dateofjourneycol.Width = 113;
            // 
            // quotacol
            // 
            this.quotacol.DataPropertyName = "quota";
            this.quotacol.FillWeight = 56.05515F;
            this.quotacol.HeaderText = "Quota";
            this.quotacol.MinimumWidth = 10;
            this.quotacol.Name = "quotacol";
            this.quotacol.ReadOnly = true;
            this.quotacol.Width = 135;
            // 
            // classcol
            // 
            this.classcol.DataPropertyName = "class";
            this.classcol.FillWeight = 57.6486F;
            this.classcol.HeaderText = "Class";
            this.classcol.MinimumWidth = 10;
            this.classcol.Name = "classcol";
            this.classcol.ReadOnly = true;
            this.classcol.Width = 150;
            // 
            // opencol
            // 
            this.opencol.FillWeight = 145.3789F;
            this.opencol.HeaderText = "Open";
            this.opencol.MinimumWidth = 10;
            this.opencol.Name = "opencol";
            this.opencol.ReadOnly = true;
            this.opencol.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.opencol.Text = "Open";
            this.opencol.UseColumnTextForButtonValue = true;
            this.opencol.Width = 101;
            // 
            // Login
            // 
            this.Login.FillWeight = 146.187F;
            this.Login.HeaderText = "Login";
            this.Login.MinimumWidth = 10;
            this.Login.Name = "Login";
            this.Login.ReadOnly = true;
            this.Login.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Login.Text = "Login";
            this.Login.UseColumnTextForButtonValue = true;
            this.Login.Width = 101;
            // 
            // editcol
            // 
            this.editcol.FillWeight = 132.2833F;
            this.editcol.HeaderText = "Edit";
            this.editcol.MinimumWidth = 10;
            this.editcol.Name = "editcol";
            this.editcol.ReadOnly = true;
            this.editcol.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.editcol.Text = "Edit";
            this.editcol.UseColumnTextForButtonValue = true;
            this.editcol.Width = 82;
            // 
            // deletecol
            // 
            this.deletecol.FillWeight = 127.2899F;
            this.deletecol.HeaderText = "Delete";
            this.deletecol.MinimumWidth = 10;
            this.deletecol.Name = "deletecol";
            this.deletecol.ReadOnly = true;
            this.deletecol.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.deletecol.Text = "Delete";
            this.deletecol.UseColumnTextForButtonValue = true;
            this.deletecol.Width = 80;
            // 
            // totakticket
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.ClientSize = new System.Drawing.Size(978, 373);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "totakticket";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "totakticket";
            this.Load += new System.EventHandler(this.totakticket_Load);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn ticketnamecol;
        private System.Windows.Forms.DataGridViewTextBoxColumn fromstationcol;
        private System.Windows.Forms.DataGridViewTextBoxColumn tostationcol;
        private System.Windows.Forms.DataGridViewTextBoxColumn dateofjourneycol;
        private System.Windows.Forms.DataGridViewTextBoxColumn quotacol;
        private System.Windows.Forms.DataGridViewTextBoxColumn classcol;
        private System.Windows.Forms.DataGridViewButtonColumn opencol;
        private System.Windows.Forms.DataGridViewButtonColumn Login;
        private System.Windows.Forms.DataGridViewButtonColumn editcol;
        private System.Windows.Forms.DataGridViewButtonColumn deletecol;
    }
}