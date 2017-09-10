using Contracts;
using MassTransit;
using System;
public class Program
{
    public static void Main()
    {
        var bus = Bus.Factory.CreateUsingRabbitMq(sbc =>
        {
            var host = sbc.Host(new Uri("rabbitmq://localhost"), h =>
            {
                h.Username("guest");
                h.Password("guest");
            });                        
        });

        bus.Start();
                
        while (true)
        {
            Console.WriteLine("Enter the participant name, or X to exit");
            var name=Console.ReadLine();

            if (name.ToLower() == "x") break;

            bus.Publish(new RegistrationMessage { ParticipantName = name, Email = $"{name.Replace(" ","")}@test.com" });
        }
        Console.WriteLine("Press any key to exit");
        Console.ReadKey();

        bus.Stop();
    }
}