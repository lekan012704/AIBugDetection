using Domain.Enums;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using System.Runtime.CompilerServices;
using ILogger = Serilog.ILogger;

namespace Application.Handlers
{
    public static class GuardClauseChecker
    {
        private static readonly ILogger _serilogLoggers;

        static GuardClauseChecker()
        {
            _serilogLoggers = Log.Logger;
        }

        /// <summary>
        /// Ensures that a reference type value is not null.
        /// </summary>
        /// <typeparam name="T">The type of the value (must be reference type)</typeparam>
        /// <param name="value">The value to check</param>
        /// <param name="paramName">Name of the parameter being checked</param>
        /// <param name="callerMemberName">Auto-populated name of the calling method</param>
        /// <param name="callerLineNumber">Auto-populated line number where the method is called</param>
        /// <returns>The original value if not null</returns>
        /// <exception cref="ArgumentNullException">Thrown when the input value is null</exception>
        /// <example>
        /// <code>
        /// public void ProcessUser(User user)
        /// {
        ///     var validatedUser = user.NotNull(nameof(user));
        ///     // ... use validatedUser
        /// }
        /// </code>
        /// </example>
        public static T NotNull<T>(
            this T value,
            string paramName,
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0)
            where T : class
        {
            if (value == null)
            {
                var exception = new ArgumentNullException(paramName,
                    $"Parameter '{paramName}' cannot be null. Called from {callerMemberName} line {callerLineNumber}");
                LogToSentry(exception);
                throw exception;
            }

            return value;
        }

        /// <summary>
        /// Converts an object to its string representation.
        /// </summary>
        /// <param name="value">The object to convert</param>
        /// <param name="callerMemberName">Auto-populated name of the calling method</param>
        /// <param name="callerLineNumber">Auto-populated line number where the method is called</param>
        /// <returns>String representation of the object</returns>
        /// <exception cref="ArgumentNullException">Thrown when the input value is null</exception>
        /// <example>
        /// <code>
        /// public void ProcessData(object data)
        /// {
        ///     string dataString = data.ObjectToString();
        ///     // ... use dataString
        /// }
        /// </code>
        /// </example>
        public static string ObjectToString(this object value,
                                    [CallerMemberName] string callerMemberName = "",
                                    [CallerLineNumber] int callerLineNumber = 0)
        {
            if (value == null)
            {
                var exception = new ArgumentNullException(nameof(value),
                    $"Null value encountered in method '{callerMemberName}' at line {callerLineNumber}");
                LogToSentry(exception);
                throw exception;
            }

            return Convert.ToString(value) ?? string.Empty;
        }

        /// <summary>
        /// Converts an object to an integer.
        /// </summary>
        /// <param name="value">The object to convert</param>
        /// <param name="callerMemberName">Auto-populated name of the calling method</param>
        /// <param name="callerLineNumber">Auto-populated line number where the method is called</param>
        /// <returns>The converted integer value</returns>
        /// <exception cref="ArgumentNullException">Thrown when the input value is null</exception>
        /// <exception cref="FormatException">Thrown when the value cannot be parsed as an integer</exception>
        /// <example>
        /// <code>
        /// public void ProcessQuantity(object quantity)
        /// {
        ///     int qty = quantity.ObjectToInt();
        ///     // ... use qty
        /// }
        /// </code>
        /// </example>
        public static int ObjectToInt(this object value,
                                    [CallerMemberName] string callerMemberName = "",
                                    [CallerLineNumber] int callerLineNumber = 0)
        {
            if (value == null)
            {
                var exception = new ArgumentNullException(nameof(value),
                    $"Null value cannot be converted to int in method '{callerMemberName}' at line {callerLineNumber}");
                LogToSentry(exception);
                throw exception;
            }

            if (int.TryParse(value.ToString(), out int result))
            {
                return result;
            }

            var parseException = new FormatException(
                $"Value '{value}' cannot be converted to int in method '{callerMemberName}' at line {callerLineNumber}");
            LogToSentry(parseException);
            throw parseException;
        }

