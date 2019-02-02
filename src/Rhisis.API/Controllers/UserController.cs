﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rhisis.Core.Models;
using Rhisis.Core.Services;

namespace Rhisis.API.Controllers
{
    /// <summary>
    /// Provides API routes to manage users.
    /// </summary>
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userService;

        /// <summary>
        /// Creates a new <see cref="UserController"/> instance.
        /// </summary>
        /// <param name="logger">Logger</param>
        public UserController(ILogger<UserController> logger, IUserService userService)
        {
            this._logger = logger;
            this._userService = userService;
        }
        
        /// <summary>
        /// Registers an user.
        /// </summary>
        /// <param name="registerModel"></param>
        /// <returns></returns>
        [HttpPost("register")]
        [AllowAnonymous]
        public IActionResult RegisterUser([FromBody] UserRegisterModel registerModel)
        {
            this._logger.LogInformation("An unknown user want to register a new account.");
            this._userService.CreateUser(registerModel);
            this._logger.LogInformation($"User {registerModel.Username} has been created.");

            return Ok();
        }
    }
}
