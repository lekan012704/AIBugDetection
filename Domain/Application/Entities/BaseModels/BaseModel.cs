using System.ComponentModel.DataAnnotations;

namespace Domain.Application.Entities.BaseModels

{
    public class BaseModel
    {
        [Key]
        public long Id { get; set; }
        public string? ReturnFileName { get; set; }
        public string? ReturnFilePath { get; set; }

        public DateTime CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsProcessed { get; set; }
        public bool IsValid { get; set; }
        public string? ValidationMessage { get; set; }
        public int? ReturnType { get; set; }
    }
}
