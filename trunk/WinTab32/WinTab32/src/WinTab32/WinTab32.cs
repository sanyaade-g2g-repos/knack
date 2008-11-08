/*
 *  Copyright 2004, 2005, 2006, 2007, 2008 Riccardo Gerosa.
 *
 *  This file is part of Knack.
 *
 *  Knack is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Knack is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Knack.  If not, see <http://www.gnu.org/licenses/>.
 * 
 *  -----------------------------------------------------------
 *  This file is based on WinTab32.pas by LI Qingrui (the3i@sohu.com)
 *  Converted from wintab.h, pktdef.h from www.pointing.com
 *  Detailed documents can be downloaded from www.pointing.com
 *  -----------------------------------------------------------
 *  WinTab is standardized programming interface to digitizing tablets,
 *  three dimensional position sensors, and other pointing devices
 *  by a group of leading digitizer manufacturers and applications developers.
 * 
 *  The manager part is omitted for not being widely used or supported.
 * 
 *  *** Note: Modify definations of PACKETDATA and PACKET to define your own data format. ***
 *
 *  This file is supplied "AS IS", without warranty of any kind.
 *  Feel free to use and modify for any purpose.
 *  Enjoy yourself.
 * 
 */

using System;
using System.Runtime.InteropServices;

namespace WinTab32 
{

	[StructLayout(LayoutKind.Sequential)]
    public struct PACKET
    {
        // public Int32 pkContext;       		// PK_CONTEXT
        // public Double pkStatus;      		// PK_STATUS
        // public Int32 pkTime;          		// PK_TIME
        // public Int32 pkChanged;       		// PK_CHANGED
        // public Double pkSerialNumber;		// PK_SERIAL_NUMBER
        // public Double pkCursor;      		// PK_CURSOR
        // public Int32 pkButtons;       		// PK_BUTTONS
        public Int32 pkX;                		// PK_X
        public Int32 pkY;                		// PK_Y
        // public Int32 pkZ;             		// PK_Z
        public Int32 pkNormalPressure;    		// PK_NORMAL_PRESSURE
        // public Int32 pkTangentPressure;    	// PK_TANGENT_PRESSURE
        // public ORIENTATION pkOrientation;	// PK_ORIENTATION
        // public ROTATION pkRotation;      	// PK_ROTATION  Ver 1.1
    } 
    
