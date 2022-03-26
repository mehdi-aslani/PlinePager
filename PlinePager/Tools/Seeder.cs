using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Timers;
using Microsoft.EntityFrameworkCore;
using PlinePager.Data;
using PlinePager.Models;

namespace PlinePager.Tools
{
    public class Seeder 
    {
        private Timer _timer;
        private Timer _timerUpdate;
        private readonly PlinePagerContext _context;

        public Seeder(PlinePagerContext context)
        {
            _context = context;
        }

        public async void DatabaseInit()
        {
            if (_context.TblAreas.LongCount() == 0)
            {
                _context.TblAreas.Add(new PlinePager.Models.TblArea()
                {
                    Name = "عمومی",
                    Desc = ".::پیش فرض::.",
                });
                await _context.SaveChangesAsync();
            }
        }

        public void StartQueue()
        {
            _timer = new Timer() {Interval = 1 * 1000, Enabled = false};
            _timer.Elapsed += TimerOnElapsed;


            _timerUpdate = new() {Interval = 3 * 1000, Enabled = true};
            _timerUpdate.Elapsed += TimerOnElapsedUpdate;
            _timerUpdate.Start();
        }

        private List<TblSchedule> _lstSchedule;
        private string _lastUpdateDate = "";

        private async void TimerOnElapsedUpdate(object sender, ElapsedEventArgs e)
        {
            _timerUpdate.Stop();
            _timer.Stop();
            DateTime dt = DateTime.Now;
            int hour = dt.Hour;
            int minute = dt.Minute;
            int year = dt.Year;
            PersianCalendar p = new PersianCalendar();
            string date = $"{p.GetYear(dt):0000}/{p.GetMonth(dt):00}/{p.GetDayOfMonth(dt):00}";

            try
            {
                if (_lastUpdateDate == string.Empty || (hour == 0 && minute >= 0 &&
                                                        string.Compare(date, _lastUpdateDate,
                                                            StringComparison.Ordinal) > 0))
                {
                    this._lstSchedule =
                        await _context.TblSchedules.Where(t => t.Enable && t.OfDate == date).ToListAsync();
                    _lastUpdateDate = date;
                }

                if (_lstSchedule is {Count: > 0})
                    _timer.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            _timerUpdate.Start();
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            DateTime dt = DateTime.Now;
            int hour = dt.Hour;
            int minute = dt.Minute;
            int year = dt.Year;

            PersianCalendar p = new PersianCalendar();
            string date = $"{p.GetYear(dt):0000}/{p.GetMonth(dt):00}/{p.GetDayOfMonth(dt):00}";
            Console.WriteLine(date);
        }
    }
}