using Models;
using System;
using System.Collections.Generic;

namespace ViewModels.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        private readonly User _activeUser;
        private readonly IDatabaseService _database;
        private readonly INavigationService _navigation;

        private string _welcomeMessage;
        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set => SetProperty(ref _welcomeMessage, value);
        }

        public string UserName => _activeUser?.Username ?? "Guest";

        private static readonly Random _random = new Random();

        public HomeViewModel(IDatabaseService database, INavigationService navigation, User user)
        {
            _activeUser = user;
            _navigation = navigation;
            _database = database;

            WelcomeMessage = GenerateWelcomeMessage();
        }

        private string GenerateWelcomeMessage()
        {
            string role = _activeUser is Trainer ? "Trainer" : "Trainee";

            var messages = _activeUser.IsTrainer
                ? new List<string>
                {
                    $"Welcome back, Coach {UserName}! 💪",
                    $"Ready to lead the grind, Boss? 🏋️",
                    $"Good to see you again, Chief. Time to build legends 🔥",
                    $"Back in command, {UserName}. Let’s make gains 📈",
                    $"The gym missed you, Coach {UserName} 💥"
                }
                : new List<string>
                {
                    $"Welcome back, {UserName}! 💪",
                    $"How are you, Chief? Let’s train 🏋️",
                    $"Ready to crush it today, {UserName}? 🔥",
                    $"Back at it again, Boss 💥",
                    $"Another day, another gain — let’s go {UserName} 🚀"
                };

            return messages[_random.Next(messages.Count)];
        }
    }
}
