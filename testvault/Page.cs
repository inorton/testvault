using System;
using System.Text;
using System.Linq;
using System.Security;
using System.Collections.Generic;
using TestVault.Data;

namespace TestVault
{


    public class Page
    {
        public static KeyValuePair<string,string> Pair(string a, string b)
        {
            return new KeyValuePair<string, string>( a, b );
        }

        public string SelfLink(string text, params KeyValuePair<string,string>[] args)
        {
            var pl = new Dictionary<string,string>();
            foreach ( var p in Params.Keys ){
                pl[p] = Params[p];
            }
            foreach ( var x in args ){
                pl[x.Key] = x.Value;
            }
            var href = new List<string>();
            foreach ( var p in pl.Keys ){
                var pn = SecurityElement.Escape(p);
                var pv = SecurityElement.Escape(pl[p]);
                var to = string.Format("{0}={1}",pn,pv);
                href.Add(to);
            }
            var linkhref = string.Join("&amp;",href.ToArray());

            return Tag(string.Format("a,href:?{0}",linkhref),text);
        }

        public static string Tag(string tag, params string[] content)
        {
            var attribs = tag.Split(',');
            var alist = attribs.Skip(1).ToArray();

            var other_attribs = new Dictionary<string,string>();
            foreach (var a in alist)
            {
                var pair = a.Split(':');

                if (SecurityElement.IsValidAttributeName(pair [0]))
                {
                    if (SecurityElement.IsValidAttributeValue(pair [1]))
                    {
                        other_attribs [pair [0]] = pair [1];
                    }
                }

            }

            tag = attribs [0];

            var tagid = tag.Split('#');

            var id = "";
            if (tagid.Length == 2)
            {
                if ( SecurityElement.IsValidAttributeValue(tagid[1]) )
                    id = tagid [1];
            }

            var tagclass = tagid [0].Split('.');
            var tn = tagclass [0];

            var classes = tagclass.Skip(1);


            var sb = new StringBuilder();
            sb.AppendFormat("<{0}", tn.ToLower());

            if (classes.Count() > 0)
            {
                foreach (var c in classes)
                {
                    if ( SecurityElement.IsValidAttributeValue( c ))
                        sb.AppendFormat(" class=\"{0}\"", c);
                }
            }

            foreach (var a in other_attribs.Keys)
            {
                sb.AppendFormat(" {0}=\"{1}\"", a, other_attribs[a] );
            }

            if (!string.IsNullOrEmpty(id))
            {
                sb.AppendFormat(" id=\"{0}\"",id);
            }

            if (content.Length == 0)
            {
                sb.Append("/");
            } else
            {
                sb.Append(">");
                sb.AppendFormat( string.Join(" ", content ) );
                sb.AppendFormat("</{0}",tn.ToLower());
            }

            sb.Append(">");
            return sb.ToString();
        }

        public Page()
        {
            Results = new List<TestResult>();
            Projects = new List<TestProject>();
            Builds = new List<string>();
            Params = new Dictionary<string, string>();

        }

        public string Title { get; set; }

        public string Heading { get; set; }

        public TestProject Project { get; set; }

        public string BuildId { get; set; }

        public TestGroup Group { get; set; }

        public List<TestResult> Results { get; private set; }

        public List<TestProject> Projects { get; private set; }

        public List<string> Builds { get; private set; }

        public Dictionary<string,string> Params { get; private set; }

        string BuildsList()
        {
            var bl = new List<string>();
            foreach ( var b in Builds.OrderBy( x => x ) )
                bl.Add(
                    Tag("li",
                    SelfLink(b, Pair( "buildid", b ) ) ) );

            return Tag("ul",
                       bl.ToArray());
        }

        string ProjectsList()
        {
            var pl = new List<string>();
            foreach ( var p in Projects.OrderBy( x => x.Project ) )
                pl.Add( Tag("li", 
                            SelfLink( p.Project, Pair("project",p.Project ) ) ) );

            return Tag("ul",
                       pl.ToArray());
        }

        string ResultsTable()
        {

            var headings = Tag("tr",
                               Tag("th", "Group"),
                               Tag("th", "Name"),
                               Tag("th", "Outcome"));

            var rows = new List<string>();
            rows.Add(headings);


            foreach (var res in Results.OrderBy( x => x.Group.Name ) )
            {
                rows.Add( Tag("tr",
                              Tag("td",res.Group.Name),
                              Tag("td",res.Name),
                              Tag("td",res.Outcome.ToString())) );
            }



            var table = Tag("table",
                            rows.ToArray());

            return table;
        }


        public override string ToString()
        {
            var head = Tag("head",
                           Tag("title", Title));


            var preamble = Tag("div",
                               Tag("div.header#top,align:right",
                               Tag("small", String.Format("Test Vault on {0}", System.Environment.MachineName))),
                               Tag("h1", Heading)
            );

            var body = "";

            if (Projects.Count > 0)
            {
                body += ProjectsList();
            }

            if (Builds.Count > 0)
            {
                body += BuildsList();
            }

            if ( Results.Count > 0 ){
                body += ResultsTable();
            }

            return Tag("html", head,
                       Tag("body",
                preamble,
                body ) );
        }
    }
}

