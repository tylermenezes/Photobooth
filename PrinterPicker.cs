using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Printing;

namespace SnapShot
{
    public partial class PrinterPicker : Form
    {
        public string PrinterName
        {
            get
            {
                return comboBox1.Text;
            }
        }
        public PrinterPicker()
        {
            InitializeComponent();
        }

        private void PrinterPicker_Load(object sender, EventArgs e)
        {
            PrintDocument prtdoc = new PrintDocument();
            string strDefaultPrinter = prtdoc.PrinterSettings.PrinterName;
            foreach (String strPrinter in PrinterSettings.InstalledPrinters)
            {
                comboBox1.Items.Add(strPrinter);
                if (strPrinter == strDefaultPrinter)
                {
                    comboBox1.SelectedIndex = comboBox1.Items.IndexOf(strPrinter);
                }
            }

        }

        private void okayButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
