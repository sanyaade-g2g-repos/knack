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

namespace H3.Sound.Wave.SoundRender
{
	public abstract class OperatorSoundRender : SoundRender
	{	
		float[] leftSoundBuffer;
		float[] rightSoundBuffer;
		
		protected OperatorSoundRender() : base(2)
		{ }
		
		protected OperatorSoundRender(ISoundRender soundRenderA,ISoundRender soundRenderB) : base(soundRenderA,soundRenderB)
		{ }
		
		public override bool Playing {
			get { return SoundInputs[0].Playing || SoundInputs[1].Playing; }
		}
		
		public abstract void RenderOperator(float[] output, float[] inputA, float[] inputB);
		
		public override void Render(float[] leftChannel,float[] rightChannel) 
		{
			int channelSize = leftChannel.Length;
			
			if ((leftSoundBuffer == null) || (leftSoundBuffer.Length<channelSize)) {
				leftSoundBuffer = new float[channelSize];
				rightSoundBuffer = new float[channelSize];
			}
		
			SoundInputs[0].Render(leftChannel,rightChannel);
			SoundInputs[1].Render(leftSoundBuffer,rightSoundBuffer);
			
			RenderOperator(leftChannel,leftChannel,leftSoundBuffer);
			RenderOperator(rightChannel,rightChannel,rightSoundBuffer);
		}
		
		public override void Stop() 
		{
			SoundInputs[0].Stop();
			SoundInputs[1].Stop();
		}
	}
}
