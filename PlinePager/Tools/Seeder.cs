using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Npgsql;
using PlinePager.Data;
using PlinePager.Models;
using SQLitePCL;

namespace PlinePager.Tools
{
    public class Seeder
    {
        private Timer _timerSchedule;
        private const int TimeSchedule = 1;
        private Timer _timerUpdate;
        private const int TimeUpdate = 3;
        private List<TblSchedule> _lstSchedule;
        private readonly PlinePagerContext _context;
        private readonly IConfiguration _configuration;
        private string _lastUpdateDate = "";

        public Seeder(PlinePagerContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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
                    var query = "SELECT * FROM \"tblSchedules\" WHERE " +
                                "\"Enable\"=True AND \"Ended\"=False AND (" +
                                $"(\"ToDateEnable\"=True AND \"IntervalEnable\"=True AND '{date}' BETWEEN \"OfDate\" AND \"ToDate\") OR " +
                                $"(\"ToDateEnable\"=False AND \"IntervalEnable\"=True AND \"OfDate\">='{date}') OR " +
                                $"(\"IntervalEnable\"=False AND \"OfDate\"='{date}') OR " +
                                $"\"NextDate\"='{date}')";

                    var connection = new NpgsqlConnection(_configuration.GetConnectionString("psql"));
                    connection.Open();
                    using (var cmd = new NpgsqlDataAdapter(query, connection))
                    {
                        var data = new DataTable();
                        cmd.Fill(data);
                        _lstSchedule = Globals.ConvertToList<TblSchedule>(data);
                    }

                    _lstSchedule.ForEach(item =>
                        {
                            //B>A
                            double diffSecs = PersianDateDiffToSeconds(item.OfDate, item.OfHour, item.OfMinute,
                                date, hour, minute);
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
                                var updateQuery =
                                    $"UPDATE \"tblSchedules\" SET \"NextDate\"='{item.NextDate}', \"NextHour\"={item.NextHour}, " +
                                    $"\"NextMinute\"={item.NextMinute} " +
                                    $"WHERE \"Id\"={item.Id}";
                                var cmd = new NpgsqlCommand(updateQuery, connection);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    );

                    connection.Close();
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


        private double PersianDateDiffToSeconds(string persianDateA, int hourA, int minuteA, string persianDateB,
            int hourB,
            int minuteB)
        {
            try
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

                double different = (dateTimeB - dateTimeA).TotalSeconds;
                return different;
            }
            catch
            {
                return 0;
            }
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
            if (_lstSchedule == null || _lstSchedule.Count == 0)
                return;
            _timerSchedule.Stop();
            DateTime dt = DateTime.Now;
            int hour = dt.Hour;
            int minute = dt.Minute;

            PersianCalendar persianCalendar = new PersianCalendar();
            string persianDate =
                $"{persianCalendar.GetYear(dt):0000}/{persianCalendar.GetMonth(dt):00}/{persianCalendar.GetDayOfMonth(dt):00}";
            var list = _lstSchedule.Where(t =>
                    (
                        (t.OfHour == hour && t.OfMinute == minute) ||
                        (t.NextDate == persianDate && t.NextHour == hour && t.NextMinute == minute)
                    ) &&
                    t.Ended == false
                //  && t.Played == false 
            ).ToList();

            list.ForEach(item =>
            {
                if (item.ToDateEnable &&
                    string.CompareOrdinal(persianDate, item.ToDate) >= 0 &&
                    item.ToHour >= hour && item.ToMinute >= minute)
                {
                    item.Ended = true;
                    _context.Database.ExecuteSqlRaw(
                        $"UPDATE \"tblSchedules\" SET " +
                        $"\"Ended\"={(item.Ended ? "True" : "False")} " +
                        $"WHERE \"Id\"={item.Id}"
                    );
                    return;
                }

                Console.WriteLine(item.Name);
                CallFileOnArea(item);

                if (item.IntervalEnable)
                {
                    double seconds = (item.IntervalDay * 3600 * 24) + (item.IntervalHour * 3600) +
                                     (item.IntervalMinute * 60);
                    var newDate = item.NextDate is null or ""
                        ? Globals.AddTime(item.OfDate, item.OfHour, item.OfMinute, seconds)
                        : Globals.AddTime(item.NextDate, item.NextHour, item.NextMinute, seconds);

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
                _context.Database.ExecuteSqlRaw(
                    $"UPDATE \"tblSchedules\" SET " +
                    $"\"NextDate\"='{item.NextDate}', " +
                    $"\"NextHour\"={item.NextHour}, " +
                    $"\"NextMinute\"={item.NextMinute}, " +
                    $"\"Ended\"={(item.Ended ? "True" : "False")}, " +
                    $"\"Played\"={(item.Played ? "True" : "False")} " +
                    $"WHERE \"Id\"={item.Id}"
                );
            });

            _timerSchedule.Start();
        }

        public string CallFileOnArea(TblSchedule schedule)
        {
            try
            {
                var sounds = JsonConvert.DeserializeObject<long[]>(schedule.Sounds);
                var tblSounds = _context.TblSounds.Where(t => sounds.Contains(t.Id)).ToList();
                var soundsPath = string.Empty;
                tblSounds.ForEach(sound =>
                {
                    if (sound.FileName.ToLower().EndsWith(".wav"))
                    {
                        //sound.FileName = sound.FileName.Substring(0, sound.FileName.Length - 4);
                        soundsPath += Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
                            $"{sound.FileName[..^4]}&");
                    }
                    else
                    {
                        soundsPath += $"{sound.FileName}&";
                    }
                });
                if (soundsPath != string.Empty)
                {
                    soundsPath = soundsPath[..^1];
                }

                var areas = JsonConvert.DeserializeObject<long[]>(schedule.Areas);
                var tblAgents = _context.TblAgents.Where(t => areas.Contains(t.AreaId)).ToList();
                var agentsList = string.Empty;
                tblAgents.ForEach(agent =>
                {
                    Globals.Hangup(agent.Username);
                    agentsList += agent.Agent == Globals.AgentType.Sip
                        ? $"SIP/{agent.Username}&"
                        : $"CONSOLE/{agent.Username}&";
                });
                if (agentsList != string.Empty)
                {
                    agentsList = agentsList[..^1];
                }

                var strCall = "Channel: Local/*0000@pline-page\n" +
                              $"Setvar: users={agentsList}\n" +
                              $"Setvar: vol={schedule.Volume}\n" +
                              "CallerID: \"00000000\"<pline-page>\n" +
                              "Application: Playback\n" +
                              $"Data: {soundsPath}\n";
                var path = $"/var/spool/asterisk/outgoing/{Globals.GenerateId()}.call";
                File.WriteAllText(path, strCall, Encoding.ASCII);
                Globals.RunCmd("/usr/bin/chmod", "0777 " + path);
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}