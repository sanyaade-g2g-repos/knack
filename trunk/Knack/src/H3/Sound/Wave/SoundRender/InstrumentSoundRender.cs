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
using System.ComponentModel;

using H3.Sound.Midi;
using H3.Sound.Midi.MidiRender;

namespace H3.Sound.Wave.SoundRender
{
	/// <summary>
	/// Instrument sound render	
	/// </summary>
	public class InstrumentSoundRender : SoundRender, IMidiHandler
	{
		Hashtable renders = new Hashtable();
		MidiMessageHandler midiMessageHandler = null;
		Random rand = new Random();
		ArrayList rendersToRemove = new ArrayList(10);
		float [] leftSoundBuffer;
		float [] rightSoundBuffer;
		
		[BrowsableAttribute(false)]
		public MidiMessageHandler MidiMessageHandler 
		{ 
			get { 
				if (midiMessageHandler == null)
					midiMessageHandler = new MidiMessageHandler(OnMidiMessage);
				return midiMessageHandler; 
			}
		}

		public InstrumentSoundRender() : base(1,1)
		{ }
		
		public InstrumentSoundRender(ISoundRender soundRender) : base(soundRender,1)
		{ }
		
		protected override System.Drawing.Image LoadIcon() {
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager("H3.Knack.SoundBlock",
                                 Assembly.GetExecutingAssembly());
			return (System.Drawing.Image) resources.GetObject("instrument");
		}
		
		private void AddRender(int renderID, ISoundRender render) 
		{
			lock(this) {
				if (this.renders.ContainsKey(renderID)) {
					ISoundRender finishingSR = (ISoundRender) this.renders[renderID];
					finishingSR.Stop();
					this.renders.Remove(renderID);
					for (int i=0; i<10; i++) { // prova max 10 volte!!
						int newRenderID = renderID + rand.Next(1000,1000000);
						if (!this.renders.ContainsKey(newRenderID)) {
							this.renders.Add(newRenderID,finishingSR);
							break;
						}
					}
				}
				this.renders.Add(renderID,render);
			}
		}
		
		private void StopRender(int renderID) 
		{
			ISoundRender tempRender;
			lock(this) {
				tempRender = (ISoundRender) this.renders[renderID];
				if (tempRender != null) tempRender.Stop();
			}
		}
	
		public override void Render(float[] leftChannel,float[] rightChannel)
		{
			int i, channelSize = leftChannel.Length;
			ISoundRender tempRender;
			IDictionaryEnumerator renderEnum;
			
			rendersToRemove.Clear();
		
			if ((leftSoundBuffer == null) || (leftSoundBuffer.Length<channelSize)) {
				leftSoundBuffer = new float[channelSize];
				rightSoundBuffer = new float[channelSize];
			}
			for (i=0; i<channelSize; i++) leftSoundBuffer[i] = 0.0f;
			for (i=0; i<channelSize; i++) rightSoundBuffer[i] = 0.0f;
			for (i=0; i<channelSize; i++) leftChannel[i] = 0.0f;
			for (i=0; i<channelSize; i++) rightChannel[i] = 0.0f;
			
			lock(this) {
				renderEnum = this.renders.GetEnumerator();
				
	      		while (renderEnum.MoveNext()) {
	      			tempRender = (ISoundRender)	renderEnum.Value;
	      			if (tempRender.Playing) {
	      				tempRender.Render(leftSoundBuffer,rightSoundBuffer);
		      			for (i=0; i<channelSize; i++) {
							leftChannel[i] += leftSoundBuffer[i];
		      				rightChannel[i] += rightSoundBuffer[i];
						}
	      			} else {
	      				rendersToRemove.Add(renderEnum.Key);
	      			} 
	      		}
	      		
	      		foreach (int key in rendersToRemove) {
	      			this.renders.Remove(key);
	      		}
			}
		}
		
		public void OnMidiMessage(MidiMessage msg) {
			if (msg.Velocity>0) {
				AddRender(msg.Note,(ISoundRender) SoundInputs[0].Clone(msg));
			} else {
				StopRender(msg.Note);
			}
		}
	}
}
