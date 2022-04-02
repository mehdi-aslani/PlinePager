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

namespace PlinePager.Tools
{
    public class Seeder
    {
        private Timer _timerSchedule;
        private Timer _timerAzan;
        private const int TimeSchedule = 10;
        private const int TimeAzan = 1;
        private Timer _timerUpdate;
        private const int TimeUpdate = 1;
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
            if (Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "before-azans")) == false)
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "before-azans"));
            }

            if (Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "azans")) == false)
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "azans"));
            }

            if (Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "after-azans")) == false)
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "after-azans"));
            }

            if (!_context.TblAreas.Any())
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

            _timerAzan = new Timer() {Interval = TimeAzan * 1000, Enabled = true};
            _timerAzan.Elapsed += TimerAzanOnElapsed;

            _timerUpdate = new() {Interval = TimeUpdate * 1000, Enabled = true};
            _timerUpdate.Elapsed += TimerOnElapsedUpdate;
            _timerUpdate.Start();
        }

        private void CalcTime(TblAzan azan)
        {
            Random rnd = new Random();
            var beforeAzanDir = Directory.GetFiles(Path.Combine("wwwroot", "before-azans"));
            var azanDir = Directory.GetFiles(Path.Combine("wwwroot", "azans"));
            var afterAzanDir = Directory.GetFiles(Path.Combine("wwwroot", "after-azans"));

            DateTime cur = DateTime.Now;
            var bef = JsonConvert.DeserializeObject<long[]>(_tblAzan.SoundsBeforeA);
            var la = 0;
            var azansSound = _context.TblSounds.Where(m => bef.Contains(m.Id)).ToList();
            if (azansSound.Count == 0)
            {
                if (beforeAzanDir.Length > 0)
                {
                    var sound = Path.Combine(Directory.GetCurrentDirectory(),
                        beforeAzanDir[rnd.Next(beforeAzanDir.Length - 1)]);
                    la = Globals.GetWavSoundLength(sound);
                    _tblAzan.SoundsBeforeA = $"f:{sound[..^4]}";
                }
            }
            else
            {
                azansSound.ForEach(i => { la += i.Length; });
            }

            DateTime dt = new DateTime(cur.Year, cur.Month, cur.Day, azan.HourA, azan.MinuteA, azan.SecondA);
            dt = dt.AddSeconds(-1 * la);
            azan.HourA = dt.Hour;
            azan.MinuteA = dt.Minute;
            azan.SecondA = dt.Second;

            bef = JsonConvert.DeserializeObject<long[]>(_tblAzan.SoundsBeforeB);
            la = 0;
            azansSound = _context.TblSounds.Where(m => bef.Contains(m.Id)).ToList();
            if (azansSound.Count == 0)
            {
                if (beforeAzanDir.Length > 0)
                {
                    var sound = Path.Combine(Directory.GetCurrentDirectory(),
                        beforeAzanDir[rnd.Next(beforeAzanDir.Length - 1)]);
                    la = Globals.GetWavSoundLength(sound);
                    _tblAzan.SoundsBeforeB = $"{sound[..^4]}";
                }
            }
            else
            {
                azansSound.ForEach(i => { la += i.Length; });
            }

            dt = new DateTime(cur.Year, cur.Month, cur.Day, azan.HourB, azan.MinuteB, azan.SecondB);
            dt = dt.AddSeconds(-1 * la);
            azan.HourB = dt.Hour;
            azan.MinuteB = dt.Minute;
            azan.SecondB = dt.Second;

            bef = JsonConvert.DeserializeObject<long[]>(_tblAzan.SoundsBeforeC);
            la = 0;
            azansSound = _context.TblSounds.Where(m => bef.Contains(m.Id)).ToList();
            if (azansSound.Count == 0)
            {
                if (beforeAzanDir.Length > 0)
                {
                    var sound = Path.Combine(Directory.GetCurrentDirectory(),
                        beforeAzanDir[rnd.Next(beforeAzanDir.Length - 1)]);
                    la = Globals.GetWavSoundLength(sound);
                    _tblAzan.SoundsBeforeC = $"{sound[..^4]}";
                }
            }
            else
            {
                azansSound.ForEach(i => { la += i.Length; });
            }

            dt = new DateTime(cur.Year, cur.Month, cur.Day, azan.HourC, azan.MinuteC, azan.SecondC);
            dt = dt.AddSeconds(-1 * la);
            azan.HourC = dt.Hour;
            azan.MinuteC = dt.Minute;
            azan.SecondC = dt.Second;
        }

        private string _azanUpdate = "";
        private TblAzan _tblAzan;

        private void TimerAzanOnElapsed(object sender, ElapsedEventArgs e)
        {
            _timerAzan.Stop();
            var dt = DateTime.Now;
            var hour = dt.Hour;
            var minute = dt.Minute;
            var second = dt.Second;
            var p = new PersianCalendar();
            var date = $"{p.GetYear(dt):0000}/{p.GetMonth(dt):00}/{p.GetDayOfMonth(dt):00}";

            if (Globals.ForceReloadAzan || string.Compare(_azanUpdate, date, StringComparison.Ordinal) < 0)
            {
                Globals.ForceReloadAzan = false;
                _azanUpdate = date;

                var query = $"SELECT t.* FROM \"tblAzans\" t WHERE t.\"Date\"='{date}'";
                var connection = new NpgsqlConnection(_configuration.GetConnectionString("psql"));
                connection.Open();
                using (var cmd = new NpgsqlDataAdapter(query, connection))
                {
                    var data = new DataTable();
                    cmd.Fill(data);
                    var lstAzan = Globals.ConvertToList<TblAzan>(data);
                    _tblAzan = lstAzan.FirstOrDefault();
                }

                connection.Close();

                if (_tblAzan == null)
                {
                    _timerAzan.Start();
                    return;
                }

                CalcTime(_tblAzan);
            }

            if (_tblAzan == null)
            {
                _timerAzan.Start();
                return;
            }

            if (_tblAzan.EnableA && _tblAzan.HourA == hour && _tblAzan.MinuteA == minute &&
                second - 1 <= _tblAzan.SecondA && _tblAzan.SecondA <= second + 1)
            {
                _tblAzan.EnableA = false;
                CallFileAzanOnArea(_tblAzan, 0);
            }
            else if (_tblAzan.EnableB && _tblAzan.HourB == hour && _tblAzan.MinuteB == minute &&
                     second - 1 <= _tblAzan.SecondB && _tblAzan.SecondB <= second + 1)
            {
                _tblAzan.EnableB = false;
                CallFileAzanOnArea(_tblAzan, 1);
            }
            else if (_tblAzan.EnableC && _tblAzan.HourC == hour && _tblAzan.MinuteC == minute &&
                     second - 1 <= _tblAzan.SecondC && _tblAzan.SecondC <= second + 1)
            {
                _tblAzan.EnableC = false;
                CallFileAzanOnArea(_tblAzan, 2);
            }

            _timerAzan.Start();
        }

        private void TimerOnElapsedUpdate(object sender, ElapsedEventArgs e)
        {
            _timerUpdate.Stop();
            _timerSchedule.Stop();
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
                                $"(\"ToDateEnable\"=False AND \"IntervalEnable\"=True AND \"OfDate\"<='{date}') OR " +
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
                            double diffSecs = Globals.PersianDateDiffToSeconds(item.OfDate, item.OfHour, item.OfMinute,
                                date, hour, minute);
                            if (diffSecs > 0 && item.IntervalEnable)
                            {
                                long intervalSecs = (item.IntervalDay * 3600 * 24) + (item.IntervalHour * 3600) +
                                                    (item.IntervalMinute * 60);
                                var b = diffSecs % intervalSecs;
                                var dtNext = dt.AddSeconds(intervalSecs - b);
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

                if (_lstSchedule is {Count: > 0} && _timerSchedule.Enabled == false)
                    _timerSchedule.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            _timerUpdate.Start();
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
                CallFileOnScheduleArea(item);

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

        private string CallFileOnScheduleArea(TblSchedule schedule)
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

        private string CallFileAzanOnArea(TblAzan azan, int time)
        {
            try
            {
                var finalSounds = string.Empty;
                var finalAreas = string.Empty;
                var beforeAzanDir = Directory.GetFiles(Path.Combine("wwwroot", "before-azans"));
                var azanDir = Directory.GetFiles(Path.Combine("wwwroot", "azans"));
                var afterAzanDir = Directory.GetFiles(Path.Combine("wwwroot", "after-azans"));
                Random rnd = new Random(DateTime.Now.Millisecond);
                var volume = 0;

                switch (time)
                {
                    case 0:
                    {
                        long[] bef;
                        List<TblSound> listSounds;
                        if (_tblAzan.SoundsBeforeA.StartsWith("f:"))
                        {
                            finalSounds += $"{_tblAzan.SoundsBeforeA[2..]}&";
                        }
                        else
                        {
                            bef = JsonConvert.DeserializeObject<long[]>(_tblAzan.SoundsBeforeA);
                            listSounds = _context.TblSounds.Where(m => bef.Contains(m.Id)).ToList();

                            listSounds.ForEach(i =>
                            {
                                var sound = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "sounds",
                                    i.FileName);
                                if (sound.ToLower().EndsWith(".wav"))
                                {
                                    sound = $"{sound[..^4]}";
                                }

                                finalSounds += $"{sound}&";
                            });
                        }


                        /*******************************************************/
                        bef = JsonConvert.DeserializeObject<long[]>(_tblAzan.SoundsA);
                        listSounds = _context.TblSounds.Where(m => bef.Contains(m.Id)).ToList();

                        listSounds.ForEach(i =>
                        {
                            var sound = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "sounds", i.FileName);
                            if (sound.ToLower().EndsWith(".wav"))
                            {
                                sound = $"{sound[..^4]}";
                            }

                            finalSounds += $"{sound}&";
                        });
                        if (listSounds.Count == 0)
                        {
                            if (azanDir.Length > 0)
                            {
                                var sound = Path.Combine(Directory.GetCurrentDirectory(),
                                    azanDir[rnd.Next(azanDir.Length - 1)]);
                                finalSounds += $"{sound[..^4]}&";
                            }
                        }

                        /*******************************************************/
                        bef = JsonConvert.DeserializeObject<long[]>(_tblAzan.SoundsAfterA);
                        listSounds = _context.TblSounds.Where(m => bef.Contains(m.Id)).ToList();

                        listSounds.ForEach(i =>
                        {
                            var sound = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "sounds", i.FileName);
                            if (sound.ToLower().EndsWith(".wav"))
                            {
                                sound = $"{sound[..^4]}";
                            }

                            finalSounds += $"{sound}&";
                        });
                        if (listSounds.Count == 0)
                        {
                            if (afterAzanDir.Length > 0)
                            {
                                var sound = Path.Combine(Directory.GetCurrentDirectory(),
                                    afterAzanDir[rnd.Next(afterAzanDir.Length - 1)]);
                                finalSounds += $"{sound[..^4]}&";
                            }
                        }

                        var area = JsonConvert.DeserializeObject<long[]>(_tblAzan.AreasA);
                        var lstAgents = area is {Length: > 0}
                            ? _context.TblAgents.Where(m => area.Contains(m.AreaId)).ToList()
                            : _context.TblAgents.ToList();

                        lstAgents.ForEach(item =>
                        {
                            finalAreas += item.Agent == Globals.AgentType.Sip
                                ? $"SIP/{item.Username}&"
                                : $"CONSOLE/{item.Username}&";
                        });
                        volume = azan.VolumeA;
                    }
                        break;

                    case 1:
                    {
                        long[] bef;
                        List<TblSound> listSounds;
                        if (_tblAzan.SoundsBeforeB.StartsWith("f:"))
                        {
                            finalSounds += $"{_tblAzan.SoundsBeforeB[2..]}&";
                        }
                        else
                        {
                            bef = JsonConvert.DeserializeObject<long[]>(_tblAzan.SoundsBeforeB);
                            listSounds = _context.TblSounds.Where(m => bef.Contains(m.Id)).ToList();

                            listSounds.ForEach(i =>
                            {
                                var sound = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "sounds",
                                    i.FileName);
                                if (sound.ToLower().EndsWith(".wav"))
                                {
                                    sound = $"{sound[..^4]}";
                                }

                                finalSounds += $"{sound}&";
                            });
                        }

                        /*******************************************************/
                        bef = JsonConvert.DeserializeObject<long[]>(_tblAzan.SoundsB);
                        listSounds = _context.TblSounds.Where(m => bef.Contains(m.Id)).ToList();

                        listSounds.ForEach(i =>
                        {
                            var sound = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "sounds", i.FileName);
                            if (sound.ToLower().EndsWith(".wav"))
                            {
                                sound = $"{sound[..^4]}";
                            }

                            finalSounds += $"{sound}&";
                        });
                        if (listSounds.Count == 0)
                        {
                            if (azanDir.Length > 0)
                            {
                                var sound = Path.Combine(Directory.GetCurrentDirectory(),
                                    azanDir[rnd.Next(azanDir.Length - 1)]);
                                finalSounds += $"{sound[..^4]}&";
                            }
                        }

                        /*******************************************************/
                        bef = JsonConvert.DeserializeObject<long[]>(_tblAzan.SoundsAfterB);
                        listSounds = _context.TblSounds.Where(m => bef.Contains(m.Id)).ToList();

                        listSounds.ForEach(i =>
                        {
                            var sound = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "sounds", i.FileName);
                            if (sound.ToLower().EndsWith(".wav"))
                            {
                                sound = $"{sound[..^4]}";
                            }

                            finalSounds += $"{sound}&";
                        });
                        if (listSounds.Count == 0)
                        {
                            if (afterAzanDir.Length > 0)
                            {
                                var sound = Path.Combine(Directory.GetCurrentDirectory(),
                                    afterAzanDir[rnd.Next(afterAzanDir.Length - 1)]);
                                finalSounds += $"{sound[..^4]}&";
                            }
                        }

                        var area = JsonConvert.DeserializeObject<long[]>(_tblAzan.AreasB);
                        var lstAgents = area is {Length: > 0}
                            ? _context.TblAgents.Where(m => area.Contains(m.AreaId)).ToList()
                            : _context.TblAgents.ToList();

                        lstAgents.ForEach(item =>
                        {
                            finalAreas += item.Agent == Globals.AgentType.Sip
                                ? $"SIP/{item.Username}&"
                                : $"CONSOLE/{item.Username}&";
                        });
                        volume = azan.VolumeB;
                    }
                        break;

                    case 2:
                    {
                        long[] bef;
                        List<TblSound> listSounds;
                        if (_tblAzan.SoundsBeforeC.StartsWith("f:"))
                        {
                            finalSounds += $"{_tblAzan.SoundsBeforeC[2..]}&";
                        }
                        else
                        {
                            bef = JsonConvert.DeserializeObject<long[]>(_tblAzan.SoundsBeforeC);
                            listSounds = _context.TblSounds.Where(m => bef.Contains(m.Id)).ToList();

                            listSounds.ForEach(i =>
                            {
                                var sound = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "sounds",
                                    i.FileName);
                                if (sound.ToLower().EndsWith(".wav"))
                                {
                                    sound = $"{sound[..^4]}";
                                }

                                finalSounds += $"{sound}&";
                            });
                        }

                        /*******************************************************/
                        bef = JsonConvert.DeserializeObject<long[]>(_tblAzan.SoundsC);
                        listSounds = _context.TblSounds.Where(m => bef.Contains(m.Id)).ToList();

                        listSounds.ForEach(i =>
                        {
                            var sound = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "sounds", i.FileName);
                            if (sound.ToLower().EndsWith(".wav"))
                            {
                                sound = $"{sound[..^4]}";
                            }

                            finalSounds += $"{sound}&";
                        });
                        if (listSounds.Count == 0)
                        {
                            if (azanDir.Length > 0)
                            {
                                var sound = Path.Combine(Directory.GetCurrentDirectory(),
                                    azanDir[rnd.Next(azanDir.Length - 1)]);
                                finalSounds += $"{sound[..^4]}&";
                            }
                        }

                        /*******************************************************/
                        bef = JsonConvert.DeserializeObject<long[]>(_tblAzan.SoundsAfterC);
                        listSounds = _context.TblSounds.Where(m => bef.Contains(m.Id)).ToList();

                        listSounds.ForEach(i =>
                        {
                            var sound = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "sounds", i.FileName);
                            if (sound.ToLower().EndsWith(".wav"))
                            {
                                sound = $"{sound[..^4]}";
                            }

                            finalSounds += $"{sound}&";
                        });
                        if (listSounds.Count == 0)
                        {
                            if (afterAzanDir.Length > 0)
                            {
                                var sound = Path.Combine(Directory.GetCurrentDirectory(),
                                    afterAzanDir[rnd.Next(afterAzanDir.Length - 1)]);
                                finalSounds += $"{sound[..^4]}&";
                            }
                        }

                        var area = JsonConvert.DeserializeObject<long[]>(_tblAzan.AreasC);
                        var lstAgents = area is {Length: > 0}
                            ? _context.TblAgents.Where(m => area.Contains(m.AreaId)).ToList()
                            : _context.TblAgents.ToList();

                        lstAgents.ForEach(item =>
                        {
                            finalAreas += item.Agent == Globals.AgentType.Sip
                                ? $"SIP/{item.Username}&"
                                : $"CONSOLE/{item.Username}&";
                        });
                        volume = azan.VolumeC;
                    }
                        break;
                }

                if (finalSounds.EndsWith("&"))
                {
                    finalSounds = finalSounds[..^1];
                }

                if (finalAreas.EndsWith("&"))
                {
                    finalAreas = finalAreas[..^1];
                }

                var strCall = "Channel: Local/*0000@pline-page\n" +
                              $"Setvar: users={finalAreas}\n" +
                              $"Setvar: vol={volume}\n" +
                              "CallerID: \"00000000\"<pline-page>\n" +
                              "Application: Playback\n" +
                              $"Data: {finalSounds}\n";
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