namespace BuildingInsurance.Application.Features.Common.Dtos
{
    public sealed class CityDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public Guid CountyId { get; set; }
    }
}