using Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace ViewModels.ViewModels
{
    public class ChatsViewModel : BaseViewModel
    {
        private readonly IDatabaseService _dbService;
        private readonly INavigationService _navigationService;
        private readonly User _currentUser;
        private User _initialChatUser;

        private ObservableCollection<ChatPreviewItemViewModel> _chats;
        public ObservableCollection<ChatPreviewItemViewModel> Chats
        {
            get => _chats;
            set => SetProperty(ref _chats, value);
        }

        private ObservableCollection<ChatMessageItemViewModel> _messages;
        public ObservableCollection<ChatMessageItemViewModel> Messages
        {
            get => _messages;
            set => SetProperty(ref _messages, value);
        }

        private ChatPreviewItemViewModel _selectedChat;
        public ChatPreviewItemViewModel SelectedChat
        {
            get => _selectedChat;
            set
            {
                if (SetProperty(ref _selectedChat, value))
                {
                    LoadMessagesForSelectedChat();
                }
            }
        }

        private string _newMessage;
        public string NewMessage
        {
            get => _newMessage;
            set => SetProperty(ref _newMessage, value);
        }

        public ICommand SelectChatCommand { get; }
        public ICommand SendMessageCommand { get; }

        public ChatsViewModel(IDatabaseService dbService, INavigationService navigationService, User currentUser, User initialChatUser = null)
        {
            _dbService = dbService;
            _navigationService = navigationService;
            _currentUser = currentUser;
            _initialChatUser = initialChatUser;

            Chats = new ObservableCollection<ChatPreviewItemViewModel>();
            Messages = new ObservableCollection<ChatMessageItemViewModel>();
            SelectChatCommand = new RelayCommand(param => SelectChat(param as ChatPreviewItemViewModel));
            SendMessageCommand = new RelayCommand(_ => SendMessage());

            LoadChats();
        }

        public override void OnNavigatedTo()
        {
            base.OnNavigatedTo();
            LoadChats();
        }

        private void LoadChats()
        {
            if (_currentUser == null)
            {
                Chats.Clear();
                Messages.Clear();
                SelectedChat = null;
                return;
            }

            int? selectedChatUserId = SelectedChat?.UserId ?? _initialChatUser?.Id;

            List<User> contacts = ResolveContacts()
                .Where(user => user != null && user.Id != _currentUser.Id)
                .GroupBy(user => user.Id)
                .Select(group => group.First())
                .ToList();

            Chats.Clear();

            foreach (ChatPreviewItemViewModel chat in contacts
                .Select(CreateChatPreview)
                .OrderByDescending(item => item.LastActivity ?? DateTime.MinValue)
                .ThenBy(item => item.Name))
            {
                Chats.Add(chat);
            }

            _initialChatUser = null;

            SelectedChat = selectedChatUserId.HasValue
                ? Chats.FirstOrDefault(chat => chat.UserId == selectedChatUserId.Value) ?? Chats.FirstOrDefault()
                : Chats.FirstOrDefault();

            if (SelectedChat == null)
            {
                Messages.Clear();
            }
        }

        private List<User> ResolveContacts()
        {
            var contacts = new List<User>();

            if (_currentUser.IsTrainer)
            {
                contacts.AddRange(_dbService.GetTraineesByTrainerId(_currentUser.Id).Cast<User>());
            }
            else
            {
                contacts.AddRange(_dbService.SearchTrainers(string.Empty).Cast<User>());
            }

            foreach (int contactId in ChatStore.GetContactsForUser(_currentUser.Id))
            {
                User contact = _dbService.GetUserById(contactId);
                if (contact != null)
                {
                    contacts.Add(contact);
                }
            }

            if (_initialChatUser != null)
            {
                contacts.Add(_initialChatUser);
            }

            return contacts;
        }

        private ChatPreviewItemViewModel CreateChatPreview(User user)
        {
            ChatMessageRecord latestMessage = ChatStore.GetLatestMessage(_currentUser.Id, user.Id);
            return new ChatPreviewItemViewModel
            {
                UserId = user.Id,
                Name = user.Username,
                LastMessage = latestMessage != null ? latestMessage.Text : "No messages yet",
                LastActivity = latestMessage?.Timestamp,
                AvatarColor = GetColorForUser(user.Username)
            };
        }

        private void SelectChat(ChatPreviewItemViewModel chat)
        {
            if (chat != null)
            {
                SelectedChat = chat;
            }
        }

        private void LoadMessagesForSelectedChat()
        {
            Messages.Clear();

            if (SelectedChat == null || _currentUser == null)
            {
                return;
            }

            foreach (ChatMessageRecord record in ChatStore.GetMessages(_currentUser.Id, SelectedChat.UserId))
            {
                Messages.Add(new ChatMessageItemViewModel
                {
                    SenderId = record.SenderId,
                    Text = record.SenderId == _currentUser.Id ? $"You: {record.Text}" : record.Text,
                    Timestamp = record.Timestamp
                });
            }
        }

        private void SendMessage()
        {
            if (SelectedChat == null || string.IsNullOrWhiteSpace(NewMessage))
            {
                return;
            }

            int selectedChatUserId = SelectedChat.UserId;
            string text = NewMessage.Trim();
            ChatStore.AddMessage(_currentUser.Id, selectedChatUserId, text);
            NewMessage = string.Empty;

            LoadChats();
            SelectedChat = Chats.FirstOrDefault(chat => chat.UserId == selectedChatUserId);
        }

        private static string GetColorForUser(string username)
        {
            if (string.IsNullOrEmpty(username)) return "#FF00BCD4";
            int hash = username.GetHashCode();
            var colors = new[] { "#FF00BCD4", "#FF9C27B0", "#FF4CAF50", "#FFFF9800", "#FF3F51B5", "#FFE91E63" };
            return colors[Math.Abs(hash) % colors.Length];
        }
    }

    public class ChatPreviewItemViewModel : BaseViewModel
    {
        private string _lastMessage;

        public int UserId { get; set; }
        public string Name { get; set; }
        public string AvatarColor { get; set; }
        public DateTime? LastActivity { get; set; }

        public string LastMessage
        {
            get => _lastMessage;
            set => SetProperty(ref _lastMessage, value);
        }
    }

    public class ChatMessageItemViewModel
    {
        public int SenderId { get; set; }
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }
    }

    internal class ChatMessageRecord
    {
        public int SenderId { get; set; }
        public int RecipientId { get; set; }
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }
    }

    internal static class ChatStore
    {
        private static readonly object SyncRoot = new object();
        private static readonly Dictionary<string, List<ChatMessageRecord>> Conversations = new Dictionary<string, List<ChatMessageRecord>>();

        public static void AddMessage(int senderId, int recipientId, string text)
        {
            lock (SyncRoot)
            {
                string key = BuildKey(senderId, recipientId);
                if (!Conversations.TryGetValue(key, out List<ChatMessageRecord> messages))
                {
                    messages = new List<ChatMessageRecord>();
                    Conversations[key] = messages;
                }

                messages.Add(new ChatMessageRecord
                {
                    SenderId = senderId,
                    RecipientId = recipientId,
                    Text = text,
                    Timestamp = DateTime.Now
                });
            }
        }

        public static List<ChatMessageRecord> GetMessages(int firstUserId, int secondUserId)
        {
            lock (SyncRoot)
            {
                string key = BuildKey(firstUserId, secondUserId);
                if (!Conversations.TryGetValue(key, out List<ChatMessageRecord> messages))
                {
                    return new List<ChatMessageRecord>();
                }

                return messages
                    .OrderBy(message => message.Timestamp)
                    .Select(message => new ChatMessageRecord
                    {
                        SenderId = message.SenderId,
                        RecipientId = message.RecipientId,
                        Text = message.Text,
                        Timestamp = message.Timestamp
                    })
                    .ToList();
            }
        }

        public static ChatMessageRecord GetLatestMessage(int firstUserId, int secondUserId)
        {
            lock (SyncRoot)
            {
                string key = BuildKey(firstUserId, secondUserId);
                if (!Conversations.TryGetValue(key, out List<ChatMessageRecord> messages))
                {
                    return null;
                }

                ChatMessageRecord latest = messages.OrderByDescending(message => message.Timestamp).FirstOrDefault();
                if (latest == null)
                {
                    return null;
                }

                return new ChatMessageRecord
                {
                    SenderId = latest.SenderId,
                    RecipientId = latest.RecipientId,
                    Text = latest.Text,
                    Timestamp = latest.Timestamp
                };
            }
        }

        public static IEnumerable<int> GetContactsForUser(int userId)
        {
            lock (SyncRoot)
            {
                return Conversations.Keys
                    .SelectMany(key => ParseParticipants(key)
                        .Contains(userId)
                        ? ParseParticipants(key).Where(id => id != userId)
                        : Enumerable.Empty<int>())
                    .Distinct()
                    .ToList();
            }
        }

        private static string BuildKey(int firstUserId, int secondUserId)
        {
            int lowerId = Math.Min(firstUserId, secondUserId);
            int higherId = Math.Max(firstUserId, secondUserId);
            return $"{lowerId}:{higherId}";
        }

        private static IEnumerable<int> ParseParticipants(string key)
        {
            string[] parts = key.Split(':');
            foreach (string part in parts)
            {
                if (int.TryParse(part, out int userId))
                {
                    yield return userId;
                }
            }
        }
    }
}
