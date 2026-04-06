using Dapper;
using System.Data;
using System.Data.Common;


namespace Application.Dapper
{
    public interface IDapperFactory : IDisposable
    {
        //https://code-maze.com/using-dapper-with-asp-net-core-web-api/

        //Get Connection String
        DbConnection GetDbConnection(string? connectionString = null);

        //Create Connection String
        IDbConnection CreateConnection();

        void CommitTransaction(IDbTransaction transaction);

        //Get a record.
        Task<T?> GetAsync<T>(string procedureName, DynamicParameters? parameters, CommandType commandType = CommandType.StoredProcedure, string? connectionString = null);

        //Get list of records.
        Task<List<T>> GetAllAsync<T>(string procedureName, DynamicParameters parameters, CommandType commandType = CommandType.StoredProcedure, string? connectionString = null);

        //Execute query and return DataSet or DataTable.
        Task<SqlMapper.GridReader> GetMultipleResultAsync(string procedureName, DynamicParameters parameters, CommandType commandType = CommandType.StoredProcedure, string? connectionString = null);

        //Execute query and return scope Identity.
        Task<int> QuerySingleAsync(string procedureName, DynamicParameters parameters, CommandType commandType = CommandType.StoredProcedure, string? connectionString = null);

        //Execute query and return single response object.
        Task<T> QuerySingleAsync<T>(string procedureName, DynamicParameters parameters, CommandType commandType = CommandType.StoredProcedure, string? connectionString = null);

        //Execute query and return single response object or null record if not available
        Task<T> QueryFirstOrDefaultAsync<T>(string procedureName, DynamicParameters parameters, CommandType commandType = CommandType.StoredProcedure, string? connectionString = null);

        //Execute query and return number of affected rows count.
        Task<int> ExecuteAsync(string procedureName, DynamicParameters parameters, CommandType commandType = CommandType.StoredProcedure, string? connectionString = null);

        //Execute multiple insert query and return number of affected rows count.
        Task ExecuteMultipleAsync(string procedureName, List<DynamicParameters> parameters, CommandType commandType = CommandType.StoredProcedure, string? connectionString = null);

        Task SqlBulkCopy(DataTable dataTable, string tableName, string? connectionString = null);
        Task LoadDataAsync<T>(string storedProcedure, T parameters, string connectionString);
        Task<int> ExecuteAsync<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, string? connectionString = null);
        Task<IQueryable<T>> GetAllAsQueryableAsync<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, string connectionString = null);
        Task<List<T>> GetAllsAsync<T>(string sp, DynamicParameters parms, string connectionString, CommandType commandType = CommandType.StoredProcedure);
        Task<T> InsertAsync<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, string connectionString = null);
        Task<T> InsertsAsync<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.Text, string connectionString = null);
        Task<T> UpdateAsync<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, string connectionString = null);
        Task<int> BulkInsertAsync<T>(DynamicParameters parms, string connectionString = null);
    }
}
