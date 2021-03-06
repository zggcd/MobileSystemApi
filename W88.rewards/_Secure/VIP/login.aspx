﻿<%@ Page Language="C#" AutoEventWireup="true" CodeFile="login.aspx.cs" Inherits="_Secure_VIP_login" Async="true" %>
<%@ Import Namespace="W88.BusinessLogic.Rewards.Helpers" %>
<%@ Import Namespace="W88.BusinessLogic.Rewards.Models" %>
<%@ Import Namespace="W88.BusinessLogic.Shared.Helpers" %>

<!DOCTYPE html>
<html lang="en">
    <head>
        <title><%=HttpContext.Current.Request.Url.Host%></title>
        <!--#include virtual="~/_static/head.inc" -->
        <link href="data:image/x-icon;base64,AAABAAEAEBAQAAAAAAAoAQAAFgAAACgAAAAQAAAAIAAAAAEABAAAAAAAgAAAAAAAAAAAAAAAEAAAAAAAAAAAAAAAsC8qAP+EAACzh1cAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACAAAAAAAAACAAAAAAAAACAAAAAAAAEiAAAAADAAAiAAAAAAMzAiAAAAAAAAMzAAAAAAAAAiMzMAAAAAAAADAzAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD//wAA//8AAP//AAD//wAA//8AAP//AAD//wAA//8AAP//AAD//wAA//8AAP//AAD//wAA//8AAP//AAD//wAA" rel="icon" type="image/x-icon" />
    </head>
    <body>
        <div data-role="page" data-theme="b">
            <!--#include virtual="~/_static/header.shtml" -->
            <section class="viplogin">
                <div class="viplogin-container">
                    <div class="viplogin-box">
                        <form action="" class="viplogin-form" runat="server">
                            <h3><span id="formHeader"></span></h3>
                            <div class="input-group">
                                <div class="input-box">
                                    <input type="text" class="input" placeholder="" id="txtUsername" data-role="none">
                                    <span class="input-box-icon">
                                        <img src="img/icon-user.png" alt=""></span>
                                </div>
                            </div>
                            <div class="input-group">
                                <div class="input-box">
                                    <input type="password" class="input" placeholder="" id="txtPassword" data-role="none">
                                    <span class="input-box-icon">
                                        <img src="img/icon-password.png" alt=""></span>
                                </div>
                            </div>
                            <div class="input-group captcha">
                                <asp:Image ID="imgCaptcha" runat="server" CssClass="imgCaptcha" data-role="none"/>
                                <div class="input-box">
                                    <asp:TextBox ID="txtCaptcha" runat="server" MaxLength="4" type="tel" data-mini="true" data-corners="false" CssClass="input" data-role="none"/>
                                    <span class="input-box-icon">
                                        <img src="img/icon-password.png" alt=""></span>
                                </div>
                            </div>
                            <div class="input-group">
                                <input id="btnSubmit" type="submit" class="button" value="" data-role="none">
                            </div>
                        </form>
                        <div class="text-center">
                            <p>
                                <span id="loginNote0"></span>
                                <a id="csLink" href="/LiveChat/Default.aspx"></a>
                                <span id="loginNote1"></span>
                            </p>
                            <div id="sslNote"></div>
                        </div>
                    </div>
                </div>
            </section>
        </div>
    </body>
    <script type="text/javascript">
        $(function () {
            var elems = {
                submitButton: $('#btnSubmit'),
                captchaDiv: $('.captcha'),
                username: $('#txtUsername'),
                password: $('#txtPassword'),
                captcha: $('#<%=txtCaptcha.ClientID%>'),
                captchaImg: $('#<%=imgCaptcha.ClientID%>')
            }, 
            translations = {
                Exception: '<%=RewardsHelper.GetTranslation(TranslationKeys.Errors.Exception)%>',
                IncorrectVCode: '<%=RewardsHelper.GetTranslation(TranslationKeys.Errors.IncorrectVCode)%>',
                InactiveAccount: '<%=GetTranslation("Login_InactiveAccount", "messages")%>',
                InvalidUsernamePassword: '<%=RewardsHelper.GetTranslation(TranslationKeys.Errors.InvalidUsernamePassword)%>',
                MissingPassword: '<%=RewardsHelper.GetTranslation(TranslationKeys.Errors.MissingPassword)%>',
                MissingUsername: '<%=RewardsHelper.GetTranslation(TranslationKeys.Errors.MissingUsername)%>',
                VipOnly: '<%=GetTranslation("VIP_NotAllowed", "messages")%>'
            },
            login = new Login(translations, elems, '/Index.aspx', true);
            login.initializeButtons();

            var language = '<%=Language%>';
            if (language == 'zh-cn') {
                $("body").addClass("ch");
            }
            else {
                $("body").removeClass("ch");
            }

            $("#formHeader").text('<%=GetTranslation("LABEL_VIP_LOGIN")%>');
            $("#txtUsername").attr("placeholder", '<%=GetTranslation("LABEL_USERNAME")%>');
            $("#txtPassword").attr("placeholder", '<%=GetTranslation("LABEL_PASSWORD")%>');
            $("#txtCaptcha").attr("placeholder", '<%=GetTranslation("LABEL_CAPTCHA")%>');
            $("#btnSubmit").val('<%=GetTranslation("BUTTON_LOGIN")%>');

            var note0 = '<%=GetTranslation("LABEL_VIP_LOGIN_NOTE_0")%>'.split('{0}');
            $("#loginNote0").text(note0[0]);
            $("#loginNote1").text(note0[1]);
            $("#sslNote").text('<%=GetTranslation("LABEL_VIP_LOGIN_NOTE_1")%>');
            $("#forgot").text('<%=GetTranslation("LABEL_FORGOTPASSWORD")%>');
            $("#csLink").text('<%=GetTranslation("LABEL_CS_LINK")%>');                           
        });
    </script>
</html>