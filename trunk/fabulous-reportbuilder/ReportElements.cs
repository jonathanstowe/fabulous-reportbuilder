// TODO: Generalize the representation of the report here.

using System.Collections;
using Fabulous.Reports;

namespace Fabulous.Reports.Elements
{
    public class Report
    {
        private ReportDefinition rd;
        
        public Header ReportHeader;
        public ReportRow[] Rows;
        
        public Report(ReportDefinition theReport)
        {
            this.rd = theReport;
            ReportHeader = new Header(this.rd.Columns);
        }
    }
    
    public class Header
    {
        public string[] ColumnNames;
        
        public Header(Item[] columns)
        {
            ColumnNames = new string[columns.Length];
            
            int colIndex = 0;
            
            foreach (Item column in columns)
            {
                ColumnNames[colIndex] = column.Name;
                colIndex++;
            } 
        }
    }
    
    public class ReportField
    {
    }
    
    public class ReportRow
    {
        public ReportField[] Fields;
        
    }
    
    
}