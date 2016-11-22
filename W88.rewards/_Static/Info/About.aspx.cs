﻿using System;
using System.Web.UI;
using W88.BusinessLogic.Shared.Helpers;

public partial class _Info_About : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack)
        {
            return;
        }
        var path = Server.MapPath("~").ToLower() + string.Format("\\_Static\\Info\\About\\aboutus.{0}.htm", LanguageHelpers.SelectedLanguage);
        htmltext.Text = System.IO.File.ReadAllText(path);       
    }
}