using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace WAClient
{
    public partial class GridView : System.Web.UI.Page
    {
        Socket socketReceive = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        protected void Page_Load(object sender, EventArgs e)
        {

            

            //Thread thread = new Thread(Receive);
            //thread.IsBackground = true;
            //thread.Start(socketReceive);
            
         
        }
        void Receive(object o)
        {
            Socket socketRec = o as Socket;
            byte[] data = new byte[1024*1024*10];
            IPEndPoint listenPoint = new IPEndPoint(IPAddress.Any, 0);
            EndPoint remote = (EndPoint)listenPoint;
            int c = socketRec.ReceiveFrom(data, ref remote);

            DataSet ds = RetrieveDataSet(data);
            this.GridView1.DataSource = ds;
            this.GridView1.DataMember = "UserTable";
           
        }
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

        protected void Button1_Click(object sender, EventArgs e)
        {
           

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
            string str = "Get User";
            byte[] fetchStr = Encoding.UTF8.GetBytes(str);
            socketReceive.SendTo(fetchStr, fetchStr.Length, SocketFlags.None, point);

            Receive(socketReceive);
        }
    }
}