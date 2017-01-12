/// <reference path="B1SLReference.ts"/>
window.onload = function () {
    // Login
    var loginInfo = new SAPB1.LoginInfo();
    loginInfo.CompanyDB = "SBODEMOUS";
    loginInfo.UserName = "manager";
    loginInfo.Password = "1234";
    SAPB1.Session.Login(loginInfo);
    // Logout
    SAPB1.Session.Logout(sessionId, routeId);
    // Add BP
    var bp = new SAPB1.BusinessPartner();
    bp.CardCode = "te";
    bp.CardCode = "TestTS1";
    bp.CardName = "TestTS1";
    bp.CardType = SAPB1.BoCardTypes.cLid;
    bp.CardForeignName = "FoName";
    bp.DeductibleAtSource = SAPB1.BoYesNoEnum.tNO;
    bp.ContactEmployees = new Array(2);
    bp.ContactEmployees[0] = new SAPB1.ContactEmployee();
    bp.ContactEmployees[0].Name = "E1";
    bp.ContactEmployees[0].Address = "FR";
    var e2 = new SAPB1.ContactEmployee();
    e2.Name = "E2";
    e2.Address = "US";
    bp.ContactEmployees[1] = e2;
    var sessionId = "";
    var routeId = "";
    SAPB1.BusinessPartnersEntity.Add(bp, sessionId, routeId);
    // Get BP
    var bpp = new SAPB1.BusinessPartnerParams();
    bpp.CardCode = "C20000";
    SAPB1.BusinessPartnersEntity.Get(bpp, sessionId, routeId);
    // Add Document
    var so = new SAPB1.Document();
    so.DocType = SAPB1.BoDocumentTypes.dDocument_Items;
    so.CardCode = "C20000";
    so.DocDueDate = new Date(Date.now());
    so.DocDate = new Date(Date.now());
    so.DocTime = new Date(Date.now());
    so.DocumentLines = new Array();
    so.DocumentLines[0] = new SAPB1.DocumentLine();
    so.DocumentLines[0].ItemCode = "A00001";
    so.DocumentLines[0].Quantity = 2;
    so.DocumentLines[0].Price = 20;
    so.DocumentLines[1] = new SAPB1.DocumentLine();
    so.DocumentLines[1].ItemCode = "A00002";
    so.DocumentLines[1].Quantity = 3;
    so.DocumentLines[1].Price = 30;
    SAPB1.OrdersEntity.Add(so, sessionId, routeId);
    // Update Document - SO
    so = new SAPB1.Document();
    so.DocEntry = 1;
    so.Comments = "Updated via SL";
    SAPB1.OrdersEntity.Update(so, sessionId, routeId);
    // Get Document - SO
    var soParams = new SAPB1.DocumentParams();
    soParams.DocEntry = 1;
    SAPB1.OrdersEntity.Get(soParams, sessionId, routeId);
    // Delete Document - SO
    var soParams = new SAPB1.DocumentParams();
    soParams.DocEntry = 1;
    SAPB1.OrdersEntity.Delete(soParams, sessionId, routeId);
    // CompanyService -> GetAdminInfo
    var companyInfo = new SAPB1.CompanyInfo();
    companyInfo.CompanyName = "";
    SAPB1.CompanyService.GetAdminInfo(sessionId, routeId);
    // Add Campaign
    var newCampaign = new SAPB1.Campaign();
    newCampaign.CampaignName = "FirstCampaign";
    newCampaign.CampaignType = SAPB1.CampaignTypeEnum.ctEmail;
    newCampaign.Status = SAPB1.CampaignStatusEnum.csOpen;
    newCampaign.StartDate = new Date();
    newCampaign.CampaignBusinessPartners = new Array();
    newCampaign.CampaignBusinessPartners[0] = new SAPB1.CampaignBusinessPartner();
    newCampaign.CampaignBusinessPartners[0].BPCode = "bp1";
    newCampaign.CampaignBusinessPartners[1] = new SAPB1.CampaignBusinessPartner();
    newCampaign.CampaignBusinessPartners[1].BPCode = "bp2";
    newCampaign.CampaignItems = new Array();
    newCampaign.CampaignItems[0] = new SAPB1.CampaignItem();
    newCampaign.CampaignItems[0].ItemCode = "itm1";
    newCampaign.CampaignItems[1] = new SAPB1.CampaignItem();
    newCampaign.CampaignItems[1].ItemCode = "itm2";
    SAPB1.CampaignsEntity.Add(newCampaign, "", "");
};
//# sourceMappingURL=app.js.map