namespace Domain.Application.Entities.UserPermissions
{
    public record UserPermissionId(string Value)
    {
        public static UserPermissionId New() => new(Guid.NewGuid().ToString());
    }
}
