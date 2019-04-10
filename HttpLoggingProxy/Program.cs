namespace HttpLoggingProxy
{
    using Symbotic.Framework.Chassis;
    using Symbotic.Framework.Chassis.Console;

    public class Program
    {
        public static void Main(string[] args)
        {
            var host = HostBuilder.Create(args)
                .WithStandardLogging()
                .AsConsoleApplication<Application>()
                .Build();

            host.Run();
        }
    }

}