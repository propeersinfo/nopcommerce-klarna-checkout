using System.Collections.Generic;
using Nop.Core.Configuration;
using Nop.Web.Framework;

namespace Motillo.Nop.Plugin.KlarnaCheckout
{
    public class KlarnaCheckoutSettings : ISettings
    {
        [NopResourceDisplayName("Motillo.Plugin.KlarnaCheckout.Settings.EId")]
        public int EId { get; set; }

        [NopResourceDisplayName("Motillo.Plugin.KlarnaCheckout.Settings.SharedSecret")]
        public string SharedSecret { get; set; }

        [NopResourceDisplayName("Motillo.Plugin.KlarnaCheckout.Settings.EnabledCountries")]
        public string EnabledCountries { get; set; }

        [NopResourceDisplayName("Motillo.Plugin.KlarnaCheckout.Settings.TermsUrl")]
        public string TermsUrl { get; set; }

        [NopResourceDisplayName("Motillo.Plugin.KlarnaCheckout.Settings.CheckoutUrl")]
        public string CheckoutUrl { get; set; }

        [NopResourceDisplayName("Motillo.Plugin.KlarnaCheckout.Settings.DisableAutofocus")]
        public bool DisableAutofocus { get; set; }

        [NopResourceDisplayName("Motillo.Plugin.KlarnaCheckout.Settings.AllowSeparateShippingAddress")]
        public bool AllowSeparateShippingAddress { get; set; }

        [NopResourceDisplayName("Motillo.Plugin.KlarnaCheckout.Settings.TestMode")]
        public bool TestMode { get; set; }

        [NopResourceDisplayName("Motillo.Plugin.KlarnaCheckout.Settings.ColorButton")]
        public string ColorButton { get; set; }

        [NopResourceDisplayName("Motillo.Plugin.KlarnaCheckout.Settings.ColorButtonText")]
        public string ColorButtonText { get; set; }

        [NopResourceDisplayName("Motillo.Plugin.KlarnaCheckout.Settings.ColorCheckbox")]
        public string ColorCheckbox { get; set; }

        [NopResourceDisplayName("Motillo.Plugin.KlarnaCheckout.Settings.ColorCheckboxCheckmark")]
        public string ColorCheckboxCheckmark { get; set; }

        [NopResourceDisplayName("Motillo.Plugin.KlarnaCheckout.Settings.ColorHeader")]
        public string ColorHeader { get; set; }

        [NopResourceDisplayName("Motillo.Plugin.KlarnaCheckout.Settings.ColorLink")]
        public string ColorLink { get; set; }
    }
}
