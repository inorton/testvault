using System;
using System.Collections.Generic;

namespace TestVault.Data
{
    public interface ITestVaultData
    {
        List<TestProject> GetProjects();

        List<TestGroup> GetGroups( TestProject project );

        List<TestResult> GetResults ( TestGroup grp );

        List<TestResult> GetResults ( TestGroup grp, string buildId );

        List<string> GetBuilds( TestProject project, TestGroup grp, DateTime? since, DateTime? until );

        void Save( TestResult result );

        void Delete( string buildid );

        void Delete ( TestProject project );

        void Clean(DateTime olderThan, bool personal_only);

    }

}

