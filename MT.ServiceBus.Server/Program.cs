using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts;
using MassTransit;
using MassTransit.AzureServiceBusTransport;
using Microsoft.ServiceBus;

namespace MT.ServiceBus.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            //NOTE: Set the Service Bus namespace and access keys

            var bus = Bus.Factory.CreateUsingAzureServiceBus(sbc =>
            {
                var serviceUri = ServiceBusEnvironment.CreateServiceUri("sb",
                    ConfigurationManager.AppSettings["AzureSbNamespace"],
                    ConfigurationManager.AppSettings["AzureSbPath"]);

                var host = sbc.Host(serviceUri, h =>
                {
                    h.TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(
                        ConfigurationManager.AppSettings["AzureSbKeyName"],
                        ConfigurationManager.AppSettings["AzureSbSharedAccessKey"], TimeSpan.FromDays(1),
                        TokenScope.Namespace);
                });

                sbc.ReceiveEndpoint(host, ConfigurationManager.AppSettings["ServiceQueueName"], e =>
                {
                    // Configure your consumer(s)
                    e.Handler<RegistrationMessage>(context =>
                    {
                        return Console.Out.WriteLineAsync($"Received: {context.Message.ParticipantName} - {context.Message.Email}");
                    });
                    e.DefaultMessageTimeToLive = TimeSpan.FromMinutes(1);
                });
            });

            bus.Start();

            bus.Publish(new RegistrationMessage { ParticipantName = "SelfTest", Email = "self@test.com" });

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();

            bus.Stop();
        }
    }
}
