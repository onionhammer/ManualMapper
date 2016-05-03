using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wivuu.ManualMapper.Tests.Domain;
using Wivuu.ManualMapper.Tests.Migrations;

namespace Wivuu.ManualMapper.Tests.Base
{
    [TestClass]
    public class TestInitialization
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            Database.SetInitializer(
                new MigrateDatabaseToLatestVersion<TestDataContext, Configuration>());
        }
    }

    public abstract class DatabaseTests
    {
        #region Properties

        internal TestDataContext Db { get; set; }

        internal DbContextTransaction Transaction { get; set; }

        #endregion

        #region Tests

        public virtual void TestSetup() { }

        #endregion

        #region Setup

        [TestInitialize]
        public void TestStart()
        {
            this.Db = new TestDataContext();
            this.Transaction = Db.Database.BeginTransaction();

            this.TestSetup();

            this.Db.SaveChanges();
        }

        #endregion

        #region Teardown

        [TestCleanup]
        public void TestEnd()
        {
            Transaction.Rollback();
            this.Db.Dispose();
        }

        #endregion
    }
}
