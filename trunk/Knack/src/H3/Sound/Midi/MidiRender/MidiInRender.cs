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
using System.Reflection;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace H3.Sound.Midi.MidiRender
{
	
	public class MidiInRender : SoundBlock, IMidiRender, IDisposable
	{
		/// <summary>
		/// A minimalist MIDI-IN class for Windows
		/// </summary>
		private class MidiIn : IMidiRender, IDisposable
		{
			[DllImport("winmm.dll")]
	        static extern int midiInOpen(ref int handle, int deviceId, MidiInProc proc, int instance, int flags);
	
	        [DllImport("winmm.dll")]
	        static extern int midiInClose(int handle);
			
			[DllImport("winmm.dll")]
	        static extern int midiInStart(int handle);
			
			[DllImport("winmm.dll")]
	        static extern int midiInStop(int handle);
	        
	        [DllImport("winmm.dll")]
	        static extern uint midiInGetNumDevs();
			
			delegate void MidiInProc(int handle, int msg, int instance, int param1, int param2); 
			
			MidiInProc messageHandler = null;
			
			const int CALLBACK_FUNCTION = 0x30000; 
			const int MIM_DATA = 0x3C3;
	        const int MIM_ERROR = 0x3C5;
	        const int MIM_LONGDATA = 0x3C4;
			
			int handle = 0;
			int deviceId = 0;
			bool started = false;
			
			public event MidiMessageHandler OnMidiMessage;
			
			public MidiIn() 
			{
				messageHandler = new MidiInProc(OnMessage);    
			}
			
			public int DeviceID {
				get { return deviceId; }
				set { 
					if ((value < midiInGetNumDevs()) && (value >= 0)) {
						deviceId = value;
						if (started) Start();
					}
				}
			}
			
			public void Start() 
			{
				if (started) Stop(); //se già avviato riavvia
				midiInOpen(ref handle, deviceId, messageHandler, 0, CALLBACK_FUNCTION);
				midiInStart(handle);
				System.Console.WriteLine("MidiIn started");
				started = true;
			}
			
			public void Stop()
			{
				if (started) {
					midiInStop(handle);
					midiInClose(handle);
					System.Console.WriteLine("MidiIn stopped");
				}
				started = false;
			}
			
			private void OnMessage(int handle, int msg, int instance, int param1, int param2)
	        {
	            if(msg == MIM_DATA || msg == MIM_ERROR || msg == MIM_LONGDATA)
	            {
	            	MidiMessage message = new MidiMessage(handle,msg,instance,param1,param2);
	            	Console.WriteLine(message.ToString());
	                OnMidiMessage(message);
	            }  
	        }
			
			public void Dispose()
			{
				Stop();
			}
			
			[BrowsableAttribute(false), CategoryAttribute("Status"), DescriptionAttribute("Tells if the soundrender has an OnEdit")]
			public virtual bool Editable {
				get { return false; }
			}
			
			public virtual void OnEdit() 
			{ }
		}
		
		public event MidiMessageHandler OnMidiMessage;
	
		MidiIn midiIn;
		
		public MidiInRender()
		{
			midiIn = new MidiIn();
			midiIn.OnMidiMessage += new MidiMessageHandler(OnMidiMessageHandler);
			midiIn.Start();
		}
		
		~MidiInRender()
		{
			midiIn.Stop();
		}
		
		[CategoryAttribute("MIDI"), DescriptionAttribute("MIDI input device ID.")]
		public int DeviceID {
			get { return midiIn.DeviceID; }
			set { midiIn.DeviceID = value; }
		}
		
		protected override System.Drawing.Image LoadIcon() {
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager("H3.Knack.SoundBlock",
                                 Assembly.GetExecutingAssembly());
			return ((System.Drawing.Image)(resources.GetObject("midi-in")));
		}
		
		private void OnMidiMessageHandler(MidiMessage msg) 
		{
			OnMidiMessage(msg);
		}
		
		public void Dispose()
		{
			midiIn.Dispose();
		}
	}
}
