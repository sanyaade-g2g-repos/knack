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
using Microsoft.Ink;
using Microsoft.StylusInput;

namespace WinTablet
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
	    int x,y,normalPressure;
	    TabletSettings tabletSettings;
	
	    public TabletPacketEventArgs(int x, int y, int normalPressure, TabletSettings tabletSettings)
	    {
	    	this.x = x;
	    	this.y = y;
	    	this.normalPressure = normalPressure;
	        this.tabletSettings = tabletSettings;
	    }
	    
	    public float RelativeX {
	    	get { return (float)(this.x) / (float)(this.tabletSettings.MaxX); }
	    }
	    
	    public float RelativeY {
	    	get {  return (float)(this.y) / (float)(this.tabletSettings.MaxY); }
	    }
	
	    public float AbsoluteX {
	    	get { 
	    		return RelativeX; // this is wrong but it's ok when the window is maximized...
	    	}
	    }
	    
	    public float AbsoluteY {
	    	get { 
	    		return RelativeY; // this is wrong but it's ok when the window is maximized...
	    	}
	    }
	    
	    public float ScreenX {
	    	get { return AbsoluteX * (float)SystemInformation.PrimaryMonitorSize.Width; }
	    }
	    
	    public float ScreenY {
	    	get { return AbsoluteY * (float)SystemInformation.PrimaryMonitorSize.Height; }
	    }
	    
	    public float NormalPressure {
	    	get { return (float)(this.normalPressure) / (float)(this.tabletSettings.MaxNPressure); }
	    }
	    
	    public System.Drawing.PointF GetRelativePosition(Control c) {
	    	System.Drawing.PointF relPointF = new System.Drawing.PointF(RelativeX, RelativeY);
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
		private RealTimeStylus rts;
		private TabletSettings tabletSettings = new TabletSettings();
		public event TabletPacketEventHandler TabletPacket;
		
		public TabletForm() : base()
		{
			this.Closed += new System.EventHandler(TabletFormClosed);
			
			Tablets tablets = new Tablets();
			Console.WriteLine("[" + tablets.Count + "] pointing devices found");
			Tablet tablet = tablets.DefaultTablet;
			Console.WriteLine("Default tablet name [" + tablet.Name + "]");
			Console.WriteLine("Hardware capabilities [" + tablet.HardwareCapabilities + "]");
			tabletSettings.MaxX = tablet.GetPropertyMetrics(PacketProperty.X).Maximum;
			tabletSettings.MaxY = tablet.GetPropertyMetrics(PacketProperty.Y).Maximum;
			tabletSettings.MaxNPressure = tablet.GetPropertyMetrics(PacketProperty.NormalPressure).Maximum;
			
			rts = new RealTimeStylus(this.Handle, tablet);
			rts.AsyncPluginCollection.Add(new TabletAsyncPlugin(this));
			rts.Enabled = true;
			
			//TODO call dispose on dispose
			  		
			Log("Tablet initialized");
		}
		
		protected virtual void Log(string logline) {
			Console.Error.WriteLine(logline);
		}
		
		private void EnqueuePacket(int x, int y, int normalPressure) {
			//Log("Packet: x = "+x+" y = "+y+" normalPressure = "+normalPressure);
			TabletPacketEventArgs evArgs = new TabletPacketEventArgs(x, y, normalPressure, tabletSettings);
			OnTabletPacket(evArgs);
		}
		
		public virtual void OnTabletPacket(TabletPacketEventArgs e) 
		{
			if (TabletPacket != null) {
		       TabletPacket(this, e); 
		    }
		}
		
		void TabletFormClosed(object sender, System.EventArgs e)
		{
			// dispose driver!
		}
	}
}
