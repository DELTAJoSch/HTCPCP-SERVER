using HTCPCP_Server.Database.Interfaces;
using HTCPCP_Server.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using Spectre.Console;
using System.Data.Entity;

namespace HTCPCP_Server.Database.Implementations
{
    /// <summary>
    /// A sqlite database driver
    /// </summary>
    internal class SQLiteDriver : IDatabaseDriver
    {
        private SQLiteConnection connection;

        /// <summary>
        /// Creates or opens the database
        /// </summary>
        public SQLiteDriver() {
            this.connection = new SQLiteConnection("Data Source = database.db; Version = 3; New = True; Compress = True;");
            this.connection.Open();
        }

        /// <inheritdoc/>
        public Task Add(Option option, string pot, int count = 1)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<bool> Consume(Option option, string pot)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<bool> Consume(Option option, string pot, int count)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Disposes of this driver
        /// </summary>
        public void Dispose()
        {
            this.connection.Close();
            this.connection.Dispose();
        }

        /// <inheritdoc/>
        public Task<Dictionary<string, Dictionary<Option, int>>> GetAsDict()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task InitDb(Dictionary<string, Dictionary<Option, int>> dict)
        {
            throw new NotImplementedException();
        }
    }
}
