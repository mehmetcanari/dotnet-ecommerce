using System.ComponentModel.DataAnnotations;

namespace ECommerce.Web.Models; 

public class LoginViewModel
{
    [Required(ErrorMessage = "Email alanı zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir email giriniz.")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Şifre alanı zorunludur.")]
    [DataType(DataType.Password)]
    public required string Password { get; set; }
}