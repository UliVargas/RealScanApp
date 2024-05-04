using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RealScan
{
    public partial class EsperaForm : Form
    {
        public EsperaForm()
        {
            InitializeComponent();
        }

        public void UpdateStatusText(string text)
        {
            if (StatusLabel.InvokeRequired)
            {
                StatusLabel.Invoke(new Action(() => StatusLabel.Text = text));
            }
            else
            {
                StatusLabel.Text = text;
            }
        }
    }
}
