# PRODUCT_VISION.md - Shorten Link Library + Demo App

## 1. Product Vision

Xây dựng một bộ thư viện rút gọn link cho hệ sinh thái .NET, được tách thành project/package riêng có thể `dotnet pack` thành NuGet để nhiều project khác reference lại, đồng thời cung cấp một demo application hoàn chỉnh để chứng minh luồng tạo link, quản lý link và redirect hoạt động thực tế.

Sản phẩm không chỉ là một demo URL shortener đơn lẻ. Mục tiêu dài hạn là tạo một nền tảng thư viện có kiến trúc rõ ràng, cấu hình linh hoạt, dễ thay đổi database/cache/analytics, và đủ sạch để đóng gói thành NuGet hoặc nhúng vào các service .NET khác.

## 2. Target Users

- Developer .NET muốn tích hợp tính năng rút gọn link vào ứng dụng hiện có.
- Backend/API project cần một module short link có thể cấu hình bằng DI.
- Demo/admin user cần giao diện web đơn giản để tạo, xem, copy và kiểm tra short link.
- Maintainer cần một codebase rõ ràng, có test, có hướng dẫn chạy local bằng SQLite và mở rộng sang PostgreSQL/Redis.

## 3. Product Principles

- Library-first: logic cốt lõi phải nằm trong library reusable, không bị khóa vào demo app.
- Package-first: phần Shorten Link library phải là project riêng có metadata/package setup rõ ràng để pack NuGet và dùng từ source .NET khác.
- Config-driven: database provider, redirect fallback, analytics và cache phải điều khiển bằng cấu hình.
- Thin demo app: API demo và React app dùng thư viện để chứng minh behavior, không chứa business logic trùng lặp.
- Safe default: SQLite là mặc định để chạy local nhanh, PostgreSQL/Redis là tùy chọn mở rộng.
- Clear contracts: endpoint, model, service interface và DI extension phải ổn định để project khác có thể dùng lại.
- Production-aware: phase đầu có thể tối giản, nhưng thiết kế phải chừa đường cho analytics, cache, worker, rate limiting và CI.

## 4. Core Product Capabilities

- Tạo short link từ URL hợp lệ.
- Hỗ trợ custom alias với validate ký tự và chống duplicate.
- Redirect từ `/{code}` về original URL.
- Xử lý link không tồn tại hoặc hết hạn theo cấu hình fallback.
- Lưu trữ bằng SQLite mặc định, có thể chuyển PostgreSQL bằng config.
- Expose DI extension `AddShortenLink(...)` và endpoint mapping `MapShortenLinkEndpoints()`.
- Đóng gói phần library thành NuGet package riêng để consumer có thể cài và gọi `AddShortenLink(...)`.
- Cung cấp React UI để tạo link, xem kết quả, copy short URL và hiển thị fallback page.
- Có test tối thiểu cho generator, validation, service và SQLite integration.

## 5. Desired Architecture

```txt
src/
  ShortenLink.Core/              # Domain models, interfaces, validation, core service
  ShortenLink.Infrastructure/    # EF Core, repositories, SQLite/PostgreSQL providers
  ShortenLink.AspNetCore/        # DI setup, options, endpoint mapping, middleware helpers
  ShortenLink.Worker/            # Optional analytics/background jobs
  ShortenLink.Api/               # Demo backend API using the library
  ShortenLink.Web/               # React + Vite frontend

tests/
  ShortenLink.Core.Tests/
  ShortenLink.Infrastructure.Tests/
  ShortenLink.Api.Tests/
```

## 6. Roadmap And Phase Goals

### Phase 1 - MVP

Goal: Deliver a reusable core short link library with SQLite persistence and a working demo flow from React form to API creation to redirect.

Scope:

- Create solution/project structure for core library, infrastructure, ASP.NET Core integration, demo API, demo Web, and tests.
- Keep the Shorten Link library projects separate from the demo API/Web so they can be packed and consumed independently.
- Add basic NuGet package metadata for the reusable library surface.
- Implement `ShortLink`, optional `ShortLinkClick`, request/result DTOs, and core service contracts.
- Implement Base62 short code generation with default length 7.
- Implement custom alias validation for letters, numbers, `_`, and `-`.
- Reject empty, malformed, and non-HTTP/HTTPS URLs.
- Implement duplicate-code retry and duplicate custom alias handling.
- Implement EF Core SQLite persistence with required indexes.
- Expose `AddShortenLink(builder.Configuration)` for DI registration.
- Implement demo API endpoints:
  - `POST /api/short-links`
  - `GET /{code}`
  - `GET /api/short-links/{code}`
  - `DELETE /api/short-links/{code}`
