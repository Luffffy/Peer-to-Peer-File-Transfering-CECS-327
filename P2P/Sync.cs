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

        private void button1_Click(object sender, EventArgs e)
        {
            // Establish the local endpoint  
            // for the socket. Dns.GetHostName 
            // returns the name of the host  
            // running the application. 
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 1723);

            // Creation TCP/IP Socket using  
            // Socket Class Costructor 
            Socket listener = new Socket(AddressFamily.InterNetwork,
                         SocketType.Stream, ProtocolType.IP);
            string[] filePaths = Directory.GetFiles(folderName);

            try
            {
                foreach (var n in filePaths)
                {
                    string filePath = "";
                    string fileName = n.Replace("\\", "/");

                    while (fileName.IndexOf("/") > -1)
                    {
                        filePath += fileName.Substring(0, fileName.IndexOf("/") + 1);
                        fileName = fileName.Substring(fileName.IndexOf("/") + 1);
                    }
                    byte[] fileNameByte = Encoding.ASCII.GetBytes(fileName);
                    if (fileNameByte.Length > 850 * 1024)
                    {
                        MessageBox.Show("File size is more than 850kb, please try with small file.");
                        return;
                    }

                    byte[] fileData = File.ReadAllBytes(filePath + fileName);

                    byte[] clientData = new byte[4 + fileNameByte.Length + fileData.Length];

                    byte[] fileNameLen = BitConverter.GetBytes(fileNameByte.Length);
                    /* File name length’s binary data. */
                    fileNameLen.CopyTo(clientData, 0);
                    fileNameByte.CopyTo(clientData, 4);
                    fileData.CopyTo(clientData, 4 + fileNameByte.Length);


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
            catch(Exception E)
            {
                MessageBox.Show(E.ToString());
            }
        }



        private void button2_Click(object sender, EventArgs e)
        {
            try
            {

                // Establish the remote endpoint  
                // for the socket. This example  
                // uses port 11111 on the local  
                // computer. 
                IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddr = ipHost.AddressList[0];
                IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 1723);

                // Creation TCP/IP Socket using  
                // Socket Class Costructor 
                Socket listener = new Socket(AddressFamily.InterNetwork,
                           SocketType.Stream, ProtocolType.IP);

                listener.Bind(localEndPoint);


                List <FileInfo> files = new List<FileInfo>();
                string[] filePaths = Directory.GetFiles(folderName, "", SearchOption.AllDirectories);
                foreach (var n in filePaths)
                {
                    FileInfo temp = new FileInfo(n);
                    files.Add(temp);
                }

                byte[] clientData = new byte[1024 * 5000];
                int receivedBytesLen = listener.Receive(clientData);
                textBox1.Text = "Receiving data...";
                int fileNameLen = BitConverter.ToInt32(clientData, 0);
                string fileName = Encoding.ASCII.GetString(clientData, 4, fileNameLen);

                var x = files.Where(y => y.Equals(fileName)).SingleOrDefault();
                if (x.Name.Equals(fileName) )
                {

                }
                else
                {
                    BinaryWriter bWrite = new BinaryWriter(File.Open(folderName + "/" + fileName, FileMode.Append)); ;
                    bWrite.Write(clientData, 4 + fileNameLen, receivedBytesLen - 4 - fileNameLen);

                    textBox1.Text = "Saving file...";
                    bWrite.Close();

                }

                listener.Close();
                /* Close binary writer and client socket */
                textBox1.Text = "Received & Saved file.";

            }
            catch (Exception E)
            {
                MessageBox.Show("Error Receiving File");
            }

            textBox1.Text = "Done receiving all files";
        }
    }
}
