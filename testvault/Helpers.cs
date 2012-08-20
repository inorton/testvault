using System;
using System.Collections.Generic;
using System.Linq;

namespace TestVault
{
    public static class Helpers
    {
        public static DateTime? GetDateTime(this Dictionary<string,string> args, string name)
        {
            DateTime? rv = null;
            DateTime tmp_rv;
            string tmp = null;
            if (args.TryGetValue(name, out tmp))
            {
                if ( DateTime.TryParse(tmp, out tmp_rv) ){
                    rv = tmp_rv;
                }
            }
            return rv;
        }

        public static string GetString(this Dictionary<string, string> args, string name)
        {
            string tmp = null;
            args.TryGetValue(name, out tmp);
            return tmp;
        }

        public static bool? GetBool(this Dictionary<string,string> args, string name)
        {
            bool? rv = null;
            bool tmp_rv;
            string tmp = null;
            if (args.TryGetValue(name, out tmp))
            {
                if ( bool.TryParse(tmp, out tmp_rv) ){
                    rv = tmp_rv;
                }
            }
            return rv;
        }
    }
}

