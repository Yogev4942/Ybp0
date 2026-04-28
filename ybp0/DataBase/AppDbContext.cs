using Microsoft.EntityFrameworkCore;
using Models;

namespace DataBase;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Trainer> Trainers => Set<Trainer>();
    public DbSet<Trainee> Trainees => Set<Trainee>();
    public DbSet<Muscle> Muscles => Set<Muscle>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<Workout> Workouts => Set<Workout>();
    public DbSet<WorkoutExercise> WorkoutExercises => Set<WorkoutExercise>();
    public DbSet<WorkoutSet> WorkoutSets => Set<WorkoutSet>();
    public DbSet<WorkoutSession> WorkoutSessions => Set<WorkoutSession>();
    public DbSet<WorkoutSessionExercise> WorkoutSessionExercises => Set<WorkoutSessionExercise>();
    public DbSet<WorkoutSessionSet> WorkoutSessionSets => Set<WorkoutSessionSet>();
    public DbSet<WeekPlan> WeekPlans => Set<WeekPlan>();
    public DbSet<WeekPlanDay> WeekPlanDays => Set<WeekPlanDay>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.UseTptMappingStrategy();
            entity.ToTable("Users");
            entity.Ignore(user => user.Joindate);
            entity.Property(user => user.Username).IsRequired();
            entity.Property(user => user.Password).IsRequired();
            entity.HasIndex(user => user.Username).IsUnique();
            entity.HasIndex(user => user.Email).IsUnique();
        });

        modelBuilder.Entity<Trainer>(entity =>
        {
            entity.ToTable("Trainers");
            entity.Property(trainer => trainer.TrainerProfileId).IsRequired();
            entity.HasIndex(trainer => trainer.TrainerProfileId).IsUnique();
        });

        modelBuilder.Entity<Trainee>(entity =>
        {
            entity.ToTable("Trainees");
            entity.Ignore(trainee => trainee.BMI);
            entity.Property(trainee => trainee.TraineeProfileId).IsRequired();
            entity.HasIndex(trainee => trainee.TraineeProfileId).IsUnique();
            entity.Property<int?>("AssignedTrainerUserId");
            entity.HasOne(trainee => trainee.AssignedTrainer)
                .WithMany(trainer => trainer.AssignedTrainees)
                .HasForeignKey("AssignedTrainerUserId")
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Muscle>(entity =>
        {
            entity.ToTable("Muscles");
            entity.Property(muscle => muscle.MuscleName).IsRequired();
            entity.Property(muscle => muscle.BodyMapKey).IsRequired();
            entity.HasIndex(muscle => muscle.MuscleName).IsUnique();
            entity.HasData(
                new Muscle { Id = 1, MuscleName = "Chest", BodyRegion = "Upper Body", DiagramZone = 1, BodyMapKey = "chest" },
                new Muscle { Id = 2, MuscleName = "Back", BodyRegion = "Upper Body", DiagramZone = 2, BodyMapKey = "back" },
                new Muscle { Id = 3, MuscleName = "Shoulders", BodyRegion = "Upper Body", DiagramZone = 3, BodyMapKey = "shoulders" },
                new Muscle { Id = 4, MuscleName = "Biceps", BodyRegion = "Upper Body", DiagramZone = 4, BodyMapKey = "biceps" },
                new Muscle { Id = 5, MuscleName = "Triceps", BodyRegion = "Upper Body", DiagramZone = 5, BodyMapKey = "triceps" },
                new Muscle { Id = 6, MuscleName = "Legs", BodyRegion = "Lower Body", DiagramZone = 6, BodyMapKey = "legs" },
                new Muscle { Id = 7, MuscleName = "Core", BodyRegion = "Midsection", DiagramZone = 7, BodyMapKey = "core" });
        });

        modelBuilder.Entity<Exercise>(entity =>
        {
            entity.ToTable("Exercises");
            entity.Ignore(exercise => exercise.MuscleId);
            entity.Property(exercise => exercise.ExerciseName).IsRequired();
            entity.HasOne(exercise => exercise.PrimaryMuscle)
                .WithMany(muscle => muscle.PrimaryExercises)
                .HasForeignKey(exercise => exercise.PrimaryMuscleId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(exercise => exercise.SecondaryMuscle)
                .WithMany(muscle => muscle.SecondaryExercises)
                .HasForeignKey(exercise => exercise.SecondaryMuscleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Workout>(entity =>
        {
            entity.ToTable("Workouts");
            entity.Property(workout => workout.WorkoutName).IsRequired();
            entity.HasOne(workout => workout.User)
                .WithMany(user => user.Workouts)
                .HasForeignKey(workout => workout.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WorkoutExercise>(entity =>
        {
            entity.ToTable("WorkoutExercises");
            entity.HasOne(workoutExercise => workoutExercise.Workout)
                .WithMany(workout => workout.WorkoutExercises)
                .HasForeignKey(workoutExercise => workoutExercise.WorkoutId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(workoutExercise => workoutExercise.Exercise)
                .WithMany(exercise => exercise.WorkoutExercises)
                .HasForeignKey(workoutExercise => workoutExercise.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WorkoutSet>(entity =>
        {
            entity.ToTable("WorkoutSets");
            entity.HasOne(workoutSet => workoutSet.WorkoutExercise)
                .WithMany(workoutExercise => workoutExercise.Sets)
                .HasForeignKey(workoutSet => workoutSet.WorkoutExerciseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WorkoutSession>(entity =>
        {
            entity.ToTable("WorkoutSessions");
            entity.Ignore(session => session.Exercises);
            entity.Property(session => session.Mode).HasConversion<string>();
            entity.HasOne(session => session.User)
                .WithMany(user => user.WorkoutSessions)
                .HasForeignKey(session => session.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(session => session.Workout)
                .WithMany(workout => workout.WorkoutSessions)
                .HasForeignKey(session => session.WorkoutId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(session => session.WeekPlanDay)
                .WithMany(day => day.WorkoutSessions)
                .HasForeignKey(session => session.WeekPlanDayId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<WorkoutSessionExercise>(entity =>
        {
            entity.HasNoKey();
            entity.ToView(null);
        });

        modelBuilder.Entity<WorkoutSessionSet>(entity =>
        {
            entity.ToTable("WorkoutSessionSets");
            entity.HasOne(sessionSet => sessionSet.WorkoutSession)
                .WithMany(session => session.SessionSets)
                .HasForeignKey(sessionSet => sessionSet.WorkoutSessionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(sessionSet => sessionSet.Exercise)
                .WithMany(exercise => exercise.WorkoutSessionSets)
                .HasForeignKey(sessionSet => sessionSet.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WeekPlan>(entity =>
        {
            entity.ToTable("WeekPlans");
            entity.Property(plan => plan.PlanName).IsRequired();
            entity.HasOne(plan => plan.User)
                .WithMany(user => user.WeekPlans)
                .HasForeignKey(plan => plan.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WeekPlanDay>(entity =>
        {
            entity.ToTable("WeekPlanDays");
            entity.HasOne(day => day.WeekPlan)
                .WithMany(plan => plan.Days)
                .HasForeignKey(day => day.WeekPlanId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(day => day.Workout)
                .WithMany()
                .HasForeignKey(day => day.WorkoutId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.ToTable("Posts");
            entity.HasOne(post => post.Owner)
                .WithMany()
                .HasForeignKey(post => post.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("Messages");
            entity.Property(message => message.MessageText).IsRequired();
            entity.HasOne(message => message.Sender)
                .WithMany()
                .HasForeignKey(message => message.SenderId)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(message => message.Recipient)
                .WithMany()
                .HasForeignKey(message => message.RecipientId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}
