﻿window.w88Mobile.Gateways.WeChatV2 = WeChatV2();
var _w88_wechat = window.w88Mobile.Gateways.WeChatV2;

function WeChatV2() {

    var wechat = {};

    try {
        wechat = Object.create(new w88Mobile.Gateway(_w88_paymentSvcV2));
    } catch (err) {
        wechat = {};
    }

    wechat.init = function () {
        $('[id$="lblSwitchLine"]').text($.i18n("LABEL_SWITCH_LINE"));

        $(".pay-note").show();
        $("#paymentNote").text($.i18n("LABEL_PAYMENT_NOTE"));
        $("#paymentNoteContent").html($.i18n("LABEL_PAYMENT_NOTE_WECHAT"));
    };

    wechat.createDeposit = function () {
        var _self = this;
        var params = _self.getUrlVars();
        var data = {
            Amount: params.Amount,
            ThankYouPage: params.ThankYouPage,
            SwitchLine: params.SwitchLine,
        };

        _self.methodId = params.MethodId;
        _self.changeRoute();
        _self.deposit(data, function (response) {
            switch (response.ResponseCode) {
                case 1:
                    if (response.ResponseData.VendorRedirectionUrl) {
                        window.open(response.ResponseData.VendorRedirectionUrl);
                    } else {
                        if (response.ResponseData.PostUrl) {
                            w88Mobile.PostPaymentForm.create(response.ResponseData.FormData, response.ResponseData.PostUrl, "body");
                            w88Mobile.PostPaymentForm.submit();
                        } else if (response.ResponseData.DummyURL) {
                            w88Mobile.PostPaymentForm.create(response.ResponseData.FormData, response.ResponseData.DummyURL, "body");
                            w88Mobile.PostPaymentForm.submit();
                        }
                    }

                    break;
                default:
                    if (_.isArray(response.ResponseMessage))
                        w88Mobile.Growl.shout(w88Mobile.Growl.bulletedList(response.ResponseMessage), _self.shoutCallback);
                    else
                        w88Mobile.Growl.shout(response.ResponseMessage, _self.shoutCallback);

                    break;
            }
        },
            function () {
                pubsub.publish('stopLoadItem', { selector: "" });
            });
    }

    return wechat;
}
