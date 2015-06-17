using Klarna.Api;
using Klarna.Checkout;
using Motillo.Nop.Plugin.KlarnaCheckout.Data;
using Motillo.Nop.Plugin.KlarnaCheckout.Models;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Data;
using Nop.Services.Customers;
using Nop.Services.Logging;
using System;
using System.Globalization;
using System.Linq;

namespace Motillo.Nop.Plugin.KlarnaCheckout.Services
{
    public class KlarnaCheckoutPaymentService : IKlarnaCheckoutPaymentService
    {
        private const string ContentType = "application/vnd.klarna.checkout.aggregated-order-v2+json";

        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IWebHelper _webHelper;
        private readonly IRepository<KlarnaCheckoutEntity> _klarnaRepository;
        private readonly IKlarnaCheckoutHelper _klarnaCheckoutUtils;
        private readonly ICustomerService _customerService;
        private readonly ILogger _logger;
        private readonly KlarnaCheckoutSettings _klarnaSettings;

        public KlarnaCheckoutPaymentService(
            IStoreContext storeContext,
            IWorkContext workContext,
            IWebHelper webHelper,
            IRepository<KlarnaCheckoutEntity> klarnaRepository,
            IKlarnaCheckoutHelper klarnaCheckoutUtils,
            ICustomerService customerService,
            ILogger logger,
            KlarnaCheckoutSettings klarnaSettings)
        {
            _storeContext = storeContext;
            _workContext = workContext;
            _webHelper = webHelper;
            _klarnaRepository = klarnaRepository;
            _klarnaCheckoutUtils = klarnaCheckoutUtils;
            _customerService = customerService;
            _logger = logger;
            _klarnaSettings = klarnaSettings;
        }

        private string BaseUri
        {
            get
            {
                return _klarnaSettings.TestMode
                    ? "https://checkout.testdrive.klarna.com/checkout/orders"
                    : "https://checkout.klarna.com/checkout/orders";
            }
        }

        public void Acknowledge(Uri resourceUri, global::Nop.Core.Domain.Orders.Order order)
        {
            try
            {
                var connector = Connector.Create(_klarnaSettings.SharedSecret);
                var klarnaOrder = new Klarna.Checkout.Order(connector, resourceUri)
                {
                    ContentType = ContentType
                };

                klarnaOrder.Fetch();
                var fetchedData = klarnaOrder.Marshal();
                var typedData = KlarnaOrder.FromDictionary(fetchedData);

                if (typedData.Status == KlarnaOrder.StatusCheckoutComplete)
                {
                    var updateData = new KlarnaOrder
                    {
                        Status = KlarnaOrder.StatusCreated,
                        MerchantReference = new MerchantReference
                        {
                            OrderId1 = order.Id.ToString(CultureInfo.InvariantCulture),
                            OrderId2 = order.OrderGuid.ToString()
                        }
                    };

                    var dictData = updateData.ToDictionary();

                    klarnaOrder.Update(dictData);
                }
            }
            catch (Exception ex)
            {
                throw new NopException("Error Acknowledging Klarna Order", ex);
            }
        }

        public Uri Create()
        {
            var cart = _klarnaCheckoutUtils.GetCart();
            var merchant = _klarnaCheckoutUtils.GetMerchant();
            var supportedLocale = _klarnaCheckoutUtils.GetSupportedLocale();
            var gui = _klarnaCheckoutUtils.GetGui();
            var options = _klarnaCheckoutUtils.GetOptions();

            var klarnaOrder = new KlarnaOrder
            {
                Cart = cart,
                Merchant = merchant,
                Gui = gui,
                Options = options,
                Locale = supportedLocale.Locale,
                PurchaseCountry = supportedLocale.PurchaseCountry,
                PurchaseCurrency = supportedLocale.PurchaseCurrency
            };

            var dictData = klarnaOrder.ToDictionary();
            var connector = Connector.Create(_klarnaSettings.SharedSecret);
            var order = new Klarna.Checkout.Order(connector)
            {
                BaseUri = new Uri(BaseUri),
                ContentType = ContentType
            };

            order.Create(dictData);

            var location = order.Location;

            var kcoOrderRequest = GetKcoOrderRequest(_workContext.CurrentCustomer, location);
            _klarnaRepository.Insert(kcoOrderRequest);

            return location;
        }

        private KlarnaCheckoutEntity GetKcoOrderRequest(global::Nop.Core.Domain.Customers.Customer currentCustomer, Uri resourceUri)
        {
            return new KlarnaCheckoutEntity
            {
                CustomerId = currentCustomer.Id,
                OrderGuid = Guid.NewGuid(),
                KlarnaResourceUri = resourceUri.ToString(),
                Status = KlarnaCheckoutStatus.Pending,
                IpAddress = _webHelper.GetCurrentIpAddress(),
                AffiliateId = currentCustomer.AffiliateId,
                StoreId = _storeContext.CurrentStore.Id,
                CreatedOnUtc = DateTime.UtcNow
            };
        }

