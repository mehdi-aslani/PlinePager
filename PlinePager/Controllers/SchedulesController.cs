using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlineFaxServer.Tools;
using PlinePager.Data;
using PlinePager.Models;
using PlinePager.Tools;

namespace PlinePager.Controllers
{
    public class SchedulesController : Controller
    {
        private readonly PlinePagerContext _context;

        public SchedulesController(PlinePagerContext context)
        {
            _context = context;
        }

        private IEnumerable<TblArea> AreasList => _context.TblAreas.ToList();
        private IEnumerable<TblSound> SoundsList => _context.TblSounds.Where(t => t.Enable == true).ToList();

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

            var areas = JsonConvert.DeserializeObject<long[]>(tblSchedule.Areas);
            ViewBag.tblAreas = _context.TblAreas
                .Where(t => areas.Contains(t.Id)).ToList();
            var sounds = JsonConvert.DeserializeObject<long[]>(tblSchedule.Sounds);
            ViewBag.tblSounds = _context.TblSounds
                .Where(t => sounds.Contains(t.Id)).ToList();

            return View(tblSchedule);
        }

        // GET: Schedules/Create
        public IActionResult Create()
        {
            ViewBag.Areas = AreasList;
            ViewBag.Sounds = SoundsList;
            PersianCalendar p = new PersianCalendar();
            return View(new TblSchedule()
            {
                Enable = true,
                Volume = 0,
                ToDate =
                    $"{p.GetYear(DateTime.Now):0000}/{p.GetMonth(DateTime.Now):00}/{p.GetDayOfMonth(DateTime.Now):00}"
            });
        }

        // POST: Schedules/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(long[] Areas, long[] Sounds,
            [Bind(
                "Id,Name,Areas,Sounds,Volume,Enable,OfDate,OfHour,OfMinute,IntervalEnable,IntervalDay,IntervalHour,IntervalMinute,ToDateEnable,ToDate,ToHour,ToMinute,Ended,Played,NextDate,NextHour,NextMinute")]
            TblSchedule tblSchedule)
        {
            if (ModelState.IsValid)
            {
                if (await _context.TblSchedules.Where(t => t.Name == tblSchedule.Name).AnyAsync())
                {
                    ModelState.AddModelError("Name", "نام وارد شده تکراری است");
                }
                else
                {
                    tblSchedule.Areas = JsonConvert.SerializeObject(Areas);
                    tblSchedule.Sounds = JsonConvert.SerializeObject(Sounds);
                    _context.Add(tblSchedule);
                    await _context.SaveChangesAsync();
                    Globals.ForceReload = true;
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewBag.Areas = AreasList;
            ViewBag.Sounds = SoundsList;
            return View(tblSchedule);
        }

        // GET: Schedules/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblSchedule = await _context.TblSchedules.FindAsync(id);
            if (tblSchedule == null)
            {
                return NotFound();
            }

            ViewBag.Areas = AreasList;
            ViewBag.Sounds = SoundsList;
            return View(tblSchedule);
        }

        // POST: Schedules/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, long[] Areas, long[] Sounds,
            [Bind(
                "Id,Name,Areas,Sounds,Volume,Enable,OfDate,OfHour,OfMinute,IntervalEnable,IntervalDay,IntervalHour,IntervalMinute,ToDateEnable,ToDate,ToHour,ToMinute,Ended,Played,NextDate,NextHour,NextMinute")]
            TblSchedule tblSchedule)
        {
            if (id != tblSchedule.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                Globals.TimersSet(false);
                try
                {
                    if (await _context.TblSchedules.Where(t => t.Name == tblSchedule.Name && t.Id != tblSchedule.Id)
                        .AnyAsync())
                    {
                        ModelState.AddModelError("Name", "نام وارد شده تکراری است");
                    }
                    else
                    {
                        tblSchedule.Areas = JsonConvert.SerializeObject(Areas);
                        tblSchedule.Sounds = JsonConvert.SerializeObject(Sounds);
                        _context.Update(tblSchedule);
                        if (await _context.SaveChangesAsync() > 0)
                            Globals.ForceReload = true;
                    }
                }
                catch
                {
                    ModelState.AddModelError("", "در ویرایش اطلاعات خطایی رخ داده است");
                }

                Globals.TimersSet(true);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Areas = AreasList;
            ViewBag.Sounds = SoundsList;
            return View(tblSchedule);
        }

        // POST: Schedules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            try
            {
                var tblSchedule = await _context.TblSchedules.FindAsync(id);
                _context.TblSchedules.Remove(tblSchedule);
                int result = await _context.SaveChangesAsync();
                if (result > 0)
                {
                    Globals.ForceReload = true;
                    return Json(new
                    {
                        error = ""
                    });
                }
                else
                {
                    return Json(new
                    {
                        error = "خطا در حذف زمانبندی پخش. لطفا با راهبر سیستم تماس بگیرید."
                    });
                }
            }
            catch
            {
                return Json(new
                {
                    error = "خطا در حذف زمانبندی پخش. لطفا با راهبر سیستم تماس بگیرید."
                });
            }
        }

        private bool TblScheduleExists(long id)
        {
            return _context.TblSchedules.Any(e => e.Id == id);
        }
    }
}