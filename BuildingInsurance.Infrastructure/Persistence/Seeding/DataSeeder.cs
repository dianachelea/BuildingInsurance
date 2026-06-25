using BuildingInsurance.Domain.Entities.Geography;
using Microsoft.EntityFrameworkCore;

namespace BuildingInsurance.Infrastructure.Persistence.Seeding
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(BuildingInsuranceDbContext db, CancellationToken ct = default)
        {
            await db.Database.MigrateAsync(ct);

            if (await db.Countries.AnyAsync(ct))
                return;

            var roId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var bgId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var huId = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var atId = Guid.Parse("44444444-4444-4444-4444-444444444444");
            var deId = Guid.Parse("55555555-5555-5555-5555-555555555555");

            var countries = new List<Country>
            {
                new Country(roId, "Romania"),
                new Country(bgId, "Bulgaria"),
                new Country(huId, "Hungary"),
                new Country(atId, "Austria"),
                new Country(deId, "Germany"),
            };

            var ilfovId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1");
            var clujId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2");
            var brasovId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3");
            var timisId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4");

            var sofiaId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1");
            var plovId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2");
            var varnaId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb3");

            var pestId = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc1");
            var gyorId = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc2");
            var szegId = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc3");

            var iasiId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5");
            var constId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa6");
            var sibiuId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa7");

            var burgasId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb4");
            var ruseId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb5");
            var plevenId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb6");

            var bacsId = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc4");
            var hevesId = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc5");
            var baranyaId = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc6");

            var viennaStateId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddd01");
            var styriaId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddd02");

            var bavariaId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee01");
            var nrwId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee02");

            var counties = new List<County>
            {
                new County(ilfovId, "Ilfov", roId),
                new County(clujId, "Cluj", roId),
                new County(brasovId, "Brasov", roId),
                new County(timisId, "Timis", roId),
                new County(iasiId, "Iasi", roId),
                new County(constId, "Constanta", roId),
                new County(sibiuId, "Sibiu", roId),

                new County(sofiaId, "Sofia", bgId),
                new County(plovId, "Plovdiv", bgId),
                new County(varnaId, "Varna", bgId),
                new County(burgasId, "Burgas", bgId),
                new County(ruseId, "Ruse", bgId),
                new County(plevenId, "Pleven", bgId),

                new County(pestId, "Pest", huId),
                new County(gyorId, "Gyor-Moson-Sopron", huId),
                new County(szegId, "Csongrad-Csanad", huId),
                new County(bacsId, "Bacs-Kiskun", huId),
                new County(hevesId, "Heves", huId),
                new County(baranyaId, "Baranya", huId),

                new County(viennaStateId, "Vienna", atId),
                new County(styriaId, "Styria", atId),

                new County(bavariaId, "Bavaria", deId),
                new County(nrwId, "North Rhine-Westphalia", deId),
            };

            var cities = new List<City>
            {
                new City(Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddd01"), "Bucuresti", ilfovId),
                new City(Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddd02"), "Voluntari", ilfovId),
                new City(Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddd03"), "Otopeni", ilfovId),

                new City(Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddd11"), "Cluj-Napoca", clujId),
                new City(Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddd12"), "Turda", clujId),

                new City(Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddd21"), "Brasov", brasovId),
                new City(Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddd22"), "Sacele", brasovId),

                new City(Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddd31"), "Timisoara", timisId),
                new City(Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddd32"), "Lugoj", timisId),

                new City(Guid.Parse("99999999-0000-0000-0000-000000000101"), "Iasi", iasiId),
                new City(Guid.Parse("99999999-0000-0000-0000-000000000102"), "Pascani", iasiId),

                new City(Guid.Parse("99999999-0000-0000-0000-000000000111"), "Constanta", constId),
                new City(Guid.Parse("99999999-0000-0000-0000-000000000112"), "Mangalia", constId),

                new City(Guid.Parse("99999999-0000-0000-0000-000000000121"), "Sibiu", sibiuId),
                new City(Guid.Parse("99999999-0000-0000-0000-000000000122"), "Medias", sibiuId),

                new City(Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee01"), "Sofia", sofiaId),
                new City(Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee02"), "Bankya", sofiaId),

                new City(Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee11"), "Plovdiv", plovId),
                new City(Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee12"), "Asenovgrad", plovId),

                new City(Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee21"), "Varna", varnaId),
                new City(Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee22"), "Devnya", varnaId),

                new City(Guid.Parse("88888888-0000-0000-0000-000000000201"), "Burgas", burgasId),
                new City(Guid.Parse("88888888-0000-0000-0000-000000000202"), "Nessebar", burgasId),

                new City(Guid.Parse("88888888-0000-0000-0000-000000000211"), "Ruse", ruseId),
                new City(Guid.Parse("88888888-0000-0000-0000-000000000212"), "Byala", ruseId),

                new City(Guid.Parse("88888888-0000-0000-0000-000000000221"), "Pleven", plevenId),
                new City(Guid.Parse("88888888-0000-0000-0000-000000000222"), "Levski", plevenId),

                new City(Guid.Parse("ffffffff-ffff-ffff-ffff-fffffffff001"), "Budapest", pestId),
                new City(Guid.Parse("ffffffff-ffff-ffff-ffff-fffffffff002"), "Szentendre", pestId),

                new City(Guid.Parse("ffffffff-ffff-ffff-ffff-fffffffff011"), "Gyor", gyorId),
                new City(Guid.Parse("ffffffff-ffff-ffff-ffff-fffffffff012"), "Sopron", gyorId),

                new City(Guid.Parse("ffffffff-ffff-ffff-ffff-fffffffff021"), "Szeged", szegId),
                new City(Guid.Parse("ffffffff-ffff-ffff-ffff-fffffffff022"), "Hodmezovasarhely", szegId),

                new City(Guid.Parse("77777777-0000-0000-0000-000000000301"), "Kecskemet", bacsId),
                new City(Guid.Parse("77777777-0000-0000-0000-000000000302"), "Baja", bacsId),

                new City(Guid.Parse("77777777-0000-0000-0000-000000000311"), "Eger", hevesId),
                new City(Guid.Parse("77777777-0000-0000-0000-000000000312"), "Gyongyos", hevesId),

                new City(Guid.Parse("77777777-0000-0000-0000-000000000321"), "Pecs", baranyaId),
                new City(Guid.Parse("77777777-0000-0000-0000-000000000322"), "Mohacs", baranyaId),

                new City(Guid.Parse("66666666-0000-0000-0000-000000000401"), "Vienna", viennaStateId),
                new City(Guid.Parse("66666666-0000-0000-0000-000000000402"), "Donaustadt", viennaStateId),

                new City(Guid.Parse("66666666-0000-0000-0000-000000000411"), "Graz", styriaId),
                new City(Guid.Parse("66666666-0000-0000-0000-000000000412"), "Leoben", styriaId),

                new City(Guid.Parse("55555555-0000-0000-0000-000000000501"), "Munich", bavariaId),
                new City(Guid.Parse("55555555-0000-0000-0000-000000000502"), "Nuremberg", bavariaId),

                new City(Guid.Parse("55555555-0000-0000-0000-000000000511"), "Cologne", nrwId),
                new City(Guid.Parse("55555555-0000-0000-0000-000000000512"), "Dusseldorf", nrwId),
            };

            await db.Countries.AddRangeAsync(countries, ct);
            await db.Counties.AddRangeAsync(counties, ct);
            await db.Cities.AddRangeAsync(cities, ct);

            await db.SaveChangesAsync(ct);
        }
    }
}