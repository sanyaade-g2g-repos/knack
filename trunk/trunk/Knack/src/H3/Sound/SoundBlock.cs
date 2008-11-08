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
using System.ComponentModel;

using H3.Sound.Wave.SoundRender;
using H3.Sound.Midi.MidiRender;

namespace H3.Sound
{
	public class SoundBlock
	{
		IMidiRender[] midiInputs;
		ISoundRender[] soundInputs;
		System.Drawing.Image icon;
		protected bool editable = false;
		string name;
		static int id = 0;
	
		public SoundBlock() : this(0,0)
		{ }
		
		public SoundBlock(int numSoundInputs) : this(numSoundInputs,0) 
		{ }
		
		public SoundBlock(int numSoundInputs, int numMidiInputs) 
		{
			if (!(this is NullSoundRender)) {
				string[] temp = this.GetType().ToString().Split('.');
				string classname = temp[temp.Length-1].Replace("SoundRender","").Replace("Render","");
				name = classname + id.ToString(); id++;
			}
			soundInputs = new ISoundRender[numSoundInputs];
			for(int i=0; i<numSoundInputs; i++) soundInputs[i] = new NullSoundRender();
			midiInputs = new IMidiRender[numMidiInputs];
			for(int i=0; i<numMidiInputs; i++) midiInputs[i] = new NullMidiRender();
		}
		
		public SoundBlock(ISoundRender soundRender) : this(1,0) 
		{
			soundInputs[0] = soundRender;
		}
		
		public SoundBlock(ISoundRender soundRenderA, ISoundRender soundRenderB) : this(2,0)
		{
			soundInputs[0] = soundRenderA;
			soundInputs[1] = soundRenderB;
		}
		
		public SoundBlock(ISoundRender soundRender, int numMidiInputs) : this(1,numMidiInputs)
		{
			soundInputs[0] = soundRender;
		}
		
		[BrowsableAttribute(false), CategoryAttribute("Inputs"), DescriptionAttribute("Sound inputs.")]
		public ISoundRender[] SoundInputs {
			get { return soundInputs; }
			set { soundInputs = value; }
		}
		
		[BrowsableAttribute(false), CategoryAttribute("Inputs"), DescriptionAttribute("Midi inputs.")]
		public IMidiRender[] MidiInputs {
			get { return midiInputs; }
			set { midiInputs = value; }
		}
		
		[BrowsableAttribute(false), CategoryAttribute("Aspect"), DescriptionAttribute("Sound Block Icon.")]
		public System.Drawing.Image Icon {
			get { 
				if(icon == null) icon = LoadIcon(); 
				return icon; 
			}
		}
		
		[BrowsableAttribute(false), CategoryAttribute("Status"), DescriptionAttribute("Tells if the soundrender has an OnEdit")]
		public virtual bool Editable {
			get { return editable; }
		}
		
		protected virtual System.Drawing.Image LoadIcon()
		{ return null; }
		
		[CategoryAttribute("Sound Block"), DescriptionAttribute("Sound block name.")]
		public string Name {
			get { return name; }
			set { name = value; }
		}
		
		public virtual void OnEdit() 
		{ }
	}
	
}
