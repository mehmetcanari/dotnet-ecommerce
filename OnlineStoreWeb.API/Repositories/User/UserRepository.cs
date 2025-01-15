
public class UserRepository : IUserRepository
{
    public readonly StoreDbContext _context;

    public UserRepository(StoreDbContext context)
    {
        _context = context;
    }

    public void AddUser(CreateUserDto createUserDto)
    {
        throw new NotImplementedException();
    }

    public void DeleteUser(int id)
    {
        throw new NotImplementedException();
    }

    public List<User> GetAllUsers()
    {
        throw new NotImplementedException();
    }

    public User GetUserWithId(int id)
    {
        throw new NotImplementedException();
    }

    public void UpdateUser(UpdateUserDto updateUserDto)
    {
        throw new NotImplementedException();
    }
}