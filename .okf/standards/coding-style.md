# Coding Style Standard

## C#

- Use nullable reference types.
- Prefer small records/classes for explicit request and response contracts.
- Use async APIs with `CancellationToken` for service, repository, and endpoint operations.
- Keep public interfaces stable and named according to `REQUEST.md` unless the task explicitly changes them.
- Prefer clear domain names: `ShortLink`, `ShortLinkClick`, `IShortLinkService`, `IShortCodeGenerator`, `IShortLinkRepository`.
- Keep comments sparse and only where they explain non-obvious behavior.

## Packaging

- Reusable projects must build independently from demo hosts.
- NuGet package metadata should include package id, description, authors, repository URL placeholder if needed, license/readme metadata when available, and tags.
- Avoid package references in reusable projects that force consumers to install demo-only dependencies.

## TypeScript/React

- Use TypeScript types for API requests and responses.
- Keep components small and feature-scoped.
- Use Tailwind utility classes for styling.
- Keep copy-to-clipboard and fallback states explicit in the UI.

## Documentation

- Update README when commands, package usage, configuration, endpoints, or setup steps change.
- Keep `.okf` task and phase files synchronized with completed work.

