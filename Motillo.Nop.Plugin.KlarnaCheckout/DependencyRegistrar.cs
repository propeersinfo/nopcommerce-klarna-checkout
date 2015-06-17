using Autofac;
using Autofac.Core;
using Motillo.Nop.Plugin.KlarnaCheckout.Data;
using Motillo.Nop.Plugin.KlarnaCheckout.Services;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Web.Framework.Mvc;

namespace Motillo.Nop.Plugin.KlarnaCheckout
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        private const string ContextName = "Motillo_Context_KlarnaCheckoutRequest";

        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            builder.RegisterType<KlarnaCheckoutHelper>().As<IKlarnaCheckoutHelper>().InstancePerLifetimeScope();
            builder.RegisterType<KlarnaCheckoutPaymentService>().As<IKlarnaCheckoutPaymentService>().InstancePerLifetimeScope();

            //data context
            this.RegisterPluginDataContext<KlarnaCheckoutContext>(builder, ContextName);

            //override required repository with our custom context
            builder.RegisterType<EfRepository<KlarnaCheckoutEntity>>()
                .As<IRepository<KlarnaCheckoutEntity>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(ContextName))
                .InstancePerLifetimeScope();
        }

        public int Order
        {
            get { return 1; }
        }
    }
}
