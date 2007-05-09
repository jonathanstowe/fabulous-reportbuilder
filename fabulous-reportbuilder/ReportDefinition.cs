using System;
using System.Xml;
using System.Xml.Serialization;


namespace Fabulous.Reports
{
	/// <summary>
	/// Summary description for ReportDefinition.
	/// </summary>
	/// <remarks/>

	public class ReportDefinition 
	{
        
		/// <remarks>Contains the title that is used on both the index of reports
		/// and the title of the report page.
		/// </remarks>
		public string Title;
        
		/// <remarks>
		/// The SQL Query string for this report.
		/// </remarks>
		public string Query;
        
		/// 
		/// <remarks>
		/// The columns in the report output.  These are used to format the rows and provide the header.
		/// </remarks>

		[XmlArrayItem(ElementName = "Item")]
		[XmlArray(ElementName = "Columns")]
		public Item[] Columns;

      /// <remarks>
      /// The parameters to the query. This is used to form the parameter UI
      /// </remarks>
      
		[XmlArrayItem(ElementName = "Item")]
		[XmlArray(ElementName = "Interface")]
		public Item[] Interface;
	}
    
	/// <remarks/>

	public class Item 
	{
        
		/// <remarks/>
		[XmlElement("Name")]
		public string Name;
        
		/// <remarks/>
		[XmlElement("Type")]
		public string Type;

		/// <remarks/>
		[XmlElement("DBType")]
		public string DBType;

	}
	
}