    [StructLayout(LayoutKind.Sequential)]
    public struct AXIS
    {
        public Int32 axMin;
        public Int32 axMax;
        public Double axWinTabs;
        public Int32 axResolution;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct LOGCONTEXT
    {
    	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
      	public String lcName;

        public UInt32 lcOptions; // unsupported option will cause WTOpen to fail
        public UInt32 lcStatus;
        public UInt32 lcLocks;   // specify attributes of the context that cannot be changed once the context has been opened
        public UInt32 lcMsgBase;
        public UInt32 lcDevice;  // device whose input the context processes
        public UInt32 lcPktRate; // desired packet report rate in Hertz. returns the actual report rate.
        public Int32 lcPktData;   // which optional data items will be in packets. unsupported items will cause WTOpen to fail.
        public Int32 lcPktMode;   // whether the packet data items will be returned in absolute or relative mode
        public Int32 lcMoveMask;  // which packet data items can generate move events in the context
        public Int32 lcBtnDnMask; // buttons for which button press events will be processed in the context
        public Int32 lcBtnUpMask; // buttons for which button release events will be processed in the context
        public Int32 lcInOrgX;
        public Int32 lcInOrgY;
        public Int32 lcInOrgZ;    // origin of the context's input area in the tablet's native coordinates
        public Int32 lcInExtX;
        public Int32 lcInExtY;
        public Int32 lcInExtZ;    // extent of the context's input area in the tablet's native coordinates
        public Int32 lcOutOrgX;
        public Int32 lcOutOrgY;
        public Int32 lcOutOrgZ;   // origin of the context's output area in context output coordinates, absolute mode only
        public Int32 lcOutExtX;
        public Int32 lcOutExtY;
        public Int32 lcOutExtZ;   // extent of the context's output area in context output coordinates, absolute mode only
        public Int32 lcSensX;
        public Int32 lcSensY;
        public Int32 lcSensZ;     // specifies the relative-mode sensitivity factor
        public Boolean lcSysMode;   // system cursor tracking mode. Zero specifies absolute; non-zero means relative
        public Int32 lcSysOrgX;
        public Int32 lcSysOrgY;    // the origin of the screen mapping area for system cursor tracking, in screen coordinates
        public Int32 lcSysExtX;
        public Int32 lcSysExtY;    // the extent of the screen mapping area for system cursor tracking, in screen coordinates
        public Int32 lcSysSensX;
        public Int32 lcSysSensY;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct ORIENTATION
    {
        public Int32 orAzimuth;
        public Int32 orAltitude;
        public Int32 orTwist;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct ROTATION
    {
        public Int32 roPitch;
        public Int32 roRoll;
        public Int32 roYaw;
    }

    public class WinTab 
    {       
        // Message constants
        public const Int32 WT_DEFBASE = 0x7FF0;
        public const Int32 WT_MAXOFFSET = 0xF;
        public const Int32 WT_PACKET = WinTab.WT_DEFBASE + 0;
        public const Int32 WT_CTXOPEN = WinTab.WT_DEFBASE + 1;
        public const Int32 WT_CTXCLOSE = WinTab.WT_DEFBASE + 2;
        public const Int32 WT_CTXUPDATE = WinTab.WT_DEFBASE + 3;
        public const Int32 WT_CTXOVERLAP = WinTab.WT_DEFBASE + 4;
        public const Int32 WT_PROXIMITY = WinTab.WT_DEFBASE + 5;
        public const Int32 WT_INFOCHANGE = WinTab.WT_DEFBASE + 6;
        public const Int32 WT_CSRCHANGE = WinTab.WT_DEFBASE + 7;
        public const Int32 WT_MAX = WinTab.WT_DEFBASE + WinTab.WT_MAXOFFSET;

        /* 
         PACKET DEFINITION
         The following definition controls what data items are requested from the Tablet during the
         "WTOpen" command. Note that while some tablets will open with all data items requested
         (i.e. X, Y, Z, and Pressure information), some tablets will not open if they do not support
         a particular data item. For example, the GTCO Sketchmaster driver will fail on "WTOpen" if
         you request Z data or Pressure data. However, the SummaSketch driver will succeed on open
         even though Z and Pressure are not supported by this tablet. In this case, 0 is returned for
         the Z and Pressure data, as you might expect.
        */
        public const Int32 PK_CONTEXT = 0x1; // reporting context
        public const Int32 PK_STATUS = 0x2; // status bits
        public const Int32 PK_TIME = 0x4; // time stamp
        public const Int32 PK_CHANGED = 0x8; // change bit vector
        public const Int32 PK_SERIAL_NUMBER = 0x10; // packet serial number
        public const Int32 PK_CURSOR = 0x20; // reporting cursor
        public const Int32 PK_BUTTONS = 0x40; // button information
        public const Int32 PK_X = 0x80; // x axis
        public const Int32 PK_Y = 0x100; // y axis
        public const Int32 PK_Z = 0x200; // z axis
        public const Int32 PK_NORMAL_PRESSURE = 0x400; // normal or tip pressure
        public const Int32 PK_TANGENT_PRESSURE = 0x800; // tangential or barrel pressure
        public const Int32 PK_ORIENTATION = 0x1000; // orientation info: tilts
        public const Int32 PK_ROTATION = 0x2000; // rotation info; 1.1
        
        // this constant is used to define PACKET record
        public const Int32 PACKETDATA = WinTab.PK_X | WinTab.PK_Y | WinTab.PK_NORMAL_PRESSURE;
        
        // this constant is used to define PACKET record
        public const Int32 PACKETMODE = 0;
        
        // Info data defs
        
        // WinTab specifiers
        public const Int32 TU_NONE = 0;
        public const Int32 TU_INCHES = 1;
        public const Int32 TU_CENTIMETERS = 2;
        public const Int32 TU_CIRCLE = 3;
        
        // system button assignment values
        public const Int32 SBN_NONE = 0x00;
        public const Int32 SBN_LCLICK = 0x01;
        public const Int32 SBN_LDBLCLICK = 0x02;
        public const Int32 SBN_LDRAG = 0x03;
        public const Int32 SBN_RCLICK = 0x04;
        public const Int32 SBN_RDBLCLICK = 0x05;
        public const Int32 SBN_RDRAG = 0x06;
        public const Int32 SBN_MCLICK = 0x07;
        public const Int32 SBN_MDBLCLICK = 0x08;
        public const Int32 SBN_MDRAG = 0x09;
        
        // hardware capabilities
        public const Int32 HWC_INTEGRATED = 0x0001;
        public const Int32 HWC_TOUCH = 0x0002;
        public const Int32 HWC_HARDPROX = 0x0004;
        public const Int32 HWC_PHYSID_CURSORS = 0x0008;
        
        // cursor capabilities
        public const Int32 CRC_MULTIMODE = 0x0001; 
        public const Int32 CRC_AGGREGATE = 0x0002; 
        public const Int32 CRC_INVERT = 0x0004; 
        
        // info categories
        public const Int32 WTI_INTERFACE = 1;
        public const Int32 IFC_WINTABID = 1;
        public const Int32 IFC_SPECVERSION = 2;
        public const Int32 IFC_IMPLVERSION = 3;
        public const Int32 IFC_NDEVICES = 4;
        public const Int32 IFC_NCURSORS = 5;
        public const Int32 IFC_NCONTEXTS = 6;
        public const Int32 IFC_CTXOPTIONS = 7;
        public const Int32 IFC_CTXSAVESIZE = 8;
        public const Int32 IFC_NEXTENSIONS = 9;
        public const Int32 IFC_NMANAGERS = 10;
        public const Int32 IFC_MAX = 10;
        public const Int32 WTI_STATUS = 2;
        public const Int32 STA_CONTEXTS = 1;
        public const Int32 STA_SYSCTXS = 2;
        public const Int32 STA_PKTRATE = 3;
        public const Int32 STA_PKTDATA = 4;
        public const Int32 STA_MANAGERS = 5;
        public const Int32 STA_SYSTEM = 6;
        public const Int32 STA_BUTTONUSE = 7;
        public const Int32 STA_SYSBTNUSE = 8;
        public const Int32 STA_MAX = 8;
        public const Int32 WTI_DEFCONTEXT = 3;
        public const Int32 WTI_DEFSYSCTX = 4;
        public const Int32 WTI_DDCTXS = 400; 
        public const Int32 WTI_DSCTXS = 500; 
        public const Int32 CTX_NAME = 1;
        public const Int32 CTX_OPTIONS = 2;
        public const Int32 CTX_STATUS = 3;
        public const Int32 CTX_LOCKS = 4;
        public const Int32 CTX_MSGBASE = 5;
        public const Int32 CTX_DEVICE = 6;
        public const Int32 CTX_PKTRATE = 7;
        public const Int32 CTX_PKTDATA = 8;
        public const Int32 CTX_PKTMODE = 9;
        public const Int32 CTX_MOVEMASK = 10;
        public const Int32 CTX_BTNDNMASK = 11;
        public const Int32 CTX_BTNUPMASK = 12;
        public const Int32 CTX_INORGX = 13;
        public const Int32 CTX_INORGY = 14;
        public const Int32 CTX_INORGZ = 15;
        public const Int32 CTX_INEXTX = 16;
        public const Int32 CTX_INEXTY = 17;
        public const Int32 CTX_INEXTZ = 18;
        public const Int32 CTX_OUTORGX = 19;
        public const Int32 CTX_OUTORGY = 20;
        public const Int32 CTX_OUTORGZ = 21;
        public const Int32 CTX_OUTEXTX = 22;
        public const Int32 CTX_OUTEXTY = 23;
        public const Int32 CTX_OUTEXTZ = 24;
        public const Int32 CTX_SENSX = 25;
        public const Int32 CTX_SENSY = 26;
        public const Int32 CTX_SENSZ = 27;
        public const Int32 CTX_SYSMODE = 28;
        public const Int32 CTX_SYSORGX = 29;
        public const Int32 CTX_SYSORGY = 30;
        public const Int32 CTX_SYSEXTX = 31;
        public const Int32 CTX_SYSEXTY = 32;
        public const Int32 CTX_SYSSENSX = 33;
        public const Int32 CTX_SYSSENSY = 34;
        public const Int32 CTX_MAX = 34;
        public const Int32 WTI_DEVICES = 100;
        public const Int32 DVC_NAME = 1;
        public const Int32 DVC_HARDWARE = 2;
        public const Int32 DVC_NCSRTYPES = 3;
        public const Int32 DVC_FIRSTCSR = 4;
        public const Int32 DVC_PKTRATE = 5;
        public const Int32 DVC_PKTDATA = 6;
        public const Int32 DVC_PKTMODE = 7;
        public const Int32 DVC_CSRDATA = 8;
        public const Int32 DVC_XMARGIN = 9;
        public const Int32 DVC_YMARGIN = 10;
        public const Int32 DVC_ZMARGIN = 11;
        public const Int32 DVC_X = 12;
        public const Int32 DVC_Y = 13;
        public const Int32 DVC_Z = 14;
        public const Int32 DVC_NPRESSURE = 15;
        public const Int32 DVC_TPRESSURE = 16;
        public const Int32 DVC_ORIENTATION = 17;
        public const Int32 DVC_ROTATION = 18; 
        public const Int32 DVC_PNPID = 19; 
        public const Int32 DVC_MAX = 19;
        public const Int32 WTI_CURSORS = 200;
        public const Int32 CSR_NAME = 1;
        public const Int32 CSR_ACTIVE = 2;
        public const Int32 CSR_PKTDATA = 3;
        public const Int32 CSR_BUTTONS = 4;
        public const Int32 CSR_BUTTONBITS = 5;
        public const Int32 CSR_BTNNAMES = 6;
        public const Int32 CSR_BUTTONMAP = 7;
        public const Int32 CSR_SYSBTNMAP = 8;
        public const Int32 CSR_NPBUTTON = 9;
        public const Int32 CSR_NPBTNMARKS = 10;
        public const Int32 CSR_NPRESPONSE = 11;
        public const Int32 CSR_TPBUTTON = 12;
        public const Int32 CSR_TPBTNMARKS = 13;
        public const Int32 CSR_TPRESPONSE = 14;
        public const Int32 CSR_PHYSID = 15; 
        public const Int32 CSR_MODE = 16; 
        public const Int32 CSR_MINPKTDATA = 17; 
        public const Int32 CSR_MINBUTTONS = 18; 
        public const Int32 CSR_CAPABILITIES = 19; 
        public const Int32 CSR_MAX = 19;
        public const Int32 WTI_EXTENSIONS = 300;
        public const Int32 EXT_NAME = 1;
        public const Int32 EXT_TAG = 2;
        public const Int32 EXT_MASK = 3;
        public const Int32 EXT_SIZE = 4;
        public const Int32 EXT_AXES = 5;
        public const Int32 EXT_DEFAULT = 6;
        public const Int32 EXT_DEFCONTEXT = 7;
        public const Int32 EXT_DEFSYSCTX = 8;
        public const Int32 EXT_CURSORS = 9;
        public const Int32 EXT_MAX = 109; // Allow 100 cursors
        
        // Context data defs
        
        public const Int32 LCNAMELEN = 40;
        public const Int32 LC_NAMELEN = 40; 
        
        // context option values
        public const Int32 CXO_SYSTEM = 0x0001;      // the context is a system cursor context
        public const Int32 CXO_PEN = 0x0002;         // the context is a PenWin context, also a system cursor context
        public const Int32 CXO_MESSAGES = 0x0004;    // the context sends WT_PACKET messages to its owner
        public const Int32 CXO_MARGIN = 0x8000;      // the margin is an area outside the input area where events will be mapped to the edge of the input area
        public const Int32 CXO_MGNINSIDE = 0x4000;   // the margin will be inside the specified context
        public const Int32 CXO_CSRMESSAGES = 0x0008; // 1.1 sends WT_CSRCHANGE messages
        
        // context status values
        public const Int32 CXS_DISABLED = 0x0001;
        public const Int32 CXS_OBSCURED = 0x0002;
        public const Int32 CXS_ONTOP = 0x0004;
        
        // context lock values
        public const Int32 CXL_INSIZE = 0x0001;      // the context's input size cannot be changed
        public const Int32 CXL_INASPECT = 0x0002;    // the context's input aspect ratio cannot be changed
        public const Int32 CXL_SENSITIVITY = 0x0004; // the context's sensitivity settings for x, y, and z cannot be changed
        public const Int32 CXL_MARGIN = 0x0008;      // the context's margin options cannot be changed
        public const Int32 CXL_SYSOUT = 0x0010;
        
        // Event data defs
        
        // packet status values
        public const Int32 TPS_PROXIMITY = 0x0001;
        public const Int32 TPS_QUEUE_ERR = 0x0002;
        public const Int32 TPS_MARGIN = 0x0004;
        public const Int32 TPS_GRAB = 0x0008;
        public const Int32 TPS_INVERT = 0x0010;
        
        // relative buttons
        public const Int32 TBN_NONE = 0;
        public const Int32 TBN_UP = 1;
        public const Int32 TBN_DOWN = 2;
        
        // device config constants
        public const Int32 WTDC_NONE = 0;
        public const Int32 WTDC_CANCEL = 1;
        public const Int32 WTDC_OK = 2;
        public const Int32 WTDC_RESTART = 3;
        
        // functions

		// Used to read various pieces of information about the tablet.
        [DllImport("WinTab32", EntryPoint="WTInfoA")]
        public static extern UInt32 WTInfo(UInt32 wCategory, UInt32 nIndex, [Out] out LOGCONTEXT lpLogCtx);
		[DllImport("WinTab32", EntryPoint="WTInfoA")]
		public static extern UInt32 WTInfo(UInt32 wCategory, UInt32 nIndex, [Out] out AXIS lpAxis);
        
        // Used to begin accessing the Tablet.
        [DllImport("WinTab32", EntryPoint="WTOpenA")]
  		public static extern Int32 WTOpen(Int32 hWnd, ref LOGCONTEXT lpLogCtx, Boolean fEnable);
        
  		// Used to end accessing the Tablet.
        [DllImport("WinTab32", EntryPoint="WTOpenA")]
  		public static extern Boolean WTClose(Int32 hCtx);
  		
  		// Used to poll the Tablet for input.
  		[DllImport("WinTab32", EntryPoint="WTPacketsGet")]
  		public static extern Int32 WTPacketsGet(Int32 hCtx, Int32 cMaxPkts, [Out, MarshalAs(UnmanagedType.LPArray)] PACKET[] lpPkts);
  		
  		// Similar to WTPacketsGet but is used in a window function.
  		[DllImport("WinTab32", EntryPoint="WTPacket")]
  		public static extern Boolean WTPacket(Int32 hCtx, UInt32 wSerial, [Out] out PACKET lpPkt);

        // Enables and Disables a Tablet Context, temporarily turning on or off the processing of packets.
   		[DllImport("WinTab32", EntryPoint="WTEnable")]
  		public static extern Boolean WTEnable(Int32 hCtx, Boolean fEnable);
  		
  		// Sends a tablet context to the top or bottom of the order of overlapping tablet contexts.
  		[DllImport("WinTab32", EntryPoint="WTOverlap")]
  		public static extern Boolean WTOverlap(Int32 hCtx, Boolean fToTop);

    } 
    
}
