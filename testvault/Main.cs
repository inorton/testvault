using System;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Xml.Serialization;
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

                if (req.HttpMethod.ToUpper() == "POST")
                {
                    SubmitResult(ctx);
                } else
                {
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
            if ( !req.HttpMethod.ToUpper().Equals("POST") ) throw new InvalidOperationException("POST expected");

            var resp = ctx.Response;
            resp.StatusCode = 500;

            var sb = new StringBuilder();

            try {
                var xsc = new XmlSerializer(typeof(TestResult));
                var tr = (TestResult)xsc.Deserialize(req.InputStream);

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
