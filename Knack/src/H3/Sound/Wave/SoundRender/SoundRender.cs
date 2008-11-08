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

using H3.Sound.Midi;

namespace H3.Sound.Wave.SoundRender
{
	public abstract class SoundRender : SoundBlock, ISoundRender//, ICloneable
	{
		protected bool playing = true;
		protected bool dynamic = false;
		
		protected SoundRender() : base()
		{ }
		
		protected SoundRender(int numSoundInputs) : base(numSoundInputs)
		{ }
		
		protected SoundRender(int numSoundInputs,int numMidiInputs) : base(numSoundInputs,numMidiInputs)
		{ }
		
		protected SoundRender(ISoundRender soundRender) : base(soundRender)
		{ }
		
		protected SoundRender(ISoundRender soundRenderA,ISoundRender soundRenderB) : base(soundRenderA,soundRenderB)
		{ }
		
		protected SoundRender(ISoundRender soundRender,int numMidiInputs) : base(soundRender,numMidiInputs)
		{ }
		
		[BrowsableAttribute(false), CategoryAttribute("Status"), DescriptionAttribute("Tells if the sound is playing")]
		public virtual bool Playing {
			get { return playing; }
		}
		
		public virtual object Clone(MidiMessage msg)
		{
			// SoundRender sr = (SoundRender) this.MemberwiseClone(); // DOES NOT WORK because of shared references
			Type type = this.GetType();
			SoundRender sr = (SoundRender) type.GetConstructor(new Type[0]).Invoke(new object[0]);
			for(int i=0; i<sr.SoundInputs.Length; i++) {
				sr.SoundInputs[i] = (ISoundRender) SoundInputs[i].Clone(msg);
			}
			return sr;
		}
		
		public abstract void Render(float[] leftChannel,float[] rightChannel);
		
		public virtual void Stop()
		{
			playing = false;
		}
	}
}
