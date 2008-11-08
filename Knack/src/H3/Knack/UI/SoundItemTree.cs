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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Reflection;

using H3.Sound;
using H3.Sound.Midi.MidiRender;

namespace H3.Knack.UI
{
	/// <summary>
	/// A Component derived by Panel with the tree of the SoundItems	
	/// </summary>
	public sealed class SoundItemTree : UserControl
	{
		enum SoundItemTreeStatus {
			Default,
			MouseDown,
			MouseDownControl,
			Dragging
		}
		SoundItemTreeStatus status = SoundItemTreeStatus.Default;
		KnackForm knackForm;
		System.Resources.ResourceManager resources = new System.Resources.ResourceManager("H3.Knack.SoundItemTree",
                                 Assembly.GetExecutingAssembly());
		
		ArrayList soundItems = new ArrayList(20);
		bool linking = false, selecting = false;
		float linkX1,linkY1,linkX2,linkY2;
		PropertyGrid propertyGrid;
		Point snap = new System.Drawing.Point(21,21);
		Point selectionStart, selectionEnd;
		Bitmap onePixelBitmap = new Bitmap(1,1);
		Point draggingStart = new Point(0,0);
			
		public PropertyGrid PropertyGrid {
			get { return propertyGrid;}
		}
		public ArrayList SoundItems {
			get { return soundItems; }
		}
		public System.Resources.ResourceManager Resources {
			get { return resources; }
		}
		public System.Drawing.Point Snap {
			get { return snap; }
		}
		
		public SoundItemTree(KnackForm knackForm,PropertyGrid propertyGrid) : base() 
		{ 
			this.knackForm = knackForm;
			this.propertyGrid = propertyGrid;
			this.BackColor = System.Drawing.SystemColors.AppWorkspace;
			this.Size = new System.Drawing.Size(64*10, 64*10);
			this.AutoScroll = true;
			this.AutoScrollMargin = new Size(21,21);

			this.SetStyle(ControlStyles.AllPaintingInWmPaint |
			              ControlStyles.UserPaint |
			              ControlStyles.DoubleBuffer,true);
			onePixelBitmap.SetPixel(0,0,Color.Black);
		}
		
		protected override bool ProcessDialogKey(Keys keyData)
		{	
			if (keyData.Equals(Keys.Delete)) {
				ArrayList itemsToRemove = new ArrayList(soundItems.Count);
				this.Enabled = false;
				propertyGrid.SelectedObject = null; 
				foreach (SoundItem sm in soundItems) {
					if (sm.Selected) {
						itemsToRemove.Add(sm);
					}
				}
				foreach (SoundItem sm in itemsToRemove) {
					soundItems.Remove(sm);
					sm.Dispose();
				}
				this.Enabled = true;
				this.Invalidate();
				this.Update();
			}
			return base.ProcessDialogKey(keyData);
		}
		
		public void Clear()
		{
			
			ArrayList itemsToRemove = new ArrayList(soundItems.Count);
			this.Enabled = false;
			propertyGrid.SelectedObject = null; 
			foreach (SoundItem sm in soundItems) {
				itemsToRemove.Add(sm);
			}
			foreach (SoundItem sm in itemsToRemove) {
				sm.Dispose();
			}
			soundItems.Clear();
			this.Enabled = true;
			this.Invalidate();
			this.Update();
		}
		
		public void AddSoundItem(SoundItem sItem) 
		{
			SnapToGrid(sItem);
			SoundItems.Add(sItem);
			Controls.Add(sItem);
			
			sItem.MouseDown += new MouseEventHandler(this.EventSoundItemMouseDown);
			sItem.MouseMove += new MouseEventHandler(this.EventSoundItemMouseMove);
			sItem.MouseUp += new MouseEventHandler(this.EventSoundItemMouseUp);
		}
		
		public void UnselectAll() {
			foreach (SoundItem sm in soundItems) {
				sm.Selected = false;
			}
		}
		
		public SoundItem GetSoundItem(string name) 
		{
			IEnumerator e = soundItems.GetEnumerator();
			while (e.MoveNext()) {
				SoundItem tempSI = (SoundItem) e.Current;
				if (tempSI.SoundBlock.Name.Equals(name)) return tempSI;
			}
			return null;
		}
		
		public void BeginLink(float x1, float y1,float x2, float y2)
		{
			linking = true;
			linkX1 = x1;
			linkY1 = y1;
			linkX2 = x2;
			linkY2 = y2;
			this.Invalidate();
		}
		
