﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services;
using System.Data;
using System.Diagnostics;
using System.Text;

public partial class Catalogue_Redeem : BasePage
{

    protected void Page_Init(object sender, EventArgs e)
    {


    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(commonVariables.CurrentMemberSessionId))
        {
          
            lblPoint.InnerText = "Points Bal: " + getCurrentPoints().ToString();
            divLevel.Visible = true;
        }


     //   string productid = Request.QueryString.Get("productId");

          
      

        //string userMemberId = string.IsNullOrEmpty((string) Session["MemberId"]) ? "" : (string) Session["MemberId"];
        //string countryCode = string.IsNullOrEmpty((string) Session["CountryCode"])
        //    ? "0"
        //    : (string) Session["CountryCode"];
        //string currencyCode = string.IsNullOrEmpty((string) Session["CurrencyCode"])
        //    ? "0"
        //    : (string) Session["CurrencyCode"];
        //string riskId = string.IsNullOrEmpty((string) Session["RiskId"]) ? "0" : (string) Session["RiskId"];
        //string productID = HttpContext.Current.Request.QueryString.Get("id");
        //System.Web.HttpContext.Current.Session["productId"] = productID;

        //using (RewardsServices.RewardsServicesClient sClient = new RewardsServices.RewardsServicesClient())
        //{



        //}


        
        string userMemberId = string.IsNullOrEmpty((string)Session["MemberId"]) ? "" : (string)Session["MemberId"];
        string countryCode = string.IsNullOrEmpty((string)Session["CountryCode"])
            ? "0"
            : (string)Session["CountryCode"];
        string currencyCode = string.IsNullOrEmpty((string)Session["CurrencyCode"])
            ? "0"
            : (string)Session["CurrencyCode"];
        string riskId = string.IsNullOrEmpty((string)Session["RiskId"]) ? "0" : (string)Session["RiskId"];
        string productID = HttpContext.Current.Request.QueryString.Get("productId");
        System.Web.HttpContext.Current.Session["productId"] = productID;
       

        lblproductid.Value = productID;
        
