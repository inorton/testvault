using System;
using System.Runtime.Serialization;

namespace TestVault.Data
{
    [Serializable]
    public class TestProject
    {
        public string Project { get; set; }

        [System.Xml.Serialization.XmlIgnore]
        public ITestVaultData DataStore { get; set; }
    }
}

