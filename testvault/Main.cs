using System;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Xml.Serialization;
using TestVault.Core;
using TestVault.Data;

using TestVault.Mono.Options;

namespace TestVault
{
    class MainClass
    {
        static OptionSet opts;

        public static void Help()
        {
            Console.Error.WriteLine("Usage {0} [OPTIONS]", "testvault" );

            opts.WriteOptionDescriptions( Console.Error );

        }

        public static void Main(string[] args)
        {
            var w = new WebServer();

            opts = new OptionSet();
            opts.Add("help|h", "Print this message", x => {
                Help();
                Environment.Exit(0);
            } );

            opts.Add("port=", "Listen on this port", (int x) => { w.ServerPort = x; } );

            opts.Parse( args );

            w.Run();
        }


    }

    public class WebServer {

        ITestVaultData DataStore { get; set; }

        public int ServerPort = 33333;

        public void Run()
        {
            var l = new HttpListener();
            l.Prefixes.Add("http://+:" + ServerPort.ToString() + "/");


            l.Start();

            Console.Error.WriteLine("listening on port {0}",ServerPort);

            DataStore = new TestStore().Open(Backend.SQLite);

            Console.Error.WriteLine("waiting for requests");

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
                if ( !string.IsNullOrEmpty(k) ){
                    var v = req.QueryString.GetValues(i) as string[] ;
                    rv[k] = string.Join(",",v);
                }
            } 

            return rv;
        }

        public void HandleRequest(HttpListenerContext ctx)
        {
            var req = ctx.Request;

            var p = QueryDictionary(req);

            try
            {

                if ((req.HttpMethod.ToUpper() == "POST") || p.ContainsKey("add"))
                {
                    SubmitResult(ctx);
                } else
                {
                    Console.Error.WriteLine( "request: {0}", req.Url );
                    var want = p.GetString("want");

                    switch ( want ){
                        case "css":
                            Css(ctx);
                            break;

                        default:
                            List(ctx);
                            break;
                    }
                }
            } catch (Exception e)
            {
                Console.Error.WriteLine(e);
                try {
                    var resp = ctx.Response;
                    resp.StatusCode = 500;

                    var error = Page.Tag("html",
                                    Page.Tag("head", Page.Tag("title","Server Error")),
                                    Page.Tag("body", Page.Tag("h1", "500 Server Error"), Page.Tag("hr"), Page.Tag("pre",e.ToString()) ) );
                    var buf = Encoding.UTF8.GetBytes(error);

                    resp.OutputStream.Write( buf, 0, buf.Length );
                    resp.Close();

                } catch {}
            }
        }

        public void Css(HttpListenerContext ctx)
        {
            var resp = ctx.Response;
            var css = @"

.header {
 background-color: #ccccff;
}

span.failed {
 background-color: red;
 color: black;
}

span.ignored {
 background-color: yellow;
 color: black;
}

span.passed {
 background-color: green;
 color: black;
}


span.testgroupname {
 font-size: 1.1em;
 font-weight: bold;
}

";

            var buf = Encoding.UTF8.GetBytes(css);

            resp.OutputStream.Write(buf, 0, buf.Length);
            resp.OutputStream.Close();
        }

        public void List(HttpListenerContext ctx)
        {
            var page = new ListPage() { Title = "Results List", Heading = "Test Results" };


            var args = QueryDictionary(ctx.Request);
            page.StyleSheet = "?want=css";
            page.Params = args;

            DateTime? since = args.GetDateTime("since");
            DateTime? until = args.GetDateTime("until");
            page.BuildId = args.GetString("buildid");

            var project = args.GetString("project");

            if (string.IsNullOrEmpty(project))
            {
                page.Projects.AddRange(DataStore.GetProjects());
            } else
            {
                page.Project = new TestProject() { Project = project };
                if ( string.IsNullOrEmpty(page.BuildId) )
                {
                    page.Builds.AddRange( DataStore.GetBuilds( page.Project, null, since, until ) );
                } else {
                    // get the tests for this project at this build
                    var grps = DataStore.GetGroups( page.Project );
                    foreach ( var grp in grps ){
                        var tests = DataStore.GetResults( grp, page.BuildId );
                        if ( tests.Count > 0 ){
                            page.Results[grp] = new List<TestResult>();
                            page.Results[grp].AddRange( tests );
                        }
                    }
                }
            }

            var buf = Encoding.UTF8.GetBytes(page.ToString());
            var resp = ctx.Response;
            resp.StatusCode = 200;

            resp.OutputStream.Write(buf, 0, buf.Length);
            resp.OutputStream.Close();
        }

        public void SubmitResult( HttpListenerContext ctx )
        {
            var req = ctx.Request;
            var args = QueryDictionary(ctx.Request);
            
            var resp = ctx.Response;
            resp.StatusCode = 500;

            var sb = new StringBuilder();

            try {
                TestResult tr;
                if ( req.HttpMethod.ToUpper().Equals("POST") ) {
                    var xsc = new XmlSerializer(typeof(TestResult));
                    tr = (TestResult)xsc.Deserialize(req.InputStream);
                } else {
                    var project = new TestProject()
                    { 
                        Project = args["project"], 
                        DataStore = DataStore
                    };
                    var pgroup = new TestGroup() { 
                        Project = project, Name = args["group"] };
                    var build = args["buildid"];
                    var name = args["name"];
                    var date = DateTime.Now;
                    var outc = args["outcome"];
                    var sess = args["session"];
                    var outcome = (TestOutcome)Enum.Parse(typeof(TestOutcome), outc);

                    tr = new TestResult(){ 
                        Group = pgroup,
                        Time = date,
                        BuildID = build,
                        Name = name,
                        Outcome = outcome,
                        TestSession = sess,
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
