<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="FastDFS.Web._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>无标题页</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:FileUpload ID="FileUpload1" runat="server" />
        <asp:Button ID="Button2" runat="server" OnClick="Button1_Click" Text="Button" />
        <asp:Button ID="Button1" runat="server" Text="Button batch" OnClick="Button1_Click1" />
        <asp:FileUpload ID="FileUpload3" runat="server" />
        <asp:FileUpload ID="FileUpload2" runat="server" />
        <asp:FileUpload ID="FileUpload4" runat="server" />
        <asp:FileUpload ID="FileUpload5" runat="server" /></div>
    </form>
</body>
</html>
