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

using H3.Sound.Wave;

namespace H3.Sound.Wave.SoundRender
{
	
	public class SoundOutRender : SoundRender, IDisposable
	{
		
		private class SoundOutFromSoundRender : SoundOut
		{
			NullSoundRender nullSoundRender;
			SoundOutRender soundOutRender;
			
			public SoundOutFromSoundRender(SoundOutRender soundOutRender) : base()
			{
				this.soundOutRender = soundOutRender;
			}

			public override void Render(float[] floatLeftChannel,float[] floatRightChannel)
			{
				if ((soundOutRender == null) || (soundOutRender.SoundInputs[0] == null)) {
					if (nullSoundRender == null) nullSoundRender = new NullSoundRender();
					nullSoundRender.Render(floatLeftChannel,floatRightChannel);
				}
				else
					soundOutRender.SoundInputs[0].Render(floatLeftChannel,floatRightChannel);
			}
		}
		
		SoundOutFromSoundRender soundOut;
		
		public SoundOutRender() : base(1,0)
		{ 
			Initialize();
		}
		
		public SoundOutRender(ISoundRender soundRender) : base(soundRender)
		{ 
			Initialize();
		}
		
		private void Initialize()
		{
			soundOut = new SoundOutFromSoundRender(this);
			soundOut.Start();
		}
		
		protected override System.Drawing.Image LoadIcon()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager("H3.Knack.SoundBlock",
                                 Assembly.GetExecutingAssembly());
			return (System.Drawing.Image) resources.GetObject("sound-out");
		}
		
		public override void Render(float[] leftChannel,float[] rightChannel) 
		{ }
		
		public override void Stop()
		{
			base.Stop();
			soundOut.Stop();
		}
		
		public void Dispose()
		{
			soundOut.Dispose();
		}
		
	}
}
