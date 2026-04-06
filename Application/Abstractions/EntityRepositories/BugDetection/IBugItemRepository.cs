using Application.Abstractions.GenericRepository;
using Domain.Application.Entities.BugDetection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Abstractions.EntityRepositories.BugDetection
{
    public interface IBugItemRepository :IRepositoryAsync<BugItem, Guid>
    {

    }
}
