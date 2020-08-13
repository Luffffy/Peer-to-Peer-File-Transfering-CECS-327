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
using System.Net;

namespace P2P
{
    public partial class Send : Form
    {
        const int PORT = 1723;
        public DHT HT { get; set; }

        public string folderName;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="H"></param>
        /// <param name="n"></param>
        public Send(DHT H, string n)
        {
            InitializeComponent();
            HT = H;
            folderName = n;
        }

        /// <summary>
        /// Resets form's text
        /// </summary>
        void resetControls()
        {
            textBox1.Text = "Waiting for Connection";
            progressBar1.Value = 0;
            progressBar1.Style = ProgressBarStyle.Continuous;
        }

        /// <summary>
        /// Starts connecting and sending to client onShow and.
        /// </summary>
        /// <param name="e"></param>
        protected override async void OnShown(EventArgs e)
        {

            progressBar1.Style = ProgressBarStyle.Marquee;
            var IPs = HT.getValues();

            foreach (var i in IPs)
            {
                string ip = i;
                IPAddress address;
                IPAddress.TryParse(ip, out address);
                IPEndPoint localEndPoint = new IPEndPoint(address, PORT);


                // Creation TCP/IP Socket using  
                // Socket Class Constructor 
                Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                
                //Gets all File path names from selected folder
                string[] filePaths = Directory.GetFiles(folderName);

                try
                {
                    textBox1.Text = "Connecting...";
                    int currentFileNumber = 0;
                    int totalFiles = filePaths.Length;

                    TcpClient client = new TcpClient();
                    await client.ConnectAsync(address, PORT);
                    foreach (var n in filePaths)
                    {
                        currentFileNumber++;
                        string progress = currentFileNumber + "/" + totalFiles;

                        textBox1.Text = "Sending connecting Info... " + progress;
                        NetworkStream ns = client.GetStream();
                        FileInfo file;
                        FileStream fileStream;
                        file = new FileInfo(n);
                        fileStream = file.OpenRead();


                        textBox1.Text = "Seeing if receiver is ready " + progress;
                        byte[] permission = new byte[1];
                        await ns.ReadAsync(permission, 0, 1);
                        while (permission[0] != 4)
                        {
                            textBox1.Text = "Receiver Not Ready " + progress;
                            await ns.ReadAsync(permission, 0, 1);

                        }
                        ns.WriteByte(1);// Tells receiver sender is ready
                        permission[0] = 0;

                        // Send file info
                        textBox1.Text = "Sending file info... " + progress;
                        //https://condor.depaul.edu/sjost/nwdp/notes/cs1/CSDatatypes.htm
                        byte[] fileName = ASCIIEncoding.ASCII.GetBytes(file.Name);
                        byte[] fileNameLength = BitConverter.GetBytes(fileName.Length);
                        byte[] fileLength = BitConverter.GetBytes(file.Length);
                        string DateTime = file.LastWriteTime.ToString();
                        byte[] fileDateTime = ASCIIEncoding.ASCII.GetBytes(DateTime);
                        byte[] fileDateTimeLength = BitConverter.GetBytes(DateTime.Length);

                        //asynchronously write files
                        await ns.WriteAsync(fileLength, 0, fileLength.Length);
                        await ns.WriteAsync(fileNameLength, 0, fileNameLength.Length);
                        await ns.WriteAsync(fileName, 0, fileName.Length);
                        await ns.WriteAsync(fileDateTimeLength, 0, fileDateTimeLength.Length);
                        await ns.WriteAsync(fileDateTime, 0, fileDateTime.Length);

                        textBox1.Text = "Sending..." + progress;
                        progressBar1.Style = ProgressBarStyle.Continuous;
                        int read = 0;
                        int totalWritten = 0;
                        byte[] buffer = new byte[32 * 1024]; // 32k chunks
                        while ((read = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await ns.WriteAsync(buffer, 0, read);
                            totalWritten += read;
                            progressBar1.Value = (int)((100d * totalWritten) / file.Length);
                        }


                        textBox1.Text = "Seeing if receiver is done " + progress;
                        byte[] sent = new byte[1];
                        await ns.ReadAsync(sent, 0, 1);
                        while (sent[0] != 4)
                        {
                            await ns.ReadAsync(sent, 0, 1);
                            textBox1.Text = "Receiver Not Done " + progress;
                        }
                        ns.WriteByte(1);

                        //MessageBox.Show("Sending complete!");
                        textBox1.Text = "Sending complete! " + progress;
                        resetControls();
                    }
                }
                catch (Exception E)
                {
                    MessageBox.Show(E.ToString());
                    resetControls();
                }

                // Close client Socket using the 
                // Close() method. After closing, 
                // we can use the closed Socket  
                // for a new Client Connection 
                listener.Close();
                MessageBox.Show("Sent all files");
            }

            
            this.Close();
        }
    }
}
