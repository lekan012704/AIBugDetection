using Domain.Application.Entities.Users;
using Domain.ExternalEntities.Dtos;

namespace Application.Abstractions.EntityRepositories.Users;

public interface IUserRespository
{
    // ─── Core CRUD ───────────────────────────────────────────────
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    // ─── Queries ─────────────────────────────────────────────────
    IQueryable<User> GetAll();
    IQueryable<User> Get(Func<User, bool> predicate);

    Task<User?> GetByKeycloakIdAsync(string keycloakId, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<User?> GetByUserEmailAsync(string userEmail, CancellationToken cancellationToken = default);
    Task<User?> FindUserAsync(string userIdentifier, CancellationToken cancellationToken = default);

    Task<bool> GetAnyByUserEmailAsync(string userEmail, CancellationToken cancellationToken = default);
    Task<bool> GetAnyByUserNameAsync(string userName, CancellationToken cancellationToken = default);

    Task<List<User>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    // ─── Keycloak Operations ──────────────────────────────────────
    Task<string> VerifyPasswordWithKeycloakAsync(string username, string password, CancellationToken cancellationToken);
    Task<string> CreateUserWithKeycloakAsync(KeycloakUser keycloakUser, CancellationToken cancellationToken);
}