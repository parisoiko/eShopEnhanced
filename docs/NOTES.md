# Study notes

These are the notes I wrote while building this shop to learn ASP.NET Core MVC, EF Core, Razor,
Blazor and Identity. They used to live as comments in the code; the code now only keeps comments
that explain *why* something is done. Paths are relative to the `eShopEnhanced/` project folder
unless stated otherwise.

## Contents

1. [MVC and routing](#1-mvc-and-routing)
2. [Razor views, partials, view components and tag helpers](#2-razor-views-partials-view-components-and-tag-helpers)
3. [EF Core and migrations](#3-ef-core-and-migrations)
4. [Model binding and validation](#4-model-binding-and-validation)
5. [Shopping cart, session and TempData](#5-shopping-cart-session-and-tempdata)
6. [JavaScript: fetch and JSON endpoints](#6-javascript-fetch-and-json-endpoints)
7. [Blazor Server live search](#7-blazor-server-live-search)
8. [Identity and authentication](#8-identity-and-authentication)
9. [Bootstrap and front-end details](#9-bootstrap-and-front-end-details)
10. [Testing](#10-testing)
11. [C# language bits](#11-c-language-bits)

---

## 1. MVC and routing

**Attribute routing** — `Controllers/ProductController.cs`
- `[Route("products")]` on the controller means every URL of that controller starts with `/products`.
  The actions then add their own part with `[HttpGet("...")]`:
  - `/products` and `/products/cupcakes` → `List(string? category)` (`[HttpGet("")]` + `[HttpGet("{category}")]`).
  - `/products/search` → `Search`. A literal segment like `search` wins over a parameter like
    `{category}`, because a fixed word is more specific.
  - `/products/7/black-forest-cake` → `Details`. The `{id:int}` route constraint only matches numbers,
    so `/products/7` is a product and `/products/cakes` is a category.
- The **slug** (`Helpers/Slug.cs`) only makes the URL readable; the id alone finds the product.
  A missing or wrong slug gets a permanent (301) redirect to the one correct URL, so every product
  has exactly one address.
- `return NotFound();` gives a 404, e.g. for an unknown category (`/products/pizza`) or an unknown
  product id (`/products/999`).

**Conventional routing** — `Program.cs`
- The other controllers use the default route `{controller=Home}/{action=Index}/{id?}`, so
  `/ShoppingCart` is `ShoppingCartController.Index` and `/ShoppingCart/MiniCart` is `MiniCart`.

**GET vs POST, anti-forgery** — `Controllers/ShoppingCartController.cs`, `Controllers/OrderController.cs`
- `[HttpPost]`: only a form submit can change the cart or place an order, never a plain link click
  (links and crawlers only do GET).
- `[ValidateAntiForgeryToken]`: rejects posts that didn't come from one of our own forms (CSRF
  protection). The form tag helper adds the hidden token to every `<form method="post">` automatically.
- The add/remove actions take simple parameters from the form fields: `productId` and an optional
  `returnUrl`.

**Post/Redirect/Get** — `Controllers/OrderController.cs`
- After a successful POST, redirect instead of showing a page directly. Refreshing the thank-you page
  then can't submit the order a second time. TempData carries the order number across that redirect.

**Returning different kinds of results** — `Controllers/ShoppingCartController.cs`, `Controllers/ProductController.cs`
- `View(model)` renders a view inside `_Layout`.
- `PartialView("_MiniCart", model)` renders `Views/ShoppingCart/_MiniCart.cshtml` **without** `_Layout`:
  just an HTML fragment, perfect for JavaScript to insert into the page (also used for
  `_SearchSuggestions`).
- `Json(new { ... })` returns JSON for `fetch` calls (see section 6).
- `[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]` stops the browser from
  showing an old cached copy (used for the mini cart and search suggestions).

**ViewBag** — `Controllers/OrderController.cs`, `Controllers/ProductController.cs`
- `ViewBag` is a quick way to hand a single value to the view without making a view model
  (e.g. `ViewBag.CheckoutCompleteMessage`, read in `Views/Order/CheckoutComplete.cshtml`).

**Safe redirects** — `Controllers/ShoppingCartController.cs`, `Controllers/ProductController.cs`, `Areas/Identity/Pages/Account/Register.cshtml.cs`
- `Url.IsLocalUrl(returnUrl)` only allows addresses on our own site, so nobody can craft a form that
  adds to the cart and then sends the visitor off to another website (open redirect). `LocalRedirect`
  also throws for non-local URLs.

**Middleware pipeline** — `Program.cs`
- Order matters. `UseAuthentication()` first finds out *who* the visitor is (reads the login cookie),
  then `UseAuthorization()` decides *what* they may do. The other way round, authorization would
  always see an anonymous visitor.
- `UseRequestLocalization` with only `el-GR` supported: numbers, prices and dates are formatted the
  Greek way (`15,95 €`) for every request, even if the browser asks for another language.
- `UseSession()` enables the session used by the shopping cart.

---

## 2. Razor views, partials, view components and tag helpers

**Partial views** — `Views/Shared/_ProductCard.cshtml`, `Views/Shared/_AddToCartButton.cshtml`, `Views/Shared/_QuickAddControl.cshtml`
- A partial is a reusable piece of a view. Use it with `<partial name="_ProductCard" model="product" />`.
- `_ProductCard` leaves the surrounding column (`<div class="col-...">`) to the page, so each page
  decides how wide the cards are.

**View components** — `ViewComponents/CategoryMenu.cs`, `ViewComponents/ShoppingCartSummary.cs`
- A view component is like a mini controller + view that can be dropped into any page, including
  `_Layout.cshtml`, and **fetches its own data**. So the navbar category menu and cart summary work on
  every page without the controllers having to pass anything.
- Used in the layout as `<vc:category-menu />` and `<vc:shopping-cart-summary />` (class name in
  kebab-case).
- `return View(model)` inside a view component renders
  `Views/Shared/Components/{ComponentName}/Default.cshtml`.

**Form tag helpers** — `Views/Order/Checkout.cshtml`, `Areas/Identity/Pages/Account/Register.cshtml`

| Tag helper | Renders as |
|---|---|
| `<form asp-action="Checkout">` | `action="/Order/Checkout" method="post"` + a hidden anti-forgery token |
| `<label asp-for="FirstName">` | `<label for="FirstName">First name</label>` (text from `[Display]`) |
| `<input asp-for="FirstName">` | `<input id="FirstName" name="FirstName" type="text" value="...">` (+ `data-val-*` validation attributes) |
| `<span asp-validation-for="...">` | where the error message for that field appears |
| `<div asp-validation-summary="ModelOnly">` | a list of errors not tied to one field |

- The `name="FirstName"` attributes are what model binding uses to fill an `Order` object (section 4).
- On the Register page, `asp-for="Input.FirstName"` points at the `InputModel` in
  `Register.cshtml.cs`; it's the same tag helpers as the checkout form.

**A custom tag helper** — `TagHelpers/EmailTagHelper.cs`, used in `Views/Shared/_Layout.cshtml`
- Write `<email address="..." content="..."></email>` in any view and it becomes
  `<a href="mailto:...">...</a>` on the server.
- How Razor finds it:
  - the class name `EmailTagHelper` minus `TagHelper` = the tag name `<email>`;
  - each public property becomes an attribute in kebab-case (`Address` → `address`, `Content` → `content`);
  - `@addTagHelper *, eShopEnhanced` in `Views/_ViewImports.cshtml` makes all tag helpers in this
    project available.
- In `Process`: `output.TagName = "a"` turns `<email>` into `<a>`; `SetAttribute("href", ...)` adds the
  `mailto:` (Razor HTML-encodes the value for us).
- Attributes that aren't properties of the tag helper (like `class="nav-link"`) are copied onto the
  output element unchanged.

**_ViewImports and _Imports** — `Views/_ViewImports.cshtml`, `Areas/Identity/Pages/_ViewImports.cshtml`, `Components/_Imports.razor`
- `_ViewImports.cshtml` holds the `@using` and `@addTagHelper` lines for every view in its folder and
  below. The Identity area has its own one for *our* pages there (Register); the Identity package's own
  pages have their imports inside the package.
- `_Imports.razor` is the same idea for Blazor components (`.razor` files) in that folder.

**Sections** — `Views/Shared/_Layout.cshtml`, `Views/Order/Checkout.cshtml`, `Views/Product/Search.cshtml`
- `@await RenderSectionAsync("Scripts", required: false)` at the end of `_Layout` lets a page add
  scripts that run *after* jQuery and Bootstrap have loaded (e.g. the validation scripts on checkout).
- A `Head` section lets a page add extra tags to `<head>` (the `<base>` tag on the search page).

**HTML rules worth remembering** — `Views/Shared/_ProductCard.cshtml`, `Views/Product/_SearchSuggestions.cshtml`, `Views/Shared/_Layout.cshtml`
- A button can't sit inside a link, so a product card is no longer one big link: the photo and the
  name link to the details page and the cart button sits separately.
- A form can't be inside another form (the browser silently drops the inner one), which is why the
  search dropdown is *next to* the navbar search form.

---

## 3. EF Core and migrations

**DbContext + Identity** — `Data/eShopEnhancedDbContext.cs`
- `IdentityDbContext` = `DbContext` + the tables ASP.NET Core Identity needs (`AspNetUsers`,
  `AspNetRoles`, `AspNetUserRoles`, ...). It's still our one shop database.
- Identity configures those tables in `base.OnModelCreating(...)`, so that call must stay.
- `IdentityDbContext<ApplicationUser>`: use our own user class (with name and address) instead of
  `IdentityUser`.

**Column types** — `Data/eShopEnhancedDbContext.cs`, `Models/Order.cs`
- `HasPrecision(18, 2)` → `decimal(18,2)`: up to 18 digits, 2 after the decimal point (e.g. 15.95).
- `[StringLength(50)]` is also read by EF Core: the column becomes `nvarchar(50)` instead of
  `nvarchar(max)`. `HasMaxLength` does the same in the fluent API (used for the extra user columns).
- `Ignore(u => u.Initials)`: a calculated property, not stored.

**Querying** — `Repositories/ProductRepository.cs`, `Repositories/CategoryRepository.cs`
- `AsNoTracking()`: we only read, so EF doesn't need to watch the objects for changes (faster, less memory).
- `Include(p => p.SubCategory).ThenInclude(s => s.Category)`: load the subcategory and its category in
  the same SQL query (eager loading).
- LINQ is translated to SQL: `Where` becomes a SQL `WHERE`, so only the matching rows are loaded;
  `FirstOrDefaultAsync` loads only one row; `Contains(...)` becomes `LIKE '%text%'`; `SumAsync` becomes a
  SQL `SUM` (used for the cart count instead of loading every item and its product).
- SQL Server's default collation ignores upper/lower case, so `"cupcakes"` finds `"Cupcakes"` and
  "CAKE", "cake" and "Cake" all match in the search.
- Every repository method runs its query right away (`ToListAsync`) and returns a finished list, so a
  query never runs twice or while a view is being rendered (`Repositories/IProductRepository.cs`).

**Saving** — `Repositories/OrderRepository.cs`, `Services/ShoppingCart.cs`
- Adding the order also adds its `OrderDetails`: EF follows the navigation list and inserts the whole graph.
- Set the **foreign key** (`ProductId = ...`), not the navigation (`Product = product`). Our products are
  loaded with `AsNoTracking`, so EF would think the product is new and try to insert it again.
- `ShoppingCartItem.ProductId` is written out so we can set it directly. It maps to the `ProductId`
  column the migration had already created, when EF had made it as a hidden **shadow property**
  (`Models/ShoppingCartItem.cs`).
- `OrderDetail.Price` copies the price at the moment of ordering, so later price changes don't change
  old orders (`Models/OrderDetail.cs`).

**Seeding and creating the database** — `Data/DbInitializer.cs`
- The DbContext is a **scoped** service (one per request). At start-up there is no request, so we
  create a scope ourselves to get one.
- SQL Server: `Database.Migrate()` applies pending migrations (does nothing when up to date).
  Other providers (SQLite in the integration tests) use `EnsureCreated()` instead, because the
  migrations are SQL Server specific.
- Only an empty database is seeded, so restarting the app doesn't add duplicates.
- The seed dictionaries are built once and reused, so every product points at the *same* `SubCategory`
  object and every subcategory at the same `Category`; EF then inserts each one only once.

**Migrations** — `Migrations/` (repo: `eShopEnhanced/Migrations/`)
- `RenameProduct` was rewritten by hand: EF scaffolded the Pie → Product rename as
  `DropTable("Pies")` + `CreateTable("Products")`, which would lose data. The hand-written version
  renames the table and re-creates the constraints, which are named after the table.

---

## 4. Model binding and validation

**Model binding** — `Controllers/OrderController.cs`
- In `Checkout(Order order)` ASP.NET Core creates an `Order` and fills each property from the form field
  with the same name (`FirstName=Maria` → `order.FirstName = "Maria"`). While doing that it also checks
  the validation rules and writes any problems into `ModelState`.
- If `ModelState.IsValid` is false, `return View(order)` shows the form again *with* the order, so
  everything the visitor typed stays filled in and the error messages appear next to the fields.

**Validation attributes** — `Models/Order.cs`, `Areas/Identity/Pages/Account/Register.cshtml.cs`
- Each rule is checked twice:
  - in the browser, before the form is sent (jQuery Validation, driven by the `data-val-*` attributes
    that `asp-for` writes into the `<input>`), so the visitor gets instant feedback;
  - on the server, during model binding (`ModelState.IsValid`), because browser checks can be bypassed.
- `[Display(Name = "...")]` is the label text. In an error message `{0}` is replaced by the display name,
  `{1}` by the maximum length and `{2}` by the minimum length.
- `[Required]` is written out explicitly: the implicit "non-nullable string = required" rule is **not**
  checked when a field is left out of the request completely (I tested this).
- `[Compare(nameof(Password))]` checks that the two passwords match (Register).

**Client-side validation scripts** — `Views/Order/Checkout.cshtml`, `Views/Shared/_ValidationScriptsPartial.cshtml`
- jQuery Validation + Microsoft's "unobtrusive" adapter read the `data-val-*` attributes and check the
  inputs in the browser. The partial is added in the `Scripts` section, which `_Layout` renders after
  jQuery has loaded.
- ASP.NET Core adds the `input-validation-error` class to invalid inputs; `site.css` gives them a red border.

**Form-wide errors** — `Controllers/OrderController.cs`, `Areas/Identity/Pages/Account/Register.cshtml.cs`
- `ModelState.AddModelError("", "...")`: an empty key means an error for the whole form, shown by
  `<div asp-validation-summary="ModelOnly">`. Used for "your cart is empty" and Identity's errors.

**Over-posting** — `Models/Order.cs`
- `[BindNever]`: model binding never fills these properties from the form, even if someone adds fake
  fields like `OrderTotal=0.01` to the request. Only the server sets `OrderId`, `OrderDetails`,
  `OrderTotal` and `OrderPlaced`.

---

## 5. Shopping cart, session and TempData

**Session-based cart** — `Services/ShoppingCart.cs`, `Program.cs`
- `ShoppingCart.GetCart` reads a `CartId` from the session (or creates a new GUID) and stores it back.
  Cart rows in the database are keyed by that id.
- Because the cart is tied to the session, not to the user, it survives logging in during checkout.
- `ShoppingCart` is registered as a **scoped** service (one per request), so values cached in it
  (like `GetProductAmountsAsync`) are only reused within the same request. A page with 24 product cards
  still needs only one query for "how many of each are in the cart".

**TempData** — `Controllers/ShoppingCartController.cs`, `Controllers/OrderController.cs`, `Views/Shared/_Layout.cshtml`
- TempData survives exactly one redirect: it's set before the redirect, read on the next page, then gone.
- Used for the "added to cart" toast (`TempData["CartMessage"]`) and for the order number on the
  thank-you page (`TempData["OrderId"]`).

**Navbar cart summary** — `ViewComponents/ShoppingCartSummary.cs`, `Views/Shared/Components/ShoppingCartSummary/Default.cshtml`
- `Model.ItemCount` = how many pieces are in the cart (2 cakes + 1 croissant = 3), `Model.Total` = what
  they cost.

---

## 6. JavaScript: fetch and JSON endpoints

**Add/remove without reloading** — `wwwroot/js/site.js`, `Controllers/ShoppingCartController.cs`, `Views/Shared/_AddToCartButton.cshtml`, `Views/Shared/_QuickAddControl.cshtml`
- Every cart button lives in a form with class `js-cart-form` ("site.js may send this form with fetch").
  Instead of letting the browser post it (which reloads the page), site.js sends it with `fetch`.
- A single `submit` listener on `document` handles all of them (event delegation); other forms
  (checkout, search...) are left alone.
- `new FormData(form)` sends all the form fields: `productId`, `returnUrl` and the anti-forgery token.
- site.js sends the header `X-Requested-With: XMLHttpRequest`; a normal form post doesn't. The
  controller checks it and answers with JSON instead of a redirect:
  `{ productId: 7, amount: 2, cartCount: 5, cartTotal: 53.1, cartTotalText: "53,10 €", message: "..." }`.
- `cartTotalText` is formatted on the server with the site's culture (el-GR), so the navbar shows exactly
  the same format as the rest of the site; JavaScript just displays it.
- If anything goes wrong, `form.submit()` falls back to a normal post (page reload). Without JavaScript
  at all, the forms post normally and `returnUrl` brings the visitor back to the same page
  (**progressive enhancement**).
- The button is disabled while the request is on its way, to prevent double clicks.
- All controls for a product are updated (a card *and* a search suggestion can show the same product).

**Building HTML safely** — `wwwroot/js/site.js`
- The toast message is put in with `textContent`, not `innerHTML`, so a product name can never inject HTML.

**Search suggestions** — `wwwroot/js/site.js`, `Controllers/ProductController.cs`, `Views/Product/_SearchSuggestions.cshtml`
- From 2 letters, site.js fetches `/products/search/suggestions?q=...` and inserts the returned HTML
  fragment (a partial view) under the search box.
- **Debouncing**: wait until the visitor pauses typing for 250 ms instead of searching on every key.
- `AbortController` cancels a search that's still running when the visitor types on.
- `returnUrl` = the current page, so the quick-add buttons in the dropdown bring the visitor back there.
- Only the first 6 results are shown; "See all results" opens the full search page.
- Escape or clicking outside closes the dropdown; focusing the box again reopens it.

**Mini cart** — `wwwroot/js/site.js`, `Controllers/ShoppingCartController.cs`, `Views/ShoppingCart/_MiniCart.cshtml`
- Every time the slide-out panel starts opening (`show.bs.offcanvas` event), site.js fetches
  `/ShoppingCart/MiniCart` and puts the HTML into the panel. Loading on open (instead of on every page)
  means pages don't run a cart query nobody looks at.

**Template note** — `wwwroot/js/site.js`
- The project template pointed to https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
  for configuring bundling and minification of static assets.

---

## 7. Blazor Server live search

`Components/ProductSearch.razor`, `Views/Product/Search.cshtml`, `Program.cs`

- The live search is a **Blazor component** that runs on the **server**. `blazor.server.js` in the
  browser keeps a live connection (SignalR) open; every keystroke is sent to the server, `OnInput` runs
  in C#, and Blazor sends back only the parts of the HTML that changed. No page reload, and no
  JavaScript written by us.
- Setup in `Program.cs`: `AddServerSideBlazor()` lets Razor components run interactively on the server;
  `MapBlazorHub()` maps the SignalR endpoint (`/_blazor`) that `blazor.server.js` talks to.
- Hosted in an MVC view with the component tag helper:
  `<component type="typeof(eShopEnhanced.Components.ProductSearch)" render-mode="ServerPrerendered" />`.
  `ServerPrerendered`: the server first sends normal HTML (fast first view, works for search engines),
  then `blazor.server.js` connects and makes it live.
- `param-InitialQuery="..."` sets the component's `[Parameter] InitialQuery` (what was typed in the
  navbar, e.g. `/products/search?q=cake` → `"cake"`).
- `OnInitializedAsync` runs when the component starts: it searches straight away if the page was
  opened with a query.
- Only the search page needs Blazor, so `blazor.server.js` is loaded in that page's `Scripts` section,
  not in `_Layout`. `~/` means "from the root of the site"; without it the browser would look in
  `/products/_framework/`, because the page lives at `/products/search`.
- `<base href="~/" />`: Blazor works out its connection address (`/_blazor`) relative to the page's
  base address. Without it, on `/products/search` it would try `/products/_blazor`.
- Blazor components can't use MVC partials or tag helpers like `asp-route-id`, so the product card
  markup and details link are written out by hand.
- **One DbContext per search**: a Blazor Server component lives as long as the browser tab, and fast
  typing starts overlapping searches. A single shared DbContext would fail with "a second operation was
  started on this context", so each search creates its own scope via `IServiceScopeFactory`.
- A search counter (`latestSearch`) makes sure a slow, old search can never overwrite the results of a
  newer one.

---

## 8. Identity and authentication

**Setup** — `Program.cs`
- `AddDefaultIdentity<ApplicationUser>()` = user accounts stored in our database (via
  `AddEntityFrameworkStores<eShopEnhancedDbContext>()`) plus ready-made Register / Log in / Log out pages,
  the "default UI", built as **Razor Pages**. That's why the app also needs `AddRazorPages()` and
  `MapRazorPages()` (URLs like `/Identity/Account/Login`).
- `RequireConfirmedAccount = false`: no confirmation email needed (the app doesn't send emails yet).

**Custom user** — `Models/ApplicationUser.cs`
- `ApplicationUser : IdentityUser` has everything Identity already has (Email, PasswordHash,
  PhoneNumber...) *plus* the details the checkout form asks for, so registered customers don't type them
  twice. They're stored as extra columns in `AspNetUsers`. `PhoneNumber` already exists on `IdentityUser`.

**Claims** — `Services/AppClaimsPrincipalFactory.cs`, `Views/Shared/_LoginPartial.cshtml`
- When someone logs in, Identity builds a **ClaimsPrincipal**: the bundle of facts about the user (id,
  email...) that's stored, encrypted, in the login cookie and available on every request as `User`.
- `AppClaimsPrincipalFactory` adds the first and last name to that bundle, so the navbar can show the
  initials ("MP") without loading the user from the database on every page.

**Authorization** — `Controllers/OrderController.cs`
- `[Authorize]` on a controller = every action needs a logged-in user. A visitor who isn't logged in is
  sent to `/Identity/Account/Login?ReturnUrl=/Order/Checkout` and comes straight back after logging in.

**Razor Pages and overriding the default UI** — `Areas/Identity/Pages/Account/Register.cshtml(.cs)`, `Areas/Identity/Pages/_Layout.cshtml`
- A Razor Page = a `.cshtml` file (the HTML) + a `PageModel` class (the C#). `@page` at the top makes the
  file a page of its own (URL: `/Identity/Account/Register`).
  - `OnGet()` shows the empty form (like an MVC `[HttpGet]` action);
  - `OnPostAsync()` receives the filled-in form (like an MVC `[HttpPost]` action);
  - `[BindProperty] Input`: model binding fills it from the form, like the `Order` parameter in checkout;
  - `return Page()` shows the form again with what was typed and the error messages.
- Our Register page is at the same path as Identity's built-in one, so it **replaces** it.
- Identity's built-in pages use the layout at `/Areas/Identity/Pages/_Layout.cshtml`. A file at that path
  in our project takes priority over the package's own (plain, unbranded) one. Ours passes everything
  on to the shop's real layout (including the `Scripts` section), so the Identity pages get the same
  header, navbar and footer.
- `UserManager.CreateAsync(user, password)` hashes the password and saves the user. It also checks
  Identity's own rules (password strength, email already taken...) and reports them as errors.
  `SignInManager.SignInAsync` logs the new user in straight away.
- Logging out changes something, so it's a POST form (with anti-forgery token), not a link
  (`Views/Shared/_LoginPartial.cshtml`).

---

## 9. Bootstrap and front-end details

- `mt-auto` in a flex column pushes the button to the bottom, so buttons line up across cards of
  different heights (`Views/Shared/_ProductCard.cshtml`).
- `data-bs-toggle="offcanvas" data-bs-target="#miniCart"` on the navbar cart link: with JavaScript,
  clicking opens the slide-out panel instead of following the link; without JavaScript the `href` still
  goes to the cart page (`Views/Shared/Components/ShoppingCartSummary/Default.cshtml`).
- Bootstrap toasts are hidden until JavaScript shows them (`bootstrap.Toast.getOrCreateInstance(el).show()`);
  they hide themselves after `data-bs-delay` milliseconds (`wwwroot/js/site.js`, `Views/Shared/_Layout.cshtml`).
- The cart badge and total are always in the page (just `hidden` when the cart is empty), so site.js can
  update them after an add/remove.
- The logo is inline SVG rather than `<img src="logo.svg">`, because an SVG loaded as an image can't use
  the page's web fonts. Its colours come from CSS (`currentColor` and `--logo-accent`)
  (`Views/Shared/_Logo.cshtml`, `wwwroot/css/site.css`).

---

## 10. Testing

- Top-level statements in `Program.cs` make the `Program` class internal. `public partial class Program { }`
  makes it public, so the integration tests can start the app in memory with
  `WebApplicationFactory<Program>` (tests in `eShopEnhanced.Tests/` at the repo root).
- The integration tests use SQLite, which is why `DbInitializer` uses `EnsureCreated()` for non-SQL
  Server providers (section 3).
- The explicit `[Required]` behaviour in section 4 was confirmed with a test.

---

## 11. C# language bits

- A `record` (`ViewModels/ShoppingCartSummaryViewModel.cs`) is a short way to write a small class that
  just holds values: `public record ShoppingCartSummaryViewModel(int ItemCount, decimal Total);`.
- `??=` assigns only when the left side is null; used to cache query results for the rest of the request
  (`Services/ShoppingCart.cs`).
