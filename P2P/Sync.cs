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

        }

        private async void button1_Click(object sender, EventArgs e)
        {

            button1.Enabled = button2.Enabled = false;
            progressBar1.Style = ProgressBarStyle.Marquee;
            var IPs = HT.getValues();

            foreach (var i in IPs)
            {
                // Establish the local endpoint  
                // for the socket. Dns.GetHostName 
                // returns the name of the host  
                // running the application. 
                //IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
                //IPAddress ip = ipHost.AddressList[0];
                string ip = i;
                IPAddress address;
                IPAddress.TryParse(ip, out address);
                IPEndPoint localEndPoint = new IPEndPoint(address, PORT);


                // Creation TCP/IP Socket using  
                // Socket Class Costructor 
                Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                string[] filePaths = Directory.GetFiles(folderName);


                try
                {

                    textBox1.Text = "Connecting...";
                    TcpClient client = new TcpClient();
                    try
                    {
                        textBox1.Text = "Sending connecting info...";
                        await client.ConnectAsync(address, PORT);
                        //await listener.ConnectAsync(localEndPoint);
                        foreach (var n in filePaths)
                        {


                            //listener.SendFile(n);
                            textBox1.Text = "Sending connecting Info...";
                            NetworkStream ns = client.GetStream();
                            FileInfo file;
                            FileStream fileStream;
                            file = new FileInfo(n);
                            fileStream = file.OpenRead();

                            // Send file info
                            textBox1.Text = "Sending file info...";
                            {   //https://condor.depaul.edu/sjost/nwdp/notes/cs1/CSDatatypes.htm
                                byte[] fileName = ASCIIEncoding.ASCII.GetBytes(file.Name);
                                byte[] fileNameLength = BitConverter.GetBytes(fileName.Length);
                                byte[] fileLength = BitConverter.GetBytes(file.Length);
                                await ns.WriteAsync(fileLength, 0, fileLength.Length);
                                await ns.WriteAsync(fileNameLength, 0, fileNameLength.Length);
                                await ns.WriteAsync(fileName, 0, fileName.Length);
                            }


                            // Close client Socket using the 
                            // Close() method. After closing, 
                            // we can use the closed Socket  
                            // for a new Client Connection 
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
                        button1.Enabled = button2.Enabled = true;
                        return;
                    }



                }
                catch (Exception E)
                {
                    MessageBox.Show(E.ToString());
                    button1.Enabled = button2.Enabled = true;
                }
            }
            button1.Enabled = button2.Enabled = true;
        }



        private async void button2_Click(object sender, EventArgs e)
        {
            button1.Enabled = button2.Enabled = false;

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


            // Get permission
            if (MessageBox.Show(String.Format("Requesting permission to receive file:\r\n\r\n{0}\r\n{1} bytes long", fileName, fileLength), "", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }

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


            resetControls();
            button1.Enabled = button2.Enabled = true;
        }
    }
}
