using LearnAI.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.IO;
using System.Collections.Generic;

namespace LearnAI.Web.Controllers
{
    public class CourseController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public CourseController(IWebHostEnvironment env, IConfiguration config)
        {
            _env = env;
            _config = config;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("Course/Week{id:int}")]
        public IActionResult LoadWeek(int id)
        {
            if (id < 1 || id > 8)
                return NotFound();

            var relativePath = _config["AppSettings:CurriculumFilePath"] ?? "data/curriculum-data.json";
            var filePath = Path.Combine(_env.WebRootPath, relativePath);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var jsonString = System.IO.File.ReadAllText(filePath);
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var rootData = JsonSerializer.Deserialize<Dictionary<string, WeekData>>(jsonString, options);

            string activeKey = $"week{id}";
            var weekModel = rootData != null && rootData.ContainsKey(activeKey) ? rootData[activeKey] : new WeekData();

            ViewBag.WeekNumber = id;

            return View("Week", weekModel);
        }
    }
}
