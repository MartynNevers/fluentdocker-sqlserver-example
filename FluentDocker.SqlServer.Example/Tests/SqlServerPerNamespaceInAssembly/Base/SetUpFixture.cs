namespace FluentDocker.SqlServer.Example.Tests.SqlServerPerNamespaceInAssembly
{
    using Ductus.FluentDocker.Builders;
    using FluentDocker.SqlServer.Example.Helpers;

    /// <summary>
    /// <a href="https://docs.nunit.org/articles/nunit/writing-tests/attributes/setupfixture">SetUpFixtureAttribute</a>
    /// </summary>
    [SetUpFixture]
    internal class SetUpFixture
    {
        // ContainerBuilder Config
        private const string Password = "P@ssword123";
        private const string Prefix = "mssql-db-";
        private const int HostPort = 14331;
        private const int ContainerPort = 1433;
        private static readonly string[] Images =
        {
            "mcr.microsoft.com/mssql/server:2022-CU13-ubuntu-22.04",
            "mcr.microsoft.com/mssql/server:2022-latest",
        };

        private static MyContainerService sqlContainerService;

        public static MyContainerService SqlContainerService
        {
            get
            {
                return sqlContainerService;
            }
        }

        [OneTimeSetUp]
        public virtual void OneTimeSetUp()
        {
            // Kill any zombie SQL containers
            MyContainerService.KillZombieContainers(Prefix);

            // Start containerized SQL database
            var containerBuilder = GetSqlContainerBuilder();
            sqlContainerService = containerBuilder.Build().Start();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            sqlContainerService.Stop();
            sqlContainerService.Dispose();
        }

        private static MyContainerBuilder GetSqlContainerBuilder()
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

            return new MyContainerBuilder(builder);
        }
    }
}
