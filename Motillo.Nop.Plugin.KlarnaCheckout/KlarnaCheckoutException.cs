using System;
using System.Runtime.Serialization;

namespace Motillo.Nop.Plugin.KlarnaCheckout
{
    [Serializable]
    public class KlarnaCheckoutException : Exception
    {
        public KlarnaCheckoutException()
        {
        }

        public KlarnaCheckoutException(string message) : base(message)
        {
        }

        public KlarnaCheckoutException(string message, Exception inner) : base(message, inner)
        {
        }

        protected KlarnaCheckoutException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
