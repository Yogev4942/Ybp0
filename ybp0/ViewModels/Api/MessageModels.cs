namespace ViewModels.Api;

public record MessageViewModel(
    int Id,
    int SenderId,
    string? SenderUsername,
    int RecipientId,
    string? RecipientUsername,
    string MessageText,
    DateTime SentAt);

public record CreateMessageRequest(int SenderId, int RecipientId, string MessageText);
