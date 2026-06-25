namespace BuildingInsurance.Application.Features.Common.Dtos
{
    public sealed class CountyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public Guid CountryId { get; set; }
    }
}