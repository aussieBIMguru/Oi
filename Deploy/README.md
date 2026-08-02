# Deploy

This folder contains pre-built Autodesk Application Plug-in bundles (`Oi.bundle`) for manual deployment.

## Folder Structure

* **Bundles/Pre 2027** – For Revit 2026 and earlier.
* **Bundles/2027** – For Revit 2027 and later.

## Installation

Copy the appropriate `Oi.bundle` folder to the Autodesk Application Plug-ins directory for the target Revit version.

### Revit 2026 and Earlier

```text
C:\ProgramData\Autodesk\ApplicationPlugins
```

### Revit 2027 and Later

```text
C:\Program Files\Autodesk\ApplicationPlugins
```

Restart Revit after copying the bundle.

## Code Signing

The assemblies included in these bundles are **not code signed**.

As a result, Revit will display the **"Always Load"** security prompt when the add-in is first loaded. If your deployment requires this prompt to be avoided, you should apply your own trusted code-signing certificate to the assemblies as part of your deployment process.

Given that this add-in is intended to provide non-optional element protection, code signing is strongly recommended for production deployments.

## Installer

No installer is currently provided. The bundles are intended for manual deployment or incorporation into your own deployment process.

An installer may be provided in the future, however this is not currently planned as code signing infrastructure is not yet available.
