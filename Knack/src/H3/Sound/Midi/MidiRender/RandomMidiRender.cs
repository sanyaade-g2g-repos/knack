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
using System.Collections;
using System.Threading;
using System.ComponentModel;

namespace H3.Sound.Midi.MidiRender
{
	public class RandomMidiRender : SoundBlock, IMidiRender
	{	
		int noteMin = 50;
		int noteMax = 120;
		int waitMin = 10;
		int waitMax = 100;
		float velocityMin = 0.3f;
		float velocityMax = 1.0f;
		double noteOnProbability = 0.5;
		double noteOffProbability = 0.5;
		bool stop = false;
		int maxConcurrentNotes = 3;
		Random rand;
		ArrayList notesList;
		
		public event MidiMessageHandler OnMidiMessage;
		
		[CategoryAttribute("Midi"), DescriptionAttribute("Lowest possible index for a note.")]
		public int NoteMin {
			get{ return noteMin; }
			set{ 
				noteMin = value;
				if (noteMin<0) noteMin = 0;
				if (noteMin>noteMax) noteMin = noteMax;
			}
		}
		[CategoryAttribute("Midi"), DescriptionAttribute("Highest possible index for a note.")]
		public int NoteMax {
			get{ return noteMax; }
			set{ 
				noteMax = value;
				if (noteMax<noteMin) noteMax = noteMin;
			}
		}
		[CategoryAttribute("Midi"), DescriptionAttribute("Lowest possible time between two notes (in seconds).")]
		public float WaitMin {
			get{ return waitMin / 1000.0f; }
			set{ 
				waitMin = (int) Math.Round(value * 1000.0f);
				if (waitMin<1) waitMin = 1;
				if (waitMin>waitMax) waitMin = waitMax;
			}
		}
		[CategoryAttribute("Midi"), DescriptionAttribute("Highest possible time between two notes (in seconds).")]
		public float WaitMax {
			get{ return waitMax / 1000.0f; }
			set{ 
				waitMax = (int) Math.Round(value * 1000.0f);
				if (waitMax<waitMin) waitMax = waitMin;
			}
		}
		[CategoryAttribute("Midi"), DescriptionAttribute("Lowest possible velocity of a note (in range 0..1).")]
		public float VelocityMin {
			get{ return velocityMin; }
			set{ 
				velocityMin = value;
				if (velocityMin<0.0001f) velocityMin = 0.0001f;
				if (velocityMin>velocityMax) velocityMin = velocityMax;
			}
		}
		[CategoryAttribute("Midi"), DescriptionAttribute("Highest possible velocity of a note (in range 0..1).")]
		public float VelocityMax {
			get{ return velocityMax; }
			set{ 
				velocityMax = value;
				if (velocityMax<velocityMin) velocityMax = velocityMin;
				if (velocityMax>1.0f) velocityMax = 1.0f;
			}
		}
		[CategoryAttribute("Midi"), DescriptionAttribute("Probability of a key press (in range 0..1).")]
		public double NoteOnProbability {
			get{ return noteOnProbability; }
			set{ 
				noteOnProbability = value;
				if (noteOnProbability<0.0) noteOnProbability = 0.0;
				if (noteOnProbability>1.0) noteOnProbability = 1.0;
			}
		}
		[CategoryAttribute("Midi"), DescriptionAttribute("Probability of a key release (in range 0..1).")]
		public double NoteOffProbability {
			get{ return noteOffProbability; }
			set{ 
				noteOffProbability = value;
				if (noteOffProbability<0.0) noteOffProbability = 0.0;
				if (noteOffProbability>1.0) noteOffProbability = 1.0;
			}
		}
		[CategoryAttribute("Midi"), DescriptionAttribute("Maximum number of concurrent notes played.")]
		public int ConcurrentNotesMax {
			get{ return maxConcurrentNotes; }
			set{ 
				maxConcurrentNotes = value;
				if (maxConcurrentNotes<1) maxConcurrentNotes = 1;
			}
		}
		
		public RandomMidiRender() 
		{
			rand = new Random();
			notesList = new ArrayList(10);
			OnMidiMessage += new MidiMessageHandler(NullMidiMessageHandler);
			Start();
		}
		
		~RandomMidiRender()
		{
			Stop();
		}
		
		protected override System.Drawing.Image LoadIcon() {
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager("H3.Knack.SoundBlock",
                                 Assembly.GetExecutingAssembly());
			return (System.Drawing.Image) resources.GetObject("midi-random");
		}
		
		private void NullMidiMessageHandler(MidiMessage msg) 
		{ }
		
		public void Start() 
		{  
			Thread midiThread = new Thread(new ThreadStart(RandomMidiThread));  
			midiThread.Name = "RandomMidiThread";  
			midiThread.Priority = ThreadPriority.AboveNormal;  
			midiThread.IsBackground = true;
			midiThread.Start();  
		}
		
		public void Stop()
		{
			stop = true;
		}
		
		private void NoteOn()
		{
			MidiMessage message = new MidiMessage();
			message.Status = 0x90;
        	message.Note = rand.Next(noteMin,noteMax);
			message.Velocity = rand.Next((int)Math.Round(velocityMin*128.0),(int)Math.Round(velocityMax*128.0));
			//Logger.Instance.Log(message.ToString());
        	OnMidiMessage(message);
        	notesList.Add(message);
		}
		
		private void NoteOff()
		{
			if (notesList.Count <= 0) return;
			int noteToRemove = rand.Next(notesList.Count);
        	MidiMessage message = (MidiMessage) notesList[noteToRemove];
        	
        	notesList.RemoveAt(noteToRemove);
        	message.Velocity = 0;
        	OnMidiMessage(message);
		}
		
		private void RandomMidiThread()
        {
        	while (!stop) {
        		System.Threading.Thread.Sleep(rand.Next(waitMin,waitMax));
        		if (notesList.Count >= maxConcurrentNotes) {
        			if (rand.NextDouble()<noteOffProbability) NoteOff();
        		} else {
        			if (rand.NextDouble()<noteOffProbability) NoteOff();
        			if (rand.NextDouble()<noteOnProbability) NoteOn();
        		}
        	}
        }
	}
}
