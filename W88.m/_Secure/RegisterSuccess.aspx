﻿<%@ Page Language="C#" AutoEventWireup="true" CodeFile="RegisterSuccess.aspx.cs" Inherits="_Secure_RegisterSuccess" %>

<!DOCTYPE html>
<html>
<head>
    <title><%=commonCulture.ElementValues.getResourceString("brand", commonVariables.LeftMenuXML) + commonCulture.ElementValues.getResourceString("register", commonVariables.LeftMenuXML)%></title>
    <!--#include virtual="~/_static/head.inc" -->
    <%--<script type="text/javascript" src="/_Static/Js/PreLoad.js"></script>--%>
    <script type="text/javascript" src="/_Static/Js/Main.js"></script>
    <script type="application/javascript" src="/_Static/Js/add2home.js"></script>
    <script type="text/javascript" src="/_Static/Js/radar.js"></script>
</head>
<body>
    <div id="register"  data-role="page" data-theme="b" data-ajax="false">
        <!--#include virtual="~/_static/header.shtml" -->

        <div class="ui-content" role="main">
            <div class="register-success-message">
                <div class="register-success-icon"><span class="icon icon-check"></span></div>
                <h4>Welcome to W88, and thank you for opening an account.</h4>
                <p>Your account is now ready for you to login and play on W88.com and mobile.</p>
                <p>Depositing is Quick and Easy. We have a huge range of deposit options available.</p>
                <p id="paymentNote">
                </p>
            </div>

            <div class="ui-navbar ui-navbar-justified register-gateway" role="navigation" id="depositTabs" runat="server">
            </div>

            <div class="bank_logo">
                <p class="reg-contact">
                    If you have any queries or problems when making a deposit,
                                    please <a href="/LiveChat/Default.aspx"
                                        target="_blank">Contact Us</a>.
                </p>
                <i class="logo_10"></i>
                <i class="logo_11"></i>
                <i class="logo_12"></i>
                <i class="logo_13"></i>
                <i class="logo_14"></i>
                <i class="logo_15"></i>
            </div>
        </div>

    <!--#include virtual="~/_static/navMenu.shtml" -->
    </div>

    <script type="text/javascript">
        $(function () {
            window.history.forward();
            $(".register-success-content").css("display", "none");
            if ($('#depositTabs li').length == 0) {
                w88Mobile.PiwikManager.trackEvent({
                    category: "RegSuccess",
                    action: "<%=base.strCountryCode %>",
                    name: "<%=base.strMemberID %>"
                });
                $('#paymentNote').append('<%= commonCulture.ElementValues.getResourceString("paymentNotice", commonVariables.PaymentMethodsXML)%>');
            } else {
                $('#paymentNote').append('<%= commonCulture.ElementValues.getResourceString("paymentDescription", commonVariables.PaymentMethodsXML)%>');
                $(".register-success-content").css("display", "");
            }
        });
    </script>
    <!-- /page -->
</body>
</html>
