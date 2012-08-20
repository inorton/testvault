using System;
using System.Runtime.Serialization;

namespace TestVault.Data
{
    [Serializable]
    public class TestProject
    {
        public string Project { get; set; }

        public ITestVaultData DataStore { get; set; }
    }
}

