using Nop.Core;
using System;

namespace Motillo.Nop.Plugin.KlarnaCheckout.Data
{
    public enum KlarnaCheckoutStatus
    {
        Pending = 0,
        Complete = 1,
        Failed = 2,
        Activated = 3
    }

    public class KlarnaCheckoutEntity : BaseEntity
    {
        public int StoreId { get; set; }
        public string KlarnaResourceUri { get; set; }
        public int CustomerId { get; set; }
        public int AffiliateId { get; set; }
        public Guid OrderGuid { get; set; }
        public KlarnaCheckoutStatus Status { get; set; }
        public string IpAddress { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }
}
