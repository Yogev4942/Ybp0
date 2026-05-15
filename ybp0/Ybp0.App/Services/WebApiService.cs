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

    public async Task<UserDto> UpdateProfileAsync(UpdateProfileRequest request)
    {
        UserDto updated = await GenericApiClient.PutAsync<UpdateProfileRequest, UserDto>($"user/profile/{request.Id}", request)
            ?? throw new InvalidOperationException("The API did not return the updated user.");

        CurrentUser = updated;
        return updated;
    }

    public async Task<IReadOnlyList<WorkoutPlanDto>> GetWorkoutPlansAsync()
    {
        EnsureSignedIn();
        return await GenericApiClient.GetAsync<IReadOnlyList<WorkoutPlanDto>>($"workoutplans/user/{CurrentUser!.Id}")
            ?? Array.Empty<WorkoutPlanDto>();
    }

    public async Task<WorkoutPlanDto> CreateWorkoutPlanAsync(string workoutName)
    {
        EnsureSignedIn();
        CreateWorkoutPlanRequest request = new(CurrentUser!.Id, workoutName);
        return await GenericApiClient.PostAsync<CreateWorkoutPlanRequest, WorkoutPlanDto>("workoutplans", request)
            ?? throw new InvalidOperationException("The API did not return the created workout plan.");
    }

    public async Task<WorkoutPlanDto> RenameWorkoutPlanAsync(int workoutId, string workoutName)
    {
        RenameWorkoutPlanRequest request = new(workoutName);
        return await GenericApiClient.PutAsync<RenameWorkoutPlanRequest, WorkoutPlanDto>($"workoutplans/{workoutId}/name", request)
            ?? throw new InvalidOperationException("The API did not return the renamed workout plan.");
    }

    public async Task<WorkoutPlanExerciseDto> AddExerciseToWorkoutPlanAsync(int workoutId, int exerciseId)
    {
        AddWorkoutExerciseRequest request = new(exerciseId);
        return await GenericApiClient.PostAsync<AddWorkoutExerciseRequest, WorkoutPlanExerciseDto>($"workoutplans/{workoutId}/exercises", request)
            ?? throw new InvalidOperationException("The API did not return the added exercise.");
    }

    public Task DeleteWorkoutPlanExerciseAsync(int workoutExerciseId)
    {
        return GenericApiClient.DeleteAsync($"workoutplans/exercises/{workoutExerciseId}");
    }

    public async Task<WorkoutSetDto> AddWorkoutPlanSetAsync(int workoutExerciseId, int setNumber, int reps, double weight)
    {
        SaveWorkoutSetRequest request = new(setNumber, reps, weight);
        return await GenericApiClient.PostAsync<SaveWorkoutSetRequest, WorkoutSetDto>($"workoutplans/exercises/{workoutExerciseId}/sets", request)
            ?? throw new InvalidOperationException("The API did not return the added set.");
    }

    public Task DeleteWorkoutPlanSetAsync(int setId)
    {
        return GenericApiClient.DeleteAsync($"workoutplans/sets/{setId}");
    }

    public async Task<IReadOnlyList<ExerciseDto>> GetExercisesAsync()
    {
        return await GenericApiClient.GetAsync<IReadOnlyList<ExerciseDto>>("exercises")
            ?? Array.Empty<ExerciseDto>();
    }

    public void SignOut()
    {
        CurrentUser = null;
    }

    private void EnsureSignedIn()
    {
        if (CurrentUser == null)
        {
            throw new InvalidOperationException("Sign in before loading workout plans.");
        }
    }
}
