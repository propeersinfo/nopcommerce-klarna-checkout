<h1>Klanra Checkout plugin for nop commerce 3.60 by Professionals Peers Info Services Pvt. Ltd.</h1>

Features:

    Implemented as a nopCommerce payment method.
    Test/live mode.
    Configuration of snippet colors.
    Automatic activation of the Klarna order when the nop order is marked as shipped.
    Works with the "Google Analytics or Universal Analytics" widget.

Premium Support:
Need help? Our Premium Support will get you up and running in no time. Contact us for pricing and more information by dropping an email to info@propeersinfo.com.

Installation:
Installation can be done either from the precompiled package or directly from source. The easiest way is to use the package.

After installation, make sure to edit the Checkout/Completed.cshtml view so that the Klarna confirmation snippet is shown.

@if (TempData["KlarnaSnippet"] is string)
{
    <div class="row">
        @Html.Raw((string)TempData["KlarnaSnippet"])
    </div>
}

From Package
    Download zip.
    Unzip the content into the Plugins folder.
    Install and configure the plugin through nopCommerce's administration interface.

From Source
    Clone the repository
    Make a link from the cloned src directory to the Plugins folder in your nop project.
        Open a command prompt (admin privileges might be required)
        Type: mklink /J C:\Path\To\nop\Project\Plugins\Motillo.Nop.Plugin.KlarnaCheckout C:\Path\To\Cloned\klarna-checkout-nopcommerce\src
    Within the solution, right click the Plugins folder and add a reference to the existing project.
    Rebuild solution.
    Install and configure the plugin through nopCommerce's administration interface.

After making changes to the plugin, make sure you rebuild the project. Every time the plugin is rebuilt, necessary content is copied to Presentation/Nop.Web/Plugins.
Important Notes

This plugin was created for a specific case where the Klarna Checkout snippet is rendered directly inside the payment info area, which means that the customer won't be redirected to a specific payment page. Therefor nop's ordinary "Complete Payment" button needs to be disabled/hidden when using the plugin, since Klarna has its own "Complete Payment" button. Otherwise you will end up with an unpaid order.

This plugin is inspired by Klarna Checkout by Majako. The Updates features are implemented by Professionals Peers Info Services Pvt. Ltd. http://www.propeersinfo.com
