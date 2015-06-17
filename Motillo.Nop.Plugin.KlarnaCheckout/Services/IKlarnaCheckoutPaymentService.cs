using Klarna.Api;
using Motillo.Nop.Plugin.KlarnaCheckout.Models;
using Nop.Core.Domain.Orders;
using System;

namespace Motillo.Nop.Plugin.KlarnaCheckout.Services
{
    public interface IKlarnaCheckoutPaymentService
    {
        void Acknowledge(Uri resourceUri, Order order);
        Uri Create();
        Klarna.Checkout.Order Fetch(Uri resourceUri);
        bool Update(Uri resourceUri);
        void SyncBillingAndShippingAddress(global::Nop.Core.Domain.Customers.Customer customer, KlarnaOrder klarnaOrder);
        bool CancelPayment(string reservation, global::Nop.Core.Domain.Customers.Customer customer);
        ActivateReservationResponse Activate(string reservation, global::Nop.Core.Domain.Customers.Customer customer);
    }
}
