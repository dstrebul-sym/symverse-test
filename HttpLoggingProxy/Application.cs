namespace HttpLoggingProxy
{
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Extensions.Configuration;
    using Symbotic.Framework.Chassis.Abstractions;
    using Symbotic.Framework.Logging;

    public class Application:IApplication
    {
        private static IConfigurationRoot _configuration;
        private readonly ILogger<Application> _logger;
        private readonly List<Proxy> _services;
        public Application(ILogger<Application> logger)
        {
            _logger = logger;
            _services = new List<Proxy>();

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            _configuration = builder.Build();

        }
        public void Start()
        {
            foreach (var arg in _configuration.GetSection("Routings").GetChildren())
            {
                _services.Add(new Proxy(int.Parse(arg.Key), int.Parse(arg.Value), _logger));
            }
        }

        public void Stop()
        {
            _services.ForEach(s => s.Dispose());
        }
    }
}