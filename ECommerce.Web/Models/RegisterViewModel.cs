using System.ComponentModel.DataAnnotations;

namespace ECommerce.Web.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Ad alanı zorunludur.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Ad 2-50 karakter arasinda olmalidir.")]
    [Display(Name = "Ad")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad alanı zorunludur.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Soyad 2-50 karakter arasinda olmalidir.")]
    [Display(Name = "Soyad")]
    public string Surname { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email alanı zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "T.C. Kimlik No zorunludur.")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "T.C. Kimlik No 11 haneli olmalıdır.")]
    [RegularExpression(@"^[0-9]{11}$", ErrorMessage = "T.C. Kimlik No sadece rakamlardan oluşmalıdır.")]
    [Display(Name = "T.C. Kimlik No")]
    public string IdentityNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre alani zorunludur.")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "Şifre en az 8 karakter olmalıdır.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Cinsiyet")]
    public string? Gender { get; set; }

    [Required(ErrorMessage = "Telefon alanı zorunludur.")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Telefon numarası 10 haneli olmalıdır.")]
    [Display(Name = "Telefon")]
    public string Phone { get; set; } = string.Empty;

    [Display(Name = "Telefon Kodu")]
    public string PhoneCode { get; set; } = "90";

    [Required(ErrorMessage = "Doğum tarihi zorunludur.")]
    [DataType(DataType.Date)]
    [Display(Name = "Doğum Tarihi")]
    public DateTime? DateOfBirth { get; set; }

    [Display(Name = "Elektronik Ticaret Iletişim Onayı")]
    public bool ElectronicConsent { get; set; }

    [Display(Name = "Üyelik Sözleşmesi Onayı")]
    public bool MembershipAgreement { get; set; }

    [Display(Name = "KVKK Aydınlatma Metni Onayı")]
    public bool KvkkConsent { get; set; }

    public string Country { get; set; } = "Türkiye";
    public string City { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}
