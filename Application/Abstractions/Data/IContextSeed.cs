using Domain.ExternalEntities.Dtos;
using Microsoft.AspNetCore.Identity;
using SharedKernel;

namespace Application.Abstractions.Data
{
    public interface IContextSeed
    {
      
        string GetDefaultPassword();
    }
}
