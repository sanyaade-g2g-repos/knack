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

using System.Reflection;
using System.ComponentModel;

namespace H3.Sound.Midi.MidiRender
{
	public class MidiFilterRender : SoundBlock, IMidiHandler, IMidiRender
	{	
		int noteMin = 50;
		int noteMax = 120;
		float velocityMin = 0.01f;
		float velocityMax = 1.0f;
		
		MidiMessageHandler midiMessageHandler;
		public event MidiMessageHandler OnMidiMessage;
		
		[BrowsableAttribute(false)]
		public MidiMessageHandler MidiMessageHandler 
		{ 
			get { 
				if (midiMessageHandler == null)
					midiMessageHandler = new MidiMessageHandler(OnMidiMessageReceived);
				return midiMessageHandler; 
			}
		}
		[CategoryAttribute("Midi"), DescriptionAttribute("Lowest possible index for a note.")]
		public int NoteMin {
			get{ return noteMin; }
			set{ 
				noteMin = value;
				if (noteMin<0) noteMin = 0;
				if (noteMin>noteMax) noteMin = noteMax;
			}
		}
		[CategoryAttribute("Midi"), DescriptionAttribute("Highest possible index for a note.")]
		public int NoteMax {
			get{ return noteMax; }
			set{ 
				noteMax = value;
				if (noteMax<noteMin) noteMax = noteMin;
			}
		}
		[CategoryAttribute("Midi"), DescriptionAttribute("Lowest possible velocity of a note (in range 0..1).")]
		public float VelocityMin {
			get{ return velocityMin; }
			set{ 
				velocityMin = value;
				if (velocityMin<0.0001f) velocityMin = 0.0001f;
				if (velocityMin>velocityMax) velocityMin = velocityMax;
			}
		}
		[CategoryAttribute("Midi"), DescriptionAttribute("Highest possible velocity of a note (in range 0..1).")]
		public float VelocityMax {
			get{ return velocityMax; }
			set{ 
				velocityMax = value;
				if (velocityMax<velocityMin) velocityMax = velocityMin;
				if (velocityMax>1.0f) velocityMax = 1.0f;
			}
		}

		public MidiFilterRender() : base(0,1)
		{
			OnMidiMessage += new MidiMessageHandler(NullMidiMessageHandler);
		}
		
		protected override System.Drawing.Image LoadIcon() {
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager("H3.Knack.SoundBlock",
                                 Assembly.GetExecutingAssembly());
			return (System.Drawing.Image) resources.GetObject("midi-filter");
		}
		
		public void OnMidiMessageReceived(MidiMessage msg) {
			if (msg.Velocity == 0) this.OnMidiMessage(msg);
			else if ((msg.Note>=this.noteMin) && (msg.Note<=this.noteMax)) {
				MidiMessage msgCopy = new MidiMessage(msg);
				if (msgCopy.Velocity<this.velocityMin)
					msgCopy.Velocity = (int) (this.velocityMin * 127.0f);
				if (msgCopy.Velocity>this.velocityMax) 
					msgCopy.Velocity = (int) (this.velocityMax * 127.0f);
				this.OnMidiMessage(msgCopy);
			}
		}
		
		private void NullMidiMessageHandler(MidiMessage msg) 
		{ }
	}
}
