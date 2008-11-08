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
using System.Collections;
using System.Windows.Forms;
using System.Drawing;

using H3.Sound;
using H3.Sound.Wave.SoundRender;
using H3.Sound.Midi.MidiRender;

namespace H3.Knack.UI
{
	/// <summary>
	/// A SoundItem is a block in the SoundItemTree	
	/// </summary>
	public sealed class SoundItem : UserControl
	{
		int soundInputs = 0;
		int soundOutputs = 0;
		int midiInputs = 0;
		int midiOutputs = 0;
		bool selected = false;
		SoundItemTree soundItemTree;
		ArrayList soundItemLinks;
		SoundItemLink[] soundItemLinkSoundInput;
		SoundItemLink[] soundItemLinkSoundOutput;
		SoundItemLink[] soundItemLinkMidiInput;
		SoundItemLink[] soundItemLinkMidiOutput;
		//Random rand = new Random();
		SoundBlock soundBlock;
		ISoundRender soundRender;
		IMidiRender midiRender;
		System.Drawing.Image unselectedBackground, selectedBackground;
		System.Drawing.Font nameFont;
		System.Drawing.Brush nameFontBrush;
		
		public SoundBlock SoundBlock {
			get { return soundBlock; }
		}
		public ISoundRender SoundRender {
			get { return soundRender; }
		}
		public IMidiRender MidiRender {
			get { return midiRender; }
		}
		public SoundItemTree SoundItemTree{
			get { return soundItemTree; }
		}
		public int SoundInputs{
			get { return soundInputs; }
		}
		public int SoundOutputs{
			get { return soundOutputs; }
		}
		public int MidiInputs{
			get { return midiInputs; }
		}
		public int MidiOutputs{
			get { return midiOutputs; }
		}
		public new string Name {
			get { if(soundBlock!=null) return soundBlock.Name; return ""; }
			set { if(soundBlock!=null) soundBlock.Name = value; }
		}
		public ICollection SoundItemLinks {
			get { 
				if (soundItemLinks == null) {
					soundItemLinks = new ArrayList(6);
					foreach (SoundItemLink sil in soundItemLinkSoundInput) soundItemLinks.Add(sil);
					foreach (SoundItemLink sil in soundItemLinkSoundOutput) soundItemLinks.Add(sil);
					foreach (SoundItemLink sil in soundItemLinkMidiInput) soundItemLinks.Add(sil);
					foreach (SoundItemLink sil in soundItemLinkMidiOutput) soundItemLinks.Add(sil);
				}
				return soundItemLinks;
			}
		}
		public bool Selected {
			get { return selected; }
			set { 
				if (value != selected) {
					selected = value;
					this.BringToFront();
					this.Invalidate();
				}
			}
		}

		public SoundItemLink GetSoundItemLink(SoundItemLink.SoundItemLinkType linkType, int index) {
			switch (linkType) {
				case SoundItemLink.SoundItemLinkType.SoundInput :
					return soundItemLinkSoundInput[index];
				case SoundItemLink.SoundItemLinkType.SoundOutput :
					return soundItemLinkSoundOutput[index];
				case SoundItemLink.SoundItemLinkType.MidiInput :
					return soundItemLinkMidiInput[index];
				case SoundItemLink.SoundItemLinkType.MidiOutput :
					return soundItemLinkMidiOutput[index];
			}
			return null;
		}
		
		void EditBlockClick(object sender, System.EventArgs e)
		{
			this.soundRender.OnEdit();
		}
		
		void DeleteBlockClick(object sender, System.EventArgs e)
		{
			this.soundItemTree.PropertyGrid.SelectedObject = null;
			this.Dispose();
			this.SoundItemTree.Invalidate();
			this.SoundItemTree.Update();
		}
		
		public new void Dispose()
   		{
			foreach (SoundItemLink sil in this.SoundItemLinks) this.SoundItemTree.Unlink(sil);
			this.soundItemTree.SoundItems.Remove(this);
			//Console.WriteLine("Disposing a SoundItem");
			if (soundBlock is IDisposable) {
				IDisposable sb = (IDisposable) soundBlock;
				//Console.WriteLine("Disposing a SoundBlock");
				sb.Dispose();
			}
			soundBlock = null;
			soundRender = null;
			midiRender = null;
			base.Dispose();
		}
		
