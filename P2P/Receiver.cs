using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO;
namespace P2P
{
    public partial class Receiver : Form
    {
        //tcp port
        const int PORT = 1723;
        public DHT HT { get; set; }

        public string folderName;
        public Receiver(DHT H)
        {
            InitializeComponent();
            HT = H;
            folderName = "";
        }

        private void resetControls()
        {
            progressBar1.Style = ProgressBarStyle.Marquee;
            textBox1.Text = "Waiting to connect to Sender";
        }

        protected override async void OnShown(EventArgs e)
        {
            while (true)
            {
                try
                {
                    TcpListener listener = TcpListener.Create(PORT);
                    listener.Start();
                    textBox1.Text = "Waiting for connection";

                    TcpClient client = await listener.AcceptTcpClientAsync();
                    NetworkStream ns = client.GetStream();
                    textBox1.Text = "Client connected. Starting to receive the file";
                    long fileLength;
                    string fileName;
                    byte[] fileNameBytes;
                    byte[] fileNameLengthBytes = new byte[4]; //int32
                    byte[] fileLengthBytes = new byte[8]; //int64

                    await ns.ReadAsync(fileLengthBytes, 0, 8); // int64
                    await ns.ReadAsync(fileNameLengthBytes, 0, 4); // int32
                    fileNameBytes = new byte[BitConverter.ToInt32(fileNameLengthBytes, 0)];
                    await ns.ReadAsync(fileNameBytes, 0, fileNameBytes.Length);

                    fileLength = BitConverter.ToInt64(fileLengthBytes, 0);
                    fileName = ASCIIEncoding.ASCII.GetString(fileNameBytes);

                    textBox1.Text = "Receiving...";
                    progressBar1.Style = ProgressBarStyle.Continuous;
                    progressBar1.Value = 0;
                    int read;
                    int totalRead = 0;
                    byte[] buffer = new byte[32 * 1024];
                    FileStream fs = File.Open(folderName + fileName, FileMode.Create);

                    while ((read = await ns.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fs.WriteAsync(buffer, 0, read);
                        totalRead += read;
                        progressBar1.Value = (int)((100d * totalRead) / fileLength);
                    }

                    fs.Dispose();
                    client.Close();
                    resetControls();
                }
                catch (Exception E)
                {

                    MessageBox.Show(E.ToString());
                }
            }
        }

        /*
        protected override async void OnShown(EventArgs e)
        {
            // Listen
            TcpListener listener = TcpListener.Create(PORT);
            listener.Start();
            textBox1.Text = "Waiting for connection...";
            TcpClient client = await listener.AcceptTcpClientAsync();
            NetworkStream ns = client.GetStream();

            // Get file info
            long fileLength;
            string fileName;
            {
                byte[] fileNameBytes;
                byte[] fileNameLengthBytes = new byte[4]; //int32
                byte[] fileLengthBytes = new byte[8]; //int64

                await ns.ReadAsync(fileLengthBytes, 0, 8); // int64
                await ns.ReadAsync(fileNameLengthBytes, 0, 4); // int32
                fileNameBytes = new byte[BitConverter.ToInt32(fileNameLengthBytes, 0)];
                await ns.ReadAsync(fileNameBytes, 0, fileNameBytes.Length);

                fileLength = BitConverter.ToInt64(fileLengthBytes, 0);
                fileName = ASCIIEncoding.ASCII.GetString(fileNameBytes);
            }

            // Get permission
            if (MessageBox.Show(String.Format("Requesting permission to receive file:\r\n\r\n{0}\r\n{1} bytes long", fileName, fileLength), "", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }

            // Set save location
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.CreatePrompt = false;
            sfd.OverwritePrompt = true;
            sfd.FileName = fileName;
            if (sfd.ShowDialog() != DialogResult.OK)
            {
                ns.WriteByte(0); // Permission denied
                return;
            }
            ns.WriteByte(1); // Permission grantedd
            FileStream fileStream = File.Open(sfd.FileName, FileMode.Create);

            // Receive
            textBox1.Text = "Receiving...";
            progressBar1.Style = ProgressBarStyle.Continuous;
            progressBar1.Value = 0;
            int read;
            int totalRead = 0;
            byte[] buffer = new byte[32 * 1024]; // 32k chunks
            while ((read = await ns.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, read);
                totalRead += read;
                progressBar1.Value = (int)((100d * totalRead) / fileLength);
            }

            fileStream.Dispose();
            client.Close();
            MessageBox.Show("File successfully received");
            resetControls();
        }
        */
    }
}
