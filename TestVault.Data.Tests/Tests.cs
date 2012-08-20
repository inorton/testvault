using System;
using NUnit.Framework;
using TestVault.Data;
using TestVault.Data.SQLite;

namespace TestVault.Data.Tests
{
    public class SQLite
    {
        string buildid = "testvault-tests-001";
        TestProject project = new TestProject(){ Project = "testvault-tests" };
        string[] groups = new string[]{ "comp1", "comp2" };

        ITestVaultData db = null;

        [SetUp]
        public void SetUp()
        {
            if ( db == null )
                db = new SQLiteTestData();
        }

        [Test]
        [Repeat(10)]
        public void Insert()
        {
            foreach (var c in groups)
            {
                var result = new TestResult();
                var tn = "Foo.Whatever.Test_" + Guid.NewGuid().ToString().Substring(0, 6);
                result.Name = tn;
                result.Group = new TestGroup() { Project = project, Name = c };
                result.Outcome = TestOutcome.Passed;
                result.Time = DateTime.Now;
                result.BuildID = buildid;

                db.Save( result );
            }
        }

        [Test]
        [Repeat(100)]
        public void ListProjects()
        {
            var prjs = db.GetProjects();
            Assert.Greater( prjs.Count, 0 );
        }

        [Test]
        [Repeat(100)]
        public void ListProjectsGroups()
        {
            var prjs = db.GetProjects();
            Assert.Greater(prjs.Count, 0);
            foreach (var p in prjs)
            {
                var grps = db.GetGroups(p);
                Assert.Greater( grps.Count, 0 );
            }
        }

        [Test]
        public void Wipe()
        {
            foreach (var p in db.GetProjects())
            {
                db.Delete( p );
            }
        }
    }
}

