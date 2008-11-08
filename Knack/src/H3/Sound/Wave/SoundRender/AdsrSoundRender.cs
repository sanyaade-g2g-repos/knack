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
using H3.Sound.Midi;

namespace H3.Sound.Wave.SoundRender
{
	/// <summary>
	/// Attack Decay Sustain Release wave sound render	
	/// </summary>
	public class AdsrSoundRender : SoundRender
	{
		public enum AdsrStatus { PreDelay, Attack, Decay, Sustain, Release }
		AdsrStatus status = AdsrStatus.PreDelay;
		float ADSRenvelope = 0.0f;
		float attackLevel = 1.0f / 0.01f / 44100.0f;
		float decayLevel = 1.0f / 0.05f / 44100.0f;
		float sustainLevel = 0.4f;
		float releaseLevel = 1.0f / 0.5f / 44100.0f;
		float velocity = 1.0f;
		float volume = 1.0f;
		float pan = 0.5f;
		int preDelaySamples = 0;
		int preDelaySamplesElapsed = 0;
		int samplesPerSecond = 44100;
		Random rand = new Random();

		[BrowsableAttribute(false), CategoryAttribute("Status"), DescriptionAttribute("ADSR envelope status")]
		public AdsrStatus AdsrCurrentStatus {
			get { return status; }
			set { status = value; 
				if (status == AdsrStatus.PreDelay) {
					ADSRenvelope = 0.0f;
					preDelaySamplesElapsed = 0;
				}
				if (status == AdsrStatus.Attack) ADSRenvelope = 0.0f;
				if (status == AdsrStatus.Decay) ADSRenvelope = 1.0f;
				if (status == AdsrStatus.Sustain) ADSRenvelope = sustainLevel;
				if (status == AdsrStatus.Release) ADSRenvelope = sustainLevel;
				playing = true;
			}
		}
		[CategoryAttribute("Envelope Modifiers"), DescriptionAttribute("Sound amplification level (default is 1).")]
		public float Velocity {
			get { return velocity; }
			set { velocity = value; }
		}
		[CategoryAttribute("Envelope Modifiers"), DescriptionAttribute("Time to wait before playing the note.")]
		public float PreDelay {
			get { return (float)preDelaySamples / samplesPerSecond;  }
			set { preDelaySamples = (int)Math.Round(value * samplesPerSecond); }
		}
		[CategoryAttribute("Envelope"), DescriptionAttribute("Attack time in seconds.")]
		public float Attack {
			get { return 1.0f / attackLevel / samplesPerSecond;  }
			set { attackLevel = 1.0f / value / samplesPerSecond; }
		}
		[CategoryAttribute("Envelope"), DescriptionAttribute("Decay time in seconds.")]
		public float Decay {
			get { return 1.0f / decayLevel / samplesPerSecond; }
			set { decayLevel = 1.0f / value / samplesPerSecond; }
		}
		[CategoryAttribute("Envelope"), DescriptionAttribute("Sustain level (in range 0..1).")]
		public float Sustain {
			get { return sustainLevel; }
			set { sustainLevel = value; }
		}
		[CategoryAttribute("Envelope"), DescriptionAttribute("Release time in seconds.")]
		public float Release {
			get { return 1.0f / releaseLevel / samplesPerSecond;; }
			set { releaseLevel = 1.0f / value / samplesPerSecond; }
		}
		[CategoryAttribute("Envelope Modifiers"), DescriptionAttribute("Pan of the note (in range 0..1).")]
		public float Pan {
			get { return pan; }
			set { pan = value; }
		}
		
		void Initialize()
		{
			samplesPerSecond = Settings.Instance.GetInt(
				"/Settings/Output/Sound/General/SamplesPerSecond");
			attackLevel = 1.0f / 0.01f / samplesPerSecond;
			decayLevel = 1.0f / 0.05f / samplesPerSecond;
			releaseLevel = 1.0f / 0.5f / samplesPerSecond;
		}
		
		public AdsrSoundRender() : base(1)
		{ 
			Initialize();
		}
		
		public AdsrSoundRender(ISoundRender soundRender) : base(soundRender)
		{ 
			Initialize();
		}
		
		protected override System.Drawing.Image LoadIcon()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager("H3.Knack.SoundBlock",
                                 Assembly.GetExecutingAssembly());
			return (System.Drawing.Image) resources.GetObject("adsr");
		}
		
		public override object Clone(MidiMessage msg)
		{
			AdsrSoundRender asr = new AdsrSoundRender();
			asr.SoundInputs[0] = (ISoundRender) this.SoundInputs[0].Clone(msg);
			asr.PreDelay = PreDelay;
			asr.Attack = Attack;
			asr.Decay = Decay;
			asr.Sustain = Sustain;
			asr.Release = Release;
			asr.Velocity = ((msg.Velocity + (float)rand.NextDouble()) / 128.0f) * Velocity;
			asr.Pan = (float) (Pan + ((msg.Frequency - 1000.0)/13000.0) * 0.3 + ((rand.NextDouble()-0.5)*0.1));
			return asr;
		}
		
		public override void Render(float[] leftChannel,float[] rightChannel)
		{
			ISoundRender soundRender = this.SoundInputs[0];
			int channelSize = leftChannel.Length;
			float leftVolume = volume * (1.0f-pan) * velocity;
			float rightVolume = volume * pan * velocity;
			
			if (soundRender.Playing) soundRender.Render(leftChannel,rightChannel);
			else {
				int i;
				this.playing = false;
				for (i=0; i<channelSize; i++) leftChannel[i] = 0.0f;
				for (i=0; i<channelSize; i++) rightChannel[i] = 0.0f;
				return;
			}
			
			for (int i=0; i<channelSize; i++) {  
				switch(status) {
					case AdsrStatus.PreDelay:
						preDelaySamplesElapsed++;
						if (preDelaySamplesElapsed >= preDelaySamples) {
							status = AdsrStatus.Attack;
						}
						break;
					case AdsrStatus.Attack: 
						ADSRenvelope += attackLevel; 
						if (ADSRenvelope >= 1.0f) {
							ADSRenvelope = 1.0f;
							status = AdsrStatus.Decay;
						}
						break;
					case AdsrStatus.Decay: 
						ADSRenvelope -= decayLevel; 
						if (ADSRenvelope <= sustainLevel) {
							ADSRenvelope = sustainLevel;
							status = AdsrStatus.Sustain;
						}
						break;
					case AdsrStatus.Sustain: 
						ADSRenvelope = sustainLevel; 
						break;
					case AdsrStatus.Release: 
						ADSRenvelope -= releaseLevel;
						if (ADSRenvelope <= 0.0f) {
							ADSRenvelope = 0.0f;
							playing = false;
						}
						break;
				}
				leftChannel[i] = leftChannel[i] * leftVolume * ADSRenvelope;
				rightChannel[i] = rightChannel[i] * rightVolume * ADSRenvelope;                
			}
		}
		
		public override void Stop() {
			this.status = AdsrStatus.Release;
		}
	}
}
