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
using System.Drawing.Drawing2D;

using H3.Utils;
using H3.Sound;
using H3.Sound.Wave;
using H3.Sound.Wave.SoundRender;
using H3.Sound.Midi;
using H3.Sound.Midi.MidiRender;

namespace H3.Knack.UI
{
	/// <summary>
	/// The main Knack Form
	/// </summary>
	public class KnackForm : System.Windows.Forms.Form
	{
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.Panel BlocksPanel;
		private System.Windows.Forms.MenuItem menuItemFileNew;
		private System.Windows.Forms.PropertyGrid BlockPropertyGrid;
		private System.Windows.Forms.Panel panelTop;
		private System.Windows.Forms.OpenFileDialog openKNKFileDialog;
		private System.Windows.Forms.MenuItem menuItemAbout;
		private System.Windows.Forms.MenuItem menuItemFileExit;
		private System.Windows.Forms.StatusBar statusBar;
		private System.Windows.Forms.Panel PropertyPanel;
		private System.Windows.Forms.SaveFileDialog saveKNKFileDialog;
		private System.Windows.Forms.MainMenu mainMenu;
		private System.Windows.Forms.MenuItem menuItem6;
		private System.Windows.Forms.Label BlockPropertyLabel;
		private System.Windows.Forms.ToolBarButton toolBarFileNew;
		private System.Windows.Forms.MenuItem menuItemFileSaveAs;
		private System.Windows.Forms.MenuItem menuItemHelp;
		private System.Windows.Forms.ToolBarButton toolBarFileSave;
		private System.Windows.Forms.Panel PanelProperyGridInside;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.ToolBarButton toolBarFileOpen;
		private System.Windows.Forms.MenuItem menuItemFileSave;
		private System.Windows.Forms.Panel PanelTopProperty;
		private System.Windows.Forms.MenuItem menuItemFileOpen;
		private System.Windows.Forms.MenuItem menuItemFile;
		private System.Windows.Forms.ImageList imageList;
		private System.Windows.Forms.ToolBar mainToolBar;
		
		private SoundItemTree sItemTree;
		private System.Drawing.Point TreeContextMenuLocation = new System.Drawing.Point(0,0);
		private System.Windows.Forms.ContextMenu TreeContextMenu = new System.Windows.Forms.ContextMenu();
		
		private class MenuItemNewSoundBlock : MenuItem
		{
			private Type sbType;
			
			public MenuItemNewSoundBlock(string text,EventHandler onClick,Type sbType) : base(text,onClick)
			{ 
				this.sbType = sbType;
			}
			
			public Type SoundBlockType {
				get { return sbType; }
			}
		}
		
