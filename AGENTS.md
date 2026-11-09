# AGENTS.md

This file provides guidance to the AI agent when working with code in this repository.

## Build, test, lint

.NET 10 (pinned in `global.json`). Solution is `SkillSync.slnx`.

```bash
dotnet build -c Release    # lint = build; analyzers + .editorconfig run via EnforceCodeStyleInBuild
dotnet test                # there is NO separate lint command
```

- **Warnings are errors only when `CI=true`** (`Directory.Build.props`). Locally, run `CI=true dotnet build -c Release` before declaring work done — CI fails on any warning.
- Run the architecture suite alone with `dotnet test tests/SkillSync.ArchitectureTests`. It is the main safety net; treat failures as design errors, not test bugs to loosen.

## Architecture rules (enforced by tests — do not weaken them)

- Each module under `src/Modules/<Name>/` has two projects: `SkillSync.<Name>` and `SkillSync.<Name>.Contracts`.
- A module may reference **only** `SkillSync.SharedKernel` and its own `.Contracts`. Cross-module communication goes through the other module's `.Contracts` project.
- `.Contracts` projects reference only `SkillSync.SharedKernel`, and must have **no** `PackageReference` or `FrameworkReference`.
- `SkillSync.SharedKernel` references nothing.
- Layers are namespaces inside the module assembly: `SkillSync.<Name>.{Domain,Application,Infrastructure,Endpoints}`. Allowed direction: `Domain` ← `Application`/`Infrastructure` ← `Endpoints`. `Domain` and `Application` must not touch EF Core or ASP.NET (use abstractions in Domain/Application, implement in Infrastructure).
- **Exactly one public type per module: `SkillSync.<Name>.<Name>Module`** (a `public static class`). Everything else — including DbContexts — must be `internal`. `IdentityModule.cs` is the reference layout.
- Adding a module means updating: `ModuleCatalog.Names`, the `ProjectReference` list in the architecture test csproj, and `SkillSync.slnx`.

## Config and secrets

- `appsettings*.json` must contain **no** secrets: any non-empty value under a key matching `password|secret|signingkey|apikey|token|privatekey` fails `SecretsTests`. Copy a connection string with no `Password=` either.
- Required at runtime (from user-secrets or env vars, e.g. `Jwt__SecretKey`): `Jwt:SecretKey`, `Jwt:Issuer`, `Jwt:Audience`. The API dereferences `Jwt:SecretKey` with `!` at startup, so it crashes without it.
- gitleaks runs on pushed history — never commit real keys, even in a throwaway branch.

## Gotchas

- The messaging abstractions live in `src/SkillSync.SharedKernel/Messsaging/` — **triple s**, and the namespace is `SkillSync.SharedKernel.Messsaging`. Match it exactly.
- `.github/dependonabot.yml` is misspelled (should be `dependabot.yml`), so Dependabot is not active. Renaming it is a valid fix, not scope creep.
- Use `SkillSync.SharedKernel.Results.Result` / `Result<T>` and `Results.Error` for outcomes instead of throwing. Commands/queries implement `ICommand`/`IQuery` from `...Messsaging`.

## Repo etiquette

- Branch off `main` as `feature/<name>`, `chore/<name>`, or `fix/<name>`.
- Commit subjects use Conventional Commits (`feat:`, `fix:`, `ci:`, `chore:`); imperative mood.
