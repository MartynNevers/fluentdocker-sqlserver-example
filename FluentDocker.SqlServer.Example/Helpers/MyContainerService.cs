namespace FluentDocker.SqlServer.Example.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Ductus.FluentDocker.Commands;
    using Ductus.FluentDocker.Services;

    internal class MyContainerService : IDisposable
    {
        private readonly IContainerService containerService;

        public MyContainerService(IContainerService containerService)
        {
            if (containerService == null)
            {
                ArgumentNullException.ThrowIfNull(containerService);
            }

            this.containerService = containerService;
        }

        public IContainerService ContainerService
        {
            get
            {
                return this.containerService;
            }
        }

        public IList<string> Logs
        {
            get
            {
                List<string> result = new List<string>();
                using (var logs = this.containerService.DockerHost.Logs(this.containerService.Id, certificates: this.containerService.Certificates))
                {
                    while (!logs.IsFinished)
                    {
                        // Do a read with timeout
                        var line = logs.TryRead(5000);
                        if (line == null)
                        {
                            break;
                        }

                        Debug.WriteLine(line);
                        result.Add(line);
                    }
                }

                return result;
            }
        }

        public static void KillZombieContainers(string prefix)
        {
            var hosts = new Hosts().Discover();
            var docker = hosts.FirstOrDefault(x => x.IsNative) ?? hosts.FirstOrDefault(x => x.Name == "default");
            Debug.WriteLine($"Docker host: {docker?.Host.Host}, {docker?.Host.AbsolutePath}, {docker?.Host.AbsoluteUri}");
            var containers = docker?.GetContainers().Where(c => c.Name.StartsWith(prefix));
            Debug.WriteLine($"Number of containers: {containers?.Count<IContainerService>()}");
            if (containers is not null)
            {
                foreach (var container in containers)
                {
                    Debug.WriteLine($"Stopping container: {container.Name}");
                    container.Stop();
                    Debug.WriteLine($"Destroying container: {container.Name}");
                    container.Dispose();
                }
            }
        }

        public MyContainerService Start()
        {
            this.containerService.Start();
            return this;
        }

        public MyContainerService Stop()
        {
            this.containerService.Stop();
            return this;
        }

        public void Dispose()
        {
            this.containerService.Dispose();
        }
    }
}