		public KnackForm()
		{
			// The InitializeComponent() call is required for Windows Forms designer support.
			InitializeComponent();
			
			//
			// Add constructor code after the InitializeComponent() call.
			//
			sItemTree = new SoundItemTree(this,this.BlockPropertyGrid);
			sItemTree.Dock = System.Windows.Forms.DockStyle.Fill;
			sItemTree.ContextMenu = this.TreeContextMenu;
			this.BlocksPanel.Controls.Add(sItemTree);
			this.Width = (int) (SystemInformation.PrimaryMonitorSize.Width * 0.85);
			this.Height = (int) (SystemInformation.PrimaryMonitorSize.Height * 0.85);
			
			{
				// Toolbar background shading
				Bitmap bgBitmap = new Bitmap(10,panelTop.Height);
				Rectangle bgRect = new Rectangle(0,0,bgBitmap.Width,bgBitmap.Height);
				LinearGradientBrush bgBrush = new LinearGradientBrush(bgRect,
	         		Color.LightGray,this.BackColor,90f);
				Graphics g = Graphics.FromImage(bgBitmap);
				g.FillRectangle (bgBrush, bgRect);
				this.panelTop.BackgroundImage = bgBitmap;
			}
			// 
			// TreeContextMenu
			// 
			EventHandler newSBHandler = new EventHandler(this.NewSoundBlockClick);
			MenuItem menuItemNewInput = new MenuItem("New Input");
			MenuItem menuItemNewOutput = new MenuItem("New Output");
			MenuItem menuItemNewWave = new MenuItem("New Wave");
			MenuItem menuItemNewOperator = new MenuItem("New Operator");
			MenuItem menuItemNewEnvelope = new MenuItem("New Envelope");
			MenuItem menuItemNewEffect = new MenuItem("New Effect");
			MenuItem menuItemNewInstrument = new MenuItemNewSoundBlock("New Instrument",newSBHandler,typeof(H3.Sound.Wave.SoundRender.InstrumentSoundRender));
			MenuItem menuItemNewMidiIn = new MenuItemNewSoundBlock("Midi In",newSBHandler,typeof(H3.Sound.Midi.MidiRender.MidiInRender));
			MenuItem menuItemNewMidiEditor = new MenuItemNewSoundBlock("Midi Editor",newSBHandler,typeof(H3.Sound.Midi.MidiRender.MidiEditorRender));
			MenuItem menuItemNewMidiRandom = new MenuItemNewSoundBlock("Midi Random",newSBHandler,typeof(H3.Sound.Midi.MidiRender.RandomMidiRender));
			MenuItem menuItemNewSoundOut = new MenuItemNewSoundBlock("Sound Out",newSBHandler,typeof(H3.Sound.Wave.SoundRender.SoundOutRender));
			MenuItem menuItemNewWaveSine = new MenuItemNewSoundBlock("Sine",newSBHandler,typeof(H3.Sound.Wave.SoundRender.SineSoundRender));
			MenuItem menuItemNewWaveSquare = new MenuItemNewSoundBlock("Square",newSBHandler,typeof(H3.Sound.Wave.SoundRender.SquareSoundRender));
			MenuItem menuItemNewWaveSawtooth = new MenuItemNewSoundBlock("Sawtooth",newSBHandler,typeof(H3.Sound.Wave.SoundRender.SawtoothSoundRender));
			MenuItem menuItemNewWaveTriangle = new MenuItemNewSoundBlock("Triangle",newSBHandler,typeof(H3.Sound.Wave.SoundRender.TriangleSoundRender));
	    #if PLATFORM_WIN32
			MenuItem menuItemNewWaveTablet = new MenuItemNewSoundBlock("Tablet",newSBHandler,typeof(H3.Sound.Wave.SoundRender.TabletSoundRender));
		#endif
			MenuItem menuItemNewOperatorAdd = new MenuItemNewSoundBlock("Add",newSBHandler,typeof(H3.Sound.Wave.SoundRender.AddOperatorSoundRender));
			MenuItem menuItemNewOperatorSubtract = new MenuItemNewSoundBlock("Subtract",newSBHandler,typeof(H3.Sound.Wave.SoundRender.SubtractOperatorSoundRender));
			MenuItem menuItemNewOperatorMultiply = new MenuItemNewSoundBlock("Multiply",newSBHandler,typeof(H3.Sound.Wave.SoundRender.MultiplyOperatorSoundRender));
			MenuItem menuItemNewEnvelopeADSR = new MenuItemNewSoundBlock("ADSR",newSBHandler,typeof(H3.Sound.Wave.SoundRender.AdsrSoundRender));
			MenuItem menuItemNewEffectDelay = new MenuItemNewSoundBlock("Delay",newSBHandler,typeof(H3.Sound.Wave.SoundRender.DelayEffectSoundRender));
			MenuItem menuItemNewEffectMidiFilter = new MenuItemNewSoundBlock("Midi Filter",newSBHandler,typeof(H3.Sound.Midi.MidiRender.MidiFilterRender));
			
			menuItemNewInput.MenuItems.AddRange(new MenuItem[] {
			#if PLATFORM_WIN32
			            menuItemNewWaveTablet,
			#endif
						menuItemNewMidiIn,
						menuItemNewMidiEditor,
						menuItemNewMidiRandom});
			
			menuItemNewOutput.MenuItems.AddRange(new MenuItem[] {
						menuItemNewSoundOut});
			
			menuItemNewWave.MenuItems.AddRange(new MenuItem[] {
						menuItemNewWaveSine,
						menuItemNewWaveSquare,
						menuItemNewWaveSawtooth,
						menuItemNewWaveTriangle});
			
			menuItemNewOperator.MenuItems.AddRange(new MenuItem[] {
						menuItemNewOperatorAdd,
						menuItemNewOperatorSubtract,
			            menuItemNewOperatorMultiply});
			
			menuItemNewEnvelope.MenuItems.AddRange(new MenuItem[] {
						menuItemNewEnvelopeADSR});
			
			menuItemNewEffect.MenuItems.AddRange(new MenuItem[] {
						menuItemNewEffectDelay,
			            menuItemNewEffectMidiFilter});
			
			this.TreeContextMenu.MenuItems.AddRange(new MenuItem[] {
						menuItemNewWave,
						menuItemNewOperator,
						menuItemNewEnvelope,
						menuItemNewEffect,
						menuItemNewInstrument,
			            menuItemNewInput,
			            menuItemNewOutput});
			
			this.TreeContextMenu.Popup += new System.EventHandler(this.TreeContextMenuPopup);
		}
		
