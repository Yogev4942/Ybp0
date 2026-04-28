using System;

namespace Models
{
    public class Message
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int RecipientId { get; set; }
        public string MessageText { get; set; }
        public DateTime SentAt { get; set; }
        public User Sender { get; set; }
        public User Recipient { get; set; }
    }
}
