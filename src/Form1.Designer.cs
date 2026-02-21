namespace FCS
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            textBox1 = new System.Windows.Forms.TextBox();
            folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            button2 = new System.Windows.Forms.Button();
            comboBox1 = new System.Windows.Forms.ComboBox();
            comboBox2 = new System.Windows.Forms.ComboBox();
            textBox4 = new System.Windows.Forms.TextBox();
            progressBar1 = new System.Windows.Forms.ProgressBar();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            groupBox1 = new System.Windows.Forms.GroupBox();
            checkedListBox2 = new System.Windows.Forms.CheckedListBox();
            groupBox2 = new System.Windows.Forms.GroupBox();
            label22 = new System.Windows.Forms.Label();
            trackBar6 = new System.Windows.Forms.TrackBar();
            label23 = new System.Windows.Forms.Label();
            label20 = new System.Windows.Forms.Label();
            trackBar5 = new System.Windows.Forms.TrackBar();
            label21 = new System.Windows.Forms.Label();
            label15 = new System.Windows.Forms.Label();
            textBox8 = new System.Windows.Forms.TextBox();
            label16 = new System.Windows.Forms.Label();
            label17 = new System.Windows.Forms.Label();
            textBox10 = new System.Windows.Forms.TextBox();
            textBox9 = new System.Windows.Forms.TextBox();
            checkedListBox3 = new System.Windows.Forms.CheckedListBox();
            label10 = new System.Windows.Forms.Label();
            label11 = new System.Windows.Forms.Label();
            trackBar3 = new System.Windows.Forms.TrackBar();
            textBox7 = new System.Windows.Forms.TextBox();
            label12 = new System.Windows.Forms.Label();
            label13 = new System.Windows.Forms.Label();
            trackBar4 = new System.Windows.Forms.TrackBar();
            label14 = new System.Windows.Forms.Label();
            label8 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            label9 = new System.Windows.Forms.Label();
            trackBar2 = new System.Windows.Forms.TrackBar();
            textBox5 = new System.Windows.Forms.TextBox();
            label7 = new System.Windows.Forms.Label();
            pictureBox3 = new System.Windows.Forms.PictureBox();
            textBox6 = new System.Windows.Forms.TextBox();
            pictureBox2 = new System.Windows.Forms.PictureBox();
            label5 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            label3 = new System.Windows.Forms.Label();
            trackBar1 = new System.Windows.Forms.TrackBar();
            labelSightInfo = new System.Windows.Forms.Label();
            colorDialog1 = new System.Windows.Forms.ColorDialog();
            label18 = new System.Windows.Forms.Label();
            label19 = new System.Windows.Forms.Label();
            timer1 = new System.Windows.Forms.Timer(components);
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBar6).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBar5).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBar3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBar4).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBar2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBar1).BeginInit();
            SuspendLayout();
            //
            // textBox1
            //
            textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            textBox1.Location = new System.Drawing.Point(4, 20);
            textBox1.Margin = new System.Windows.Forms.Padding(2);
            textBox1.Name = "textBox1";
            textBox1.Size = new System.Drawing.Size(281, 20);
            textBox1.TabIndex = 0;
            textBox1.Text = "Game Path";
            textBox1.Click += TextBox1_Click;
            //
            // button2
            //
            button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            button2.Location = new System.Drawing.Point(10, 426);
            button2.Margin = new System.Windows.Forms.Padding(2);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(567, 23);
            button2.TabIndex = 21;
            button2.Text = "Generate Sights";
            button2.UseVisualStyleBackColor = true;
            button2.Click += Button2_Click;
            //
            // comboBox1
            //
            comboBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            comboBox1.FormattingEnabled = true;
            comboBox1.Items.AddRange(new object[] { "Tochka-SM2", "Duga", "Duga-2", "Luch", "Luch Lite", "Sector" });
            comboBox1.Location = new System.Drawing.Point(4, 93);
            comboBox1.Margin = new System.Windows.Forms.Padding(2);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new System.Drawing.Size(281, 21);
            comboBox1.TabIndex = 5;
            comboBox1.Text = "Sight type";
            comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
            //
            // comboBox2
            //
            comboBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            comboBox2.FormattingEnabled = true;
            comboBox2.Items.AddRange(new object[] { "English", "French", "Italian", "German", "Spanish", "Russian", "Polish", "Czech", "Turkish", "Chinese", "Japanese", "Portuguese", "Ukrainian", "Serbian", "Hungarian", "Korean", "Belarusian", "Romanian", "TChinese", "HChinese" });
            comboBox2.Location = new System.Drawing.Point(4, 68);
            comboBox2.Margin = new System.Windows.Forms.Padding(2);
            comboBox2.Name = "comboBox2";
            comboBox2.Size = new System.Drawing.Size(281, 21);
            comboBox2.TabIndex = 4;
            comboBox2.Text = "English";
            //
            // textBox4
            //
            textBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            textBox4.Location = new System.Drawing.Point(4, 44);
            textBox4.Margin = new System.Windows.Forms.Padding(2);
            textBox4.Name = "textBox4";
            textBox4.Size = new System.Drawing.Size(281, 20);
            textBox4.TabIndex = 3;
            textBox4.Text = "Output";
            textBox4.Click += TextBox4_Click;
            //
            // progressBar1
            //
            progressBar1.Location = new System.Drawing.Point(12, 453);
            progressBar1.Margin = new System.Windows.Forms.Padding(2);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new System.Drawing.Size(566, 22);
            progressBar1.TabIndex = 8;
            //
            // label1
            //
            label1.AutoEllipsis = true;
            label1.AutoSize = true;
            label1.BackColor = System.Drawing.Color.Transparent;
            label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            label1.Location = new System.Drawing.Point(12, 478);
            label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label1.MaximumSize = new System.Drawing.Size(271, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(26, 13);
            label1.TabIndex = 11;
            label1.Text = "File:";
            label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // label2
            //
            label2.AutoEllipsis = true;
            label2.AutoSize = true;
            label2.BackColor = System.Drawing.Color.Transparent;
            label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            label2.Location = new System.Drawing.Point(5, 21);
            label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label2.MaximumSize = new System.Drawing.Size(271, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(54, 13);
            label2.TabIndex = 13;
            label2.Text = "Sensitivity";
            label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // labelSightInfo
            //
            labelSightInfo.BackColor = System.Drawing.Color.Transparent;
            labelSightInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            labelSightInfo.ForeColor = System.Drawing.Color.Gray;
            labelSightInfo.Location = new System.Drawing.Point(4, 118);
            labelSightInfo.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            labelSightInfo.MaximumSize = new System.Drawing.Size(281, 0);
            labelSightInfo.Name = "labelSightInfo";
            labelSightInfo.Size = new System.Drawing.Size(281, 56);
            labelSightInfo.TabIndex = 129;
            labelSightInfo.Text = "";
            labelSightInfo.Visible = false;
            //
            // checkedListBox1
            //
            checkedListBox1.CheckOnClick = true;
            checkedListBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            checkedListBox1.FormattingEnabled = true;
            checkedListBox1.Location = new System.Drawing.Point(4, 179);
            checkedListBox1.Margin = new System.Windows.Forms.Padding(2);
            checkedListBox1.Name = "checkedListBox1";
            checkedListBox1.Size = new System.Drawing.Size(281, 154);
            checkedListBox1.TabIndex = 6;
            //
            // groupBox1
            //
            groupBox1.Controls.Add(checkedListBox2);
            groupBox1.Controls.Add(textBox1);
            groupBox1.Controls.Add(labelSightInfo);
            groupBox1.Controls.Add(checkedListBox1);
            groupBox1.Controls.Add(textBox4);
            groupBox1.Controls.Add(comboBox2);
            groupBox1.Controls.Add(comboBox1);
            groupBox1.Location = new System.Drawing.Point(5, 8);
            groupBox1.Margin = new System.Windows.Forms.Padding(2);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new System.Windows.Forms.Padding(2);
            groupBox1.Size = new System.Drawing.Size(289, 405);
            groupBox1.TabIndex = 100;
            groupBox1.TabStop = false;
            groupBox1.Text = "Basic Settings";
            groupBox1.Click += groupBox1_Click;
            //
            // checkedListBox2
            //
            checkedListBox2.CheckOnClick = true;
            checkedListBox2.ColumnWidth = 75;
            checkedListBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            checkedListBox2.FormattingEnabled = true;
            checkedListBox2.Location = new System.Drawing.Point(4, 337);
            checkedListBox2.Margin = new System.Windows.Forms.Padding(2);
            checkedListBox2.MultiColumn = true;
            checkedListBox2.Name = "checkedListBox2";
            checkedListBox2.Size = new System.Drawing.Size(281, 64);
            checkedListBox2.TabIndex = 7;
            //
            // groupBox2
            //
            groupBox2.Controls.Add(label22);
            groupBox2.Controls.Add(trackBar6);
            groupBox2.Controls.Add(label23);
            groupBox2.Controls.Add(label20);
            groupBox2.Controls.Add(trackBar5);
            groupBox2.Controls.Add(label21);
            groupBox2.Controls.Add(label15);
            groupBox2.Controls.Add(textBox8);
            groupBox2.Controls.Add(label16);
            groupBox2.Controls.Add(label17);
            groupBox2.Controls.Add(textBox10);
            groupBox2.Controls.Add(textBox9);
            groupBox2.Controls.Add(checkedListBox3);
            groupBox2.Controls.Add(label10);
            groupBox2.Controls.Add(label11);
            groupBox2.Controls.Add(trackBar3);
            groupBox2.Controls.Add(textBox7);
            groupBox2.Controls.Add(label12);
            groupBox2.Controls.Add(label13);
            groupBox2.Controls.Add(trackBar4);
            groupBox2.Controls.Add(label14);
            groupBox2.Controls.Add(label8);
            groupBox2.Controls.Add(label6);
            groupBox2.Controls.Add(label9);
            groupBox2.Controls.Add(trackBar2);
            groupBox2.Controls.Add(textBox5);
            groupBox2.Controls.Add(label7);
            groupBox2.Controls.Add(pictureBox3);
            groupBox2.Controls.Add(textBox6);
            groupBox2.Controls.Add(pictureBox2);
            groupBox2.Controls.Add(label5);
            groupBox2.Controls.Add(label4);
            groupBox2.Controls.Add(pictureBox1);
            groupBox2.Controls.Add(label3);
            groupBox2.Controls.Add(trackBar1);
            groupBox2.Controls.Add(label2);
            groupBox2.Location = new System.Drawing.Point(296, 8);
            groupBox2.Margin = new System.Windows.Forms.Padding(2);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new System.Windows.Forms.Padding(2);
            groupBox2.Size = new System.Drawing.Size(289, 405);
            groupBox2.TabIndex = 101;
            groupBox2.TabStop = false;
            groupBox2.Text = "Advanced Settings";
            //
            // label22
            //
            label22.AutoEllipsis = true;
            label22.AutoSize = true;
            label22.BackColor = System.Drawing.Color.Transparent;
            label22.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            label22.Location = new System.Drawing.Point(255, 125);
            label22.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label22.MaximumSize = new System.Drawing.Size(271, 0);
            label22.Name = "label22";
            label22.RightToLeft = System.Windows.Forms.RightToLeft.No;
            label22.Size = new System.Drawing.Size(13, 13);
            label22.TabIndex = 127;
            label22.Text = "1";
            label22.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // trackBar6
            //
            trackBar6.AutoSize = false;
            trackBar6.LargeChange = 1;
            trackBar6.Location = new System.Drawing.Point(149, 143);
            trackBar6.Margin = new System.Windows.Forms.Padding(2);
            trackBar6.Maximum = 100;
            trackBar6.Minimum = 10;
            trackBar6.Name = "trackBar6";
            trackBar6.Size = new System.Drawing.Size(131, 27);
            trackBar6.TabIndex = 126;
            trackBar6.TickStyle = System.Windows.Forms.TickStyle.None;
            trackBar6.Value = 20;
            trackBar6.Scroll += trackBar6_Scroll;
            //
            // label23
            //
            label23.AutoEllipsis = true;
            label23.AutoSize = true;
            label23.BackColor = System.Drawing.Color.Transparent;
            label23.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            label23.Location = new System.Drawing.Point(147, 125);
            label23.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label23.MaximumSize = new System.Drawing.Size(271, 0);
            label23.Name = "label23";
            label23.Size = new System.Drawing.Size(82, 13);
            label23.TabIndex = 128;
            label23.Text = "Distance Factor";
            label23.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // label20
            //
            label20.AutoEllipsis = true;
            label20.AutoSize = true;
            label20.BackColor = System.Drawing.Color.Transparent;
            label20.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            label20.Location = new System.Drawing.Point(100, 125);
            label20.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label20.MaximumSize = new System.Drawing.Size(271, 0);
            label20.Name = "label20";
            label20.RightToLeft = System.Windows.Forms.RightToLeft.No;
            label20.Size = new System.Drawing.Size(28, 13);
            label20.TabIndex = 124;
            label20.Text = "0.75";
            label20.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // trackBar5
            //
            trackBar5.AutoSize = false;
            trackBar5.LargeChange = 1;
            trackBar5.Location = new System.Drawing.Point(8, 143);
            trackBar5.Margin = new System.Windows.Forms.Padding(2);
            trackBar5.Maximum = 40;
            trackBar5.Minimum = 1;
            trackBar5.Name = "trackBar5";
            trackBar5.Size = new System.Drawing.Size(131, 27);
            trackBar5.TabIndex = 123;
            trackBar5.TickStyle = System.Windows.Forms.TickStyle.None;
            trackBar5.Value = 15;
            trackBar5.Scroll += trackBar5_Scroll;
            //
            // label21
            //
            label21.AutoEllipsis = true;
            label21.AutoSize = true;
            label21.BackColor = System.Drawing.Color.Transparent;
            label21.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            label21.Location = new System.Drawing.Point(5, 125);
            label21.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label21.MaximumSize = new System.Drawing.Size(271, 0);
            label21.Name = "label21";
            label21.Size = new System.Drawing.Size(51, 13);
            label21.TabIndex = 125;
            label21.Text = "Font Size";
            label21.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // label15
            //
            label15.AutoEllipsis = true;
            label15.AutoSize = true;
            label15.BackColor = System.Drawing.Color.Transparent;
            label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            label15.Location = new System.Drawing.Point(4, 313);
            label15.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label15.MaximumSize = new System.Drawing.Size(271, 0);
            label15.Name = "label15";
            label15.Size = new System.Drawing.Size(79, 13);
            label15.TabIndex = 122;
            label15.Text = "Detect Ally Pos";
            label15.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // textBox8
            //
            textBox8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            textBox8.Location = new System.Drawing.Point(100, 313);
            textBox8.Margin = new System.Windows.Forms.Padding(2);
            textBox8.Name = "textBox8";
            textBox8.Size = new System.Drawing.Size(62, 20);
            textBox8.TabIndex = 15;
            textBox8.Text = "-345, 0.2";
            //
            // label16
            //
            label16.AutoEllipsis = true;
            label16.AutoSize = true;
            label16.BackColor = System.Drawing.Color.Transparent;
            label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            label16.Location = new System.Drawing.Point(4, 337);
            label16.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label16.MaximumSize = new System.Drawing.Size(271, 0);
            label16.Name = "label16";
            label16.Size = new System.Drawing.Size(70, 13);
            label16.TabIndex = 121;
            label16.Text = "Distance Pos";
            label16.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // label17
            //
            label17.AutoEllipsis = true;
            label17.AutoSize = true;
            label17.BackColor = System.Drawing.Color.Transparent;
            label17.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            label17.Location = new System.Drawing.Point(4, 292);
            label17.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label17.MaximumSize = new System.Drawing.Size(271, 0);
            label17.Name = "label17";
            label17.Size = new System.Drawing.Size(80, 13);
            label17.TabIndex = 120;
            label17.Text = "Rangefiner Pos";
            label17.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // textBox10
            //
            textBox10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            textBox10.Location = new System.Drawing.Point(100, 289);
            textBox10.Margin = new System.Windows.Forms.Padding(2);
            textBox10.Name = "textBox10";
            textBox10.Size = new System.Drawing.Size(62, 20);
            textBox10.TabIndex = 13;
            textBox10.Text = "250, 0.2";
            //
            // textBox9
            //
            textBox9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            textBox9.Location = new System.Drawing.Point(100, 337);
            textBox9.Margin = new System.Windows.Forms.Padding(2);
            textBox9.Name = "textBox9";
            textBox9.Size = new System.Drawing.Size(62, 20);
            textBox9.TabIndex = 14;
            textBox9.Text = "70.5, 47";
            //
            // checkedListBox3
            //
            checkedListBox3.CheckOnClick = true;
            checkedListBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            checkedListBox3.FormattingEnabled = true;
            checkedListBox3.Location = new System.Drawing.Point(4, 179);
            checkedListBox3.Margin = new System.Windows.Forms.Padding(2);
            checkedListBox3.Name = "checkedListBox3";
            checkedListBox3.Size = new System.Drawing.Size(281, 94);
            checkedListBox3.TabIndex = 12;
            //
            // label10
            //
            label10.AutoEllipsis = true;
            label10.AutoSize = true;
            label10.BackColor = System.Drawing.Color.Transparent;
            label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            label10.Location = new System.Drawing.Point(183, 316);
            label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label10.MaximumSize = new System.Drawing.Size(271, 0);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(49, 13);
            label10.TabIndex = 107;
            label10.Text = "Width, m";
            label10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // label11
            //
            label11.AutoEllipsis = true;
            label11.AutoSize = true;
            label11.BackColor = System.Drawing.Color.Transparent;
            label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            label11.Location = new System.Drawing.Point(259, 73);
            label11.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label11.MaximumSize = new System.Drawing.Size(271, 0);
            label11.Name = "label11";
            label11.Size = new System.Drawing.Size(13, 13);
            label11.TabIndex = 108;
            label11.Text = "2";
            label11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // trackBar3
            //
            trackBar3.AutoSize = false;
            trackBar3.LargeChange = 1;
            trackBar3.Location = new System.Drawing.Point(149, 96);
            trackBar3.Margin = new System.Windows.Forms.Padding(2);
            trackBar3.Maximum = 50;
            trackBar3.Name = "trackBar3";
            trackBar3.Size = new System.Drawing.Size(131, 27);
            trackBar3.TabIndex = 11;
            trackBar3.TickStyle = System.Windows.Forms.TickStyle.None;
            trackBar3.Value = 20;
            trackBar3.Scroll += trackBar3_Scroll;
            //
            // textBox7
            //
            textBox7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            textBox7.Location = new System.Drawing.Point(241, 313);
            textBox7.Margin = new System.Windows.Forms.Padding(2);
            textBox7.Name = "textBox7";
            textBox7.Size = new System.Drawing.Size(44, 20);
            textBox7.TabIndex = 18;
            textBox7.Text = "3.2";
            //
            // label12
            //
            label12.AutoEllipsis = true;
            label12.AutoSize = true;
            label12.BackColor = System.Drawing.Color.Transparent;
            label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            label12.Location = new System.Drawing.Point(145, 75);
            label12.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label12.MaximumSize = new System.Drawing.Size(271, 0);
            label12.Name = "label12";
            label12.Size = new System.Drawing.Size(83, 13);
            label12.TabIndex = 109;
            label12.Text = "Point Thickness";
            label12.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // label13
            //
            label13.AutoEllipsis = true;
            label13.AutoSize = true;
            label13.BackColor = System.Drawing.Color.Transparent;
            label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            label13.Location = new System.Drawing.Point(114, 77);
            label13.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label13.MaximumSize = new System.Drawing.Size(271, 0);
            label13.Name = "label13";
            label13.RightToLeft = System.Windows.Forms.RightToLeft.No;
            label13.Size = new System.Drawing.Size(13, 13);
            label13.TabIndex = 110;
            label13.Text = "5";
            label13.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // trackBar4
            //
            trackBar4.AutoSize = false;
            trackBar4.LargeChange = 1;
            trackBar4.Location = new System.Drawing.Point(8, 96);
            trackBar4.Margin = new System.Windows.Forms.Padding(2);
            trackBar4.Maximum = 100;
            trackBar4.Name = "trackBar4";
            trackBar4.Size = new System.Drawing.Size(131, 27);
            trackBar4.TabIndex = 10;
            trackBar4.TickStyle = System.Windows.Forms.TickStyle.None;
            trackBar4.Value = 50;
            trackBar4.Scroll += trackBar4_Scroll;
            //
            // label14
            //
            label14.AutoEllipsis = true;
            label14.AutoSize = true;
            label14.BackColor = System.Drawing.Color.Transparent;
            label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            label14.Location = new System.Drawing.Point(5, 77);
            label14.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label14.MaximumSize = new System.Drawing.Size(271, 0);
            label14.Name = "label14";
            label14.Size = new System.Drawing.Size(76, 13);
            label14.TabIndex = 111;
            label14.Text = "Inner Diameter";
            label14.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // label8
            //
            label8.AutoEllipsis = true;
            label8.AutoSize = true;
            label8.BackColor = System.Drawing.Color.Transparent;
            label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            label8.Location = new System.Drawing.Point(183, 292);
            label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label8.MaximumSize = new System.Drawing.Size(271, 0);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(54, 13);
            label8.TabIndex = 103;
            label8.Text = "Length, m";
            label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // label6
            //
            label6.AutoEllipsis = true;
            label6.AutoSize = true;
            label6.BackColor = System.Drawing.Color.Transparent;
            label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            label6.Location = new System.Drawing.Point(248, 18);
            label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label6.MaximumSize = new System.Drawing.Size(271, 0);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(22, 13);
            label6.TabIndex = 112;
            label6.Text = "1.5";
            label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // label9
            //
            label9.AutoEllipsis = true;
            label9.AutoSize = true;
            label9.BackColor = System.Drawing.Color.Transparent;
            label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            label9.Location = new System.Drawing.Point(183, 340);
            label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label9.MaximumSize = new System.Drawing.Size(271, 0);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(52, 13);
            label9.TabIndex = 105;
            label9.Text = "Height, m";
            label9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // trackBar2
            //
            trackBar2.AutoSize = false;
            trackBar2.LargeChange = 1;
            trackBar2.Location = new System.Drawing.Point(149, 39);
            trackBar2.Margin = new System.Windows.Forms.Padding(2);
            trackBar2.Maximum = 50;
            trackBar2.Minimum = 1;
            trackBar2.Name = "trackBar2";
            trackBar2.Size = new System.Drawing.Size(131, 27);
            trackBar2.TabIndex = 9;
            trackBar2.TickStyle = System.Windows.Forms.TickStyle.None;
            trackBar2.Value = 15;
            trackBar2.Scroll += trackBar2_Scroll;
            //
            // textBox5
            //
            textBox5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            textBox5.Location = new System.Drawing.Point(241, 289);
            textBox5.Margin = new System.Windows.Forms.Padding(2);
            textBox5.Name = "textBox5";
            textBox5.Size = new System.Drawing.Size(44, 20);
            textBox5.TabIndex = 16;
            textBox5.Text = "6.5";
            //
            // label7
            //
            label7.AutoEllipsis = true;
            label7.AutoSize = true;
            label7.BackColor = System.Drawing.Color.Transparent;
            label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            label7.Location = new System.Drawing.Point(145, 18);
            label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label7.MaximumSize = new System.Drawing.Size(271, 0);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(50, 13);
            label7.TabIndex = 113;
            label7.Text = "Line Size";
            label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // pictureBox3
            //
            pictureBox3.BackColor = System.Drawing.Color.White;
            pictureBox3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pictureBox3.Location = new System.Drawing.Point(192, 379);
            pictureBox3.Margin = new System.Windows.Forms.Padding(2);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new System.Drawing.Size(88, 22);
            pictureBox3.TabIndex = 114;
            pictureBox3.TabStop = false;
            pictureBox3.Click += pictureBox3_Click;
            //
            // textBox6
            //
            textBox6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            textBox6.Location = new System.Drawing.Point(241, 337);
            textBox6.Margin = new System.Windows.Forms.Padding(2);
            textBox6.Name = "textBox6";
            textBox6.Size = new System.Drawing.Size(44, 20);
            textBox6.TabIndex = 17;
            textBox6.Text = "2.7";
            //
            // pictureBox2
            //
            pictureBox2.BackColor = System.Drawing.Color.Lime;
            pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pictureBox2.Location = new System.Drawing.Point(100, 379);
            pictureBox2.Margin = new System.Windows.Forms.Padding(2);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new System.Drawing.Size(88, 22);
            pictureBox2.TabIndex = 115;
            pictureBox2.TabStop = false;
            pictureBox2.Click += pictureBox2_Click;
            //
            // label5
            //
            label5.AutoEllipsis = true;
            label5.AutoSize = true;
            label5.BackColor = System.Drawing.Color.Transparent;
            label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            label5.Location = new System.Drawing.Point(4, 364);
            label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label5.MaximumSize = new System.Drawing.Size(271, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(56, 13);
            label5.TabIndex = 114;
            label5.Text = "Light color";
            label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // label4
            //
            label4.AutoEllipsis = true;
            label4.AutoSize = true;
            label4.BackColor = System.Drawing.Color.Transparent;
            label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            label4.Location = new System.Drawing.Point(100, 364);
            label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label4.MaximumSize = new System.Drawing.Size(271, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(155, 13);
            label4.TabIndex = 115;
            label4.Text = "Progressbar Colors (Alpha = 64)";
            label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // pictureBox1
            //
            pictureBox1.BackColor = System.Drawing.Color.Red;
            pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pictureBox1.Location = new System.Drawing.Point(5, 379);
            pictureBox1.Margin = new System.Windows.Forms.Padding(2);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(88, 22);
            pictureBox1.TabIndex = 116;
            pictureBox1.TabStop = false;
            pictureBox1.Click += pictureBox1_Click;
            //
            // label3
            //
            label3.AutoEllipsis = true;
            label3.AutoSize = true;
            label3.BackColor = System.Drawing.Color.Transparent;
            label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            label3.Location = new System.Drawing.Point(100, 21);
            label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label3.MaximumSize = new System.Drawing.Size(271, 0);
            label3.Name = "label3";
            label3.RightToLeft = System.Windows.Forms.RightToLeft.No;
            label3.Size = new System.Drawing.Size(27, 13);
            label3.TabIndex = 116;
            label3.Text = "50%";
            label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // trackBar1
            //
            trackBar1.AutoSize = false;
            trackBar1.Location = new System.Drawing.Point(8, 39);
            trackBar1.Margin = new System.Windows.Forms.Padding(2);
            trackBar1.Maximum = 100;
            trackBar1.Minimum = 1;
            trackBar1.Name = "trackBar1";
            trackBar1.Size = new System.Drawing.Size(131, 27);
            trackBar1.TabIndex = 8;
            trackBar1.TickStyle = System.Windows.Forms.TickStyle.None;
            trackBar1.Value = 50;
            trackBar1.Scroll += TrackBar1_Scroll;
            //
            // label18
            //
            label18.AutoEllipsis = true;
            label18.AutoSize = true;
            label18.BackColor = System.Drawing.Color.Transparent;
            label18.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            label18.Location = new System.Drawing.Point(294, 478);
            label18.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label18.MaximumSize = new System.Drawing.Size(271, 0);
            label18.Name = "label18";
            label18.Size = new System.Drawing.Size(33, 13);
            label18.TabIndex = 102;
            label18.Text = "Time:";
            label18.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // label19
            //
            label19.AutoEllipsis = true;
            label19.AutoSize = true;
            label19.BackColor = System.Drawing.Color.Transparent;
            label19.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            label19.Location = new System.Drawing.Point(411, 478);
            label19.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label19.MaximumSize = new System.Drawing.Size(271, 0);
            label19.Name = "label19";
            label19.Size = new System.Drawing.Size(82, 13);
            label19.TabIndex = 103;
            label19.Text = "Remaining time:";
            label19.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // timer1
            //
            timer1.Enabled = true;
            timer1.Interval = 1000;
            timer1.Tick += timer1_Tick;
            //
            // Form1
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(590, 508);
            Controls.Add(label19);
            Controls.Add(label18);
            Controls.Add(label1);
            Controls.Add(progressBar1);
            Controls.Add(button2);
            Controls.Add(groupBox1);
            Controls.Add(groupBox2);
            Margin = new System.Windows.Forms.Padding(2);
            MaximizeBox = false;
            MaximumSize = new System.Drawing.Size(606, 547);
            MinimumSize = new System.Drawing.Size(606, 547);
            Name = "Form1";
            Text = "FCS Manager";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBar6).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBar5).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBar3).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBar4).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBar2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBar1).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckedListBox checkedListBox1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckedListBox checkedListBox2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TrackBar trackBar2;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textBox6;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox textBox7;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TrackBar trackBar3;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TrackBar trackBar4;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.CheckedListBox checkedListBox3;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox textBox8;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox textBox9;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox textBox10;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.TrackBar trackBar6;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.TrackBar trackBar5;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label labelSightInfo;
    }
}

