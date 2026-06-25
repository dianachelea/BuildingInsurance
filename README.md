# Building Insurance API

Building Insurance API este o aplicație backend dezvoltată în **.NET 9**, pentru administrarea unui flux de asigurări pentru clădiri. Proiectul permite gestionarea brokerilor, clienților, clădirilor, polițelor, monedelor, configurațiilor de taxe și factori de risc, precum și generarea de rapoarte administrative.

Aplicația este structurată pe o arhitectură separată pe layere, cu domain logic, application services, infrastructură de persistență și un API REST expus prin ASP.NET Core.

## Funcționalități principale

- Administrare brokeri: creare, actualizare, activare, dezactivare și listare.
- Administrare clienți și clădiri asociate acestora.
- Creare polițe draft, activare polițe și anulare polițe.
- Calcul de primă pe baza taxelor și factorilor de risc configurați.
- Administrare metadate: monede, taxe și factori de risc.
- Date geografice pentru țări, județe/regiuni și orașe.
- Rapoarte pentru polițe grupate după țară, județ, oraș sau broker.
- Procesare asincronă pentru joburi de raportare.
- Worker pentru expirarea polițelor și materializarea datelor de raportare.
- Teste unitare și teste de integrare.

## Tehnologii folosite

- **.NET 9**
- **ASP.NET Core Web API**
- **Entity Framework Core**
- **SQL Server**
- **MediatR** pentru CQRS
- **FluentValidation** pentru validarea requesturilor
- **Polly** pentru mecanisme de reziliență
- **xUnit** și **Moq** pentru testare
- **Swagger / OpenAPI** pentru documentarea endpointurilor

## Structura proiectului

```text
BuildingInsurance.API/                    # API REST ASP.NET Core
BuildingInsurance.Application/            # Use case-uri, comenzi, query-uri, validări, servicii
BuildingInsurance.Domain/                 # Entități, value objects, enum-uri și evenimente de domeniu
BuildingInsurance.Infrastructure/         # EF Core, repository-uri, seeding, migrări, caching, joburi
BuildingInsurance.PolicyExpiration.Worker/# Worker pentru procese de background
BuildingInsurance.Tests/                  # Teste unitare
BuildingInsurance.IntegrationTests/       # Teste de integrare
```

## Cerințe

Pentru rulare locală ai nevoie de:

- .NET SDK 9
- SQL Server sau SQL Server LocalDB
- Un editor precum Visual Studio, Rider sau Visual Studio Code

## Configurare

Connection string-ul implicit se află în `BuildingInsurance.API/appsettings.json`:

```json
"ConnectionStrings": {
  "BuildingInsuranceDb": "Server=(localdb)\\MSSQLLocalDB;Database=BuildingInsuranceDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Pentru alt SQL Server, poți suprascrie connection string-ul prin variabilă de mediu:

```bash
ConnectionStrings__BuildingInsuranceDb="Server=localhost,1433;Database=BuildingInsuranceDb;User Id=sa;Password=your_password;TrustServerCertificate=True;"
```

La pornirea API-ului, aplicația aplică migrările EF Core și rulează seed pentru datele inițiale, dacă baza de date este goală.

## Rulare locală

Clonează repository-ul și intră în folderul soluției:

```bash
git clone <repo-url>
cd <repo-folder>
```

Restaurează pachetele:

```bash
dotnet restore BuildingInsurance.slnx
```

Build:

```bash
dotnet build BuildingInsurance.slnx
```

Pornește API-ul:

```bash
dotnet run --project BuildingInsurance.API
```

În mediul de development, documentația Swagger este disponibilă la:

```text
/swagger
```

## Rulare worker

Worker-ul rulează procese de background pentru expirarea polițelor, materializarea datelor de raportare și procesarea joburilor de raportare.

```bash
dotnet run --project BuildingInsurance.PolicyExpiration.Worker
```

## Testare

Rulează toate testele:

```bash
dotnet test BuildingInsurance.slnx
```

Rulează doar testele unitare:

```bash
dotnet test BuildingInsurance.Tests
```

Rulează testele de integrare:

```bash
dotnet test BuildingInsurance.IntegrationTests
```

## Endpointuri principale

### Administrator

- `POST /api/admin/brokers` - creează broker
- `GET /api/admin/brokers` - listează brokeri
- `GET /api/admin/brokers/{brokerId}` - detalii broker
- `PUT /api/admin/brokers/{brokerId}` - actualizează broker
- `POST /api/admin/brokers/{brokerId}/activate` - activează broker
- `POST /api/admin/brokers/{brokerId}/deactivate` - dezactivează broker
- `POST /api/admin/currencies` - creează monedă
- `GET /api/admin/currencies` - listează monede
- `POST /api/admin/fees` - creează configurație de taxă
- `GET /api/admin/fees` - listează configurații de taxe
- `POST /api/admin/risk-factors` - creează configurație factor de risc
- `GET /api/admin/risk-factors` - listează factori de risc
- `GET /api/admin/reports/policies-by-country` - raport polițe după țară
- `GET /api/admin/reports/policies-by-county` - raport polițe după județ/regiune
- `GET /api/admin/reports/policies-by-city` - raport polițe după oraș
- `GET /api/admin/reports/policies-by-broker` - raport polițe după broker
- `POST /api/admin/reports/jobs` - creează job de raportare
- `GET /api/admin/reports/jobs/{jobId}` - status job raportare
- `GET /api/admin/reports/jobs/{jobId}/result` - rezultat job raportare

### Broker

- `POST /api/brokers/clients` - creează client
- `GET /api/brokers/clients` - listează clienți
- `GET /api/brokers/clients/{clientId}` - detalii client
- `PUT /api/brokers/clients/{clientId}` - actualizează client
- `POST /api/brokers/clients/{clientId}/buildings` - creează clădire pentru client
- `GET /api/brokers/clients/{clientId}/buildings` - listează clădirile unui client
- `GET /api/brokers/buildings/{buildingId}` - detalii clădire
- `PUT /api/brokers/buildings/{buildingId}` - actualizează clădire
- `GET /api/brokers/countries` - listează țări
- `GET /api/brokers/countries/{countryId}/counties` - listează județe/regiuni
- `GET /api/brokers/counties/{countyId}/cities` - listează orașe
- `POST /api/brokers/policies` - creează poliță draft
- `GET /api/brokers/policies` - listează polițe
- `GET /api/brokers/policies/{policyId}` - detalii poliță
- `POST /api/brokers/policies/{policyId}/activate` - activează poliță
- `POST /api/brokers/policies/{policyId}/cancel` - anulează poliță

## Arhitectură

Proiectul folosește o separare clară a responsabilităților:

- **Domain** conține modelul de business: entități, enum-uri, value objects și evenimente de domeniu.
- **Application** conține use case-urile aplicației, organizate în comenzi și query-uri MediatR, plus validări FluentValidation.
- **Infrastructure** implementează persistența cu EF Core, repository-uri, migrări, caching și joburi de procesare.
- **API** expune funcționalitățile prin controllere REST.
- **Worker** rulează procese de background pentru operațiuni periodice și asincrone.

## Status proiect

Proiect realizat ca aplicație backend pentru gestionarea asigurărilor de clădiri, cu accent pe arhitectură curată, validare, persistență, testare și procese de background.