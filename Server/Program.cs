using Contracts;
using MassTransit;
using System;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var bus = Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                var host = sbc.Host(new Uri("rabbitmq://localhost"), h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                sbc.ReceiveEndpoint(host, "test_queue", ep =>
                {
                    ep.Handler<RegistrationMessage>(context =>
                    {
                        return Console.Out.WriteLineAsync($"Received: {context.Message.ParticipantName} - {context.Message.Email}");
                    });
                });
            });

            bus.Start();

            bus.Publish(new RegistrationMessage { ParticipantName="SelfTest", Email="self@test.com" });

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();

            bus.Stop();
        }
    }
}
