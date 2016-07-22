using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Data.SqlClient;
using System.Security.Cryptography;
namespace Correspond
{
    public partial class Server : Form
    {
        SqlConnection conn = new SqlConnection("server = .;database = SunMonitor;integrated security=SSPI");

        List<string> userNameList = new List<string>();
        //List<string> passwordList = new List<string>();
        DataSet ds = new DataSet();
        public Server()
        {
            InitializeComponent();


            conn.Open();
            string selectStr = "select * from dbo.tb_User";
            SqlDataAdapter adapter = new SqlDataAdapter(selectStr, conn);
            conn.Close();
            ds = new DataSet();
            adapter.Fill(ds, "UserTable");


            dataGridView1.DataSource = ds;
            dataGridView1.DataMember = "UserTable";
            for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
            {
                switch (i)
                {
                    case 1: dataGridView1.Columns[i].Width = 80;
                        break;
                    case 2: dataGridView1.Columns[i].Width = 290;
                        break;
                    case 3: dataGridView1.Columns[i].Width = 80;
                        break;
                    case 15: dataGridView1.Columns[i].Width = 230;

                        break;
                    default: dataGridView1.Columns[i].Visible = false;
                        break;
                }
            }

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                string str1 = (string)dr["UserName"];
                string str2 = (string)dr["Password"];
                userNameList.Add(str1);
                //passwordList.Add(str2);
            }


        }

        private void Server_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void buttonOberserve_Click(object sender, EventArgs e)
        {
            Socket socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPAddress ipAddr = IPAddress.None;
            IPAddress[] addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            foreach (IPAddress ip in addressList)
            {
                if(ip.AddressFamily.ToString()=="InterNetwork")
                {
                    ipAddr = ip;
                }
            }
            IPEndPoint point = new IPEndPoint(ipAddr, 8000);

            socketWatch.Bind(point);
            showMsg("监听成功");
            socketWatch.Listen(20);

            Thread thread = new Thread(Listen);
            thread.IsBackground = true;
            thread.Start(socketWatch);

        }
        void showMsg(String str)
        {
            textLog.AppendText(str + ":" + "\r\n");
        }

        Socket socketSend;

        void Listen(object o)
        {
            Socket socketWatch = o as Socket;
            while (true)
            {
                showMsg("等待连接......");
                try
                {
                    socketSend = socketWatch.Accept();
                    showMsg(socketSend.RemoteEndPoint.ToString() + ":" + "连接成功");

                    Thread thread = new Thread(Receive);
                    thread.IsBackground = true;
                    thread.Start(socketSend);

                }
                catch
                {

                }

            }
        }
        /// <summary>
        /// 接收客户端发送的消息
        /// </summary>
        /// <param name="o"></param>
        void Receive(object o)
        {
            Socket socketSend = o as Socket;
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[1024 * 1024 * 20];
                    int a = socketSend.Receive(buffer);
                    if (a == 0)
                    {
                        break;
                    }
                    string str = Encoding.UTF8.GetString(buffer, 0, a);

                    if (str == "get User")
                    {
                        byte[] binaryDataSet = GetBinaryFormatDataSet((DataSet)dataGridView1.DataSource);
                        socketSend.Send(binaryDataSet);

                    }
                    if (UserNameIsEqual(SelectUserName(str)))
                    {
                        if (PasswordIsEqual(SelectUserName(str), SelectPassword(str)))
                        {

                            byte[] loginSuccess = Encoding.UTF8.GetBytes("登录成功");
                            socketSend.Send(loginSuccess);
                        }
                        else
                        {
                            byte[] passwordError = Encoding.UTF8.GetBytes("密码错误");
                            socketSend.Send(passwordError);
                        }
                    }
                    else
                    {
                        byte[] userNameError = Encoding.UTF8.GetBytes("无效用户名");
                        socketSend.Send(userNameError);
                    }

                    showMsg(socketSend.RemoteEndPoint.ToString() + ":" + str);
                }
                catch
                {

                }

            }
        }
        /// <summary>
        /// 判断用户名
        /// </summary>
        /// <param name="userStr"></param>
        /// <returns></returns>
        public bool UserNameIsEqual(string userStr)
        {
            bool b = false;
            for (int i = 0; i < userNameList.Count; i++)
            {
                if (userStr == userNameList[i])
                {
                    b = true;

                }
            }
            return b;

        }


        /// <summary>
        /// 判断密码
        /// </summary>
        /// <param name="userStr"></param>
        /// <param name="passStr"></param>
        /// <returns></returns>
        public bool PasswordIsEqual(string userStr, string passStr)
        {
            bool b = false;
            string password;
            string md5Str = GetMD5_32(passStr);
            byte[] passwordArr = HexStringToByteArray(md5Str);
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < passwordArr.Length; i++)
            {
                result.Append(passwordArr[i].ToString());
            }

            conn.Open();
            string sqlStr = "select * from dbo.tb_User where Username = '" + userStr + "'";
            SqlCommand command = new SqlCommand(sqlStr, conn);
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                password = reader["Password"].ToString();
                if (result.ToString() == password)
                {
                    b = true;
                }
            }
            conn.Close();
            return b;

            //if (result.ToString() == password)
            //{
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}


        }

        public static byte[] HexStringToByteArray(string s)
        {
            s = s.Replace(" ", "");
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            return buffer;
        }

        public string SelectUserName(string str)
        {

            int j = str.IndexOf("User");
            int k = str.IndexOf("&");
            string userStr = str.Substring(j + 9, k - j - 9);
            return userStr;
        }
        public string SelectPassword(string str)
        {
            int l = str.IndexOf("Pass");
            int m = str.IndexOf("!");
            string passStr = str.Substring(l + 9, m - l - 9);
            return passStr;
        }
        //public String EncryptCode(String message)
        //{
        //    Byte[] clearBytes = new UnicodeEncoding().GetBytes(message);
        //    Byte[] hashedBytes = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(clearBytes);

        //    String tt = BitConverter.ToString(hashedBytes).Replace("-", "");
        //    // MessageBox.Show(tt.Length.ToString());
        //    return tt;
        //}
        public static string GetMD5_32(string input)
        {
            MD5 md5 = MD5.Create();
            byte[] data = md5.ComputeHash(Encoding.Default.GetBytes(input));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sb.Append(data[i].ToString("x2"));
            }
            return sb.ToString();
        }
        /// <summary>
        /// 将DataSet串行化
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        public static byte[] GetBinaryFormatDataSet(DataSet ds)
        {
            MemoryStream memoryS = new MemoryStream();//创建内存流
            memoryS.Seek(0, SeekOrigin.Begin);
            IFormatter formatter = new BinaryFormatter();//产生二进制序列化格式
            ds.RemotingFormat = SerializationFormat.Binary;//指定DataSet串行化格式是二进制
            formatter.Serialize(memoryS, ds);//串行化到内存中
            byte[] binaryResult = memoryS.ToArray();//将DataSet转换为byte[]
            //清空和释放内存流
            memoryS.Close();
            memoryS.Dispose();
            return binaryResult;
        }
    }
}
