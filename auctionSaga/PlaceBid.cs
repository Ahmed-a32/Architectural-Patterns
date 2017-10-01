using System;

namespace AuctionSaga
{
    public class PlaceBid
    {
        public Guid BidId { get; set; }

        public Guid AuctionId { get; set; }

        public decimal MaximumBid { get; set; }

        public string BidderEmail { get; set; }
    }
}