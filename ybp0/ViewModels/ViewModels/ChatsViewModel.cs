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
        private int? _preferredChatUserId;

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
            _preferredChatUserId = initialChatUser?.Id;

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

            int? selectedChatUserId = _preferredChatUserId ?? SelectedChat?.UserId ?? _initialChatUser?.Id;

            List<User> contacts = ResolveContacts(selectedChatUserId)
                .Where(user => user != null && user.Id != _currentUser.Id)
                .GroupBy(user => user.Id)
                .Select(group => group.First())
                .ToList();
            Dictionary<int, Message> latestMessages = _dbService.GetLatestMessagesByContacts(
                _currentUser.Id,
                contacts.Select(user => user.Id));

            Chats.Clear();

            foreach (ChatPreviewItemViewModel chat in contacts
                .Select(user => CreateChatPreview(user, latestMessages))
                .OrderByDescending(item => item.LastActivity ?? DateTime.MinValue)
                .ThenBy(item => item.Name))
            {
                Chats.Add(chat);
            }

            SelectedChat = selectedChatUserId.HasValue
                ? Chats.FirstOrDefault(chat => chat.UserId == selectedChatUserId.Value) ?? Chats.FirstOrDefault()
                : Chats.FirstOrDefault();

            _preferredChatUserId = SelectedChat?.UserId ?? selectedChatUserId;
            _initialChatUser = null;

            if (SelectedChat == null)
            {
                Messages.Clear();
            }
        }

        private List<User> ResolveContacts(int? selectedChatUserId)
        {
            var contactIds = new HashSet<int>();
            var contacts = new List<User>();

            if (_currentUser.IsTrainer)
            {
                foreach (Trainee trainee in _dbService.GetTraineesByTrainerId(_currentUser.Id))
                {
                    contactIds.Add(trainee.Id);
                    contacts.Add(trainee);
                }
            }
            else
            {
                Trainee activeTrainee = _dbService.GetUserById(_currentUser.Id) as Trainee;
                if (activeTrainee != null && activeTrainee.TrainerId.HasValue)
                {
                    Trainer assignedTrainer = _dbService.SearchTrainers(string.Empty)
                        .FirstOrDefault(trainer => trainer.TrainerProfileId == activeTrainee.TrainerId.Value);

                    if (assignedTrainer != null)
                    {
                        contactIds.Add(assignedTrainer.Id);
                        contacts.Add(assignedTrainer);
                    }
                }
            }

            foreach (int contactId in _dbService.GetChatContactIds(_currentUser.Id))
            {
                contactIds.Add(contactId);
            }

            if (selectedChatUserId.HasValue)
            {
                contactIds.Add(selectedChatUserId.Value);
            }

            Dictionary<int, User> extraUsers = _dbService.GetUsersByIds(contactIds);
            foreach (KeyValuePair<int, User> pair in extraUsers)
            {
                if (pair.Value != null && !contacts.Any(user => user.Id == pair.Key))
                {
                    contacts.Add(pair.Value);
                }
            }

            return contacts;
        }

        private ChatPreviewItemViewModel CreateChatPreview(User user, Dictionary<int, Message> latestMessages)
        {
            latestMessages.TryGetValue(user.Id, out Message latestMessage);
            return new ChatPreviewItemViewModel
            {
                UserId = user.Id,
                Name = user.Username,
                LastMessage = latestMessage != null ? latestMessage.MessageText : "No messages yet",
                LastActivity = latestMessage?.SentAt,
                AvatarColor = GetColorForUser(user.Username)
            };
        }

        private void SelectChat(ChatPreviewItemViewModel chat)
        {
            if (chat != null)
            {
                _preferredChatUserId = chat.UserId;
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

            foreach (Message msg in _dbService.GetConversation(_currentUser.Id, SelectedChat.UserId))
            {
                Messages.Add(new ChatMessageItemViewModel
                {
                    SenderId = msg.SenderId,
                    Text = msg.MessageText,
                    Timestamp = msg.SentAt,
                    IsSentByMe = msg.SenderId == _currentUser.Id
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
            _dbService.SendMessage(_currentUser.Id, selectedChatUserId, text);
            NewMessage = string.Empty;

            _preferredChatUserId = selectedChatUserId;
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
        public bool IsSentByMe { get; set; }

        public string FormattedTime => Timestamp.ToString("HH:mm");
        public string SenderLabel => IsSentByMe ? "You" : "";
    }
}
