<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="WAClient.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <br />
        <asp:TextBox ID="userNameTextBox" runat="server" style="z-index: 1; left: 455px; top: 33px; position: fixed; height: 18px"></asp:TextBox>
        <asp:Label ID="userNameLabel" runat="server" style="z-index: 1; left: 368px; top: 35px; position: fixed; height: 15px; width: 92px; " Text="用户名"></asp:Label>
        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
        <br />
        <br />
        <br />
        <asp:Button ID="loginButton" runat="server" style="z-index: 1; left: 429px; top: 169px; position: fixed; right: 322px;" Text="登录" OnClick="loginButton_Click" Width="80px" />
        <asp:TextBox ID="passwordTextBox" runat="server" style="z-index: 1; left: 455px; top: 83px; position: fixed; height: 18px"></asp:TextBox>
        <asp:Label ID="passwordLabel" runat="server" style="z-index: 1; left: 368px; top: 83px; position: fixed; height: 15px; width: 92px; " Text="密码"></asp:Label>
        <br />
        <br />
        <br />
        <br />
        <br />
        <br />
        <br />
        <br />
        <div>
        </div>


    </form>
</body>
</html>
