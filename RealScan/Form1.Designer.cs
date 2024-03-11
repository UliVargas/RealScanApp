namespace RealScanUICSharp
{
    partial class MainForm
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
            MsgPanel = new System.Windows.Forms.TextBox();
            PreviewWindow = new System.Windows.Forms.PictureBox();
            groupCapture = new System.Windows.Forms.GroupBox();
            StopCapture = new System.Windows.Forms.Button();
            StartCapture = new System.Windows.Forms.Button();
            CaptureMode = new System.Windows.Forms.ComboBox();
            callbackDrawBtn = new System.Windows.Forms.RadioButton();
            groupBox3 = new System.Windows.Forms.GroupBox();
            rightFingers = new System.Windows.Forms.CheckedListBox();
            leftFingers = new System.Windows.Forms.CheckedListBox();
            label2 = new System.Windows.Forms.Label();
            DeviceInfo = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)PreviewWindow).BeginInit();
            groupCapture.SuspendLayout();
            groupBox3.SuspendLayout();
            SuspendLayout();
            // 
            // MsgPanel
            // 
            MsgPanel.Font = new System.Drawing.Font("Arial", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            MsgPanel.Location = new System.Drawing.Point(12, 15);
            MsgPanel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            MsgPanel.Name = "MsgPanel";
            MsgPanel.ReadOnly = true;
            MsgPanel.Size = new System.Drawing.Size(706, 39);
            MsgPanel.TabIndex = 1;
            MsgPanel.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // PreviewWindow
            // 
            PreviewWindow.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            PreviewWindow.Location = new System.Drawing.Point(12, 82);
            PreviewWindow.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            PreviewWindow.Name = "PreviewWindow";
            PreviewWindow.Size = new System.Drawing.Size(471, 540);
            PreviewWindow.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            PreviewWindow.TabIndex = 2;
            PreviewWindow.TabStop = false;
            // 
            // groupCapture
            // 
            groupCapture.Controls.Add(StopCapture);
            groupCapture.Controls.Add(StartCapture);
            groupCapture.Controls.Add(CaptureMode);
            groupCapture.Location = new System.Drawing.Point(504, 131);
            groupCapture.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            groupCapture.Name = "groupCapture";
            groupCapture.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            groupCapture.Size = new System.Drawing.Size(241, 147);
            groupCapture.TabIndex = 4;
            groupCapture.TabStop = false;
            groupCapture.Text = "Opciones de Captura";
            // 
            // StopCapture
            // 
            StopCapture.Enabled = false;
            StopCapture.Location = new System.Drawing.Point(128, 71);
            StopCapture.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            StopCapture.Name = "StopCapture";
            StopCapture.Size = new System.Drawing.Size(107, 42);
            StopCapture.TabIndex = 8;
            StopCapture.Text = "Detener Captura";
            StopCapture.UseVisualStyleBackColor = true;
            StopCapture.Click += StopCapture_Click;
            // 
            // StartCapture
            // 
            StartCapture.Enabled = false;
            StartCapture.Location = new System.Drawing.Point(15, 71);
            StartCapture.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            StartCapture.Name = "StartCapture";
            StartCapture.Size = new System.Drawing.Size(107, 42);
            StartCapture.TabIndex = 6;
            StartCapture.Text = "Iniciar Captura";
            StartCapture.UseVisualStyleBackColor = true;
            StartCapture.Click += StartCapture_Click;
            // 
            // CaptureMode
            // 
            CaptureMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            CaptureMode.FormattingEnabled = true;
            CaptureMode.Items.AddRange(new object[] { "Seleccione el modo de captura", "Plana 4 dedos(Izquierda)", "Plana 4 dedos(Derecha)", "Plana Pulgares" });
            CaptureMode.Location = new System.Drawing.Point(15, 22);
            CaptureMode.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            CaptureMode.Name = "CaptureMode";
            CaptureMode.Size = new System.Drawing.Size(220, 23);
            CaptureMode.TabIndex = 6;
            CaptureMode.SelectedIndexChanged += CaptureMode_SelectedIndexChanged;
            // 
            // callbackDrawBtn
            // 
            callbackDrawBtn.AutoSize = true;
            callbackDrawBtn.Location = new System.Drawing.Point(68, 26);
            callbackDrawBtn.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            callbackDrawBtn.Name = "callbackDrawBtn";
            callbackDrawBtn.Size = new System.Drawing.Size(70, 19);
            callbackDrawBtn.TabIndex = 5;
            callbackDrawBtn.TabStop = true;
            callbackDrawBtn.Text = "Callback";
            callbackDrawBtn.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(rightFingers);
            groupBox3.Controls.Add(leftFingers);
            groupBox3.Location = new System.Drawing.Point(504, 310);
            groupBox3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            groupBox3.Name = "groupBox3";
            groupBox3.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            groupBox3.Size = new System.Drawing.Size(241, 261);
            groupBox3.TabIndex = 1;
            groupBox3.TabStop = false;
            groupBox3.Text = "Dedos faltantes";
            // 
            // rightFingers
            // 
            rightFingers.FormattingEnabled = true;
            rightFingers.Items.AddRange(new object[] { "Meñique derecho", "Anular derecho", "Medio derecho", "Indice derecho" });
            rightFingers.Location = new System.Drawing.Point(15, 148);
            rightFingers.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            rightFingers.Name = "rightFingers";
            rightFingers.Size = new System.Drawing.Size(220, 76);
            rightFingers.TabIndex = 3;
            // 
            // leftFingers
            // 
            leftFingers.BackColor = System.Drawing.SystemColors.Window;
            leftFingers.FormattingEnabled = true;
            leftFingers.Items.AddRange(new object[] { "Meñique izquierdo", "Anular izquierdo", "Medio izquierdo", "Indice izquierdo" });
            leftFingers.Location = new System.Drawing.Point(15, 40);
            leftFingers.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            leftFingers.Name = "leftFingers";
            leftFingers.Size = new System.Drawing.Size(220, 76);
            leftFingers.TabIndex = 2;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(504, 82);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(41, 15);
            label2.TabIndex = 20;
            label2.Text = "Model";
            // 
            // DeviceInfo
            // 
            DeviceInfo.Location = new System.Drawing.Point(553, 78);
            DeviceInfo.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            DeviceInfo.Name = "DeviceInfo";
            DeviceInfo.ReadOnly = true;
            DeviceInfo.Size = new System.Drawing.Size(186, 23);
            DeviceInfo.TabIndex = 2;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(760, 645);
            Controls.Add(label2);
            Controls.Add(DeviceInfo);
            Controls.Add(groupBox3);
            Controls.Add(groupCapture);
            Controls.Add(PreviewWindow);
            Controls.Add(MsgPanel);
            Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            MaximizeBox = false;
            Name = "MainForm";
            ((System.ComponentModel.ISupportInitialize)PreviewWindow).EndInit();
            groupCapture.ResumeLayout(false);
            groupBox3.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.TextBox MsgPanel;
        private System.Windows.Forms.PictureBox PreviewWindow;
        private System.Windows.Forms.GroupBox groupCapture;
        private System.Windows.Forms.Button StartCapture;
        private System.Windows.Forms.ComboBox CaptureMode;
        private System.Windows.Forms.Button StopCapture;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckedListBox rightFingers;
        private System.Windows.Forms.CheckedListBox leftFingers;
        private System.Windows.Forms.RadioButton callbackDrawBtn;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox DeviceInfo;
    }
}

