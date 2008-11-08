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

using System.Threading;
using System.Reflection;
using System.ComponentModel;
using System.Windows.Forms;

namespace H3.Sound.Midi.MidiRender
{
	public class MidiEditorRender : SoundBlock, IMidiHandler, IMidiRender
	{	
		MidiMessageHandler midiMessageHandler;
		public event MidiMessageHandler OnMidiMessage;
		MidiFile midiFile;
		bool stop = false;
		MidiEditorForm midiEditorForm;
		
		[BrowsableAttribute(false)]
		public MidiMessageHandler MidiMessageHandler 
		{ 
			get { 
				if (midiMessageHandler == null)
					midiMessageHandler = new MidiMessageHandler(OnMidiMessageReceived);
				return midiMessageHandler; 
			}
		}

		public MidiEditorRender() : base(0,1)
		{
			this.midiFile = new MidiFile("testfile.mid");
			this.OnMidiMessage += new MidiMessageHandler(NullMidiMessageHandler);
			this.midiEditorForm = new MidiEditorForm();
			this.editable = true;
			Start();
		}
		
		~MidiEditorRender() {
			Stop();
		}
		
		protected override System.Drawing.Image LoadIcon() {
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager("H3.Knack.SoundBlock",
                                 Assembly.GetExecutingAssembly());
			return (System.Drawing.Image) resources.GetObject("midi-filter");
		}
		
		public void OnMidiMessageReceived(MidiMessage msg) {
			this.OnMidiMessage(msg);
		}
		
		private void NullMidiMessageHandler(MidiMessage msg) 
		{ }
		
		public void Start() 
		{  
			Thread midiThread = new Thread(new ThreadStart(PlayMidiThread));
			midiThread.Name = "PlayMidiThread";  
			midiThread.Priority = ThreadPriority.AboveNormal;  
			midiThread.IsBackground = true;
			midiThread.Start();  
		}
		
		public void Stop()
		{
			stop = true;
		}
		
		private void PlayMidiThread()
        {
			System.Threading.Thread.Sleep(4000);
			MidiFileChunkMTrk mtrk = (MidiFileChunkMTrk) midiFile.ChunkMTrk[1];
			int i = 0;
        	while (!stop) {
				MidiEvent evt = (MidiEvent) mtrk.MidiEvents[i];
				//Logger.Instance.Log("DeltaTime: "+evt.DeltaTime);
        		System.Threading.Thread.Sleep(evt.DeltaTime / 50);
        		if ((evt.Status & 0xF0) == 0x90) {
	        		MidiMessage message = new MidiMessage();
					message.Status = 0x90;
					message.Note = evt.Data[0];
					message.Velocity = evt.Data[1];
					//Logger.Instance.Log(message.ToString());
		        	OnMidiMessage(message);
        		}
        		if ((evt.Status & 0xF0) == 0x80) {
	        		MidiMessage message = new MidiMessage();
					message.Status = 0x90;
					message.Note = evt.Data[0];
					message.Velocity = 0;
					//Logger.Instance.Log(message.ToString());
		        	OnMidiMessage(message);
        		}
	        	i++;
	        	if (i >= mtrk.MidiEvents.Count) i = 0;
        	}
        }		
	}	
	
	public class MidiEditorForm : Form
	{
		public MidiEditorForm() : base() 
		{
			this.BackColor = System.Drawing.Color.White;
			this.FormBorderStyle =  FormBorderStyle.SizableToolWindow;
			this.WindowState = FormWindowState.Normal;
			/*
			this.Top = Screen.PrimaryScreen.Bounds.Top;
			this.Left = Screen.PrimaryScreen.Bounds.Left;
			this.Width = Screen.PrimaryScreen.Bounds.Width;
			this.Height = Screen.PrimaryScreen.Bounds.Height;
			*/
			this.ShowInTaskbar = true;
		}
	}
}
