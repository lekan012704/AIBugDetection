namespace Domain.Application.Dtos
{
    public class MenuSetup
    {
        public string? PermissionId { get; set; }
        public string? PermissionUrl { get; set; }
        public string? PermissionName { get; set; }
        public string? SectionName { get; set; }
        public string? ParentPermissionCode { get; set; }
        public string? PermissionCode { get; set; }
        public string? MenuFileName { get; set; }
        public string? ImgClass { get; set; }
        public string? SectionImgClass { get; set; }
        public int? PermissionOrder { get; set; }
        //public bool? IsSubMenu { get; set; }
        public bool IsFinancialInstitution { get; set; }
    }
}
