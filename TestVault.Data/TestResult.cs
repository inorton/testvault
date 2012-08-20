using System;
using System.Runtime.Serialization;

namespace TestVault.Data
{
    [Serializable]
    public class TestResult
    {
        public DateTime Time { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public TestOutcome Outcome { get; set; }

        public string Notes { get; set; }

        public TestProject Project { get { return Group.Project; } }

        public TestGroup Group { get; set; }

        public string BuildID { get; set; }

        public bool IsPersonal { get; set; }

    }
}

