using Models;
using ViewModels.Api;

namespace WebServices.Controllers;

internal static class ApiMappings
{
    public static UserViewModel ToUserViewModel(User user)
    {
        Trainer? trainer = user as Trainer;
        Trainee? trainee = user as Trainee;

        return new UserViewModel(
            user.Id,
            user.Username,
            user.Email,
            user.JoinDate,
            user.Bio,
            user.Gender,
            user.IsTrainer,
            user.IsAdmin,
            user.CurrentWeekPlanId,
            trainer?.TrainerProfileId,
            trainee?.TraineeProfileId,
            trainee?.TrainerId,
            trainee?.FitnessGoal,
            trainee?.CurrentWeight,
            trainee?.Height,
            trainer?.Specialization,
            trainer?.HourlyRate,
            trainer?.MaxTrainees,
            trainer?.TotalTrainees,
            trainer?.Rating,
            trainer?.TotalRatings);
    }

    public static TrainerViewModel ToTrainerViewModel(Trainer trainer)
    {
        return new TrainerViewModel(
            trainer.Id,
            trainer.TrainerProfileId,
            trainer.Username,
            trainer.Email,
            trainer.JoinDate,
            trainer.Bio,
            trainer.Gender,
            trainer.Specialization,
            trainer.HourlyRate,
            trainer.MaxTrainees,
            trainer.TotalTrainees,
            trainer.Rating,
            trainer.TotalRatings);
    }

    public static TraineeViewModel ToTraineeViewModel(Trainee trainee)
    {
        return new TraineeViewModel(
            trainee.Id,
            trainee.TraineeProfileId,
            trainee.Username,
            trainee.Email,
            trainee.JoinDate,
            trainee.Bio,
            trainee.Gender,
            trainee.TrainerId,
            trainee.AssignedTrainer?.Username,
            trainee.FitnessGoal,
            trainee.CurrentWeight,
            trainee.Height,
            trainee.BMI);
    }

    public static MuscleViewModel ToMuscleViewModel(Muscle muscle)
    {
        return new MuscleViewModel(muscle.Id, muscle.MuscleName, muscle.BodyRegion, muscle.DiagramZone, muscle.BodyMapKey);
    }

    public static ExerciseViewModel ToExerciseViewModel(Exercise exercise)
    {
        return new ExerciseViewModel(
            exercise.Id,
            exercise.ExerciseName,
            exercise.PrimaryMuscleId,
            exercise.PrimaryMuscle?.MuscleName,
            exercise.SecondaryMuscleId,
            exercise.SecondaryMuscle?.MuscleName,
            exercise.MuscleGroup ?? exercise.PrimaryMuscle?.MuscleName,
            exercise.SecondaryMuscleGroup ?? exercise.SecondaryMuscle?.MuscleName);
    }

    public static WorkoutViewModel ToWorkoutViewModel(Workout workout)
    {
        return new WorkoutViewModel(
            workout.Id,
            workout.UserId,
            workout.WorkoutName,
            (workout.WorkoutExercises ?? new List<WorkoutExercise>())
                .OrderBy(exercise => exercise.OrderNumber)
                .Select(ToWorkoutExerciseViewModel)
                .ToList());
    }

    public static WorkoutExerciseViewModel ToWorkoutExerciseViewModel(WorkoutExercise workoutExercise)
    {
        return new WorkoutExerciseViewModel(
            workoutExercise.Id,
            workoutExercise.ExerciseId,
            workoutExercise.Exercise?.ExerciseName ?? workoutExercise.ExerciseName,
            workoutExercise.Exercise?.PrimaryMuscle?.MuscleName ?? workoutExercise.MuscleGroup,
            workoutExercise.Exercise?.SecondaryMuscle?.MuscleName ?? workoutExercise.SecondaryMuscleGroup,
            workoutExercise.OrderNumber,
            (workoutExercise.Sets ?? new List<WorkoutSet>())
                .OrderBy(set => set.SetNumber)
                .Select(set => new WorkoutSetViewModel(set.Id, set.SetNumber, set.Reps, set.Weight))
                .ToList());
    }

    public static WorkoutSessionViewModel ToWorkoutSessionViewModel(WorkoutSession session)
    {
        return new WorkoutSessionViewModel(
            session.Id,
            session.UserId,
            session.WorkoutId,
            session.WeekPlanDayId,
            session.SessionDate,
            session.StartTime,
            session.EndTime,
            session.IsCompleted,
            session.Mode.ToString(),
            session.WorkoutName,
            (session.Exercises ?? new List<WorkoutSessionExercise>())
                .Select(ToWorkoutSessionExerciseViewModel)
                .ToList());
    }

    public static WorkoutSessionExerciseViewModel ToWorkoutSessionExerciseViewModel(WorkoutSessionExercise exercise)
    {
        return new WorkoutSessionExerciseViewModel(
            exercise.ExerciseId,
            exercise.Exercise?.ExerciseName ?? exercise.ExerciseName,
            exercise.Exercise?.PrimaryMuscle?.MuscleName ?? exercise.MuscleGroup,
            exercise.Exercise?.SecondaryMuscle?.MuscleName ?? exercise.SecondaryMuscleGroup,
            (exercise.Sets ?? new List<WorkoutSessionSet>())
                .OrderBy(set => set.SetNumber)
                .Select(set => new WorkoutSessionSetViewModel(set.Id, set.ExerciseId, set.SetNumber, set.Reps, set.Weight, set.IsCompleted))
                .ToList());
    }

    public static WeekPlanViewModel ToWeekPlanViewModel(WeekPlan plan)
    {
        return new WeekPlanViewModel(
            plan.Id,
            plan.UserId,
            plan.PlanName,
            (plan.Days ?? new List<WeekPlanDay>())
                .Select(ToWeekPlanDayViewModel)
                .ToList());
    }

    public static WeekPlanDayViewModel ToWeekPlanDayViewModel(WeekPlanDay day)
    {
        return new WeekPlanDayViewModel(
            day.Id,
            day.WeekPlanId,
            day.DayOfWeek,
            day.WorkoutId,
            day.Workout?.WorkoutName ?? day.WorkoutName,
            day.RestDay);
    }

    public static MessageViewModel ToMessageViewModel(Message message)
    {
        return new MessageViewModel(
            message.Id,
            message.SenderId,
            message.Sender?.Username,
            message.RecipientId,
            message.Recipient?.Username,
            message.MessageText,
            message.SentAt);
    }
}
