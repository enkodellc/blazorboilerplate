using BlazorBoilerplate.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorBoilerplate.Server.Models;
using Microsoft.Extensions.Logging;
using BlazorBoilerplate.Server.Services;
using Newtonsoft.Json;
using System.Net.Http;

namespace BlazorBoilerplate.Server.Controllers
{
    [Route("api/[controller]")]
    // [Authorize(Roles = "Admin")]
    [ApiController]
    public class SampleDataController : ControllerBase
    {
        // Logger instance
        ILogger<SampleDataController> _logger;

        public SampleDataController(ILogger<SampleDataController> logger)
        {
            _logger = logger;
        }

        private static string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet("[action]")]
        [Authorize(Roles = "SuperAdmin, Admin, User")]
        public IEnumerable<WeatherForecast> WeatherForecasts()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date         = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary      = Summaries[rng.Next(Summaries.Length)]
            });
        }

        [HttpGet("IsAdmin")]
        [Authorize(Roles = "Admin")]
        public IActionResult IsAdmin()
        {
            return Ok(new {UserInRole = "Admin" });
        }

        [HttpGet("IsUser")]
        [Authorize(Roles = "IsUser")]
        public IActionResult IsUser()
        {
            return Ok(new { UserInRole = "User" });
        }

        [HttpGet("IsReadOnly")]
        [Authorize(Policy = "ReadOnly")]
        public IActionResult IsReadOnly()
        {
            return Ok(new {policy = "ReadOnly" });
        }


        //For testing Admin UI
        [HttpGet("[action]")]
        [Authorize(Roles = "SuperAdmin, Admin, User")]
        public async Task<IEnumerable<DemoUser>> GetDemoUsers()
        {
            using (var client = new HttpClient())
            {
                string content = await client.GetStringAsync("https://blazorboilerplate.com/users.json");
                IEnumerable<DemoUser> users = JsonConvert.DeserializeObject<IEnumerable<DemoUser>>(content);
                return users;
            }
        }
    }
}
