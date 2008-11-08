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
using Microsoft.Ink;
using Microsoft.StylusInput;

namespace WinTablet
{
	/// <summary>
	/// Description of Plugin.
	/// </summary>
	public class TabletAsyncPlugin : IStylusAsyncPlugin
	{
		private TabletForm tabletForm;
		private TabletSettings tabletSettings;
		
		public TabletAsyncPlugin(TabletForm tabletForm)
		{
			this.tabletForm = tabletForm;
			tabletSettings = new TabletSettings();
			tabletSettings.MaxX = 37800;
			tabletSettings.MaxY = 22500;
			tabletSettings.MaxNPressure=511;
		}
		
		DataInterestMask IStylusAsyncPlugin.DataInterest {
			get { return DataInterestMask.Packets | DataInterestMask.StylusDown | DataInterestMask.StylusUp; }
		}
		
		void IStylusAsyncPlugin.RealTimeStylusEnabled(RealTimeStylus sender, Microsoft.StylusInput.PluginData.RealTimeStylusEnabledData data) { }
		void IStylusAsyncPlugin.RealTimeStylusDisabled(RealTimeStylus sender, Microsoft.StylusInput.PluginData.RealTimeStylusDisabledData data) { }
		void IStylusAsyncPlugin.StylusInRange(RealTimeStylus sender, Microsoft.StylusInput.PluginData.StylusInRangeData data) { }
		void IStylusAsyncPlugin.StylusOutOfRange(RealTimeStylus sender, Microsoft.StylusInput.PluginData.StylusOutOfRangeData data) { }
		void IStylusAsyncPlugin.StylusDown(RealTimeStylus sender, Microsoft.StylusInput.PluginData.StylusDownData data) { }
		void IStylusAsyncPlugin.StylusUp(RealTimeStylus sender, Microsoft.StylusInput.PluginData.StylusUpData data) { 
			tabletForm.OnTabletPacket(new TabletPacketEventArgs(0, 0, 0, tabletSettings));
		}
		void IStylusAsyncPlugin.StylusButtonDown(RealTimeStylus sender, Microsoft.StylusInput.PluginData.StylusButtonDownData data) { }
		void IStylusAsyncPlugin.StylusButtonUp(RealTimeStylus sender, Microsoft.StylusInput.PluginData.StylusButtonUpData data) { }
		void IStylusAsyncPlugin.InAirPackets(RealTimeStylus sender, Microsoft.StylusInput.PluginData.InAirPacketsData data) { }
		void IStylusAsyncPlugin.Packets(RealTimeStylus sender, Microsoft.StylusInput.PluginData.PacketsData data)
		{
			int[] intData = data.GetData();
			tabletForm.OnTabletPacket(new TabletPacketEventArgs(intData[0], intData[1], intData[2], tabletSettings));
		}
		void IStylusAsyncPlugin.SystemGesture(RealTimeStylus sender, Microsoft.StylusInput.PluginData.SystemGestureData data) { }
		void IStylusAsyncPlugin.TabletAdded(RealTimeStylus sender, Microsoft.StylusInput.PluginData.TabletAddedData data) { }
		void IStylusAsyncPlugin.TabletRemoved(RealTimeStylus sender, Microsoft.StylusInput.PluginData.TabletRemovedData data) { }
		void IStylusAsyncPlugin.CustomStylusDataAdded(RealTimeStylus sender, Microsoft.StylusInput.PluginData.CustomStylusData data) { }
		void IStylusAsyncPlugin.Error(RealTimeStylus sender, Microsoft.StylusInput.PluginData.ErrorData data) { }
	}
}
