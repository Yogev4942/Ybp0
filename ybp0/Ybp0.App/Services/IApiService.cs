namespace Ybp0.App.Services;

public interface IApiService
{
    UserDto? CurrentUser { get; }
    Task<UserDto> LoginAsync(string username, string password);
    Task<UserDto> RegisterTraineeAsync(string username, string email, string password, string fitnessGoal, double currentWeight, double height);
    Task<UserDto> RegisterTrainerAsync(string username, string email, string password, string specialization, double hourlyRate, int maxTrainees);
    Task<IReadOnlyList<TrainerDto>> GetTrainersAsync();
    Task<UserDto?> GetUserAsync(int id);
    void SignOut();
}
