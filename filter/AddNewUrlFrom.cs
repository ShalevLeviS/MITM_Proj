using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSNA
{
    public partial class AddNewUrlFrom : Form
    {
        public AddNewUrlFrom()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string badSite = textBox1.Text.ToString();
            if (!badSite.Contains("www.") && !badSite.Contains(".com"))
            {
                File.AppendAllText("G:\\avoda\\CSNA COPY\\CSNA\\Properties\\Url Blacklist.txt", "$" + badSite + "$");
                MessageBox.Show(textBox1.Text + " succsesfully added");
                textBox1.Text = "";
                this.Visible = false;
            }
            else
            {
                MessageBox.Show("Enter ONLY site name (Ex : 'Google' ");
                textBox1.Text = "";
            }
            
            
        }

        private void AddNewUrlFrom_Load(object sender, EventArgs e)
        {

        }
    }
}
