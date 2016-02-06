# Tools #

Develompment is being done using SharpDevelop v.3.0 under Windows.
There are also MonoDevelop project files for development under Linux.
Compatibility of project files with Visual Studio has not been tested yet.
Please, never modify project files in a way that alters compatibility
with SharpDevelop or MonoDevelop.

# Code Style #

Knack development started in 2004 using .NET v1.1, that's why
even if the current version is compiled for .NET v.2.0 most of the code does not use the improvements of C# 2.0 (no generics etc.).
Note that if you want to use SharpDevelop v.3.0 for development you need to install also the .NET Framework v.3.5.

# Dependencies #

Knack requires the .NET Framework v.2.0 or later.

The current version of Knack includes Tablet support under Windows XP Tablet Edition, Windows Vista, or later. For this functionality a managed DLL called WinTablet.dll is used (source is included in the trunk).

There's also an older version of this library called WinTab32m.dll (source included in the trunk), this version works with every version of windows that has a WinTab32 driver (usually available when using tablet in older versions of Windows). This file is not being used at the moment but may be enabled changing a few lines of code in Knack.

Sound output currently requires Managed DirectX 9.

# How to contribute #

Please look at page ThingsToDo