        /// <summary>
        /// Converts an object to a decimal.
        /// </summary>
        /// <param name="value">The object to convert</param>
        /// <param name="callerMemberName">Auto-populated name of the calling method</param>
        /// <param name="callerLineNumber">Auto-populated line number where the method is called</param>
        /// <returns>The converted decimal value</returns>
        /// <exception cref="ArgumentNullException">Thrown when the input value is null</exception>
        /// <exception cref="FormatException">Thrown when the value cannot be parsed as a decimal</exception>
        /// <example>
        /// <code>
        /// public void ProcessPrice(object price)
        /// {
        ///     decimal priceValue = price.ObjectToDecimal();
        ///     // ... use priceValue
        /// }
        /// </code>
        /// </example>
        public static decimal ObjectToDecimal(this object value,
                                           [CallerMemberName] string callerMemberName = "",
                                           [CallerLineNumber] int callerLineNumber = 0)
        {
            if (value == null)
            {
                var exception = new ArgumentNullException(nameof(value),
                    $"Null value cannot be converted to decimal in method '{callerMemberName}' at line {callerLineNumber}");
                LogToSentry(exception);
                throw exception;
            }

            if (decimal.TryParse(value.ToString(), out decimal result))
            {
                return result;
            }

            var parseException = new FormatException(
                $"Value '{value}' cannot be converted to decimal in method '{callerMemberName}' at line {callerLineNumber}");
            LogToSentry(parseException);
            throw parseException;
        }

        /// <summary>
        /// Converts an object to a boolean.
        /// </summary>
        /// <param name="value">The object to convert</param>
        /// <param name="callerMemberName">Auto-populated name of the calling method</param>
        /// <param name="callerLineNumber">Auto-populated line number where the method is called</param>
        /// <returns>The converted boolean value</returns>
        /// <exception cref="ArgumentNullException">Thrown when the input value is null</exception>
        /// <exception cref="FormatException">Thrown when the value cannot be parsed as a boolean</exception>
        /// <remarks>
        /// Valid boolean strings include "True", "False", "true", "false", or any other value that Boolean.TryParse can handle.
        /// </remarks>
        /// <example>
        /// <code>
        /// public void ProcessStatus(object status)
        /// {
        ///     bool isActive = status.ObjectToBool(); 
        ///     // ... use isActive
        /// }
        /// </code>
        /// </example>
        public static bool ObjectToBool(this object value,
                                      [CallerMemberName] string callerMemberName = "",
                                      [CallerLineNumber] int callerLineNumber = 0)
        {
            if (value == null)
            {
                var exception = new ArgumentNullException(nameof(value),
                    $"Null value cannot be converted to bool in method '{callerMemberName}' at line {callerLineNumber}");
                LogToSentry(exception);
                throw exception;
            }

            if (bool.TryParse(value.ToString(), out bool result))
            {
                return result;
            }

            var parseException = new FormatException(
                $"Value '{value}' cannot be converted to bool in method '{callerMemberName}' at line {callerLineNumber}");
            LogToSentry(parseException);
            throw parseException;
        }

        /// <summary>
        /// Validates that a string is not null or empty.
        /// </summary>
        /// <param name="value">The string to validate</param>
        /// <param name="callerMemberName">Auto-populated name of the calling method</param>
        /// <param name="callerLineNumber">Auto-populated line number where the method is called</param>
        /// <returns>The original string if valid</returns>
        /// <exception cref="ArgumentException">Thrown when the string is null or empty</exception>
        /// <example>
        /// <code>
        /// public void ProcessName(string name)
        /// {
        ///     string validName = name.StringNotNullOrEmpty();
        ///     // ... use validName
        /// }
        /// </code>
        /// </example>
        public static string StringNotNullOrEmpty(this string value,
                                               [CallerMemberName] string callerMemberName = "",
                                               [CallerLineNumber] int callerLineNumber = 0)
        {
            if (string.IsNullOrEmpty(value))
            {
                var exception = new ArgumentException(
                    $"String cannot be null or empty in method '{callerMemberName}' at line {callerLineNumber}", nameof(value));
                LogToSentry(exception);
                throw exception;
            }

            return value;
        }

