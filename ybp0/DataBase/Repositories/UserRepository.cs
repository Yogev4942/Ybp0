using DataBase.Interfaces;
using DataBase.Security;
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

        PasswordHash passwordHash = PasswordHasher.Create(item.Password);
        item.Password = passwordHash.Hash;
        item.PasswordSalt = passwordHash.Salt;

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

    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        User? user = await _context.Users.FirstOrDefaultAsync(item => item.Username == username);
        if (user is null)
        {
            return null;
        }

        bool isValid = PasswordHasher.Verify(password, user.Password, user.PasswordSalt, out bool needsUpgrade);
        if (!isValid)
        {
            return null;
        }

        if (needsUpgrade)
        {
            PasswordHash passwordHash = PasswordHasher.Create(password);
            user.Password = passwordHash.Hash;
            user.PasswordSalt = passwordHash.Salt;
            await _context.SaveChangesAsync();
        }

        return user;
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
