namespace ECommerce.Web.Utility;

public static class ClientErrorMessageTranslator
{
    public static string GetUserFriendlyMessage(string? apiErrorMessage)
    {
        if (string.IsNullOrEmpty(apiErrorMessage))
            return "İşlem başarısız oldu. Lütfen tekrar deneyiniz.";

        return apiErrorMessage switch
        {
            "authentication.invalid.credentials" => "E-posta veya şifre hatalı.",
            "authentication.account.not.authorized" => "Bu hesap yetkili değil.",
            "account.not.found" => "Kullanıcı bulunamadı.",
            "account.email.already.in.use" => "Bu e-posta adresi zaten kullanılıyor.",
            _ when apiErrorMessage.Contains("invalid", StringComparison.OrdinalIgnoreCase) => "Geçersiz bilgi girdiniz.",
            _ => "İşlem başarısız oldu. Lütfen tekrar deneyiniz."
        };
    }
}