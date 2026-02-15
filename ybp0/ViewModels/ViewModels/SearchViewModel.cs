using Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// ViewModel for the Search view. Allows trainees to search for trainers.
    /// </summary>
    public class SearchViewModel : BaseViewModel
    {
        private readonly IDatabaseService _database;
        private readonly INavigationService _navigation;
        private readonly User _activeUser;

        private string _searchQuery;
        public string SearchQuery
        {
            get => _searchQuery;
            set => SetProperty(ref _searchQuery, value);
        }

        private ObservableCollection<TrainerCardViewModel> _searchResults;
        public ObservableCollection<TrainerCardViewModel> SearchResults
        {
            get => _searchResults;
            set => SetProperty(ref _searchResults, value);
        }

        private bool _hasSearched;
        public bool HasSearched
        {
            get => _hasSearched;
            set => SetProperty(ref _hasSearched, value);
        }

        private bool _hasResults;
        public bool HasResults
        {
            get => _hasResults;
            set => SetProperty(ref _hasResults, value);
        }

        public ICommand SearchCommand { get; }

        public SearchViewModel(IDatabaseService database, INavigationService navigation, User activeUser)
        {
            _database = database;
            _navigation = navigation;
            _activeUser = activeUser;
            SearchResults = new ObservableCollection<TrainerCardViewModel>();
            SearchCommand = new RelayCommand(_ => PerformSearch());

            // Load all trainers on initial load
            PerformSearch();
        }

        private void PerformSearch()
        {
            var trainers = _database.SearchTrainers(SearchQuery);
            SearchResults.Clear();

            foreach (var trainer in trainers)
            {
                SearchResults.Add(new TrainerCardViewModel(trainer, _navigation));
            }

            HasSearched = true;
            HasResults = SearchResults.Count > 0;
        }
    }

    /// <summary>
    /// Lightweight card ViewModel for displaying a trainer in search results.
    /// </summary>
    public class TrainerCardViewModel : BaseViewModel
    {
        private readonly INavigationService _navigation;

        public int TrainerId { get; }
        public string Username { get; }
        public string Specialization { get; }
        public double Rating { get; }
        public string RatingDisplay { get; }
        public string TraineesDisplay { get; }
        public string HourlyRateDisplay { get; }
        public bool IsAccepting { get; }
        public string AvatarColor { get; }

        public string Initials => !string.IsNullOrEmpty(Username) && Username.Length >= 2
            ? Username.Substring(0, 2).ToUpper()
            : Username?.FirstOrDefault().ToString().ToUpper();

        public ICommand ViewProfileCommand { get; }

        public TrainerCardViewModel(Trainer trainer, INavigationService navigation)
        {
            _navigation = navigation;
            TrainerId = trainer.Id;
            Username = trainer.Username;
            Specialization = trainer.Specialization ?? "General";
            Rating = trainer.Rating;
            RatingDisplay = trainer.Rating > 0 ? $"{trainer.Rating:F1} ⭐" : "New";
            TraineesDisplay = $"{trainer.TotalTrainees}/{trainer.MaxTrainees}";
            HourlyRateDisplay = $"${trainer.HourlyRate}/hr";
            IsAccepting = trainer.CanAcceptMoreTrainees;
            AvatarColor = GetColorForUser(Username);

            ViewProfileCommand = new RelayCommand(_ => _navigation.NavigateToProfile(TrainerId));
        }

        private static string GetColorForUser(string username)
        {
            if (string.IsNullOrEmpty(username)) return "#FF00BCD4";
            int hash = username.GetHashCode();
            var colors = new[] { "#FF00BCD4", "#FF9C27B0", "#FF4CAF50", "#FFFF9800", "#FF3F51B5", "#FFE91E63" };
            return colors[Math.Abs(hash) % colors.Length];
        }
    }
}
