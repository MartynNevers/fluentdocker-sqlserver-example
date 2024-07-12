namespace FluentDocker.SqlServer.Example.Tests.SqlServerPerTestFixture
{
    using Ductus.FluentDocker.Extensions;
    using Ductus.FluentDocker.Services;

    [TestFixture]
    internal class SqlDockerTests : BaseTest
    {
        [Test]
        public void WhenContainerIsBuilt_ThenContainerIsNamedPrecisely()
        {
            var config = this.SqlContainerService.ContainerService.GetConfiguration(true);
            Assert.That(config.Name, Does.Contain("mssql-db-"));
            Assert.That(config.Config.Hostname, Does.Contain("mssql-db-"));
        }

        [Test]
        public void WhenContainerIsStarted_ThenContainerIsRunning()
        {
            var config = this.SqlContainerService.ContainerService.GetConfiguration(true);
            Assert.That(config.State.ToServiceState(), Is.EqualTo(ServiceRunningState.Running));
        }

        [Test]
        public void WhenContainerIsStarted_ThenAnticipatedLogsAreGenerated()
        {
            var logs = this.SqlContainerService.Logs;
            Assert.Multiple(() =>
            {
                Assert.That(logs, Has.Exactly(1).Matches("SQL Server 2022 will run as non-root by default."));
                Assert.That(logs, Has.Exactly(1).Matches("This container is running as user mssql."));
                Assert.That(logs, Has.Exactly(1).Matches<string>(l => l.Contains("To learn more visit ")));
            });
        }

        [Test]
        public void WhenDbConnectionIsOpened_ThenClientConnectionIdIsAValidGuid()
        {
            var clientConnectionId = ((Microsoft.Data.SqlClient.SqlConnection)this.SqlConnection.DbConnection).ClientConnectionId;
            Assert.That(clientConnectionId, Is.Not.Empty);
        }
    }
}
