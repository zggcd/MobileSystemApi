﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using svcPayDeposit;
using System.Web.Services;
using System.Web.Script.Services;
using System.Collections.Specialized;

public partial class Deposit_AllDebit : PaymentBasePage
{
    protected string strStatusCode = string.Empty;
    protected string strAlertCode = string.Empty;
    protected string strAlertMessage = string.Empty;
    protected string strCCVHelp = string.Empty;

    protected void Page_Init(object sender, EventArgs e)
    {
        base.PageName = Convert.ToString(commonVariables.DepositMethod.AllDebit);
        base.PaymentType = commonVariables.PaymentTransactionType.Deposit;
        base.PaymentMethodId = Convert.ToString((int)commonVariables.DepositMethod.AllDebit);

    }

    protected void Page_Load(object sender, EventArgs e)
    {
        ddlCardType.Items.Insert(0, new ListItem(commonCulture.ElementValues.getResourceString("ddlCardType", xeResources), "-1"));

        if (!Page.IsPostBack)
        {
            CheckAgentAndRedirect(string.Concat(V2DepositPath, "Pay", PaymentMethodId, ".aspx"));
            this.InitializeExpiryMonthYear();
            this.InitializeLabels();

            this.InitializeMemberAccount();
        }
    }

    private void InitializeLabels()
    {
        lblMode.Text = base.strlblMode;
        txtMode.Text = base.strtxtMode;

        lblMinMaxLimit.Text = base.strlblMinMaxLimit;
        txtMinMaxLimit.Text = base.strtxtMinMaxLimit;

        lblDailyLimit.Text = base.strlblDailyLimit;
        txtDailyLimit.Text = base.strtxtDailyLimit;

        lblTotalAllowed.Text = base.strlblTotalAllowed;
        txtTotalAllowed.Text = base.strtxtTotalAllowed;

        lblDepositAmount.Text = base.strlblAmount;

        btnSubmit.Text = base.strbtnSubmit;

        lblCardType.Text = commonCulture.ElementValues.getResourceString("lblCardType", xeResources);

        lblCardName.Text = commonCulture.ElementValues.getResourceString("lblCardName", xeResources);

        lblCardNo.Text = commonCulture.ElementValues.getResourceString("lblCardNo", xeResources);

        lblExpiry.Text = commonCulture.ElementValues.getResourceString("lblExpiry", xeResources);

        lblSecurityCode.Text = commonCulture.ElementValues.getResourceString("lblSecurityCode", xeResources);

        strCCVHelp = commonCulture.ElementValues.getResourceString("securityCodeHelp", xeResources);
    }
    private void InitializeExpiryMonthYear()
    {
        for (int i = 1; i <= 12; i++)
        {
            ddlExpiryMonth.Items.Add(new ListItem(i.ToString("00"), i.ToString("00")));
        }
        ddlExpiryMonth.Items.Insert(0, new ListItem(commonCulture.ElementValues.getResourceString("ddlMonthDefaultText", xeResources), "-1"));

        int year = DateTime.Now.Year;
        for (int i = year; i <= year + 10; i++)
        {
            ddlExpiryYear.Items.Add(new ListItem(i.ToString(), i.ToString()));
        }
        ddlExpiryYear.Items.Insert(0, new ListItem(commonCulture.ElementValues.getResourceString("ddlYearDefaultText", xeResources), "-1"));
    }

    private void InitializeMemberAccount()
    {
        DataTable dtMemberAccount;

        try
        {
            using (svcPayMember.MemberClient client = new svcPayMember.MemberClient())
            {
                dtMemberAccount = client.getMemberLastCreditCardDepositDetail(Convert.ToInt64(strOperatorId), strMemberCode);

                // it is ok if dtMemberAccount is null and should not be treated as error
                if (dtMemberAccount != null)
                {
                    if (dtMemberAccount.Rows.Count > 0)
                    {
                        DataRow dr = dtMemberAccount.Rows[0];
                        ddlCardType.SelectedValue = dr["cardType"].ToString();
                        ddlExpiryMonth.SelectedValue = dr["expMonth"].ToString();
                        ddlExpiryYear.SelectedValue = dr["expYear"].ToString();
                        txtCardName.Text = dr["cardName"] == DBNull.Value ? string.Empty : dr["cardName"].ToString();
                        txtCardNo.Text = dr["cardNumber"] == DBNull.Value ? string.Empty : dr["cardNumber"].ToString();
                    }
                }
            }

        }
        catch (Exception ex)
        {
            commonAuditTrail.appendLog("system", base.PageName, "InitializeMemberAccount", string.Empty, string.Empty, string.Empty, "-99", "exception", ex.Message, string.Empty, string.Empty, true);
        }
    }

