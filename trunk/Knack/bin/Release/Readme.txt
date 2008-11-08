Copyright 2004, 2005, 2006, 2007, 2008 Riccardo Gerosa.
Knack is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Knack is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Knack.  If not, see <http://www.gnu.org/licenses/>.
mailto: r.gerosa@gmail.com - All rights reserved.

* INTRODUCTION

Knack is a modular sound synthesizer, it does Additive and FM synthesis.
This software is developed using C# and the .NET Framework, a MIDI input
is supported (you can use a MIDI keyboard to play) and a Managed 
DirectX 9 sound output.

* INSTALLATION

Knack is designed to work under Windows 2000/XP/Vista or later.
This program requires that the following components are installed
in the following order:

   1. The Microsoft .NET Framework Runtime v.1.1 or later
   
   2. The Microsoft DirectX 9 Runtime or later, installed
   using the following command line: 
   DXSetup.exe /InstallManagedDX
   (you have to use the full version of the DirectX installer,
   not the version which downloads the components from the web)

If you did install the .NET Runtime after the DirectX Runtime, 
then reinstall the DirectX Runtime (the Managed DirectX
libraries can't be installed if the .NET Framework is not 
already present).
These Windows components are available for free on the 
Microsoft web site and through Windows Update.
When you have installed these components you can simply install
the knack setup file.

* FEATURES

Stereo synthesis and mixing with floating point 32bit precision.
Different FM operators and waveforms. A stereo delay effect 
with feedback. A random note generator. XML based settings and file format.
This project is a work in progress, it's not complete yet and
it's still under developement.