		public void Unlink(SoundItemLink soundItemLink)
		{
			if (!soundItemLink.Linked) return;
			
			SoundItemLink producerSIL = null, consumerSIL = null;
			SoundItemLink soundItemLinkA = soundItemLink;
			SoundItemLink soundItemLinkB = soundItemLink.LinkedSoundItemLink;
			
			if (((soundItemLinkA.Type == SoundItemLink.SoundItemLinkType.SoundOutput)
				&& (soundItemLinkB.Type == SoundItemLink.SoundItemLinkType.SoundInput))
				|| ((soundItemLinkA.Type == SoundItemLink.SoundItemLinkType.MidiOutput)
				&& (soundItemLinkB.Type == SoundItemLink.SoundItemLinkType.MidiInput))) {
				
				producerSIL = soundItemLinkA;
				consumerSIL = soundItemLinkB;
			}
			
			if (((soundItemLinkB.Type == SoundItemLink.SoundItemLinkType.SoundOutput)
				&& (soundItemLinkA.Type == SoundItemLink.SoundItemLinkType.SoundInput))
				|| ((soundItemLinkB.Type == SoundItemLink.SoundItemLinkType.MidiOutput)
				&& (soundItemLinkA.Type == SoundItemLink.SoundItemLinkType.MidiInput))) {
				
				producerSIL = soundItemLinkB;
				consumerSIL = soundItemLinkA;
			}
			
			if ((producerSIL == null) || (consumerSIL == null)) return;
		
		// NOTE: these two lines were causing problems with midi and are not needed!	
		//	if ((producerSIL.LinkedSoundItemLink != consumerSIL) 
		//		|| (consumerSIL.LinkedSoundItemLink != producerSIL)) return;
			
			if (producerSIL.Type == SoundItemLink.SoundItemLinkType.SoundOutput) {
				consumerSIL.SoundItem.SoundRender.SoundInputs[consumerSIL.Index] = new H3.Sound.Wave.SoundRender.NullSoundRender();
			} else if (producerSIL.Type == SoundItemLink.SoundItemLinkType.MidiOutput) {
				IMidiRender midiRender = (IMidiRender) producerSIL.SoundItem.MidiRender;
				IMidiHandler midiHandler = (IMidiHandler) consumerSIL.SoundItem.SoundBlock;
				if ((midiRender!= null) && (midiHandler!=null)) midiRender.OnMidiMessage -= midiHandler.MidiMessageHandler;
			}
		
			consumerSIL.Unlink();
			producerSIL.Unlink();
		}
		
		public void Unlink(SoundBlock soundBlock)
		{
			foreach (SoundItem sItem in soundItems) {
				foreach (SoundItemLink sLink in sItem.SoundItemLinks) {
					if (sLink.Linked && (sLink.Type == SoundItemLink.SoundItemLinkType.MidiInput)){
						if (sLink.LinkedSoundItemLink.SoundItem.SoundBlock == soundBlock) {
							//MessageBox.Show(sLink.SoundItem.Name);
							Unlink(sLink);
						}
					}
				}
			}
		}
		
