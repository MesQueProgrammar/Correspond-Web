using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace WAClient
{
    public partial class WebForm1 : System.Web.UI.Page
    {

        Socket socketSend = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            
        protected void Page_Load(object sender, EventArgs e)
        {
       

        }

        protected void loginButton_Click(object sender, EventArgs e)
        {
            string str = "UserName=" + userNameTextBox.Text + "&" + "Password=" + passwordTextBox.Text + "!";
            byte[] buffer = Encoding.UTF8.GetBytes(str);

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
            socketSend.SendTo(buffer, buffer.Length, SocketFlags.None, point);


            ReceiveInfo(socketSend);


        }
        void ReceiveInfo(object o)
        {
            Socket socketSend = o as Socket;

            byte[] data = new byte[1024];
            IPEndPoint listenPoint = new IPEndPoint(IPAddress.Any, 0);
            EndPoint remote = (EndPoint)listenPoint;
            int c = socketSend.ReceiveFrom(data, ref remote);

            string recString = Encoding.UTF8.GetString(data, 0, c);

            if (recString == "登录成功" || recString == "密码错误" || recString == "无效用户名")
            {
                alert(recString,this.Page);
            }
            if(recString =="登录成功")
            {
                Response.Redirect("GridView.aspx");
            }
            else if(recString == "密码错误")
            {
                alert(recString, this.Page);
            }
            else
            {
                alert(recString, this.Page);
            }
            
        }

        public void alert(string str_message, Page page)
        {
            page.RegisterStartupScript("", "<script>alert('" + str_message + "');</script>");
        }

    }
}