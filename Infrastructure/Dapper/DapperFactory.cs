//using Application.Abstractions.Data;
//using Application.Dapper;
//using Application.Helper;
//using Dapper;
//using Microsoft.Data.SqlClient;
//using Microsoft.Extensions.Configuration;
//using Newtonsoft.Json;
//using System.Data;
//using System.Data.Common;
//using Z.Dapper.Plus;

//namespace Infrastructure.Dapper
//{
//    /// <summary>
//    /// Provides Dapper-based data access services.
//    /// </summary>
//    public class DapperFactory : IDapperFactory
//    {
//        private readonly IConfiguration _configuration;
//        private readonly string _connectionstring = "Database";
//        private readonly AppSettings _appSettings;
//        private readonly IHandleException _iHandleException;

//        /// <summary>
//        /// Initializes a new instance of the <see cref="DapperServices"/> class.
//        /// </summary>
//        /// <param name="config">The configuration.</param>
//        /// <param name="logger">The logger.</param>
//        public DapperFactory(IConfiguration configuration, IHandleException iHandleException)
//        {
//            _configuration = configuration;
//            _appSettings = _configuration.GetSection("AppSettings").Get<AppSettings>() ?? throw new ArgumentNullException(nameof(configuration));
//            _connectionstring = _configuration.GetConnectionString("Database") ?? throw new ArgumentNullException(nameof(configuration));
//            _iHandleException = iHandleException;
//        }

//        /// <summary>
//        /// Disposes the resources.
//        /// </summary>
//        public void Dispose()
//        {
//            // No resources to dispose
//        }

//        /// <summary>
//        /// Gets the database connection.
//        /// </summary>
//        /// <param name="connecString">The connection string.</param>
//        /// <returns>The database connection.</returns>
//        public DbConnection GetDbConnection(string? connectionString = null)
//        {
//            if (string.IsNullOrEmpty(connectionString))
//            {
//                connectionString = _connectionstring;
//            }
//            return new SqlConnection(_configuration.GetConnectionString(connectionString));
//        }

//        public IDbConnection CreateConnection()
//        {
//            var connection = new SqlConnection(_connectionstring);
//            connection.Open();

//            return connection;
//        }

//        /// <summary>
//        /// Executes a stored procedure asynchronously.
//        /// </summary>
//        /// <param name="sp">The stored procedure name.</param>
//        /// <param name="parms">The parameters.</param>
//        /// <param name="commandType">The command type.</param>
//        /// <param name="connecString">The connection string.</param>
//        /// <returns>The number of affected rows.</returns>
//        public async Task<int> ExecuteAsync(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, string? connecString = null)
//        {
//            int result;
//            using IDbConnection db = GetDbConnection(connecString);
//            if (db.State == ConnectionState.Closed)
//                db.Open();

//            using var tran = db.BeginTransaction();
//            try
//            {
//                result = await db.ExecuteAsync(sp, parms, commandType: commandType, transaction: tran);
//                tran.Commit();
//            }
//            catch (Exception ex)
//            {
//                tran.Rollback();
//                await _iHandleException.LogEvents(ex);
//                throw;
//            }

//            return result;
//        }

//        /// <summary>
//        /// Loads data asynchronously.
//        /// </summary>
//        /// <typeparam name="T">The type of the parameters.</typeparam>
//        /// <param name="storedProcedure">The stored procedure name.</param>
//        /// <param name="parameters">The parameters.</param>
//        /// <param name="connecString">The connection string.</param>
//        /// <returns>A task representing the asynchronous operation.</returns>
//        public async Task LoadDataAsync<T>(string storedProcedure, T parameters, string connecString)
//        {
//            using IDbConnection db = GetDbConnection(connecString);
//            await db.ExecuteAsync(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
//        }

//        /// <summary>
//        /// Executes a stored procedure asynchronously.
//        /// </summary>
//        /// <typeparam name="T">The type of the result.</typeparam>
//        /// <param name="sp">The stored procedure name.</param>
//        /// <param name="parms">The parameters.</param>
//        /// <param name="commandType">The command type.</param>
//        /// <param name="connecString">The connection string.</param>
//        /// <returns>The number of affected rows.</returns>
//        public async Task<int> ExecuteAsync<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, string? connecString = null)
//        {
//            int result;
//            using IDbConnection db = GetDbConnection(connecString);
//            if (db.State == ConnectionState.Closed)
//                db.Open();

