﻿using System;
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
    public partial class Receive : Form
    {
        //tcp port
        const int PORT = 1723;
        public DHT HT { get; set; }

        public string folderName;
        public Receive(DHT H, string n)
        {
            InitializeComponent();
            HT = H;
            folderName = n;
        }

        private void resetControls()
        {
            progressBar1.Style = ProgressBarStyle.Marquee;
            textBox1.Text = "Waiting to connect to Sender";
        }

        protected override async void OnShown(EventArgs e)
        {
            TcpListener listener = TcpListener.Create(PORT);
            listener.Start();

            textBox1.Text = "Waiting for connection";

            TcpClient client = await listener.AcceptTcpClientAsync();
            NetworkStream ns = client.GetStream();
            while (true)
            {

                try
                {

                    textBox1.Text = "Client connected. Starting to receive the file";

                    ns.WriteByte(4);
                    byte[] permission = new byte[1];
                    await ns.ReadAsync(permission, 0, 1);
                    while (permission[0] != 1)
                    {
                        await ns.ReadAsync(permission, 0, 1);
                        textBox1.Text = "Waiting for response from Sender";
                    }
                    permission[0] = 0;
                    long fileLength;
                    string fileName;
                    DateTime fileLastModified;
                    byte[] fileNameBytes;
                    byte[] fileNameLengthBytes = new byte[4]; //int32
                    byte[] fileLengthBytes = new byte[8]; //int64
                    byte[] fileDateTimeBytes;
                    byte[] fileDateTimeLengthBytes = new byte[4];
                    await ns.ReadAsync(fileLengthBytes, 0, 8); // int64
                    await ns.ReadAsync(fileNameLengthBytes, 0, 4); // int32
                    fileNameBytes = new byte[BitConverter.ToInt32(fileNameLengthBytes, 0)];
                    await ns.ReadAsync(fileNameBytes, 0, fileNameBytes.Length);
                    await ns.ReadAsync(fileDateTimeLengthBytes, 0, 4);
                    fileDateTimeBytes = new byte[BitConverter.ToInt32(fileDateTimeLengthBytes, 0)];
                    await ns.WriteAsync(fileDateTimeBytes, 0, fileDateTimeBytes.Length);


                    fileLength = BitConverter.ToInt64(fileLengthBytes, 0);
                    fileName = ASCIIEncoding.ASCII.GetString(fileNameBytes);
                    fileLastModified = DateTime.Parse(ASCIIEncoding.ASCII.GetString(fileDateTimeBytes));
                    
                    textBox1.Text = "Receiving...";
                    progressBar1.Style = ProgressBarStyle.Continuous;
                    progressBar1.Value = 0;
                    int read = 0;
                    int totalRead = 0;
                    byte[] buffer = new byte[32 * 1024];

                    /*
                    if(File.Exists(folderName + "\\" + fileName))
                    {
                        fs = File.Open(folderName + "\\" + fileName, FileMode.Append);
                    }
                    else */
                    using (FileStream fs = File.Open(folderName + "\\" + fileName, FileMode.Append, FileAccess.Write)) 
                    {
                        
                        if(File.Exists(folderName + "\\" + fileName))
                        {
                            var existingLastModified = File.GetLastWriteTime(folderName + "\\" + fileName);
                            
                            int compare = DateTime.Compare(existingLastModified, fileLastModified);
                            if (compare < 0) //if Existing file is earlier than
                            {
                                FileStream temp = File.Open(folderName + "\\" + fileName, FileMode.Open);
                                while ((read = await ns.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                {
                                    await fs.WriteAsync(buffer, 0, read);
                                    totalRead += read;
                                    int a = (int)((100d * totalRead) / fileLength);
                                    if (a == 100)
                                        break;
                                    progressBar1.Value = (int)((100d * totalRead) / fileLength);
                                }
                            }
                            else if (compare == 0) //if Existing file is same time
                            {
                                //don't overwrite
                                var tempFile = Path.GetTempFileName();
                                FileStream temp = File.Open(tempFile, FileMode.Open);
                                while ((read = await ns.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                {
                                    await fs.WriteAsync(buffer, 0, read);
                                    totalRead += read;
                                    int a = (int)((100d * totalRead) / fileLength);
                                    if (a == 100)
                                        break;
                                    //progressBar1.Value = (int)((100d * totalRead) / fileLength);
                                }
                            }
                            else //if Existing file is later than
                            {
                                //don't overwrite
                                var tempFile = Path.GetTempFileName();
                                FileStream temp = File.Open(tempFile, FileMode.Open);
                                while ((read = await ns.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                {
                                    await fs.WriteAsync(buffer, 0, read);
                                    totalRead += read;
                                    int a = (int)((100d * totalRead) / fileLength);
                                    if (a == 100)
                                        break;
                                    //progressBar1.Value = (int)((100d * totalRead) / fileLength);
                                }
                            }
                        }
                        
                    }
                    
                    textBox1.Text = "Done Receiving...";
                    ns.WriteByte(4);
                    byte[] received = new byte[1];
                    await ns.ReadAsync(received, 0, 1);
                    while (received[0] != 1)
                    {
                        await ns.ReadAsync(received, 0, 1);
                        textBox1.Text = "Waiting for response from Sender";
                    }

                    //fs.Dispose();
                    //client.Close();
                    resetControls();

                   
                }
                catch (Exception E)
                {

                    //fs.Dispose();
                    client.Close();
                    listener.Stop();
                    resetControls();
                    this.Close();
                    MessageBox.Show(E.ToString());
                    return;
                }

            }
        }

    }
}
