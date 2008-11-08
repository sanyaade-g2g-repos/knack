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

using H3.Utils;
using H3.Sound.Midi;
using H3.Sound.Midi.MidiRender;

namespace H3.Sound.Wave.SoundRender
{
	/// <summary>
	/// Generic wave sound render	
	/// </summary>
	public abstract class WaveSoundRender : SoundRender, IMidiHandler
	{
		protected double pos = 0.0;
		protected double posIncrement = 0.0;
		protected double freq = 440.0;
		double octScaling = 0.0;
		double toneScaling = 0.0;
		double freqScaling = 0.0;
		protected int samplesPerSecond = 44100;
		MidiMessageHandler midiMessageHandler;
		
		[BrowsableAttribute(false)]
		public MidiMessageHandler MidiMessageHandler 
		{ 
			get { 
				if (midiMessageHandler == null)
					midiMessageHandler = new MidiMessageHandler(OnMidiMessage);
				return midiMessageHandler; 
			}
		}
		
		protected System.Drawing.Image icon;
		
		void Initialize()
		{
			icon = null;
			samplesPerSecond = Settings.Instance.GetInt(
				"/Settings/Output/Sound/General/SamplesPerSecond");
			posIncrement = Math.PI * 2.0 / (double) samplesPerSecond;
		}
		
		protected WaveSoundRender() : base(0,1)
		{ 
			Initialize();
		}
		
		protected WaveSoundRender(double freq) : this()
		{
			Initialize();
			this.freq = freq;
		}
		
		[CategoryAttribute("Wave"), DescriptionAttribute("Wave frequency, in Hertz(Hz).")]
		public double Frequency {
			get { return freq; }
			set { freq = value; }
		}
		[CategoryAttribute("Wave Modifiers"), DescriptionAttribute("Number of octaves to add to the base frequency.")]
		public double OctaveScaling {
			get { return octScaling; }
			set { octScaling = value; }
		}
		[CategoryAttribute("Wave Modifiers"), DescriptionAttribute("Number of tones to add to the base frequency.")]
		public double ToneScaling {
			get { return toneScaling; }
			set { toneScaling = value; }
		}
		[CategoryAttribute("Wave Modifiers"), DescriptionAttribute("Frequency to add to the base frequency, in Hertz(Hz).")]
		public double FrequencyScaling {
			get { return freqScaling; }
			set { freqScaling = value; }
		}
		
		public override object Clone(MidiMessage msg)
		{
			WaveSoundRender wsr = (WaveSoundRender) base.Clone(msg);
			wsr.Frequency = msg.Frequency;
			wsr.OctaveScaling = OctaveScaling;
			wsr.toneScaling = ToneScaling;
			wsr.FrequencyScaling = FrequencyScaling;
			return wsr;
		}
		
		protected double NoteToScaledFrequency(double note)
		{
			double freq = MidiMessage.NoteToFrequency(note + octScaling * 12 + toneScaling * 2) + freqScaling;
			/*
			  Console.WriteLine(note + " (" + MidiMessage.NoteToFrequency(note) 
			                  + ") -> " + MidiMessage.FrequencyToNote(freq) 
			                  + " (" + freq + ")");
			*/
			if (freq < 0.0) return 0.0;
			return freq;	
		}
		
		protected double FrequencyToScaledFrequency(double freq)
		{
			return NoteToScaledFrequency(MidiMessage.FrequencyToNote(freq));
		}
		
		public void OnMidiMessage(MidiMessage msg) {
			if (msg.Frequency > 0) {
				this.freq = NoteToScaledFrequency(msg.Note);
			}
		}
	}
}
