using System;

namespace HttpLoggingProxy
{
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Extensions.Configuration;
    using Symbotic.Framework.Chassis.Abstractions;
    using Symbotic.Framework.Logging;
    using System.Linq;

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
            foreach (var routing in _configuration.GetSection("Routings").GetChildren())
            {
                foreach (var service in routing.GetSection("mappings").GetChildren())
                {
                    _services.Add(new Proxy(routing.Key,
                        int.Parse(service.GetValue<string>("fromPort")),
                        int.Parse(service.GetValue<string>("toPort")), 
                        _logger));
                }
                
            }
        }

        public void Stop()
        {
            _services.ForEach(s => s.Dispose());
        }
    }
}