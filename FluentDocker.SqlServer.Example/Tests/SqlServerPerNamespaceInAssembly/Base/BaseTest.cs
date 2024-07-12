namespace FluentDocker.SqlServer.Example.Tests.SqlServerPerNamespaceInAssembly
{
    using FluentDocker.SqlServer.Example.Helpers;

    [TestFixture]
    [Parallelizable(scope: ParallelScope.Fixtures)]
    internal abstract class BaseTest
    {
        // SqlConnection Config
        private const string Password = "P@ssword123";
        private const string DataSource = "localhost, 14331";
        private const string UserId = "sa";

        protected MyDbConnection SqlConnection { get; set; }

        [OneTimeSetUp]
        public virtual void OneTimeSetUp()
        {
            // Open SQL database connection
            var connectionString = MyDbConnection.BuildConnectionString(DataSource, UserId, Password, true);
            this.SqlConnection = new MyDbConnection(connectionString);
            this.SqlConnection.Open(TimeSpan.FromSeconds(30));
        }

        [SetUp]
        public void SetUp()
        {
            // No setup
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            this.SqlConnection.Close();
            this.SqlConnection.Dispose();
        }

        [TearDown]
        public void TearDown()
        {
            // No teardown
        }
    }
}
