using System;
using System.Linq;
using System.Net.Http;
using System.Timers;
using PlinePager.Controllers;
using PlinePager.Data;

namespace PlineFaxServer.Tools
{
    public class Seeder
    {
        private Timer _timer;
        private PlinePagerContext _context;

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
            _timer = new Timer() { Interval = 1000 };
            _timer.Elapsed += TimerOnElapsed;
            _timer.Enabled = true;
            _timer.Start();
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {

        }
    }
}