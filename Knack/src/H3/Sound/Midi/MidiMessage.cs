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

namespace H3.Sound.Midi
{
	public class MidiMessage 
	{
		int handle,msg,instance,param1,param2;
		double frequency = -1;
	
		public MidiMessage() 
		{ 
			Note = 0;
			Velocity = 127;
		}
	
		public MidiMessage(int handle, int msg, int instance, int param1, int param2)
		{
			this.handle = handle;
			this.msg = msg;
			this.instance = instance;
			this.param1 = param1;
			this.param2 = param2;
			//Logger.Instance.Log(this.ToString());
		}
		
		public MidiMessage(MidiMessage messageToCopy)
		{
			this.handle = messageToCopy.handle;
			this.msg = messageToCopy.msg;
			this.instance = messageToCopy.instance;
			this.param1 = messageToCopy.param1;
			this.param2 = messageToCopy.param2;
			//Logger.Instance.Log(this.ToString());
		}
		
		public int Handle {
			get { return this.handle; }
		}
		public int Msg {
			get { return this.msg; }
		}
		public int Instance {
			get { return this.instance; }
		}
		public int Param1 {
			get { return this.param1; }
		}
		public int Param2 {
			get { return this.param2; }
		}
		
		public int Status {
			get { return (param1) & 0xFF; }
			set { param1 = (int)(param1 & 0xFFFFFF00) | (int)(value); }
		}
		public int Note {
			get { return (param1>>8) & 0xFF; }
			set { param1 = (int)(param1 & 0xFFFF00FF) | (int)(value<<8); }
		}
		public int Velocity {
			get { return (param1>>16) & 0xFF; }
			set { param1 = (int)(param1 & 0xFF00FFFF) | (int)(value<<16); }
		}
		public double Frequency {
			get { if (frequency == -1)
					frequency = NoteToFrequency(Note);
				  return frequency; }
		}	
		
		public static double NoteToFrequency(double note)
		{
			//NOTE: the result can be negative! and it's correct!
			return (440.0 / 32.0) * Math.Pow(2,(note - 9.0) / 12.0); 
		}
		
		public static double FrequencyToNote(double freq)
		{
			if (freq<=0) return 0.0;
			return Math.Log(freq * 32.0 / 440.0, 2) * 12.0 + 9.0;
		}
		
		public override string ToString() {
			return "*MidiMessage* Status:"+Status+" Note: "+Note+" Frequency: "+Frequency+" Velocity: "+Velocity;
		}
	}
}
