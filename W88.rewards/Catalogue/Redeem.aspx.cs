﻿using System;
using System.Text;
using System.Web;
using System.Web.UI;
using W88.BusinessLogic.Rewards.Helpers;
using W88.BusinessLogic.Rewards.Redemption.Factories;
using W88.BusinessLogic.Shared.Models;
using W88.Utilities;
using W88.Utilities.Log.Helpers;
using W88.WebRef.RewardsServices;
using W88.BusinessLogic.Rewards.Models;
using RedemptionRequest = W88.BusinessLogic.Rewards.Redemption.Model.RedemptionRequest;

public partial class Catalogue_Redeem : CatalogueBasePage
{
    protected string Status = string.Empty;
    protected string Message = string.Empty;
    protected string ProductType = string.Empty;
    protected string VipOnly = string.Empty;
    protected ProductDetails ProductDetails = null;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack)
        {
            return;
        }
        SetLabels();
        GetProductDetails();  
    }

    protected async void RedeemButtonOnClick(object sender, EventArgs e)
    {
        ProductDetails = Common.DeserializeObject<ProductDetails>(ProductDetailsField.Value);
        Status = string.Empty;
        Message = string.Empty;
        var productType = (ProductTypeEnum)int.Parse(ProductDetails.ProductType);

        if (!HasSession)
        {
            Response.Redirect("/Catalogue?categoryId=0&sortBy=2", false);
            return;
        }

        #region redeem product
        try
        {
            var response = await RedemptionStrategy.Initialize(GetRequest(productType)).Redeem();
            Status = Convert.ToString(response.Code);
            Message = response.Message;
            ShowMessage(Status, Message);
            RefreshPoints();
        }
        catch (Exception exception)
        {
            Status = Convert.ToString((int) Constants.StatusCode.Error);
            Message = RewardsHelper.GetTranslation(TranslationKeys.Errors.Exception);
            ShowMessage(Status, Message);
            AuditTrail.AppendLog(exception);
        }
        #endregion
    }

    protected override void SetLabels()
    {
        #region labels
        if (!HasSession)
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
        var pointsLabelText = RewardsHelper.GetTranslation(TranslationKeys.Label.Points);
        var stringBuilder = new StringBuilder();

        stringBuilder.Append(pointsLabelText)
            .Append(": ")
            .Append(MemberRewardsInfo != null ? Convert.ToString(MemberRewardsInfo.CurrentPoints) : "0");
        pointsLabel.InnerText = stringBuilder.ToString();

        var pointLevelLabelText = RewardsHelper.GetTranslation(TranslationKeys.Label.PointLevel);
        stringBuilder = new StringBuilder();
        stringBuilder.Append(pointLevelLabelText)
            .Append(" ")
            .Append(MemberRewardsInfo != null ? Convert.ToString(MemberRewardsInfo.CurrentPointLevel) : "0");
        pointLevelLabel.InnerText = stringBuilder.ToString();
        divLevel.Visible = true;
        #endregion
    }

    private void InitFields()
    {
        #region fields
        const string colon = ":";
        lbcat.Text = RewardsHelper.GetTranslation(TranslationKeys.Redemption.Category) + colon;
        lbproduct.Text = RewardsHelper.GetTranslation(TranslationKeys.Redemption.Product) + colon;
        lbcurr.Text = RewardsHelper.GetTranslation(TranslationKeys.Redemption.Currency) + colon;
        lbpoint.Text = RewardsHelper.GetTranslation(TranslationKeys.Label.Points) + colon;
        lbperiod.Text = RewardsHelper.GetTranslation(TranslationKeys.Redemption.Delivery);
        lbqty.Text = RewardsHelper.GetTranslation(TranslationKeys.Redemption.Quantity) + colon;
        lbaccount.Text = RewardsHelper.GetTranslation(TranslationKeys.Redemption.Account) + colon;
        tbRName.Attributes.Add("PLACEHOLDER", RewardsHelper.GetTranslation(TranslationKeys.Redemption.EnterName));
        tbAddress.Attributes.Add("PLACEHOLDER", RewardsHelper.GetTranslation(TranslationKeys.Redemption.EnterAddress));
        tbPostal.Attributes.Add("PLACEHOLDER", RewardsHelper.GetTranslation(TranslationKeys.Redemption.EnterPostal));
        tbCity.Attributes.Add("PLACEHOLDER", RewardsHelper.GetTranslation(TranslationKeys.Redemption.EnterCity));
        tbCountry.Attributes.Add("PLACEHOLDER", RewardsHelper.GetTranslation(TranslationKeys.Redemption.EnterCountry));
        tbContact.Attributes.Add("PLACEHOLDER", RewardsHelper.GetTranslation(TranslationKeys.Redemption.EnterContactNumber));
        txtBoxRemarks.Attributes.Add("PLACEHOLDER", RewardsHelper.GetTranslation(TranslationKeys.Redemption.EnterRemarks));
        nameLabel.Text = RewardsHelper.GetTranslation(TranslationKeys.Label.Name);
        addressLabel.Text = RewardsHelper.GetTranslation(TranslationKeys.Label.Address);
        postalLabel.Text = RewardsHelper.GetTranslation(TranslationKeys.Label.Postal);
        cityLabel.Text = RewardsHelper.GetTranslation(TranslationKeys.Label.City);
        countryLabel.Text = RewardsHelper.GetTranslation(TranslationKeys.Label.Country);
        contactLabel.Text = RewardsHelper.GetTranslation(TranslationKeys.Label.ContactNumber);
        redeemButton.Text = RewardsHelper.GetTranslation(TranslationKeys.Redemption.RedeemNow);
        remarksLabel.Text = RewardsHelper.GetTranslation(TranslationKeys.Label.Remarks);
        if ((ProductTypeEnum)int.Parse(ProductType) == ProductTypeEnum.Wishlist)
        {
            remarksLabel.Visible = true;
            txtBoxRemarks.Visible = true;
        }
        redeemButton.Visible = true;
        #endregion
    }

    private async void SetProductInfo()
    {
        var memberCode = UserSessionInfo == null ? "" : UserSessionInfo.MemberCode;
        var countryCode = MemberSession == null ? "0" : MemberSession.CountryCode;
        var currencyCode = MemberSession == null ? "0" : MemberSession.CurrencyCode;

        try
        {
            ProductType = ProductDetails.ProductType;
            if (ProductType == "1" && (ProductDetails.CurrencyCode != currencyCode))
            {
                Response.Redirect("/Catalogue?categoryId=53&sortBy=2", false);
                return;
            }

            if (ProductDetails.CountryCode.Trim() != "All")
            {
                if (!ProductDetails.CountryCode.Contains(countryCode))
                {
                    Response.Redirect(string.Format("/Catalogue?categoryId={0}&sortBy=2", ProductDetails.CategoryId), false);
                    return;
                }
            }

            if (!(ProductDetails.RedemptionValidity == "1" && ProductDetails.RedemptionValidityCategory == "1"))
            {
                Status = Convert.ToString((int) Constants.StatusCode.MemberVip);
                Message = RewardsHelper.GetTranslation(TranslationKeys.Redemption.VipOnly);
                ShowMessage(Status, Message);
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
            var vipCategoryId = Common.GetAppSetting<string>("vipCategoryId");
            if (ProductDetails.CategoryId == vipCategoryId)
            {
                tbQuantity.Enabled = false;
            }

            imgPic.ImageUrl = ProductDetails.ImageUrl;

            if (!string.IsNullOrEmpty(ProductDetails.DiscountPoints) && int.Parse(ProductDetails.DiscountPoints) != 0)
            {
                var builder = new StringBuilder();
                // Before discount
                builder.Append(string.Format(@"{0:#,###,##0.##} ", ProductDetails.PointsRequired))
                    .Append(RewardsHelper.GetTranslation(TranslationKeys.Label.Points));
                lblBeforeDiscount.Text = builder.ToString();

                builder = new StringBuilder();
                builder.Append(string.Format(@"{0:#,###,##0.##} ", ProductDetails.DiscountPoints))
                    .Append(RewardsHelper.GetTranslation(TranslationKeys.Label.Points));
                lblPointCenter.Text = builder.ToString();
            }
            else
            {
                lblBeforeDiscount.Text = string.Empty;
                var builder = new StringBuilder();
                // Before discount
                builder.Append(string.Format(@"{0:#,###,##0.##} ", ProductDetails.PointsRequired))
                    .Append(RewardsHelper.GetTranslation(TranslationKeys.Label.Points));
                lblPointCenter.Text = builder.ToString();
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
            var redemptionDetails = await RewardsHelper.GetMemberRedemptionDetails(memberCode);
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
            AuditTrail.AppendLog(exception);
        }
    }

    private RedemptionRequest GetRequest(ProductTypeEnum type)
    {
        var request = new RedemptionRequest();
        request.ProductType = type;
        request.MemberCode = UserSessionInfo == null ? string.Empty : UserSessionInfo.MemberCode;
        request.ProductId = lblproductid.Value;
        request.CategoryId = string.IsNullOrEmpty(ProductDetails.CategoryId) ? "0" : ProductDetails.CategoryId;
        request.RiskId = MemberSession == null ? "0" : MemberSession.RiskId;
        request.Currency = MemberSession == null ? "0" : MemberSession.CurrencyCode;
        request.PointRequired = string.IsNullOrEmpty(ProductDetails.PointsRequired) ? string.Empty : ProductDetails.PointsRequired;
        request.Quantity = tbQuantity.Text.Trim();

        switch (type)
        {
            case ProductTypeEnum.Freebet:
                request.CreditAmount = string.IsNullOrEmpty(ProductDetails.AmountLimit) ? string.Empty : ProductDetails.AmountLimit;
                break;
            case ProductTypeEnum.Online:
                request.AimId = tbAccount.Text.Trim();
                break;
            default:
                if (type == ProductTypeEnum.Normal || type == ProductTypeEnum.Wishlist)
                {
                    request.Name = tbRName.Text.Trim();
                    request.ContactNumber = tbContact.Text.Trim();
                    request.Address = tbAddress.Value.Trim();
                    request.PostalCode = tbPostal.Text.Trim();
                    request.City = tbCity.Text.Trim();
                    request.Country = tbCountry.Text.Trim();
                    if (type == ProductTypeEnum.Wishlist)
                    {
                        request.Remarks = txtBoxRemarks.Value.Trim();
                    }
                }
                break;
        }

        return request;
    }

    private async void GetProductDetails()
    {
        var productId = HttpContext.Current.Request.QueryString.Get("productId");
        if (string.IsNullOrEmpty(productId))
        {
            Response.Redirect("/Catalogue?categoryId=0&sortBy=2", false);
            return;
        }
        ProductDetails = await RewardsHelper.GetProductDetails(MemberSession, productId, HasSession);
        SetProductInfo();
        ProductDetails.ProductDescription = HttpUtility.HtmlEncode(ProductDetails.ProductDescription);
        ProductDetailsField.Value = Common.SerializeObject(ProductDetails);
        InitFields();     
    }

    private async void RefreshPoints()
    {
        if (MemberRewardsInfo == null)
        {
            MemberRewardsInfo = new MemberRewardsInfo();
        }
        MemberRewardsInfo.CurrentPoints = await MembersHelper.GetRewardsPoints(UserSessionInfo);
        MemberRewardsInfo.CurrentPointLevel = await RewardsHelper.GetPointLevel(MemberSession.MemberId);
        SetLabels();
        SetProductInfo();
    }

    private void ShowMessage(string status, string message)
    {
        var scriptBuilder = new StringBuilder();
        scriptBuilder.Append("setTimeout(function() {showMessage('")
            .Append(status + "','")
            .Append(message + "');}, 300);");
        ScriptManager.RegisterStartupScript(Page, GetType(), (new Guid()).ToString(), scriptBuilder.ToString(), true);
    }
}