		#region Windows Forms Designer generated code
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KnackForm));
			this.mainToolBar = new System.Windows.Forms.ToolBar();
			this.toolBarFileNew = new System.Windows.Forms.ToolBarButton();
			this.toolBarFileOpen = new System.Windows.Forms.ToolBarButton();
			this.toolBarFileSave = new System.Windows.Forms.ToolBarButton();
			this.imageList = new System.Windows.Forms.ImageList(this.components);
			this.menuItemFile = new System.Windows.Forms.MenuItem();
			this.menuItemFileNew = new System.Windows.Forms.MenuItem();
			this.menuItemFileOpen = new System.Windows.Forms.MenuItem();
			this.menuItemFileSave = new System.Windows.Forms.MenuItem();
			this.menuItemFileSaveAs = new System.Windows.Forms.MenuItem();
			this.menuItem6 = new System.Windows.Forms.MenuItem();
			this.menuItemFileExit = new System.Windows.Forms.MenuItem();
			this.PanelTopProperty = new System.Windows.Forms.Panel();
			this.BlockPropertyLabel = new System.Windows.Forms.Label();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.PanelProperyGridInside = new System.Windows.Forms.Panel();
			this.BlockPropertyGrid = new System.Windows.Forms.PropertyGrid();
			this.menuItemHelp = new System.Windows.Forms.MenuItem();
			this.menuItemAbout = new System.Windows.Forms.MenuItem();
			this.mainMenu = new System.Windows.Forms.MainMenu(this.components);
			this.saveKNKFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.PropertyPanel = new System.Windows.Forms.Panel();
			this.statusBar = new System.Windows.Forms.StatusBar();
			this.openKNKFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.panelTop = new System.Windows.Forms.Panel();
			this.BlocksPanel = new System.Windows.Forms.Panel();
			this.PanelTopProperty.SuspendLayout();
			this.PanelProperyGridInside.SuspendLayout();
			this.PropertyPanel.SuspendLayout();
			this.panelTop.SuspendLayout();
			this.SuspendLayout();
			// 
			// mainToolBar
			// 
			this.mainToolBar.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
			this.mainToolBar.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
									this.toolBarFileNew,
									this.toolBarFileOpen,
									this.toolBarFileSave});
			this.mainToolBar.DropDownArrows = true;
			this.mainToolBar.ImageList = this.imageList;
			this.mainToolBar.Location = new System.Drawing.Point(0, 0);
			this.mainToolBar.Name = "mainToolBar";
			this.mainToolBar.ShowToolTips = true;
			this.mainToolBar.Size = new System.Drawing.Size(616, 30);
			this.mainToolBar.TabIndex = 0;
			this.mainToolBar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.MainToolBarButtonClick);
			// 
			// toolBarFileNew
			// 
			this.toolBarFileNew.ImageIndex = 0;
			this.toolBarFileNew.Name = "toolBarFileNew";
			this.toolBarFileNew.ToolTipText = "New file";
			// 
			// toolBarFileOpen
			// 
			this.toolBarFileOpen.ImageIndex = 1;
			this.toolBarFileOpen.Name = "toolBarFileOpen";
			this.toolBarFileOpen.ToolTipText = "Open file..";
			// 
			// toolBarFileSave
			// 
			this.toolBarFileSave.ImageIndex = 2;
			this.toolBarFileSave.Name = "toolBarFileSave";
			this.toolBarFileSave.ToolTipText = "Save file...";
			// 
			// imageList
			// 
			this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
			this.imageList.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList.Images.SetKeyName(0, "new.png");
			this.imageList.Images.SetKeyName(1, "open.png");
			this.imageList.Images.SetKeyName(2, "save.png");
			// 
			// menuItemFile
			// 
			this.menuItemFile.Index = 0;
			this.menuItemFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
									this.menuItemFileNew,
									this.menuItemFileOpen,
									this.menuItemFileSave,
									this.menuItemFileSaveAs,
									this.menuItem6,
									this.menuItemFileExit});
			this.menuItemFile.Text = "File";
			// 
			// menuItemFileNew
			// 
			this.menuItemFileNew.Index = 0;
			this.menuItemFileNew.Text = "New";
			this.menuItemFileNew.Click += new System.EventHandler(this.MenuItemFileNewClick);
			// 
			// menuItemFileOpen
			// 
			this.menuItemFileOpen.Index = 1;
			this.menuItemFileOpen.Text = "Open...";
			this.menuItemFileOpen.Click += new System.EventHandler(this.MenuItemFileOpenClick);
			// 
			// menuItemFileSave
			// 
			this.menuItemFileSave.Index = 2;
			this.menuItemFileSave.Text = "Save";
			this.menuItemFileSave.Click += new System.EventHandler(this.MenuItemFileSaveClick);
			// 
			// menuItemFileSaveAs
			// 
			this.menuItemFileSaveAs.Index = 3;
			this.menuItemFileSaveAs.Text = "Save As...";
			this.menuItemFileSaveAs.Click += new System.EventHandler(this.MenuItemFileSaveAsClick);
			// 
			// menuItem6
			// 
			this.menuItem6.Index = 4;
			this.menuItem6.Text = "-";
			// 
			// menuItemFileExit
			// 
			this.menuItemFileExit.Index = 5;
			this.menuItemFileExit.Text = "Exit";
			this.menuItemFileExit.Click += new System.EventHandler(this.MenuItemFileExitClick);
			// 
			// PanelTopProperty
			// 
			this.PanelTopProperty.Controls.Add(this.BlockPropertyLabel);
			this.PanelTopProperty.Dock = System.Windows.Forms.DockStyle.Top;
			this.PanelTopProperty.Location = new System.Drawing.Point(0, 0);
			this.PanelTopProperty.Name = "PanelTopProperty";
			this.PanelTopProperty.Padding = new System.Windows.Forms.Padding(0, 8, 0, 6);
			this.PanelTopProperty.Size = new System.Drawing.Size(192, 24);
			this.PanelTopProperty.TabIndex = 3;
			// 
			// BlockPropertyLabel
			// 
			this.BlockPropertyLabel.AutoSize = true;
			this.BlockPropertyLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.BlockPropertyLabel.Location = new System.Drawing.Point(0, 8);
			this.BlockPropertyLabel.Name = "BlockPropertyLabel";
			this.BlockPropertyLabel.Size = new System.Drawing.Size(160, 13);
			this.BlockPropertyLabel.TabIndex = 3;
			this.BlockPropertyLabel.Text = "Selected SoundBlock Properties";
			// 
			// splitter1
			// 
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Right;
			this.splitter1.Location = new System.Drawing.Point(420, 26);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(4, 443);
			this.splitter1.TabIndex = 4;
			this.splitter1.TabStop = false;
			// 
			// PanelProperyGridInside
			// 
			this.PanelProperyGridInside.Controls.Add(this.BlockPropertyGrid);
			this.PanelProperyGridInside.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PanelProperyGridInside.Location = new System.Drawing.Point(0, 24);
			this.PanelProperyGridInside.Name = "PanelProperyGridInside";
			this.PanelProperyGridInside.Padding = new System.Windows.Forms.Padding(0, 4, 4, 4);
			this.PanelProperyGridInside.Size = new System.Drawing.Size(192, 419);
			this.PanelProperyGridInside.TabIndex = 4;
			// 
			// BlockPropertyGrid
			// 
			this.BlockPropertyGrid.CommandsBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(237)))));
			this.BlockPropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BlockPropertyGrid.LineColor = System.Drawing.SystemColors.ScrollBar;
			this.BlockPropertyGrid.Location = new System.Drawing.Point(0, 4);
			this.BlockPropertyGrid.Name = "BlockPropertyGrid";
			this.BlockPropertyGrid.Size = new System.Drawing.Size(188, 411);
			this.BlockPropertyGrid.TabIndex = 2;
			this.BlockPropertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.BlockPropertyGridPropertyValueChanged);
			// 
			// menuItemHelp
			// 
			this.menuItemHelp.Index = 1;
			this.menuItemHelp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
									this.menuItemAbout});
			this.menuItemHelp.Text = "Help";
			// 
			// menuItemAbout
			// 
			this.menuItemAbout.Index = 0;
			this.menuItemAbout.Text = "About Knack...";
			this.menuItemAbout.Click += new System.EventHandler(this.MenuItemAboutClick);
			// 
			// mainMenu
			// 
			this.mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
									this.menuItemFile,
									this.menuItemHelp});
			// 
			// saveKNKFileDialog
			// 
			this.saveKNKFileDialog.DefaultExt = "knk";
			this.saveKNKFileDialog.Filter = "Knack files|*.knk|All files|*.*";
			// 
			// PropertyPanel
			// 
			this.PropertyPanel.AutoScroll = true;
			this.PropertyPanel.Controls.Add(this.PanelProperyGridInside);
			this.PropertyPanel.Controls.Add(this.PanelTopProperty);
			this.PropertyPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.PropertyPanel.Location = new System.Drawing.Point(424, 26);
			this.PropertyPanel.Name = "PropertyPanel";
			this.PropertyPanel.Size = new System.Drawing.Size(192, 443);
			this.PropertyPanel.TabIndex = 2;
			// 
			// statusBar
			// 
			this.statusBar.Location = new System.Drawing.Point(0, 469);
			this.statusBar.Name = "statusBar";
			this.statusBar.Size = new System.Drawing.Size(616, 22);
			this.statusBar.TabIndex = 1;
			this.statusBar.Text = "Ready.";
			// 
			// openKNKFileDialog
			// 
			this.openKNKFileDialog.DefaultExt = "knk";
			this.openKNKFileDialog.Filter = "Knack files|*.knk|All files|*.*";
			// 
			// panelTop
			// 
			this.panelTop.Controls.Add(this.mainToolBar);
			this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelTop.ForeColor = System.Drawing.Color.DarkSlateGray;
			this.panelTop.Location = new System.Drawing.Point(0, 0);
			this.panelTop.Name = "panelTop";
			this.panelTop.Size = new System.Drawing.Size(616, 26);
			this.panelTop.TabIndex = 0;
			// 
			// BlocksPanel
			// 
			this.BlocksPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BlocksPanel.Location = new System.Drawing.Point(0, 26);
			this.BlocksPanel.Name = "BlocksPanel";
			this.BlocksPanel.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
			this.BlocksPanel.Size = new System.Drawing.Size(424, 443);
			this.BlocksPanel.TabIndex = 3;
			// 
			// KnackForm
			// 
			this.AllowDrop = true;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(616, 491);
			this.Controls.Add(this.splitter1);
			this.Controls.Add(this.BlocksPanel);
			this.Controls.Add(this.PropertyPanel);
			this.Controls.Add(this.statusBar);
			this.Controls.Add(this.panelTop);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Menu = this.mainMenu;
			this.MinimumSize = new System.Drawing.Size(320, 240);
			this.Name = "KnackForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
			this.Text = "Knack";
			this.DragDrop += new System.Windows.Forms.DragEventHandler(this.KnackFormDragDrop);
			this.DragEnter += new System.Windows.Forms.DragEventHandler(this.KnackFormDragEnter);
			this.PanelTopProperty.ResumeLayout(false);
			this.PanelTopProperty.PerformLayout();
			this.PanelProperyGridInside.ResumeLayout(false);
			this.PropertyPanel.ResumeLayout(false);
			this.panelTop.ResumeLayout(false);
			this.panelTop.PerformLayout();
			this.ResumeLayout(false);
		}
		#endregion
		
		#region TreeContextMenuHandlers
		/// <summary>
		/// Handlers for the menu...
		/// </summary>
		
		void TreeContextMenuPopup(object sender, System.EventArgs e)
		{
			TreeContextMenuLocation = new System.Drawing.Point(
				Control.MousePosition.X-this.Left-sItemTree.Left-32,
				Control.MousePosition.Y-this.Top-sItemTree.Top-90);
		}
		
		void NewSoundBlockClick(object sender, System.EventArgs e)
		{
			if  (sender is MenuItemNewSoundBlock) {
				MenuItemNewSoundBlock senderMI = (MenuItemNewSoundBlock) sender;
				Console.WriteLine("\""+senderMI.Text+"\" -> "+senderMI.SoundBlockType.ToString());
				SoundBlock sb = (SoundBlock) senderMI.SoundBlockType.GetConstructor(new Type[0]).Invoke(new object[0]);
				NewSoundItem(sb,TreeContextMenuLocation);
			}
		}
		
		#endregion
		
		void NewSoundItem(SoundBlock sb, System.Drawing.Point location) {
			int numSoundOutputs = (sb is ISoundRender) ? 1 : 0;
			int numMidiOutputs = (sb is IMidiRender) ? 1 : 0;
			if (sb is SoundOutRender) numSoundOutputs = 0;
			SoundItem sItem = new SoundItem(sItemTree,sb,sb.SoundInputs.Length,numSoundOutputs,sb.MidiInputs.Length,numMidiOutputs);
			sItem.Location = location;
			sItemTree.AddSoundItem(sItem);
		}
		
		void BlockPropertyGridPropertyValueChanged(object s, System.Windows.Forms.PropertyValueChangedEventArgs e)
		{
			if (e.ChangedItem.Label.Equals("Name")) {
				int equalNames = 0;
				foreach(SoundItem si in sItemTree.SoundItems) {
					if (si.Name.Equals(e.ChangedItem.Value.ToString())) {
						equalNames++;
					}
				}
				if(equalNames>1) {
					MessageBox.Show(this,"The Name you wrote already exists.\nEvery block must have a unique Name.", "Warning", 
						MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
				this.sItemTree.Refresh();
			}
		}
		
		private string RemovePathFromFilename(string filename)
		{
			return filename.Remove(0,filename.LastIndexOf('\\')+1);
		}
		
		public void FileNew()
		{
			Logger.Instance.Log("New file...");
			this.Enabled = false;
			this.statusBar.Text = "New file...";
			sItemTree.Clear();
			saveKNKFileDialog.FileName = "";
			this.Text = "Knack";
			this.Enabled = true;
			this.statusBar.Text = "Ready.";
		}
		
		public void FileLoad(string filename)
		{
			FileNew();
			Logger.Instance.Log("Load file \""+filename+"\"...");
			this.Enabled = false;
			this.statusBar.Text = "Loading file...";
			this.openKNKFileDialog.FileName = filename;
			KnackFile knkFile = new KnackFile(filename);
			foreach(KnackFile.SoundBlockData sbData in knkFile.SoundBlockList) {
				NewSoundItem(sbData.SoundBlock,new System.Drawing.Point(sbData.X,sbData.Y));
			}
			foreach(KnackFile.SoundBlockData sbData in knkFile.SoundBlockList) {
				for(int i=0; i<sbData.SoundInputs.Count; i++) {
					string linkedSB = (string) sbData.SoundInputs[i];
					//Console.WriteLine("linking "+sbData.SoundBlock.Name+" SoundInput["+i+"] to "+linkedSB);
					SoundItem siProducer,siConsumer;
					siConsumer = this.sItemTree.GetSoundItem(sbData.SoundBlock.Name);
					siProducer = this.sItemTree.GetSoundItem(linkedSB);
					SoundItemLink siConsumerLink = (SoundItemLink) siConsumer.GetSoundItemLink(SoundItemLink.SoundItemLinkType.SoundInput,i);
					SoundItemLink siProducerLink = (SoundItemLink) siProducer.GetSoundItemLink(SoundItemLink.SoundItemLinkType.SoundOutput,0);
					siConsumerLink.Link(siProducerLink);
				 	sbData.SoundBlock.SoundInputs[i] = siProducerLink.SoundItem.SoundRender;
				}
				for(int i=0; i<sbData.MidiInputs.Count; i++) {
					string linkedSB = (string) sbData.MidiInputs[i];
					//Console.WriteLine("linking "+sbData.SoundBlock.Name+" MidiInput["+i+"] to "+linkedSB);
					SoundItem siProducer,siConsumer;
					siConsumer = this.sItemTree.GetSoundItem(sbData.SoundBlock.Name);
					siProducer = this.sItemTree.GetSoundItem(linkedSB);
					SoundItemLink siConsumerLink = (SoundItemLink) siConsumer.GetSoundItemLink(SoundItemLink.SoundItemLinkType.MidiInput,i);
					SoundItemLink siProducerLink = (SoundItemLink) siProducer.GetSoundItemLink(SoundItemLink.SoundItemLinkType.MidiOutput,0);
					siConsumerLink.Link(siProducerLink);
					sbData.SoundBlock.MidiInputs[i] = siProducerLink.SoundItem.MidiRender;
					IMidiRender midiRender = (IMidiRender) siProducerLink.SoundItem.MidiRender;	
					IMidiHandler midiHandler = (IMidiHandler) sbData.SoundBlock;
					midiRender.OnMidiMessage += midiHandler.MidiMessageHandler;
				}
			}
			saveKNKFileDialog.FileName = openKNKFileDialog.FileName;
			this.Text = "Knack - "+RemovePathFromFilename(openKNKFileDialog.FileName);
			this.Enabled = true;
			this.statusBar.Text = "Ready.";
			this.sItemTree.Invalidate();
		}
		
		void MenuItemFileNewClick(object sender, System.EventArgs e)
		{
			FileNew();
		}
		
		void MenuItemFileOpenClick(object sender, System.EventArgs e)
		{
			if(this.openKNKFileDialog.ShowDialog() == DialogResult.OK)
		   		FileLoad(openKNKFileDialog.FileName);
		}
		
		void MenuItemFileSaveClick(object sender, System.EventArgs e)
		{
			if (saveKNKFileDialog.FileName.Equals(""))
				MenuItemFileSaveAsClick(sender,e);
			else {
				Logger.Instance.Log("Save file \""+saveKNKFileDialog.FileName+"\"...");
				this.Enabled = false;
				this.statusBar.Text = "Saving file...";
				KnackFile knkFile = new KnackFile();
				ArrayList SoundItems = sItemTree.SoundItems;
				for (int i=0; i<SoundItems.Count; i++) {
					SoundItem sItem = (SoundItem) SoundItems[i];
					SoundBlock sBlock = (SoundBlock) sItem.SoundRender;
					if (sBlock == null) sBlock = (SoundBlock) sItem.MidiRender;
					knkFile.Add(sBlock,
					            sItem.Location.X - sItemTree.AutoScrollPosition.X,
					            sItem.Location.Y - sItemTree.AutoScrollPosition.Y);
				}
				knkFile.Save(saveKNKFileDialog.FileName);
				this.Text = "Knack - "+RemovePathFromFilename(saveKNKFileDialog.FileName);
				this.Enabled = true;
				this.statusBar.Text = "Ready.";
			}
		}
		
		void MenuItemFileSaveAsClick(object sender, System.EventArgs e)
		{
			if(this.saveKNKFileDialog.ShowDialog() == DialogResult.OK)
		   		MenuItemFileSaveClick(sender,e);
		}
		
		void MenuItemFileExitClick(object sender, System.EventArgs e)
		{
			this.Close();
		}	
		
		void MenuItemAboutClick(object sender, System.EventArgs e)
		{
			AboutForm aboutForm = new AboutForm();
			aboutForm.ShowDialog(this);
		}
		
		void MainToolBarButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			switch (e.Button.ImageIndex) {
				case 0: MenuItemFileNewClick(sender,e); break;
				case 1: MenuItemFileOpenClick(sender,e); break;
				case 2: MenuItemFileSaveClick(sender,e); break;
			}
			
		}
		
		void KnackFormDragEnter(object sender, System.Windows.Forms.DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop,false))
				e.Effect = DragDropEffects.All;
		}
		
		void KnackFormDragDrop(object sender, System.Windows.Forms.DragEventArgs e)
		{
			string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

			if (files.Length == 1) {
				FileLoad(files[0]);
			} else if (files.Length>1) {
				MessageBox.Show(this,"You can't drag more then one file over the window", "Drag and drop", 
						MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}
		
	}	
	
}
