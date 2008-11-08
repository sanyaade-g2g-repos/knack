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
using System.Windows.Forms;


namespace H3.Knack.UI
{
	public sealed class SoundItemLink : System.Windows.Forms.Panel
	{
		bool dragging = false;
		const int size = 8;
		int index;
		SoundItem soundItem;
		SoundItemLink linkedSoundItemLink = null;
		
		public enum SoundItemLinkType { SoundInput, SoundOutput, MidiInput, MidiOutput }
		
		SoundItemLinkType soundItemLinkType;
		
		public int Index {
			get { return this.index; }
		}
		public SoundItem SoundItem {
			get { return this.soundItem; }
		}
		public SoundItemLinkType Type {
			get { return this.soundItemLinkType; }
		}
		public SoundItemLink LinkedSoundItemLink {
			get { return this.linkedSoundItemLink; }
			set { this.linkedSoundItemLink = value; }
		}
		public bool Linked {
			get { return (this.linkedSoundItemLink != null); }
		}
		
		public SoundItemLink(SoundItem soundItem, int index, SoundItemLinkType soundItemLinkType) : base() 
		{ 
			this.soundItem = soundItem;
			this.soundItemLinkType = soundItemLinkType;
			this.index = index;
			this.Size = new System.Drawing.Size(size,size);
			this.ContextMenu = new ContextMenu();
			
			if (soundItemLinkType == SoundItemLinkType.SoundInput) {
				//this.BackColor = System.Drawing.Color.LightGreen;
				this.BackgroundImage = (System.Drawing.Image) soundItem.SoundItemTree.Resources.GetObject("sound-in-handle");
				this.Location = new System.Drawing.Point(soundItem.Location.X,
				                                         soundItem.Location.Y + 32 - (soundItem.SoundInputs*size) + index * size * 2 + 4);
			}
			if (soundItemLinkType == SoundItemLinkType.SoundOutput) {
				this.BackgroundImage = (System.Drawing.Image) soundItem.SoundItemTree.Resources.GetObject("sound-out-handle");
				
				//this.BackColor = System.Drawing.Color.Yellow;
				this.Location = new System.Drawing.Point(soundItem.Location.X+64-size,
				                                         soundItem.Location.Y + 32 - (soundItem.SoundOutputs*size) + index * size * 2 + 4);
			}
			if (soundItemLinkType == SoundItemLinkType.MidiInput) {
				//this.BackColor = System.Drawing.Color.Aqua;
				this.BackgroundImage = (System.Drawing.Image) soundItem.SoundItemTree.Resources.GetObject("midi-in-handle");
				
				this.Location = new System.Drawing.Point(soundItem.Location.X + 32 - (soundItem.MidiInputs*size) + index * size * 2 + 4,
				                                         soundItem.Location.Y);
			}
			if (soundItemLinkType == SoundItemLinkType.MidiOutput) {
				//this.BackColor = System.Drawing.Color.Blue;
				this.BackgroundImage = (System.Drawing.Image) soundItem.SoundItemTree.Resources.GetObject("midi-out-handle");
				
				this.Location = new System.Drawing.Point(soundItem.Location.X + 32 - (soundItem.MidiOutputs*size) + index * size * 2 + 4,
				                                         soundItem.Location.Y + 64 - size);
			}
				
		}
		
		public void Link(SoundItemLink soundItemLink)
		{
			this.linkedSoundItemLink = soundItemLink;
			soundItemLink.LinkedSoundItemLink = this;
		}
		
		public void Unlink()
		{
			if (this.linkedSoundItemLink != null) {
				this.linkedSoundItemLink.linkedSoundItemLink = null;
				this.linkedSoundItemLink = null;
			}
		}
		
		protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
		{
		    base.OnMouseDown(e);
			this.BringToFront();
			dragging = true;
		}
		
		protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
		{
		    base.OnMouseMove(e);
			if (dragging) {
				this.SoundItem.SoundItemTree.BeginLink(this.Location.X+this.SoundItem.Location.X+size/2,
				                                       this.Location.Y+this.SoundItem.Location.Y+size/2,
				                                       this.Location.X+this.SoundItem.Location.X+e.X,
				                                       this.Location.Y+this.SoundItem.Location.Y+e.Y);
			}
		}
		
		protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
		{
		    base.OnMouseUp(e);
			if (dragging) {
				dragging = false;
				this.soundItem.SoundItemTree.EndLink(this);
			}
		}
		/*
		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
		{
		    base.OnPaint(e);
			e.Graphics.DrawRectangle(new System.Drawing.Pen(System.Drawing.Color.DarkGray),0,0,this.Width-1,this.Height-1);
		}
		*/
	}
}
