namespace Domain.External.Dtos.States
{
    public class StateDto
    {
        public int StateId { get; set; }
        public string StateCode { get; set; } = string.Empty;
        public string StateName { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
    }
}