        public Order Fetch(Uri resourceUri)
        {
            var connector = Connector.Create(_klarnaSettings.SharedSecret);
            var order = new Klarna.Checkout.Order(connector, resourceUri)
            {
                ContentType = ContentType
            };

            order.Fetch();

            return order;
        }

        public bool Update(Uri resourceUri)
        {
            try
            {
                var cart = _klarnaCheckoutUtils.GetCart();
                var options = _klarnaCheckoutUtils.GetOptions();
                var connector = Connector.Create(_klarnaSettings.SharedSecret);
                var supportedLocale = _klarnaCheckoutUtils.GetSupportedLocale();

                var klarnaOrder = new KlarnaOrder
                {
                    Cart = cart,
                    Options = options,
                    Locale = supportedLocale.Locale,
                    PurchaseCountry = supportedLocale.PurchaseCountry,
                    PurchaseCurrency = supportedLocale.PurchaseCurrency
                };

                var order = new Order(connector, resourceUri)
                {
                    ContentType = ContentType
                };
                var dictData = klarnaOrder.ToDictionary();

                order.Update(dictData);

                return true;
            }
            catch (Exception ex)
            {
                var exceptionJson = JsonConvert.SerializeObject(ex.Data);

                _logger.Warning(string.Format(CultureInfo.CurrentCulture, "KlarnaCheckout: Error updating Klarna order. Will try to create a new one. ResourceURI: {0}, Data: {1}",
                    resourceUri, exceptionJson), exception: ex);
            }

            return false;
        }

        public void SyncBillingAndShippingAddress(global::Nop.Core.Domain.Customers.Customer customer, KlarnaOrder klarnaOrder)
        {
            try
            {
                var billingAddress = klarnaOrder.BillingAddress;
                var shippingAddress = klarnaOrder.ShippingAddress;

                var nopBillingAddress = customer.Addresses.FirstOrDefault(billingAddress.RepresentsAddress);
                if (nopBillingAddress == null)
                {
                    nopBillingAddress = new global::Nop.Core.Domain.Common.Address { CreatedOnUtc = DateTime.UtcNow };
                    customer.Addresses.Add(nopBillingAddress);
                }

                customer.BillingAddress = nopBillingAddress;
                billingAddress.CopyTo(nopBillingAddress);

                var nopShippingAddress = customer.Addresses.FirstOrDefault(shippingAddress.RepresentsAddress);
                if (nopShippingAddress == null)
                {
                    nopShippingAddress = new global::Nop.Core.Domain.Common.Address { CreatedOnUtc = DateTime.UtcNow };
                    customer.Addresses.Add(nopShippingAddress);
                }

                customer.ShippingAddress = nopShippingAddress;
                shippingAddress.CopyTo(nopShippingAddress);

                _customerService.UpdateCustomer(customer);
            }
            catch (Exception ex)
            {
                var billing = JsonConvert.SerializeObject(klarnaOrder.BillingAddress);
                var shipping = JsonConvert.SerializeObject(klarnaOrder.ShippingAddress);
                throw new KlarnaCheckoutException(string.Format(CultureInfo.CurrentCulture, "Error syncing addresses. Billing: {0}, Shipping: {1}", billing, shipping), ex);
            }
        }

        public bool CancelPayment(string reservation, global::Nop.Core.Domain.Customers.Customer customer)
        {
            try
            {
                var configuration = new Configuration(Country.Code.SE, Language.Code.SV, Currency.Code.SEK, Encoding.Sweden)
                {
                    Eid = _klarnaSettings.EId,
                    Secret = _klarnaSettings.SharedSecret,
                    IsLiveMode = !_klarnaSettings.TestMode
                };

                var api = new Api(configuration);
                var cancelled = api.CancelReservation(reservation);

                if (cancelled)
                {
                    _logger.Warning("KlarnaCheckout: Reservation cancelled: " + reservation, customer: customer);
                }

                return cancelled;
            }
            catch (Exception ex)
            {
                _logger.Error("KlarnaCheckout: Error cancelling reservation: " + reservation, exception: ex, customer: customer);
            }

            return false;
        }

        public ActivateReservationResponse Activate(string reservation, global::Nop.Core.Domain.Customers.Customer customer)
        {
            try
            {
                var configuration = new Configuration(Country.Code.SE, Language.Code.SV, Currency.Code.SEK, Encoding.Sweden)
                {
                    Eid = _klarnaSettings.EId,
                    Secret = _klarnaSettings.SharedSecret,
                    IsLiveMode = !_klarnaSettings.TestMode
                };

                var api = new Api(configuration);
                var result = api.Activate(reservation);

                _logger.Debug(string.Format(CultureInfo.InvariantCulture, "KlarnaCheckout: Reservation Activated: RiskStatus: {0}, InvoiceNumber: {1}",
                    result.RiskStatus, result.InvoiceNumber), customer: customer);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("KlarnaCheckout: Error activating reservation: " + reservation, exception: ex, customer: customer);
            }

            return null;
        }
    }
}
