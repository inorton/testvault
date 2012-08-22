using System;
using TestVault.Mono.Options;
using TestVault.Core;
using TestVault.Data;
namespace TestVault
{
    class MainClass
    {
        static bool AnyNull(params string[] x)
        {
            foreach (var n in x)
            {
                if ( string.IsNullOrEmpty(n) ) return true;
            }
            return false;
        }

        static OptionSet opts = new OptionSet();

        public static void Main(string[] args)
        {
            string tvserver = null;
            string tvproject = null;
            string tvgroup = null;
            string tvbuildid = null;
            string tvsession = null;

            string tvname = null;
            string tvoutcome = null;
            bool tvpersonal = false;

            int exitcode = 0;

            opts.Add("help|h", "Print this message",
                     x => {

                Console.Error.WriteLine("Usage: testvault_submit [OPTIONS]");
                opts.WriteOptionDescriptions(Console.Error);
                Environment.Exit(exitcode);
            });

            opts.Add("server=", "the testvault server address", x => tvserver = x);
            opts.Add("project=", "The Project name", x => tvproject = x);
            opts.Add("group=", "The Group/Component name", x => tvgroup = x);
            opts.Add("build=", "The Build name", x => tvbuildid = x);
            opts.Add("session=", "The ID of this test session", x => tvsession = x);
            opts.Add("name=", "The test name", x => tvname = x);
            opts.Add("outcome=", "The test result (Passed,Failed,Unknown,Indeterminate)", x => tvoutcome = x);
            opts.Add("personal=", "The test is a private test", x => tvpersonal = true);
            
            opts.Parse(args);

            if (AnyNull(tvserver, tvgroup, tvbuildid, tvserver, tvname, tvserver, tvoutcome))
            {
                Console.Error.WriteLine("missing required options");
                exitcode = 1;
                opts.Parse(new string[]{"--help"});
            }
            var outcome = (TestOutcome)Enum.Parse(typeof(TestOutcome), tvoutcome);

            TestVaultUtils.SubmitResult( new Uri(tvserver),
                                        tvsession, tvproject, tvbuildid, tvgroup, tvname, outcome, tvpersonal );
        }
    }
}
