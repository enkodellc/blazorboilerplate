using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using BlazorBoilerplate.Shared;
using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Server.Middleware.Wrappers;

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
        public IEnumerable<WeatherForecastDto> WeatherForecasts()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecastDto
            {
                Date         = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary      = Summaries[rng.Next(Summaries.Length)]
            });
        }

        //For testing Admin UI
        [HttpGet("[action]")]
        [Authorize(Roles = "SuperAdmin, Admin, User")]
        public async Task<ApiResponse> GetDemoUsers()
        {
            using (var client = new HttpClient())
            {
                string content = await client.GetStringAsync("https://blazorboilerplate.com/users.json");
                IEnumerable<DemoUserDto> users = JsonConvert.DeserializeObject<IEnumerable<DemoUserDto>>(content);
                return new ApiResponse(200, "Retrieved Demo Users", users);
            }
        }
    }
}
