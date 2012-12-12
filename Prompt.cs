using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SnapShot
{
    public partial class Prompt : Form
    {
        public string Result
        {
            get
            {
                return result.Text;
            }
        }

        public Prompt(string text)
        {
            InitializeComponent();
            label.Text = text;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
