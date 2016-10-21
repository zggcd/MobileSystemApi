﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using System.Web;
using W88.BusinessLogic.Base.Helpers;
using W88.BusinessLogic.Shared.Helpers;
using W88.BusinessLogic.Accounts.Models;
using W88.BusinessLogic.Shared.Models;
using W88.Utilities;
using W88.Utilities.Constant;
using W88.Utilities.Log.Helpers;
using W88.WebRef.RewardsServices;
using W88.BusinessLogic.Rewards.Models;

namespace W88.BusinessLogic.Rewards.Helpers
{

    /// <summary>
    /// Summary description for RewardsHelper
    /// </summary>
    public class RewardsHelper : BaseHelper
    {
        public async Task<int> CheckRedemptionLimitForVipCategory(string memberCode, string vipCategoryId)
        {
            try
            {
                using (var client = new RewardsServicesClient())
                {
                    return await client.CheckRedemptionLimitForVIPCategoryAsync(OperatorId.ToString(CultureInfo.InvariantCulture), 
                        memberCode,
                        vipCategoryId);
                } 
            }
            catch (Exception exception)
            {
                return 0;
            }
        }

        public async Task<DataSet> GetCatalogueSet(MemberSession memberSession)
        {
            try
            {
                using (var client = new RewardsServicesClient())
                {
                    var countryCode = memberSession == null ? "0" : memberSession.CountryCode;
                    var currencyCode = memberSession == null ? "0" : memberSession.CurrencyCode;
                    var riskId = memberSession == null ? "0" : memberSession.RiskId;

                    var dataSet = await client.getCatalogueSearchAsync(
                        OperatorId.ToString(CultureInfo.InvariantCulture)
                        , LanguageHelpers.SelectedLanguage
                        , countryCode
                        , currencyCode
                        , riskId);

                    if (dataSet.Tables.Count == 0)
                    {
                        return null;
                    }
                    if (!dataSet.Tables[0].Columns.Contains("redemptionValidity"))
                    {
                        dataSet.Tables[0].Columns.Add("redemptionValidity");
                    }

                    foreach (DataRow dataRow in dataSet.Tables[0].Rows)
                    {
                        var imgNameOn = dataRow["imageNameOn"].ToString().Split('.')[0];
                        var imgPathOn = imgNameOn + ".png";
                        var imgPathOff = imgNameOn + ".png";

                        dataRow["imagePathOn"] = Convert.ToString(Common.GetAppSetting<string>("ImagesDirectoryPath") + "Category/" + imgPathOn);
                        dataRow["imagePathOff"] = Convert.ToString(Common.GetAppSetting<string>("ImagesDirectoryPath") + "Category/" + imgPathOff);

                        if (!riskId.Equals("0"))
                        {
                            dataRow["redemptionValidity"] += ",";
                            var validity = (string) dataRow["redemptionValidity"];
                            if (!validity.ToUpper().Equals("ALL,"))
                            {
                                dataRow["redemptionValidity"] = !validity.Contains(riskId.ToUpper() + ",") ? "0" : "1";
                            }
                            else
                            {
                                dataRow["redemptionValidity"] = "1";
                            }
                        }
                        else
                        {
                            dataRow["redemptionValidity"] += "0";
                        }
                    }
                    return dataSet;
                }               
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public async Task<string> GetCategoryName(string categoryCode)
        {
            try
            {
                using (var client = new RewardsServicesClient())
                {
                    return await client.getCategoryNameAsync(categoryCode, LanguageHelpers.SelectedLanguage);
                }              
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public async Task<int> GetMemberPointLevelDiscount(MemberSession memberSession)
        {
            try
            {
                using (var client = new RewardsServicesClient())
                {
                    return await client.getMemberPointLevelDiscountAsync(
                        OperatorId.ToString(CultureInfo.InvariantCulture),
                        memberSession.CurrencyCode,
                        (await GetPointLevel(memberSession.MemberId)).ToString(CultureInfo.InvariantCulture));
                }
            }
            catch (Exception exception)
            {
                return 0;
            }
        }

        public async Task<MemberRedemptionDetails> GetMemberRedemptionDetails(string memberCode)
        {
            try
            {
                using (var client = new RewardsServicesClient())
                {
                    var dataSet = await client.getMemberRedemptionDetailAsync(OperatorId.ToString(CultureInfo.InvariantCulture), memberCode);
                    if (dataSet.Tables.Count == 0 || dataSet.Tables[0].Rows.Count == 0)
                    {
                        return null;
                    }
                    var dataRow = dataSet.Tables[0].Rows[0];
                    var redemptionDetails = new MemberRedemptionDetails();
                    redemptionDetails.FullName = dataRow["firstName"] + " " + dataRow["lastName"];
                    redemptionDetails.Address = dataRow["address"].ToString();
                    redemptionDetails.Postal = dataRow["postal"].ToString();
                    redemptionDetails.City = dataRow["city"].ToString();
                    redemptionDetails.CountryCode = dataRow["countryCode"].ToString();
                    redemptionDetails.Mobile = dataRow["mobile"].ToString();
                    if (dataSet.Tables.Count > 1)
                    {
                        redemptionDetails.PointsBefore = dataSet.Tables[1].Rows[0]["pointsBefore"].ToString();
                    }
                    return redemptionDetails;
                }
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public async Task<int> GetPointLevel(string memberId)
        {
            try
            {
                if (string.IsNullOrEmpty(memberId))
                {
                    return 0;
                }
                using (var client = new RewardsServicesClient())
                {
                    var pointLevel = await client.getMemberPointLevelFEAsync(memberId);
                    return int.Parse(pointLevel); 
                }              
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<ProductDetails> GetProductDetails(MemberSession memberSession, string productId, bool hasSession)
        {
            try
            {
                using (var client = new RewardsServicesClient())
                {
                    var countryCode = memberSession == null ? "" : memberSession.CountryCode;
                    var currencyCode = memberSession == null ? "" : memberSession.CurrencyCode;
                    var riskId = memberSession == null ? "" : memberSession.RiskId.ToUpper();

                    var dataSet = await client.getProductDetailAsync(
                        productId,
                        LanguageHelpers.SelectedLanguage,
                        riskId,
                        countryCode,
                        currencyCode,
                        riskId);
                    
                    if (dataSet.Tables.Count == 0 || dataSet.Tables[0].Rows.Count == 0)
                    {
                        return null;
                    }

                    var dataRow = dataSet.Tables[0].Rows[0];
                    if (dataRow == null)
                    {
                        return null;
                    }

                    // Set product details
                    var productDetails = new ProductDetails();
                    productDetails.PointsRequired = dataRow["pointsRequired"].ToString().Replace(" ", string.Empty);

                    if (dataRow["discountPoints"] == DBNull.Value)
                    {
                        if (hasSession && dataRow["productType"].ToString() != "1")
                        {
                            //grab member point level
                            var pointLevelDiscount = await GetMemberPointLevelDiscount(memberSession);
                            var percentage = Convert.ToDouble(pointLevelDiscount) / 100;
                            var normalPoint = int.Parse(productDetails.PointsRequired);
                            var points = Math.Floor(normalPoint * (1 - percentage));
                            var pointsAfterLevelDiscount = Convert.ToInt32(points).ToString(CultureInfo.InvariantCulture);

                            productDetails.PointsRequired = pointsAfterLevelDiscount;
                            productDetails.PointLevelDiscount = pointLevelDiscount.ToString();
                        }    
                    }

                    if (!string.IsNullOrEmpty(riskId))
                    {
                        //valid category
                        productDetails.RedemptionValidityCategory = dataRow["redemptionValidityCat"] + ",";

                        if (productDetails.RedemptionValidityCategory.ToUpper() != "ALL,")
                        {
                            productDetails.RedemptionValidityCategory = !productDetails.RedemptionValidityCategory.Contains(riskId + ",") ? "0" : "1";
                        }
                        else
                        {
                            productDetails.RedemptionValidityCategory = "1";
                        }

                        productDetails.RedemptionValidity = dataRow["redemptionValidity"] + ",";
                        if (productDetails.RedemptionValidity.ToUpper() != "ALL,")
                        {
                            productDetails.RedemptionValidity = !productDetails.RedemptionValidity.Contains(riskId + ",") ? "0" : "1";
                        }
                        else
                        {
                            productDetails.RedemptionValidity = "1";
                        }
                    }
                    else
                    {
                        productDetails.RedemptionValidity = dataRow["redemptionValidity"] + "0";
                        productDetails.RedemptionValidityCategory = dataRow["redemptionValidityCat"] + "0";
                    }

                    productDetails.ProductId = productId;
                    productDetails.ProductType = dataRow["productType"].ToString();
                    productDetails.AmountLimit = dataRow["amountLimit"].ToString();
                    productDetails.CategoryId = dataRow["categoryId"].ToString();
                    productDetails.CurrencyCode = dataRow["currencyValidity"].ToString();
                    productDetails.CountryCode = dataRow["countryValidity"].ToString();
                    productDetails.ImageUrl = Common.GetAppSetting<string>("ImagesDirectoryPath") + "Product/" + dataRow["imageName"];
                    productDetails.ProductCategoryName = dataRow["categoryName"].ToString();
                    productDetails.ProductName = dataRow["productName"].ToString();
                    productDetails.ProductDescription = dataRow["productDescription"].ToString();
                    productDetails.DeliveryPeriod = dataRow["deliveryPeriod"].ToString();
                    productDetails.DiscountPoints = dataRow["discountPoints"] == DBNull.Value ? string.Empty : dataRow["discountPoints"].ToString();
            
                    return productDetails;
                }                
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public async Task<DataSet> GetProductSearch(MemberSession memberSession, 
            string categoryId, 
            int pointsFrom, 
            int pointsTo, 
            string searchText,
            string sortBy,
            string pageSize,
            string numberOfPages)
        {
            try
            {
                using (var client = new RewardsServicesClient())
                {
                    var countryCode = memberSession == null ? "0" : memberSession.CountryCode;
                    var currencyCode = memberSession == null ? "0" : memberSession.CurrencyCode;
                    var riskId = memberSession == null ? "0" : memberSession.RiskId;
                    var token = memberSession == null ? string.Empty : memberSession.Token;
                    var hasSession = !string.IsNullOrEmpty(token);

                    var dataSet = await client.getProductSearchAsync(
                        OperatorId.ToString(CultureInfo.InvariantCulture),
                        categoryId,
                        LanguageHelpers.SelectedLanguage,
                        pointsFrom,
                        pointsTo,
                        searchText,
                        countryCode,
                        currencyCode,
                        riskId,
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        sortBy,
                        pageSize,
                        numberOfPages);

                    if (dataSet.Tables.Count == 0 || dataSet.Tables[0].Rows.Count == 0)
                    {
                        return null;
                    }

                    if (!dataSet.Tables[0].Columns.Contains("redemptionValidity"))
                    {
                        dataSet.Tables[0].Columns.Add("redemptionValidity");
                    }

                    foreach (DataRow dataRow in dataSet.Tables[0].Rows)
                    {
                        if (hasSession)
                        {
                            var pointLevelDiscount = await GetMemberPointLevelDiscount(memberSession);
                            
                            if (dataRow["discountPoints"] == DBNull.Value && pointLevelDiscount != 0 &&
                                dataRow["productType"].ToString() != "1")
                            {
                                var percentage = Convert.ToDouble(pointLevelDiscount) / 100;
                                var normalPoint = int.Parse(dataRow["pointsRequired"].ToString());
                                var points = Math.Floor(normalPoint * (1 - percentage));
                                var pointAfterLevelDiscount = Convert.ToInt32(points);
                                dataRow["pointsRequired"] = pointAfterLevelDiscount;
                            }

                            dataRow["redemptionValidity"] += ",";
                            if (dataRow["redemptionValidity"].ToString().ToUpper() != "ALL,")
                            {
                                dataRow["redemptionValidity"] = !((string)dataRow["redemptionValidity"]).Contains(riskId.ToUpper() + ",") ? "0" : "1";
                            }
                            else
                            {
                                dataRow["redemptionValidity"] = "1";
                            }
                        }
                        else
                        {
                            dataRow["redemptionValidity"] += "0";
                        }

                        dataRow["imagePath"] = Common.GetAppSetting<string>("ImagesDirectoryPath") + "Product/" + dataRow["imageName"];
                    }

                    if (sortBy == "2")
                    {
                        dataSet.Tables[0].DefaultView.Sort = "pointsRequired";
                    }

                    return dataSet;
                }
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public async Task<DataSet> GetAccountSummary(string memberCode)
        {
            try
            {
                using (var client = new RewardsServicesClient())
                {
                    var accountSet = await client.getMemberAccountAsync(OperatorId.ToString(CultureInfo.InvariantCulture), memberCode);
                    
                    var table0 = accountSet.Tables[0];
                    var table1 = accountSet.Tables[1];
                    var table2 = accountSet.Tables[2];
                    var table3 = accountSet.Tables[3];
                    var table4 = accountSet.Tables[4];
                    var table5 = accountSet.Tables[5];
                    var table6 = accountSet.Tables[6];

                    var totalStake = table0.Rows.Count == 0 ? 0 : (decimal)table0.Rows[0]["totalStake"];
                    var pointsAwarded = table1.Rows.Count == 0 ? 0 : (int)table1.Rows[0]["pointsAwarded"];
                    var pointsRequired = table2.Rows.Count == 0 ? 0 : (int)table2.Rows[0]["pointsRequired"];
                    var pointsAdjusted = table3.Rows.Count == 0 ? 0 : (int)table3.Rows[0]["pointsAdjusted"];
                    var pointsExpired = table4.Rows.Count == 0 ? 0 : (int)table4.Rows[0]["pointsExpired"];
                    var pointsBalance = table5.Rows.Count == 0 ? 0 : (int)table5.Rows[0]["pointsBalance"];
                    var pointsCart = table6.Rows.Count == 0 ? 0 : (int)table6.Rows[0]["pointsCart"];
                    var finalBalance = pointsBalance - pointsCart;

                    var dataTable = new DataTable("History");
                    dataTable.Columns.Add(new DataColumn("stake", typeof(decimal)));
                    dataTable.Columns.Add(new DataColumn("earning", typeof(int)));
                    dataTable.Columns.Add(new DataColumn("redemption", typeof(int)));
                    dataTable.Columns.Add(new DataColumn("expired", typeof(int)));
                    dataTable.Columns.Add(new DataColumn("adjusted", typeof(int)));
                    dataTable.Columns.Add(new DataColumn("balance", typeof(int)));
                    dataTable.Columns.Add(new DataColumn("cart", typeof(int)));

                    var pointsDataRow = dataTable.NewRow();
                    pointsDataRow["stake"] = Math.Round(Convert.ToDecimal(totalStake), 2);
                    pointsDataRow["earning"] = pointsAwarded;
                    pointsDataRow["redemption"] = pointsRequired;
                    pointsDataRow["expired"] = pointsExpired;
                    pointsDataRow["adjusted"] = pointsAdjusted;
                    pointsDataRow["balance"] = finalBalance;
                    pointsDataRow["cart"] = pointsCart;

                    dataTable.Rows.Add(pointsDataRow);
                    var dataSet = new DataSet();
                    dataSet.Tables.Add(dataTable);
                    
                    return dataSet;
                }
            }
            catch (Exception exception)
            {
                return null;
            }    
        }

        public async Task<ProcessCode> SendMail(string memberCode, string redemptionId)
        {
            var process = new ProcessCode();
            process.Id = Guid.NewGuid();

            try
            {
                var recipient = string.Empty;
                var language = string.Empty;
                var isAlternative = false;

                using (var client = new RewardsServicesClient())
                {
                    var dataSet = await client.getMemberInfoAsync(Settings.OperatorId.ToString(CultureInfo.InvariantCulture), memberCode);
                    if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
                    {
                        recipient = dataSet.Tables[0].Rows[0]["email"].ToString();                        
                        language = dataSet.Tables[0].Rows[0]["languageCode"] == null 
                            ? LanguageHelpers.SelectedLanguage : dataSet.Tables[0].Rows[0]["languageCode"].ToString();                        
                    }
                }

                if (string.IsNullOrEmpty(recipient))
                {
                    process.ProcessSerialId += 1;
                    process.Code = (int)Constants.StatusCode.Error;
                    AuditTrail.AppendLog(memberCode, Constants.PageNames.MailApi, Constants.TaskNames.SendMail,
                        Constants.PageNames.ComponentName, Convert.ToString((int)Constants.StatusCode.Error), string.Empty, string.Empty,
                        "Recipient address is empty", string.Empty, Convert.ToString(process.ProcessSerialId), Convert.ToString(process.Id), false);      
                    return process;
                }
                
                var mailRequest = new MailRequest();
                mailRequest.To = recipient;

                string[] splitChar = {"|"};
                var mailDomains = Common.GetAppSetting<string>("smtp_alternative").Split(splitChar, StringSplitOptions.None);

                foreach (var domain in mailDomains)
                {
                    if (mailRequest.To.Contains(domain))
                    {
                        isAlternative = true;
                        break;
                    }
                }

                if (isAlternative)
                {
                    mailRequest.Port = int.Parse(Common.GetAppSetting<string>("mail_port"));
                    mailRequest.Host = Common.GetAppSetting<string>("mail_host");
                    mailRequest.Username = Common.GetAppSetting<string>("mail_username");
                    mailRequest.Password = Common.GetAppSetting<string>("mail_password");
                }
                else
                {
                    mailRequest.UseDefaultCredentials = true;
                    var bccAddress = Common.GetAppSetting<string>("bcc_addresses");
                    if (!string.IsNullOrEmpty(bccAddress))
                    {
                        mailRequest.BccAddresses = bccAddress.Split(splitChar, StringSplitOptions.None);
                    }
                }

                var subject = GetTranslation(TranslationKeys.Redemption.MailSubject, language);
                var body = GetTranslation(TranslationKeys.Redemption.MailBody, language);
                body = string.IsNullOrEmpty(body) ? string.Empty : HttpUtility.HtmlDecode(string.Format(body.Trim(), memberCode.Trim(), redemptionId));
                
                // Send mail
                mailRequest.Subject = subject;
                mailRequest.Body = body;
                mailRequest.From = Common.GetAppSetting<string>("sender_address");
                mailRequest.SenderName = Common.GetAppSetting<string>("sender_name");
                MailHelper.SendMail(mailRequest);

                process.ProcessSerialId += 1;
                process.Code = (int)Constants.StatusCode.Success;
                return process;
            }
            catch (Exception exception)
            {
                AuditTrail.AppendLog(exception);
                process.Code = (int)Constants.StatusCode.Error;
                return process;
            }
        }

        public static string GetTranslation(string key, string language = "", string path = "rewards/rewards")
        {
            LOV keyValue;
            if (string.IsNullOrWhiteSpace(language))
            {
                keyValue = GetTranslations(language, path).Find(x => x.Text == key);
                return keyValue == null ? string.Empty : keyValue.Value;
            }
            keyValue = GetTranslations(language, path).Find(x => x.Text == key);
            return keyValue == null ? string.Empty : keyValue.Value;
        }

        private static List<LOV> GetTranslations(string language, string path)
        {             
            var list = new List<LOV>();
            try
            {
                var translations = Common.DeserializeObject<dynamic>(CultureHelpers.AppData.GetLocale_i18n_Resource(path, true, language));
                if (translations == null)
                {
                    return list;
                }
                foreach (var translation in translations)
                {
                    list.Add(new LOV
                    {
                        Text = translation.Name,
                        Value = translation.Value
                    });
                }
                return list;     
            }
            catch (Exception exception)
            {
                return list;      
            }   
        }
    }
}