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

using H3.Sound.Midi;

namespace H3.Sound.Wave.SoundRender
{
	
	public class NullSoundRender : SoundRender
	{
		public override void Render(float[] leftChannel,float[] rightChannel)
		{
			int i;
			
			for (i=0; i<leftChannel.Length; i++) {
				leftChannel[i] = 0.0f;
			}
			for (i=0; i<rightChannel.Length; i++) {
				rightChannel[i] = 0.0f;
			}
		}
		
		public override object Clone(MidiMessage msg)
		{
			return this; // No need to clone a NullSoundRender, we can use the same instance.
		}
	}
}
