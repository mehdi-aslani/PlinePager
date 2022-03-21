using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
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
    }
}