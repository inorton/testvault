using System;
using System.Net;
using System.Text;
using System.Collections.Generic;

using TestVault.Core;
using TestVault.Data;

namespace TestVault
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var w = new WebServer();
            w.Run();
        }
    }

    public class WebServer {

        ITestVaultData DataStore { get; set; }

        public void Run()
        {
            var l = new HttpListener();
            l.Prefixes.Add("http://+:33333/");


            l.Start();

            DataStore = new TestStore().Open(Backend.SQLite);

            while (true)
            {
                var ctx = l.GetContext();
                HandleRequest( ctx );
            }

        }

        public static Dictionary<string,string> QueryDictionary(HttpListenerRequest req)
        {
            var rv = new Dictionary<string,string>();
            for ( int i = 0; i < req.QueryString.Count; i++ ){
                var k = req.QueryString.GetKey(i);
                var v = req.QueryString.GetValues(i) as string[] ;
                rv[k] = string.Join(",",v);
            } 
            return rv;
        }

        public void HandleRequest(HttpListenerContext ctx)
        {
            var req = ctx.Request;
            var p = QueryDictionary(req);
            if (p.ContainsKey("add"))
            {
                SubmitResult(ctx);
            } else
            {
                List(ctx);
            }
        }

        public void List(HttpListenerContext ctx)
        {
            var page = new Page() { Title = "Results List", Heading = "Test Results" };

            var args = QueryDictionary(ctx.Request);

            if (!args.ContainsKey("project"))
            {
                var prs = DataStore.GetProjects();
                page.Projects.AddRange(prs);
            } 

            var buf = Encoding.UTF8.GetBytes(page.ToString());
            var resp = ctx.Response;
            resp.StatusCode = 200;

            resp.OutputStream.Write(buf, 0, buf.Length);
            resp.OutputStream.Close();
        }

        public void SubmitResult( HttpListenerContext ctx )
        {

            var resp = ctx.Response;
            resp.StatusCode = 500;

            var args = QueryDictionary(ctx.Request);
            var sb = new StringBuilder();
            try {
                var project = new TestProject(){ Project = args["project"], DataStore = DataStore };
                var pgroup = new TestGroup() { Project = project, Name = args["group"] };
                var build = args["buildid"];
                var name = args["name"];
                var date = DateTime.Now;
                var outc = args["outcome"];
                var outcome = (TestOutcome)Enum.Parse(typeof(TestOutcome), outc);



                var tr = new TestResult(){ 
                    Group = pgroup,
                    Time = date,
                    BuildID = build,
                    Name = name,
                    Outcome = outcome 
                };

                if ( args.ContainsKey("desc") ){
                    tr.Description = args["desc"];
                }
                if ( args.ContainsKey("note") ){
                    tr.Notes = args["note"];
                }
                if ( args.ContainsKey("personal") )
                {
                    bool pers = false;
                    bool.TryParse( args["personal"], out pers );
                    tr.IsPersonal = pers;
                }

                DataStore.Save(tr);

                sb.AppendFormat("OK");
                resp.StatusCode = 200;
            } catch ( KeyNotFoundException e ){
                sb.AppendFormat("one or more test params not specified \n: {0}", e.ToString());
            }

            var buf = Encoding.UTF8.GetBytes(sb.ToString());

            resp.OutputStream.Write(buf, 0, buf.Length);
            resp.OutputStream.Close();
        }
    }
}