        /// <summary>
        /// Logs details about processing with specific object context
        /// </summary>
        /// <param name="exception">The exception that occurred</param>
        /// <param name="objectValues">The values in object</param>
        /// <param name="identifierId">The customer, user or any identifier Id</param>
        /// <param name="guid">Generated any Guid</param>
        /// <param name="additionalDetails">Any additional context details</param>
        /// <param name="callerMemberName">Method where the exception occurred</param>
        /// <param name="callerLineNumber">Line number where the exception occurred</param>
        /// <example>
        /// <code>
        /// LogProcessingError( ex,
        ///     objectValue,
        ///     identifierId,
        ///     guid,
        ///     new Dictionary<string, object>
        ///     {
        ///     ["FileName"] = FileName,
        ///     ["EDIFolderPath"] = EDIEngine?.EDIFolderDetails?.ProcessingOut
        ///     });
        /// </code>
        /// </example>
        public static void LogProcessingError(
            Exception exception,
            object objectValues,
            LoggerTypes loggerType = LoggerTypes.Serilog,
            LogLevel logLevel = LogLevel.Information,
            object? identifierId = null,
            string? guid = null,
            Dictionary<string, object>? additionalDetails = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            var contextDetails = new Dictionary<string, object>();
            guid = guid ?? Guid.NewGuid().ToString();
            if (objectValues != null)
            {
                // Get all properties of the Object values using reflection
                var properties = objectValues.GetType().GetProperties();
                foreach (var prop in properties)
                {
                    try
                    {
                        var value = prop.GetValue(objectValues);
                        contextDetails[$"ObjectValue.{prop.Name}"] = value?.ToString() ?? "null";
                    }
                    catch
                    {
                        // Ignore errors in property access
                        contextDetails[$"ObjectValue.{prop.Name}"] = "Error accessing property";
                    }
                }

                // Add serialized JSON of the entire object
                contextDetails["ObjectValue.Json"] = JsonConvert.SerializeObject(objectValues);
            }
            else
            {
                contextDetails["ObjectValue"] = "null";
            }

            contextDetails["IdentifierId"] = identifierId?.ToString() ?? "null";
            contextDetails["EventOccurredAt"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            // Add any additional details
            if (additionalDetails != null)
            {
                foreach (var detail in additionalDetails)
                {
                    contextDetails[detail.Key] = detail.Value;
                }
            }

            var additionalInfo = $"An error occurred in method '{callerMemberName}' at line {callerLineNumber} additional details => {contextDetails}";

            if (loggerType == LoggerTypes.Serilog)
            {
                LogToSerilog(exception, logLevel, additionalInfo, callerMemberName, callerLineNumber);
            }
            else if (loggerType == LoggerTypes.Sentry)
            {
                LogToSentry(exception, additionalInfo, identifierId?.ToString(), guid);
            }
            else
            {
                throw new ArgumentException("Invalid logger type specified.");
            }
        }

        /// <summary>
        /// Logs an exception to Sentry with error handling.
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <remarks>
        /// This method includes error handling to prevent Sentry logging failures from crashing the application.
        /// Any errors during Sentry logging are written to the trace log.
        /// </remarks>
        public static void LogToSentry(Exception exception, string? AdditionalInfo = null, string? userId = null, string? guid = null)
        {
            try
            {
                //option 1
                //SentrySdk.CaptureException(exception);

                //option 2
                if (!string.IsNullOrWhiteSpace(AdditionalInfo))
                {
                    exception.Data.Add("AdditionalInfo", AdditionalInfo);
                }

                SentryEvent sentryEvent = new(exception);
                Scope scope = new(new SentryOptions
                {
                    //Dsn = Environment.GetEnvironmentVariable("SENTRY_DSN") ?? throw new InvalidOperationException("SENTRY_DSN is not set in environment variables."),
                    //Debug = true,
                    //TracesSampleRate = 1.0,
                    Environment = Environment.GetEnvironmentVariable("APPLICATION_ENVIRONMENT") ?? "production",
                });

                if (!string.IsNullOrWhiteSpace(userId))
                {
                    scope.User = new()
                    {
                        Id = userId
                    };
                }

                if (!string.IsNullOrWhiteSpace(guid))
                {
                    scope.SetTag("icmaGuid", guid);
                }

                if (string.IsNullOrWhiteSpace(guid))
                {
                    scope.SetTag("icmaGuid", Guid.NewGuid().ToString());
                }

                SentrySdk.CaptureEvent(sentryEvent, scope);
                //Ends

                // Option 3
                SentrySdk.CaptureException(exception, scope =>
                {
                    if (!string.IsNullOrWhiteSpace(userId))
                    {
                        scope.User = new()
                        {
                            Id = userId
                        };
                    }

                    if (!string.IsNullOrWhiteSpace(guid))
                    {
                        scope.SetTag("icmaGuid", guid);
                    }

                    if (string.IsNullOrWhiteSpace(guid))
                    {
                        scope.SetTag("icmaGuid", Guid.NewGuid().ToString());
                    }
                });
            }
            catch (Exception sentryException)
            {
                System.Diagnostics.Trace.WriteLine($"Failed to log to Sentry: {sentryException.Message}");
            }
        }

        /// <summary>
        /// Centralized error logging to MSSQL via Serilog
        /// </summary>
        public static void LogToSerilog(Exception exception, LogLevel logLevel, string? AdditionalInfo = null, string callerMemberName = "", int callerLineNumber = 0)
        {
            if (!string.IsNullOrWhiteSpace(AdditionalInfo))
            {
                exception.Data.Add("AdditionalInfo", AdditionalInfo);
            }

            try
            {
                if (callerLineNumber > 0 || !string.IsNullOrWhiteSpace(callerMemberName))
                {
                    switch (logLevel)
                    {
                        case LogLevel.Debug:
                            _serilogLoggers.Debug(exception,
                                "Error in {CallerMethod} at line {CallerLineNumber}: {ErrorMessage}",
                                callerMemberName,
                                callerLineNumber,
                                exception.Message);
                            break;
                        case LogLevel.Information:
                            _serilogLoggers.Information(exception,
                                "Error in {CallerMethod} at line {CallerLineNumber}: {ErrorMessage}",
                                callerMemberName,
                                callerLineNumber,
                                exception.Message);
                            break;
                        case LogLevel.Warning:
                            _serilogLoggers.Warning(exception,
                                "Error in {CallerMethod} at line {CallerLineNumber}: {ErrorMessage}",
                                callerMemberName,
                                callerLineNumber,
                                exception.Message);
                            break;
                        case LogLevel.Error:
                            _serilogLoggers.Error(exception,
                                "Error in {CallerMethod} at line {CallerLineNumber}: {ErrorMessage}",
                                callerMemberName,
                                callerLineNumber,
                                exception.Message);
                            break;
                        case LogLevel.Critical:
                            _serilogLoggers.Fatal(exception,
                                "Error in {CallerMethod} at line {CallerLineNumber}: {ErrorMessage}",
                                callerMemberName,
                                callerLineNumber,
                                exception.Message);
                            break;
                        default:
                            _serilogLoggers.Error(exception,
                                "Error in {CallerMethod} at line {CallerLineNumber}: {ErrorMessage}",
                                callerMemberName,
                                callerLineNumber,
                                exception.Message);
                            break;
                    }
                }
                else
                {
                    switch (logLevel)
                    {
                        case LogLevel.Debug:
                            _serilogLoggers.Debug(exception, "Error: {ErrorMessage}", exception.Message);
                            break;
                        case LogLevel.Information:
                            _serilogLoggers.Information(exception, "Error: {ErrorMessage}", exception.Message);
                            break;
                        case LogLevel.Warning:
                            _serilogLoggers.Warning(exception, "Error: {ErrorMessage}", exception.Message);
                            break;
                        case LogLevel.Error:
                            _serilogLoggers.Error(exception, "Error: {ErrorMessage}", exception.Message);
                            break;
                        case LogLevel.Critical:
                            _serilogLoggers.Fatal(exception, "Error: {ErrorMessage}", exception.Message);
                            break;
                        default:
                            _serilogLoggers.Error(exception, "Error: {ErrorMessage}", exception.Message);
                            break;
                    }
                }
            }
            catch (Exception sqlLogEx)
            {
                System.Diagnostics.Trace.WriteLine($"Failed to log to MSSQL: {sqlLogEx.Message}");
            }
        }
    }
}

public static class GuardChecker
{
    public static string? ObjectToString(this object? value)
    {
        return value switch
        {
            null => null,
            IFormattable formattable => formattable.ToString(null, System.Globalization.CultureInfo.CurrentCulture),
            _ => value.ToString()
        };
    }
}