    protected void btnSubmit_Click(object sender, EventArgs e)
    {
        string strDepositAmount = txtAmount.Text.Trim();
        string strCardName = txtCardName.Text;
        string strCardNo = txtCardNo.Text.Replace("-", "");
        string strCCV = txtSecurityCode.Text;
        string strIssuingBank = "OTHER";

        string selectedCardTypeText = ddlCardType.SelectedItem.Text;
        string selectedCardTypeValue = ddlCardType.SelectedItem.Value;
        string selectedMonth = ddlExpiryMonth.SelectedItem.Value;
        string selectedYear = ddlExpiryYear.SelectedItem.Value;


        decimal decDepositAmount = commonValidation.isDecimal(strDepositAmount) ? Convert.ToDecimal(strDepositAmount) : 0;
        decimal decMinLimit = commonValidation.isDecimal(strMinLimit) ? Convert.ToDecimal(strMinLimit) : 0;
        decimal decMaxLimit = commonValidation.isDecimal(strMaxLimit) ? Convert.ToDecimal(strMaxLimit) : 0;

        CommonStatus status = new CommonStatus();

        try
        {
            if (decDepositAmount == 0)
            {
                status = base.GetErrors("/MissingDepositAmount");
            }
            else if (decDepositAmount <= 0)
            {
                status = base.GetErrors("/InvalidDepositAmount");
            }
            else if (string.IsNullOrEmpty(strCardName))
            {
                status = base.GetErrors("/MissingCardName");
            }
            else if (string.IsNullOrEmpty(strCardNo))
            {
                status = base.GetErrors("/MissingCardNo");
            }
            else if (string.IsNullOrEmpty(strCCV))
            {
                status = base.GetErrors("/MissingCCV");
            }
            else if (selectedCardTypeValue == "-1")
            {
                status = base.GetErrors("/MissingCardType");
            }
            else if (selectedMonth == "-1" || selectedYear == "-1")
            {
                status = base.GetErrors("/MissingCardExpiry");
            }
            else if (decDepositAmount < decMinLimit)
            {
                status = base.GetErrors("/AmountMinLimit");
            }
            else if (decDepositAmount > decMaxLimit)
            {
                status = base.GetErrors("/AmountMaxLimit");
            }
            else if ((strTotalAllowed != strUnlimited) && (decDepositAmount > Convert.ToDecimal(strTotalAllowed)) && Convert.ToDecimal(strTotalAllowed) > 0)
            {
                status = base.GetErrors("/TotalAllowedExceeded");
            }

            if (!status.IsProcessAbort)
            {
                using (svcPayDeposit.DepositClient client = new svcPayDeposit.DepositClient())
                {
                    xeResponse = client.createCreditCardTransaction(Convert.ToInt64(strOperatorId), long.Parse(strMemberID), strMemberCode, Convert.ToInt64(commonVariables.DepositMethod.AllDebit), strMerchantId, strCurrencyCode, decDepositAmount, DepositSource.Mobile,
                                    strCardName, strCardNo, selectedCardTypeText, selectedMonth, selectedYear, strCCV, strIssuingBank);

                    if (xeResponse == null)
                    {
                        status = base.GetErrors("/TransferFail");
                    }
                    else
                    {
                        bool isTransactionSuccessful = Convert.ToBoolean(commonCulture.ElementValues.getResourceString("result", xeResponse));
                        string strTransferId = commonCulture.ElementValues.getResourceString("invId", xeResponse);


                        if (isTransactionSuccessful)
                        {
                            status.AlertCode = "0";
                            status.AlertMessage = string.Format("{0}\\n{1}: {2}", commonCulture.ElementValues.getResourceXPathString(base.PaymentType.ToString() + "/TransferSuccess", xeErrors), strlblTransactionId, strTransferId);

                            litForm.Text = CallVendor(strDepositAmount, strCardNo, strCCV, strIssuingBank, selectedMonth, selectedYear, strTransferId, selectedCardTypeValue);
                        }
                        else
                        {
                            status = GetErrors("/TransferFail", strTransferId, "/error");
                        }
                    }
                }
            }

        }
        catch (Exception ex)
        {
            status = base.GetErrors("/Exception");

            strErrorDetail = ex.Message;
        }

        strAlertCode = status.AlertCode;
        strAlertMessage = status.AlertMessage;

        string strProcessRemark = string.Format("OperatorId: {0} | MemberCode: {1} | CurrencyCode: {2} | DepositAmount: {3} | BankName: {4} |  MinLimit: {5} | MaxLimit: {6} | TotalAllowed: {7} | DailyLimit: {8} | Response: {9}",
           Convert.ToInt64(strOperatorId), strMemberCode, strCurrencyCode, strDepositAmount, strIssuingBank, decMinLimit, decMaxLimit, strTotalAllowed, strDailyLimit, xeResponse == null ? string.Empty : xeResponse.ToString());

        intProcessSerialId += 1;
        commonAuditTrail.appendLog("system", base.PageName, "InitiateDeposit", "DataBaseManager.DLL", strResultCode, strResultDetail, strErrorCode, strErrorDetail, strProcessRemark, Convert.ToString(intProcessSerialId), strProcessId, isSystemError);
    }

