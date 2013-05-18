
using System;
using System.Collections.Generic;
using System.Text;
using Contensive.BaseClasses;

namespace aoVisitTracking
{
    //
    // 1) Change the namespace to the collection name
    // 2) Change this class name to the addon name
    // 3) Create a Contensive Addon record with the namespace apCollectionName.ad
    //
    public class processTracksClass : Contensive.BaseClasses.AddonBaseClass
    {
        //
        // execute method is the only public
        //
        public override object Execute(Contensive.BaseClasses.CPBaseClass cp)
        {
            string s = "Hello World";
            try
            {
                // rqs defined once and passed so when we do ajax integration, the rqs will be passed from
                //      from the ajax call so it can be the rqs of the original page, not the /remoteMethod rqs
                //      you get from cp.doc.RefreshQueryString.
                // srcFormId - passed from a submitting form. Otherwise 0.
                // dstFormId - the next for to display. used for links to new pages. Will be over-ridden
                //      by the formProcessing of srcFormId if it is present.
                // appId - Forms typically save data back to the Db. The 'application' is the table
                //      when the data is saved.
                // rightNow - the date and time when the page is hit. Set once and passed as argument to
                //      enable a test-mode where the time can be hard-coded.
                //
                int srcFormId = cp.Utils.EncodeInteger(cp.Doc.GetProperty(statics.rnSrcFormId, ""));
                int dstFormId = cp.Utils.EncodeInteger(cp.Doc.GetProperty(statics.rnDstFormId, ""));
                int appId = cp.Utils.EncodeInteger(cp.Doc.GetProperty(statics.rnAppId, ""));
                string rqs = cp.Doc.RefreshQueryString;
                DateTime rightNow = DateTime.Now;
                CPCSBaseClass cs = cp.CSNew();
                CPCSBaseClass csSeg = cp.CSNew();
                CPCSBaseClass csVisits = cp.CSNew();
                adminFramework.pageWithNavClass page = new adminFramework.pageWithNavClass();
                DateTime dateLastProcessed;
                DateTime dateOneHourAgo = rightNow.AddHours(-1);
                int segmentId = 0;
                string sqlCriteria = "";
                string sql = "";
                string ReferringSite;
                string ReferringPathPage;
                string copy = "";
                int segmentLandingPageId = 0;
                int visitPageId = 0;
                int visitId = 0;
                int lastVisitId = 0;
                string segmentAllowedPageIdList = "";
                string pageIdTarget = "";
                string trackIdList = "";
                DateTime visitStartDate;
                int trackCnt;
                string trackIdListTarget;
                string segmentName;
                string trackName;
                DateTime dateLastFinishedVisitSinceProcessing;
                //
                //------------------------------------------------------------------------
                // open all the segments, process one at a time
                //------------------------------------------------------------------------
                //
                if (csSeg.Open("Tracking Segments","","",true,"",1,1))
                {
                    //
                    // go through each tracking segment
                    //
                    do
                    {
                        segmentName = csSeg.GetText("name");
                        segmentId = csSeg.GetInteger("id");
                        dateLastProcessed = csSeg.GetDate("dateLastProcessed");
                        ReferringSite = csSeg.GetText("ReferringSite");
                        ReferringPathPage = csSeg.GetText("ReferringPathPage");
                        ReferringPathPage = csSeg.GetText("ReferringPathPage");
                        segmentLandingPageId = csSeg.GetInteger("landingPageId");
                        segmentAllowedPageIdList = "";
                        //
                        // mark the segment processed
                        //
                        csSeg.SetField("dateLastProcessed", rightNow.ToString());
                        //
                        // create pageId list from Tracking Segment Page Rules
                        //
                        sql = "select pageId from vtTrackingSegmentPageRules where trackingSegmentId=" + segmentId;
                        if ( cs.OpenSQL(sql ) )
                        {
                            do
                            {
                                segmentAllowedPageIdList += "," + cs.GetText("pageId" );
                                cs.GoNext();
                            } while ( cs.OK());
                            segmentAllowedPageIdList += ",";
                        }
                        cs.Close();
                        //
                        // test all visits where:
                        //  the last page visit was over an hour ago
                        //  was not a bot
                        //  included cookies
                        //
                        sqlCriteria = ""
                            + "(v.LastVisitTime<" + cp.Db.EncodeSQLDate(dateOneHourAgo) + ")"
                            + " and (v.excludeFromAnalytics=0)"
                            + " and (v.CookieSupport=1)"
                            + "";
                        if (dateLastProcessed > new DateTime(2012, 1, 1))
                        {
                            //
                            // only include visits that finished after the last time this segment was processed
                            // so we do not include any visits that were already counted
                            //
                            dateLastFinishedVisitSinceProcessing = dateLastProcessed.AddHours(-1);
                            sqlCriteria += " and (v.LastVisitTime>" + cp.Db.EncodeSQLDate(dateLastFinishedVisitSinceProcessing) + ")";
                        }
                        if (ReferringSite != "")
                        {
                            copy = ReferringSite;
                            copy = "'%" + cp.Db.EncodeSQLText(copy).Substring(1,copy.Length-2) + "%'";
                            sqlCriteria += "(v.ReferringSite like '%" + copy + "%')";
                        }
                        if (ReferringPathPage != "")
                        {
                            copy = ReferringPathPage;
                            copy = "'%" + cp.Db.EncodeSQLText(copy).Substring(1, copy.Length - 2) + "%'";
                            sqlCriteria += "(v.ReferringPathPage like '%" + copy + "%')";
                        }
                        sql = ""
                            + " select"
                            + " v.id"
                            + " ,v.startTime as visitStartDate"
                            + " ,h.id as hitId"
                            + " ,h.recordId as pageId"
                            + " "
                            + " from ccvisits v left join ccviewings h on h.visitId=v.id"
                            + " "
                            + " where " + sqlCriteria
                            + " "
                            + " order by v.id,h.id"
                            + " ";
                        if ( csVisits.OpenSQL( sql ) )
                        {
                            lastVisitId=0;
                            do{
                                //
                                // next line of the data - might be new visit or might be next page in same visit
                                //
                                visitId = csVisits.GetInteger( "id" );
                                visitStartDate = csVisits.GetDate("visitStartDate").Date;
                                if (lastVisitId != visitId)
                                {
                                    //
                                    // this is another visit, start a new track
                                    //
                                    trackIdList = "";
                                    lastVisitId = visitId;
                                    visitPageId = csVisits.GetInteger("pageId");
                                    if ((segmentLandingPageId == 0) | (segmentLandingPageId == visitPageId))
                                    {
                                        //
                                        // This visit qualifies in the tracking segment, track its hits
                                        //  build the track id list
                                        //
                                        do
                                        {
                                            pageIdTarget = "," + visitPageId.ToString() + ",";
                                            if (segmentAllowedPageIdList.IndexOf(pageIdTarget) >= 0)
                                            {
                                                //
                                                // the page is allowed in this track
                                                //
                                                trackIdListTarget = "," + trackIdList + ",";
                                                if (trackIdListTarget.IndexOf(pageIdTarget) == -1)
                                                {
                                                    //
                                                    // this is the first time this page is in the track
                                                    //
                                                    trackIdList += "," + visitPageId.ToString();
                                                    if (trackIdList.Substring(0, 1) == ",") trackIdList = trackIdList.Substring(1);
                                                    //
                                                    // count an instance of this track for this segment and time period
                                                    //
                                                    sqlCriteria = ""
                                                        + "(trackingSegmentId=" + segmentId + ")"
                                                        + "and(dateStart=" + cp.Db.EncodeSQLDate(visitStartDate) + ")"
                                                        + "and(pageIdList=" + cp.Db.EncodeSQLText(trackIdList) + ")"
                                                        + "";
                                                    if (!cs.Open("tracks", sqlCriteria, "", true, "", 1, 1))
                                                    {
                                                        cs.Insert("tracks");
                                                        cs.SetField("trackingSegmentId", segmentId.ToString());
                                                        cs.SetField("pageIdList", trackIdList);
                                                        cs.SetField("dateStart", visitStartDate.ToShortDateString());
                                                    }
                                                    trackName = "";
                                                    if (dateOneHourAgo.Date <= visitStartDate.Date)
                                                    {
                                                        trackName = "(incomplete) ";
                                                    }
                                                    trackName += "segment '" + segmentName + "' on " + visitStartDate.ToShortDateString() + " for page ID(s) " + trackIdList;
                                                    cs.SetField("name", trackName );
                                                    trackCnt = cs.GetInteger("cnt") + 1;
                                                    cs.SetField("cnt", trackCnt.ToString());
                                                    cs.Close();
                                                }
                                            }
                                            //
                                            csVisits.GoNext();
                                            if (csVisits.OK())
                                            {
                                                visitId = csVisits.GetInteger("id");
                                                visitPageId = csVisits.GetInteger("pageId");
                                            }
                                        } while ((csVisits.OK()) & (lastVisitId == visitId));
                                    }
                                    else
                                    {
                                        csVisits.GoNext();
                                    }
                                }
                                else
                                {
                                    csVisits.GoNext();
                                }
                            } while ( csVisits.OK() );
                        }
                        csVisits.Close();
                        csSeg.GoNext();
                    } while (csSeg.OK());

                }
                csSeg.Close();

            }
            catch (Exception ex)
            {
                errorReport(cp, ex, "execute");
            }
            return s;
        }
        //
        // ===============================================================================
        // handle errors for this class
        // ===============================================================================
        //
        private void errorReport(CPBaseClass cp, Exception ex, string method)
        {
            cp.Site.ErrorReport(ex, "error in addonTemplateCs2005.blankClass.getForm");
        }
    }
}
