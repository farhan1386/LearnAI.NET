using LearnAI.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using System.IO;
using System.Collections.Generic;

namespace LearnAI.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public HomeController(IWebHostEnvironment env)
        {
            _env = env;
        }

        public IActionResult Index()
        {
            var filePath = Path.Combine(_env.WebRootPath, "data", "curriculum-data.json");
            var totalCounts = new Dictionary<int, int>();

            for (int i = 1; i <= 8; i++)
            {
                totalCounts[i] = 0;
            }

            if (System.IO.File.Exists(filePath))
            {
                var jsonString = System.IO.File.ReadAllText(filePath);
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var rootData = JsonSerializer.Deserialize<Dictionary<string, WeekData>>(jsonString, options);

                if (rootData != null)
                {
                    for (int i = 1; i <= 8; i++)
                    {
                        string key = $"week{i}";
                        if (rootData.ContainsKey(key) && rootData[key]?.Days != null)
                        {
                            int itemCount = 0;
                            foreach (var day in rootData[key].Days)
                            {
                                if (day.Sections == null) continue;
                                foreach (var sec in day.Sections)
                                {
                                    if (sec.Items == null) continue;
                                    itemCount += sec.Items.Count;
                                }
                            }
                            totalCounts[i] = itemCount;
                        }
                    }
                }
            }

            ViewBag.TotalCounts = JsonSerializer.Serialize(totalCounts);
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
