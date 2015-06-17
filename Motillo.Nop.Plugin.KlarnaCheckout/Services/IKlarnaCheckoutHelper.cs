using Motillo.Nop.Plugin.KlarnaCheckout.Models;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using System.Collections.Generic;

namespace Motillo.Nop.Plugin.KlarnaCheckout.Services
{
    public interface IKlarnaCheckoutHelper
    {
        int ConvertToCents(decimal value);
        Cart GetCart();
        Merchant GetMerchant();
        IEnumerable<CartItem> GetCartItems(IEnumerable<ShoppingCartItem> items);
        Motillo.Nop.Plugin.KlarnaCheckout.Services.KlarnaCheckoutHelper.SupportedLocale GetSupportedLocale();

        /// <summary>
        /// Gets the reference used for physical klarna cart items. For products it's simple the ID. For attribute combinations it's PRODUCTID_COMBOID
        /// </summary>
        string GetReference(Product product, ProductAttributeCombination combination);

        Gui GetGui();
        Options GetOptions();
    }
}
