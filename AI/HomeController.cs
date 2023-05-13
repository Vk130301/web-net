using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApplication2.Models;
using Python.Runtime;
using System.Text;
using System.Runtime.CompilerServices;


namespace WebApplication2.Controllers
{
    public class HomeController : Controller
    {
        
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {




            {
                //string scriptPath = "C:/Users/Admin/Desktop/WebApplication2/WebApplication2/Controllers/test1.py";
                //string pythonScript = System.IO.File.ReadAllText(scriptPath);
                //var outputStream = new MemoryStream();
                //var streamWriter = new StreamWriter(outputStream);
                //streamWriter.AutoFlush = true;
                //Console.SetOut(streamWriter);


                //var psi = new ProcessStartInfo();
                string pythonPath = "C:/Users/Admin/AppData/Local/Programs/Python/Python37/python.exe";
                string pythonScript = "C:/Users/Admin/Desktop/WebApplication2/WebApplication2/Controllers/test1.py 2";

                // Configure the process start info
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = pythonPath;
                startInfo.Arguments = pythonScript;
                Process process = new Process();
                //psi.Arguments = script;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                process.StartInfo = startInfo;
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                process.Dispose();
                //psi.UseShellExecute = false;
                //psi.RedirectStandardOutput = true;
                //psi.RedirectStandardError = true;
                //PythonEngine.Exec(pythonScript);
                ViewBag.kq = output;
                ViewBag.bug = error;
                return View();
                
            }
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