namespace BuildingInsurance.Domain.Common
{
    public static class ReasonNormalizer
    {
        public static bool TryNormalize(string? reason, IEnumerable<string> allowed, out string normalized)
        {
            normalized = string.Empty;

            if (string.IsNullOrWhiteSpace(reason))
                return false;

            var trimmed = reason.Trim();

            var match = allowed.FirstOrDefault(r => string.Equals(r, trimmed, StringComparison.OrdinalIgnoreCase));

            if (match is null)
                return false;

            normalized = match;
            return true;
        }
    }
}