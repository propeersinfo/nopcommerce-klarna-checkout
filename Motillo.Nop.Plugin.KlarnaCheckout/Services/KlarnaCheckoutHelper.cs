using Motillo.Nop.Plugin.KlarnaCheckout.Models;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Tax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace Motillo.Nop.Plugin.KlarnaCheckout.Services
{
    public class KlarnaCheckoutHelper : IKlarnaCheckoutHelper
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ITaxService _taxService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ICurrencyService _currencyService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly KlarnaCheckoutSettings _klarnaSettings;

        // https://developers.klarna.com/en/api-references-v1/klarna-checkout#supported_locales
        private static readonly List<SupportedLocale> ValidLocales = new List<SupportedLocale>
            {
                new SupportedLocale
                {
                    Country = "Sweden",
                    Language = "Swedish",
                    PurchaseCountry = "SE",
                    PurchaseCurrency = "SEK",
                    Locale = "sv-se"
                },
                new SupportedLocale
                {
                    Country = "Finland",
                    Language = "Finnish",
                    PurchaseCountry = "FI",
                    PurchaseCurrency = "EUR",
                    Locale = "fi-fi"
                },
                new SupportedLocale
                {
                    Country = "Finland",
                    Language = "Swedish",
                    PurchaseCountry = "FI",
                    PurchaseCurrency = "EUR",
                    Locale = "sv-fi"
                },
                new SupportedLocale
                {
                    Country = "Norway",
                    Language = "Norwegian",
                    PurchaseCountry = "NO",
                    PurchaseCurrency = "NOK",
                    Locale = "nb-no"
                },
                new SupportedLocale
                {
                    Country = "Germany",
                    Language = "German",
                    PurchaseCountry = "DE",
                    PurchaseCurrency = "EUR",
                    Locale = "de-de"
                },
                new SupportedLocale
                {
                    Country = "Austria",
                    Language = "German",
                    PurchaseCountry = "AT",
                    PurchaseCurrency = "EUR",
                    Locale = "de-at"
                }
            };

        public KlarnaCheckoutHelper(
            IWorkContext workContext,
            IStoreContext storeContext,
            IOrderTotalCalculationService orderTotalCalculationService,
            ILocalizationService localizationService,
            IProductAttributeParser productAttributeParser,
            ITaxService taxService,
            IPriceCalculationService priceCalculationService,
            ICurrencyService currencyService,
            IGenericAttributeService genericAttributeService,
            ICheckoutAttributeParser checkoutAttributeParser,
            KlarnaCheckoutSettings klarnaSettings
            )
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _orderTotalCalculationService = orderTotalCalculationService;
            _localizationService = localizationService;
            _productAttributeParser = productAttributeParser;
            _taxService = taxService;
            _priceCalculationService = priceCalculationService;
            _currencyService = currencyService;
            _genericAttributeService = genericAttributeService;
            _checkoutAttributeParser = checkoutAttributeParser;
            _klarnaSettings = klarnaSettings;
        }

        public int ConvertToCents(decimal value)
        {
            return (int)Math.Round(value * 100, 0, MidpointRounding.AwayFromZero);
        }

        public Cart GetCart()
        {
            var storeId = _storeContext.CurrentStore.Id;
            var customer = _workContext.CurrentCustomer;
            var cartItems = customer.ShoppingCartItems
                .Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(storeId)
                .ToList();

            var items = new List<CartItem>();

            items.AddRange(GetCartItems(cartItems));
            items.Add(GetShippingItem(cartItems));
            items.AddRange(GetDiscountAndGiftCardItems(cartItems));
            items.AddRange(GetCeckoutAttributeItems());

            return new Cart
            {
                Items = items
            };
        }

        private IEnumerable<CartItem> GetCeckoutAttributeItems()
        {
            var result = new List<CartItem>();
            var storeId = _storeContext.CurrentStore.Id;
            var customer = _workContext.CurrentCustomer;

            var checkoutAttributesXml = customer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, _genericAttributeService, storeId);
            var attributeValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(checkoutAttributesXml);
            
            if (attributeValues != null)
            {
                foreach (var attributeValue in attributeValues)
                {
                    decimal taxRate;
                    var caInclTax = _taxService.GetCheckoutAttributePrice(attributeValue, true, customer, out taxRate);
                    var amountInCurrentCurrency = _currencyService.ConvertFromPrimaryStoreCurrency(caInclTax, _workContext.WorkingCurrency);
                    var price = ConvertToCents(amountInCurrentCurrency);
                    var taxRateInCents = ConvertToCents(taxRate);
                    var checkoutAttributePromptName = attributeValue.CheckoutAttribute.GetLocalized(x => x.TextPrompt);
                    var attributeValueName = attributeValue.GetLocalized(x => x.Name);
                    var name = string.Format("{0}: {1}", checkoutAttributePromptName, attributeValueName);

                    result.Add(new CartItem
                    {
                        Type = CartItem.TypeDiscount,
                        Reference = string.Format("KC_CA_{0}_{1}", checkoutAttributePromptName, attributeValueName),
                        Name = name,
                        Quantity = 1,
                        UnitPrice = price,
                        TaxRate = taxRateInCents
                    });
                }
            }

            return result;
        }

        public Merchant GetMerchant()
        {
            var storeUrl = _storeContext.CurrentStore.Url;
            var eid = _klarnaSettings.EId;

            return new Merchant
            {
                Id = eid.ToString(CultureInfo.InvariantCulture),
                TermsUri = storeUrl + _klarnaSettings.TermsUrl,
                CheckoutUri = storeUrl + _klarnaSettings.CheckoutUrl,
                ConfirmationUri = string.Format("{0}Plugins/Motillo.KlarnaCheckout/ThankYou?sid={1}&klarna_order={2}", storeUrl, eid, "{checkout.order.uri}"),
                PushUri = string.Format("{0}Plugins/Motillo.KlarnaCheckout/KlarnaCheckoutPush?sid={1}&klarna_order={2}", storeUrl, eid, "{checkout.order.uri}")
            };
        }

        public IEnumerable<CartItem> GetCartItems(IEnumerable<ShoppingCartItem> items)
        {
            var cartItems = new List<CartItem>();

            foreach (var item in items)
            {
                var klarnaItem = CreateCartItem(item);
                cartItems.Add(klarnaItem);
            }

            return cartItems;
        }

        public SupportedLocale GetSupportedLocale()
        {
            var language = _workContext.WorkingLanguage.LanguageCulture.ToLowerInvariant();
            var currency = _workContext.WorkingCurrency.CurrencyCode.ToUpperInvariant();
            var customer = _workContext.CurrentCustomer;
            var enabledCountries = (_klarnaSettings.EnabledCountries ?? string.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string purchaseCountry;

            if (customer.ShippingAddress != null && customer.ShippingAddress.Country != null)
            {
                purchaseCountry = customer.ShippingAddress.Country.TwoLetterIsoCode;
            }
            else
            {
                var culture = new CultureInfo(language);
                var regionInfo = new RegionInfo(culture.LCID);
                purchaseCountry = regionInfo.TwoLetterISORegionName;
            }

            var options = ValidLocales.Where(x => x.PurchaseCountry == purchaseCountry && (enabledCountries.Length == 0 || enabledCountries.Contains(x.PurchaseCountry)) && x.PurchaseCurrency == currency).ToList();
            var match = options.FirstOrDefault(x => x.Locale == language);

            return match ?? options.FirstOrDefault();
        }

        private IEnumerable<CartItem> GetDiscountAndGiftCardItems(IList<ShoppingCartItem> cart)
        {
            var result = new List<CartItem>();

            decimal orderDiscountAmount;
            List<AppliedGiftCard> appliedGiftCards;
            Discount orderAppliedDiscount;
            int redeemedRewardPoints;
            decimal redeemedRewardPointsAmount;

            _orderTotalCalculationService.GetShoppingCartTotal(cart,
                        out orderDiscountAmount, out orderAppliedDiscount, out appliedGiftCards,
                        out redeemedRewardPoints, out redeemedRewardPointsAmount);

            foreach (var giftCard in appliedGiftCards)
            {
                var amountInCurrentCurrency = _currencyService.ConvertFromPrimaryStoreCurrency(giftCard.AmountCanBeUsed, _workContext.WorkingCurrency);
                var price = ConvertToCents(amountInCurrentCurrency) * -1;

                result.Add(new CartItem
                {
                    Type = CartItem.TypeDiscount,
                    Reference = giftCard.GiftCard.Id.ToString(CultureInfo.InvariantCulture),
                    Name = _localizationService.GetResource("shoppingcart.giftcardcouponcode"),
                    Quantity = 1,
                    UnitPrice = price,
                    TaxRate = 0
                });
            }

            if (redeemedRewardPointsAmount > 0)
            {
                var amountInCurrentCurrency = _currencyService.ConvertFromPrimaryStoreCurrency(redeemedRewardPointsAmount, _workContext.WorkingCurrency);
                var discount = ConvertToCents(amountInCurrentCurrency) * -1;
                var name = string.Format(_localizationService.GetResource("order.rewardpoints"), redeemedRewardPoints);

                result.Add(new CartItem
                {
                    Type = CartItem.TypeDiscount,
                    Reference = "KC_REWARD_POINTS",
                    Name = name,
                    Quantity = 1,
                    UnitPrice = discount,
                    TaxRate = 0
                });
            }

            if (orderDiscountAmount > 0)
            {
                var amountInCurrentCurrency = _currencyService.ConvertFromPrimaryStoreCurrency(orderDiscountAmount, _workContext.WorkingCurrency);
                var discount = ConvertToCents(amountInCurrentCurrency) * -1;
                var name = _localizationService.GetResource("order.totaldiscount");

                if (orderAppliedDiscount.RequiresCouponCode)
                {
                    name = string.Format(CultureInfo.CurrentUICulture, "{0}: {1}", _localizationService.GetResource("shoppingcart.discountcouponcode"), orderAppliedDiscount.CouponCode);
                }

                result.Add(new CartItem
                {
                    Type = CartItem.TypeDiscount,
                    Reference = "KC_D_T",
                    Name = name,
                    Quantity = 1,
                    UnitPrice = discount,
                    TaxRate = 0
                });
            }

            decimal subDiscountAmount;
            Discount subOrderAppliedDiscount;
            decimal subTotalWithoutDiscount;
            decimal subTotalWithDiscount;
            _orderTotalCalculationService.GetShoppingCartSubTotal(cart, true, out subDiscountAmount, out subOrderAppliedDiscount, out subTotalWithoutDiscount, out subTotalWithDiscount);

            if (subDiscountAmount > 0)
            {
                var amountInCurrentCurrency = _currencyService.ConvertFromPrimaryStoreCurrency(subDiscountAmount, _workContext.WorkingCurrency);
                var discount = ConvertToCents(amountInCurrentCurrency) * -1;
                var name = _localizationService.GetResource("order.totaldiscount");

                if (subOrderAppliedDiscount.RequiresCouponCode)
                {
                    name = string.Format(CultureInfo.CurrentUICulture, "{0}: {1}", _localizationService.GetResource("shoppingcart.discountcouponcode"), subOrderAppliedDiscount.CouponCode);
                }

                result.Add(new CartItem
                {
                    Type = CartItem.TypeDiscount,
                    Reference = "KC_D_S",
                    Name = name,
                    Quantity = 1,
                    UnitPrice = discount,
                    TaxRate = 0
                });
            }

            return result;
        }

        private CartItem GetShippingItem(IList<ShoppingCartItem> items)
        {
            var shippingOption = _workContext.CurrentCustomer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, _storeContext.CurrentStore.Id);
            var shippingId = shippingOption != null ? shippingOption.Name : string.Empty;
            var shippingName = shippingOption != null ? shippingOption.Name : _localizationService.GetResource("order.shipping");
            var shippingPrice = 0;
            var intTaxRate = 0;

            decimal taxRate;
            Discount appliedDiscount;
            var shippingTotal = _orderTotalCalculationService.GetShoppingCartShippingTotal(items, true, out taxRate, out appliedDiscount);

            if (shippingTotal.HasValue)
            {
                var priceInCurrentCurrency = _currencyService.ConvertFromPrimaryStoreCurrency(shippingTotal.Value, _workContext.WorkingCurrency);

                shippingPrice = ConvertToCents(priceInCurrentCurrency);
                intTaxRate = ConvertToCents(taxRate);
            }

            return new CartItem
            {
                Type = CartItem.TypeShippingFee,
                Reference = shippingId,
                Name = shippingName,
                Quantity = 1,
                UnitPrice = shippingPrice,
                TaxRate = intTaxRate
            };
        }

        public string GetReference(Product product, ProductAttributeCombination combination)
        {
            var reference = product.Sku;

            if (string.IsNullOrEmpty(reference))
            {
                reference = product.Id.ToString(CultureInfo.InvariantCulture);
            }

            if (combination != null)
            {
                reference = combination.Sku;

                if (string.IsNullOrEmpty(reference))
                {
                    reference = string.Format(CultureInfo.InvariantCulture, "{0}_{1}", product.Id, combination.Id);
                }
            }

            return reference;
        }

        public Gui GetGui()
        {
            var result = new Gui();

            if (_klarnaSettings.DisableAutofocus)
            {
                result.Options = new[] {Gui.OptionDisableAutofocus};
            }

            return result;
        }

        public Options GetOptions()
        {
            var result = new Options();

            if (_klarnaSettings.AllowSeparateShippingAddress)
            {
                result.AllowSeparateShippingAddress = true;
            }

            if (!string.IsNullOrEmpty(_klarnaSettings.ColorButton))
            {
                result.ColorButton = _klarnaSettings.ColorButton;
            }

            if (!string.IsNullOrEmpty(_klarnaSettings.ColorButtonText))
            {
                result.ColorButtonText = _klarnaSettings.ColorButtonText;
            }

            if (!string.IsNullOrEmpty(_klarnaSettings.ColorCheckbox))
            {
                result.ColorCheckbox = _klarnaSettings.ColorCheckbox;
            }

            if (!string.IsNullOrEmpty(_klarnaSettings.ColorCheckboxCheckmark))
            {
                result.ColorCheckboxCheckmark = _klarnaSettings.ColorCheckboxCheckmark;
            }

            if (!string.IsNullOrEmpty(_klarnaSettings.ColorHeader))
            {
                result.ColorHeader = _klarnaSettings.ColorHeader;
            }

            if (!string.IsNullOrEmpty(_klarnaSettings.ColorLink))
            {
                result.ColorLink = _klarnaSettings.ColorLink;
            }

            return result;
        }

        private CartItem CreateCartItem(ShoppingCartItem item)
        {
            var product = item.Product;
            var combo = _productAttributeParser.FindProductAttributeCombination(product, item.AttributesXml);
            var reference = GetReference(product, combo);
            var name = product.GetLocalized(x => x.Name);
            int discountRate;
            var unitPrice = GetIntUnitPriceAndPercentageDiscount(item, out discountRate);
            var taxRate = GetIntTaxRate(item);

            var result = new CartItem
            {
                Type = CartItem.TypePhysical,
                Reference = reference,
                Name = name,
                Quantity = item.Quantity,
                UnitPrice = unitPrice,
                DiscountRate = discountRate,
                TaxRate = taxRate
            };

            return result;
        }

        private int GetIntTaxRate(ShoppingCartItem item)
        {
            decimal taxRate;
            _taxService.GetProductPrice(item.Product, _priceCalculationService.GetUnitPrice(item, includeDiscounts: true), out taxRate);

            return ConvertToCents(taxRate);
        }

        private int GetIntUnitPriceAndPercentageDiscount(ShoppingCartItem item, out int discountRate)
        {
            decimal discountAmount;
            Discount appliedDiscount;
            var unitPrice = _priceCalculationService.GetUnitPrice(item, true, out discountAmount, out appliedDiscount);
            discountRate = 0;

            // Only set discount rate if the discount uses percentage, otherwise
            // we can't get the exact correct amount since klarna assumes 2 decimals.
            if (appliedDiscount != null && appliedDiscount.UsePercentage)
            {
                unitPrice += discountAmount;
                discountRate = ConvertToCents(appliedDiscount.DiscountPercentage);
            }

            var priceInCurrentCurrency = _currencyService.ConvertFromPrimaryStoreCurrency(unitPrice, _workContext.WorkingCurrency);

            return ConvertToCents(priceInCurrentCurrency);
        }

        [DebuggerDisplay("{Country} ({PurchaseCountry}): {PurchaseCurrency}")]
        public class SupportedLocale
        {
            public string Country { get; set; }
            public string Language { get; set; }
            public string PurchaseCountry { get; set; }
            public string PurchaseCurrency { get; set; }
            public string Locale { get; set; }
        }
    }
}