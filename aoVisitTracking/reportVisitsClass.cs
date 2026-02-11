using Contensive.BaseClasses;
using System;
using static Contensive.BaseClasses.LayoutBuilder.LayoutBuilderBaseClass;
//
namespace aoVisitTracking {
    /// <summary>
    /// addon class
    /// </summary>
    public class reportVisitsClass : Contensive.BaseClasses.AddonBaseClass {
        /// <summary>
        /// addon execute method
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(Contensive.BaseClasses.CPBaseClass cp) {
            string returnHtml = "";
            try {
                var doc = cp.AdminUI.CreateLayoutBuilder();
                var report = cp.AdminUI.CreateLayoutChartTimeLine();

                string sql;
                CPCSBaseClass cs = cp.CSNew();
                int dateNumber;
                //int timeNumber;
                //double plotValue;
                DateTime plotDate = new DateTime();
                DateTime zeroDay = new DateTime(1899, 12, 30);
                DateTime rightNow = DateTime.Now;
                double dateNumberEnd = (rightNow - zeroDay).Days - 1;
                double dateNumberStart = dateNumberEnd - 365;
                //
                report.addColumn();
                report.columnCaption = "Visits";
                report.columnCaptionClass = AfwStyles.afwWidth100px;
                report.columnCellClass = AfwStyles.afwTextAlignRight;
                report.addColumn();
                report.columnCaption = "Bounces";
                report.columnCaptionClass = AfwStyles.afwWidth100px;
                report.columnCellClass = AfwStyles.afwTextAlignRight;
                report.addColumn();
                report.columnCaption = "New";
                report.columnCaptionClass = AfwStyles.afwWidth100px;
                report.columnCellClass = AfwStyles.afwTextAlignRight;
                report.addColumn();
                report.columnCaption = "Auth";
                report.columnCaptionClass = AfwStyles.afwWidth100px;
                report.columnCellClass = AfwStyles.afwTextAlignRight;
                report.addColumn();
                report.columnCaption = "Mobile";
                report.columnCaptionClass = AfwStyles.afwWidth100px;
                report.columnCellClass = AfwStyles.afwTextAlignRight;
                //report.addColumn();
                //report.columnCaption = "Known Bots";
                //report.columnCaptionClass = AfwStyles.afwWidth100px;
                //report.columnCellClass = AfwStyles.afwTextAlignRight;
                //report.addColumn();
                //report.columnCaption = "No Cookie";
                //report.columnCaptionClass = AfwStyles.afwWidth100px;
                //report.columnCellClass = AfwStyles.afwTextAlignRight;
                report.chartWidth = 800;
                //
                sql = "select "
                    + " DateNumber,TimeNumber,Visits,PagesViewed,NewVisitorVisits,AveTimeOnSite,SinglePageVisits,AuthenticatedVisits,mobileVisits,botVisits,noCookieVisits"
                    + " from"
                    + " ccVisitSummary"
                    + " where"
                    + " (TimeDuration=24)"
                    + " AND(DateNumber>=" + dateNumberStart.ToString() + ")"
                    + " AND(DateNumber<=" + dateNumberEnd.ToString() + ")"
                    + " order by"
                    + " DateNumber, TimeNumber";
                if (cs.OpenSQL(sql)) {

                    do {
                        dateNumber = cs.GetInteger("dateNumber");
                        //timeNumber = int( cs.GetNumber("timeNumber") + 0.5);
                        plotDate = new DateTime(1899, 12, 30).AddDays(dateNumber);
                        report.addRow();
                        report.rowDate = plotDate;
                        report.setCell(cs.GetNumber("visits"));
                        report.setCell(cs.GetNumber("SinglePageVisits"));
                        report.setCell(cs.GetNumber("NewVisitorVisits"));
                        report.setCell(cs.GetNumber("AuthenticatedVisits"));
                        report.setCell(cs.GetNumber("mobileVisits"));
                        //report.setCell(cs.GetNumber("botVisits"));
                        //report.setCell(cs.GetNumber("noCookieVisits"));
                        cs.GoNext();
                    } while (cs.OK());
                }
                cs.Close();
                //
                doc.body = report.getHtml(cp);
                doc.title = "Visits";
                doc.addFormButton("Refresh");
                doc.isOuterContainer = true;
                returnHtml = doc.getHtml(cp);
            } catch (Exception ex) {
            }

            return returnHtml;
        }
    }
}
