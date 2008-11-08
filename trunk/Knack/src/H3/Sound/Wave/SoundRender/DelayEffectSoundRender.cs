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
using System.ComponentModel;

using H3.Utils;

namespace H3.Sound.Wave.SoundRender
{
	public class DelayEffectSoundRender : EffectSoundRender
	{
		int samplesPerSecond = 44100;
		int bufferSize;
		float [] leftSoundBuffer;
		float [] rightSoundBuffer;
		float[] leftPeriodBuffer;
		float[] rightPeriodBuffer;
		int writePos = 0;
		int period;
		float dry = 1.0f;
		float wet = 0.3f;
		float pan = 0.4f;
		float feedback = 0.5f;
		
		[BrowsableAttribute(false), CategoryAttribute("Status"), DescriptionAttribute("Size of the sound buffer")]
		public int BufferSize {
			get { return bufferSize; }
			set { 
				bufferSize = value;
				leftSoundBuffer = new float[bufferSize];
				rightSoundBuffer = new float[bufferSize];
			}
		}
		[CategoryAttribute("Effect"), DescriptionAttribute("Dry (original unmodified sound) output level (in range 0..1).")]
		public float Dry {
			get { return dry; }
			set { dry = value; }
		}
		[CategoryAttribute("Effect"), DescriptionAttribute("Wet (effect-modified sound) output level (in range 0..1).")]
		public float Wet {
			get { return wet; }
			set { wet = value; }
		}
		[CategoryAttribute("Effect"), DescriptionAttribute("Pan of the wet (effect-modified sound) output (in range 0..1).")]
		public float Pan {
			get { return pan; }
			set { pan = value; }
		}
		[CategoryAttribute("Effect"), DescriptionAttribute("Feedback (in range 0..1).")]
		public float Feedback {
			get { return feedback; }
			set { feedback = value; }
		}
		[CategoryAttribute("Effect"), DescriptionAttribute("Delay period in seconds.")]
		public float Period {
			get { return (float)period/(float)samplesPerSecond; }
			set { 
				int newperiod = (int) Math.Round(value*samplesPerSecond);
				ResizeBuffer(newperiod);
			}
		}
		
		public DelayEffectSoundRender() : base()
		{ 
			Initialize();
		}
		
		public DelayEffectSoundRender(ISoundRender soundRender) : base(soundRender)
		{ 
			Initialize();
		}
		
		private void Initialize() 
		{
			samplesPerSecond = Settings.Instance.GetInt(
				"/Settings/Output/Sound/General/SamplesPerSecond");
			bufferSize = samplesPerSecond; // 1 second
			period = samplesPerSecond / 8; // 1/8 second
			leftSoundBuffer = new float[bufferSize];
			rightSoundBuffer = new float[bufferSize];
		}
		
		protected override System.Drawing.Image LoadIcon() {
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager("H3.Knack.SoundBlock",
                                 Assembly.GetExecutingAssembly());
			return (System.Drawing.Image) resources.GetObject("delay");
		}
		
		private void ResizeBuffer(int periodSize) 
		{
			if (periodSize>bufferSize) {
				lock(this) {
					int pos, newSize = periodSize*2;
					float[] newLeftSoundBuffer = new float[newSize];
					float[] newRightSoundBuffer = new float[newSize];
					pos = 0;
					for (int i=writePos; i<bufferSize; i++) {
						newLeftSoundBuffer[pos] = leftSoundBuffer[i];
						newRightSoundBuffer[pos] = rightSoundBuffer[i];
						pos++;
					}
					for (int i=0; i<writePos; i++) {
						newLeftSoundBuffer[pos] = leftSoundBuffer[i];
						newRightSoundBuffer[pos] = rightSoundBuffer[i];
						pos++;
					}
					leftSoundBuffer = newLeftSoundBuffer;
					rightSoundBuffer = newRightSoundBuffer;
					bufferSize = newSize;
					writePos = pos;
					period = periodSize;
				}
			} else if (period!=periodSize) {
				lock(this) {
					period = periodSize;
				}
			}
		}
		
		public override void Render(float[] leftChannel,float[] rightChannel) 
		{
			int i, pos, channelSize = leftChannel.Length;
			
			if ((leftPeriodBuffer == null) || (leftPeriodBuffer.Length<channelSize)) {
				leftPeriodBuffer = new float[channelSize];
				rightPeriodBuffer = new float[channelSize];
			}

			SoundInputs[0].Render(leftChannel,rightChannel);
			
			lock(this) { // Locked because the buffer may change
				
				// Write the input after writepos
				pos = writePos;
				for (i=0; (i<channelSize) && (writePos+i<bufferSize); i++) {
					leftSoundBuffer[pos] = leftChannel[i];
					rightSoundBuffer[pos] = rightChannel[i];
					pos++;
				}
				pos = 0;
				while (i<channelSize) {
					leftSoundBuffer[pos] = leftChannel[i];
					rightSoundBuffer[pos] = rightChannel[i];
					i++;
					pos++;
				}
				
				// Copy from soundBuffer to periodBuffer
				pos = 0;
				for (i=writePos-period; (i<0) && (i<writePos-period+channelSize); i++) {
					leftPeriodBuffer[pos] = leftSoundBuffer[i+bufferSize];
					rightPeriodBuffer[pos] = rightSoundBuffer[i+bufferSize];
					pos++;
				}
				while (i<writePos-period+channelSize) {
					leftPeriodBuffer[pos] = leftSoundBuffer[i];
					rightPeriodBuffer[pos] = rightSoundBuffer[i];
					i++;
					pos++;
				}
					
				// Apply delay to the output
				for (i=0; i<channelSize; i++) {
					leftChannel[i] = leftChannel[i]*dry + leftPeriodBuffer[i]*wet*(1.0f-pan);;
					rightChannel[i] = rightChannel[i]*dry + rightPeriodBuffer[i]*wet*pan;
				}
				
				// Apply feedback on the buffer
				if (feedback>0) {
					pos = writePos;
					for (i=0; (i<channelSize) && (writePos+i<bufferSize); i++) {
						leftSoundBuffer[pos] += leftChannel[i]*feedback;
						rightSoundBuffer[pos] += rightChannel[i]*feedback;
						pos++;
					}
					pos = 0;
					while (i<channelSize) {
						leftSoundBuffer[pos] += leftChannel[i]*feedback;
						rightSoundBuffer[pos] += rightChannel[i]*feedback;
						i++;
						pos++;
					}
				}
				
				// Update writePos
				writePos += channelSize;
				if (writePos>=bufferSize) writePos-=bufferSize;
			}
		}
	}
}
