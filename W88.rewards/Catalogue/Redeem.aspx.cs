﻿using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Text;
using System.Web.UI;
using W88.Rewards.BusinessLogic.Rewards.Models;
using W88.Rewards.BusinessLogic.Shared.Helpers;
using W88.Utilities.Constant;
using W88.Utilities.Log.Helpers;
using W88.WebRef.RewardsServices;

public partial class Catalogue_Redeem : CatalogueBasePage
{
    protected string AlertCode = string.Empty;
    protected string AlertMessage = string.Empty;
    protected string ProductType = string.Empty;
    protected string VipOnly = "Hey there! This rewards redemption is only for VIP-Gold and above HOUSE OF HIGHROLLERS, YOU DESERVED IT!";
    private readonly string _operatorId = Settings.OperatorId.ToString(CultureInfo.InvariantCulture);
    protected ProductDetails ProductDetails = CookieHelpers.ProductCookie;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
        {    
            SetLabels();
            SetFields();
            GetProductDetails();
        }
    }

    protected void RedeemButtonOnClick(object sender, EventArgs e)
    {
        AlertCode = string.Empty;
        AlertMessage = string.Empty;
        var memberCode = UserSessionInfo == null ? string.Empty : UserSessionInfo.MemberCode;
        var productTypeEnum = (ProductTypeEnum)int.Parse(ProductDetails.ProductType);
        var quantitytext = tbQuantity.Text.Trim();
        int quantity;

        var isParsed = int.TryParse(quantitytext, out quantity);
        if (!isParsed)
        {
            AlertCode = "FAIL";
            AlertMessage = HttpContext.GetLocalResourceObject(LocalResx, "lbl_invalid_number").ToString();     
            return;
        }

        quantity = int.Parse(quantitytext);
        if (quantity < 1)
        {
            AlertCode = "FAIL";
            AlertMessage = HttpContext.GetLocalResourceObject(LocalResx, "lbl_invalid_at_least").ToString();
            return;
        }
        
        if (!CheckIsValid())
        {
            return;
        }

        #region redeem product
        try
        {
            using (var client = new RewardsServicesClient())
            {
                RedemptionResponse response = null;

                switch (productTypeEnum)
                {
                    case ProductTypeEnum.Freebet:
                        var freebetRequest = GetRedemptionFreebetRequest();
                        response = client.RedemptionFreebet(freebetRequest);
                        break;
                    case ProductTypeEnum.Normal:
                        var normalRequest = GetRedemptionNormalRequest();
                        response = client.RedemptionNormal(normalRequest);
                        break;
                    case ProductTypeEnum.Online:
                        var onlineRequest = GetRedemptionOnlineRequest();
                        response = client.RedemptionOnline(onlineRequest);
                        break;
                }

                if (response == null)
                {
                    AlertMessage = (string)HttpContext.GetLocalResourceObject(LocalResx, "lblPointCheckError");
                    return;
                }

                switch (response.Result)
                {
                    case RedemptionResultEnum.ConcurrencyDetected:
                        break;
                    case RedemptionResultEnum.LimitReached:
                        AlertCode = "FAIL";
                        AlertMessage = (string)HttpContext.GetLocalResourceObject(LocalResx, "lbl_redemption_limit_reached");
                        break;
                    case RedemptionResultEnum.VIPSuccessLimitReached:
                        AlertCode = "FAIL";
                        AlertMessage = (string)HttpContext.GetLocalResourceObject(LocalResx, "lbl_redemption_success_limit_reached");
                        break;
                    case RedemptionResultEnum.VIPProcessingLimitReached:
                        AlertCode = "FAIL";
                        AlertMessage = (string)HttpContext.GetLocalResourceObject(LocalResx, "lblPoints_insufficient");
                        break;
                    case RedemptionResultEnum.PointIsufficient:
                        AlertCode = "FAIL";
                        AlertMessage = (string)HttpContext.GetLocalResourceObject(LocalResx, "lblPoints_insufficient");
                        break;
                    case RedemptionResultEnum.Success:
                        AlertCode = "SUCCESS";
                        if (productTypeEnum == ProductTypeEnum.Freebet) //Freebet success
                        {
                            foreach (var redemptionItemId in response.RedemptionIds)
                            {
                                (new MailHelper()).SendMail(memberCode,
                                    redemptionItemId.ToString(CultureInfo.InvariantCulture));
                            }

                            AlertMessage = (string)HttpContext.GetLocalResourceObject(LocalResx, "lbl_redeem_successProcessed");
                        }
                        else
                        {
                            AlertMessage = (string)HttpContext.GetLocalResourceObject(LocalResx, "lbl_redeem_success_submit");
                        }                         
                        break;
                    case RedemptionResultEnum.UnknownError:
                        break;
                    case RedemptionResultEnum.PointCheckError:
                        AlertCode = "FAIL";
                        AlertMessage = (string)HttpContext.GetLocalResourceObject(LocalResx, "lblPointCheckError");
                        break;
                }

                var pointsRequired = ProductDetails == null ? string.Empty : ProductDetails.PointsRequired;
                var redeemId = response.RedemptionIds != null ? String.Join("|", response.RedemptionIds.ToArray()) : "";
                var remark = "Product Id:" + lblproductid.Value + "; Points Required:" + pointsRequired + "; Quantity:" + tbQuantity.Text.Trim();
                var detail = ";Redeem Result:" + response.Result + ";RedeemId:" + redeemId + ";Type:" + productTypeEnum;
                var sessionId = HasSession ? MemberSession.Token : string.Empty;                   
                AuditTrail.AppendLog(memberCode, "/Catalogue/Redeem.aspx", "Redeem Now", "Catalogue/Redeem", string.Empty, detail, "-", string.Empty, remark, string.Empty, sessionId, true);
            }
            
            SetMemberRewardsInfo();
            SetLabels();

            var scriptBuilder = new StringBuilder();
            scriptBuilder.Append("setTimeout(function() {showMessage('")
                .Append(AlertCode + "','")
                .Append(AlertMessage + "');}, 300);");
            ScriptManager.RegisterStartupScript(Page, GetType(), (new Guid()).ToString(), scriptBuilder.ToString(), true);
        }
        catch (Exception exception)
        {
            AlertCode = "FAIL";
            AlertMessage = HttpContext.GetLocalResourceObject(LocalResx, "lbl_Exception").ToString();          
            AuditTrail.AppendLog(memberCode, "/Catalogue/Redeem.aspx", "Redeem Now", "Catalogue/Redeem", string.Empty, string.Empty, string.Empty, exception.Message, (new Guid()).ToString(), string.Empty, string.Empty, true);
        }
        #endregion
    }

    protected override void SetLabels()
    {
        #region labels
        if (!HasSession && UserSessionInfo == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(MemberSession.FirstName))
        {
            usernameLabel.Visible = false;
        }
        else
        {
            usernameLabel.InnerText = UserSessionInfo.MemberCode;
        }
        var pointsLabelText = (string)HttpContext.GetLocalResourceObject(LocalResx, "lbl_points");
        var stringBuilder = new StringBuilder();

        stringBuilder.Append(pointsLabelText)
            .Append(": ")
            .Append(MemberRewardsInfo != null ? Convert.ToString(MemberRewardsInfo.CurrentPoints) : "0");
        pointsLabel.InnerText = stringBuilder.ToString();

        var pointLevelLabelText = (string)HttpContext.GetLocalResourceObject(HeaderResx, "lbl_point_level");
        stringBuilder = new StringBuilder();
        stringBuilder.Append(pointLevelLabelText)
            .Append(" ")
            .Append(MemberRewardsInfo != null ? Convert.ToString(MemberRewardsInfo.CurrentPointLevel) : "0");
        pointLevelLabel.InnerText = stringBuilder.ToString();
        divLevel.Visible = true;
        #endregion
    }

    private void SetFields()
    {
        #region fields
        const string colon = ":";
        lbcat.Text = HttpContext.GetLocalResourceObject(LocalResx, "lbl_category") + colon;
        lbproduct.Text = HttpContext.GetLocalResourceObject(LocalResx, "lbl_product") + colon;
        lbcurr.Text = HttpContext.GetLocalResourceObject(LocalResx, "lbl_currency") + colon;
        lbpoint.Text = HttpContext.GetLocalResourceObject(LocalResx, "lbl_points") + colon;
        lbperiod.Text = HttpContext.GetLocalResourceObject(LocalResx, "lbl_delivery_period") + colon;
        lbqty.Text = HttpContext.GetLocalResourceObject(LocalResx, "lbl_quantity") + colon;
        lbaccount.Text = HttpContext.GetLocalResourceObject(LocalResx, "lbl_account") + colon;
        tbRName.Attributes.Add("PLACEHOLDER", HttpContext.GetLocalResourceObject(LocalResx, "lbl_enter_name").ToString());
        tbAddress.Attributes.Add("PLACEHOLDER", HttpContext.GetLocalResourceObject(LocalResx, "lbl_enter_address").ToString());
        tbPostal.Attributes.Add("PLACEHOLDER", HttpContext.GetLocalResourceObject(LocalResx, "lbl_enter_postal").ToString());
        tbCity.Attributes.Add("PLACEHOLDER", HttpContext.GetLocalResourceObject(LocalResx, "lbl_enter_city").ToString());
        tbCountry.Attributes.Add("PLACEHOLDER", HttpContext.GetLocalResourceObject(LocalResx, "lbl_enter_country").ToString());
        tbContact.Attributes.Add("PLACEHOLDER", HttpContext.GetLocalResourceObject(LocalResx, "lbl_enter_contact").ToString());
        redeemButton.Text = HttpContext.GetLocalResourceObject(LocalResx, "lbl_redeem_now").ToString();
        redeemButton.Visible = true;
        #endregion
    }

    private RedemptionFreebetRequest GetRedemptionFreebetRequest()
    {
        var request = new RedemptionFreebetRequest();
        request.OperatorId = _operatorId;
        request.MemberCode = UserSessionInfo == null ? "" : UserSessionInfo.MemberCode;
        request.ProductId = lblproductid.Value;
        request.CategoryId = int.Parse(string.IsNullOrEmpty(ProductDetails.CategoryId) ? "" : ProductDetails.CategoryId);
        request.RiskId = MemberSession == null ? "0" : MemberSession.RiskId;
        request.Currency = string.IsNullOrEmpty(ProductDetails.CurrencyCode) ? "0" : ProductDetails.CurrencyCode;
        request.PointRequired = int.Parse(string.IsNullOrEmpty(ProductDetails.PointsRequired) ? "" : ProductDetails.PointsRequired);
        request.Quantity = int.Parse(tbQuantity.Text.Trim());
        request.CreditAmount = Convert.ToDecimal(string.IsNullOrEmpty(ProductDetails.AmountLimit) ? "" : ProductDetails.AmountLimit);
        return request;
    }

    private RedemptionNormalRequest GetRedemptionNormalRequest()
    {
        var request = new RedemptionNormalRequest();
        request.OperatorId = _operatorId;
        request.MemberCode = UserSessionInfo == null ? "" : UserSessionInfo.MemberCode;
        request.ProductId = lblproductid.Value;
        request.CategoryId = int.Parse(string.IsNullOrEmpty(ProductDetails.CategoryId) ? "" : ProductDetails.CategoryId);
        request.RiskId = MemberSession == null ? "0" : MemberSession.RiskId;
        request.Currency = string.IsNullOrEmpty(ProductDetails.CurrencyCode) ? "0" : ProductDetails.CurrencyCode;
        request.PointRequired = int.Parse(string.IsNullOrEmpty(ProductDetails.PointsRequired) ? "" : ProductDetails.PointsRequired);
        request.Quantity = int.Parse(tbQuantity.Text.Trim());
        request.Name = tbRName.Text.Trim();
        request.ContactNumber = tbContact.Text.Trim();
        request.Address = tbAddress.Value.Trim();
        request.PostalCode = tbPostal.Text.Trim();
        request.City = tbCity.Text.Trim();
        request.Country = tbCountry.Text.Trim();
        return request;
    }

    private RedemptionOnlineRequest GetRedemptionOnlineRequest()
    {
        var request = new RedemptionOnlineRequest();
        request.OperatorId = _operatorId;
        request.MemberCode = UserSessionInfo == null ? "" : UserSessionInfo.MemberCode;
        request.ProductId = lblproductid.Value;
        request.CategoryId = int.Parse(string.IsNullOrEmpty(ProductDetails.CategoryId) ? "" : ProductDetails.CategoryId);
        request.RiskId = MemberSession == null ? "0" : MemberSession.RiskId;
        request.Currency = string.IsNullOrEmpty(ProductDetails.CurrencyCode) ? "0" : ProductDetails.CurrencyCode;
        request.PointRequired = int.Parse(string.IsNullOrEmpty(ProductDetails.PointsRequired) ? "" : ProductDetails.PointsRequired);
        request.Quantity = int.Parse(tbQuantity.Text.Trim());
        request.AimId = tbAccount.Text.Trim();
        return request;
    }

    private bool CheckIsValid()
    {
        var productType = (ProductTypeEnum)int.Parse(ProductDetails.ProductType);
    
        switch (productType)
        {
            case ProductTypeEnum.Freebet:
                return true;
            case ProductTypeEnum.Normal:
                {
                    var name = tbRName.Text.Trim();
                    var contact = tbContact.Text.Trim();
                    var address = tbAddress.Value.Trim();
                    var postal = tbPostal.Text.Trim();
                    var city = tbCity.Text.Trim();
                    var country = tbCountry.Text.Trim();

                    if (string.IsNullOrEmpty(name))
                    {
                        return false;
                    }
                    if (string.IsNullOrEmpty(contact))
                    {
                        return false;
                    }
                    if (string.IsNullOrEmpty(address))
                    {
                        return false;
                    }
                    if (string.IsNullOrEmpty(postal))
                    {
                        return false;
                    }
                    if (string.IsNullOrEmpty(city))
                    {
                        return false;
                    }

                    return !string.IsNullOrEmpty(country);
                }
            case ProductTypeEnum.Wishlist:               
                return false;               
            case ProductTypeEnum.Online:
                return !string.IsNullOrEmpty(tbAccount.Text.Trim());
        }
        return false;
    }

    private void GetProductDetails()
    {
        var memberCode = UserSessionInfo == null ? "" : UserSessionInfo.MemberCode;
        var countryCode = MemberSession == null ? "0" : MemberSession.CountryCode;
        var currencyCode = MemberSession == null ? "0" : MemberSession.CurrencyCode;

        try
        {
            ProductType = ProductDetails.ProductType;
            if (ProductType == "1" && (ProductDetails.CurrencyCode != currencyCode))
            {
                Response.Redirect("/Catalogue?categoryId=53&sortBy=2");
                return;
            }

            if (ProductDetails.CountryCode.Trim() != "All")
            {
                if (!ProductDetails.CountryCode.Contains(countryCode))
                {
                    Response.Redirect("/Catalogue?categoryId=" + ProductDetails.CategoryId + "&sortBy=2");
                    return;
                }
            }

            if (!(ProductDetails.RedemptionValidity == "1" && ProductDetails.RedemptionValidityCategory == "1"))
            {
                AlertCode = "VIP";
                VipOnly = HttpContext.GetLocalResourceObject(LocalResx, "lbl_redeem_vip").ToString();
                return;
            }

            lblproductid.Value = ProductDetails.ProductId;
            /**
                freebet show  currency, hide recipient panel , hide delivery, hide account
                normal product show recipient, show delivery if any,  hide currency, hide account
                online show account, hide delivery, hide recipient, hide currency
            **/
            switch (ProductType)
            {
                case "1": //freebet
                    CurrencyDiv.Visible = true;
                    RecipientDiv.Visible = false;
                    DeliveryDiv.Visible = false;
                    AccountDiv.Visible = false;
                    break;
                case "2": //normal
                    RecipientDiv.Visible = true;
                    CurrencyDiv.Visible = false;
                    AccountDiv.Visible = false;
                    break;
                case "3": //wishlist same as normal
                    RecipientDiv.Visible = true;
                    CurrencyDiv.Visible = false;
                    AccountDiv.Visible = false;
                    break;
                case "4": //online
                    AccountDiv.Visible = true;
                    CurrencyDiv.Visible = false;
                    RecipientDiv.Visible = false;
                    DeliveryDiv.Visible = false;
                    break;
            }

            //vip cannot select quantity
            var vipCategoryId = ConfigurationManager.AppSettings.Get("vipCategoryId");
            if (ProductDetails.CategoryId == vipCategoryId)
            {
                tbQuantity.Enabled = false;
            }

            imgPic.ImageUrl = ProductDetails.ImageUrl;

            if (!string.IsNullOrEmpty(ProductDetails.DiscountPoints))
            {
                lblBeforeDiscount.Text =
                    string.Format("{0:#,###,##0.##}", ProductDetails.PointsRequired) + " " +
                    HttpContext.GetLocalResourceObject(LocalResx, "lblPoints");
                lblPointCenter.Text = string.Format("{0:#,###,##0.##}", ProductDetails.DiscountPoints) +
                                      (string)HttpContext.GetLocalResourceObject(LocalResx, "lblPoints");
            }
            else
            {
                lblBeforeDiscount.Text = "";
                lblPointCenter.Text = string.Format("{0:#,###,##0.##}", ProductDetails.PointsRequired) +
                                      " " + (string)HttpContext.GetLocalResourceObject(LocalResx, "lblPoints");
            }

            lblName.Text = ProductDetails.ProductName;
            lblCategory.Text = ProductDetails.ProductCategoryName;

            if (!string.IsNullOrEmpty(ProductDetails.DeliveryPeriod))
            {
                lblDelivery.Text = ProductDetails.DeliveryPeriod;
                DeliveryDiv.Visible = true;
            }

            if (!string.IsNullOrEmpty(ProductDetails.CurrencyCode))
            {
                lblCurrency.Text = ProductDetails.CurrencyCode;
            }

            #region memberInfo
            var redemptionDetails = RewardsHelper.GetMemberRedemptionDetails(memberCode);
            if (redemptionDetails == null)
            {
                return;
            }

            if (ProductType == "2" || ProductType == "3") //normal & wishlist
            {
                tbRName.Text = redemptionDetails.FullName;
                tbAddress.Value = redemptionDetails.Address;                   
                tbPostal.Text = redemptionDetails.Postal;
                tbCity.Text = redemptionDetails.City;
                tbCountry.Text = redemptionDetails.CountryCode;
                tbContact.Text = redemptionDetails.Mobile;
            }
            #endregion
        }
        catch (Exception exception)
        {
            AlertCode = "FAIL";
            AlertMessage = HttpContext.GetLocalResourceObject(LocalResx, "lbl_Exception").ToString();
            AuditTrail.AppendLog(memberCode, "/Catalogue/Redeem.aspx", "Redeem Now", "Catalogue/Redeem", string.Empty, string.Empty, string.Empty, exception.Message, (new Guid()).ToString(), string.Empty, string.Empty, true);
        }
    }
}





