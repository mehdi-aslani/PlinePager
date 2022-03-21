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
    public class AreasController : Controller
    {
        private readonly PlinePagerContext _context;

        public AreasController(PlinePagerContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_Index", _context.Set<TblArea>());
            return View();
        }
        
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Desc")] TblArea tblArea)
        {
            if (ModelState.IsValid)
            {
                int count = await _context.TblAreas.Where(t => t.Name == tblArea.Name).CountAsync();
                if (count > 0)
                {
                    ModelState.AddModelError("Name", "این نام قبلا استفاده شده است");
                    return View(tblArea);
                }
                _context.Add(tblArea);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tblArea);
        }

        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblArea = await _context.TblAreas.FindAsync(id);
            if (tblArea == null)
            {
                return NotFound();
            }
            return View(tblArea);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Name,Desc")] TblArea tblArea)
        {
            if (id != tblArea.Id)
            {
                return NotFound();
            }

            int count = await _context.TblAreas.Where(t => t.Name == tblArea.Name && t.Id != id).CountAsync();
            if (count > 0)
            {
                ModelState.AddModelError("Name", "این نام قبلا استفاده شده است");
                return View(tblArea);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tblArea);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TblAreaExists(tblArea.Id))
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
            return View(tblArea);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            try
            {
                if (id == 1)
                {
                    return Json(new
                    {
                        error = "امکان حذف ناحیه پیش فرض وجود ندارد"
                    });
                }

                var cntAgent = await _context.TblAgents.Where(t => t.AreaId == id).CountAsync();
                if (cntAgent > 0)
                {
                    return Json(new
                    {
                        error = "امکان حذف این ناحیه به دلیل تخصیص پبجر وجود ندارد"
                    });
                }

                var tblArea = await _context.TblAreas.FindAsync(id);
                _context.TblAreas.Remove(tblArea);
                var res = await _context.SaveChangesAsync();

                if (res == 0)
                {
                    return Json(new
                    {
                        error = "خطا در حذف ناحیه. لطفا با راهبر سیستم تماس بگیرید."
                    });
                }
                return Json(new { error = "" });
            }
            catch
            {
                return Json(new
                {
                    error = "خطا در حذف ناحیه. لطفا با راهبر سیستم تماس بگیرید."
                });
            }
        }

        private bool TblAreaExists(long id)
        {
            return _context.TblAreas.Any(e => e.Id == id);
        }
    }
}
