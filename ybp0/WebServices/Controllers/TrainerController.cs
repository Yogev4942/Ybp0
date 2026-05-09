using Models;
using System;
using System.Net;
using System.Web.Http;
using ViewModels;
using ViewModels.Services;

namespace WebServices.Controllers
{
    public class TrainerController : ApiController
    {
        private readonly IDatabaseService _databaseService;

        public TrainerController() : this(new DatabaseService())
        {
        }

        public TrainerController(IDatabaseService databaseService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        }

        [HttpPost]
        [Route("api/user/TrainerLogin")]
        public IHttpActionResult Login([FromBody] TrainerLoginRequest request)
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
            Trainer trainer = user as Trainer;

            if (trainer == null)
            {
                return Content(HttpStatusCode.Unauthorized, "Invalid trainer username or password.");
            }

            trainer.Password = null;
            return Ok(trainer);
        }
    }

    public class TrainerLoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
