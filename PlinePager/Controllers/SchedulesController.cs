using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PlinePager.Data;
using PlinePager.Models;

namespace PlinePager.Controllers
{
    public class SchedulesController : Controller
    {
        private readonly PlinePagerContext _context;

        public SchedulesController(PlinePagerContext context)
        {
            _context = context;
        }

        private IEnumerable<TblArea> Areas => _context.TblAreas.ToList();
        private IEnumerable<TblSound> Sounds => _context.TblSounds.Where(t => t.Enable == true).ToList();

        // GET: Schedules
        public IActionResult Index()
        {
            if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_Index", _context.Set<TblSchedule>());
            return View();
        }

        // GET: Schedules/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblSchedule = await _context.TblSchedules
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tblSchedule == null)
            {
                return NotFound();
            }

            return View(tblSchedule);
        }

        // GET: Schedules/Create
        public IActionResult Create()
        {
            ViewBag.Areas = Areas;
            ViewBag.Sounds = Sounds;
            return View(new TblSchedule()
            {
                Enable = true,
                Volume = 0,
                Schedules = "{}",
            });
        }

        // POST: Schedules/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Areas,Sounds,Schedules,Volume,Enable")]
            TblSchedule tblSchedule)
        {
            ViewBag.Areas = Areas;
            ViewBag.Sounds = Sounds;

            if (ModelState.IsValid)
            {
                _context.Add(tblSchedule);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(tblSchedule);
        }

        // GET: Schedules/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            ViewBag.Areas = Areas;
            ViewBag.Sounds = Sounds;

            if (id == null)
            {
                return NotFound();
            }

            var tblSchedule = await _context.TblSchedules.FindAsync(id);
            if (tblSchedule == null)
            {
                return NotFound();
            }

            return View(tblSchedule);
        }

        // POST: Schedules/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Name,Areas,Sounds,Schedules,Volume,Enable")]
            TblSchedule tblSchedule)
        {
            ViewBag.Areas = Areas;
            ViewBag.Sounds = Sounds;

            if (id != tblSchedule.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tblSchedule);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TblScheduleExists(tblSchedule.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            return View(tblSchedule);
        }

        // POST: Schedules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var tblSchedule = await _context.TblSchedules.FindAsync(id);
            _context.TblSchedules.Remove(tblSchedule);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TblScheduleExists(long id)
        {
            return _context.TblSchedules.Any(e => e.Id == id);
        }
    }
}