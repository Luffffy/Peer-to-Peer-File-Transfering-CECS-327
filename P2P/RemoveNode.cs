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
    public partial class RemoveNode : Form
    {
        public DHT HT { get; set; }
        public RemoveNode(DHT H)
        {
            InitializeComponent();
            HT = H;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string key = textBox1.Text;
            //Nothing entered
            if (key == "")
            {
                MessageBox.Show("Enter in a key to delete the value");
            }
            else
            {
                //if key exists remove
                if (HT.Remove(key))
                {
                    MessageBox.Show("Key and Value Successfully Removed");
                    this.Close();
                }
                else
                    MessageBox.Show("Key does not exist");
            }
        }
    }
}
