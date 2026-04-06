using Application.Abstractions.Authentication.KeyCloak;
using Application.Abstractions.EntityRepositories.Users;
using Domain.Application.Entities.Users;
using Domain.ExternalEntities.Dtos;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Helpers;

namespace Infrastructure.EntityRepositories.Users
{
    public class UserRepository(
        ApplicationDbContext context,
        IKeyCloakService keyCloakService,
        ILogger<UserRepository> logger) : IUserRespository
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IKeyCloakService _keyCloakService = keyCloakService;
        private readonly ILogger<UserRepository> _logger = logger;

        // ✅ Simple add to YOUR DB — Keycloak already created the identity
        public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));

            await _context.Set<User>().AddAsync(user, cancellationToken);
            _logger.LogInformation(
                "User {Email} added to app DB with KeycloakId {KeycloakId}",
                user.Email, user.KeycloakId);
        }

        // ✅ Update user in YOUR DB
        public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));

            _context.Set<User>().Update(user);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "User {Email} updated in app DB.", user.Email);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

       
        public async Task<User?> GetByKeycloakIdAsync(
            string keycloakId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(keycloakId))
                return null;

            return await _context.Set<User>()
                .FirstOrDefaultAsync(u =>
                    u.KeycloakId == keycloakId &&
                    !u.IsDeleted,
                    cancellationToken);
            }

        public async Task<User?> GetByIdAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<User>()
                .FirstOrDefaultAsync(u =>
                    u.Id == userId &&
                    !u.IsDeleted,
                    cancellationToken);
        }

        public async Task<User?> GetByUserEmailAsync(
            string userEmail,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<User>()
                .SingleOrDefaultAsync(u =>
                    u.Email == userEmail &&
                    !u.IsDeleted,
                    cancellationToken);
        }

        public async Task<bool> GetAnyByUserEmailAsync(
            string userEmail,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<User>()
                .AnyAsync(u =>
                    u.Email == userEmail,
                    cancellationToken);
        }

        public async Task<bool> GetAnyByUserNameAsync(
            string userName,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<User>()
                .AnyAsync(u =>
                    u.UserName == userName,
                    cancellationToken);
        }

        public IQueryable<User> GetAll()
        {
            return _context.Set<User>()
                .Where(u => !u.IsDeleted);
        }

        public IQueryable<User> Get(Func<User, bool> predicate)
        {
            return _context.Set<User>()
                .Where(predicate)
                .AsQueryable();
        }

        public async Task<User?> FindUserAsync(
            string userIdentifier,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userIdentifier))
                return null;

            var trimmed = userIdentifier.Trim();

            if (!StringFormatter.IsValidEmailAddress(trimmed))
                return null;

            return await _context.Set<User>()
                .FirstOrDefaultAsync(u =>
                    (u.Id == trimmed || u.Email == trimmed) &&
                    !u.IsDeleted,
                    cancellationToken);
        }

        public async Task<List<User>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1)
                throw new ArgumentException(
                    "Page number must be greater than 0", nameof(pageNumber));

            if (pageSize < 1)
                throw new ArgumentException(
                    "Page size must be greater than 0", nameof(pageSize));

            return await _context.Set<User>()
                .Where(u => !u.IsDeleted)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        // ✅ Keycloak operations — delegate everything to KeyCloakService
        public async Task<string> VerifyPasswordWithKeycloakAsync(
            string username,
            string password,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
                return string.Empty;

            var token = await _keyCloakService
                .GetAccessTokenAsync(username, password, cancellationToken);

            return token.IsError ? string.Empty : token.Value.Trim();
        }

        public async Task<string> CreateUserWithKeycloakAsync(
            KeycloakUser keycloakUser,
            CancellationToken cancellationToken)
        {
            if (keycloakUser is null || string.IsNullOrWhiteSpace(keycloakUser.Email))
                return string.Empty;

            var token = await _keyCloakService
                .CreateUserAsync(keycloakUser, cancellationToken);

            return token.IsError ? string.Empty : token.Value.Trim();
        }
    }
}