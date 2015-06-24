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

		  #region Ctor
		  public KlarnaCheckoutContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
			  // PPIS
           // Database.SetInitializer<KlarnaCheckoutContext>(null);
        }
		  #endregion


		  #region Utilities
		  protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new KlarnaCheckoutEntityConfiguration());
				//disable EdmMetadata generation
				//modelBuilder.Conventions.Remove<IncludeMetadataConvention>();
            base.OnModelCreating(modelBuilder);
        }

		  #endregion

		  #region Methods

		  public string CreateDatabaseInstallationScript()
		  {
			  return ((IObjectContextAdapter)this).ObjectContext.CreateDatabaseScript();
		  }

		  //public IDbSet<TEntity> Set<TEntity>() where TEntity : BaseEntity
		  //{
		  //	return base.Set<TEntity>();
		  //}

		  //PPIS
		  public new IDbSet<TEntity> Set<TEntity>() where TEntity : BaseEntity
		  {
			  return base.Set<TEntity>();
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

		  //PPIS
		  /// <summary>
		  /// Detach an entity
		  /// </summary>
		  /// <param name="entity">Entity</param>
		  public void Detach(object entity)
		  {
			  if (entity == null)
				  throw new ArgumentNullException("entity");

			  ((IObjectContextAdapter)this).ObjectContext.Detach(entity);
		  }
		  #endregion
		  // PPIS
		  #region Properties

		  /// <summary>
		  /// Gets or sets a value indicating whether proxy creation setting is enabled (used in EF)
		  /// </summary>
		  public virtual bool ProxyCreationEnabled
		  {
			  get
			  {
				  return this.Configuration.ProxyCreationEnabled;
			  }
			  set
			  {
				  this.Configuration.ProxyCreationEnabled = value;
			  }
		  }

		  /// <summary>
		  /// Gets or sets a value indicating whether auto detect changes setting is enabled (used in EF)
		  /// </summary>
		  public virtual bool AutoDetectChangesEnabled
		  {
			  get
			  {
				  return this.Configuration.AutoDetectChangesEnabled;
			  }
			  set
			  {
				  this.Configuration.AutoDetectChangesEnabled = value;
			  }
		  }

		  #endregion
    }
}
