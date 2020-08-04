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
    public partial class MainForm : Form
    {
        DHT HashTable;

        public MainForm()
        {
            InitializeComponent();
            HashTable = new DHT(10);
            HashTable.Add("1", "2");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (HashTable.isEmpty())
            {
                MessageBox.Show("Add a Node first!");
            }
            else
            {
                var x = new Sender(HashTable);
                this.Hide();
                //MessageBox.Show(x.HT.getKeys().ToString());
                x.ShowDialog();
                this.Show();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (HashTable.isEmpty())
            {
                MessageBox.Show("Add a Node first!");
            }
            else
            {
                var x = new Receiver(HashTable);
                this.Hide();
                x.ShowDialog();
                this.Show();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var x = new AddNode(HashTable);
            this.Hide();
            x.ShowDialog();
            this.Show();            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var x = new RemoveNode(HashTable);
            this.Hide();
            x.ShowDialog();
            this.Show();
        }
    }
}
