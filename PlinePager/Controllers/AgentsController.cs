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
    public class AgentsController : Controller
    {
        private readonly PlinePagerContext _context;

        public AgentsController(PlinePagerContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_Index", _context.Set<TblAgent>());
            return View();
        }

        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblAgent = await _context.tblAgents
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tblAgent == null)
            {
                return NotFound();
            }

            return View(tblAgent);
        }

        public IActionResult Create()
        {
            TblAgent tblAgent = new TblAgent
            {
                Enable = true,
            };
            return View(tblAgent);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Agent,Username,Password,Area,Desc,Enable")] TblAgent tblAgent)
        {
            if (ModelState.IsValid)
            {
                int cntUsername = await _context.tblAgents.Where(t => t.Username == tblAgent.Username).CountAsync();
                if (cntUsername > 0)
                {
                    ModelState.AddModelError("username", "این  شناسه پیجر قبلا تعریف شده است");
                    return View(tblAgent);

                }
                _context.Add(tblAgent);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tblAgent);
        }

        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblAgent = await _context.tblAgents.FindAsync(id);
            if (tblAgent == null)
            {
                return NotFound();
            }
            return View(tblAgent);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Agent,Username,Password,AreaId,Desc,Enable")] TblAgent tblAgent)
        {
            if (id != tblAgent.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                int count = await _context.tblAgents.Where(t => t.Username == tblAgent.Username && t.Id != id).CountAsync();
                if (count > 0)
                {
                    ModelState.AddModelError("Name", "این نام قبلا استفاده شده است");
                    return View(tblAgent);
                }

                try
                {
                    _context.Update(tblAgent);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TblAgentExists(tblAgent.Id))
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
            return View(tblAgent);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            try
            {
                var tblAgent = await _context.tblAgents.FindAsync(id);
                _context.tblAgents.Remove(tblAgent);
                var res = await _context.SaveChangesAsync();

                if (res == 0)
                {
                    return Json(new
                    {
                        error = "خطا در حذف کاربر. لطفا با راهبر سیستم تماس بگیرید."
                    });
                }

                return Json(new { error = "" });
            }
            catch
            {
                return Json(new
                {
                    error = "خطا در حذف کاربر. لطفا با راهبر سیستم تماس بگیرید."
                });
            }

        }

        private bool TblAgentExists(long id)
        {
            return _context.tblAgents.Any(e => e.Id == id);
        }
    }
}
