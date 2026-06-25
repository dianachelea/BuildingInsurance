namespace BuildingInsurance.Application.Features.Common.Dtos
{
    public sealed class CountryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
    }
}