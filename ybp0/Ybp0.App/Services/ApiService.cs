namespace Ybp0.App.Services;

public class ApiService : IApiService
{
    public UserDto? CurrentUser { get; private set; }

    public async Task<UserDto> LoginAsync(string username, string password)
    {
        UserDto? user = await GenericApiClient.PostAsync<LoginRequest, UserDto>(
            "auth/login",
            new LoginRequest(username, password));

        CurrentUser = user ?? throw new InvalidOperationException("The server returned an empty login response.");
        return CurrentUser;
    }

    public async Task<UserDto> RegisterTraineeAsync(string username, string email, string password, string fitnessGoal, double currentWeight, double height)
    {
        var request = new RegisterUserRequest(
            username,
            email,
            password,
            "trainee",
            null,
            null,
            fitnessGoal,
            currentWeight,
            height,
            null,
            null,
            null,
            null);

        UserDto? user = await GenericApiClient.PostAsync<RegisterUserRequest, UserDto>("auth/register", request);
        CurrentUser = user ?? throw new InvalidOperationException("The server returned an empty register response.");
        return CurrentUser;
    }

    public async Task<UserDto> RegisterTrainerAsync(string username, string email, string password, string specialization, double hourlyRate, int maxTrainees)
    {
        var request = new RegisterUserRequest(
            username,
            email,
            password,
            "trainer",
            null,
            null,
            null,
            null,
            null,
            null,
            specialization,
            hourlyRate,
            maxTrainees);

        UserDto? user = await GenericApiClient.PostAsync<RegisterUserRequest, UserDto>("auth/register", request);
        CurrentUser = user ?? throw new InvalidOperationException("The server returned an empty register response.");
        return CurrentUser;
    }

    public async Task<IReadOnlyList<TrainerDto>> GetTrainersAsync()
    {
        IReadOnlyList<TrainerDto>? trainers = await GenericApiClient.GetAsync<IReadOnlyList<TrainerDto>>("trainers");
        return trainers ?? Array.Empty<TrainerDto>();
    }

    public Task<UserDto?> GetUserAsync(int id)
    {
        return GenericApiClient.GetAsync<UserDto>($"users/{id}");
    }

    public void SignOut()
    {
        CurrentUser = null;
    }
}
