using Motillo.Nop.Plugin.KlarnaCheckout.Data;
using Motillo.Nop.Plugin.KlarnaCheckout.Services;
using Newtonsoft.Json;
using Nop.Core.Data;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Plugins;
using Nop.Services.Events;
using Nop.Services.Logging;
using Nop.Services.Orders;
using System;
using System.Globalization;
using System.Linq;

namespace Motillo.Nop.Plugin.KlarnaCheckout
{
    public class OrderActivationPlugin : BasePlugin, IConsumer<ShipmentSentEvent>
    {
        private readonly IRepository<KlarnaCheckoutEntity> _repository;
        private readonly IKlarnaCheckoutPaymentService _klarnaCheckoutPaymentService;
        private readonly IOrderService _orderService;
        private readonly ILogger _logger;

        public OrderActivationPlugin(
            IRepository<KlarnaCheckoutEntity> repository,
            IKlarnaCheckoutPaymentService klarnaCheckoutPaymentService,
            IOrderService orderService,
            ILogger logger)
        {
            _repository = repository;
            _klarnaCheckoutPaymentService = klarnaCheckoutPaymentService;
            _orderService = orderService;
            _logger = logger;
        }

        public void HandleEvent(ShipmentSentEvent eventMessage)
        {
            var nopOrder = eventMessage.Shipment.Order;
            var klarnaRequest = _repository.Table.FirstOrDefault(x => x.OrderGuid == nopOrder.OrderGuid);

            // Ignore, might've been paid with paypal for example.
            // We could check the PaymentSystemMethodName, but these should be in sync.
            // Further down the klarnaRequest status needs to be updated.
            if (klarnaRequest == null)
            {
                return;
            }
            
            try
            {
                var reservation = nopOrder.AuthorizationTransactionId;

                // The Klarna reservation wasn't initially stored in AuthorizationTransactionId. Fall back to fetching it.
                if (string.IsNullOrEmpty(reservation))
                {
                    var resourceUri = new Uri(klarnaRequest.KlarnaResourceUri);
                    var order = _klarnaCheckoutPaymentService.Fetch(resourceUri);
                    var data = order.Marshal();
                    var jsonData = JsonConvert.SerializeObject(data);
                    var klarnaOrder = JsonConvert.DeserializeObject<Models.KlarnaOrder>(jsonData);

                    reservation = klarnaOrder.Reservation;
                }

                var activationResult = _klarnaCheckoutPaymentService.Activate(reservation, nopOrder.Customer);

                if (activationResult != null)
                {
                    nopOrder.OrderNotes.Add(new OrderNote
                    {
                        Note = string.Format(CultureInfo.CurrentCulture, "KlarnaCheckout: Klarna order activated because the order has been shipped. Reservation: {0}, RiskStatus: {1}, InvoiceNumber: {2}",
                        reservation, activationResult.RiskStatus, activationResult.InvoiceNumber),
                        CreatedOnUtc = DateTime.UtcNow,
                        DisplayToCustomer = false
                    });
                    _orderService.UpdateOrder(nopOrder);

                    klarnaRequest.Status = KlarnaCheckoutStatus.Activated;
                    _repository.Update(klarnaRequest);

                    _logger.Information(string.Format(CultureInfo.CurrentCulture, "KlarnaCheckout: Klarna order activated because the order has been shipped. OrderId: {0}, OrderGuid: {1}",
                        nopOrder.Id, nopOrder.OrderGuid));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format(CultureInfo.CurrentCulture, "KlarnaCheckout: Error activating order. OrderId: {0}, OrderGuid: {1}",
                    nopOrder.Id, nopOrder.OrderGuid), exception: ex, customer: nopOrder.Customer);
            }
        }
    }
}