- Implement redirect behavior for active links.
- Implement fallback behavior for unknown code through config:
  - frontend fallback when enabled
  - JSON 404 when disabled
- Build React home page with long URL input, custom alias input, optional expiry, create button, result display, and copy button.
- Build React detail and not-found/fallback pages.
- Add minimum tests for generator, URL validation, create/resolve service, and SQLite integration.
- Add README instructions for local SQLite run.
- Add README instructions for packing and consuming the library from another .NET project.

Success criteria:

- Solution builds successfully.
- Demo API runs with SQLite default config.
- React app can create a short link through the API.
- `GET /{code}` redirects to the original URL.
- Custom alias works and duplicate alias is rejected.
- Unknown code follows configured fallback behavior.
- Library can be referenced by another .NET project through DI.
- Reusable library can be packed with `dotnet pack`.
- Minimum core and SQLite tests pass.

### Phase 2 - PostgreSQL Toggle

Goal: Allow the same library and demo API to switch between SQLite and PostgreSQL by configuration only, without changing application code.

Scope:

- Add PostgreSQL EF Core provider support.
- Implement `ShortenLink:Database:UsePostgres` toggle.
- Ensure `UsePostgres = false` selects SQLite.
- Ensure `UsePostgres = true` selects PostgreSQL.
- Keep repository/service/API contracts unchanged.
- Keep NuGet package boundaries unchanged while adding PostgreSQL support.
- Add or document migration strategy for both SQLite and PostgreSQL where practical.
- Add configuration samples for both providers.
- Add README guide for running with PostgreSQL.
- Add integration verification for provider selection and PostgreSQL setup where environment allows.

Success criteria:

- App continues to run with SQLite as default.
- App can run with PostgreSQL when `UsePostgres = true` and a valid connection string is provided.
- No code changes are required to switch database provider.
- Unique code index and date indexes exist for both providers.
- README documents migration/update database commands and config examples.

### Phase 3 - Production Readiness

Goal: Strengthen the system for real-world usage by adding analytics, cache, async processing, operational packaging, and CI.

Scope:

- Implement click tracking abstraction.
- Track click count, IP address, user agent, referer, and clicked timestamp.
- Add async worker/channel path so redirect is not slowed by analytics persistence.
- Implement cache abstraction for redirect lookup.
- Add in-memory cache provider if useful for local mode.
- Add Redis provider controlled by config.
- Ensure cache lookup happens before database lookup.
- Ensure cache is invalidated when link is deleted/deactivated.
- Add rate limiting for create and redirect-sensitive endpoints.
- Add Docker Compose for API, frontend, PostgreSQL, and Redis where appropriate.
- Add GitHub Actions CI for build and tests.
- Expand tests for cache behavior, analytics behavior, and endpoint contracts.

Success criteria:

- Redirect path remains fast and does not synchronously depend on analytics persistence.
- Cache miss falls back to database and then stores successful lookup.
- Delete/deactivate removes cache entries.
- Redis can be enabled by config.
- Local stack can be started through documented Docker Compose commands.
- CI validates build and tests on every push or pull request.

## 7. Non-Goals For Early Phases

- Public SaaS billing, tenant management, or authentication.
- Full admin dashboard beyond basic demo needs.
- Advanced analytics reports or charts.
- Distributed ID/key coordination beyond current retry-based uniqueness.
- Hard dependency on PostgreSQL, Redis, or worker infrastructure in Phase 1.

## 8. Definition Of Done For The Product

The product is considered complete for the requested vision when:

- The reusable library exposes stable models, services, repositories, options, DI setup, and optional endpoint mapping.
- The reusable library is isolated from the demo app and can be packed as a NuGet package.
- The demo API proves create, detail, delete/deactivate, and redirect flows.
- The React demo proves a user can create, copy, inspect, and recover from not-found short links.
- SQLite works by default.
- PostgreSQL can be enabled by configuration.
- Optional analytics and cache are designed as abstractions and implemented in the production-readiness phase.
- Tests cover the most important core logic and persistence behavior.
- README explains how to run, configure, test, and reuse the library.
