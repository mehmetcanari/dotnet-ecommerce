interface IUserRepository
{
    List<User> GetAllUsers();
    User GetUserWithId(int id);
    void AddUser(CreateUserDto createUserDto);
    void UpdateUser(UpdateUserDto updateUserDto);
    void DeleteUser(int id);
}