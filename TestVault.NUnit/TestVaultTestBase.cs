using System;
using NUnit.Framework;
using TestVault.Data;
using System.Collections.Generic;

namespace TestVault.NUnit
{
    public class TestVaultTestBase
    {
        public virtual Uri TestVaultServer
        {
            get
            {
                return new Uri("http://localhost:33333");
            }
        }

        public virtual string TestVaultProject 
        {
            get {
                return "Unit Tests";
            }
        }


        /// <summary>
        /// Gets the test test group.
        /// </summary>
        /// <remarks>
        /// This might be a component name, or server, or perhaps component@arch, eg,  cutils@v12_0lin1
        /// </remarks>            
        public virtual string TestVaultGroup 
        {
            get {


                return String.Format("{0}@{1}", GetType().Name, Environment.MachineName );
            }
        }

        public virtual string TestVaultBuildId 
        {
            get {
                return "local";
            }
        }

        public virtual bool TestVaultIsPersonal
        {
            get
            {
                return true;
            }
        }

        protected Dictionary<string,TestStatus> runtimeHistory = new Dictionary<string, TestStatus>();

        /// <summary>
        /// Gets or sets the test session identifier.
        /// </summary>
        /// <remarks>
        /// This might be a date or a version control identifier ( like a change number )
        /// </remarks>            
        public virtual string TestVaultSession
        { 
            get
            { 
                if ( String.IsNullOrEmpty( _testVaultSessionID ) ){
                    _testVaultSessionID = DateTime.Now.ToString("s");
                }
                return _testVaultSessionID;
            }
            set
            {
                _testVaultSessionID = value;
            }
        }

        static string _testVaultSessionID;


        [TearDown]
        public void AfterTest()
        {
            var ctx = TestContext.CurrentContext;
            var name = ctx.Test.FullName;

            if ( !runtimeHistory.ContainsKey(name) || ( runtimeHistory[name] != ctx.Result.Status ) ){

                TestOutcome outcome = TestOutcome.Unknown;

                switch (ctx.Result.Status)
                {
                    case TestStatus.Passed:
                        outcome = TestOutcome.Passed;
                        break;
                    case TestStatus.Skipped:
                        outcome = TestOutcome.Ignored;
                        break;
                    case TestStatus.Inconclusive:
                        outcome = TestOutcome.Inconclusive;
                        break;
                    default:
                        outcome = TestOutcome.Failed;
                    break;
                }
                runtimeHistory[name] = ctx.Result.Status;
                TestVaultUtils.SubmitResult( TestVaultServer, TestVaultSession, TestVaultProject, TestVaultBuildId, TestVaultGroup, name, outcome, TestVaultIsPersonal );
            }
        }

    }
}

