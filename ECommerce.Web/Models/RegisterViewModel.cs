using System.ComponentModel.DataAnnotations;

namespace ECommerce.Web.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Ad alani zorunludur.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Ad 2-50 karakter arasinda olmalidir.")]
    [Display(Name = "Ad")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad alani zorunludur.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Soyad 2-50 karakter arasinda olmalidir.")]
    [Display(Name = "Soyad")]
    public string Surname { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email alani zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sifre alani zorunludur.")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "Sifre en az 8 karakter olmalidir.")]
    [DataType(DataType.Password)]
    [Display(Name = "Sifre")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Cinsiyet")]
    public string? Gender { get; set; }

    [Required(ErrorMessage = "Telefon alani zorunludur.")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Telefon numarasi 10 haneli olmalidir.")]
    [Display(Name = "Telefon")]
    public string Phone { get; set; } = string.Empty;

    [Display(Name = "Telefon Kodu")]
    public string PhoneCode { get; set; } = "90";

    [Required(ErrorMessage = "Dogum tarihi zorunludur.")]
    [DataType(DataType.Date)]
    [Display(Name = "Dogum Tarihi")]
    public DateTime? DateOfBirth { get; set; }

    [Display(Name = "Elektronik Ticaret Iletisim Onayi")]
    public bool ElectronicConsent { get; set; }

    [Display(Name = "Üyelik Sözlesmesi Onayi")]
    public bool MembershipAgreement { get; set; }

    [Display(Name = "KVKK Aydinlatma Metni Onayi")]
    public bool KvkkConsent { get; set; }

    public string IdentityNumber { get; set; } = string.Empty;
    public string Country { get; set; } = "Türkiye";
    public string City { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}
