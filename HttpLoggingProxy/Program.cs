﻿


using Microsoft.Extensions.Options;
using Symbotic.Framework.Logging;

namespace HttpLoggingProxy
{
    using Microsoft.Extensions.Configuration;
    using Symbotic.Framework.Chassis.Abstractions;
    using Microsoft.Extensions.DependencyInjection;
    using Symbotic.Framework.Chassis;
    using Symbotic.Framework.Chassis.Console;
    using System;

    public class Program
    {
        public static void Main(string[] args)
        {
            var host = HostBuilder.Create(args)
                .WithStandardLogging()
                .AsConsoleApplication<Application>()
                .Build();

            host.Run();

            //var a = new Application(new LoggerFactory(new OptionsWrapper<LoggingOptions>(LoggingOptions.Default))
            //    .Create<Application>());
            //a.Start();
        }
    }

}