using System.Data.SqlClient;
using Nop.Core;
using Nop.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Motillo.Nop.Plugin.KlarnaCheckout.Data
{
    public class KlarnaCheckoutContext : DbContext, IDbContext
    {
        public const string TableName = "Motillo_KlarnaCheckout";

        public KlarnaCheckoutContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
            Database.SetInitializer<KlarnaCheckoutContext>(null);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new KlarnaCheckoutEntityConfiguration());

            base.OnModelCreating(modelBuilder);
        }

        public string CreateDatabaseInstallationScript()
        {
            return ((IObjectContextAdapter)this).ObjectContext.CreateDatabaseScript();
        }

        public void Install()
        {
            Database.SetInitializer<KlarnaCheckoutContext>(null);

            Database.ExecuteSqlCommand(CreateDatabaseInstallationScript());
            SaveChanges();
        }

        public void Uninstall()
        {
            const string sql = @"DROP TABLE " + TableName;

            Database.ExecuteSqlCommand(sql);
            SaveChanges();
        }

        public IDbSet<TEntity> Set<TEntity>() where TEntity : BaseEntity
        {
            return base.Set<TEntity>();
        }

        public IList<TEntity> ExecuteStoredProcedureList<TEntity>(string commandText, params object[] parameters) where TEntity : BaseEntity, new()
        {
            throw new NotSupportedException();
        }

        public IEnumerable<TElement> SqlQuery<TElement>(string sql, params object[] parameters)
        {
            throw new NotSupportedException();
        }

        public int ExecuteSqlCommand(string sql, bool doNotEnsureTransaction = false, int? timeout = null, params object[] parameters)
        {
            throw new NotSupportedException();
        }

        public int ExecuteSqlCommand(string sql, int? timeout = null, params object[] parameters)
        {
            throw new NotSupportedException();
        }
    }
}
