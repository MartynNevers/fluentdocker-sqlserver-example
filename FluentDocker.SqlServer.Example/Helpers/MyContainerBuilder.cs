namespace FluentDocker.SqlServer.Example.Helpers
{
    using Ductus.FluentDocker.Builders;

    internal class MyContainerBuilder
    {
        private readonly ContainerBuilder containerBuilder;

        public MyContainerBuilder(ContainerBuilder containerBuilder)
        {
            this.containerBuilder = containerBuilder;
        }

        public MyContainerService Build()
        {
            var containerService = this.containerBuilder.Build();
            return new MyContainerService(containerService);
        }
    }
}
