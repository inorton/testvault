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

        public virtual string TestVaultGroup 
        {
            get {
                return GetType().Name;
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

        protected string testVaultSession = null;

        [TearDown]
        public void AfterTest()
        {
            var ctx = TestContext.CurrentContext;
            var name = ctx.Test.FullName;

            if ( testVaultSession == null )
                testVaultSession = DateTime.Now.ToString("s");

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
                TestVaultUtils.SubmitResult( TestVaultServer, testVaultSession, TestVaultProject, TestVaultBuildId, TestVaultGroup, name, outcome, TestVaultIsPersonal );
            }
        }

    }
}

