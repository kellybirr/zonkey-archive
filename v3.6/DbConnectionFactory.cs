using System;
using System.Configuration;
using System.Data.Common;

namespace Zonkey
{
    /// <summary>
    /// Provides methods for managing SQL database connections
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase")]
    public static class DbConnectionFactory
    {
        /// <summary>
        /// Creates and Opens a new connection configured based on a named ConnectionString in the App.Config/Web.Config
        /// </summary>
        /// <param name="name">the name of the ConnectionString element from the config file</param>
        /// <returns>an open DbConnection</returns>
        public static DbConnection OpenConnection(string name)
        {
            var cnxn = CreateConnection(name);
            cnxn.Open();

            return cnxn;
        }

        /// <summary>
        /// Creates, but does not open a new connection configured based on a named ConnectionString in the App.Config/Web.Config
        /// </summary>
        /// <param name="name">the name of the ConnectionString element from the config file</param>
        /// <returns>an open DbConnection</returns>
        public static DbConnection CreateConnection(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[name];
            if (settings == null)
                throw new InvalidOperationException(string.Format("A ConnectionString element named '{0}' was not found in the App.Config/Web.Config file", name));

            if (string.IsNullOrEmpty(settings.ProviderName))
                throw new InvalidOperationException("The ConnectionString element must define a provider name for use with this method");

            return CreateConnection(settings.ProviderName, settings.ConnectionString);
        }

        /// <summary>
        /// Creates and Opens a new connection based on the given ProviderName and ConnectionString.
        /// </summary>
        /// <remarks>
        /// Always opens the connection.
        /// </remarks>
        /// <param name="providerName">Name of the provider.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>A <see cref="System.Data.Common.DbConnection"/> object with the connection information.</returns>
        public static DbConnection OpenConnection(string providerName, string connectionString)
        {
            var cnxn = CreateConnection(providerName, connectionString);
            cnxn.Open();

            return cnxn;
        }

        /// <summary>
        /// Creates, but does not open a new connection based on the given ProviderName and ConnectionString.
        /// </summary>
        /// <param name="providerName">Name of the provider.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>A <see cref="System.Data.Common.DbConnection"/> object with the connection information.</returns>
        public static DbConnection CreateConnection(string providerName, string connectionString)
        {
            if (string.IsNullOrEmpty(providerName)) throw new ArgumentNullException("providerName");
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException("connectionString");

            DbProviderFactory providerFactory = DbProviderFactories.GetFactory(providerName);
            DbConnection cnxn = providerFactory.CreateConnection();
            cnxn.ConnectionString = connectionString;

            return cnxn;
        }
    }
}