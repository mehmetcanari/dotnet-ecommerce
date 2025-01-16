interface IUserRepository
{
    Task<List<User>> GetAllUsersAsync();
    Task<User?> GetUserWithIdAsync(int id);
    Task AddUserAsync(CreateUserDto createUserDto);
    Task UpdateUserAsync(UpdateUserDto updateUserDto);
    Task DeleteUserAsync(int id);
}