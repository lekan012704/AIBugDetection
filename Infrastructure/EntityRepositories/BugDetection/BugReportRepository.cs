using Application.Abstractions.Data;
using Application.Abstractions.EntityRepositories.BugDetection;
using Domain.Application.Entities.BugDetection;
using ErrorOr;
using Infrastructure.GenericRepository;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Polly;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.EntityRepositories.BugDetection
{
    public sealed class BugReportRepository : RepositoryAsync<BugReport, Guid>, IBugReportRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BugReportRepository> _logger;
        public BugReportRepository(IUnitOfWork unitOfWork, ILogger<BugReportRepository> logger) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ErrorOr<BugReport>> GetByIdWithItemsAsync(
            Guid reportId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (reportId == Guid.Empty)
                    return Errors.Common.Validation(
                        "ReportId",
                        "ReportId cannot be empty");

                var report = await EntitySet
                    .Include(r => r.BugItems)
                    .FirstOrDefaultAsync(
                        r => r.Id == reportId,
                        cancellationToken);

                if (report is null)
                    return Errors.Common.NotFound(
                        "BugReport",
                        reportId.ToString());

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to get bug report with items {ReportId}",
                    reportId);
                return Errors.Infrastructure.DatabaseError(
                    "GetBugReportWithItems", ex.Message);
            }
        }

        public async Task<ErrorOr<List<BugReport>>> GetByUserIdAsync(
            string userId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return Errors.Common.Validation(
                        "UserId",
                        "UserId cannot be empty");

                if (pageNumber < 1)
                    return Errors.Common.Validation(
                        "PageNumber",
                        "Page number must be greater than 0");

                if (pageSize < 1)
                    return Errors.Common.Validation(
                        "PageSize",
                        "Page size must be greater than 0");

                var reports = await EntitySet
                    .Where(r => r.UserId == userId)
                    .OrderByDescending(r => r.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                return reports;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to get bug reports for user {UserId}",
                    userId);
                return Errors.Infrastructure.DatabaseError(
                    "GetBugReportsByUser", ex.Message);
            }
        }

        public async Task<ErrorOr<int>> GetTotalCountByUserIdAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return Errors.Common.Validation(
                        "UserId",
                        "UserId cannot be empty");

                var count = await EntitySet
                    .CountAsync(
                        r => r.UserId == userId,
                        cancellationToken);

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to get bug report count for user {UserId}",
                    userId);
                return Errors.Infrastructure.DatabaseError(
                    "GetBugReportCount", ex.Message);
            }
        }

        
    }
}
