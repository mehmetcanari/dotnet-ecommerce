public record UpdateUserDto
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
}    