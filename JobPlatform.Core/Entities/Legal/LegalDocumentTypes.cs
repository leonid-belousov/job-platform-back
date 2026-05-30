namespace JobPlatform.Core.Entities.Legal;

public static class LegalDocumentTypes
{
    public const string PrivacyPolicy = "privacy_policy";
    public const string UserAgreement = "user_agreement";

    public static readonly IReadOnlyCollection<string> All = new[]
    {
        PrivacyPolicy,
        UserAgreement
    };
}