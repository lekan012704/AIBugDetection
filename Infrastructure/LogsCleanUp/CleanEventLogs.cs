using Application.Abstractions.LogsCleanUp;
using Application.Dapper;
using Dapper;
using Infrastructure.Dapper;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Globalization;

namespace Infrastructure.LogsCleanUp
{
    public class CleanEventLogs : ICleanEventLogs
    {
        private readonly ILogger<CleanEventLogs> _logger;
        private readonly IDapperFactory _dapperFactory;
        public CleanEventLogs(ILogger<CleanEventLogs> logger, IDapperFactory dapperFactory)
        {
            _logger = logger;
            _dapperFactory = dapperFactory;
        }

        public async Task<bool> DeletePreviousLogsAsync()
        {
            try
            {
                DateTime today = DateTime.Now;
                DateTime resultedDate = today.AddDays(-1);
                var answer = resultedDate.ToString("yyyy-MM-dd HH:mm:ss", DateTimeFormatInfo.InvariantInfo);
                var query = "DELETE FROM Logs.SelfServiceApi WHERE TimeStamp <= @ResultedDate";

                var parameters = new DynamicParameters();
                parameters.Add("@ResultedDate", answer);

                var result = await _dapperFactory.ExecuteAsync(query, parameters, commandType: CommandType.Text, connectionString: GetConnectionStringExtention.GetDefaultConnectionString());
            }
            catch (Exception ex)
            {
                _logger.LogCritical("Request in DeletePreviousLogsAsync: {@Request}", ex.InnerException?.Message ?? ex.Message);
                return false;
            }

            return true;
        }
    }
}
