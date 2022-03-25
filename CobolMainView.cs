using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace COBOL.NET
{
    public partial class CobolMainView : Form
    {
        public CobolMainView()
        {
            InitializeComponent();
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            CobolLearnMore help = new CobolLearnMore();
            help.Show();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FolderBrowserDialog fBD = new FolderBrowserDialog();
            if(fBD.ShowDialog() == DialogResult.OK)
            {
                ProjectForm project = new ProjectForm();
                project.ProjectName = fBD.SelectedPath;
                project.Show();
            }
        }
    }
}
