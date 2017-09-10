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

namespace MT.ServiceBus.Client
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
            });

            bus.Start();

            while (true)
            {
                Console.WriteLine("Enter the participant name, or X to exit");
                var name = Console.ReadLine();

                if (name.ToLower() == "x") break;

                bus.Publish(new RegistrationMessage { ParticipantName = name, Email = $"{name.Replace(" ", "")}@test.com" });
            }
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();

            bus.Stop();
        }
    }
}
