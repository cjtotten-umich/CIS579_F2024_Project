namespace ImageLabelling
{
    partial class Form1
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
            this.picCurrent = new System.Windows.Forms.PictureBox();
            this.txtImageDirectory = new System.Windows.Forms.TextBox();
            this.lblImageDirectory = new System.Windows.Forms.Label();
            this.lblImageCount = new System.Windows.Forms.Label();
            this.cmdNextImage = new System.Windows.Forms.Button();
            this.lblCurrent = new System.Windows.Forms.Label();
            this.txtLabelledRoot = new System.Windows.Forms.TextBox();
            this.lblLabelledRoot = new System.Windows.Forms.Label();
            this.cmdSaveImage = new System.Windows.Forms.Button();
            this.chkHasSign = new System.Windows.Forms.CheckBox();
            this.lblXminLabel = new System.Windows.Forms.Label();
            this.lblXmin = new System.Windows.Forms.Label();
            this.lblYmin = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblXmax = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lblYmax = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblFilename = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picCurrent)).BeginInit();
            this.SuspendLayout();
            // 
            // picCurrent
            // 
            this.picCurrent.Location = new System.Drawing.Point(9, 56);
            this.picCurrent.Margin = new System.Windows.Forms.Padding(2);
            this.picCurrent.Name = "picCurrent";
            this.picCurrent.Size = new System.Drawing.Size(384, 416);
            this.picCurrent.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picCurrent.TabIndex = 0;
            this.picCurrent.TabStop = false;
            this.picCurrent.Click += new System.EventHandler(this.picCurrent_Click);
            // 
            // txtImageDirectory
            // 
            this.txtImageDirectory.Location = new System.Drawing.Point(602, 36);
            this.txtImageDirectory.Margin = new System.Windows.Forms.Padding(2);
            this.txtImageDirectory.Name = "txtImageDirectory";
            this.txtImageDirectory.Size = new System.Drawing.Size(285, 20);
            this.txtImageDirectory.TabIndex = 2;
            this.txtImageDirectory.Text = "C:\\ECE579\\Images";
            // 
            // lblImageDirectory
            // 
            this.lblImageDirectory.AutoSize = true;
            this.lblImageDirectory.Location = new System.Drawing.Point(602, 19);
            this.lblImageDirectory.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblImageDirectory.Name = "lblImageDirectory";
            this.lblImageDirectory.Size = new System.Drawing.Size(81, 13);
            this.lblImageDirectory.TabIndex = 3;
            this.lblImageDirectory.Text = "Image Directory";
            // 
            // lblImageCount
            // 
            this.lblImageCount.AutoSize = true;
            this.lblImageCount.Location = new System.Drawing.Point(722, 78);
            this.lblImageCount.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblImageCount.Name = "lblImageCount";
            this.lblImageCount.Size = new System.Drawing.Size(13, 13);
            this.lblImageCount.TabIndex = 4;
            this.lblImageCount.Text = "0";
            // 
            // cmdNextImage
            // 
            this.cmdNextImage.Location = new System.Drawing.Point(602, 60);
            this.cmdNextImage.Margin = new System.Windows.Forms.Padding(2);
            this.cmdNextImage.Name = "cmdNextImage";
            this.cmdNextImage.Size = new System.Drawing.Size(86, 48);
            this.cmdNextImage.TabIndex = 5;
            this.cmdNextImage.Text = "Next Image";
            this.cmdNextImage.UseVisualStyleBackColor = true;
            this.cmdNextImage.Click += new System.EventHandler(this.cmdNextImage_Click);
            // 
            // lblCurrent
            // 
            this.lblCurrent.AutoSize = true;
            this.lblCurrent.Location = new System.Drawing.Point(692, 78);
            this.lblCurrent.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblCurrent.Name = "lblCurrent";
            this.lblCurrent.Size = new System.Drawing.Size(13, 13);
            this.lblCurrent.TabIndex = 6;
            this.lblCurrent.Text = "0";
            // 
            // txtLabelledRoot
            // 
            this.txtLabelledRoot.Location = new System.Drawing.Point(602, 216);
            this.txtLabelledRoot.Margin = new System.Windows.Forms.Padding(2);
            this.txtLabelledRoot.Name = "txtLabelledRoot";
            this.txtLabelledRoot.Size = new System.Drawing.Size(285, 20);
            this.txtLabelledRoot.TabIndex = 7;
            this.txtLabelledRoot.Text = "..\\..\\..\\TrainingData";
            // 
            // lblLabelledRoot
            // 
            this.lblLabelledRoot.AutoSize = true;
            this.lblLabelledRoot.Location = new System.Drawing.Point(602, 201);
            this.lblLabelledRoot.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblLabelledRoot.Name = "lblLabelledRoot";
            this.lblLabelledRoot.Size = new System.Drawing.Size(105, 13);
            this.lblLabelledRoot.TabIndex = 8;
            this.lblLabelledRoot.Text = "Labelled Image Root";
            // 
            // cmdSaveImage
            // 
            this.cmdSaveImage.Location = new System.Drawing.Point(602, 378);
            this.cmdSaveImage.Margin = new System.Windows.Forms.Padding(2);
            this.cmdSaveImage.Name = "cmdSaveImage";
            this.cmdSaveImage.Size = new System.Drawing.Size(86, 48);
            this.cmdSaveImage.TabIndex = 9;
            this.cmdSaveImage.Text = "Save Labelled Image";
            this.cmdSaveImage.UseVisualStyleBackColor = true;
            this.cmdSaveImage.Click += new System.EventHandler(this.cmdSaveImage_Click);
            // 
            // chkHasSign
            // 
            this.chkHasSign.AutoSize = true;
            this.chkHasSign.Location = new System.Drawing.Point(602, 257);
            this.chkHasSign.Margin = new System.Windows.Forms.Padding(2);
            this.chkHasSign.Name = "chkHasSign";
            this.chkHasSign.Size = new System.Drawing.Size(69, 17);
            this.chkHasSign.TabIndex = 10;
            this.chkHasSign.Text = "Has Sign";
            this.chkHasSign.UseVisualStyleBackColor = true;
            // 
            // lblXminLabel
            // 
            this.lblXminLabel.AutoSize = true;
            this.lblXminLabel.Location = new System.Drawing.Point(602, 279);
            this.lblXminLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblXminLabel.Name = "lblXminLabel";
            this.lblXminLabel.Size = new System.Drawing.Size(34, 13);
            this.lblXminLabel.TabIndex = 11;
            this.lblXminLabel.Text = "X Min";
            // 
            // lblXmin
            // 
            this.lblXmin.AutoSize = true;
            this.lblXmin.Location = new System.Drawing.Point(640, 279);
            this.lblXmin.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblXmin.Name = "lblXmin";
            this.lblXmin.Size = new System.Drawing.Size(13, 13);
            this.lblXmin.TabIndex = 11;
            this.lblXmin.Text = "0";
            // 
            // lblYmin
            // 
            this.lblYmin.AutoSize = true;
            this.lblYmin.Location = new System.Drawing.Point(640, 299);
            this.lblYmin.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblYmin.Name = "lblYmin";
            this.lblYmin.Size = new System.Drawing.Size(13, 13);
            this.lblYmin.TabIndex = 12;
            this.lblYmin.Text = "0";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(602, 299);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(34, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "Y Min";
            // 
            // lblXmax
            // 
            this.lblXmax.AutoSize = true;
            this.lblXmax.Location = new System.Drawing.Point(640, 321);
            this.lblXmax.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblXmax.Name = "lblXmax";
            this.lblXmax.Size = new System.Drawing.Size(13, 13);
            this.lblXmax.TabIndex = 14;
            this.lblXmax.Text = "0";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(602, 321);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(37, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "X Max";
            // 
            // lblYmax
            // 
            this.lblYmax.AutoSize = true;
            this.lblYmax.Location = new System.Drawing.Point(640, 342);
            this.lblYmax.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblYmax.Name = "lblYmax";
            this.lblYmax.Size = new System.Drawing.Size(13, 13);
            this.lblYmax.TabIndex = 16;
            this.lblYmax.Text = "0";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(602, 342);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(37, 13);
            this.label6.TabIndex = 17;
            this.label6.Text = "Y Max";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(602, 432);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(10, 13);
            this.lblStatus.TabIndex = 18;
            this.lblStatus.Text = "-";
            // 
            // lblFilename
            // 
            this.lblFilename.AutoSize = true;
            this.lblFilename.Location = new System.Drawing.Point(605, 114);
            this.lblFilename.Name = "lblFilename";
            this.lblFilename.Size = new System.Drawing.Size(10, 13);
            this.lblFilename.TabIndex = 19;
            this.lblFilename.Text = "-";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(903, 622);
            this.Controls.Add(this.lblFilename);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblYmax);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.lblXmax);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lblYmin);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblXmin);
            this.Controls.Add(this.lblXminLabel);
            this.Controls.Add(this.chkHasSign);
            this.Controls.Add(this.cmdSaveImage);
            this.Controls.Add(this.lblLabelledRoot);
            this.Controls.Add(this.txtLabelledRoot);
            this.Controls.Add(this.lblCurrent);
            this.Controls.Add(this.cmdNextImage);
            this.Controls.Add(this.lblImageCount);
            this.Controls.Add(this.lblImageDirectory);
            this.Controls.Add(this.txtImageDirectory);
            this.Controls.Add(this.picCurrent);
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form1";
            this.Text = "Form1";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.picCurrent)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picCurrent;
        private System.Windows.Forms.TextBox txtImageDirectory;
        private System.Windows.Forms.Label lblImageDirectory;
        private System.Windows.Forms.Label lblImageCount;
        private System.Windows.Forms.Button cmdNextImage;
        private System.Windows.Forms.Label lblCurrent;
        private System.Windows.Forms.TextBox txtLabelledRoot;
        private System.Windows.Forms.Label lblLabelledRoot;
        private System.Windows.Forms.Button cmdSaveImage;
        private System.Windows.Forms.CheckBox chkHasSign;
        private System.Windows.Forms.Label lblXminLabel;
        private System.Windows.Forms.Label lblXmin;
        private System.Windows.Forms.Label lblYmin;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblXmax;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblYmax;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblFilename;
    }
}

