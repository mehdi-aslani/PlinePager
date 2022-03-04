using System;
using System.Net.Http;
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
            SIP = 0,
            CONSOLE = 1,
        }
    }
}