		public void EndLink(SoundItemLink soundItemLink)
		{
			SoundItemLink foundLink = null;
			linking = false;
			for (int i=0; i<soundItems.Count; i++) {
				SoundItem tempSI = (SoundItem) soundItems[i];
				if (tempSI != soundItemLink.SoundItem) {
					foundLink = tempSI.GetLink(linkX2-tempSI.Location.X,linkY2-tempSI.Location.Y);
					if (foundLink != null) break;
				}
			}
			if (foundLink == null) {
				Unlink(soundItemLink);
				if (soundItemLink.Type == SoundItemLink.SoundItemLinkType.MidiOutput) {
					Unlink(soundItemLink.SoundItem.SoundBlock);
				}
			} else {
				if ((foundLink.Type == SoundItemLink.SoundItemLinkType.SoundInput)
					|| (foundLink.Type == SoundItemLink.SoundItemLinkType.MidiInput)) {
						//swap
						SoundItemLink temp = foundLink;
						foundLink = soundItemLink;
						soundItemLink = temp;
					}
				
				
				bool isMidiLink = ((soundItemLink.Type == SoundItemLink.SoundItemLinkType.MidiInput) &&
					(foundLink.Type == SoundItemLink.SoundItemLinkType.MidiOutput));
				bool isSoundLink = ((soundItemLink.Type == SoundItemLink.SoundItemLinkType.SoundInput) &&
					(foundLink.Type == SoundItemLink.SoundItemLinkType.SoundOutput));
				
				if (isSoundLink) {
					Unlink(soundItemLink);
					Unlink(foundLink);
				}
				if (isMidiLink) {
					if (soundItemLink.Linked) Unlink(soundItemLink);
				}
				    
				if (isSoundLink || isMidiLink)
					 {
					 	soundItemLink.Link(foundLink);
					 	//Console.WriteLine("connected: "+soundItemLink.SoundItem.Name+" - "+soundItemLink.LinkedSoundItemLink.SoundItem.Name);
					 	if (soundItemLink.Type == SoundItemLink.SoundItemLinkType.SoundInput) {
					 		soundItemLink.SoundItem.SoundRender.SoundInputs[soundItemLink.Index] = soundItemLink.LinkedSoundItemLink.SoundItem.SoundRender;
					 	}
					 	else if (soundItemLink.Type == SoundItemLink.SoundItemLinkType.MidiInput) {
					 		SoundBlock sb = (SoundBlock) soundItemLink.SoundItem.SoundBlock;
					 		IMidiRender midiRender = (IMidiRender) soundItemLink.LinkedSoundItemLink.SoundItem.MidiRender;	
							IMidiHandler midiHandler = (IMidiHandler) sb;
					 		sb.MidiInputs[soundItemLink.Index] = midiRender;	
					 		midiRender.OnMidiMessage += midiHandler.MidiMessageHandler;
					 	}
					 }
			}
			this.Invalidate();
			this.Update();
		}
		
		protected override void OnMouseDown(MouseEventArgs e)
		{
		    base.OnMouseDown(e);
		    if(e.Button.Equals(MouseButtons.Left)) {
			    selecting = true;
			    selectionStart = new Point(e.X,e.Y);
			    selectionEnd = selectionStart;
			    this.UnselectAll();
			    this.propertyGrid.SelectedObject = null;
			    this.Invalidate();
			    this.Update();
		    }
		}
		
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			//this.Invalidate(new Region(new Rectangle(selectionStart.X-4,selectionStart.Y-4,
			//	selectionEnd.X-selectionStart.X+32,selectionEnd.Y-selectionStart.Y+32)));
			if(selecting) {
				int minX,minY,maxX,maxY;
				if (selectionStart.X < selectionEnd.X) {
					minX = selectionStart.X;
					maxX = selectionEnd.X;
				} else {
					minX = selectionEnd.X;
					maxX = selectionStart.X;
				}
				if (selectionStart.Y < selectionEnd.Y) {
					minY = selectionStart.Y;
					maxY = selectionEnd.Y;
				} else {
					minY = selectionEnd.Y;
					maxY = selectionStart.Y;
				}
				foreach(SoundItem sItem in this.soundItems) {
					if ((sItem.Location.X >= minX)
					    && (sItem.Location.Y >= minY)
					    && (sItem.Location.X + sItem.Width <= maxX)
					    && (sItem.Location.Y + sItem.Height <= maxY)) {
						sItem.Selected = true;
					} else {
						sItem.Selected = false;
					}
				}
				selectionEnd = new Point(e.X,e.Y);
				this.Invalidate();
				this.Update();
			}
		}
		
		protected override void OnMouseUp(MouseEventArgs e)
		{
		    base.OnMouseUp(e);
		    selecting = false;
		    this.Invalidate();
		    this.Update();
		}
		
