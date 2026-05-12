using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using ViewModels;
using ViewModels.Services;

namespace WebServices.Controllers
{
    public class ExercisesController : ApiController
    {
        private readonly IDatabaseService _databaseService;

        public ExercisesController() : this(new DatabaseService())
        {
        }

        public ExercisesController(IDatabaseService databaseService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        }

        [HttpGet]
        [Route("api/exercises")]
        public IHttpActionResult Get()
        {
            List<ExerciseDto> exercises = _databaseService.GetAllExercises()
                .OrderBy(exercise => exercise.ExerciseName)
                .Select(MapExercise)
                .ToList();

            return Ok(exercises);
        }

        private static ExerciseDto MapExercise(Exercise exercise)
        {
            return new ExerciseDto
            {
                Id = exercise.Id,
                ExerciseName = exercise.ExerciseName,
                MuscleGroup = exercise.MuscleGroup,
                SecondaryMuscleGroup = exercise.SecondaryMuscleGroup
            };
        }
    }

    public class ExerciseDto
    {
        public int Id { get; set; }
        public string ExerciseName { get; set; }
        public string MuscleGroup { get; set; }
        public string SecondaryMuscleGroup { get; set; }
    }
}
