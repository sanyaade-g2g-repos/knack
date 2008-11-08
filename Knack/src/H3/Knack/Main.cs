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
using System.Windows.Forms;

using H3.Utils;
using H3.Sound.Midi;
using H3.Sound.Wave;
using H3.Sound;
using H3.Knack.UI;

namespace H3.Knack
{
	class MainClass
	{
		[STAThread]
		public static void Main(string[] args)
		{
			try {
				new Knack().Run(args);
			} catch (Exception e) {
				Logger.Instance.Log(e.ToString());
				Console.Error.WriteLine(e.ToString());
			}
		}
	}
	
	class Knack
	{	
		public void Run(string[] args)
		{
			Console.WriteLine();
			Console.WriteLine("Knack - v." + Assembly.GetExecutingAssembly().GetName().Version.ToString());
			Console.WriteLine("------------------------------");
			Console.WriteLine();
			
			KnackForm mainForm = new KnackForm();
			
			Settings.Instance.ToString(); // Temporary solution to load settings immediately
	
			if (args.Length == 1) {
				Console.WriteLine("Loading "+args[0]);
				mainForm.FileLoad(args[0]);
			}
			Console.WriteLine("Close the main window to quit");
			Console.WriteLine();
			
			Application.Run(mainForm);
		}
	}
	/*
	class KnackTest : SoundBlock, IMidiHandler
	{
		InstrumentSoundRender myMixer;
		Random rand = new Random();
		
		public MidiMessageHandler MidiMessageHandler 
		{ 
			get { return new MidiMessageHandler(OnMidiMessage); }
		}
		
		public void Run()
		{
			Console.WriteLine("Knack - v."+Assembly.GetExecutingAssembly().GetName().Version.ToString());
			Console.WriteLine("------------------------------");

			IMidiRender myMidiIn = new RandomMidiRender();
			myMixer = new InstrumentSoundRender();
			DelayEffectSoundRender delay1 = new DelayEffectSoundRender(myMixer);
			DelayEffectSoundRender delay2 = new DelayEffectSoundRender(delay1);
			delay2.BufferSize = 44100/4;
			delay2.Pan = 0.7f;
			delay2.Wet = 0.4f;
			SoundOutRender mySoundOut = new SoundOutRender(delay2);
			
			myMidiIn.OnMidiMessage += new MidiMessageHandler(OnMidiMessage);
			
			
			Console.WriteLine();
			Console.WriteLine("Use your MIDI keyboard and press ENTER to quit");
			Console.WriteLine();
	
			System.Console.ReadLine();
			
			mySoundOut.Stop();
		}
		
		public void OnMidiMessage(MidiMessage msg) {
			if (msg.Velocity > 0) {
				ADSRSoundRender soundA = new ADSRSoundRender(new SawtoothSoundRender(msg.Frequency*0.999));
				ADSRSoundRender soundB = new ADSRSoundRender(new SquareSoundRender(msg.Frequency*1.001));
				ADSRSoundRender soundC = new ADSRSoundRender(new SineSoundRender(msg.Frequency));
				ISoundRender mySound = new AddOperatorSoundRender(new MultiplyOperatorSoundRender(soundA,soundB),soundC);
			
				soundA.Velocity = (msg.Velocity / 127.0f) * 0.4f;
				soundB.Attack = 0.0001f;
				soundA.Release = 0.00006f;
				soundB.Release = 0.00006f;
				soundC.Velocity = (msg.Velocity / 127.0f) * 0.5f + 0.2f;
				soundA.Pan=(float) (0.5 + ((msg.Frequency - 1000.0)/13000.0) * 0.3);
				soundB.Pan=soundA.Pan+(float)((rand.NextDouble()-0.5)*0.1);
				soundC.Pan=soundA.Pan+(float)((rand.NextDouble()-0.5)*0.1);
				
				myMixer.addRender(msg.Note,mySound);
			} else myMixer.stopRender(msg.Note);
		}
	}
	*/
}
