using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MySolution.BusinessLayers;
using MySolution.DomainModels;

namespace MySolution.HRM.Controllers
{
    public class EmployeeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            var model = await HRMDataService.ListEmployeesAsync();
            return View(model);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var emp = await HRMDataService.GetEmployeeAsync(id);
            if (emp == null) return NotFound();

            return View(emp);
        }

        // GET: /Employee/Edit/EMP001
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var emp = await HRMDataService.GetEmployeeAsync(id);
            if (emp == null) return NotFound();

            return View(emp);
        }

        // POST: /Employee/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Employee model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await HRMDataService.UpdateEmployeeAsync(model);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Employee/Delete/EMP001
        public async Task<IActionResult> Delete(string id)
        {
            var emp = await HRMDataService.GetEmployeeAsync(id);
            if (emp == null) return NotFound();

            return View(emp);
        }

        // POST: /Employee/DeleteConfirmed
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await HRMDataService.DeleteEmployeeAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
