using System;
using System.Collections.Generic;
using System.IO;
using TestVault.Data;
using Mono.Data.Sqlite;

namespace TestVault.Data.SQLite
{
    public class SQLiteTestData : ITestVaultData
    {

        public SQLiteTestData()
        {

        }

        static bool initCalled = false;

        SqliteConnection Connect()
        {
            var dbpath = ".";
            var db = Path.Combine(dbpath, "data.sqlite");

            if (!Directory.Exists(dbpath))
                Directory.CreateDirectory(dbpath);

            var conn = new SqliteConnection();
            conn.ConnectionString = String.Format ("URI=file:{0},Version=3,encoding=UTF-8", db);
            conn.Open ();

            if ( !initCalled )
                InitTables( conn );

            return conn;
        }

        void NonQuery(SqliteConnection conn, string sql)
        {
            using (var cmd = new SqliteCommand(conn))
            {
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }

        void InitTables(SqliteConnection conn)
        {

            NonQuery(conn,@"CREATE TABLE IF NOT EXISTS
results (
 Name text, 
 Project text,
 Description text,
 Outcome integer,
 Notes text,
 TestGroup text,
 BuildId text,
 IsPersonal boolean,
 Timestamp datetime
)");

            NonQuery(conn,@"CREATE INDEX IF NOT EXISTS 
names_idx ON results (
 Project, 
 TestGroup 
)");

            NonQuery(conn,@"CREATE INDEX IF NOT EXISTS 
builds_idx ON results (
 BuildId 
)");



            initCalled = true;
        }

        List<string> GetDistinct( SqliteConnection conn, string column)
        {
            List<string> rv = new List<string>();
            using ( var cmd = new SqliteCommand( conn ) ){
                cmd.CommandText = @"SELECT DISTINCT " + column + " FROM results ORDER BY Timestamp DESC";
                using ( var sth = cmd.ExecuteReader() ){
                    while ( sth.Read() ){
                        var str = sth.GetString(0);
                        rv.Add(str);
                    }
                }
            }
            return rv;
        }

        #region ITestVaultData implementation
        public List<TestProject> GetProjects()
        {
            var rv = new List<TestProject>();
            using (var conn = Connect())
            {
                var projects = GetDistinct( conn, "Project" );
                foreach ( var p in projects )
                {
                    var proj = new TestProject();
                    proj.DataStore = this;
                    proj.Project = p;
                    rv.Add(proj);
                }
            }

            return rv;
        }

        public List<TestGroup> GetGroups(TestProject project)
        {
            var rv = new List<TestGroup>();
            using (var conn = Connect())
            {
                using ( var cmd = new SqliteCommand( conn ) ){
                    cmd.CommandText = @"SELECT DISTINCT TestGroup FROM results WHERE Project = :PROJ ORDER BY Timestamp DESC";
                    cmd.Parameters.Add( new SqliteParameter( ":PROJ", project.Project ) );
                    using ( var sth = cmd.ExecuteReader() ){
                        while ( sth.Read() ){
                            var g = new TestGroup();
                            g.Project = project;
                            g.Name = sth.GetString(0);
                            rv.Add(g);
                        }
                    }
                }
            }

            return rv;
        }

        public List<TestResult> GetResults(TestGroup tgroup,  string buildId)
        {

            var rv = new List<TestResult>();
            using (var conn = Connect())
            {
                using ( var cmd = new SqliteCommand( conn ) ){
                    var build = "";
                    if ( !string.IsNullOrEmpty(buildId) ){
                        build = @"AND BuildId = :BID";
                        cmd.Parameters.Add( new SqliteParameter( ":BID", buildId ) );
                    }

                    cmd.Parameters.Add( new SqliteParameter( ":PROJ", tgroup.Project ) );
                    cmd.Parameters.Add( new SqliteParameter( ":GRP", tgroup.Name ) );

                    cmd.CommandText = @"SELECT 
 Name, 
 Outcome,
 Description,
 Notes,
 Timetamp,
 IsPersonal
FROM results 
 WHERE Project = :PROJ 
 AND TestGroup = :GRP " + build + " ORDER BY Timestamp DESC";

                    using ( var sth = cmd.ExecuteReader() ){
                        while ( sth.Read() ){
                            var t = new TestResult();
                            t.Group = tgroup;
                            t.Name = sth.GetString(0);
                            t.Outcome = (TestOutcome) sth.GetInt32(1);
                            t.Description = sth.GetString(2);
                            t.Notes = sth.GetString(3);
                            t.Time = sth.GetDateTime(4);
                            t.IsPersonal = sth.GetBoolean(5);

                            rv.Add(t);
                        }
                    }
                }
            }

            return rv;
        }


        public List<TestResult> GetResults(TestGroup grp)
        {
            return GetResults( grp, null);
        }

        public List<string> GetBuilds(TestProject project, TestGroup grp, DateTime? since, DateTime? until)
        {
            var rv = new List<string>();
            using (var conn = Connect())
            {
                if ((project == null) && (grp == null) && (since == null) && (until == null))
                {
                    rv = GetDistinct(conn, "BuildId");
                } else {

                    using ( var cmd = new SqliteCommand( conn ) )
                    {
                        List<string> constraints = new List<string>();
                        if ( project != null ){
                            constraints.Add( "Project = :PROJ" );
                            cmd.Parameters.Add( new SqliteParameter(":PROJ", project.Project ));
                        }

                        if ( grp != null ){
                            constraints.Add( "TestGroup = :GRP" );
                            cmd.Parameters.Add( new SqliteParameter(":GRP", grp.Name) );
                        }
                        string conds = "";
                        if ( constraints.Count > 0 )
                            conds = " WHERE " + string.Join(" AND ", constraints.ToArray() );

                        cmd.CommandText = string.Format("SELECT BuildId, Timestamp FROM results {0}", conds);

                        using ( var sth = cmd.ExecuteReader() ){
                            var buildid = sth.GetString(0);
                            var buildtime = sth.GetDateTime(1);
                            if ( ( since == null ) || ( buildtime > since.Value ) ){
                                if ( ( until == null ) || ( buildtime <= until.Value ) ){
                                    rv.Add( buildid );
                                }
                            }
                        }
                    }
                }
            }
            return rv;
        }

        public void Save(TestResult result)
        {
            using (var conn = Connect())
            {
                using ( var cmd = new SqliteCommand( conn ) ){
                    cmd.CommandText = @"REPLACE INTO results ( 
 Name,
 Outcome,
 Description,
 Notes,
 Timestamp,
 Project,
 TestGroup,
 BuildId,
 IsPersonal
) 
VALUES (
 :NAME,
 :OUTC,
 :DESC,
 :NOTE,
 :TIME,
 :PROJ,
 :GRP,
 :BUILD,
 :PERS
)";
                    cmd.Parameters.Add( new SqliteParameter( ":NAME", result.Name ) );
                    cmd.Parameters.Add( new SqliteParameter( ":OUTC", (int)result.Outcome ) );
                    cmd.Parameters.Add( new SqliteParameter( ":DESC", result.Description ) );
                    cmd.Parameters.Add( new SqliteParameter( ":NOTE", result.Notes ) );
                    cmd.Parameters.Add( new SqliteParameter( ":TIME", result.Time ) );
                    cmd.Parameters.Add( new SqliteParameter( ":PROJ", result.Group.Project.Project ) );
                    cmd.Parameters.Add( new SqliteParameter( ":GRP", result.Group.Name ) );
                    cmd.Parameters.Add( new SqliteParameter( ":BUILD", result.BuildID ) );
                    cmd.Parameters.Add( new SqliteParameter( ":PERS", result.IsPersonal ) );

                    cmd.ExecuteNonQuery();
                }
            }
        }


        public void Delete( string buildid )
        {
            var sql = "DELETE FROM results WHERE BuildId = :BUILD";
            using (var conn = Connect())
            using (var cmd = new SqliteCommand(conn))
            {
                cmd.Parameters.Add(new SqliteParameter(":BUILD", buildid));
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete ( TestProject project )
        {
            var sql = "DELETE FROM results WHERE Project = :PROJ";
            using (var conn = Connect())
            using (var cmd = new SqliteCommand(conn))
            {
                cmd.Parameters.Add(new SqliteParameter(":PROJ", project.Project));
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }

        public void Clean(DateTime olderThan, bool personalOnly)
        {
            var sql = "DELETE FROM results WHERE Timestamp < :TIME";
            using (var conn = Connect())
            using (var cmd = new SqliteCommand(conn))
            {
                cmd.Parameters.Add(new SqliteParameter(":TIME", olderThan));
                cmd.CommandText = sql;

                if ( personalOnly ){
                    sql += " AND IsPersonal = TRUE ";
                }

                cmd.ExecuteNonQuery();
            }
        
        }

        #endregion
    }
}


