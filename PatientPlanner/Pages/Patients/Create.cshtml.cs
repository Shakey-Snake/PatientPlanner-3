using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PatientPlanner.Data;
using PatientPlanner.Models;

namespace PatientPlanner.Pages.Patients
{
    public class CreateModel : PageModel
    {
        private readonly PatientPlanner.Data.TimetableContext _context;

        public CreateModel(PatientPlanner.Data.TimetableContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Device Device { get; set; }


        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Devices.Add(Device);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
