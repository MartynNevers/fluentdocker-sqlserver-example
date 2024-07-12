namespace FluentDocker.SqlServer.Example.Helpers
{
    using System.Text.RegularExpressions;

    internal class Resources
    {
        private const string WindowsPattern = @"(\\bin\\(Debug|Release)(\\[a-zA-Z0-9.]*)$)";
        private const string LinuxPattern = @"(/bin/(Debug|Release)(/[a-zA-Z0-9.]*)$)";

        public static string Path
        {
            get
            {
                var pathRegex = new Regex($"{WindowsPattern}|{LinuxPattern}", RegexOptions.Compiled);
                var projectPath = pathRegex.Replace(Directory.GetCurrentDirectory(), string.Empty);
                return System.IO.Path.Combine(projectPath, "Resources");
            }
        }
    }
}
