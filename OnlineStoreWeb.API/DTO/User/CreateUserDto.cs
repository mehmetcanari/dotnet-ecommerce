public record CreateUserDto
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public DateTime UserCreated = DateTime.UtcNow;
}