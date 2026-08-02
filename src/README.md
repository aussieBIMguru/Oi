# Oi — Source Overview

This document describes how the add-in is put together internally: the startup sequence, the protection framework, and the base classes intended for extension. For what Oi does and how to deploy it, see the [root README](../README.md).

## Solution Layout

```text
src/Oi/
    Application.cs              IExternalApplication entry point
    General/                    Cross-cutting statics: session state, authorization, command availability
    Extensions/                 Extension methods on Revit/BCL types (Ext_ prefix)
    Utilities/                  Static helper classes (file I/O, ribbon construction, misc data helpers)
    Commands/                   IExternalCommand implementations, grouped by ribbon pulldown
    Forms/                      WPF windows and a static FormCallers facade
    Protection/                 The protection framework (see below)
    Resources/                  Icons, shared XAML styles, tooltip resx
    Oi.addin, Oi.csproj
```

Namespaces roughly mirror folders (`Oi.Protection`, `Oi.Forms`, `Oi.Commands.Cmds_Delete`, etc). `GlobalUsings.cs` aliases the Revit namespaces (`DB`, `UI`) and the add-in's own static utility classes (`UtilFil`, `UtilDat`, `UtilRib`) so they don't need to be qualified in every file.

## Startup Sequence (`Application.cs`)

`OnStartup` runs in this order:

1. Cache `UICTLAPP`, subscribe to `Idling` (fires once, then unsubscribes itself), register tooltips from the embedded `.resx`.
2. Register the two document-lifecycle registries: `DocumentRegistry` (tracks which `Document`s are open) and `ManagerRegistry` (owns the `SchemaManager` instances and their `IUpdater`s).
3. Call `Authorization.UserIsAdmin()`. This has a side effect regardless of the result: it creates the per-user Oi settings folder/file in Local AppData if missing. In `DEBUG` builds a non-admin sees a message pointing them at the self-appoint command instead of being cut off.
4. If the user is an admin (or the build is `DEBUG`), the ribbon tab, panel, and pulldowns are constructed via the `RibbonPanel_Ext` / `PulldownButton_Ext` helpers, generic on the `IExternalCommand` type so button internal names/icons/tooltips can be derived by convention (see `Ribbon_Utils.CommandClassToBaseName`).

The first `Idling` callback (`OnIdling`) is where the `UIApplication` actually becomes available — `DocumentRegistry.Initialize` captures any documents already open (e.g. if Revit launched by double-clicking a file), and `ManagerRegistry.RefreshCache` is run for each so caches aren't stale on the very first document.

`OnShutdown` just unregisters both registries; there is no other teardown.

## The Protection Framework (`Protection/`)

This is the core of the add-in and the part designed to be reused for protection types beyond deletion/modification. A protection system is composed of four pieces that a `SchemaManager<TProtectionInfo, TProtectionUpdater>` ties together:

```text
SchemaManager<TInfo, TUpdater>   generic orchestrator (schema, cache, events, triggers)
    ├─ ProtectionInfo (abstract)     reads/writes one Extensible Storage entity's fields
    ├─ ProtectionUpdater (abstract)  IUpdater that reacts to the protected ChangeType
    └─ Schema (Revit ExtStorage)     identified by a fixed name + Guid, built via ConfigureSchema
```

`ManagerRegistry` holds one `SchemaManager` instance per protection type as a static property (`DeleteSchemaManager`, `ModifySchemaManager`) and fans out `Register` / `Unregister` / `RefreshCache` calls to all of them. `DocumentRegistry` is the shared source of truth for which documents are currently open; `SchemaManager` subscribes to its `DocumentOpened` / `DocumentCreated` / `DocumentClosing` events (plus sync/reload events directly on `ControlledApplication`) to keep its own per-document cache in sync without every protection type re-implementing that plumbing.

### Why a cache exists

`SchemaManager.ProtectedElementsCache` is a `Dictionary<Document, Dictionary<ElementId, ProtectionReviewItem>>`. Protection checks (`IsProtected`) happen on every relevant Revit change and need to be cheap, and Revit updaters need their trigger `ElementId` list rebuilt (`RefreshIUpdaterTriggers`) whenever protection state changes — reading Extensible Storage per-element on every transaction would be far too slow and, for deletions, the entity may already be gone from the model by the time the updater runs. The cache is rebuilt wholesale via `RefreshCache` (full document scan) whenever a document opens/is created, and updated incrementally by `ProtectElement(s)` / `UnprotectElement(s)`.

### Request/response flow for a protected change

Both `DeleteProtectionUpdater` and `ModifyProtectionUpdater` follow the same shape in `Execute`:

1. Filter the changed/deleted `ElementId`s down to ones the relevant `SchemaManager` reports as protected (cache lookup only).
2. If any are protected, build `ProtectionReviewItem`s and show the review form (`FormCallers.ReviewProtection`).
3. If the user opts to continue, show the bypass code entry form (`FormCallers.BypassProtection`) against a challenge code derived from the specific `ElementId`s involved (`Authorization.BuildUserFacingCode`).
4. On success, unprotect the elements in their own transaction and return — Revit's own operation then proceeds normally against now-unprotected elements.
5. On failure (declined, cancelled, or wrong code), post a `ProtectionFailure` (a `FailureDefinition` registered once in `Application.OnStartup`) against the failing elements, which rolls the whole operation back.

### Adding a new protection type

The framework is meant to support additional schema-based protection systems beyond deletion/modification. To add one:

