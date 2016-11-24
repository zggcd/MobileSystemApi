﻿<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Site.master" AutoEventWireup="true" CodeFile="Rebates.aspx.cs" Inherits="Rebates" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <script type="text/javascript" src="/_Static/JS/modules/rebates.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">

    <div class="ui-content rebates" role="main">
        <form action="" class="form">
            <ul class="list fixed-tablet-size">
                <li class="item item-select">
                    <label id="labelPeriod"></label>
                    <div class="ui-select">
                        <div class="">
                            <span>
                                <select id="weeks"></select>
                            </span>
                        </div>
                    </div>
                    <p class="text-center"><small><span id="monday"></span> (<span id="startdate"></span>) - <span id="sunday"></span> (<span id="endDate"></span>)</small> </p>
                </li>
            </ul>

            <div id="group"></div>

            <div class="rebates-disclaimer">
                <p><small>Disclaimer: Min. Rebate Amount: USD 1.00 Rebate Amount displayed may deviate from the actual rebate paid out. Refer to "Promotion Claim" history for the actual payout.</small></p>
            </div>
        </form>
    </div>


    <div id="rebatesModal" data-role="popup" data-overlay-theme="b" data-theme="b" data-history="false">
        <a href="#" id="closeModal" class="close close-enhanced">&times;</a>

        <div class="padding">
            <div id="modalContent">
          
            </div>
        </div>
    </div>

    <script>

        $("#rebatesClaim").bind("click", function () {
            $('#rebatesModal').popup('open');
        });
        $("#closeModal").bind("click", function () {
            $('#rebatesModal').popup('close');
        });

        $(document).ready(function () {
            window.w88Mobile.Rebates.Initialize();

            $("#group").load(function () {

                window.w88Mobile.Rebates.Statement();
            });

            $("#weeks").change(function () {

                window.w88Mobile.Rebates.Statement();
            });

        });

    </script>

</asp:Content>

