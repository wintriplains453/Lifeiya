﻿namespace SignaliEdge
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
            this.fdIzborSlike = new System.Windows.Forms.OpenFileDialog();
            this.trbLower = new System.Windows.Forms.TrackBar();
            this.trbUpper = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnFileDialog = new System.Windows.Forms.Button();
            this.btnDetect = new System.Windows.Forms.Button();
            this.lblLowerTreshold = new System.Windows.Forms.Label();
            this.lblUpperTreshold = new System.Windows.Forms.Label();
            this.cbCalcTresholds = new System.Windows.Forms.CheckBox();
            this.trbPrecision = new System.Windows.Forms.TrackBar();
            this.lblPrecisionText = new System.Windows.Forms.Label();
            this.lblPrecision = new System.Windows.Forms.Label();
            this.cbMaxPrecision = new System.Windows.Forms.CheckBox();
            this.ssInfo = new System.Windows.Forms.StatusStrip();
            this.lblImageText = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblImageResolution = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblImageSize = new System.Windows.Forms.ToolStripStatusLabel();
            this.spring = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblLastDetectionText = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblLastDetection = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblS = new System.Windows.Forms.ToolStripStatusLabel();
            this.button1 = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.button2 = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.pbSlikaOriginal = new System.Windows.Forms.PictureBox();
            this.pbSlika = new System.Windows.Forms.PictureBox();
            this.button3 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.trbLower)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trbUpper)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trbPrecision)).BeginInit();
            this.ssInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbSlikaOriginal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbSlika)).BeginInit();
            this.SuspendLayout();
            // 
            // fdIzborSlike
            // 
            this.fdIzborSlike.FileName = "openFileDialog1";
            // 
            // trbLower
            // 
            this.trbLower.Location = new System.Drawing.Point(13, 155);
            this.trbLower.Maximum = 100;
            this.trbLower.Name = "trbLower";
            this.trbLower.Size = new System.Drawing.Size(272, 45);
            this.trbLower.TabIndex = 1;
            this.trbLower.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            this.trbLower.Scroll += new System.EventHandler(this.trbLower_Scroll);
            // 
            // trbUpper
            // 
            this.trbUpper.Location = new System.Drawing.Point(336, 155);
            this.trbUpper.Maximum = 100;
            this.trbUpper.Name = "trbUpper";
            this.trbUpper.Size = new System.Drawing.Size(290, 45);
            this.trbUpper.TabIndex = 3;
            this.trbUpper.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            this.trbUpper.Scroll += new System.EventHandler(this.trbUpper_Scroll);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 130);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Lower Threshold";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(335, 130);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Upper Treshold";
            // 
            // btnFileDialog
            // 
            this.btnFileDialog.Location = new System.Drawing.Point(16, 13);
            this.btnFileDialog.Name = "btnFileDialog";
            this.btnFileDialog.Size = new System.Drawing.Size(142, 23);
            this.btnFileDialog.TabIndex = 6;
            this.btnFileDialog.Text = "Choose Image";
            this.btnFileDialog.UseVisualStyleBackColor = true;
            this.btnFileDialog.Click += new System.EventHandler(this.btnFileDialog_Click);
            // 
            // btnDetect
            // 
            this.btnDetect.Location = new System.Drawing.Point(177, 13);
            this.btnDetect.Name = "btnDetect";
            this.btnDetect.Size = new System.Drawing.Size(149, 23);
            this.btnDetect.TabIndex = 7;
            this.btnDetect.Text = "Detect Edges";
            this.btnDetect.UseVisualStyleBackColor = true;
            this.btnDetect.Click += new System.EventHandler(this.btnDetect_Click);
            // 
            // lblLowerTreshold
            // 
            this.lblLowerTreshold.AutoSize = true;
            this.lblLowerTreshold.Location = new System.Drawing.Point(291, 155);
            this.lblLowerTreshold.Name = "lblLowerTreshold";
            this.lblLowerTreshold.Size = new System.Drawing.Size(22, 13);
            this.lblLowerTreshold.TabIndex = 8;
            this.lblLowerTreshold.Text = "0.0";
            // 
            // lblUpperTreshold
            // 
            this.lblUpperTreshold.AutoSize = true;
            this.lblUpperTreshold.Location = new System.Drawing.Point(632, 155);
            this.lblUpperTreshold.Name = "lblUpperTreshold";
            this.lblUpperTreshold.Size = new System.Drawing.Size(22, 13);
            this.lblUpperTreshold.TabIndex = 9;
            this.lblUpperTreshold.Text = "0.0";
            // 
            // cbCalcTresholds
            // 
            this.cbCalcTresholds.AutoSize = true;
            this.cbCalcTresholds.Location = new System.Drawing.Point(333, 60);
            this.cbCalcTresholds.Name = "cbCalcTresholds";
            this.cbCalcTresholds.Size = new System.Drawing.Size(172, 17);
            this.cbCalcTresholds.TabIndex = 10;
            this.cbCalcTresholds.Text = "Automatic Treshold Calculation";
            this.cbCalcTresholds.UseVisualStyleBackColor = true;
            this.cbCalcTresholds.CheckedChanged += new System.EventHandler(this.cbCalcTresholds_CheckedChanged);
            // 
            // trbPrecision
            // 
            this.trbPrecision.Location = new System.Drawing.Point(13, 83);
            this.trbPrecision.Maximum = 100;
            this.trbPrecision.Minimum = 1;
            this.trbPrecision.Name = "trbPrecision";
            this.trbPrecision.Size = new System.Drawing.Size(273, 45);
            this.trbPrecision.TabIndex = 11;
            this.trbPrecision.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            this.trbPrecision.Value = 50;
            this.trbPrecision.Scroll += new System.EventHandler(this.trbPrecision_Scroll);
            // 
            // lblPrecisionText
            // 
            this.lblPrecisionText.AutoSize = true;
            this.lblPrecisionText.Location = new System.Drawing.Point(22, 64);
            this.lblPrecisionText.Name = "lblPrecisionText";
            this.lblPrecisionText.Size = new System.Drawing.Size(271, 13);
            this.lblPrecisionText.TabIndex = 12;
            this.lblPrecisionText.Text = "Edge Detection Precision (higher values take more time)";
            // 
            // lblPrecision
            // 
            this.lblPrecision.AutoSize = true;
            this.lblPrecision.Location = new System.Drawing.Point(293, 83);
            this.lblPrecision.Name = "lblPrecision";
            this.lblPrecision.Size = new System.Drawing.Size(19, 13);
            this.lblPrecision.TabIndex = 13;
            this.lblPrecision.Text = "50";
            // 
            // cbMaxPrecision
            // 
            this.cbMaxPrecision.AutoSize = true;
            this.cbMaxPrecision.Location = new System.Drawing.Point(333, 83);
            this.cbMaxPrecision.Name = "cbMaxPrecision";
            this.cbMaxPrecision.Size = new System.Drawing.Size(265, 17);
            this.cbMaxPrecision.TabIndex = 14;
            this.cbMaxPrecision.Text = "Maximum Precision - Find all the Lines you can find";
            this.cbMaxPrecision.UseVisualStyleBackColor = true;
            this.cbMaxPrecision.CheckedChanged += new System.EventHandler(this.cbMaxPrecision_CheckedChanged);
            // 
            // ssInfo
            // 
            this.ssInfo.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblImageText,
            this.lblImageResolution,
            this.lblImageSize,
            this.spring,
            this.lblLastDetectionText,
            this.lblLastDetection,
            this.lblS});
            this.ssInfo.Location = new System.Drawing.Point(0, 518);
            this.ssInfo.Name = "ssInfo";
            this.ssInfo.ShowItemToolTips = true;
            this.ssInfo.Size = new System.Drawing.Size(851, 22);
            this.ssInfo.TabIndex = 15;
            this.ssInfo.Text = "Info";
            // 
            // lblImageText
            // 
            this.lblImageText.Name = "lblImageText";
            this.lblImageText.Size = new System.Drawing.Size(43, 17);
            this.lblImageText.Text = "Image:";
            // 
            // lblImageResolution
            // 
            this.lblImageResolution.Name = "lblImageResolution";
            this.lblImageResolution.Size = new System.Drawing.Size(25, 17);
            this.lblImageResolution.Text = "0x0";
            // 
            // lblImageSize
            // 
            this.lblImageSize.Name = "lblImageSize";
            this.lblImageSize.Size = new System.Drawing.Size(34, 17);
            this.lblImageSize.Text = "0 MB";
            // 
            // spring
            // 
            this.spring.Name = "spring";
            this.spring.Size = new System.Drawing.Size(597, 17);
            this.spring.Spring = true;
            // 
            // lblLastDetectionText
            // 
            this.lblLastDetectionText.Name = "lblLastDetectionText";
            this.lblLastDetectionText.Size = new System.Drawing.Size(112, 17);
            this.lblLastDetectionText.Text = "Last Detection took:";
            this.lblLastDetectionText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblLastDetection
            // 
            this.lblLastDetection.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.lblLastDetection.Name = "lblLastDetection";
            this.lblLastDetection.Size = new System.Drawing.Size(13, 17);
            this.lblLastDetection.Text = "0";
            this.lblLastDetection.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblS
            // 
            this.lblS.Name = "lblS";
            this.lblS.Size = new System.Drawing.Size(12, 17);
            this.lblS.Text = "s";
            this.lblS.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(333, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 16;
            this.button1.Text = "Add Image";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(659, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(176, 173);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 17;
            this.pictureBox1.TabStop = false;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(414, 12);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 18;
            this.button2.Text = "Read Text";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(333, 107);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(86, 17);
            this.checkBox1.TabIndex = 19;
            this.checkBox1.Text = "Binary image";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer1.Location = new System.Drawing.Point(8, 229);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.pbSlikaOriginal);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.pbSlika);
            this.splitContainer1.Size = new System.Drawing.Size(827, 286);
            this.splitContainer1.SplitterDistance = 414;
            this.splitContainer1.TabIndex = 21;
            // 
            // pbSlikaOriginal
            // 
            this.pbSlikaOriginal.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbSlikaOriginal.InitialImage = null;
            this.pbSlikaOriginal.Location = new System.Drawing.Point(3, 3);
            this.pbSlikaOriginal.Name = "pbSlikaOriginal";
            this.pbSlikaOriginal.Size = new System.Drawing.Size(406, 278);
            this.pbSlikaOriginal.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbSlikaOriginal.TabIndex = 0;
            this.pbSlikaOriginal.TabStop = false;
            // 
            // pbSlika
            // 
            this.pbSlika.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbSlika.InitialImage = null;
            this.pbSlika.Location = new System.Drawing.Point(3, -1);
            this.pbSlika.Name = "pbSlika";
            this.pbSlika.Size = new System.Drawing.Size(401, 281);
            this.pbSlika.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbSlika.TabIndex = 0;
            this.pbSlika.TabStop = false;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(496, 12);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(102, 23);
            this.button3.TabIndex = 22;
            this.button3.Text = "Default Zoom";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(851, 540);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.ssInfo);
            this.Controls.Add(this.cbMaxPrecision);
            this.Controls.Add(this.lblPrecision);
            this.Controls.Add(this.lblPrecisionText);
            this.Controls.Add(this.trbPrecision);
            this.Controls.Add(this.cbCalcTresholds);
            this.Controls.Add(this.lblUpperTreshold);
            this.Controls.Add(this.lblLowerTreshold);
            this.Controls.Add(this.btnDetect);
            this.Controls.Add(this.btnFileDialog);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.trbUpper);
            this.Controls.Add(this.trbLower);
            this.Name = "Form1";
            this.Text = "Edge Detector";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.trbLower)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trbUpper)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trbPrecision)).EndInit();
            this.ssInfo.ResumeLayout(false);
            this.ssInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbSlikaOriginal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbSlika)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.OpenFileDialog fdIzborSlike;
        private System.Windows.Forms.TrackBar trbLower;
        private System.Windows.Forms.TrackBar trbUpper;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnFileDialog;
        private System.Windows.Forms.Button btnDetect;
        private System.Windows.Forms.Label lblLowerTreshold;
        private System.Windows.Forms.Label lblUpperTreshold;
        private System.Windows.Forms.CheckBox cbCalcTresholds;
        private System.Windows.Forms.TrackBar trbPrecision;
        private System.Windows.Forms.Label lblPrecisionText;
        private System.Windows.Forms.Label lblPrecision;
        private System.Windows.Forms.CheckBox cbMaxPrecision;
        private System.Windows.Forms.StatusStrip ssInfo;
        private System.Windows.Forms.ToolStripStatusLabel lblLastDetectionText;
        private System.Windows.Forms.ToolStripStatusLabel lblLastDetection;
        private System.Windows.Forms.ToolStripStatusLabel lblS;
        private System.Windows.Forms.ToolStripStatusLabel spring;
        private System.Windows.Forms.ToolStripStatusLabel lblImageText;
        private System.Windows.Forms.ToolStripStatusLabel lblImageResolution;
        private System.Windows.Forms.ToolStripStatusLabel lblImageSize;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.PictureBox pbSlikaOriginal;
        private System.Windows.Forms.PictureBox pbSlika;
        private System.Windows.Forms.Button button3;
    }
}

