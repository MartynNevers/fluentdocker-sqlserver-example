namespace FluentDocker.SqlServer.Example.Tests.SqlServerPerTestFixture
{
    using Ductus.FluentDocker.Builders;
    using FluentDocker.SqlServer.Example.Helpers;

    [TestFixture]
    [Parallelizable(scope: ParallelScope.All)]
    internal abstract class BaseTest
    {
        // Common Config
        private const string Password = "P@ssword123";

        // SqlConnection Config
        private const string DataSource = "localhost, 14331";
        private const string UserId = "sa";

        // ContainerBuilder Config
        private const string Prefix = "mssql-db-";
        private const int HostPort = 14331;
        private const int ContainerPort = 1433;
        private static readonly string[] Images =
        {
            "mcr.microsoft.com/mssql/server:2022-CU13-ubuntu-22.04",
            "mcr.microsoft.com/mssql/server:2022-latest",
        };

        protected MyContainerService SqlContainerService { get; set; }

        protected MyDbConnection SqlConnection { get; set; }

        [OneTimeSetUp]
        public virtual void OneTimeSetUp()
        {
            // Kill any zombie SQL containers
            MyContainerService.KillZombieContainers(Prefix);

            // Start containerized SQL database
            var containerBuilder = this.GetSqlContainerBuilder();
            this.SqlContainerService = containerBuilder.Build().Start();

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
            this.SqlContainerService.Stop();
            this.SqlConnection.Dispose();
            this.SqlContainerService.Dispose();
        }

        [TearDown]
        public void TearDown()
        {
            // No teardown
        }

        private MyContainerBuilder GetSqlContainerBuilder()
        {
            var id = Guid.NewGuid();
            var builder = new Builder()
                .UseContainer()
                .WithName($"{Prefix}{id}")
                .WithHostName($"{Prefix}{id}")
                .UseImage(Images.First())
                .ExposePort(HostPort, ContainerPort)
                .WithEnvironment($"SA_PASSWORD={Password}", "ACCEPT_EULA=Y")
                .WaitForPort($"{ContainerPort}/tcp", TimeSpan.FromSeconds(30));

            // .WaitForMessageInLog("SQL Server 2022 will run as non-root by default.", TimeSpan.FromSeconds(value))
            // .WaitForMessageInLog("This container is running as user mssql.", TimeSpan.FromSeconds(value))
            return new MyContainerBuilder(builder);
        }
    }
}
