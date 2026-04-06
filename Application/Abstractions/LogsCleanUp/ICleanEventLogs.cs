namespace Application.Abstractions.LogsCleanUp
{
    public interface ICleanEventLogs
    {
        Task<bool> DeletePreviousLogsAsync();
    }
}
