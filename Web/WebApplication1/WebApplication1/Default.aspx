<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebApplication1._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <h1>ASP.NET</h1>
        <p class="lead">ASP.NET is a free web framework for building great Web sites and Web applications using HTML, CSS, and JavaScript.</p>
        <p><a href="http://www.asp.net" class="btn btn-primary btn-lg">Learn more &raquo;</a></p>
        
            <asp:Button ID="Button1" runat="server" Height="47px" Text="Button" Width="108px" />
        
        <br />
        <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" DataSourceID="SqlDataSource1" AllowPaging="True" AllowSorting="True">
            <Columns>
                <asp:BoundField DataField="IDINSTYPE" HeaderText="IDINSTYPE" SortExpression="IDINSTYPE" />
                <asp:BoundField DataField="IDVAR" HeaderText="IDVAR" SortExpression="IDVAR" />
                <asp:BoundField DataField="DESCRIPTION" HeaderText="DESCRIPTION" SortExpression="DESCRIPTION" />
                <asp:BoundField DataField="USER_DESCRIPTION" HeaderText="USER_DESCRIPTION" SortExpression="USER_DESCRIPTION" />
            </Columns>
        </asp:GridView>
        <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:DDSQ08DNConnectionString %>" SelectCommand="SELECT * FROM [EVENTS]"></asp:SqlDataSource>
        
        <p>&nbsp;</p>
    </div>

    <div class="row">
    </div>

</asp:Content>
