using Dapper;
using Domain.Enums;
using System.Data;


namespace Infrastructure.Helpers
{
    internal sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
    {
        public override DateOnly Parse(object value) => DateOnly.FromDateTime((DateTime)value);

        public override void SetValue(IDbDataParameter parameter, DateOnly value)
        {
            parameter.DbType = DbType.Date;
            parameter.Value = value;
        }
    }
    public static class RoleHelper
    {
        public static string GetRoleName(SystemRoles role)
        {
            return role.ToString();
        }
    }
}