		public SoundItem(SoundItemTree soundItemTree, SoundBlock soundOrMidiRender, int soundInputs, int soundOutputs, int midiInputs, int midiOutputs) : base() 
		{ 
			this.soundBlock = soundOrMidiRender;
			if (soundOrMidiRender is ISoundRender) 
				this.soundRender = (ISoundRender) soundOrMidiRender;
			if (soundOrMidiRender is IMidiRender) 
				this.midiRender = (IMidiRender) soundOrMidiRender;
			this.soundItemTree = soundItemTree;
			this.soundInputs = soundInputs;
			this.soundOutputs = soundOutputs;
			this.midiInputs = midiInputs;
			this.midiOutputs = midiOutputs;		
			this.Size = new System.Drawing.Size(64, 64);
			this.SetStyle(ControlStyles.AllPaintingInWmPaint |
			              ControlStyles.UserPaint |
			              ControlStyles.DoubleBuffer,true);
			this.unselectedBackground = ((System.Drawing.Image)(soundItemTree.Resources.GetObject("unsel-block")));
			this.selectedBackground = ((System.Drawing.Image)(soundItemTree.Resources.GetObject("sel-block")));
			this.nameFont = new System.Drawing.Font(new System.Drawing.FontFamily("Arial"),
			                                                   9,
			                                                   System.Drawing.FontStyle.Regular,
			                                                   System.Drawing.GraphicsUnit.Pixel);
			this.nameFontBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
			
			ContextMenu BlockContextMenu = new ContextMenu();
			this.ContextMenu = BlockContextMenu;
			
			if ((this.soundRender != null) && (this.soundRender.Editable)) {
				MenuItem EditMenuItem = new MenuItem();
				EditMenuItem.Index = 0;
				EditMenuItem.Text = "Edit block";
				EditMenuItem.Click += new System.EventHandler(this.EditBlockClick);
				EditMenuItem.DefaultItem = true;
				BlockContextMenu.MenuItems.Add(EditMenuItem);
				this.DoubleClick += new EventHandler(EditBlockClick);
			}
			MenuItem DeleteMenuItem = new MenuItem();
			DeleteMenuItem.Index = 1;
			DeleteMenuItem.Text = "Delete block";
			DeleteMenuItem.Click += new System.EventHandler(this.DeleteBlockClick);
			BlockContextMenu.MenuItems.Add(DeleteMenuItem);		
			
			soundItemLinkSoundInput = new SoundItemLink[soundInputs];
			soundItemLinkSoundOutput = new SoundItemLink[soundOutputs];
			soundItemLinkMidiInput = new SoundItemLink[midiInputs];
			soundItemLinkMidiOutput = new SoundItemLink[midiOutputs];
			
			for (int i=0; i<soundInputs;i++) {
				soundItemLinkSoundInput[i] = new SoundItemLink(this,i,SoundItemLink.SoundItemLinkType.SoundInput);
				this.Controls.Add(soundItemLinkSoundInput[i]);
			}
			for (int i=0; i<soundOutputs;i++) {
				soundItemLinkSoundOutput[i] = new SoundItemLink(this,i,SoundItemLink.SoundItemLinkType.SoundOutput);
				this.Controls.Add(soundItemLinkSoundOutput[i]);
			}
			for (int i=0; i<midiInputs;i++) {
				soundItemLinkMidiInput[i] = new SoundItemLink(this,i,SoundItemLink.SoundItemLinkType.MidiInput);
				this.Controls.Add(soundItemLinkMidiInput[i]);
			}
			for (int i=0; i<midiOutputs;i++) {
				soundItemLinkMidiOutput[i] = new SoundItemLink(this,i,SoundItemLink.SoundItemLinkType.MidiOutput);
				this.Controls.Add(soundItemLinkMidiOutput[i]);
			}
		}
		
		public SoundItemLink GetLink(float x, float y)
		{
			foreach (SoundItemLink tempSIK  in SoundItemLinks) {
				if ((tempSIK.Location.X<x) && (tempSIK.Location.Y<y)
					&& (tempSIK.Location.X+tempSIK.Width>x)
					&& (tempSIK.Location.Y+tempSIK.Height>y))
					return tempSIK;
			}
			return null;
		}

		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
		{
			if (selected) this.BackgroundImage = selectedBackground;
			else this.BackgroundImage = unselectedBackground;
	
		    base.OnPaint(e);
		    
		    if (selected) {
				Color selectionColor = Color.FromArgb(127,
				                                      SystemColors.Highlight.R,
				                                      SystemColors.Highlight.G,
				                                      SystemColors.Highlight.B);
				Brush selectionBrush = new SolidBrush(selectionColor);
				
				e.Graphics.FillRectangle(selectionBrush,0,0,this.Width,this.Height);
			}
		    
		    if (soundBlock.Icon != null) e.Graphics.DrawImageUnscaled(soundBlock.Icon,9,9);
			e.Graphics.DrawString(soundBlock.Name,nameFont,nameFontBrush,9,44);
		}
		
	}

}
