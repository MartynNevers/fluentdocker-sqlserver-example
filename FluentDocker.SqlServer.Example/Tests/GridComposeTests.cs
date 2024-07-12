namespace FluentDocker.SqlServer.Example.Tests
{
    using System.Net;
    using Ductus.FluentDocker.Builders;
    using Ductus.FluentDocker.Services;
    using FluentDocker.SqlServer.Example.Helpers;

    /// <summary>
    /// <a href="https://github.com/SeleniumHQ/docker-selenium">docker-selenium</a>
    /// </summary>
    [TestFixture]
    internal class GridComposeTests
    {
        private const string ComposeFile = "docker-compose-v3.yml";

        private MyCompositeService gridCompositeService;

        [OneTimeSetUp]
        public virtual void OneTimeSetUp()
        {
            // Start containerized Selenium Grid
            var compositeBuilder = GetGridCompositeBuilder();
            this.gridCompositeService = compositeBuilder.Build().Start();
        }

        [Test]
        public void WhenCompositeIsStarted_ThenFourContainersAreRunning()
        {
            var containers = this.gridCompositeService.CompositeService.Containers;
            Assert.That(containers.Count, Is.EqualTo(4));
            Assert.Multiple(() =>
            {
                Assert.That(containers, Has.Exactly(1).Matches<IContainerService>(c => c.Name.Equals("selenium-hub")));
                Assert.That(containers, Has.Exactly(1).Matches<IContainerService>(c => c.Name.Contains("chrome")));
                Assert.That(containers, Has.Exactly(1).Matches<IContainerService>(c => c.Name.Contains("firefox")));
                Assert.That(containers, Has.Exactly(1).Matches<IContainerService>(c => c.Name.Contains("edge")));
                Assert.That(containers.First(c => c.Name.Equals("selenium-hub")).State, Is.EqualTo(ServiceRunningState.Running));
                Assert.That(containers.First(c => c.Name.Contains("chrome")).State, Is.EqualTo(ServiceRunningState.Running));
                Assert.That(containers.First(c => c.Name.Contains("firefox")).State, Is.EqualTo(ServiceRunningState.Running));
                Assert.That(containers.First(c => c.Name.Contains("edge")).State, Is.EqualTo(ServiceRunningState.Running));
            });
        }

        [Test]
        public void WhenCompositeIsStarted_ThenAnticipatedLogsAreGeneratedInSeleniumHub()
        {
            var logs = this.gridCompositeService.Logs("selenium-hub");
            Assert.Multiple(() =>
            {
                Assert.That(logs, Has.Exactly(1).Matches<string>(l => l.Contains("INFO spawned: 'selenium-grid-hub'")));
                Assert.That(logs, Has.Exactly(1).Matches<string>(l => l.Contains("INFO success: selenium-grid-hub entered RUNNING state")));
            });
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            this.gridCompositeService.Stop();
            this.gridCompositeService.Dispose();
        }

        private static MyCompositeBuilder GetGridCompositeBuilder()
        {
            var composeFilePath = Path.Combine(Resources.Path, ComposeFile);
            var builder = new Builder()
                .UseContainer()
                .UseCompose()
                .FromFile(composeFilePath)
                .RemoveOrphans()
                .WaitForPort("selenium-hub", "4444/tcp", 30000)
                .WaitForHttp(
                    "selenium-hub",
                    "http://localhost:4444/ui/",
                    contentType: "text/html",
                    continuation: (resp, cnt) =>
                    {
                        if (cnt >= 15)
                        {
                            return 0;
                        }

                        return resp.Code.Equals((HttpStatusCode)200) && resp.Body.Contains("Selenium Grid") ? 0 : 500;
                    })
                .WaitForHttp(
                    "selenium-hub",
                    "http://localhost:4444/status",
                    contentType: "application/json",
                    continuation: (resp, cnt) =>
                    {
                        if (cnt >= 15)
                        {
                            return 0;
                        }

                        return resp.Code.Equals((HttpStatusCode)200) && resp.Body.Contains("Selenium Grid ready.") && resp.Body.Contains("chrome") && resp.Body.Contains("firefox") && resp.Body.Contains("MicrosoftEdge") ? 0 : 500;
                    });

            return new MyCompositeBuilder(builder);
        }
    }
}
