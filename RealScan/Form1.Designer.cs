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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            MsgPanel = new System.Windows.Forms.TextBox();
            PreviewWindow = new System.Windows.Forms.PictureBox();
            groupCapture = new System.Windows.Forms.GroupBox();
            StopCapture = new System.Windows.Forms.Button();
            StartCapture = new System.Windows.Forms.Button();
            CaptureMode = new System.Windows.Forms.ComboBox();
            callbackDrawBtn = new System.Windows.Forms.RadioButton();
            missinFingers = new System.Windows.Forms.GroupBox();
            rightFingers = new System.Windows.Forms.CheckedListBox();
            leftFingers = new System.Windows.Forms.CheckedListBox();
            label2 = new System.Windows.Forms.Label();
            DeviceInfo = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)PreviewWindow).BeginInit();
            groupCapture.SuspendLayout();
            missinFingers.SuspendLayout();
            SuspendLayout();
            // 
            // MsgPanel
            // 
            resources.ApplyResources(MsgPanel, "MsgPanel");
            MsgPanel.Name = "MsgPanel";
            MsgPanel.ReadOnly = true;
            // 
            // PreviewWindow
            // 
            PreviewWindow.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(PreviewWindow, "PreviewWindow");
            PreviewWindow.Name = "PreviewWindow";
            PreviewWindow.TabStop = false;
            // 
            // groupCapture
            //
            groupCapture.Controls.Add(StopCapture);
            groupCapture.Controls.Add(StartCapture);
            groupCapture.Controls.Add(CaptureMode);
            resources.ApplyResources(groupCapture, "groupCapture");
            groupCapture.Name = "groupCapture";
            groupCapture.TabStop = false;
            // 
            // StopCapture
            // 
            resources.ApplyResources(StopCapture, "StopCapture");
            StopCapture.Name = "StopCapture";
            StopCapture.UseVisualStyleBackColor = true;
            StopCapture.Click += StopCapture_Click;
            // 
            // StartCapture
            // 
            resources.ApplyResources(StartCapture, "StartCapture");
            StartCapture.Name = "StartCapture";
            StartCapture.UseVisualStyleBackColor = true;
            StartCapture.Click += StartCapture_Click;
            // 
            // CaptureMode
            // 
            CaptureMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            CaptureMode.FormattingEnabled = true;
            CaptureMode.Items.AddRange(new object[] { resources.GetString("CaptureMode.Items"), resources.GetString("CaptureMode.Items1"), resources.GetString("CaptureMode.Items2"), resources.GetString("CaptureMode.Items3") });
            resources.ApplyResources(CaptureMode, "CaptureMode");
            CaptureMode.Name = "CaptureMode";
            CaptureMode.SelectedIndexChanged += CaptureMode_SelectedIndexChanged;
            // 
            // callbackDrawBtn
            // 
            resources.ApplyResources(callbackDrawBtn, "callbackDrawBtn");
            callbackDrawBtn.Name = "callbackDrawBtn";
            callbackDrawBtn.TabStop = true;
            callbackDrawBtn.UseVisualStyleBackColor = true;
            // 
            // missinFingers
            // 
            missinFingers.Controls.Add(rightFingers);
            missinFingers.Controls.Add(leftFingers);
            resources.ApplyResources(missinFingers, "missinFingers");
            missinFingers.Name = "missinFingers";
            missinFingers.TabStop = false;
            // 
            // rightFingers
            // 
            rightFingers.FormattingEnabled = true;
            rightFingers.Items.AddRange(new object[] { resources.GetString("rightFingers.Items"), resources.GetString("rightFingers.Items1"), resources.GetString("rightFingers.Items2"), resources.GetString("rightFingers.Items3") });
            resources.ApplyResources(rightFingers, "rightFingers");
            rightFingers.Name = "rightFingers";
            // 
            // leftFingers
            // 
            leftFingers.BackColor = System.Drawing.SystemColors.Window;
            leftFingers.FormattingEnabled = true;
            leftFingers.Items.AddRange(new object[] { resources.GetString("leftFingers.Items"), resources.GetString("leftFingers.Items1"), resources.GetString("leftFingers.Items2"), resources.GetString("leftFingers.Items3") });
            resources.ApplyResources(leftFingers, "leftFingers");
            leftFingers.Name = "leftFingers";
            // 
            // label2
            // 
            resources.ApplyResources(label2, "label2");
            label2.Name = "label2";
            // 
            // DeviceInfo
            // 
            resources.ApplyResources(DeviceInfo, "DeviceInfo");
            DeviceInfo.Name = "DeviceInfo";
            DeviceInfo.ReadOnly = true;
            // 
            // MainForm
            // 
            resources.ApplyResources(this, "$this");
            AccessibleRole = System.Windows.Forms.AccessibleRole.Window;
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.SystemColors.Control;
            Controls.Add(label2);
            Controls.Add(DeviceInfo);
            Controls.Add(missinFingers);
            Controls.Add(groupCapture);
            Controls.Add(PreviewWindow);
            Controls.Add(MsgPanel);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "MainForm";
            ((System.ComponentModel.ISupportInitialize)PreviewWindow).EndInit();
            groupCapture.ResumeLayout(false);
            missinFingers.ResumeLayout(false);
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
        private System.Windows.Forms.GroupBox missinFingers;
        private System.Windows.Forms.CheckedListBox rightFingers;
        private System.Windows.Forms.CheckedListBox leftFingers;
        private System.Windows.Forms.RadioButton callbackDrawBtn;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox DeviceInfo;
    }
}

