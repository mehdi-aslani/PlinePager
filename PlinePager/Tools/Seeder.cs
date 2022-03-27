using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Timers;
using Microsoft.EntityFrameworkCore;
using PlineFaxServer.Tools;
using PlinePager.Data;
using PlinePager.Models;

namespace PlinePager.Tools
{
    public class Seeder
    {
        private Timer _timerSchedule;
        private const int TimeSchedule = 1;
        private Timer _timerUpdate;
        private const int TimeUpdate = 3;
        private readonly PlinePagerContext _context;
        private List<TblSchedule> _lstSchedule;
        private string _lastUpdateDate = "";

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
            _timerSchedule = new Timer() {Interval = TimeSchedule * 1000, Enabled = true};
            _timerSchedule.Elapsed += TimerOnElapsed;


            _timerUpdate = new() {Interval = TimeUpdate * 1000, Enabled = true};
            _timerUpdate.Elapsed += TimerOnElapsedUpdate;
            _timerUpdate.Start();
        }

        private async void TimerOnElapsedUpdate(object sender, ElapsedEventArgs e)
        {
            _timerUpdate.Stop();
            //_timerSchedule.Stop();
            DateTime dt = DateTime.Now;
            int hour = dt.Hour;
            int minute = dt.Minute;
            int year = dt.Year;
            PersianCalendar p = new PersianCalendar();
            string date = $"{p.GetYear(dt):0000}/{p.GetMonth(dt):00}/{p.GetDayOfMonth(dt):00}";

            try
            {
                if (Globals.ForceReload ||
                    _lastUpdateDate == string.Empty ||
                    (hour == 0 && minute == 0 &&
                     string.Compare(date, _lastUpdateDate,
                         StringComparison.Ordinal) > 0))
                {
                    this._lstSchedule =
                        await _context.TblSchedules.Where(t =>
                                t.Enable && t.Ended == false && (t.OfDate == date || t.NextDate == date))
                            .ToListAsync();
                    Globals.ForceReload = false;
                    _lastUpdateDate = date;
                }

                // if (_lstSchedule is {Count: > 0} && _timerSchedule.Enabled == false)
                //     _timerSchedule.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            _timerUpdate.Start();
        }

        public DateTime AddTime(string persianDate, long seconds)
        {
            PersianCalendar p = new PersianCalendar();
            string[] parts = persianDate.Split('/', '-');
            DateTime dta = p.ToDateTime(Convert.ToInt32(parts[0]), Convert.ToInt32(parts[1]), Convert.ToInt32(parts[2]),
                0, 0, 0, 0);
            var newDate = dta.AddMilliseconds(seconds);
            return newDate;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (_lstSchedule == null || _lstSchedule.Count == 0)
                return;
            _timerSchedule.Stop();
            DateTime dt = DateTime.Now;
            int hour = dt.Hour;
            int minute = dt.Minute;

            PersianCalendar persianCalendar = new PersianCalendar();
            string persianDate =
                $"{persianCalendar.GetYear(dt):0000}/{persianCalendar.GetMonth(dt):00}/{persianCalendar.GetDayOfMonth(dt):00}";
            using var list = _lstSchedule.Where(t =>
                t.OfHour == hour &&
                t.OfMinute == minute &&
                t.Played == false &&
                t.Ended == false
            ).GetEnumerator();

            while (list.MoveNext())
            {
                var item = list.Current;
                if (item is null) continue;

                if (item.ToDateEnable &&
                    string.CompareOrdinal(persianDate, item.ToDate) >= 0 &&
                    item.ToHour >= hour && item.ToMinute >= minute)
                {
                    item.Ended = true;
                    _context.Update(item);
                    continue;
                }

                //play
                if (item.IntervalEnable)
                {
                    long seconds = (item.IntervalDay * 3600 * 24) + (item.IntervalMinute * 3600) +
                                   (item.IntervalMinute * 60);
                    var newDate = AddTime(item.NextDate ?? item.OfDate, seconds);
                    var p = new PersianCalendar();
                    item.NextDate = $"{p.GetYear(newDate):0000}/{p.GetMonth(newDate):00}/{p.GetYear(newDate):00}";
                    item.NextHour = p.GetHour(newDate);
                    item.NextMinute = p.GetMinute(newDate);
                    item.Played = true;
                }
                else
                {
                    item.Ended = true;
                }

                _context.Update(item);
            }

            _context.SaveChanges();
            _timerSchedule.Start();
        }
    }
}