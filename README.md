# Oi

**Oi** is an open-source Autodesk Revit add-in that provides model protection tools for controlling how elements can be modified within a project. It allows teams to protect important model content from accidental or unauthorized changes while providing a controlled administrative bypass workflow.

## Features

### Element Protection

* Protect elements from **deletion**
* Protect elements from **any modification or editing**
* Store protection metadata directly on elements using Revit Extensible Storage
* Record information about who applied protection and why it was applied

### Administration

* Assign trusted administrators who can authorize temporary bypasses
* Generate one-time bypass codes for specific protection requests
* Standard users can request authorization without having unrestricted access

### User Experience

* Browse protected elements through dedicated user interfaces
* View protection information and the reason protection was applied
* Simple administrator workflow with separate user authorization process

### Framework

Oi is designed as both a toolkit and an example implementation. In addition to the supplied protection systems it includes:

* Generic base classes for implementing additional schema-based protection systems
* Reusable schema management infrastructure
* Reusable bypass authorization framework
* Reusable protection management user interfaces

## Compatibility

* Autodesk Revit **2025**
* Autodesk Revit **2026**
* Autodesk Revit **2027**

The add-in has **no external library dependencies**.

Because protection data is stored using **Revit Extensible Storage**, all users working within a shared model should have Oi installed. Without the add-in present, protection functionality cannot be enforced consistently.

## Security Model

Only administrators are presented with the Oi ribbon inside Revit.

Administrator status is determined using a hashed authorization file stored within the user's **Local AppData** under an **Oi** folder. The hash is generated against the current Windows username, preventing administrator credentials from simply being copied between machines or user accounts.

To establish the first administrator:

1. Run Oi in **Debug** mode from Visual Studio.
2. Use the built-in self-appointment tools available only during debugging.
3. Generate administrator credentials for your environment.

When deploying Oi within an organisation, it is strongly recommended that you review and customise the hashing implementation to suit your own security requirements before production deployment.

## Repository Structure

```text
Deploy/
    Deployment bundles for Revit 2025-2027

src/
    C# source code
    Additional AI-generated technical documentation
```

The `Deploy` folder contains Autodesk bundle structures that simplify installation onto client machines.

## Deployment

Deployment bundles are provided in the `Deploy` folder for supported Revit versions.

Since protection relies on installed updaters and Extensible Storage, Oi should be deployed to every workstation that opens protected project files.

## Author

Produced by **Gavin Nicholls**.

## License

Please read the project license before using, redistributing, or modifying this software. The license governs both use of the application and reuse of the source code.
