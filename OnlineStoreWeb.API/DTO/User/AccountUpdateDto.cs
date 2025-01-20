public record AccountUpdateDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string Address { get; set; }
    public required string PhoneNumber { get; set; }
}    