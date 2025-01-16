public record CreateUserDto
{
    public string Name {get;set;}
    public DateTime UserCreated = DateTime.Now;
}