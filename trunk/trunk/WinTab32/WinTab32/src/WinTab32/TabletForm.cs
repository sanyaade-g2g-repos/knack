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
 */

using System;
using System.Windows.Forms;
using System.Threading;

namespace WinTab32
{
	public class TabletSettings
	{
		public int Handle = 0;
		public int MaxNPressure = 0;
	    public int MaxX = 0;
	    public int MaxY = 0;
	    public int MaxZ = 0;
	}
	
	public class TabletPacketEventArgs : EventArgs
	{
	    PACKET packet;
	    TabletSettings tabletSettings;
	
	    public TabletPacketEventArgs(PACKET packet, TabletSettings tabletSettings)
	    {
	        this.packet = packet;
	        this.tabletSettings = tabletSettings;
	    }
	
	    public float AbsoluteX {
	    	get { 
	    		return (float)(this.packet.pkX) 
	    			/ (float)(this.tabletSettings.MaxX);
	    	}
	    }
	    
	    public float AbsoluteY {
	    	get { 
	    		return 1.0f - ((float)(this.packet.pkY) 
	    			/ (float)(this.tabletSettings.MaxY));
	    	}
	    }
	    
	    public float ScreenX {
	    	get { return AbsoluteX * (float)SystemInformation.PrimaryMonitorSize.Width; }
	    }
	    
	    public float ScreenY {
	    	get { return AbsoluteY * (float)SystemInformation.PrimaryMonitorSize.Height; }
	    }
	    
	    public float NormalPressure {
	    	get { 
	    		return (float)(this.packet.pkNormalPressure) 
	    			/ (float)(this.tabletSettings.MaxNPressure);
	    	}
	    }
	    
	    public System.Drawing.PointF GetRelativePosition(Control c) {
	    	System.Drawing.Point relPoint;
	    	System.Drawing.PointF relPointF;
	    	relPoint = c.PointToClient(new System.Drawing.Point((int)ScreenX,(int)ScreenY));
	    	relPointF = new System.Drawing.PointF(relPoint.X + ScreenX - (float)((int)ScreenX),
	    	                                      relPoint.Y + ScreenY - (float)((int)ScreenY));
	    	return relPointF;
	    }
	}
	
	public delegate void TabletPacketEventHandler( object sender, TabletPacketEventArgs e);
	
	/// <summary>
	/// A Form with tablet support.
	/// Inherit your Form from TabletForm and add an handler for the TabletPacket event.
	/// </summary>
	public class TabletForm : System.Windows.Forms.Form
	{
		bool useMessages = true;
		TabletSettings tabletSettings = new TabletSettings();
		Thread tabletThread = null;
		
		public event TabletPacketEventHandler TabletPacket;
		
		public TabletForm() : base()
		{
			try {
				this.Closed += new System.EventHandler(TabletFormClosed);
				this.Activated += new System.EventHandler(TabletFormActivated);
				this.Deactivate += new System.EventHandler(TabletFormDeactivate);
				
				LOGCONTEXT lc = new LOGCONTEXT();
				AXIS npAxis = new AXIS();
				bool boolResult;
				uint uintResult;
				
				/* get default settings */
				uintResult = WinTab.WTInfo(WinTab.WTI_DEFSYSCTX/*WinTab.WTI_DEFCONTEXT*/, 0, out lc);
				Log("WTInfo result = "+uintResult);
			
				/* modify settings */
				lc.lcName = "TabletForm" + this.Handle.ToString();
				lc.lcOptions |= WinTab.CXO_SYSTEM | WinTab.CXO_MESSAGES;
				if (useMessages) lc.lcOptions |= WinTab.CXO_MESSAGES;
				lc.lcMsgBase = WinTab.WT_DEFBASE;
				lc.lcPktData = WinTab.PACKETDATA;
				lc.lcPktMode = WinTab.PACKETMODE;
				lc.lcMoveMask = WinTab.PACKETDATA;
				lc.lcBtnUpMask = lc.lcBtnDnMask;
			
				/* modify output size */
				lc.lcOutOrgX = lc.lcOutOrgY = 0;
				tabletSettings.MaxX = lc.lcOutExtX = lc.lcInExtX - lc.lcInOrgX;
				tabletSettings.MaxY = lc.lcOutExtY = lc.lcInExtY - lc.lcInOrgY;
				
				/* update settings and open context */
				tabletSettings.Handle = WinTab.WTOpen(this.Handle.ToInt32(), ref lc, true);
			  			
				if (tabletSettings.Handle == 0) {
					Log("Can't init Tablet (WTOpen handle = 0)");
					throw new TabletNotFoundException("A tablet is installed but not connected");
				}
	  		    else {
	  		    	Log("Tablet initialized (WTOpen handle = "+tabletSettings.Handle+")");
	  		    	Log("lcInExtX = "+lc.lcInExtX+" lcInExtY = "+lc.lcOutExtY);
	  		    	Log("lcOutExtX = "+lc.lcOutExtX+" lcOutExtY = "+lc.lcOutExtY);
	  		    }
	  		    
	  		    uintResult = WinTab.WTInfo(WinTab.WTI_DEVICES + lc.lcDevice, WinTab.DVC_NPRESSURE, out npAxis);
	  		    Log("WTInfo result = "+uintResult);
	  		    
	  		    tabletSettings.MaxNPressure = npAxis.axMax;
	  		    Log("Tablet MaxNormalPressure = "+tabletSettings.MaxNPressure);
	
	  		    boolResult = WinTab.WTEnable(tabletSettings.Handle, true);
	  		    if (boolResult) Log("Tablet enabled");
	  		    else Log("Can't enable tablet");
	  		    
	  		    if(!useMessages) {
	  		    	tabletThread = new Thread(new ThreadStart(TabletThread));
	  		    	tabletThread.IsBackground = true;
	  		    	tabletThread.Name = "TabletThread";
	  		    	tabletThread.Priority = ThreadPriority.AboveNormal;
	  		    	tabletThread.Start();
	  		    	Log("Tablet thread started");
	  		    }
			} catch (System.DllNotFoundException ex) {
				throw new TabletNotInstalledException("No tablet installed : " + ex.Message);
			}
		}
		
