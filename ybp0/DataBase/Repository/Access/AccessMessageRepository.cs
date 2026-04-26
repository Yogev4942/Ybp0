using DataBase.Connection;
using DataBase.Repository.Interfaces;
using Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DataBase.Repository.Access
{
    public class AccessMessageRepository : IMessageRepository
    {
        private readonly IDataBaseConnection _database;

        public AccessMessageRepository() : this(DatabaseFilter.CreateConnection())
        {
        }

        public AccessMessageRepository(IDataBaseConnection database)
        {
            _database = database ?? DatabaseFilter.CreateConnection();
        }

        public void SendMessage(int senderId, int recipientId, string messageText)
        {
            _database.ExecuteNonQuery(
                "INSERT INTO [MessagesTbl] ([SenderId], [RecipientId], [MessageText], [SentAt]) VALUES (?, ?, ?, ?)",
                senderId, recipientId, messageText, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        public List<Message> GetConversation(int userIdA, int userIdB)
        {
            var dt = _database.ExecuteQuery(
                "SELECT * FROM [MessagesTbl] WHERE ([SenderId] = ? AND [RecipientId] = ?) OR ([SenderId] = ? AND [RecipientId] = ?) ORDER BY [SentAt] ASC",
                userIdA, userIdB, userIdB, userIdA);

            var messages = new List<Message>();
            foreach (DataRow row in dt.Rows)
            {
                messages.Add(MapMessage(row));
            }

            return messages;
        }

        public Message GetLatestMessage(int userIdA, int userIdB)
        {
            var dt = _database.ExecuteQuery(
                "SELECT TOP 1 * FROM [MessagesTbl] WHERE ([SenderId] = ? AND [RecipientId] = ?) OR ([SenderId] = ? AND [RecipientId] = ?) ORDER BY [SentAt] DESC, [Id] DESC",
                userIdA, userIdB, userIdB, userIdA);

            if (dt.Rows.Count == 0)
            {
                return null;
            }

            return MapMessage(dt.Rows[0]);
        }

        public Dictionary<int, Message> GetLatestMessagesByContacts(int userId, IEnumerable<int> contactIds)
        {
            List<int> ids = contactIds?
                .Distinct()
                .Where(id => id > 0 && id != userId)
                .ToList() ?? new List<int>();

            if (ids.Count == 0)
            {
                return new Dictionary<int, Message>();
            }

            string contactPredicate = string.Join(
                " OR ",
                ids.Select(_ => "([SenderId] = ? AND [RecipientId] = ?) OR ([SenderId] = ? AND [RecipientId] = ?)"));

            var parameters = new List<object>();
            foreach (int contactId in ids)
            {
                parameters.Add(userId);
                parameters.Add(contactId);
                parameters.Add(contactId);
                parameters.Add(userId);
            }

            var dt = _database.ExecuteQuery(
                $"SELECT * FROM [MessagesTbl] WHERE {contactPredicate} ORDER BY [SentAt] DESC, [Id] DESC",
                parameters.ToArray());

            var latestByContact = new Dictionary<int, Message>();
            foreach (DataRow row in dt.Rows)
            {
                Message message = MapMessage(row);
                int contactId = message.SenderId == userId ? message.RecipientId : message.SenderId;

                if (!latestByContact.ContainsKey(contactId))
                {
                    latestByContact[contactId] = message;
                }
            }

            return latestByContact;
        }

        public List<int> GetChatContactIds(int userId)
        {
            var dt = _database.ExecuteQuery(
                "SELECT DISTINCT IIF([SenderId] = ?, [RecipientId], [SenderId]) AS ContactId FROM [MessagesTbl] WHERE [SenderId] = ? OR [RecipientId] = ?",
                userId, userId, userId);

            var contacts = new List<int>();
            foreach (DataRow row in dt.Rows)
            {
                contacts.Add(Convert.ToInt32(row["ContactId"]));
            }

            return contacts;
        }

        private static Message MapMessage(DataRow row)
        {
            return new Message
            {
                Id = Convert.ToInt32(row["Id"]),
                SenderId = Convert.ToInt32(row["SenderId"]),
                RecipientId = Convert.ToInt32(row["RecipientId"]),
                MessageText = Convert.ToString(row["MessageText"]),
                SentAt = Convert.ToDateTime(row["SentAt"])
            };
        }
    }
}
