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
        public Send(DHT H, string n)
        {
            InitializeComponent();
            HT = H;
            folderName = n;
        }

        void resetControls()
        {
            //textBox1.Enabled = textBox2.Enabled = button1.Enabled = true;
            textBox1.Text = "Waiting for Connection";
            progressBar1.Value = 0;
            progressBar1.Style = ProgressBarStyle.Continuous;
        }

        protected override async void OnShown(EventArgs e)
        {

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
                    int x = 1;
                    int totalFiles = filePaths.Length;
                    TcpClient client = new TcpClient();
                    await client.ConnectAsync(address, PORT);
                    foreach (var n in filePaths)
                    {

                        x++;
                        string progress = x + "/" + totalFiles;
                        try
                        {

                            //await listener.ConnectAsync(localEndPoint);
                            //listener.SendFile(n);
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
                            ns.WriteByte(1);
                            permission[0] = 0;
                            // Send file info
                            textBox1.Text = "Sending file info... " + progress;
                            {   //https://condor.depaul.edu/sjost/nwdp/notes/cs1/CSDatatypes.htm
                                byte[] fileName = ASCIIEncoding.ASCII.GetBytes(file.Name);
                                //int t = fileName.Length; 
                                byte[] fileNameLength = BitConverter.GetBytes(fileName.Length);
                                byte[] fileLength = BitConverter.GetBytes(file.Length);
                                string DateTime = file.LastWriteTime.ToString();
                                MessageBox.Show(DateTime);
                                byte[] fileDateTime = ASCIIEncoding.ASCII.GetBytes(DateTime);
                                byte[] fileDateTimeLength = BitConverter.GetBytes(DateTime.Length);
                                //byte[] fileLastModified = BitConverter.GetBytes(file.LastWriteTime.Ticks);
                                await ns.WriteAsync(fileLength, 0, fileLength.Length);
                                await ns.WriteAsync(fileNameLength, 0, fileNameLength.Length);
                                await ns.WriteAsync(fileName, 0, fileName.Length);
                                await ns.WriteAsync(fileDateTimeLength, 0, fileDateTimeLength.Length);
                                await ns.WriteAsync(fileDateTime, 0, fileDateTime.Length);
                                //await ns.WriteAsync(fileLastModified, 0, fileLastModified.Length);
                            }



                            // Close client Socket using the 
                            // Close() method. After closing, 
                            // we can use the closed Socket  
                            // for a new Client Connection 
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
                            

                            //ns.WriteByte(0);
                            //MessageBox.Show(read + " " + totalWritten);
                            //listener.Shutdown(SocketShutdown.Both);
                            
                            //MessageBox.Show("Sending complete!");
                            textBox1.Text = "Sending complete! " + progress;
                            resetControls();
                            


                        }
                        catch (Exception E)
                        {
                            MessageBox.Show(E.ToString());
                            resetControls();
                        }
                        
                        //listener.Shutdown(SocketShutdown.Both);
                    }
                }
                catch (Exception E)
                {
                    MessageBox.Show(E.ToString());
                    resetControls();
                }
                listener.Close();
            }

            MessageBox.Show("Close");
            this.Close();
        }
    }
}
