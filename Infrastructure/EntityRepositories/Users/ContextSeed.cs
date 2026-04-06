using Application.Abstractions.Data;
using Application.Helper;
using Application.Models;
using Domain.Application.Entities.Users;
using Domain.Enums;
using Infrastructure.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel;

namespace Infrastructure.EntityRepositories.Users
{
    public class ContextSeed : IContextSeed
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppSettings _appsettings;
        private readonly IDateTimeProvider _iDateTimeProvider;
        private readonly ILogger<ContextSeed> _logger;

        public ContextSeed(
            ApplicationDbContext context,
            UserManager<User> userManager,
            IDateTimeProvider iDateTimeProvider,
            RoleManager<IdentityRole> roleManager,
            IOptions<AppSettings> appsettings,
            ILogger<ContextSeed> logger)
        {
            _context = context;
            _userManager = userManager;
            _iDateTimeProvider = iDateTimeProvider;
            _roleManager = roleManager;
            _appsettings = appsettings.Value;
            _logger = logger;
        }

        //public async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        //{
        //    foreach (var role in Enum.GetValues(typeof(SystemRoles)))
        //    {
        //        var roleName = role!.ToString();

        //        if (!await roleManager.RoleExistsAsync(roleName))
        //        {
        //            await roleManager.CreateAsync(new IdentityRole(roleName));
        //        }
        //    }
        //}

       

        public string GetDefaultPassword()
        {
            return "123Pa$$word!";
        }
    }
}