using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PatientPlanner.Data;
using PatientPlanner.Models;

namespace PatientPlanner.Pages.Devices
{
    public class IndexModel : PageModel
    {
        private readonly PatientPlanner.Data.TimetableContext _context;

        public IndexModel(PatientPlanner.Data.TimetableContext context)
        {
            _context = context;
        }

        public IList<Device> Devices { get; set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.Devices != null)
            {
                Devices = await _context.Devices.ToListAsync();
            }
        }
    }
}