1. Create a `ProtectionInfo` subclass (override `ProtectionType`, `TryGetProtection`, optionally `GetMessage`).
2. Create a `ProtectionUpdater` subclass with its own fixed `Guid` and an `Execute` implementation (the delete/modify implementations are near-identical templates to copy).
3. Create a `SchemaManager<TYourInfo, TYourUpdater>` subclass with its own fixed schema name + `Guid` and the `ChangeType` to monitor (see `Element.GetChangeTypeElementDeletion()` / `GetChangeTypeAny()` for examples). Override `ConfigureSchema` only if extra fields beyond the four in `Fields.cs` are needed.
4. Register it in `ManagerRegistry` (`InitializeSchemas`, `Register`, `Unregister`, `RefreshCache`) alongside the existing two.

Schema and updater `Guid`s must never change post-release — they're how Revit identifies the Extensible Storage schema and the `IUpdater` registration across sessions/files.

## Authorization (`General/Authorization.cs`)

Two unrelated concerns share this file:

- **Admin gating** — `UserIsAdmin()` checks whether the per-user settings file's first line matches a deterministic hash of the current Windows username (`BuildUserNameCode`). There's no real cryptographic secret here; a valid admin file is just a SHA-256 hash of a username the file itself claims to belong to. This is why the root README calls out that self-appointment only happens in `DEBUG`, and recommends teams change the hashing approach before relying on it.
- **Bypass challenge/response** — `BuildUserFacingCode` hashes the sorted set of protected `ElementId`s a user is trying to bypass; `BuildActualBypassCode` runs that user-facing code through HMAC-SHA256 with a hardcoded `Secret` to produce the code an admin must hand back. Because the challenge is derived from the exact element set, a bypass code is only valid for that specific request. `VerifyUserBypassCode` does the comparison. Both codes are shortened via `ToShortCode` (Base64 → strip padding/slashes → uppercase → truncate).

## Forms (`Forms/`)

All WPF windows live under `Forms/Bases/` (a `.xaml` + `.xaml.cs` pair each) and are only ever invoked through the static `FormCallers` facade in `Forms/FormCallers.cs` — commands and updaters never `new` up a window directly. This keeps call sites terse and gives one place to change dialog behavior (e.g. `Topmost`/`ShowInTaskbar` conventions) across every window.

| Window | Purpose |
|---|---|
| `UserPrompt` | Generic message/confirm dialog (`Message`, `Confirm` static factories used by `FormCallers.Message`/`MessageYesNo`) |
| `ProtectionReasonPrompt` | Free-text reason capture when protecting elements |
| `ProtectionReview` | Lists `ProtectionReviewItem`s for selected/failing elements; exposes `ContinueToBypass` |
| `UserAuthorization` | Displays a challenge code, accepts a response, sets `IsAuthorized` |
| `UserCodeGenerator` | Admin tool: turns a Windows username into an admin authorization code |
| `BypassCodeGenerator` | Admin tool: turns a user's bypass request code into the matching bypass code |

`ProtectionReviewItem` (in `Protection/`, but consumed here) is the read-only view model shared between the review window and the caches — it wraps a `ProtectionInfo` plus display fields (`ElementName`, `Category`) so the UI never touches Extensible Storage directly.

## Commands (`Commands/`)

Files are grouped by ribbon pulldown (`Cmds_Admin`, `Cmds_Delete`, `Cmds_Modify`) rather than one-file-per-command, since the delete and modify commands are structurally parallel (select protected / review / protect / unprotect) and are easiest to compare side by side. Every command is `[Transaction(TransactionMode.Manual)]` and opens its own `Transaction` only around the specific ExtStorage/element writes it needs, deferring everything else to `SchemaManager` methods. `Cmd_UnprotectSelectedElements` re-checks `Authorization.UserIsAdmin()` at execution time even though the button is only ever added to admin ribbons — the check guards against the ribbon/button being reachable in ways that bypass the OnStartup admin gate (e.g. a stale pinned button after admin status is revoked).

## Extensions and Utilities

- **`Extensions/`** — small, focused extension methods named with an `Ext_` prefix to keep IntelliSense clearly separated from base-class members on Revit/BCL types (`Document`, `ElementId`, `UIDocument`, `UIControlledApplication`, `RibbonPanel`, `PulldownButton`, plus `string`/`double`). The ribbon-related ones (`RibbonPanel_Ext`, `PulldownButton_Ext`, `UIControlledApplication_Ext`) are what let `Application.cs` build the whole ribbon declaratively and generically off command types rather than repeating `PushButtonData` boilerplate per button.
- **`Utilities/`** — plain static helper classes rather than extension methods, generally because they don't have one obvious "owning" type: `File_Utils` (settings file read/write, opening links), `Ribbon_Utils` (button metadata/icon lookup by naming convention, backing the extensions above), `Data_Utils` (small dictionary lookup helper).

## Framework vs. Example Implementation

The root README describes Oi as "both a toolkit and an example implementation." Concretely, the reusable framework layer is: `SchemaManager<,>`, `ProtectionInfo`, `ProtectionUpdater`, `DocumentRegistry`, the `FormCallers`/`ProtectionReview` UI, and the `Authorization` challenge/response scheme. The example layer built on top of it is: `DeleteSchemaManager`/`ModifySchemaManager`, `DeleteProtectionInfo`/`ModifyProtectionInfo`, `DeleteProtectionUpdater`/`ModifyProtectionUpdater`, and the `Cmds_Delete`/`Cmds_Modify` command sets. A fork that only wants the framework can delete the example layer and follow the "Adding a new protection type" steps above to build its own.
