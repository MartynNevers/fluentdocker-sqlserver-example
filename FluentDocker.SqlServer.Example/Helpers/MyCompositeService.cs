namespace FluentDocker.SqlServer.Example.Helpers
{
    using Ductus.FluentDocker.Services;

    internal class MyCompositeService : IDisposable
    {
        private readonly ICompositeService compositeService;

        public MyCompositeService(ICompositeService compositeService)
        {
            ArgumentNullException.ThrowIfNull(compositeService);
            this.compositeService = compositeService;
        }

        public ICompositeService CompositeService
        {
            get
            {
                return this.compositeService;
            }
        }

        public IList<string> Logs(string containerName)
        {
            var containerService = this.compositeService.Containers.FirstOrDefault(c => c.Name.Equals(containerName));
            if (containerService == null)
            {
                throw new KeyNotFoundException($"Could not find container: {containerName}");
            }

            return new MyContainerService(containerService).Logs;
        }

        public MyCompositeService Start()
        {
            this.compositeService.Start();
            return this;
        }

        public MyCompositeService Stop()
        {
            this.compositeService.Stop();
            return this;
        }

        public void Dispose()
        {
            this.compositeService.Dispose();
        }
    }
}
