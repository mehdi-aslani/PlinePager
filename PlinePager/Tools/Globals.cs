using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;
using PlinePager.Data;
using PlinePager.Models;

namespace PlineFaxServer.Tools
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

        public static bool ForceReload { get; set; } = true;

        public static void CreateAgents(IEnumerable<TblAgent> agents)
        {
            var ext = File.ReadAllText("/etc/asterisk/extensions.conf", Encoding.ASCII);
            if (!ext.Contains("[pline-page]"))
            {
                var extentions = "[pline-page]\n" +
                                 "exten => _*0000,1,NoOp(${users},${vol})\n" +
                                 "\tsame => n,Set(VOLUME(TX)=${vol})\n" +
                                 "\tsame => n,Set(VOLUME(RX)=${vol})\n" +
                                 "\tsame => n,Page(${users})\n" +
                                 "\tsame => n,Hangup()\n";
                ext += $"\n{extentions}\n";
                File.WriteAllText("/etc/asterisk/extensions.conf", ext, Encoding.ASCII);
            }

            var sip = File.ReadAllText("/etc/asterisk/sip.conf", Encoding.ASCII);
            if (!sip.Contains("#include sip-pline-pager.conf"))
            {
                sip += "\n#include sip-pline-pager.conf\n";
                File.WriteAllText("/etc/asterisk/sip.conf", sip, Encoding.ASCII);
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

            File.WriteAllText("/etc/asterisk/sip-pline-pager.conf", plinePager, Encoding.ASCII);
            RunCmd("/usr/sbin/asterisk", "-x reload");
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
                var p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = cmd;
                p.StartInfo.Arguments = args;
                p.Start();
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
                var path = $"/var/spool/asterisk/outgoing/{Globals.GenerateId()}.call";
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
            string result = RunCmd("/usr/sbin/asterisk", "-x \"core show channels\"");
            if (result != null && result.Trim() != string.Empty)
            {
                string[] items = result.Split(new string[] {"\n"}, StringSplitOptions.RemoveEmptyEntries);
                foreach (var i in items)
                {
                    if (i.StartsWith(agent))
                    {
                        var channels = i.Split(new string[] {" "}, StringSplitOptions.RemoveEmptyEntries);
                        if (channels.Length > 0)
                        {
                            RunCmd("/usr/sbin/asterisk", $"-x \"hangup request {channels[0].Trim()}\"");
                            r = true;
                        }
                    }
                }
            }

            return r;
        }
    }
}