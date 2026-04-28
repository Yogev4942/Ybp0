using DataBase.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataBase.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly AppDbContext _context;

    public MessageRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Message>> GetAllAsync()
    {
        return await Query().OrderByDescending(message => message.SentAt).ToListAsync();
    }

    public Task<Message?> GetByIdAsync(int id)
    {
        return Query().FirstOrDefaultAsync(message => message.Id == id);
    }

    public async Task AddAsync(Message item)
    {
        if (item.SentAt == default)
        {
            item.SentAt = DateTime.UtcNow;
        }

        await _context.Messages.AddAsync(item);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Message item)
    {
        _context.Messages.Update(item);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        Message? message = await _context.Messages.FirstOrDefaultAsync(item => item.Id == id);
        if (message is null)
        {
            return;
        }

        _context.Messages.Remove(message);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Message>> GetConversationAsync(int userIdA, int userIdB)
    {
        return await Query()
            .Where(message =>
                (message.SenderId == userIdA && message.RecipientId == userIdB) ||
                (message.SenderId == userIdB && message.RecipientId == userIdA))
            .OrderBy(message => message.SentAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<int>> GetChatContactIdsAsync(int userId)
    {
        return await _context.Messages
            .Where(message => message.SenderId == userId || message.RecipientId == userId)
            .Select(message => message.SenderId == userId ? message.RecipientId : message.SenderId)
            .Distinct()
            .OrderBy(id => id)
            .ToListAsync();
    }

    private IQueryable<Message> Query()
    {
        return _context.Messages
            .Include(message => message.Sender)
            .Include(message => message.Recipient);
    }
}
