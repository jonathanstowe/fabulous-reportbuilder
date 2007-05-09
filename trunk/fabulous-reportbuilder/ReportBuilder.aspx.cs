using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Xml.Serialization;
using System.IO;

namespace Fabulous.Reports
{
	/// <summary> 
	///  Report base class
	/// </summary>
	public class ReportBuilder : System.Web.UI.Page
	{
		protected System.Web.UI.HtmlControls.HtmlGenericControl Heading;
		protected System.Web.UI.HtmlControls.HtmlGenericControl ReportList;
		protected System.Web.UI.HtmlControls.HtmlForm ReportForm;
		

		static ReportDefinition[] theReports;

		private void Page_Load(object sender, System.EventArgs e)
		{
			string reportDir = System.Configuration.ConfigurationSettings.AppSettings["ReportPath"];

			
			if ( this.Request.Params["report"] == null )
			{
				theReports = GetReports(reportDir);
				Heading.InnerText = "Available Reports";

				this.ReportList = getReportList(theReports);

				ReportForm.Controls.Add(this.ReportList);

			}
			else
			{ 
				int report_no = System.Convert.ToInt32(this.Request.Params["report"]);
				
				if ( report_no >= 0 && report_no < theReports.Length )
				{
					if ( theReports[report_no].Interface != null && theReports[report_no].Interface.Length > 0)
					{
						doInterface(report_no);
						if (this.IsPostBack)
						{
							doReport(report_no);
						}
					}
					else
					{
						doReport(report_no);
					}
				}
			}
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			
			InitializeComponent();
			base.OnInit(e);
		}
		
		
		private void InitializeComponent()
		{    
			this.Load += new System.EventHandler(this.Page_Load);

		}
		#endregion
		

		private void doInterface(int report_no)
		{
			Heading.InnerText = theReports[report_no].Title;
			
			this.ReportList.Visible = false;
			
			HtmlTable theTable = new HtmlTable();		
				
			theTable.Width = "100%";
			theTable.ID = "tbl_interface";
			
			foreach (Item item in theReports[report_no].Interface)
			{
				
				HtmlTableRow tblRow = new HtmlTableRow();
				HtmlTableCell labelCell = new HtmlTableCell("th");
				HtmlTableCell dataCell = new HtmlTableCell();

				//Label
				labelCell.Controls.Add(new LiteralControl(item.Name));
				
				//Field
				switch ( item.Type )
				{
					case "list":
						System.Web.UI.WebControls.TextBox theListCtrl = new System.Web.UI.WebControls.TextBox();
						theListCtrl.ID = item.Name.Replace(" ","_");
						theListCtrl.TextMode = System.Web.UI.WebControls.TextBoxMode.MultiLine;
						theListCtrl.Rows = 10;
						dataCell.Controls.Add(theListCtrl);
						break;
					default:
						System.Web.UI.WebControls.TextBox theTextCtrl = new System.Web.UI.WebControls.TextBox();
						theTextCtrl.ID = item.Name.Replace(" ","_");
						dataCell.Controls.Add(theTextCtrl);
						break;
				}
				
				tblRow.Cells.Add (labelCell);
				tblRow.Cells.Add (dataCell);
				theTable.Rows.Add(tblRow);

			}

			System.Web.UI.HtmlControls.HtmlInputButton  btnSubmit = new System.Web.UI.HtmlControls.HtmlInputButton("submit");
			btnSubmit.Value = "Do It !";
			
			HtmlTableRow btnRow = new HtmlTableRow();
			HtmlTableCell btnlabelcell = new HtmlTableCell();
			HtmlTableCell btndatacell = new HtmlTableCell();
			btnlabelcell.Controls.Add(new LiteralControl());
			btndatacell.Controls.Add(btnSubmit);
			btnRow.Cells.Add(btnlabelcell);
			btnRow.Cells.Add(btndatacell);
			theTable.Rows.Add(btnRow);

			this.ReportForm.Controls.Add(theTable);
		}
		
		private void doReport(int report_no)
		{
			Heading.InnerText = theReports[report_no].Title;

			this.ReportList.Visible = false;
			HtmlTable theTable = new HtmlTable();

			theTable.Width = "100%";

			HtmlTableRow headerRow = new HtmlTableRow();
				
			foreach (Item column in theReports[report_no].Columns)
			{
				HtmlTableCell theCell = new HtmlTableCell();
				theCell.Controls.Add(new LiteralControl("<b>" + column.Name + "</br>" ));
				headerRow.Cells.Add(theCell);
			}

			theTable.Rows.Add(headerRow);

			foreach (HtmlTableRow dataRow in GetReportRows(theReports[report_no]) )
			{
				theTable.Rows.Add(dataRow);
			}


			this.Controls.Add(theTable);
		}


      // TODO: Abstract this away so we can use System.Data.IDb* thingies
      
