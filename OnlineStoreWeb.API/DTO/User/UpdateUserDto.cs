public record UpdateUserDto
{
    public string Name {get;set;}
    public DateTime UserUpdated = DateTime.Now;
}