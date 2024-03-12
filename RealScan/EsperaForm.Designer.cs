namespace RealScan
{
    partial class EsperaForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EsperaForm));
            StatusLabel = new System.Windows.Forms.Label();
            progressBar1 = new System.Windows.Forms.ProgressBar();
            SuspendLayout();
            // 
            // StatusLabel
            // 
            StatusLabel.AutoSize = true;
            StatusLabel.Location = new System.Drawing.Point(12, 41);
            StatusLabel.Name = "StatusLabel";
            StatusLabel.Size = new System.Drawing.Size(378, 15);
            StatusLabel.TabIndex = 0;
            StatusLabel.Text = "Por favor, espera mientras se establece la conexión con el dispositivo...";
            StatusLabel.UseWaitCursor = true;
            // 
            // progressBar1
            // 
            progressBar1.Location = new System.Drawing.Point(12, 88);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new System.Drawing.Size(378, 23);
            progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            progressBar1.TabIndex = 1;
            progressBar1.UseWaitCursor = true;
            // 
            // EsperaForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(402, 153);
            ControlBox = false;
            Controls.Add(progressBar1);
            Controls.Add(StatusLabel);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "EsperaForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Conectando...";
            TopMost = true;
            UseWaitCursor = true;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label StatusLabel;
        private System.Windows.Forms.ProgressBar progressBar1;
    }
}