//            using var tran = db.BeginTransaction();
//            try
//            {
//                result = await db.ExecuteAsync(sp, parms, commandType: commandType, transaction: tran);
//                tran.Commit();
//            }
//            catch (Exception ex)
//            {
//                tran.Rollback();
//                await _iHandleException.LogEvents(ex);
//                throw;
//            }

//            return result;
//        }

//        /// <summary>
//        /// Queries a single record asynchronously.
//        /// </summary>
//        /// <typeparam name="T">The type of the result.</typeparam>
//        /// <param name="sp">The stored procedure name.</param>
//        /// <param name="parms">The parameters.</param>
//        /// <param name="commandType">The command type.</param>
//        /// <param name="connecString">The connection string.</param>
//        /// <returns>The queried record.</returns>
//        public async Task<int> QuerySingleAsync(string procedureName, DynamicParameters parameters,
//            CommandType commandType = CommandType.StoredProcedure, string? connectionString = null)
//        {
//            await using var db = GetDbConnection(connectionString);

//            if (db.State == ConnectionState.Closed)
//                await db.OpenAsync();

//            using var tran = db.BeginTransaction();
//            try
//            {
//                int result = await db.QuerySingleAsync(procedureName, parameters, commandType: commandType, transaction: tran).ConfigureAwait(false);
//                tran.Commit();
//                return result;
//            }
//            catch (Exception ex)
//            {
//                tran.Rollback();
//                await _iHandleException.LogEvents(ex);
//                throw new Exception(ex.InnerException?.Message ?? ex.Message);
//            }
//        }

//        public async Task<T> QuerySingleAsync<T>(string procedureName, DynamicParameters parameters,
//            CommandType commandType = CommandType.StoredProcedure, string? connectionString = null)
//        {
//            await using var dbConnection = GetDbConnection(connectionString);
//            if (dbConnection.State == ConnectionState.Closed)
//                await dbConnection.OpenAsync();

//            await using var transaction = await dbConnection.BeginTransactionAsync();

//            try
//            {
//                var result = await dbConnection.QuerySingleAsync<T>(procedureName, parameters, commandType: commandType, transaction: transaction).ConfigureAwait(false);
//                await transaction.CommitAsync();

//                return result;
//            }
//            catch (Exception ex)
//            {
//                await transaction.RollbackAsync();
//                await _iHandleException.LogEvents(ex);
//                throw new Exception(ex.InnerException?.Message ?? ex.Message);
//            }
//        }

//        /// <summary>
//        /// Gets a single record asynchronously.
//        /// </summary>
//        /// <typeparam name="T">The type of the result.</typeparam>
//        /// <param name="sp">The stored procedure name.</param>
//        /// <param name="parms">The parameters.</param>
//        /// <param name="commandType">The command type.</param>
//        /// <param name="connecString">The connection string.</param>
//        /// <returns>The queried record.</returns>
//        public async Task<T?> GetAsync<T>(string sp, DynamicParameters? parms, CommandType commandType = CommandType.StoredProcedure, string? connecString = null)
//        {
//            using IDbConnection db = GetDbConnection(connecString);
//            var result = await db.QueryAsync<T>(sp, parms, commandType: commandType);
//            return result.FirstOrDefault();
//        }

//        /// <summary>
//        /// Gets all records asynchronously.
//        /// </summary>
//        /// <typeparam name="T">The type of the result.</typeparam>
//        /// <param name="sp">The stored procedure name.</param>
//        /// <param name="parms">The parameters.</param>
//        /// <param name="commandType">The command type.</param>
//        /// <param name="connecString">The connection string.</param>
//        /// <returns>A list of queried records.</returns>
//        public async Task<List<T>> GetAllAsync<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, string? connecString = null)
//        {
//            using IDbConnection db = GetDbConnection(connecString);
//            var result = await db.QueryAsync<T>(sp, parms, commandType: commandType);
//            return [.. result];
//        }

//        /// <summary>
//        /// Gets all records as a queryable asynchronously.
//        /// </summary>
//        /// <typeparam name="T">The type of the result.</typeparam>
//        /// <param name="sp">The stored procedure name.</param>
//        /// <param name="parms">The parameters.</param>
//        /// <param name="commandType">The command type.</param>
//        /// <param name="connecString">The connection string.</param>
//        /// <returns>A queryable list of queried records.</returns>
//        public async Task<IQueryable<T>> GetAllAsQueryableAsync<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, string connecString = null)
//        {
//            using IDbConnection db = GetDbConnection(connecString);
//            var result = await db.QueryAsync<T>(sp, parms, commandType: commandType);
//            return result.ToList().AsQueryable();
//        }

