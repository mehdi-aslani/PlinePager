using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using PlinePager.Models;

namespace PlinePager.Tools
{
    public static class Globals
    {
        public static string GenerateId()
        {
            return Guid.NewGuid().ToString("N");
        }

        public enum AgentType
        {
            Sip = 0,
            Console = 1,
        }

        public static string VoipCore = "asterisk";

        public static bool ForceReload { get; set; } = true;
        public static bool ForceReloadAzan { get; set; } = true;

        public static void CreateAgents(IEnumerable<TblAgent> agents)
        {
            var ext = File.ReadAllText($"/etc/{VoipCore}/extensions.conf", Encoding.ASCII);
            if (!ext.Contains("[pline-page]"))
            {
                var extentions = "[pline-page]\n" +
                                 "exten => _*0000,1,NoOp(${users},${vol})\n" +
                                 "\tsame => n,Set(VOLUME(TX)=${vol})\n" +
                                 "\tsame => n,Set(VOLUME(RX)=${vol})\n" +
                                 "\tsame => n,Page(${users})\n" +
                                 "\tsame => n,Hangup()\n";
                ext += $"\n{extentions}\n";
                File.WriteAllText($"/etc/{VoipCore}/extensions.conf", ext, Encoding.ASCII);
            }

            var sip = File.ReadAllText($"/etc/{VoipCore}/sip.conf", Encoding.ASCII);
            if (!sip.Contains("#include sip-pline-pager.conf"))
            {
                sip += "\n#include sip-pline-pager.conf\n";
                File.WriteAllText($"/etc/{VoipCore}/sip.conf", sip, Encoding.ASCII);
            }

            string plinePager = "";
            foreach (var agent in agents)
            {
                if (agent.Agent == AgentType.Sip && agent.Enable)
                {
                    plinePager += $"[{agent.Username}]\n";
                    plinePager += $"username={agent.Username}\n";
                    plinePager += $"secret={agent.Password}\n";
                    plinePager += "type=friend\n";
                    plinePager += "host=dynamic\n";
                    plinePager += "disallow=all\n";
                    plinePager += "call-limit=1\n";
                    plinePager += "allow=ulaw,ulaw,gsm\n";
                    plinePager += "\n";
                }
            }

            File.WriteAllText($"/etc/{VoipCore}/sip-pline-pager.conf", plinePager, Encoding.ASCII);
            RunCmd($"/usr/sbin/{VoipCore}", "-x reload");
        }

        public static string RunCmd(string app, string args)
        {
            try
            {
                var p = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = app,
                        Arguments = $" {args}",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
                p.Start();
                string output = p.StandardOutput.ReadToEnd();
                return output;
            }
            catch
            {
                return null;
            }
        }

        public static bool Mp3ToWav(string mp3, string wav, bool deleteMp3)
        {
            try
            {
                const string cmd = "/usr/bin/ffmpeg";
                var args = $" -i {mp3} -ar 8000 -ac 1 -acodec pcm_s16le {wav}";
                if (File.Exists(wav)) File.Delete(wav);
                RunCmd(cmd, args);
                if (deleteMp3)
                {
                    Thread.Sleep(3000);
                    File.Delete(mp3);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string CallFileOnAgent(string sound, string agent, int volume = 0)
        {
            if (sound.EndsWith(".wav"))
            {
                sound = sound.Substring(0, sound.Length - 4);
            }

            try
            {
                var strCall = "Channel: Local/*0000@pline-page\n" +
                              "Setvar: users=" + agent + "\n" +
                              "Setvar: vol=" + volume + "\n" +
                              "CallerID: \"00000000\"<pline-page>\n" +
                              //"MaxRetries: 0\n" +
                              //"WaitTime: 45\n" +
                              //"RetryTime: 1\n" +
                              "Application: Playback\n" +
                              "Data: " + sound + "\n";
                var path = $"/var/spool/{VoipCore}/outgoing/{Globals.GenerateId()}.call";
                File.WriteAllText(path, strCall, Encoding.ASCII);
                RunCmd("/usr/bin/chmod", "0777 " + path);
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public static bool Hangup(string agent)
        {
            bool r = false;
            string result = RunCmd($"/usr/sbin/{VoipCore}", "-x \"core show channels\"");
            if (result != null && result.Trim() != string.Empty)
            {
                string[] items = result.Split(new string[] {"\n"}, StringSplitOptions.RemoveEmptyEntries);
                foreach (var i in items)
                {
                    if (i.Trim().StartsWith(agent))
                    {
                        var channels = i.Split(new string[] {" "}, StringSplitOptions.RemoveEmptyEntries);
                        if (channels.Length > 0)
                        {
                            RunCmd($"/usr/sbin/{VoipCore}", $"-x \"hangup request {channels[0].Trim()}\"");
                            r = true;
                        }
                    }
                }
            }

            return r;
        }

        public static void HangupAll()
        {
            RunCmd($"/usr/sbin/{VoipCore}", $"-x \"hangup request all\"");
        }

        public static List<T> ConvertToList<T>(DataTable dt)
        {
            var columnNames = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName.ToLower()).ToList();
            var properties = typeof(T).GetProperties();
            return dt.AsEnumerable().Select(row =>
            {
                var objT = Activator.CreateInstance<T>();
                foreach (var pro in properties)
                {
                    if (columnNames.Contains(pro.Name.ToLower()))
                    {
                        try
                        {
                            pro.SetValue(objT, row[pro.Name]);
                        }
                        catch (Exception ex)
                        {
                            //ignore
                        }
                    }
                }

                return objT;
            }).ToList();
        }

        public static DateTime AddTime(string persianDate, int hour, int minute, double seconds)
        {
            PersianCalendar p = new PersianCalendar();
            string[] parts = persianDate.Split('/', '-');
            DateTime dta = p.ToDateTime(Convert.ToInt32(parts[0]), Convert.ToInt32(parts[1]), Convert.ToInt32(parts[2]),
                hour, minute, 0, 0);
            var newDate = dta.AddSeconds(seconds);
            return newDate;
        }

        public static double PersianDateDiffToSeconds(string persianDateA, int hourA, int minuteA, string persianDateB,
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

        public static int GetWavSoundLength(string fileName)
        {
            try
            {
                string result = RunCmd("/usr/bin/soxi", $"-D \"{fileName}\"");
                var f = float.Parse(result.Trim().Replace("\n", ""));
                return (int) f;
            }
            catch
            {
                return 0;
            }
        }
    }
}