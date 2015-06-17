using System.Data.Entity.ModelConfiguration;

namespace Motillo.Nop.Plugin.KlarnaCheckout.Data
{
    public class KlarnaCheckoutEntityConfiguration : EntityTypeConfiguration<KlarnaCheckoutEntity>
    {
        public KlarnaCheckoutEntityConfiguration()
        {
            ToTable(KlarnaCheckoutContext.TableName);

            HasKey(m => m.Id);
            Property(m => m.OrderGuid).IsRequired();
            Property(m => m.StoreId).IsRequired();
            Property(m => m.CustomerId).IsRequired();
            Property(m => m.OrderGuid).IsRequired();
            Property(m => m.KlarnaResourceUri).IsRequired();
            Property(m => m.Status).IsRequired();
            Property(m => m.IpAddress);
            Property(m => m.CreatedOnUtc).IsRequired();
        }
    }
}
