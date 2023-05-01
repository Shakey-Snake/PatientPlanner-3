using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PatientPlanner.Data;
using PatientPlanner.Models;

namespace PatientPlanner.Pages.Patients
{
    public class DeleteModel : PageModel
    {
        private readonly PatientPlanner.Data.TimetableContext _context;

        public DeleteModel(PatientPlanner.Data.TimetableContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Device Device { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.Devices == null)
            {
                return NotFound();
            }

            var device = await _context.Devices.FirstOrDefaultAsync(m => m.ID == id);

            if (device == null)
            {
                return NotFound();
            }
            else
            {
                Device = device;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null || _context.Devices == null)
            {
                return NotFound();
            }
            var device = await _context.Devices.FindAsync(id);

            if (device != null)
            {
                Device = device;
                _context.Devices.Remove(Device);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
