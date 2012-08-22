using System;
using System.Text;
using System.Net;
using TestVault.Data;
using System.Security;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace TestVault.Data
{
    public class TestVaultUtils
    {
        public static void SubmitResult(Uri testVaultServer, string session, string project, string buildname, string testgroup, string testname, TestOutcome outcome, bool personal)
        {
            try
            {
                using ( var client = new WebClient() ){

                    client.BaseAddress = testVaultServer.ToString();
                    client.CachePolicy = new System.Net.Cache.RequestCachePolicy( System.Net.Cache.RequestCacheLevel.NoCacheNoStore );
                    client.Headers.Add("Content-Type", "text/xml");


                    var result = new TestResult() 
                    {
                        Group = new TestGroup() { Name = testgroup, Project = new TestProject() { Project = project } },
                        Name = testname,
                        Outcome = outcome,
                        TestSession = session,
                        IsPersonal = personal,
                        BuildID = buildname,
                    };

                    var xc = new XmlSerializer(result.GetType());
                    var io = new System.IO.MemoryStream();
                    xc.Serialize( io, result );

                    client.UploadData( testVaultServer.ToString(), io.ToArray() );

                }
            } catch ( Exception e )
            {
                Console.Error.WriteLine( e );
            }
        }
    }
}

