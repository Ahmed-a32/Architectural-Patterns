using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Automatonymous;
using MassTransit;
using MassTransit.Saga;

namespace AuctionSaga
{
    class Program
    {
        static void Main(string[] args)
        {
            var bus=Bus.Factory.CreateUsingInMemory(sbc =>
            {
                sbc.ReceiveEndpoint("my_saga_bus", e =>
                {
                    e.StateMachineSaga(new AuctionSaga(), 
                        new InMemorySagaRepository<AuctionSagaInfo>());
                });
                
                sbc.UseInMemoryScheduler();
            });            
            bus.Start();
            Console.WriteLine("bus started");
            var auctionId = Guid.NewGuid();
            bus.Publish(new CreateAuction(auctionId)
            {
                OpeningBid = 50,
                OwnerEmail = "john@doh.com",
                Title = "Sale"
            });

            var auction2Id = Guid.NewGuid();
            bus.Publish(new CreateAuction(auction2Id)
            {
                OpeningBid = 50,
                OwnerEmail = "john@doh.com",
                Title = "Sale2"
            });
            bus.Publish(new PlaceBid()
            {
                AuctionId = auctionId,
                BidderEmail = "tamir@dresher.com",
                BidId = Guid.NewGuid(),
                MaximumBid = 100
            });
            bus.Publish(new PlaceBid()
            {
                AuctionId = auction2Id,
                BidderEmail = "tamir2@dresher.com",
                BidId = Guid.NewGuid(),
                MaximumBid = 150
            });



            Console.ReadLine();
        }
    }
}
