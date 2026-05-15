using Models;
using System;
using System.Net;
using System.Web.Http;
using ViewModels;
using ViewModels.Services;

namespace WebServices.Controllers
{
    public class UserController : ApiController
    {
        private readonly IDatabaseService _databaseService;

        public UserController() : this(new DatabaseService())
        {
        }

        public UserController(IDatabaseService databaseService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        }

        [HttpPut]
        [Route("api/user/profile/{userId:int}")]
        public IHttpActionResult UpdateProfile(int userId, [FromBody] UpdateProfileRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is required.");
            }

            User user = _databaseService.GetUserById(userId);
            if (user == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(request.Username))
            {
                return BadRequest("Username is required.");
            }

            user.Username = request.Username.Trim();
            user.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
            user.Bio = request.Bio;

            if (user is Trainee trainee)
            {
                trainee.FitnessGoal = request.FitnessGoal;
                trainee.CurrentWeight = request.CurrentWeight ?? trainee.CurrentWeight;
                trainee.Height = request.Height ?? trainee.Height;
            }
            else if (user is Trainer trainer)
            {
                trainer.Specialization = request.Specialization;
                trainer.HourlyRate = request.HourlyRate ?? trainer.HourlyRate;
                trainer.MaxTrainees = request.MaxTrainees ?? trainer.MaxTrainees;
            }

            if (!_databaseService.UpdateUser(user))
            {
                return Content(HttpStatusCode.InternalServerError, "Profile could not be updated.");
            }

            user.Password = null;
            user.PasswordSalt = null;
            return Ok(user);
        }
    }

    public class UpdateProfileRequest
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Bio { get; set; }
        public string FitnessGoal { get; set; }
        public double? CurrentWeight { get; set; } = null;
        public double? Height { get; set; }
        public string Specialization { get; set; }
        public double? HourlyRate { get; set; }
        public int? MaxTrainees { get; set; }
    }
}
