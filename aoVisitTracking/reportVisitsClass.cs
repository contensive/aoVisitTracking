using System;
using System.Collections.Generic;
using System.Text;
using Contensive.BaseClasses;
using adminFramework;
//
namespace aoVisitTracking
{
    //
    // 1) Change the namespace to the collection name
    // 2) Change this class name to the addon name
    // 3) Create a Contensive Addon record with the namespace apCollectionName.ad
    // 3) add reference to CPBase.DLL, typically installed in c:\program files\kma\contensive\
    //
    public class reportVisitsClass  : Contensive.BaseClasses.AddonBaseClass
    {
        //
        // execute method is the only public
        //
        public override object Execute(Contensive.BaseClasses.CPBaseClass cp)
        {
            string returnHtml = "";
            try
            {
                formSimpleClass doc = new formSimpleClass();
                reportTimeLineChartClass report = new reportTimeLineChartClass();
                string sql;
                CPCSBaseClass cs = cp.CSNew();
                int dateNumber;
                //int timeNumber;
                double plotValue;
                DateTime plotDate = new DateTime();
                DateTime zeroDay = new DateTime( 1899,12,30 );
                DateTime rightNow = DateTime.Now;
                double dateNumberEnd= (rightNow-zeroDay).Days-1;
                double dateNumberStart= dateNumberEnd - 365;
                //
                report.addColumn();
                report.columnCaption = "column1 caption";
                report.columnCaptionClass = "column1CaptionClass";
                //
                sql = "select "
                    + " DateNumber,TimeNumber,Visits,PagesViewed,NewVisitorVisits,AveTimeOnSite,SinglePageVisits,AuthenticatedVisits"
                    + " from"
                    + " ccVisitSummary"
                    + " where"
                    + " (TimeDuration=24)"
                    + " AND(DateNumber>=" + dateNumberStart.ToString() + ")"
                    + " AND(DateNumber<=" + dateNumberEnd.ToString() + ")"
                    + " order by"
                    + " DateNumber, TimeNumber";
                if (cs.OpenSQL(sql))
                {
                    
                    do 
                    {
                        dateNumber = cs.GetInteger( "dateNumber");

                        //timeNumber = int( cs.GetNumber("timeNumber") + 0.5);
                        plotValue = cs.GetNumber( "visits" );
                        plotDate = new DateTime(1900,1,1).AddDays(dateNumber);
                        report.addRow();
                        report.rowDate = plotDate;
                        report.setCell(plotValue);
                        cs.GoNext();
                    } while ( cs.OK());
                }
                cs.Close();
                //
                doc.body = report.getHtml(cp);
                doc.title = "Visits";
                doc.addFormButton( "Refresh" );
                returnHtml = doc.getHtml(cp);
            }
            catch( Exception ex)
            {
            }

            return returnHtml;
        }
    }
}
