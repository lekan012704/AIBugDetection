//using Application.Abstractions.LogsCleanUp;
//using Hangfire;

//namespace Application.Scheduler
//{
//    public static class HangFireService
//    {
//        public static void InitialiseService()
//        {
//            const string deletePreviousLogsJobId = "983dcab9d626-123abfe6c2c6b781216K";
//            RecurringJob.RemoveIfExists(deletePreviousLogsJobId);

//            // Cron expression for 3 AM daily
//            RecurringJob.AddOrUpdate<ICleanEventLogs>(deletePreviousLogsJobId, x => x.DeletePreviousLogsAsync(), Cron.Daily(3));

//            //This "0 2 * * *" expression means: Minute: 0(on the hour), Hour: 2(2 AM), Day of Month: *(every day),Month: *(every month), Day of Week: *(every day of the week)

//        }
//    }
//}
