#region USING_DIRECTIVES
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace TheGodfatherBot.Helpers
{
    public class TokenReader
    {
        public static string GetToken(string filename)
        {
            if (!File.Exists(filename))
                return null;
            else
                return File.ReadAllLines(filename)[0].Trim();
        }
    }
}