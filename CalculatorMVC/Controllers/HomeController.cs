using System;
using CalculatorMVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CalculatorMVC.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(Operation model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // basic defensive checks
            if (model.OperationType == OperationType.Division && model.NumberB == 0.0)
            {
                ModelState.AddModelError(nameof(model.NumberB), "Division by zero is not allowed.");
                return View(model);
            }

            // warn when input magnitudes are very large — double precision may lose significant digits
            const double PrecisionThreshold = 1e15;
            if (Math.Abs(model.NumberA) >= PrecisionThreshold || Math.Abs(model.NumberB) >= PrecisionThreshold)
            {
                ModelState.AddModelError(string.Empty, "Warning: input values are very large and results may lose precision.");
            }

            double computed;

            switch (model.OperationType)
            {
                case OperationType.Addition:
                    computed = model.NumberA + model.NumberB;
                    break;
                case OperationType.Subtraction:
                    computed = model.NumberA - model.NumberB;
                    break;
                case OperationType.Multiplication:
                    computed = model.NumberA * model.NumberB;
                    break;
                case OperationType.Division:
                    computed = model.NumberA / model.NumberB; // model.NumberB checked above
                    break;
                default:
                    ModelState.AddModelError(string.Empty, "Unknown operation.");
                    return View(model);
            }

            // Check for invalid numeric results (overflow to Infinity, NaN)
            if (double.IsInfinity(computed) || double.IsNaN(computed))
            {
                ModelState.AddModelError(string.Empty, "Calculation resulted in an invalid number (overflow or undefined).");
                return View(model);
            }

            // Round result for display to avoid showing excessive floating point noise
            model.Result = Math.Round(computed, 12);

            return View(model);
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
