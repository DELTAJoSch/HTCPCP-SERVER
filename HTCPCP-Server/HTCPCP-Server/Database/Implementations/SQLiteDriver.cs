using HTCPCP_Server.Database.Interfaces;
using HTCPCP_Server.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using System.Reflection.Metadata.Ecma335;
using HTCPCP_Server.Logging;
using System.Transactions;

namespace HTCPCP_Server.Database.Implementations
{
    /// <summary>
    /// A sqlite database driver
    /// </summary>
    internal class SQLiteDriver : IDatabaseDriver
    {
        private SqliteConnection connection;

        /// <summary>
        /// Creates or opens the database
        /// </summary>
        public SQLiteDriver()
        {
            this.connection = new SqliteConnection("Data Source=database.db;Mode=ReadWriteCreate;");
            this.connection.Open();
        }

        /// <inheritdoc/>
        public async Task Add(Option option, string pot, int count = 1)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task<bool> Consume(Option option, string pot)
        {
            return await this.Consume(option, pot, 1);
        }

        /// <inheritdoc/>
        public async Task<bool> Consume(Option option, string pot, int count)
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
        public async Task<Dictionary<string, Dictionary<Option, int>>?> GetAsDict()
        {
            var res = new Dictionary<string, Dictionary<Option, int>>();

            try
            {
                string getCommand = "SELECT * FROM Options";
                SqliteCommand get = new SqliteCommand(getCommand, this.connection);
                var reader = await get.ExecuteReaderAsync();
                while(reader.Read())
                {
                    var pot = res.GetValueOrDefault(reader.GetString(0));
                    if (pot == null)
                        pot = new Dictionary<Option, int>();

                    pot.Add((Option)Enum.Parse(typeof(Option), reader.GetString(1)), reader.GetInt32(2));
                    res.Add(reader.GetString(0), pot);
                }
            }
            catch (SqliteException e)
            {
                Log.Error($"Failed to start transaction: ({e.SqliteErrorCode}, {e.Message}, {e.SqlState})");
                return null;
            }

            return res;
        }

        /// <inheritdoc/>
        public async Task<bool> InitDb(Dictionary<string, Dictionary<Option, int>> dict)
        {
            SqliteTransaction? transaction = null;
            try
            {
               transaction = this.connection.BeginTransaction(System.Data.IsolationLevel.Serializable);
            }
            catch (SqliteException e)
            {
                if (e.SqliteErrorCode == 11)
                {
                    Log.Error("The database file is corrupted!");
                    return false;
                }

                Log.Error($"Failed to start transaction: ({e.SqliteErrorCode}, {e.Message}, {e.SqlState})");
                return false;
            }

            
            SqliteCommand drop;
            string dropCommand = "DROP TABLE IF EXISTS Options;";
            drop = new SqliteCommand(dropCommand, this.connection, transaction);

            SqliteCommand create;
            string createCommand = "CREATE TABLE IF NOT EXISTS Options (pot VARCHAR NOT NULL, opt VARCHAR NOT NULL, cnt INTEGER NOT NULL DEFAULT 0, CONSTRAINT pk_Options PRIMARY KEY (pot, opt));";
            create = new SqliteCommand(createCommand, this.connection, transaction);
            try
            {
                await drop.ExecuteNonQueryAsync();
            }
            catch (SqliteException e)
            {
               if (e.SqliteErrorCode == 13)
                {
                    Log.Error("The disk is full!");
                    transaction.Rollback();
                    return false;
                }

                Log.Error($"Failed to create table: {e.SqliteErrorCode}, {e.Message}, {e.SqlState}");
                transaction.Rollback();
                return false;
            }

            try
            {
                await create.ExecuteNonQueryAsync();
            }
            catch (SqliteException e)
            {
                if(e.SqliteErrorCode == 13)
                {
                    Log.Error("The disk is full!");
                    transaction.Rollback();
                    return false;
                }

                Log.Error($"Failed to create table: {e.SqliteErrorCode}, {e.Message}, {e.SqlState}");
                transaction.Rollback();
                return false;
            }

            string insertCommand = "INSERT INTO Options @pot, @opt, @cnt";
            SqliteCommand insert = new SqliteCommand(insertCommand, this.connection, transaction);
            insert.Parameters.Add("@pot", SqliteType.Text);
            insert.Parameters.Add("@opt", SqliteType.Text);
            insert.Parameters.Add("@cnt", SqliteType.Integer);

            try
            {
                dict.AsEnumerable()
                    .ToList()
                    .ForEach(x =>
                    {
                        insert.Parameters["@pot"].Value = x.Key;
                        x.Value.AsEnumerable()
                            .ToList()
                            .ForEach(async y =>
                            {
                                try
                                {
                                    insert.Parameters["@opt"].Value = y.Key.ToString();
                                    insert.Parameters["@cnt"].Value = y.Value;
                                    await insert.ExecuteReaderAsync();
                                }
                                catch (SqliteException e)
                                {
                                    if (e.SqliteErrorCode == 18)
                                    {
                                        Log.Warn($"Ignoring Tupel ({x.Key},{y.Key},{y.Value}) - Too big!");
                                    }
                                    else if (e.SqliteErrorCode == 19)
                                    {
                                        Log.Warn($"Ignoring Tuple ({x.Key},{y.Key},{y.Value}) - Constraint violated!");
                                    }
                                    else
                                    {
                                        throw;
                                    }
                                }
                            });
                    });
            }
            catch (SqliteException e)
            {
                Log.Error($"Failed to create table: {e.SqliteErrorCode}, {e.Message}, {e.SqlState}");
                transaction.Rollback();
                return false;
            }

            transaction.Commit();
            return true;
        }
    }
}
