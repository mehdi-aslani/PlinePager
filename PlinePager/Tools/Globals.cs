using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using PlinePager.Data;

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
                        RedirectStandardOutput = false,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
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
                              "MaxRetries: 0\n" +
                              "WaitTime: 45\n" +
                              "RetryTime: 1\n" +
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

        public static void Hangup(string agent)
        {
            string result = RunCmd("/usr/sbin/asterisk", "-x 'core show channels'");
            Console.WriteLine(result);
        }
    }
}