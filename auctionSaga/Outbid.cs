using System;

namespace AuctionSaga
{
    public class Outbid
    {
        public Outbid(Guid bidId)
        {
            this.BidId = bidId;
        }

        public Guid BidId { get; set; }
    }
}