using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace P2P
{
    public partial class AddNode : Form
    {
        public DHT HT { get; set; }
        public AddNode(DHT H)
        {
            InitializeComponent();
            HT = H;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string key = textBox1.Text;
            string value = textBox2.Text;
            if(HT.FindKey(key))
            {
                MessageBox.Show("Key already Exists");
            }
            else
            {
                HT.Add(key, value);
                MessageBox.Show("Key Added");
                this.Close();
            }
            
        }
    }
}
