using DataBase.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataBase.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .OrderBy(user => user.Id)
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.FirstOrDefaultAsync(user => user.Id == id);
    }

    public async Task AddAsync(User item)
    {
        if (item.JoinDate == default)
        {
            item.JoinDate = DateTime.UtcNow;
        }

        await _context.Users.AddAsync(item);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User item)
    {
        _context.Users.Update(item);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        User? user = await _context.Users.FirstOrDefaultAsync(item => item.Id == id);
        if (user is null)
        {
            return;
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }

    public Task<User?> GetByUsernameAsync(string username)
    {
        return _context.Users.FirstOrDefaultAsync(user => user.Username == username);
    }

    public Task<User?> AuthenticateAsync(string username, string password)
    {
        return _context.Users.FirstOrDefaultAsync(user => user.Username == username && user.Password == password);
    }

    public Task<bool> UsernameExistsAsync(string username)
    {
        return _context.Users.AnyAsync(user => user.Username == username);
    }

    public Task<bool> EmailExistsAsync(string email)
    {
        return _context.Users.AnyAsync(user => user.Email == email);
    }
}
