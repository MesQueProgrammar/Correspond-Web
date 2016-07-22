using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Data.SqlClient;

namespace Client
{
    public partial class Client : Form
    {
        public Client()
        {
            InitializeComponent();
        }
        Socket socketSend;
        private void buttonConnect_Click(object sender, EventArgs e)
        {
            socketSend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAddr = IPAddress.None;
            IPAddress[] addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            foreach (IPAddress ip in addressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    ipAddr = ip;
                }
            }
            IPEndPoint point = new IPEndPoint(ipAddr, 8000);
            socketSend.Connect(point);
            textCHost.Text = "连接成功";



        }
        /// <summary>
        /// 接收服务器端返回的DataSet
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        DataSet ReceiveDataSet(object o)
        {

            while (true)
            {
                try
                {
                    byte[] buffer = new byte[1024 * 1024 * 20];
                    int a = socketSend.Receive(buffer);

                    DataSet ds = RetrieveDataSet(buffer);

                    return ds;

                }

                catch
                {

                }
            }

        }
        /// <summary>
        /// 接收服务器端返回的登录信息
        /// </summary>
        /// <param name="o"></param>
        void ReceiveInfo(object o)
        {
            Socket socketSend = o as Socket;
            
                try
                {

                    byte[] Info = new byte[1024*1024*10];
                    int a = socketSend.Receive(Info);

                    string str = Encoding.UTF8.GetString(Info, 0, a);

                    if (str == "登录成功"||str == "密码错误"||str == "无效用户名")
                    {
                        messageShow(str);
                    }
                    else
                    {
                        DataSet ds = RetrieveDataSet(Info);
                        dataGridView1.DataSource = ds;
                        dataGridView1.DataMember = ds.Tables[0].ToString();

                        for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
                        {
                            switch (i)
                            {
                                case 1: dataGridView1.Columns[i].Width = 80;
                                    break;
                                case 2: dataGridView1.Columns[i].Width = 400;
                                    break;
                                case 3: dataGridView1.Columns[i].Width = 80;
                                    break;
                                case 15: dataGridView1.Columns[i].Width = 350;

                                    break;
                                default: dataGridView1.Columns[i].Visible = false;
                                    break;
                            }
                        }
                    }
                }
                catch
                {

                }
        }
        void messageShow(string str)
        {
            MessageBox.Show(str);
        }


        private void buttonSend_Click(object sender, EventArgs e)
        {

            string str = textCMsg.Text;
            byte[] buffer = Encoding.UTF8.GetBytes(str);
            socketSend.Send(buffer);

            ReceiveInfo(socketSend);
            //DataSet ds = ReceiveDataSet(socketSend);

            //dataGridView1.DataSource = ds;
            //dataGridView1.DataMember = ds.Tables[0].ToString();

            //for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
            //{
            //    switch (i)
            //    {
            //        case 1: dataGridView1.Columns[i].Width = 80;
            //            break;
            //        case 2: dataGridView1.Columns[i].Width = 400;
            //            break;
            //        case 3: dataGridView1.Columns[i].Width = 80;
            //            break;
            //        case 15: dataGridView1.Columns[i].Width = 350;

            //            break;
            //        default: dataGridView1.Columns[i].Visible = false;
            //            break;
            //    }
            //}


        }
        private void buttonLogin_Click(object sender, EventArgs e)
        {
            string str = "UserName=" + textUserName.Text + "&" + "Password=" + textPassword.Text + "!";

            byte[] buffer = Encoding.UTF8.GetBytes(str);
            socketSend.Send(buffer);

            Thread thread = new Thread(ReceiveInfo);
            thread.IsBackground = true;
            thread.Start(socketSend);
            //Thread thread = new Thread(ReceiveInfo);
            //thread.IsBackground = true;
            //thread.Start(socketSend);

        }


        private void Client_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
        }
        /// <summary>
        /// 反串行化DataSet
        /// </summary>
        /// <param name="binaryData"></param>
        /// <returns></returns>
        public static DataSet RetrieveDataSet(byte[] binaryData)
        {
            MemoryStream memStream = new MemoryStream(binaryData);
            memStream.Seek(0, SeekOrigin.Begin);
            IFormatter formatter = new BinaryFormatter();
            object o = formatter.Deserialize(memStream);
            if (o is DataSet)
            {
                DataSet dataSetResult = (DataSet)o;
                return dataSetResult;
            }
            else return null;

        }

    




    }
}
