using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
namespace Server_UDP
{
    public partial class Server_UDP : Form
    {
        List<string> userNameList = new List<string>();
        SqlConnection conn = new SqlConnection("Server=.;database=SunMonitor;integrated security=SSPI");


        public Server_UDP()
        {
            InitializeComponent();
            conn.Open();
            string selectStr = "select * from dbo.tb_User";
            SqlDataAdapter adapter = new SqlDataAdapter(selectStr, conn);
            conn.Close();
            DataSet ds = new DataSet();
            adapter.Fill(ds, "UserTable");
            dataGridView1.DataSource = ds;
            dataGridView1.DataMember = "UserTable";

            for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
            {
                switch (i)
                {
                    case 1: dataGridView1.Columns[i].Width = 80;
                        break;
                    case 2: dataGridView1.Columns[i].Width = 220;
                        break;
                    case 3: dataGridView1.Columns[i].Width = 80;
                        break;
                    case 15: dataGridView1.Columns[i].Width = 180;

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
        Socket recSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private void Server_UDP_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;

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

            recSocket.Bind(point);

            Thread thread = new Thread(ReceiveAndSend);
            thread.IsBackground = true;
            thread.Start(recSocket);

        }
        void ReceiveAndSend(object o)
        {
            Socket socketSend = o as Socket;

            IPEndPoint remoteIP = new IPEndPoint(IPAddress.Any, 0);
            EndPoint remote = (EndPoint)remoteIP;
            while (true)
            {
                byte[] buffer = new byte[1024];
                int c = socketSend.ReceiveFrom(buffer, ref remote);

                string str = Encoding.UTF8.GetString(buffer, 0, c);

                if (str == "Get User")
                {
                    byte[] dataArr = GetBinaryFormatDataSet((DataSet)dataGridView1.DataSource);
                    recSocket.SendTo(dataArr, dataArr.Length, SocketFlags.None, remote);
                }
                else
                {
                    if (UserNameIsEqual(SelectUserName(str)))
                    {
                        if (PasswordIsEqual(SelectUserName(str), SelectPassword(str)))
                        {

                            byte[] loginSuccess = Encoding.UTF8.GetBytes("登录成功");
                            recSocket.SendTo(loginSuccess, loginSuccess.Length, SocketFlags.None, remote);
                        }
                        else
                        {
                            byte[] passwordError = Encoding.UTF8.GetBytes("密码错误");
                            recSocket.SendTo(passwordError, passwordError.Length, SocketFlags.None, remote);
                        }
                    }
                    else
                    {
                        byte[] userNameError = Encoding.UTF8.GetBytes("无效用户名");
                        recSocket.SendTo(userNameError, userNameError.Length, SocketFlags.None, remote);
                    }
                }

            }
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
        public bool PasswordIsEqual(string userStr, string passStr)
        {
            bool b = false;
            string password = string.Empty;
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
        }

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
        public static byte[] HexStringToByteArray(string s)
        {
            s = s.Replace(" ", "");
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            return buffer;
        }
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
