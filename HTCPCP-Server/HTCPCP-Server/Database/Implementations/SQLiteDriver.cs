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
            await this.ConsumeOrAdd(option, pot, count, true);
        }

        /// <inheritdoc/>
        public async Task<bool> Consume(Option option, string pot)
        {
            return await this.Consume(option, pot, 1);
        }

        /// <inheritdoc/>
        public async Task<bool> Consume(Option option, string pot, int count)
        {
            return await this.ConsumeOrAdd(option, pot, count);
        }

        /// <summary>
        /// Consume or add to the given pot and option
        /// </summary>
        /// <param name="option">The option</param>
        /// <param name="pot">The pot</param>
        /// <param name="count">The number of elements to add or add</param>
        /// <param name="add">If false, consume count, otherwise add</param>
        /// <returns>Returns true if successful</returns>
        private async Task<bool> ConsumeOrAdd(Option option, string pot, int count, bool add = false) {
            string select = "SELECT * FROM Options WHERE (pot = $pt) AND (opt = $ot)";
            SqliteCommand selectCommand = new SqliteCommand(select, this.connection);
            selectCommand.Parameters.AddWithValue("$pt", pot);
            selectCommand.Parameters.AddWithValue("$ot", option.ToString());

            try
            {
                var reader = await selectCommand.ExecuteReaderAsync();
                if (reader.Read())
                {
                    var cnt = reader.GetInt32(2);

                    if (add)
                    {
                        cnt += count;
                    }
                    else
                    {
                        if (cnt - count >= 0)
                        {
                            cnt -= count;
                        }
                        else
                        {
                            return false;
                        }
                    }

                    string insert = "UPDATE Options SET cnt = $ct WHERE (pot = $pt) AND (opt = $ot)";
                    SqliteCommand insertCommand = new SqliteCommand(insert, this.connection);
                    insertCommand.Parameters.AddWithValue("$pt", pot);
                    insertCommand.Parameters.AddWithValue("$ot", option.ToString());
                    insertCommand.Parameters.AddWithValue("$ct", cnt);

                    await insertCommand.ExecuteNonQueryAsync();
                }
                else
                {
                    return false;
                }
            }
            catch (SqliteException e)
            {
                Log.Error($"Failed to read: ({e.SqliteErrorCode}, {e.Message}, {e.SqlState})");
                return false;
            }

            return true;
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
                    // get value for pot (null if not yet created) and add or create dictionary. Then add it to res, if it is not already part of it
                    var pot = res.GetValueOrDefault(reader.GetString(0));
                    if (pot == null)
                        pot = new Dictionary<Option, int>();

                    pot.Add((Option)Enum.Parse(typeof(Option), reader.GetString(1)), reader.GetInt32(2));

                    if (!res.ContainsKey(reader.GetString(0))) 
                    {
                        res.Add(reader.GetString(0), pot);
                    }
                }
            }
            catch (SqliteException e)
            {
                Log.Error($"Failed to read: ({e.SqliteErrorCode}, {e.Message}, {e.SqlState})");
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

            string insertCommand = "INSERT INTO Options VALUES ($pot, $opt, $cnt)";
            SqliteCommand insert = new SqliteCommand(insertCommand, this.connection, transaction);

            try
            {
                dict.AsEnumerable()
                    .ToList()
                    .ForEach(x =>
                    {
                        x.Value.AsEnumerable()
                            .ToList()
                            .ForEach(async y =>
                            {
                                try
                                {
                                    insert.Parameters.Clear();
                                    insert.Parameters.AddWithValue("$pot", x.Key);
                                    insert.Parameters.AddWithValue("$opt", y.Key.ToString());
                                    insert.Parameters.AddWithValue("$cnt", y.Value);
                                    await insert.ExecuteNonQueryAsync();
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
