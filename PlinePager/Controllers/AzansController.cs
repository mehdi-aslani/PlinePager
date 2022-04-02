using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PlinePager.Data;
using PlinePager.Models;
using PlinePager.Tools;

namespace PlinePager.Controllers
{
    [Authorize]
    public class AzansController : Controller
    {
        private readonly PlinePagerContext _context;

        public AzansController(PlinePagerContext context)
        {
            _context = context;
        }

        private IEnumerable<TblArea> AreasList => _context.TblAreas.ToList();
        private IEnumerable<TblSound> SoundsList => _context.TblSounds.Where(t => t.Enable == true).ToList();


        // GET: Azans
        public IActionResult Index()
        {
            if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_Index", _context.Set<TblAzan>());
            return View();
        }

        // GET: Azans/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblAzan = await _context.TblAzans
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tblAzan == null)
            {
                return NotFound();
            }

            return View(tblAzan);
        }

        // GET: Azans/Create
        public IActionResult Create()
        {
            ViewBag.Areas = AreasList;
            ViewBag.Sounds = SoundsList;
            return View(new TblAzan()
            {
                VolumeA = 0,
                VolumeB = 0,
                VolumeC = 0,
                EnableA = true,
                EnableB = true,
                EnableC = true,
            });
        }

        // POST: Azans/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            long[] SoundsBeforeA,
            long[] SoundsA,
            long[] SoundsAfterA,
            long[] AreasA,
            long[] SoundsBeforeB,
            long[] SoundsB,
            long[] SoundsAfterB,
            long[] AreasB,
            long[] SoundsBeforeC,
            long[] SoundsC,
            long[] SoundsAfterC,
            long[] AreasC,
            [Bind(
                "Id,Date,EnableA,HourA,MinuteA,SecondA,SoundsBeforeA,SoundsA,SoundsAfterA,AreasA,EnableB," +
                "HourB,MinuteB,SecondB,SoundsBeforeB,SoundsB,SoundsAfterB,AreasB,EnableC,HourC,MinuteC,SecondC,SoundsBeforeC," +
                "SoundsC,SoundsAfterC,AreasC,VolumeA,VolumeB,VolumeC")]
            TblAzan tblAzan)
        {
            if (ModelState.IsValid)
            {
                if (await _context.TblAzans.AnyAsync(m => m.Date == tblAzan.Date))
                {
                    ModelState.AddModelError("Date", "تاریخ وارد شده تکراری می باشد");
                }
                else
                {
                    tblAzan.SoundsBeforeA = JsonConvert.SerializeObject(SoundsBeforeA);
                    tblAzan.SoundsA = JsonConvert.SerializeObject(SoundsA);
                    tblAzan.SoundsAfterA = JsonConvert.SerializeObject(SoundsAfterA);
                    tblAzan.AreasA = JsonConvert.SerializeObject(AreasA);

                    tblAzan.SoundsBeforeB = JsonConvert.SerializeObject(SoundsBeforeB);
                    tblAzan.SoundsB = JsonConvert.SerializeObject(SoundsB);
                    tblAzan.SoundsAfterB = JsonConvert.SerializeObject(SoundsAfterB);
                    tblAzan.AreasB = JsonConvert.SerializeObject(AreasB);

                    tblAzan.SoundsBeforeC = JsonConvert.SerializeObject(SoundsBeforeC);
                    tblAzan.SoundsC = JsonConvert.SerializeObject(SoundsC);
                    tblAzan.SoundsAfterC = JsonConvert.SerializeObject(SoundsAfterC);
                    tblAzan.AreasC = JsonConvert.SerializeObject(AreasC);

                    _context.Add(tblAzan);
                    await _context.SaveChangesAsync();
                    Globals.ForceReloadAzan = true;
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewBag.Areas = AreasList;
            ViewBag.Sounds = SoundsList;
            return View(tblAzan);
        }

        // GET: Azans/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblAzan = await _context.TblAzans.FindAsync(id);
            if (tblAzan == null)
            {
                return NotFound();
            }

            ViewBag.Areas = AreasList;
            ViewBag.Sounds = SoundsList;
            return View(tblAzan);
        }

        // POST: Azans/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id,
            long[] SoundsBeforeA,
            long[] SoundsA,
            long[] SoundsAfterA,
            long[] AreasA,
            long[] SoundsBeforeB,
            long[] SoundsB,
            long[] SoundsAfterB,
            long[] AreasB,
            long[] SoundsBeforeC,
            long[] SoundsC,
            long[] SoundsAfterC,
            long[] AreasC,
            [Bind("Id,Date,EnableA,HourA,MinuteA,SecondA,SoundsBeforeA" +
                  ",SoundsA,SoundsAfterA,AreasA,EnableB,HourB,MinuteB,SecondB,SoundsBeforeB," +
                  "SoundsB,SoundsAfterB,AreasB,EnableC,HourC,MinuteC,SecondC,SoundsBeforeC,SoundsC," +
                  "SoundsAfterC,AreasC,VolumeA,VolumeB,VolumeC")]
            TblAzan tblAzan)
        {
            if (id != tblAzan.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (await _context.TblAzans.AnyAsync(m => m.Date == tblAzan.Date && m.Id != tblAzan.Id))
                    {
                        ModelState.AddModelError("Date", "تاریخ وارد شده تکراری می باشد");
                    }
                    else
                    {
                        tblAzan.SoundsBeforeA = JsonConvert.SerializeObject(SoundsBeforeA);
                        tblAzan.SoundsA = JsonConvert.SerializeObject(SoundsA);
                        tblAzan.SoundsAfterA = JsonConvert.SerializeObject(SoundsAfterA);
                        tblAzan.AreasA = JsonConvert.SerializeObject(AreasA);

                        tblAzan.SoundsBeforeB = JsonConvert.SerializeObject(SoundsBeforeB);
                        tblAzan.SoundsB = JsonConvert.SerializeObject(SoundsB);
                        tblAzan.SoundsAfterB = JsonConvert.SerializeObject(SoundsAfterB);
                        tblAzan.AreasB = JsonConvert.SerializeObject(AreasB);

                        tblAzan.SoundsBeforeC = JsonConvert.SerializeObject(SoundsBeforeC);
                        tblAzan.SoundsC = JsonConvert.SerializeObject(SoundsC);
                        tblAzan.SoundsAfterC = JsonConvert.SerializeObject(SoundsAfterC);
                        tblAzan.AreasC = JsonConvert.SerializeObject(AreasC);
                        _context.Update(tblAzan);
                        await _context.SaveChangesAsync();
                        Globals.ForceReloadAzan = true;
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError("", "خطا در ذحیره اطلاعات. لطفا دوباره سعی کنید");
                }
            }

            ViewBag.Areas = AreasList;
            ViewBag.Sounds = SoundsList;
            return View(tblAzan);
        }

        // POST: Azans/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            try
            {
                var tblAzan = await _context.TblAzans.FindAsync(id);
                _context.TblAzans.Remove(tblAzan);
                int result = await _context.SaveChangesAsync();
                if (result > 0)
                {
                    Globals.ForceReloadAzan = true;
                    return Json(new
                    {
                        error = ""
                    });
                }
                else
                {
                    return Json(new
                    {
                        error = "خطا در حذف اذان. لطفا با راهبر سیستم تماس بگیرید."
                    });
                }
            }
            catch
            {
                return Json(new
                {
                    error = "خطا در حذف اذان. لطفا با راهبر سیستم تماس بگیرید."
                });
            }
        }

        private bool TblAzanExists(long id)
        {
            return _context.TblAzans.Any(e => e.Id == id);
        }
    }
}