        using (RewardsServices.RewardsServicesClient sClient = new RewardsServices.RewardsServicesClient())
        {
            System.Data.DataSet ds = sClient.getProductDetail(productID, commonVariables.SelectedLanguage, riskId);

            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    foreach (System.Data.DataRow dr in ds.Tables[0].Rows)
                    {
                        System.Web.HttpContext.Current.Session["productType"] = dr["productType"].ToString();
                        System.Web.HttpContext.Current.Session["amountLimit"] = dr["amountLimit"];
                        System.Web.HttpContext.Current.Session["categoryId"] = dr["categoryId"].ToString();
                        System.Web.HttpContext.Current.Session["categoryIdReload"] = dr["categoryId"].ToString();
                        System.Web.HttpContext.Current.Session["currencyValidity"] = currencyCode;

                        dr["pointsRequired"] =
                            Convert.ToInt32(dr["pointsRequired"].ToString().Replace(" ", string.Empty));

                        if (!ds.Tables[0].Columns.Contains("pointsLeveldiscount"))
                        {
                            ds.Tables[0].Columns.Add("pointsLeveldiscount");
                            dr["pointsLeveldiscount"] = 0;
                        }

                        if (!ds.Tables[0].Columns.Contains("pointsRequired2"))
                        {
                            ds.Tables[0].Columns.Add("pointsRequired2");
                            dr["pointsRequired2"] = dr["pointsRequired"];
                        }

                        if (!ds.Tables[0].Columns.Contains("discountPercentage"))
                        {
                            ds.Tables[0].Columns.Add("discountPercentage");
                            dr["discountPercentage"] = 0;
                        }

                        if (dr["discountPoints"] != DBNull.Value)
                        {
                            System.Web.HttpContext.Current.Session["pointsRequired"] = dr["discountPoints"];
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(commonVariables.CurrentMemberSessionId) &&
                                dr["productType"].ToString() != "1")
                            {
                                //grap member point level
                                string pointLevel = sClient.getMemberPointLevelFE(userMemberId);
                                int pointLevelDiscount = sClient.getMemberPointLevelDiscount(
                                    commonVariables.OperatorId, currencyCode, pointLevel);

                                double percentage = Convert.ToDouble(pointLevelDiscount) / 100;
                                int normalPoint = int.Parse(dr["pointsRequired"].ToString());

                                double points = Math.Floor(normalPoint * (1 - percentage));
                                int pointAfterLevelDiscount = Convert.ToInt32(points);

                                dr["pointsRequired"] = pointAfterLevelDiscount;
                                dr["pointsLeveldiscount"] = pointAfterLevelDiscount;
                                dr["discountPercentage"] = pointLevelDiscount;

                                System.Web.HttpContext.Current.Session["pointsRequired"] = dr["pointsRequired"];
                                System.Web.HttpContext.Current.Session["pointsLeveldiscount"] = dr["pointsRequired"];
                            }
                            else
                                System.Web.HttpContext.Current.Session["pointsRequired"] = dr["pointsRequired"];
                        }

                        dr["currencyValidity"] = currencyCode;
                        dr["imageName"] =
                            Convert.ToString(
                                System.Configuration.ConfigurationManager.AppSettings.Get("ImagesDirectoryPath") +
                                "Product/" + dr["imageName"]);


                        if (!string.IsNullOrEmpty(riskId))
                        {
                            //category
                            dr["redemptionValidityCat"] += ",";
                            if (dr["redemptionValidityCat"].ToString().ToUpper() != "ALL,")
                            {
                                if (((string)dr["redemptionValidityCat"]).IndexOf(riskId + ",") < 0)
                                    dr["redemptionValidityCat"] = "0";
                                else
                                    dr["redemptionValidityCat"] = "1";
                            }
                            else
                                dr["redemptionValidityCat"] = "1";


                            dr["redemptionValidity"] += ",";
                            if (dr["redemptionValidity"].ToString().ToUpper() != "ALL,")
                            {
                                if (((string)dr["redemptionValidity"]).IndexOf(riskId + ",") < 0)
                                    dr["redemptionValidity"] = "0";
                                else
                                    dr["redemptionValidity"] = "1";
                            }
                            else
                                dr["redemptionValidity"] = "1";

                        }
                        else
                        {
                            dr["redemptionValidity"] += "0";
                            dr["redemptionValidityCat"] += "0";
                        }

                        imgPic.ImageUrl = dr["imageName"].ToString();
                        lblPointCenter.Text = String.Format("{0:#,###,##0.##}", dr["pointsRequired"].ToString()) +
                                              " Points";

                   
                        lblName.Text = dr["productName"].ToString();
                       // name.Value = dr["productName"].ToString();

                      //  lblDescription.Text = "<p>" + (dr["productDescription"].ToString()) + "</p>";

                        if (!string.IsNullOrEmpty(dr["deliveryPeriod"].ToString()))
                        {
                            lblDelivery.Text =  (dr["deliveryPeriod"].ToString()) + " Day(s).";
                            DeliveryDiv.Visible = true;
                        }


                        //freebet only
                        if (!string.IsNullOrEmpty(dr["currencyValidity"].ToString()))
                        {
                            lblCurrency.Text = (dr["currencyValidity"].ToString());
                            CurrencyDiv.Visible = true;
                        }


                        
                    }




                }
            }


        }






    }


    [WebMethod()]
    [System.Web.Script.Services.ScriptMethod(
    ResponseFormat = System.Web.Script.Services.ResponseFormat.Json)]
    public static int getCurrentPoints()
    {
        int total = 0;
        int claim = 0;
        int current = 0;
        int cart = 0;

        try
        {

            if (!string.IsNullOrEmpty((string)HttpContext.Current.Session["MemberId"]))
            {
                using (RewardsServices.RewardsServicesClient sClient = new RewardsServices.RewardsServicesClient())
                {
                    string strMemberCode = string.IsNullOrEmpty(System.Web.HttpContext.Current.Session["MemberCode"] as string) ? string.Empty : Convert.ToString(System.Web.HttpContext.Current.Session["MemberCode"]);

                    System.Data.DataSet ds = sClient.getRedemptionDetail(commonVariables.OperatorId, strMemberCode);

                    if (ds.Tables.Count > 0)
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            total = int.Parse(ds.Tables[0].Rows[0][0].ToString());

                            if (ds.Tables[1].Rows.Count > 0)
                            {
                                claim = int.Parse(ds.Tables[1].Rows[0][0].ToString());
                            }

                            if (ds.Tables[2].Rows.Count > 0)
                            {
                                cart = int.Parse(ds.Tables[2].Rows[0][0].ToString());
                            }
                            claim = claim + cart;
                        }

                    }
                    current = total - claim;

                    HttpContext.Current.Session["pointsBalance"] = current;

                    return current;

                }
            }
            else
            {
                return 0;
            }
        }
        catch (Exception ex)
        {
            //throw;
            return 0;
        }
    }
}

    



