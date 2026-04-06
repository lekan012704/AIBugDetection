namespace SharedKernel.Helpers
{
    public static class ApplicationConstant
    {
        /// <summary>
        /// Endpoints 
        /// </summary>
        public const string ApiPrefix = "api/SelfService";
        public const string ApiVersion = "api/v1";
        public const string ApproveTaxPayer = "/api/v1/PayerRegistration/approve-tax-payer-individual";
        public const string DisapproveTaxPayer = "/api/v1/PayerRegistration/disapprove-tax-payer-individual";
        public const string GetPassword = "/api/v1/PayerRegistration/get-payerId-details?payerId=";
        public const string GetTaxPayerDetails = "/api/v1/PayerRegistration/self-get-tax-payer-details";
        public const string GetTaxAgentCount = "/api/v1/PayerRegistration/self-get-tax-agents-by-taxagentTIN?TaxAgentTIN=";
        public const string GetAssessmentSummaryDetails = "/api/v1/PayerRegistration/self-get-assessment-Summary-details?mainTaxAgentTin=";
        public const string GetCollectionSummaryDetails = "/api/v1/PayerRegistration/self-get-collection-Summary-details?mainTaxAgentTin=";
        public const string CreateTaxPayer = "/api/v1/PayerRegistration/create-tax-payer-individual";
        public const string TaxCalculator = "/api/TaxCalculator";
        public const string GetIncomeSourceClassify = "/api/assessment/incomeSource/classify?";
        public const string GetTccYears = "api/tcc/getTccYears";
        public const string GetTaxSmartUsersDetails = "api/account/user-details-by-roleId";
        public const string ValidAssessments = "api/assessment/validAssessments";
        public const string checkHighNetworthMethodNames = "/api/assessment/payer/CheckHighNetworthSelfStatus";
        public const string GeneratePaymentCode = "/api/v1/Invoice/generatePaymentCode";
        public const string GenerateInvoiceLink = "/api/v1/Invoice/int/generate-taxsmart-assessment-notice?&A0dTUM4S=";
        public const string GeneratePaymentLink = "/Production/Payment?&ref=";
        public const string SelfServiceIncomeTaxReturns = "api/assessment/payer/selfServiceIncomeTaxReturnsNew";
        public const string SendTaxSmartAssessmentNotice = "/api/v1/Invoice/int/send-taxsmart-assessment-notice";
        public const string AssessmentReversal = "/api/v1/Invoice/self-update-assesment-reversal";
        public const string UpdateJtbTaxSmart = "/api/registration/UpdateUserJtb";
        public const string UpdateJtbReg = "/api/v1/PayerRegistration/self-update-view-payer-info";
        




        /// <summary>
        /// Directories and files
        /// </summary>
        public const string WkHtmlToPdfExecutablePath = @"C:\Program Files\wkhtmltopdf\bin\wkhtmltopdf.exe";
        public const string EmailTemplateDir = "EmailTemplate";
        public const string GetPasswordEmailTemplate = "GetPasswordEmail.html";
        public const string GetTaxComputedNoticeTemplate = "TaxComputedNotice.html";
        public const string GetUnderRemittanceNoticeBeforeTemplate = "UnderRemittanceNoticeBefore.html";
        public const string DisapproveIndividualEmailTemplate = "DisapprovalEmail.html";
        public const string SupportDocFolderName = "SubmittedSupportingDoc";
        public const string MonthlyReturnFolderName = "SubmittedMonthReturns";
        /// <summary>
        /// Constants
        /// </summary>
        public const string DefaultPassword = "123Pa$$word.";
        public const string RecoveryPasswordTitle = "Password Recovery";
        public const string GetOTPTitle = "Get OneTime Access Password";
        public const string GetOTPTitle2 = "Registration Get OneTime Access Password";
        public static readonly string AnnualReturnFolderName = "SubmittedAnnualReturns";
        public static readonly string WtholdingReturnFolderName = "SubmittedAnnualWTHReturns";
    }
}
