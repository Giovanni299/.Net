namespace PrimeStone.ReadingsGenerator
{
    partial class PrincipalForm
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
            this.button1 = new System.Windows.Forms.Button();
            this.numSystems = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.btnLoadSystem = new System.Windows.Forms.Button();
            this.btnCalculateReadings = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.spreadsheetControl1 = new DevExpress.XtraSpreadsheet.SpreadsheetControl();
            this.btnCalculateSystems = new System.Windows.Forms.Button();
            this.dtpInitialDate = new System.Windows.Forms.DateTimePicker();
            this.dtpFinalDate = new System.Windows.Forms.DateTimePicker();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tbExecutingDay = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbExecutingSystem = new System.Windows.Forms.TextBox();
            this.tbExecutingNumber = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tbExecutingTotal = new System.Windows.Forms.TextBox();
            this.button3 = new System.Windows.Forms.Button();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numSystems)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(330, 9);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(129, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Generate systems";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // numSystems
            // 
            this.numSystems.Location = new System.Drawing.Point(144, 15);
            this.numSystems.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numSystems.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numSystems.Name = "numSystems";
            this.numSystems.Size = new System.Drawing.Size(120, 20);
            this.numSystems.TabIndex = 1;
            this.numSystems.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(32, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Number of systems";
            // 
            // textBox1
            // 
            this.textBox1.Enabled = false;
            this.textBox1.Location = new System.Drawing.Point(35, 57);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(302, 20);
            this.textBox1.TabIndex = 3;
            this.textBox1.Text = "C:\\Users\\alfonso.briceno\\Documents\\Visual Studio 2013\\Projects\\Primestone.Reading" +
    "sGenerator\\Primestone.ReadingsGenerator\\bin\\Debug\\Output";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(343, 57);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "Browse";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // btnLoadSystem
            // 
            this.btnLoadSystem.Location = new System.Drawing.Point(424, 57);
            this.btnLoadSystem.Name = "btnLoadSystem";
            this.btnLoadSystem.Size = new System.Drawing.Size(139, 23);
            this.btnLoadSystem.TabIndex = 5;
            this.btnLoadSystem.Text = "Load System";
            this.btnLoadSystem.UseVisualStyleBackColor = true;
            this.btnLoadSystem.Click += new System.EventHandler(this.btnLoadSystem_Click);
            // 
            // btnCalculateReadings
            // 
            this.btnCalculateReadings.Location = new System.Drawing.Point(406, 112);
            this.btnCalculateReadings.Name = "btnCalculateReadings";
            this.btnCalculateReadings.Size = new System.Drawing.Size(277, 23);
            this.btnCalculateReadings.TabIndex = 6;
            this.btnCalculateReadings.Text = "Export readings to INT files";
            this.btnCalculateReadings.UseVisualStyleBackColor = true;
            this.btnCalculateReadings.Click += new System.EventHandler(this.ExportReadingsToINTFiles);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(12, 670);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 7;
            this.button4.Text = "button4";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // spreadsheetControl1
            // 
            this.spreadsheetControl1.Location = new System.Drawing.Point(750, 57);
            this.spreadsheetControl1.Name = "spreadsheetControl1";
            this.spreadsheetControl1.Size = new System.Drawing.Size(178, 24);
            this.spreadsheetControl1.TabIndex = 8;
            this.spreadsheetControl1.Text = "spreadsheetControl1";
            this.spreadsheetControl1.Visible = false;
            // 
            // btnCalculateSystems
            // 
            this.btnCalculateSystems.Location = new System.Drawing.Point(569, 57);
            this.btnCalculateSystems.Name = "btnCalculateSystems";
            this.btnCalculateSystems.Size = new System.Drawing.Size(175, 23);
            this.btnCalculateSystems.TabIndex = 9;
            this.btnCalculateSystems.Text = "Calculate Systems";
            this.btnCalculateSystems.UseVisualStyleBackColor = true;
            this.btnCalculateSystems.Click += new System.EventHandler(this.btnCalculateSystems_Click);
            // 
            // dtpInitialDate
            // 
            this.dtpInitialDate.Location = new System.Drawing.Point(160, 111);
            this.dtpInitialDate.Name = "dtpInitialDate";
            this.dtpInitialDate.Size = new System.Drawing.Size(200, 20);
            this.dtpInitialDate.TabIndex = 10;
            // 
            // dtpFinalDate
            // 
            this.dtpFinalDate.Location = new System.Drawing.Point(160, 137);
            this.dtpFinalDate.Name = "dtpFinalDate";
            this.dtpFinalDate.Size = new System.Drawing.Size(200, 20);
            this.dtpFinalDate.TabIndex = 11;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(32, 117);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "Start date";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(32, 143);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "End date";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(32, 196);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "Executing day";
            // 
            // tbExecutingDay
            // 
            this.tbExecutingDay.Enabled = false;
            this.tbExecutingDay.Location = new System.Drawing.Point(160, 193);
            this.tbExecutingDay.Name = "tbExecutingDay";
            this.tbExecutingDay.Size = new System.Drawing.Size(200, 20);
            this.tbExecutingDay.TabIndex = 15;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(32, 232);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(89, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "Executing system";
            // 
            // tbExecutingSystem
            // 
            this.tbExecutingSystem.Enabled = false;
            this.tbExecutingSystem.Location = new System.Drawing.Point(160, 229);
            this.tbExecutingSystem.Name = "tbExecutingSystem";
            this.tbExecutingSystem.Size = new System.Drawing.Size(200, 20);
            this.tbExecutingSystem.TabIndex = 17;
            // 
            // tbExecutingNumber
            // 
            this.tbExecutingNumber.Enabled = false;
            this.tbExecutingNumber.Location = new System.Drawing.Point(35, 267);
            this.tbExecutingNumber.Name = "tbExecutingNumber";
            this.tbExecutingNumber.Size = new System.Drawing.Size(100, 20);
            this.tbExecutingNumber.TabIndex = 18;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(141, 270);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(16, 13);
            this.label6.TabIndex = 19;
            this.label6.Text = "of";
            // 
            // tbExecutingTotal
            // 
            this.tbExecutingTotal.Enabled = false;
            this.tbExecutingTotal.Location = new System.Drawing.Point(160, 267);
            this.tbExecutingTotal.Name = "tbExecutingTotal";
            this.tbExecutingTotal.Size = new System.Drawing.Size(100, 20);
            this.tbExecutingTotal.TabIndex = 20;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(35, 327);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(229, 23);
            this.button3.TabIndex = 21;
            this.button3.Text = "Send readigns to queue every";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click_1);
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(287, 342);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            24,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown1.TabIndex = 22;
            this.numericUpDown1.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(421, 345);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(32, 13);
            this.label7.TabIndex = 23;
            this.label7.Text = "since";
            this.label7.Click += new System.EventHandler(this.label7_Click);
            // 
            // numericUpDown2
            // 
            this.numericUpDown2.Location = new System.Drawing.Point(455, 341);
            this.numericUpDown2.Maximum = new decimal(new int[] {
            90,
            0,
            0,
            0});
            this.numericUpDown2.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown2.Name = "numericUpDown2";
            this.numericUpDown2.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown2.TabIndex = 24;
            this.numericUpDown2.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(590, 344);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(50, 13);
            this.label8.TabIndex = 25;
            this.label8.Text = "days ago";
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(35, 385);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 23);
            this.button5.TabIndex = 26;
            this.button5.Text = "stop";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(35, 356);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(229, 23);
            this.button6.TabIndex = 27;
            this.button6.Text = "Send readigns to files every";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // PrincipalForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(946, 705);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.numericUpDown2);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.tbExecutingTotal);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.tbExecutingNumber);
            this.Controls.Add(this.tbExecutingSystem);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tbExecutingDay);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.dtpFinalDate);
            this.Controls.Add(this.dtpInitialDate);
            this.Controls.Add(this.btnCalculateSystems);
            this.Controls.Add(this.spreadsheetControl1);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.btnCalculateReadings);
            this.Controls.Add(this.btnLoadSystem);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numSystems);
            this.Controls.Add(this.button1);
            this.Name = "PrincipalForm";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.numSystems)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.NumericUpDown numSystems;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button btnLoadSystem;
        private System.Windows.Forms.Button btnCalculateReadings;
        private System.Windows.Forms.Button button4;
        private DevExpress.XtraSpreadsheet.SpreadsheetControl spreadsheetControl1;
        private System.Windows.Forms.Button btnCalculateSystems;
        private System.Windows.Forms.DateTimePicker dtpInitialDate;
        private System.Windows.Forms.DateTimePicker dtpFinalDate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbExecutingDay;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbExecutingSystem;
        private System.Windows.Forms.TextBox tbExecutingNumber;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbExecutingTotal;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown numericUpDown2;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
    }
}

