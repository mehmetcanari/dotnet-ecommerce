using Microsoft.EntityFrameworkCore;

public class UserRepository : IUserRepository
{
    private readonly StoreDbContext _context;

    public UserRepository(StoreDbContext context)
    {
        _context = context;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        try
        {
            return await _context.Users.ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Error fetching users", ex);
        }
    }

    public async Task<User?> GetUserWithIdAsync(int id)
    {
        try
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }
        catch (Exception ex)
        {
            throw new Exception("Error fetching user", ex);
        }
    }

    public async Task AddUserAsync(CreateUserDto createUserDto)
    {
        try
        {
            var user = new User
            {
                Username = createUserDto.Username,
                Email = createUserDto.Email,
                Password = createUserDto.Password,
                UserCreated = createUserDto.UserCreated
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Error adding user", ex);
        }
    }

    public async Task UpdateUserAsync(UpdateUserDto updateUserDto)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == updateUserDto.Id)
                ?? throw new Exception("User not found");

            user.Username = updateUserDto.Username;
            user.Email = updateUserDto.Email;
            user.Password = updateUserDto.Password;
            user.UserUpdated = updateUserDto.UserUpdated;

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Error updating user", ex);
        }
    }

    public async Task DeleteUserAsync(int id)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id)
                ?? throw new Exception("User not found");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Error deleting user", ex);
        }
    }
}