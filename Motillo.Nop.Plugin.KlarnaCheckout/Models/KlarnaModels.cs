using Newtonsoft.Json;
using Nop.Core.Domain.Directory;
using Nop.Core.Infrastructure;
using Nop.Services.Directory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Motillo.Nop.Plugin.KlarnaCheckout.Models
{
    [DebuggerDisplay("{Reference} ({Type}): {Name} ({Quantity})")]
    public class CartItem
    {
        public const string TypePhysical = "physical";
        public const string TypeShippingFee = "shipping_fee";
        public const string TypeDiscount = "discount";

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("reference", NullValueHandling = NullValueHandling.Ignore)]
        public string Reference { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("unit_price")]
        public int UnitPrice { get; set; }

        [JsonProperty("tax_rate")]
        public int TaxRate { get; set; }

        [JsonProperty("discount_rate")]
        public int DiscountRate { get; set; }

        [JsonProperty("total_price_including_tax", NullValueHandling = NullValueHandling.Ignore)]
        public int? TotalPriceIncludingTax { get; set; }

        [JsonProperty("total_price_excluding_tax", NullValueHandling = NullValueHandling.Ignore)]
        public int? TotalPriceExcludingTax { get; set; }

        [JsonProperty("total_tax_amount", NullValueHandling = NullValueHandling.Ignore)]
        public int? TotalTaxAmount { get; set; }
    }

    public class Cart
    {
        [JsonProperty("total_price_excluding_tax", NullValueHandling = NullValueHandling.Ignore)]
        public int? TotalPriceExcludingTax { get; set; }

        [JsonProperty("total_tax_amount", NullValueHandling = NullValueHandling.Ignore)]
        public int? TotalTaxAmount { get; set; }

        [JsonProperty("total_price_including_tax", NullValueHandling = NullValueHandling.Ignore)]
        public int? TotalPriceIncludingTax { get; set; }

        [JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
        public List<CartItem> Items { get; set; }
    }

    [DebuggerDisplay("{Type} ({Gender})")]
    public class Customer
    {
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("gender", NullValueHandling = NullValueHandling.Ignore)]
        public string Gender { get; set; }

        [JsonProperty("date_of_birth", NullValueHandling = NullValueHandling.Ignore)]
        public string DateOfBirth { get; set; }
    }

    [DebuggerDisplay("{StreetAddress}, {PostalCode}, {Country}")]
    public class Address
    {
        private const int GERMANY = 276;

        [JsonProperty("given_name", NullValueHandling = NullValueHandling.Ignore)]
        public string GivenName { get; set; }

        [JsonProperty("family_name", NullValueHandling = NullValueHandling.Ignore)]
        public string FamilyName { get; set; }

        [JsonProperty("care_of", NullValueHandling = NullValueHandling.Ignore)]
        public string CareOf { get; set; }

        // Only in Sweden, Norway and Finland
        [JsonProperty("street_address", NullValueHandling = NullValueHandling.Ignore)]
        public string StreetAddress { get; set; }

        // Only in Germany
        [JsonProperty("street_name", NullValueHandling = NullValueHandling.Ignore)]
        public string StreetName { get; set; }

        // Only in Germany
        [JsonProperty("street_number", NullValueHandling = NullValueHandling.Ignore)]
        public string StreetNumber { get; set; }

        [JsonProperty("postal_code", NullValueHandling = NullValueHandling.Ignore)]
        public string PostalCode { get; set; }

        [JsonProperty("city", NullValueHandling = NullValueHandling.Ignore)]
        public string City { get; set; }

        [JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; set; }

        [JsonProperty("phone", NullValueHandling = NullValueHandling.Ignore)]
        public string Phone { get; set; }

        [JsonProperty("country", NullValueHandling = NullValueHandling.Ignore)]
        public string Country { get; set; }

        public Country GetNopCountry()
        {
            var countryService = EngineContext.Current.Resolve<ICountryService>();
            var country = countryService.GetCountryByTwoLetterIsoCode(this.Country);

            return country;
        }

        public bool RepresentsAddress(global::Nop.Core.Domain.Common.Address address)
        {
            var country = GetNopCountry();

            var result = address.CountryId == country.Id &&
                   string.Compare((address.City ?? string.Empty).Trim(), City, StringComparison.OrdinalIgnoreCase) == 0 &&
                   string.Compare((address.ZipPostalCode ?? string.Empty).Trim(), PostalCode, StringComparison.OrdinalIgnoreCase) == 0;

            if (result)
            {
                var addr1 = (address.Address1 ?? string.Empty).Trim();

                // In germany street name and number are separate fields so check if they are part of the address field.
                if (country.NumericIsoCode == GERMANY)
                {
                    result = CultureInfo.InvariantCulture.CompareInfo.IndexOf(addr1, StreetName, CompareOptions.IgnoreCase) != -1 &&
                        CultureInfo.InvariantCulture.CompareInfo.IndexOf(addr1, StreetNumber, CompareOptions.IgnoreCase) != -1;
                }
                else
                {
                    result = string.Compare(addr1, StreetAddress, StringComparison.OrdinalIgnoreCase) == 0;
                }
            }

            return result;
        }

        public void CopyTo(global::Nop.Core.Domain.Common.Address address)
        {
            var country = GetNopCountry();

            address.Email = Email;
            address.City = City;
            address.Country = country;
            address.CountryId = country.Id;
            address.FirstName = GivenName;
            address.LastName = FamilyName;
            address.ZipPostalCode = PostalCode;
            address.FaxNumber = Phone;

            if (country.NumericIsoCode == GERMANY)
            {
                address.Address1 = StreetName + " " + StreetNumber;
            }
            else
            {
                address.Address1 = StreetAddress;
            }
        }
    }

    [DebuggerDisplay("{Layout}")]
    public class Gui
    {
        public const string OptionDisableAutofocus = "disable_autofocus";

        [JsonProperty("layout", NullValueHandling = NullValueHandling.Ignore)]
        public string Layout { get; set; }

        [JsonProperty("snippet", NullValueHandling = NullValueHandling.Ignore)]
        public string Snippet { get; set; }

        [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Options { get; set; }
    }

    public class Options
    {
        [JsonProperty("allow_separate_shipping_address", NullValueHandling = NullValueHandling.Ignore)]
        public bool? AllowSeparateShippingAddress { get; set; }

        [JsonProperty("color_button", NullValueHandling = NullValueHandling.Ignore)]
        public string ColorButton { get; set; }

        [JsonProperty("color_button_text", NullValueHandling = NullValueHandling.Ignore)]
        public string ColorButtonText { get; set; }

        [JsonProperty("color_checkbox", NullValueHandling = NullValueHandling.Ignore)]
        public string ColorCheckbox { get; set; }

        [JsonProperty("color_checkbox_checkmark", NullValueHandling = NullValueHandling.Ignore)]
        public string ColorCheckboxCheckmark { get; set; }

        [JsonProperty("color_header", NullValueHandling = NullValueHandling.Ignore)]
        public string ColorHeader { get; set; }

        [JsonProperty("color_link", NullValueHandling = NullValueHandling.Ignore)]
        public string ColorLink { get; set; }
    }

    [DebuggerDisplay("{Id}")]
    public class Merchant
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("terms_uri", NullValueHandling = NullValueHandling.Ignore)]
        public string TermsUri { get; set; }

        [JsonProperty("checkout_uri", NullValueHandling = NullValueHandling.Ignore)]
        public string CheckoutUri { get; set; }

        [JsonProperty("confirmation_uri", NullValueHandling = NullValueHandling.Ignore)]
        public string ConfirmationUri { get; set; }

        [JsonProperty("push_uri", NullValueHandling = NullValueHandling.Ignore)]
        public string PushUri { get; set; }
    }

    [DebuggerDisplay("1: {OrderId1}, 2: {OrderId2}")]
    public class MerchantReference
    {
        [JsonProperty("orderid1", NullValueHandling = NullValueHandling.Ignore)]
        public string OrderId1 { get; set; }

        [JsonProperty("orderid2", NullValueHandling = NullValueHandling.Ignore)]
        public string OrderId2 { get; set; }
    }

    [DebuggerDisplay("{Id}: {Status}")]
    public class KlarnaOrder
    {
        public const string StatusCheckoutIncomplete = "checkout_incomplete";
        public const string StatusCheckoutComplete = "checkout_complete";
        public const string StatusCreated = "created";

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("purchase_country", NullValueHandling = NullValueHandling.Ignore)]
        public string PurchaseCountry { get; set; }

        [JsonProperty("purchase_currency", NullValueHandling = NullValueHandling.Ignore)]
        public string PurchaseCurrency { get; set; }

        [JsonProperty("locale", NullValueHandling = NullValueHandling.Ignore)]
        public string Locale { get; set; }

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }

        [JsonProperty("reference", NullValueHandling = NullValueHandling.Ignore)]
        public string Reference { get; set; }

        [JsonProperty("reservation", NullValueHandling = NullValueHandling.Ignore)]
        public string Reservation { get; set; }

        [JsonProperty("merchant_reference", NullValueHandling = NullValueHandling.Ignore)]
        public MerchantReference MerchantReference { get; set; }

        [JsonProperty("started_at", NullValueHandling = NullValueHandling.Ignore)]
        public string StartedAt { get; set; }

        [JsonProperty("last_modified_at", NullValueHandling = NullValueHandling.Ignore)]
        public string LastModifiedAt { get; set; }

        [JsonProperty("cart", NullValueHandling = NullValueHandling.Ignore)]
        public Cart Cart { get; set; }

        [JsonProperty("customer", NullValueHandling = NullValueHandling.Ignore)]
        public Customer Customer { get; set; }

        [JsonProperty("shipping_address", NullValueHandling = NullValueHandling.Ignore)]
        public Address ShippingAddress { get; set; }

        [JsonProperty("billing_address", NullValueHandling = NullValueHandling.Ignore)]
        public Address BillingAddress { get; set; }

        [JsonProperty("gui", NullValueHandling = NullValueHandling.Ignore)]
        public Gui Gui { get; set; }

        [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
        public Options Options { get; set; }

        [JsonProperty("merchant", NullValueHandling = NullValueHandling.Ignore)]
        public Merchant Merchant { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            var jsonData = JsonConvert.SerializeObject(this);

            return JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData);
        }

        public static KlarnaOrder FromDictionary(IDictionary<string, object> data)
        {
            if (data == null)
            {
                return new KlarnaOrder();
            }

            var jsonData = JsonConvert.SerializeObject(data);
            return JsonConvert.DeserializeObject<KlarnaOrder>(jsonData);
        }
    }
}
