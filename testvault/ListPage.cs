using System;
using System.Linq;
using System.Collections.Generic;
using TestVault.Data;

namespace TestVault
{
    public class ListPage : Page
    {
        public ListPage() : base ()
        {
            Results = new Dictionary<TestGroup, List<TestResult>>();
            Projects = new List<TestProject>();
            Builds = new List<string>();
        }
        
        public TestProject Project { get; set; }

        public string BuildId { get; set; }

        public TestGroup Group { get; set; }

        public Dictionary<TestGroup,List<TestResult>> Results { get; private set; }

        public List<TestProject> Projects { get; private set; }

        public List<string> Builds { get; private set; }

        public override IEnumerable<string> Sections
        {
            get
            {
                var rv = new List<string>();

                if ( Project != null ){
                    rv.Add( Tag("h2",Project.Project) );
                }

                if ( Projects.Count > 0 ){
                    rv.Add(Tag("h2", "Projects") );
                    rv.Add( ProjectsList() );
                }

                if ( !String.IsNullOrEmpty(BuildId) ){
                    rv.Add( Tag( "h3", BuildId ) );
                }

                if ( Builds.Count > 0 ){
                    rv.Add(Tag("h2", "Builds") );
                    rv.Add( BuildsList() );
                }

                if ( Results.Count > 0 ){
                    rv.Add(Tag("h2", "Test Results") );
                    rv.Add( ResultsTable() );
                }

                return rv;
            }
        }

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
            var alltests = new List<TestResult>();
            foreach (var tl in Results.Values)
            {
                alltests.AddRange(tl);
            }

            // limit to 50 sessions displayed at once.
            var sessions = ( from t in alltests select t.TestSession ).OrderByDescending( x => x ).Distinct().Take(50);

            var rows = new List<string>();

            foreach (var grp in Results.Keys)
            {
                rows.Add( Tag("tr", Tag("td,colspan:2", Tag("span.testgroupname",grp.Name) )));

                var names = (from n in Results[grp] select n.Name).OrderBy(x=>x).Distinct();

                foreach ( var tname in names ){

                    var row = new List<string>();

                    row.Add(Tag("td.testname", tname));

                    var outcomes = new List<string>();

                    foreach ( var s in sessions ){
                        var tsess = from t in Results[grp] where ( t.Name == tname ) && ( t.TestSession == s ) select t;
                        if ( tsess.Count() == 0 ){
                            outcomes.Add("-");
                        } else {
                            var oc = tsess.FirstOrDefault().Outcome;
                            var code = oc.Code();
                            var oclass = oc.ToString().ToLower();
                            outcomes.Add(Tag(string.Format("span.{0}",oclass),code));
                        }
                    }
                    row.Add(Tag("td.testresults,align:left",Tag("pre",string.Join("",outcomes.ToArray()))));

                    rows.Add(Tag("tr.testrow", row.ToArray()));
                }
            }
            return Tag("table", rows.ToArray());

        }
    }
}

