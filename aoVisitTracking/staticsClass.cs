
using System;
using System.Collections.Generic;
using System.Text;
using Contensive.BaseClasses;

namespace aoVisitTracking
{
    static class statics
    {
        public static string cr = "\n\t";
        public static string cr2 = "\n\t\t";
        public static string cr3 = "\n\t\t\t";
        //
        public const string rnSrcFormId = "srcFormId";
        public const string rnDstFormId = "dstFormId";
        public const string rnButton = "button";
        public const string rnAppId = "appId";
        //
        public const string rnSampleField = "sampleField";
        //
        public const string buttonSaveAndClose = "Save and Close";
        public const string buttonSaveAndContinue = "Save and Continue";
        public const string buttonLogin = "Login";
        public const string buttonContinue = "Continue";
        //
        public const int formIdHome = 1;
        public const int formIdBlank = 2;
        //
        public const string cnApps = "Sample Application Content - Change to your application if needed";
        //
        //=========================================================================
        //  create user error if requestName field is not in doc properties
        //=========================================================================
        //
        public static void checkRequiredFieldText(CPBaseClass cp, string requestName, string fieldCaption)
        {
            try
            {
                if (cp.Doc.GetProperty(requestName, "") == "")
                {
                    cp.UserError.Add("The field " + fieldCaption + " is required.");
                }
            }
            catch (Exception ex)
            {
                cp.Site.ErrorReport(ex, "Unexpected Error in checkRequiredFieldText");
            }
        }
        //
        //=========================================================================
        //  get the field value, from cs if ok, else from stream
        //=========================================================================
        //
        public static string getFormField(CPBaseClass cp, CPCSBaseClass cs, string fieldName, string requestName)
        {
            string returnValue = "";
            try
            {
                if (cp.Doc.IsProperty(requestName))
                {
                    returnValue = cp.Doc.GetText(requestName, "");
                }
                else if (cs.OK())
                {
                    returnValue = cs.GetText(fieldName);
                }
            }
            catch (Exception ex)
            {
                cp.Site.ErrorReport(ex, "Unexpected Error in getFormField");
            }
            return returnValue;
        }
        //
        //=========================================================================
        //  getFormField, variation
        //=========================================================================
        //
        public static string getFormField(CPBaseClass cp, CPCSBaseClass cs, string fieldName)
        {
            return getFormField(cp, cs, fieldName, fieldName);
        }
        //
        //=========================================================================
        //  if date is invalid, set to minValue
        //=========================================================================
        //
        public static DateTime encodeMinDate(DateTime srcDate)
        {
            DateTime returnDate = srcDate;
            if (srcDate < new DateTime(1900, 1, 1))
            {
                returnDate = DateTime.MinValue;
            }
            return returnDate;
        }
        //
        //=========================================================================
        //  if valid date, return the short date, else return blank string 
        //=========================================================================
        //
        public static string getShortDateString(DateTime srcDate)
        {
            string returnString = "";
            DateTime workingDate = encodeMinDate(srcDate);
            if (srcDate > new DateTime(1900, 1, 1))
            {
                returnString = workingDate.ToShortDateString();
            }
            return returnString;
        }
    }
}
