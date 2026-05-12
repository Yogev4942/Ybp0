using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using ViewModels;
using ViewModels.Services;

namespace WebServices.Controllers
{
    public class WorkoutPlansController : ApiController
    {
        private readonly IDatabaseService _databaseService;

        public WorkoutPlansController() : this(new DatabaseService())
        {
        }

        public WorkoutPlansController(IDatabaseService databaseService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        }

        [HttpGet]
        [Route("api/workoutplans/user/{userId:int}")]
        public IHttpActionResult GetByUser(int userId)
        {
            List<WorkoutPlanDto> plans = _databaseService.GetWorkoutsByUserId(userId)
                .Select(MapWorkout)
                .ToList();

            return Ok(plans);
        }

        [HttpPost]
        [Route("api/workoutplans")]
        public IHttpActionResult Create([FromBody] CreateWorkoutPlanRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is required.");
            }

            if (request.UserId <= 0)
            {
                return BadRequest("A valid user id is required.");
            }

            string workoutName = string.IsNullOrWhiteSpace(request.WorkoutName)
                ? "Workout Plan"
                : request.WorkoutName.Trim();

            int workoutId = _databaseService.CreateWorkout(request.UserId, workoutName);
            Workout workout = _databaseService.GetWorkoutById(workoutId);

            return Ok(MapWorkout(workout));
        }

        [HttpPut]
        [Route("api/workoutplans/{workoutId:int}/name")]
        public IHttpActionResult Rename(int workoutId, [FromBody] RenameWorkoutPlanRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is required.");
            }

            string workoutName = string.IsNullOrWhiteSpace(request.WorkoutName)
                ? "Workout Plan"
                : request.WorkoutName.Trim();

            _databaseService.UpdateWorkoutName(workoutId, workoutName);
            Workout workout = _databaseService.GetWorkoutById(workoutId);

            if (workout == null)
            {
                return NotFound();
            }

            return Ok(MapWorkout(workout));
        }

        [HttpPost]
        [Route("api/workoutplans/{workoutId:int}/exercises")]
        public IHttpActionResult AddExercise(int workoutId, [FromBody] AddWorkoutExerciseRequest request)
        {
            if (request == null || request.ExerciseId <= 0)
            {
                return BadRequest("A valid exercise id is required.");
            }

            WorkoutExercise exercise = _databaseService.AddExerciseToWorkout(workoutId, request.ExerciseId);
            if (exercise == null)
            {
                return NotFound();
            }

            return Ok(MapWorkoutExercise(exercise));
        }

        [HttpDelete]
        [Route("api/workoutplans/exercises/{workoutExerciseId:int}")]
        public IHttpActionResult RemoveExercise(int workoutExerciseId)
        {
            _databaseService.RemoveExerciseFromWorkout(workoutExerciseId);
            return Ok();
        }

        [HttpPost]
        [Route("api/workoutplans/exercises/{workoutExerciseId:int}/sets")]
        public IHttpActionResult SaveSet(int workoutExerciseId, [FromBody] SaveWorkoutSetRequest request)
        {
            if (request == null || request.SetNumber <= 0)
            {
                return BadRequest("A valid set number is required.");
            }

            WorkoutSet set = _databaseService.SaveWorkoutSet(
                workoutExerciseId,
                request.SetNumber,
                Math.Max(0, request.Reps),
                Math.Max(0, request.Weight));

            return Ok(MapWorkoutSet(set));
        }

        [HttpDelete]
        [Route("api/workoutplans/sets/{setId:int}")]
        public IHttpActionResult RemoveSet(int setId)
        {
            _databaseService.DeleteWorkoutSet(setId);
            return Ok();
        }

        private static WorkoutPlanDto MapWorkout(Workout workout)
        {
            return new WorkoutPlanDto
            {
                Id = workout.Id,
                UserId = workout.UserId,
                WorkoutName = workout.WorkoutName,
                WorkoutExercises = (workout.WorkoutExercises ?? new List<WorkoutExercise>())
                    .OrderBy(exercise => exercise.OrderNumber)
                    .Select(MapWorkoutExercise)
                    .ToList()
            };
        }

        private static WorkoutPlanExerciseDto MapWorkoutExercise(WorkoutExercise exercise)
        {
            return new WorkoutPlanExerciseDto
            {
                Id = exercise.Id,
                ExerciseId = exercise.ExerciseId,
                ExerciseName = exercise.ExerciseName,
                MuscleGroup = exercise.MuscleGroup,
                SecondaryMuscleGroup = exercise.SecondaryMuscleGroup,
                OrderNumber = exercise.OrderNumber,
                Sets = (exercise.Sets ?? new List<WorkoutSet>())
                    .OrderBy(set => set.SetNumber)
                    .Select(MapWorkoutSet)
                    .ToList()
            };
        }

        private static WorkoutSetDto MapWorkoutSet(WorkoutSet set)
        {
            return new WorkoutSetDto
            {
                Id = set.Id,
                SetNumber = set.SetNumber,
                Reps = set.Reps,
                Weight = set.Weight
            };
        }
    }

    public class CreateWorkoutPlanRequest
    {
        public int UserId { get; set; }
        public string WorkoutName { get; set; }
    }

    public class RenameWorkoutPlanRequest
    {
        public string WorkoutName { get; set; }
    }

    public class AddWorkoutExerciseRequest
    {
        public int ExerciseId { get; set; }
    }

    public class SaveWorkoutSetRequest
    {
        public int SetNumber { get; set; }
        public int Reps { get; set; }
        public double Weight { get; set; }
    }

    public class WorkoutPlanDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string WorkoutName { get; set; }
        public List<WorkoutPlanExerciseDto> WorkoutExercises { get; set; }
    }

    public class WorkoutPlanExerciseDto
    {
        public int Id { get; set; }
        public int ExerciseId { get; set; }
        public string ExerciseName { get; set; }
        public string MuscleGroup { get; set; }
        public string SecondaryMuscleGroup { get; set; }
        public int OrderNumber { get; set; }
        public List<WorkoutSetDto> Sets { get; set; }
    }

    public class WorkoutSetDto
    {
        public int Id { get; set; }
        public int SetNumber { get; set; }
        public int Reps { get; set; }
        public double Weight { get; set; }
    }
}