		public HtmlTableRow[] GetReportRows(ReportDefinition reportDef)
		{
			ArrayList theRows = new ArrayList();

			SqlConnection prismconn = new SqlConnection(System.Configuration.ConfigurationSettings.AppSettings["DSN"]);
			
			prismconn.Open();
			
			SqlCommand theQuery = new SqlCommand(reportDef.Query,prismconn);
			theQuery.CommandTimeout=600;
	
			Item[] fields = reportDef.Interface;

			if ( fields != null && fields.Length > 0 )
			{
				if (theQuery.Parameters.Contains(fields[0].Name.Replace(" ","_")))
				{
					theQuery.Parameters.Clear();
				}
			
				foreach (Item field in fields)
				{
					string fieldname = field.Name.Replace(" ","_");
					string param = "@" + fieldname;
				
					switch ( field.Type )
					{
						case "string":
							theQuery.Parameters.Add(param, SqlDbType.VarChar, 2000).Value = this.Request.Form[fieldname].ToString();
							break;
						case "date":						
							theQuery.Parameters.Add(param, SqlDbType.VarChar, 10).Value = this.Request.Form[fieldname].ToString();
							break;
						case "int":
							theQuery.Parameters.Add(param, SqlDbType.Int,4).Value = System.Convert.ToInt32(this.Request.Form[fieldname].ToString());
							break;
						case "decimal":
							theQuery.Parameters.Add(param, SqlDbType.Decimal).Value = System.Convert.ToDecimal(this.Request.Form[fieldname].ToString());
							break;
						case "list":
							string[] strList = this.Request.Form[fieldname].ToString().Split(Environment.NewLine.ToCharArray());
							CreateTmpTable(fieldname, field.DBType, strList, prismconn);
							break;
						default:
						
							break;
					}
	
				}
			}
			IDataReader theReader = theQuery.ExecuteReader();

			while (theReader.Read())
			{
				theRows.Add(GetHtmlRow(theReader, reportDef.Columns));
			}
			
			HtmlTableRow[] tmpRow = new HtmlTableRow[theRows.Count];
			theRows.CopyTo(tmpRow,0);

			

			theQuery.Dispose();
			prismconn.Close();

			return tmpRow;
		}

		void CreateTmpTable(string name, string DBtype, string[] strList, SqlConnection prismconn)
		{
			SqlCommand tmpQuery = new SqlCommand() ;
			tmpQuery.Connection = prismconn;
			string strQuery = "create table #" + name ;
			strQuery += " (" + name + " " + DBtype + " collate SQL_Latin1_General_CP1_CI_AS)";
			
			tmpQuery.CommandText = strQuery;
			tmpQuery.ExecuteNonQuery();

			foreach (string str in strList)
			{
				tmpQuery.CommandText = "insert into #" + name + " values ('" + str + "')";
				tmpQuery.ExecuteNonQuery();
			}

		}

		HtmlTableRow GetHtmlRow(IDataReader reader, Item[] columns)
		{
			HtmlTableRow theRow = new HtmlTableRow();

			for ( int colcount = 0; colcount < (columns.Length > reader.FieldCount ? reader.FieldCount : columns.Length); colcount++)
			{
				theRow.Cells.Add(GetHtmlColumn(reader,columns[colcount],colcount));
			}

			return theRow;
		}

		HtmlTableCell GetHtmlColumn(IDataReader reader, Item column, int colnumber)
		{
			string tmpValue;
			HtmlTableCell theCell = new HtmlTableCell();

			if ( reader.IsDBNull(colnumber) )
			{
				tmpValue = "";
			}
			else
			{
				switch ( column.Type )
				{
					case "string":
						tmpValue = reader.GetString(colnumber);
						break;
					case "date":
						tmpValue = reader.GetDateTime(colnumber).ToString("dd/MM/yyyy");
						break;
					case "datetime":
						tmpValue = reader.GetDateTime(colnumber).ToString();
						break;
					case "int":
						tmpValue = reader.GetInt32(colnumber).ToString();
						break;
					case "decimal":
						tmpValue = reader.GetDecimal(colnumber).ToString("0.00");
						break;
					default:
						tmpValue = "Oops unknown type";
						break;
				}
				 
			}

			theCell.Controls.Add(new LiteralControl(tmpValue));

			return theCell;
		}

		public ReportDefinition GetReportDefinition(string Filename)
		{
			
			ReportDefinition tmpReport;
			XmlSerializer serializer = new XmlSerializer(typeof(ReportDefinition));

			TextReader reader = new StreamReader(Filename);
			tmpReport  = (ReportDefinition)serializer.Deserialize(reader);
			reader.Close();
			return tmpReport;
		}

		public ReportDefinition[] GetReports(string Directory)
		{
			ArrayList tmpReports = new ArrayList();
			DirectoryInfo dir = new DirectoryInfo(Directory);
			FileInfo[] files = dir.GetFiles();
			foreach ( FileInfo file in files )
			{
				string FileName = Path.GetFileName(file.FullName);
				if ( Path.GetExtension(FileName) == ".xml" )
				{
					tmpReports.Add(GetReportDefinition(Directory + "/" + FileName));
				}
			}

			ReportDefinition[] theDefinitions = new ReportDefinition[tmpReports.Count];
			tmpReports.CopyTo(theDefinitions);
			return theDefinitions;
		}

		// TODO:  Generalize the presentation here
		//
		public HtmlGenericControl getReportList(ReportDefinition[] theReports)
		{
			HtmlGenericControl ReportList = new HtmlGenericControl("ul");
			int report_no = 0;
			foreach (ReportDefinition report in theReports )
			{
				HtmlGenericControl listItem = new HtmlGenericControl("li");
				HtmlAnchor link = new HtmlAnchor();
				link.InnerText = report.Title;
				link.HRef = this.Request.Url + "?report=" + report_no.ToString(); 
				listItem.Controls.Add(link);
				ReportList.Controls.Add(listItem);
				report_no++;
			}

			return ReportList;
		}
	}


}
