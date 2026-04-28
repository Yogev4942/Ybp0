using Models;

namespace DataBase.Interfaces;

public interface IMessageRepository
{
    Task<IEnumerable<Message>> GetAllAsync();
    Task<Message?> GetByIdAsync(int id);
    Task AddAsync(Message item);
    Task UpdateAsync(Message item);
    Task DeleteAsync(int id);
    Task<IEnumerable<Message>> GetConversationAsync(int userIdA, int userIdB);
    Task<IEnumerable<int>> GetChatContactIdsAsync(int userId);
}
