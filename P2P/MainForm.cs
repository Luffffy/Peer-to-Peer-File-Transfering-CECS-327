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

namespace P2P
{
    //192.168.1.61 / 184
    public partial class MainForm : Form
    {
        DHT HashTable;

        string folderName;
        public MainForm()
        {
            InitializeComponent();
            //DHT size of 10
            HashTable = new DHT(10);
            folderName = "";
            //HashTable.Add("Laptop", "192.168.1.184");
            //HashTable.Add("Desktop", "192.168.1.61");
        }

        /// <summary>
        /// Send files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            if (HashTable.isEmpty())
            {
                MessageBox.Show("Add a Node first!");
            }
            else
            {
                if (folderName.Equals(""))
                {
                    MessageBox.Show("Select a folder first!");
                }
                else
                {
                    var x = new Send(HashTable, folderName);
                    this.Hide();
                    //MessageBox.Show(x.HT.getKeys().ToString());
                    x.ShowDialog();
                    this.Show();
                }
            }

        }
        /// <summary>
        /// Receive files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            if (HashTable.isEmpty())
            {
                MessageBox.Show("Add a Node first!");
            }
            else
            {
                if (folderName.Equals(""))
                {
                    MessageBox.Show("Select a folder first!");
                }
                else
                {
                    var x = new Receive(HashTable, folderName);
                    this.Hide();
                    x.ShowDialog();
                    this.Show();
                }
            }
        }

        /// <summary>
        /// Add Node
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            var x = new AddNode(HashTable);
            this.Hide();
            x.ShowDialog();
            this.Show();
        }

        /// <summary>
        /// Remove Node
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            if (HashTable.isEmpty())
            {
                MessageBox.Show("No Nodes to Delete");
            }
            else
            {
                var x = new RemoveNode(HashTable);
                this.Hide();
                x.ShowDialog();
                this.Show();
            }
        }

        /// <summary>
        /// Select a Folder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    string[] files = Directory.GetFiles(fbd.SelectedPath);
                    folderName = fbd.SelectedPath;
                    button5.Text = fbd.SelectedPath;
                }
            }
        }
    }
}
