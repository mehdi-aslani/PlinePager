#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PlinePager.Data;
using PlinePager.Models;
using PlinePager.Tools;

namespace PlinePager.Controllers
{
    [Authorize]
    public class SoundsController : Controller
    {
        private readonly PlinePagerContext _context;

        public SoundsController(PlinePagerContext context)
        {
            _context = context;
        }

        // GET: Sounds
        public IActionResult Index()
        {
            if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_Index", _context.Set<TblSound>());
            return View();
        }


        // GET: Sounds/Create
        public IActionResult Create()
        {
            return View(new TblSound()
            {
                Name = "",
                FileName = "",
                Enable = true,
            });
        }

        // POST: Sounds/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile fileName, [Bind("Id,Name,FileName,Enable")] TblSound tblSound)
        {
            if (fileName.Length == 0)
            {
                ModelState.AddModelError("FileName", "لطفا فایل صوتی را انتخاب کنید");
            }
            else if (ModelState.IsValid)
            {
                int cnt = await _context.TblSounds.Where(t => t.Name == tblSound.Name).CountAsync();
                if (cnt > 0)
                {
                    ModelState.AddModelError("Name", "این  شناسه پیجر قبلا تعریف شده است");
                    return View(tblSound);
                }

                string extension = System.IO.Path.GetExtension(fileName.FileName);
                if (!(extension.ToLower() == ".mp3" || extension.ToLower() == ".wav"))
                {
                    ModelState.AddModelError("FileName", "فایل انتخاب شده باید فرمت mp3 یا wav باشد");
                    return View(tblSound);
                }

                var datePath = "sounds/upload-files";
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", datePath);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var uniqueName = Globals.GenerateId();
                await using (var file =
                    new FileStream(
                        Path.Combine(path, uniqueName + "_tmp" + extension),
                        FileMode.Create))
                {
                    await fileName.CopyToAsync(file);
                }

                if (!Globals.Mp3ToWav(Path.Combine(path, uniqueName + "_tmp" + extension),
                    Path.Combine(path, uniqueName + ".wav"), true))
                {
                    ModelState.AddModelError("", "در بارگذاری فایل خطای رخ داد. لطفا مجددا سعی کنید");
                    return View(tblSound);
                }

                tblSound.FileName = Path.Combine(datePath, uniqueName + ".wav");
                _context.Add(tblSound);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(tblSound);
        }

        // GET: Sounds/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblSound = await _context.TblSounds.FindAsync(id);
            if (tblSound == null)
            {
                return NotFound();
            }

            return View(tblSound);
        }

        // POST: Sounds/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Name,FileName,Enable")] TblSound tblSound)
        {
            if (id != tblSound.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                int count = await _context.TblSounds.Where(t => t.Name == tblSound.Name && t.Id != id).CountAsync();
                if (count > 0)
                {
                    ModelState.AddModelError("Name", "این نام قبلا استفاده شده است");
                    return View(tblSound);
                }

                try
                {
                    _context.Update(tblSound);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TblSoundExists(tblSound.Id))
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

            return View(tblSound);
        }

        // POST: Sounds/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            try
            {
                var tblSound = await _context.TblSounds.FindAsync(id);
                _context.TblSounds.Remove(tblSound);
                int res = await _context.SaveChangesAsync();
                if (res == 0)
                {
                    return Json(new
                    {
                        error = "خطا در حذف فایل صوتی. لطفا با راهبر سیستم تماس بگیرید."
                    });
                }

                return Json(new {error = ""});
            }
            catch
            {
                return Json(new
                {
                    error = "خطا در حذف فایل صوتی. لطفا با راهبر سیستم تماس بگیرید."
                });
            }
        }

        public IActionResult IndexTest()
        {
            if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_IndexTest", _context.Set<TblSound>());
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> StartTestSound([Bind("SoundId", "AgentId", "Volume")] SoundTest soundTest)
        {
            if (ModelState.IsValid)
            {
                var agentM = await _context.TblAgents.Where(t => t.Id == soundTest.AgentId).FirstAsync();
                var soundM = await _context.TblSounds.Where(t => t.Id == soundTest.SoundId).FirstAsync();

                string agent = agentM.Agent == Globals.AgentType.Console ? "CONSOLE" : "SIP";
                agent = $"{agent}/{agentM.Username}";

                var err = Globals.CallFileOnAgent(
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", soundM.FileName), agent,
                    soundTest.Volume);
                if (err != string.Empty)
                {
                    ModelState.AddModelError("", err);
                }
            }

            return View("SoundTest");
        }

        [HttpPost]
        public IActionResult Hangup(int id)
        {
            var agent = _context.TblAgents.First(t => t.Id == id);
            bool r;
            r = Globals.Hangup(agent.Agent == Globals.AgentType.Sip
                ? $"SIP/{agent.Username}"
                : $"CONSOLE/{agent.Username}");
            return Json(new {result = r});
        }

        public IActionResult SoundTest(int id)
        {
            return View(new SoundTest()
            {
                Volume = 0,
                SoundId = id,
            });
        }

        private bool TblSoundExists(long id)
        {
            return _context.TblSounds.Any(e => e.Id == id);
        }
    }
}