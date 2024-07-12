namespace FluentDocker.SqlServer.Example.Helpers
{
    using Ductus.FluentDocker.Builders;

    internal class MyCompositeBuilder
    {
        private readonly CompositeBuilder compositeBuilder;

        public MyCompositeBuilder(CompositeBuilder compositeBuilder)
        {
            this.compositeBuilder = compositeBuilder;
        }

        public MyCompositeService Build()
        {
            var compositeService = this.compositeBuilder.Build();
            return new MyCompositeService(compositeService);
        }
    }
}