//        /// <summary>
//        /// Gets all records asynchronously.
//        /// </summary>
//        /// <typeparam name="T">The type of the result.</typeparam>
//        /// <param name="sp">The stored procedure name.</param>
//        /// <param name="parms">The parameters.</param>
//        /// <param name="connecString">The connection string.</param>
//        /// <param name="commandType">The command type.</param>
//        /// <returns>A list of queried records.</returns>
//        public async Task<List<T>> GetAllsAsync<T>(string sp, DynamicParameters parms, string connecString, CommandType commandType = CommandType.StoredProcedure)
//        {
//            using (IDbConnection db = new SqlConnection(connecString))
//            {
//                var result = await db.QueryAsync<T>(sp, parms, commandType: commandType);
//                return result.ToList();
//            }
//        }

//        /// <summary>
//        /// Inserts a record asynchronously.
//        /// </summary>
//        /// <typeparam name="T">The type of the result.</typeparam>
//        /// <param name="sp">The stored procedure name.</param>
//        /// <param name="parms">The parameters.</param>
//        /// <param name="commandType">The command type.</param>
//        /// <param name="connecString">The connection string.</param>
//        /// <returns>The inserted record.</returns>
//        public async Task<T> InsertAsync<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, string connecString = null)
//        {
//            T result;
//            using IDbConnection db = GetDbConnection(connecString);
//            try
//            {
//                if (db.State == ConnectionState.Closed)
//                    db.Open();

//                using var tran = db.BeginTransaction();
//                try
//                {
//                    var queryResult = await db.QueryAsync<T>(sp, parms, commandType: commandType, transaction: tran);
//                    result = queryResult.FirstOrDefault();
//                    tran.Commit();
//                }
//                catch (Exception ex)
//                {
//                    tran.Rollback();
//                    await _iHandleException.LogEvents(ex);
//                    throw;
//                }
//            }
//            catch (Exception ex)
//            {
//                throw;
//            }
//            finally
//            {
//                if (db.State == ConnectionState.Open)
//                    db.Close();
//            }

//            return result;
//        }

//        /// <summary>
//        /// Inserts a record asynchronously.
//        /// </summary>
//        /// <typeparam name="T">The type of the result.</typeparam>
//        /// <param name="sp">The stored procedure name.</param>
//        /// <param name="parms">The parameters.</param>
//        /// <param name="commandType">The command type.</param>
//        /// <param name="connecString">The connection string.</param>
//        /// <returns>The inserted record.</returns>
//        public async Task<T> InsertsAsync<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.Text, string connecString = null)
//        {
//            T result;
//            using IDbConnection db = GetDbConnection(connecString);
//            try
//            {
//                if (db.State == ConnectionState.Closed)
//                    db.Open();

//                using var tran = db.BeginTransaction();
//                try
//                {
//                    var queryResult = await db.QueryAsync<T>(sp, parms, commandType: commandType, transaction: tran);
//                    result = queryResult.FirstOrDefault();
//                    tran.Commit();
//                }
//                catch (Exception ex)
//                {
//                    tran.Rollback();
//                    await _iHandleException.LogEvents(ex);
//                    throw;
//                }
//            }
//            catch (Exception ex)
//            {
//                throw;
//            }
//            finally
//            {
//                if (db.State == ConnectionState.Open)
//                    db.Close();
//            }

//            return result;
//        }

//        /// <summary>
//        /// Updates a record asynchronously.
//        /// </summary>
//        /// <typeparam name="T">The type of the result.</typeparam>
//        /// <param name="sp">The stored procedure name.</param>
//        /// <param name="parms">The parameters.</param>
//        /// <param name="commandType">The command type.</param>
//        /// <param name="connecString">The connection string.</param>
//        /// <returns>The updated record.</returns>
//        public async Task<T> UpdateAsync<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, string connecString = null)
//        {
//            T result;
//            using IDbConnection db = GetDbConnection(connecString);
//            try
//            {
//                if (db.State == ConnectionState.Closed)
//                    db.Open();

//                using var tran = db.BeginTransaction();
//                try
//                {
//                    var queryResult = await db.QueryAsync<T>(sp, parms, commandType: commandType, transaction: tran);
//                    result = queryResult.FirstOrDefault();
//                    tran.Commit();
//                }
//                catch (Exception ex)
//                {
//                    tran.Rollback();
//                    await _iHandleException.LogEvents(ex);
//                    throw;
//                }
//            }
//            catch (Exception ex)
//            {
//                throw;
//            }
//            finally
//            {
//                if (db.State == ConnectionState.Open)
//                    db.Close();
//            }

