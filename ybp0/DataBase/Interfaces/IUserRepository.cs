using Models;

namespace DataBase.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task AddAsync(User item);
    Task UpdateAsync(User item);
    Task DeleteAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> AuthenticateAsync(string username, string password);
    Task<bool> UsernameExistsAsync(string username);
    Task<bool> EmailExistsAsync(string email);
}
