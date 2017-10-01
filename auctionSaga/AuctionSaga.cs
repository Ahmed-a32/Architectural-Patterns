using System;
using Automatonymous;
using MassTransit;
using MassTransit.Saga;


namespace AuctionSaga
{
    public class AuctionSaga : MassTransitStateMachine<AuctionSagaInfo>
    {

        public AuctionSaga()
        {
            InstanceState(x => x.CurrentState);
            Schedule(() => AuctionExpired, x => x.ExpirationId, x =>
            {
                x.Delay = TimeSpan.FromSeconds(10);
                x.Received = e => e.CorrelateById(context => context.Message.AuctionId);
            });
            Event(() => Create, x => x.CorrelateById(context => context.Message.CorrelationId)
                  .SelectId(context => context.Message.CorrelationId));
            Event(() => Bid, x => x.CorrelateById(context => context.Message.AuctionId));
            Initially(
                When(Create).Then(
                        (context) =>
                        {
                            context.Instance.CorrelationId = context.Data.CorrelationId;
                            context.Instance.OpeningBid = context.Data.OpeningBid;
                            context.Instance.OwnerEmail = context.Data.OwnerEmail;
                            context.Instance.Title = context.Data.Title;
                        })
                    .ThenAsync(context => Console.Out.WriteLineAsync($"Auction Created: {context.Data.CorrelationId} {context.Data.CorrelationId} - {context.Data.Title}"))
                    .Schedule(AuctionExpired, context => new AuctionExpired(context.Instance))
                    .TransitionTo(Open));
            During(Open,
                When(Bid)
                    .ThenAsync(context => Console.Out.WriteLineAsync($"Bid recieved: {context.Instance.CorrelationId}"))
                    .Then(context => Handle(context.Instance, context.Data)),
                When(AuctionExpired.Received)
                    .ThenAsync(context => Console.Out.WriteLineAsync($"Auction Expired: {context.Instance.CorrelationId}"))
                    .Finalize());

            SetCompletedWhenFinalized();

        }

        public Schedule<AuctionSagaInfo, AuctionExpired> AuctionExpired { get; set; }

        public AuctionSaga(Guid correlationId)
        {
            this.CorrelationId = correlationId;
        }



        //public static State Initial { get; set; }

        public State Completed { get; set; }

        public State Open { get; set; }

        public State Closed { get; set; }

        public Event<CreateAuction> Create { get; set; }

        public Event<PlaceBid> Bid { get; set; }

        public Guid CorrelationId { get; set; }

        public IBus Bus { get; set; }

        private void Handle(AuctionSagaInfo auction, PlaceBid bid)
        {
            if (!auction.CurrentBid.HasValue || bid.MaximumBid > auction.CurrentBid)
            {
                if (auction.HighBidder != null)
                {
                    this.Bus.Publish(new Outbid(auction.HighBidId));
                }
                auction.CurrentBid = bid.MaximumBid;
                auction.HighBidder = bid.BidderEmail;
                auction.HighBidId = bid.BidId;
            }
            else
            {
                // already outbid
                this.Bus.Publish(new Outbid(bid.BidId));
            }
        }
    }

    public class AuctionSagaInfo : SagaStateMachineInstance
    {
        public decimal? CurrentBid { get; set; }

        public string HighBidder { get; set; }

        public Guid HighBidId { get; set; }

        public decimal OpeningBid { get; set; }

        public string OwnerEmail { get; set; }

        public string Title { get; set; }
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        public Guid? ExpirationId { get; set; }
    }
}