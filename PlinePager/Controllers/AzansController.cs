using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PlinePager.Data;
using PlinePager.Models;

namespace PlinePager.Controllers
{
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
        public async Task<IActionResult> Index()
        {
            return View(await _context.TblAzans.ToListAsync());
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
            return View();
        }

        // POST: Azans/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind(
                "Id,Date,EnableA,HourA,MinuteA,SecondA,SoundsBeforeA,SoundsA,SoundsAfterA,AreasA,EnableB," +
                "HourB,MinuteB,SecondB,SoundsBeforeB,SoundsB,SoundsAfterB,AreasB,EnableC,HourC,MinuteC,SecondC,SoundsBeforeC," +
                "SoundsC,SoundsAfterC,AreasC,VolumeA,VolumeB,VolumeC")]
            TblAzan tblAzan)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tblAzan);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
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
                    _context.Update(tblAzan);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TblAzanExists(tblAzan.Id))
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
            ViewBag.Areas = AreasList;
            ViewBag.Sounds = SoundsList;
            return View(tblAzan);
        }

        // GET: Azans/Delete/5
        public async Task<IActionResult> Delete(long? id)
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

        // POST: Azans/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var tblAzan = await _context.TblAzans.FindAsync(id);
            _context.TblAzans.Remove(tblAzan);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TblAzanExists(long id)
        {
            return _context.TblAzans.Any(e => e.Id == id);
        }
    }
}