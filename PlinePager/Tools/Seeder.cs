using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Timers;
using Microsoft.EntityFrameworkCore;
using PlineFaxServer.Tools;
using PlinePager.Data;
using PlinePager.Models;
using Timer = System.Timers.Timer;

namespace PlinePager.Tools
{
    public class Seeder
    {
        private Timer _timerSchedule;
        private const int TimeSchedule = 6;
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

            Globals.TimerSchedule = _timerSchedule;
            Globals.TimerUpdate = _timerUpdate;
        }

        private void TimerOnElapsedUpdate(object sender, ElapsedEventArgs e)
        {
            _timerUpdate.Stop();
            //_timerSchedule.Stop();
            var dt = DateTime.Now;
            var hour = dt.Hour;
            var minute = dt.Minute;
            var year = dt.Year;
            var p = new PersianCalendar();
            var date = $"{p.GetYear(dt):0000}/{p.GetMonth(dt):00}/{p.GetDayOfMonth(dt):00}";

            try
            {
                if (Globals.ForceReload ||
                    _lastUpdateDate == string.Empty ||
                    (hour == 0 && minute == 0 &&
                     string.Compare(date, _lastUpdateDate,
                         StringComparison.Ordinal) > 0))
                {
                    _lstSchedule =
                        _context.TblSchedules.Where(t =>
                                t.Enable && t.Ended == false && (
                                    (t.ToDateEnable && t.OfDate.CompareTo(date) <= 0 &&
                                     t.ToDate.CompareTo(date) >= 0) ||
                                    (t.ToDateEnable == false && t.OfDate == date) ||
                                    t.NextDate == date))
                            .ToList();
                    _lstSchedule.ForEach(item =>
                    {
                        //B>A
                        double diffSecs = PersianDateDiffToSeconds(date, hour, minute,
                            item.ToDate, item.ToHour,
                            item.ToMinute);
                        if (diffSecs > 0 && item.IntervalEnable)
                        {
                            long intervalSecs = (item.IntervalDay * 3600 * 24) + (item.IntervalHour * 3600) +
                                                (item.IntervalMinute * 60);
                            var b = diffSecs % intervalSecs;
                            var dtNext = DateTime.Now.AddSeconds(b);
                            var pNext = new PersianCalendar();
                            item.NextDate =
                                $"{pNext.GetYear(dtNext):0000}/{pNext.GetMonth(dtNext):00}/{pNext.GetDayOfMonth(dtNext):00}";
                            item.NextHour = pNext.GetHour(dtNext);
                            item.NextMinute = pNext.GetMinute(dtNext);
                            _context.Update(item);
                            _context.SaveChanges();
                        }
                    });

                    Globals.ForceReload = false;
                    _lastUpdateDate = date;
                    Globals.NeedToUpdate = false;
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

        private DateTime AddTime(string persianDate, int hour, int minute, double seconds)
        {
            PersianCalendar p = new PersianCalendar();
            string[] parts = persianDate.Split('/', '-');
            DateTime dta = p.ToDateTime(Convert.ToInt32(parts[0]), Convert.ToInt32(parts[1]), Convert.ToInt32(parts[2]),
                hour, minute, 0, 0);
            var newDate = dta.AddSeconds(seconds);
            return newDate;
        }

        private double PersianDateDiffToSeconds(string persianDateA, int hourA, int minuteA, string persianDateB,
            int hourB,
            int minuteB)
        {
            string[] persianDate1 = persianDateA.Split('/');
            string[] persianDate2 = persianDateB.Split('/');

            PersianCalendar date1 = new PersianCalendar();
            DateTime dateTimeA = date1.ToDateTime(Convert.ToInt32(persianDate1[0]),
                Convert.ToInt32(persianDate1[1]),
                Convert.ToInt32(persianDate1[2]), hourA, minuteA, 0, 0);

            PersianCalendar date2 = new PersianCalendar();
            DateTime dateTimeB = date2.ToDateTime(Convert.ToInt32(persianDate2[0]),
                Convert.ToInt32(persianDate2[1]),
                Convert.ToInt32(persianDate2[2]), hourB, minuteB, 0, 0);

            double different = (dateTimeB - dateTimeA).TotalMilliseconds;
            return different;
        }

        private DateTime PersianToGeorgia(string persian, int hour, int minute)
        {
            string[] persianDate = persian.Split('/');
            PersianCalendar date = new PersianCalendar();
            DateTime dateTime = date.ToDateTime(Convert.ToInt32(persianDate[0]),
                Convert.ToInt32(persianDate[1]),
                Convert.ToInt32(persianDate[2]), hour, minute, 0, 0);
            return dateTime;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (Globals.NeedToUpdate || _lstSchedule == null || _lstSchedule.Count == 0)
                return;
            _timerSchedule.Stop();
            DateTime dt = DateTime.Now;
            int hour = dt.Hour;
            int minute = dt.Minute;

            PersianCalendar persianCalendar = new PersianCalendar();
            string persianDate =
                $"{persianCalendar.GetYear(dt):0000}/{persianCalendar.GetMonth(dt):00}/{persianCalendar.GetDayOfMonth(dt):00}";
            using var list = _lstSchedule.Where(t =>
                    (
                        (t.OfHour == hour && t.OfMinute == minute) ||
                        (t.NextDate == persianDate && t.NextHour == hour && t.NextMinute == minute)
                    ) &&
                    t.Ended == false
                //  && t.Played == false 
            ).GetEnumerator();

            int changes = 0;
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

                Console.WriteLine(DateTime.Now);
                if (item.IntervalEnable)
                {
                    double seconds = (item.IntervalDay * 3600 * 24) + (item.IntervalHour * 3600) +
                                     (item.IntervalMinute * 60);
                    DateTime newDate;
                    newDate = item.NextDate is null or ""
                        ? AddTime(item.OfDate, item.OfHour, item.OfMinute, seconds)
                        : AddTime(item.NextDate, item.NextHour, item.NextMinute, seconds);

                    var p = new PersianCalendar();
                    item.NextDate = $"{p.GetYear(newDate):0000}/{p.GetMonth(newDate):00}/{p.GetDayOfMonth(newDate):00}";
                    item.NextHour = p.GetHour(newDate);
                    item.NextMinute = p.GetMinute(newDate);
                }
                else
                {
                    item.Ended = true;
                }

                item.Played = true;
                changes++;
                _context.Update(item);
            }

            if (changes > 0) _context.SaveChanges();
            _timerSchedule.Start();
        }
    }
}