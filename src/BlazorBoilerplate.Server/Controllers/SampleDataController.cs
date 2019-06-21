using BlazorBoilerplate.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Server.Controllers
{
    [Route("api/[controller]")]
    // [Authorize(Roles = "Admin")]
    [ApiController]
    public class SampleDataController : ControllerBase
    {
        private static string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet("[action]")]
        [Authorize(Policy = "hans")]
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

        [HttpGet("blazor")]
        [Authorize(Policy = "blazor")]
        public IActionResult Blazor()
        {
            return Ok(new {policy = "blazor"});
        }

        [HttpGet("hans")]
        [Authorize(Policy = "hans")]
        public IActionResult Hans()
        {
            return Ok(new {policy = "hans"});
        }

        [HttpGet("hallo")]
        [Authorize(Policy = "hallo")]
        public IActionResult Hallo()
        {
            return Ok(new {policy = "hallo"});
        }
    }
}