using System;
using System.Runtime.Serialization;

namespace TestVault.Data
{
    [Serializable]
    public class TestGroup
    {
        public string Name { get; set; }
        public TestProject Project { get; set; }
    }
}

