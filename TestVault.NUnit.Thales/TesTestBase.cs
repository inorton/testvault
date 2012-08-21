using System;

using TestVault.NUnit;
using NUnit.Framework;

namespace TestVault.NUnit.Thales
{

    public class TesTestBase : TestVaultTestBase
    {
        public override string TestVaultProject
        {
            get
            {
                return "project_pdc-testvault";
            }
        }

        public override string TestVaultBuildId
        {
            get
            {
                return "test-tagprefix";
            }
        }

        public override string TestVaultGroup
        {
            get
            {
                return "ngcslibs@v12_0mono210";
            }
        }

        static string fake_session = null;

        public override string TestVaultSession
        {
            get
            {
                if ( fake_session == null ){
                    var now = ((int)DateTime.Now.Subtract(new DateTime(1970,1,1)).TotalSeconds).ToString();

                    now = now.Substring( now.Length - 5 );

                    fake_session = string.Format("@{0}",now);
                }
                return fake_session;
            }
            set
            {

            }
        }
    }
}

