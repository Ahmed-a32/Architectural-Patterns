using System;

namespace AuctionSaga
{
    public class AuctionExpired
    {
        public AuctionExpired()
        {
            
        }

        public AuctionExpired(AuctionSagaInfo instance)
        {
            this.AuctionId = instance.CorrelationId;
        }

        public Guid AuctionId { get; set; }
    }
}