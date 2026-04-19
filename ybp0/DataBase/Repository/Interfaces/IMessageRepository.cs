using Models;
using System.Collections.Generic;

namespace DataBase.Repository.Interfaces
{
    public interface IMessageRepository
    {
        void SendMessage(int senderId, int recipientId, string messageText);
        List<Message> GetConversation(int userIdA, int userIdB);
        Message GetLatestMessage(int userIdA, int userIdB);
        List<int> GetChatContactIds(int userId);
    }
}
