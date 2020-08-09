using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace P2P
{
    public partial class Sync : Form
    {
        const int PORT = 1723;
        public DHT HT { get; set; }

        public string folderName;
        public Sync(DHT H, string f)
        {
            InitializeComponent();
            HT = H;
            folderName = f;
        }
        private void resetControls()
        {
            progressBar1.Style = ProgressBarStyle.Marquee;
            textBox1.Text = "Waiting to connect to Sender";
            
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = button2.Enabled = false;
            // Establish the local endpoint  
            // for the socket. Dns.GetHostName 
            // returns the name of the host  
            // running the application. 
            int PORT = 1723;
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ip = ipHost.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ip, PORT);
            

            // Creation TCP/IP Socket using  
            // Socket Class Costructor 
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            string[] filePaths = Directory.GetFiles(folderName);

            try
            {

                button1.Text = "Connecting...";
                //TcpClient client = new TcpClient();
                try
                {
                    //await client.ConnectAsync(ip, PORT);
                    await listener.ConnectAsync(localEndPoint);
                    foreach (var n in filePaths)
                    {


                        listener.SendFile(n);


                        // Close client Socket using the 
                        // Close() method. After closing, 
                        // we can use the closed Socket  
                        // for a new Client Connection 

                        listener.Shutdown(SocketShutdown.Both);
                        listener.Close();
                        MessageBox.Show("Sending complete!");
                        resetControls();
                    }
                }
                catch
                {
                    MessageBox.Show("Error connecting to destination");
                    resetControls();
                    return;
                }

                

            }
            catch (Exception E)
            {
                MessageBox.Show(E.ToString());
            }
            button1.Enabled = button2.Enabled = true;
        }



        private async void button2_Click(object sender, EventArgs e)
        {
            button1.Enabled = button2.Enabled = false;
            textBox1.Text = "Waiting for connection";
            TcpListener listener = new TcpListener(IPAddress.Loopback, 11000);
            listener.Start();

            using (TcpClient client = listener.AcceptTcpClient())
            using (NetworkStream ns = client.GetStream())
            using (FileStream output = File.Create(folderName))
            {
                Console.WriteLine("Client connected. Starting to receive the file");

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
                while ((read = ns.Read(buffer, 0, buffer.Length)) > 0)
                {
                    await output.WriteAsync(buffer, 0, read);
                    totalRead += read;
                    progressBar1.Value = (int)((100d * totalRead) / fileLength);
                }
            }

            resetControls();
            button1.Enabled = button2.Enabled = true;
        }
    }
}