		protected virtual void Log(string logline) {
			Console.Error.WriteLine(logline);
		}
		
		private void EnqueuePacket(PACKET packet) {
			//Log("Packet: pkX = "+p.pkX+" pkY = "+p.pkY+" pkNormalPressure = "+p.pkNormalPressure);
			TabletPacketEventArgs evArgs = 
				new TabletPacketEventArgs(packet, tabletSettings);
        
			OnTabletPacket(evArgs);
		}
		
		protected virtual void OnTabletPacket(TabletPacketEventArgs e) 
		{
			if (TabletPacket != null) {
		       TabletPacket(this, e); 
		    }
		}
		
		protected override void WndProc(ref Message m)
		{			
			switch (m.Msg) {
					
				case WinTab.WT_PROXIMITY:
					ushort lowWord = (ushort) (m.LParam.ToInt32() & 0xFFFF);
					ushort hiWord = (ushort) (m.LParam.ToInt32() >> 16);
					string logLine = "Proximity: handle = " + m.WParam.ToString();
					if (lowWord != 0) logLine += " Entering";
						else logLine += " Leaving";
					if (hiWord != 0) logLine += " Hardware";
						else logLine += " Context";
					Log(logLine);
					//base.WndProc(ref m);
					break;
					
				case WinTab.WT_PACKET:
					PACKET p = new PACKET();
					WinTab.WTPacket(m.LParam.ToInt32(),(UInt32)m.WParam.ToInt32(),out p);
					EnqueuePacket(p);
					break;
					
				default:
					base.WndProc(ref m);
					break;
					
			}
		}
		
		private void TabletThread() 
		{	
			PACKET[] Packets = new PACKET[16];
			Int32 numPackets = 0;
			
			while(tabletSettings.Handle != 0) {
				try {
					numPackets = WinTab.WTPacketsGet(tabletSettings.Handle,32,Packets);
				} catch (Exception e) {
					Log(e.ToString());
				}
				//if (numPackets>0) Log("Received "+numPackets+" packets");
				for(int i=0; i<numPackets; i++) {
					EnqueuePacket(Packets[i]);
				}
				
				if (numPackets == 0) Thread.Sleep(1);
			}
		}
		
		void TabletFormClosed(object sender, System.EventArgs e)
		{
			if (tabletSettings.Handle != 0) {
				WinTab.WTClose(tabletSettings.Handle);
				Log("Tablet closed");
				tabletSettings.Handle = 0;
			}
		}
		
		void TabletFormActivated(object sender, System.EventArgs e)
		{
			if (tabletSettings.Handle != 0) {
				bool boolResult = WinTab.WTOverlap(tabletSettings.Handle, true);
	      		if (boolResult) Log("Tablet overlap set to true");
	  		    else Log("Can't set tablet overlap to true");
			}
		}
		
		void TabletFormDeactivate(object sender, System.EventArgs e)
		{
			if (tabletSettings.Handle != 0) {
				bool boolResult = WinTab.WTOverlap(tabletSettings.Handle, false);
	      		if (boolResult) Log("Tablet overlap set to false");
	  		    else Log("Can't set tablet overlap to false");
			}
		}
	}
}
