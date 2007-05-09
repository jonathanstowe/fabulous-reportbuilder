<%@ Page language="c#" Codebehind="ReportBuilder.aspx.cs" AutoEventWireup="false" Inherits="Fabulous.Reports.ReportBuilder" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
	<HEAD>
		<title></title>
		<link href="css.css" type="text/css" rel="stylesheet" />
	</HEAD>
	<body MS_POSITIONING="GridLayout">
		<div id="top">
			<h1 id="Heading" runat="server">Available Reports</h1>
		</div>
		<form id="ReportForm" method="post" runat="server">
			<ul id="ReportList" runat="server">
			</ul>
		</form>
	</body>
</HTML>