//            return result;
//        }

//        /// <summary>
//        /// Performs bulk insert asynchronously.
//        /// </summary>
//        /// <typeparam name="T">The type of the parameters.</typeparam>
//        /// <param name="parms">The parameters.</param>
//        /// <param name="connecString">The connection string.</param>
//        /// <returns>The number of affected rows.</returns>
//        public async Task<int> BulkInsertAsync<T>(DynamicParameters parms, string connecString = null)
//        {
//            int result;
//            using (IDbConnection db = GetDbConnection(connecString))
//            {
//                if (db.State == ConnectionState.Closed)
//                    db.Open();

//                var tran = db.BeginTransaction();
//                try
//                {
//                    // Note: These bulk operations would need to be converted to their async equivalents
//                    // If the bulk operation library supports async operations
//                    await Task.Run(() =>
//                    {
//                        db.BulkInsert(parms);
//                        db.BulkUpdate(parms);
//                        db.BulkDelete(parms);
//                        db.BulkMerge(parms);
//                    });

//                    result = 1;
//                    tran.Commit();
//                }
//                catch (Exception ex)
//                {
//                    tran.Rollback();
//                    var SerializeReponse = JsonConvert.SerializeObject(parms);
//                    await _iHandleException.LogEvents(ex);
//                    throw;
//                }
//            }

//            return result;
//        }

//        public async Task<T> QueryFirstOrDefaultAsync<T>(string procedureName, DynamicParameters parameters,
//            CommandType commandType = CommandType.StoredProcedure, string? connectionString = null)
//        {
//            await using var db = GetDbConnection(connectionString);
//            if (db.State == ConnectionState.Closed)
//                await db.OpenAsync();

//            using var tran = db.BeginTransaction();
//            try
//            {
//                T result = await db.QueryFirstOrDefaultAsync(procedureName, parameters, commandType: commandType, transaction: tran).ConfigureAwait(false) ?? throw new InvalidOperationException();
//                tran.Commit();
//                return result;
//            }
//            catch (Exception ex)
//            {
//                tran.Rollback();
//                //var SerializeResponse = JsonConvert.SerializeObject(ExceptionExtensions.GetFullMessage(ex));
//                //var bc = ExceptionExtensions.SendErrorToMail<T>(ex, path.ToString());
//                throw new Exception(ex.InnerException?.Message ?? ex.Message);
//            }
//        }
//        public void CommitTransaction(IDbTransaction transaction)
//        {
//            transaction.Commit();
//        }

//        public async Task<SqlMapper.GridReader> GetMultipleResultAsync(string procedureName, DynamicParameters parameters,
//            CommandType commandType = CommandType.StoredProcedure, string? connectionString = null)
//        {
//            await using var db = GetDbConnection(connectionString);

//            if (db.State == ConnectionState.Closed)
//                await db.OpenAsync();
//            return await db.QueryMultipleAsync(procedureName, parameters, commandType: commandType).ConfigureAwait(false);
//        }

//        public async Task ExecuteMultipleAsync(string procedureName, List<DynamicParameters> parameters, CommandType commandType = CommandType.StoredProcedure,
//            string? connectionString = null)
//        {
//            await using var db = GetDbConnection(connectionString);

//            if (db.State == ConnectionState.Closed)
//                await db.OpenAsync();

//            using var tran = db.BeginTransaction();
//            try
//            {
//                foreach (var parameter in parameters)
//                {
//                    await db.ExecuteAsync(procedureName, parameter, commandType: commandType, transaction: tran).ConfigureAwait(false);
//                }

//                tran.Commit();
//            }
//            catch (Exception ex)
//            {
//                tran.Rollback();
//                throw new Exception(ex.InnerException?.Message ?? ex.Message);
//            }
//        }

//        public async Task SqlBulkCopy(DataTable dataTable, string tableName, string? connectionString = null)
//        {
//            if (connectionString != null)
//            {
//                await using var conn = GetDbConnection(connectionString);
//                await conn.OpenAsync();

//                using var bulkCopy = new SqlBulkCopy(conn.ConnectionString);
//                bulkCopy.BulkCopyTimeout = _appSettings.SqlTimeoutSeconds;
//                bulkCopy.BatchSize = _appSettings.BatchSize;
//                bulkCopy.DestinationTableName = tableName;
//                bulkCopy.EnableStreaming = true;

//                await bulkCopy.WriteToServerAsync(dataTable);
//            }
//        }
//    }
//}