    private string CallVendor(string strDepositAmount, string strCardNo, string strCCV, string strIssuingBank, string selectedMonth, string selectedYear, string strTransferId, string gatewayNo)
    {
        string signKey = GetMerchantKey(gatewayNo);
        string email = "pgwsw88@gmail.com";

        strMerchantId = ConfigurationManager.AppSettings["AllDebit_merchantid"];

        var builder = new StringBuilder();
        builder.AppendFormat("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}", strMerchantId, gatewayNo, strTransferId, strCurrencyCode, strDepositAmount, strMemberID, strMemberID, strCardNo, selectedYear, selectedMonth, strCCV, email, signKey);

        string signInfo = commonEncryption.GetSHA256Hash(builder.ToString()).ToUpper();

        string postUrl = ConfigurationManager.AppSettings["AllDebit_posturl"];
        var requestUrl = HttpContext.Current.Request.Url;

        string callbackUrl = requestUrl.Scheme + "://" + requestUrl.Host + "/_Secure/AjaxHandlers/AllDebitCallback.ashx";

        string phone = "NA";
        string country = "NA";
        string state = "NA";
        string city = "NA";
        string address = "NA";
        string zip = "NA";
        string csid = string.Empty;

        var request = (HttpWebRequest)WebRequest.Create(postUrl);
        string postData = "merNo=" + strMerchantId;
        postData += ("&gatewayNo=" + gatewayNo);
        postData += ("&orderNo=" + strTransferId);
        postData += ("&orderCurrency=" + strCurrencyCode);
        postData += ("&orderAmount=" + strDepositAmount);
        postData += ("&returnUrl=" + callbackUrl);
        postData += ("&cardNo=" + strCardNo);
        postData += ("&cardExpireMonth=" + selectedMonth);
        postData += ("&cardExpireYear=" + selectedYear);
        postData += ("&cardSecurityCode=" + strCCV);
        postData += ("&issuingBank=" + strIssuingBank);
        postData += ("&firstName=" + strMemberID);
        postData += ("&lastName=" + strMemberID);
        postData += ("&email=" + email);
        postData += ("&phone=" + phone);
        postData += ("&country=" + country);
        postData += ("&state=" + state);
        postData += ("&city=" + city);
        postData += ("&address=" + address);
        postData += ("&zip=" + zip);
        postData += ("&signInfo=" + signInfo);
        postData += ("&csid=" + csid);

        var data = Encoding.ASCII.GetBytes(postData);

        request.Method = "POST";
        request.ContentType = "application/x-www-form-urlencoded";
        request.ContentLength = data.Length;

        using (var stream = request.GetRequestStream())
        {
            stream.Write(data, 0, data.Length);
        }

        var response = (HttpWebResponse)request.GetResponse();

        var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

        return responseString.Replace("form ", @"form target=""_blank"" ").Replace("setTimeout('delayer()', 5000)", "setTimeout('delayer()', 1000)");
    }

    private static string GetMerchantKey(string gatewayNo)
    {
        string appSettings = string.Empty;

        if (gatewayNo.Equals("20751003"))
        {
            appSettings = "AllDebit_Visa";
        }
        else if (gatewayNo.Equals("20751004"))
        {
            appSettings = "AllDebit_Master";
        }

        return commonEncryption.decrypting(ConfigurationManager.AppSettings[appSettings], ConfigurationManager.AppSettings.Get("PaymentPrivateKey"));
    }

}