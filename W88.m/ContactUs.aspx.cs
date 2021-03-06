﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class _ContactUs : BasePage
{
    protected System.Xml.Linq.XElement xeResources = null;

    protected void Page_Load(object sender, EventArgs e)
    {
        System.Web.UI.WebControls.Literal litScript = (System.Web.UI.WebControls.Literal)Page.FindControl("litScript");
        //commonCulture.appData.getLocalResource(out xeResources);
        commonCulture.appData.getRootResource("ContactUs.aspx", out xeResources);
        SetTitle(commonCulture.ElementValues.getResourceString("contactUs", commonVariables.LeftMenuXML));

        if (!Page.IsPostBack)
        {
            if (!string.IsNullOrEmpty(HttpContext.Current.Request.QueryString.Get("Error")) && !string.IsNullOrEmpty(commonVariables.GetSessionVariable("Error")))
            {
                Session.Remove("Error");
                if (litScript != null)
                {
                    litScript.Text += string.Format("<script type='text/javascript'>alert('{0}');</script>", HttpContext.Current.Request.QueryString.Get("Error"));
                }
            }

            if (!string.IsNullOrEmpty(HttpContext.Current.Request.QueryString.Get("lang"))) { commonVariables.SelectedLanguage = HttpContext.Current.Request.QueryString.Get("lang"); }

            aSkype.HRef = commonCulture.ElementValues.getResourceString("lnkSkype", xeResources);
            aEmail.HRef = commonCulture.ElementValues.getResourceString("lnkEmail", xeResources);
            aBanking.HRef = commonCulture.ElementValues.getResourceString("lnkBanking", xeResources);
            aPhone.HRef = commonCulture.ElementValues.getResourceString("lnkPhone", xeResources);
            phoneMessage.InnerHtml = commonCulture.ElementValues.getResourceString("lblPhoneMessage", xeResources);

            if (commonVariables.CDNCountryCode.Equals("MY", StringComparison.OrdinalIgnoreCase))
            {
                aEmail.HRef = commonCulture.ElementValues.getResourceString("lnkEmailMY", xeResources);
                aPhone.HRef = commonCulture.ElementValues.getResourceString("lnkPhoneMY", xeResources);
                phoneMessage.InnerHtml = commonCulture.ElementValues.getResourceString("lblPhoneMessageMY", xeResources);
            }
            else if (string.IsNullOrEmpty(commonCulture.ElementValues.getResourceString("lnkPhone", xeResources)))
            {
                liPhone.Visible = false;
            }
        }
    }
}