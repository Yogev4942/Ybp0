using Models;
using System;
using System.Net;
using System.Web.Http;
using ViewModels;
using ViewModels.Services;

namespace WebServices.Controllers
{
    public class TraineeController : ApiController
    {
        private readonly IDatabaseService _databaseService;

        public TraineeController() : this(new DatabaseService())
        {
        }

        public TraineeController(IDatabaseService databaseService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        }

        [HttpPost]
        [Route("api/user/TraineeLogin")]
        public IHttpActionResult Login([FromBody] TraineeLoginRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Username and password must be provided.");
            }

            User user = _databaseService.AuthenticateUser(request.Username.Trim(), request.Password);
            Trainee trainee = user as Trainee;

            if (trainee == null)
            {
                return Content(HttpStatusCode.Unauthorized, "Invalid trainee username or password.");
            }

            trainee.Password = null;
            return Ok(trainee);
        }
    }

    public class TraineeLoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
