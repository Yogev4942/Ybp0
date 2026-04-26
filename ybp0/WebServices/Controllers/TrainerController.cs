using System;
using Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ViewModels.Services;

namespace WebServices.Controllers
{
    public class TrainerController : ApiController
    {
        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        [HttpPost]
        [Route("api/user/TrainerLogin")]
        public IHttpActionResult Post([FromBody] User request)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Username and password must be provided.");

            DatabaseService dbService = new DatabaseService();
            Trainer Trainer = dbService.ValidateLogin(request.Username, request.Password);

            // For now, just return success and echo the username (never echo the password).
            // Validate the Response (Authentication Result)
            if (Trainer == null)
            {
                // Use Unauthorized for failed logins to keep the reason specific but secure
                return Content(HttpStatusCode.Unauthorized, "Invalid username or password.");
            }

            // Return Success
            // Ensure the Teacher object doesn't include the password hash when serialized!
            return Ok(teacher);
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}