		protected override void OnPaint(PaintEventArgs e)
		{
		    base.OnPaint(e);
		    
		    int numSnapsX = this.Width / snap.X;
		    int numSnapsY = this.Height / snap.Y;
		    int gridStartX = (0 - this.AutoScrollPosition.X) / snap.X;
		    int gridStartY = (0 - this.AutoScrollPosition.Y) / snap.Y;
		 
		    for (int y = gridStartY; y <= numSnapsY + gridStartY + 1; y++)
		    	for (int x = gridStartX; x <= numSnapsX + gridStartX + 1; x++) {
		    		e.Graphics.DrawImageUnscaled(onePixelBitmap,
		    	                             x*snap.X+this.AutoScrollPosition.X,
		    	                             y*snap.Y+this.AutoScrollPosition.Y);
		    	}
		    
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			foreach(SoundItem sItem in soundItems) {
				foreach(SoundItemLink sItemLink in sItem.SoundItemLinks) {
					
					if ((sItemLink.Type == SoundItemLink.SoundItemLinkType.MidiInput)
						|| (sItemLink.Type == SoundItemLink.SoundItemLinkType.SoundInput))
					if (sItemLink.Linked) {
						int x1,y1,x2,y2;
						x1 = sItemLink.Location.X+sItem.Location.X+sItemLink.Width/2;
						y1 = sItemLink.Location.Y+sItem.Location.Y+sItemLink.Height/2;
						x2 = sItemLink.LinkedSoundItemLink.Location.X
			                    +sItemLink.LinkedSoundItemLink.SoundItem.Location.X
			                    +sItemLink.LinkedSoundItemLink.Width/2;
						y2 = sItemLink.LinkedSoundItemLink.Location.Y
			                    +sItemLink.LinkedSoundItemLink.SoundItem.Location.Y
			                    +sItemLink.LinkedSoundItemLink.Height/2;
						if (sItemLink.Type == SoundItemLink.SoundItemLinkType.SoundInput)
							KnackGraphics.DrawLeftToRightArrow(e.Graphics,x2,y2,x1,y1);
						else
							KnackGraphics.DrawTopToBottomArrow(e.Graphics,x2,y2,x1,y1);
					}
				}
			}			
			if (linking) {
				KnackGraphics.DrawLinkingLine(e.Graphics,(int)linkX1,(int)linkY1,(int)linkX2,(int)linkY2);
			}
			if (selecting) {
				KnackGraphics.DrawSelection(e.Graphics,selectionStart.X,selectionStart.Y,selectionEnd.X,selectionEnd.Y);
			}
			//e.Graphics.SmoothingMode = SmoothingMode.Default;
		}
		
		Point snapToGrid(int x, int y)
		{			
			Point result = new Point();
			
			result.X = (int)Math.Round((double)(x + snap.X / 2) / snap.X) 
		    		* snap.X
		    		+ (AutoScrollPosition.X % snap.X);
			
			result.Y = (int)Math.Round((double)(y + snap.Y / 2) / snap.Y) 
		    		* snap.Y
		    		+ (AutoScrollPosition.Y % snap.Y);

			if (result.X < 0) result.X = 0;
		    if (result.Y < 0) result.Y = 0;
			
			return result;
		}
		
		public void SnapToGrid(Control c) {
			c.Location = snapToGrid(c.Location.X, c.Location.Y);
		}
		
		#region SoundItemEvents
		
		void EventSoundItemMouseDown(object sender, MouseEventArgs e)
		{
			SoundItem sItem = (SoundItem) sender;
			if (Control.ModifierKeys == Keys.Control) {
				status = SoundItemTreeStatus.MouseDownControl;
				sItem.Selected = !sItem.Selected;
			} else {
				status = SoundItemTreeStatus.MouseDown;
				draggingStart = new Point(e.X,e.Y);
				PropertyGrid.SelectedObject = sItem.SoundBlock;
				if (!sItem.Selected) {
					UnselectAll();
					sItem.Selected = true;
				}
			}
		}
		
		void EventSoundItemMouseMove(object sender, MouseEventArgs e)
		{
			if ((status == SoundItemTreeStatus.Dragging) || (status == SoundItemTreeStatus.MouseDown)) {
				SoundItem sItem = (SoundItem) sender;
		    	Point oldLocation = sItem.Location;
		    	Point newLocation = snapToGrid(sItem.Location.X + e.X - draggingStart.X,sItem.Location.Y + e.Y - draggingStart.Y);
		    	if (!oldLocation.Equals(newLocation)) {
		    		status = SoundItemTreeStatus.Dragging;
		    		foreach(SoundItem sIt in this.soundItems) {
		    			if (sIt.Selected) {
		    				sIt.Location = new Point(sIt.Location.X + newLocation.X - oldLocation.X,
		    				                        sIt.Location.Y + newLocation.Y - oldLocation.Y);
		    			}
		    		}
		    		Invalidate();
		    		Update();
		    	}
			}
		}
		
		void EventSoundItemMouseUp(object sender, MouseEventArgs e)
		{
			if (status == SoundItemTreeStatus.MouseDown) {
				status = SoundItemTreeStatus.Dragging;
				SoundItem sItem = (SoundItem) sender;
				UnselectAll();
				sItem.Selected = true;
			}
		    status = SoundItemTreeStatus.Default;
		}
		
		#endregion
	}

}
