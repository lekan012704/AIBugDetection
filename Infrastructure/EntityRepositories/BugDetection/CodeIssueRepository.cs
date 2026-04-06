using Application.Abstractions.Data;
using Application.Abstractions.EntityRepositories.BugDetection;
using Domain.Application.Entities.BugDetection;
using Infrastructure.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.EntityRepositories.BugDetection
{
    public sealed class CodeIssueRepository:RepositoryAsync<CodeIssue, Guid>, ICodeIssueRepository
    {
        public CodeIssueRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
}
