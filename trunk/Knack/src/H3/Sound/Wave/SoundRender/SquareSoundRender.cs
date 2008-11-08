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

namespace H3.Sound.Wave.SoundRender
{
	public class SquareSoundRender : WaveSoundRender
	{
		void Initialize() {
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager("H3.Knack.SoundBlock",
                                 Assembly.GetExecutingAssembly());
			icon = ((System.Drawing.Image)(resources.GetObject("square")));
		}
		
		public SquareSoundRender() : base()
		{ Initialize(); }
		
		public SquareSoundRender(double freq) : base(freq)
		{ Initialize(); }
		
		protected override System.Drawing.Image LoadIcon() {
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager("H3.Knack.SoundBlock",
                                 Assembly.GetExecutingAssembly());
			return (System.Drawing.Image) resources.GetObject("square");
		}
		
		double oldFrequency;
		
		public override void Render(float[] leftChannel,float[] rightChannel)
		{
			int channelSize = leftChannel.Length;
			double x, frequency = this.FrequencyToScaledFrequency(freq);
			if (frequency <= 0) frequency = double.Epsilon;
			
			if (frequency != oldFrequency) {
				pos *= oldFrequency / frequency;
				oldFrequency = frequency;
			}
			
			for (int i=0; i<channelSize; i++){  
				x = pos * frequency;
				rightChannel[i] = 
					leftChannel[i] = 
						(x % (Math.PI * 2.0) < Math.PI) ? 1.0f : -1.0f;
				pos += posIncrement;
			}
		}
	}
}
