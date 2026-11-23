# Bakery París (eShopEnhanced)

ASP.NET Core 10 MVC bakery shop: EF Core 10 (SQL Server LocalDB), ASP.NET Core Identity, a Blazor Server search component, vanilla JS (`fetch` + JSON endpoints), Bootstrap 5. Public portfolio project: keep the code clean and professional.

## Layout
- `eShopEnhanced/`: the web project (Controllers, Data, Models, Repositories, Services, ViewModels, ViewComponents, TagHelpers, Components (Blazor), Areas/Identity, Views, wwwroot, Migrations)
- `eShopEnhanced.Tests/`: xUnit + Moq unit tests, and `WebApplicationFactory` integration tests on in-memory SQLite (`Integration/BakeryWebApplicationFactory.cs`)
- `docs/`: `NOTES.md` (concept explanations moved out of the code) and README screenshots
- `.github/workflows/ci.yml`: builds and tests on every push to `main`

## Commands (from the repo root unless noted)
- Run: `dotnet run --project eShopEnhanced`. On start-up the database is created, migrated and seeded (`Data/DbInitializer.cs`).
- Test: `dotnet test` (53 tests)
- EF migrations: run **from `eShopEnhanced/`**: `dotnet ef migrations add <Name>`, then `dotnet ef database update`. `dotnet-ef` is a local tool (`eShopEnhanced/dotnet-tools.json`). Run `dotnet build` first; with `--no-build`, a migration added after the last build isn't seen.
- If the developer's debugger (F5) is running, `bin/` is locked: build or test with a separate output folder (`dotnet build -o <tmp>`). `dotnet ef` needs the app stopped.

## Conventions
- General names are `Product` / `ProductController` / `IProductRepository`; "Pie" only for real pies (product names, the "Pies" category).
- Hierarchy: `Category` (Pies, Cakes, ...) > `SubCategory` (Fruit pies, ...) > `Product`.
- Repositories are async and return materialised `List<T>`; read queries use `AsNoTracking()`.
- Package versions: EF Core packages 10.0.15; Microsoft.AspNetCore.* packages 10.0.13 (the installed ASP.NET Core runtime).
- Comments explain *why*, not basic concepts (those belong in `docs/NOTES.md`).
- Prices are formatted with the `el-GR` culture (`15,95 €`); JSON endpoints send preformatted text (`cartTotalText`).

## Gotchas (all hit in practice)
- Products are loaded `AsNoTracking`: set `ProductId`, never the `Product` navigation, on cart and order lines, or EF inserts the product again.
- EF scaffolds a class rename as drop + create table. Read every migration and rewrite renames with `RenameTable` / `RenameColumn`.
- `[Required]` must be explicit: the implicit "non-nullable means required" rule is skipped when a field is missing from the request.
- HTML forms can't be nested: the navbar search dropdown (with add-to-cart forms) sits next to the search `<form>`, not inside it.
- Blazor search page lives under `/products/`, so it needs `<base href="~/">` (via the layout's `Head` section) and `~/_framework/blazor.server.js`.
- Blazor Server components are long-lived: use a fresh DI scope per database operation.
- Identity UI pages get the shop layout via `Areas/Identity/Pages/_Layout.cshtml` (same path as the package's own layout).
- Category lookup and search rely on SQL Server's case-insensitive collation; the SQLite test factory adds `NOCASE` to compensate.
- State-changing actions are POST + anti-forgery; redirects use `Url.IsLocalUrl` / `LocalRedirect`.

## Testing manually
- Test scripts must clean up only their own data (e.g. users with `@test.example` emails, carts created during the test), never all rows: the developer's own cart and account live in the same LocalDB database.
