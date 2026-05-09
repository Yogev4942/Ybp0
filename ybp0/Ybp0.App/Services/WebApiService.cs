namespace Ybp0.App.Services;

public class WebApiService : IApiService
{
    public UserDto? CurrentUser { get; private set; }

    public async Task<UserDto> LoginAsync(string username, string password)
    {
        LoginRequest request = new(username, password);
        UserDto? user = await TryLoginAsync("user/TraineeLogin", request)
            ?? await TryLoginAsync("user/TrainerLogin", request);

        CurrentUser = user ?? throw new InvalidOperationException("Invalid username or password.");
        return CurrentUser;
    }

    private static async Task<UserDto?> TryLoginAsync(string path, LoginRequest request)
    {
        try
        {
            return await GenericApiClient.PostAsync<LoginRequest, UserDto>(path, request);
        }
        catch (HttpRequestException exception) when (
            exception.StatusCode == System.Net.HttpStatusCode.BadRequest ||
            exception.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return null;
        }
    }

    public async Task<UserDto> RegisterTraineeAsync(string username, string email, string password, string fitnessGoal, double currentWeight, double height)
    {
        await Task.CompletedTask;
        throw new NotSupportedException("Register is not available in this WebServices project.");
    }

    public async Task<UserDto> RegisterTrainerAsync(string username, string email, string password, string specialization, double hourlyRate, int maxTrainees)
    {
        await Task.CompletedTask;
        throw new NotSupportedException("Register is not available in this WebServices project.");
    }

    public async Task<IReadOnlyList<TrainerDto>> GetTrainersAsync()
    {
        await Task.CompletedTask;
        return Array.Empty<TrainerDto>();
    }

    public async Task<UserDto?> GetUserAsync(int id)
    {
        await Task.CompletedTask;
        return CurrentUser?.Id == id ? CurrentUser : null;
    }

    public void SignOut()
    {
        CurrentUser = null;
    }
}
