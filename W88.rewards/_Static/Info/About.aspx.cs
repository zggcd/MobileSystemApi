﻿using System;
using System.Web.UI;
using W88.BusinessLogic.Rewards.Helpers;

public partial class _Info_About : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack)
        {
            return;
        }
        var path =  string.Format("{0}\\_Static\\Info\\About\\aboutus.{1}.htm", Server.MapPath("~").ToLower(), RewardsHelper.ContentLanguage);
        htmltext.Text = System.IO.File.ReadAllText(path);       
    }
}