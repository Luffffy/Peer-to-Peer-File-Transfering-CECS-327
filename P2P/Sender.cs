using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace P2P
{
    public partial class Sender : Form
    {
        const int PORT = 1723;
        public DHT HT { get; set; }

        public string folderName;
        public Sender(DHT H, string n)
        {
            InitializeComponent();
            HT = H;
            folderName = n;
        }

        void resetControls()
        {
            //textBox1.Enabled = textBox2.Enabled = button1.Enabled = true;
            button1.Text = "Send";
            progressBar1.Value = 0;
            progressBar1.Style = ProgressBarStyle.Continuous;
        }

        void textBox2_Click(object sender, EventArgs e)
        {
           /* OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox2.Text = ofd.FileName;
            }
           */
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    string[] files = Directory.GetFiles(fbd.SelectedPath);
                    folderName = fbd.SelectedPath;
                    textBox2.Text = fbd.SelectedPath;
                    //MessageBox.Show("Files found: " + files.Length.ToString(), "Message");
                }
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
           //textBox1.Enabled = textBox2.Enabled = button1.Enabled = false;
            progressBar1.Style = ProgressBarStyle.Marquee;

            // Parsing
            button1.Text = "Preparing...";
            IPAddress address;
            FileInfo file;
            FileStream fileStream;
            string ip = HT.getValues()[1];
            MessageBox.Show(ip);
            if(ip == null)
            {
                MessageBox.Show("Receiver does not exist");
            }
            if (!IPAddress.TryParse(ip, out address))
            {
                MessageBox.Show("Error with IP Address");
                resetControls();
                return;
            }

            string[] filePaths = Directory.GetFiles(folderName, "", SearchOption.AllDirectories);
            try
            {
                file = new FileInfo(textBox2.Text);
                fileStream = file.OpenRead();
            }
            catch
            {
                MessageBox.Show("Error opening file");
                resetControls();
                return;
            }

            // Connecting
            button1.Text = "Connecting...";
            TcpClient client = new TcpClient();
            try
            {
                await client.ConnectAsync(address, PORT);
            }
            catch
            {
                MessageBox.Show("Error connecting to destination");
                resetControls();
                return;
            }
            NetworkStream ns = client.GetStream();

            // Send file info
            button1.Text = "Sending file info...";
            {   //https://condor.depaul.edu/sjost/nwdp/notes/cs1/CSDatatypes.htm
                byte[] fileName = ASCIIEncoding.ASCII.GetBytes(file.Name);
                byte[] fileNameLength = BitConverter.GetBytes(fileName.Length);
                byte[] fileLength = BitConverter.GetBytes(file.Length);
                await ns.WriteAsync(fileLength, 0, fileLength.Length);
                await ns.WriteAsync(fileNameLength, 0, fileNameLength.Length);
                await ns.WriteAsync(fileName, 0, fileName.Length);
            }

            /*
            // Get permissions
            button1.Text = "Getting permission...";
            {
                byte[] permission = new byte[1];
                await ns.ReadAsync(permission, 0, 1);
                if (permission[0] != 1)
                {
                    MessageBox.Show("Permission denied");
                    resetControls();
                    return;
                }
            }
            */
            // Sending
            button1.Text = "Sending...";
            progressBar1.Style = ProgressBarStyle.Continuous;
            int read;
            int totalWritten = 0;
            byte[] buffer = new byte[32 * 1024]; // 32k chunks
            while ((read = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await ns.WriteAsync(buffer, 0, read);
                totalWritten += read;
                progressBar1.Value = (int)((100d * totalWritten) / file.Length);
            }

            fileStream.Dispose();
            client.Close();
            MessageBox.Show("Sending complete!");
            resetControls();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
