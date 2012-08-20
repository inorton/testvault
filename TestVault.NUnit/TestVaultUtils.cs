using System;
using System.Net;
using TestVault.Data;
using System.Security;

namespace TestVault.NUnit
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
                    client.QueryString.Add( "add", "1" );
                    client.QueryString.Add( "project", SecurityElement.Escape( project ) );
                    client.QueryString.Add( "buildid", SecurityElement.Escape( buildname ) );
                    client.QueryString.Add( "group", SecurityElement.Escape( testgroup ) );
                    client.QueryString.Add( "name", SecurityElement.Escape( testname ) );
                    client.QueryString.Add( "outcome", SecurityElement.Escape( outcome.ToString() ) );
                    client.QueryString.Add( "session", SecurityElement.Escape( session ) );
                    client.QueryString.Add( "personal", SecurityElement.Escape( personal.ToString() ) );

                    client.UploadValues( testVaultServer.ToString(), client.QueryString );
                }
            } catch 
            {
               
            }
        }
    }
}

