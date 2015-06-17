using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Motillo.Nop.Plugin.KlarnaCheckout.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        private const string HexPattern = @"^#(?:[0-9a-fA-F]{3}){1,2}$";

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
        [RegularExpression(HexPattern)]
        public string ColorButton { get; set; }

        [NopResourceDisplayName("Motillo.Plugin.KlarnaCheckout.Settings.ColorButtonText")]
        [RegularExpression(HexPattern)]
        public string ColorButtonText { get; set; }

        [NopResourceDisplayName("Motillo.Plugin.KlarnaCheckout.Settings.ColorCheckbox")]
        [RegularExpression(HexPattern)]
        public string ColorCheckbox { get; set; }

        [NopResourceDisplayName("Motillo.Plugin.KlarnaCheckout.Settings.ColorCheckboxCheckmark")]
        [RegularExpression(HexPattern)]
        public string ColorCheckboxCheckmark { get; set; }

        [NopResourceDisplayName("Motillo.Plugin.KlarnaCheckout.Settings.ColorHeader")]
        [RegularExpression(HexPattern)]
        public string ColorHeader { get; set; }

        [NopResourceDisplayName("Motillo.Plugin.KlarnaCheckout.Settings.ColorLink")]
        [RegularExpression(HexPattern)]
        public string ColorLink { get; set; }
    }
}
