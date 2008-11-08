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
using System.Threading;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
//using WinTab32;
using WinTablet;
using H3.Utils;

namespace H3.Sound.Wave.SoundRender
{
	/// <summary>
	/// Tablet wave sound render	
	/// </summary>
	public class TabletSoundRender : WaveSoundRender
	{
		//public enum FrequencyMappingType { Linear, Logarithmic } ;
		//FrequencyMappingType frequencyMapping = FrequencyMappingType.Linear;
		TabForm tabletForm;
		float frequencyMin = 0;
		float frequencyMax = 1500;
		float tabletX,tabletY,tabletPressure;
		double oldFrequency;
		double amplitude = 0.0;
		
		public TabletSoundRender() : base()
		{  
			this.editable = true;
		}
		
		public TabletSoundRender(double freq) : base(freq)
		{  }
		/*
		protected override System.Drawing.Image LoadIcon() {
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(SoundBlock));
			return (System.Drawing.Image) resources.GetObject("sine");
		}
		*/
		
		[CategoryAttribute("Frequency Range"), DescriptionAttribute("Sound frequency range.")]
		public float FrequencyMin {
			get { return frequencyMin; }
			set { frequencyMin = value; }
		}
		
		[CategoryAttribute("Frequency Range"), DescriptionAttribute("Sound frequency range.")]
		public float FrequencyMax {
			get { return frequencyMax; }
			set { frequencyMax = value; }
		}
		
		/*
		[CategoryAttribute("Frequency Mapping"), DescriptionAttribute("Sound frequency mapping.")]
		public FrequencyMappingType FrequencyMapping {
			get { return frequencyMapping; }
			set { frequencyMapping = value; }
		}
		*/
		
		public void SetTabletX(float x) {
			this.tabletX = x;
		}
		
		public void SetTabletY(float y) {
			this.tabletY = y;
		}
		
		public void SetTabletPressure(float pressure) {
			this.tabletPressure = pressure;
		}
		
		public override void Render(float[] leftChannel,float[] rightChannel)
		{
			int channelSize = leftChannel.Length;
			double squarefactor,sinefactor,sinus,square;
			double frequency;
			
			for (int i=0; i<channelSize; i++){  
				
				// This way frequency changes are more smooth:
				freq = (freq * 0.999)  + (frequencyMax - (tabletY * frequencyMax) + frequencyMin) * 0.001;
				
				frequency = this.FrequencyToScaledFrequency(freq);
				if (frequency <= 0) frequency = double.Epsilon;
				
				if (frequency != oldFrequency) {
					pos *= oldFrequency / frequency;
					oldFrequency = frequency;
				}
				
				squarefactor = tabletX;
				sinefactor = 1.0 - squarefactor;

				sinus = Math.Sin(pos * frequency);
				
				square = 1.0;
				if (sinus < 0.0) square = -1.0;
				
				// This way pressure changes are more smooth:
				amplitude = amplitude * 0.999f + tabletPressure * 0.001f;
				
				rightChannel[i] = 
					leftChannel[i] = 
						(float) ((sinus * sinefactor
					          + (square * squarefactor)) * amplitude);
		
				pos += posIncrement;
			}
		}
		
		public override void OnEdit() 
		{
			try {
				tabletForm = new TabForm(this);
				tabletForm.Text = this.Name;
				//tabletForm.Show();
				tabletForm.ShowDialog();
				tabletForm.Dispose();
			} catch (TabletNotFoundException e) {
				MessageBox.Show(e.Message, "Tablet not found", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
			} catch (TabletNotInstalledException e) {
				MessageBox.Show(e.Message, "Tablet not installed", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
			}
		}
		
		public new double NoteToScaledFrequency(double note)
		{
			return base.NoteToScaledFrequency(note);
		}
	}
	
	public class TabForm : PacketQueueTabletForm
	{
		Thread PacketQueueThread;
		TabletSoundRender tabletSoundRender;
		
		public TabForm(TabletSoundRender tabletSoundRender) : base() 
		{
			this.tabletSoundRender = tabletSoundRender;
			this.BackColor = System.Drawing.Color.Black;
			this.Cursor = Cursors.Cross;
			this.FormBorderStyle =  FormBorderStyle.None;
			this.WindowState = FormWindowState.Normal;
			this.Top = Screen.PrimaryScreen.Bounds.Top;
			this.Left = Screen.PrimaryScreen.Bounds.Left;
			this.Width = Screen.PrimaryScreen.Bounds.Width;
			this.Height = Screen.PrimaryScreen.Bounds.Height;
			this.ShowInTaskbar = true;
			this.Activated += new System.EventHandler(HandleActivate);
			this.Move += new System.EventHandler(HandleMove);
			this.TopMost = true;
			PacketQueueThread = new Thread(new ThreadStart(PacketQueueThreadStart));
			PacketQueueThread.Name = "PacketQueueThread";
			PacketQueueThread.Priority = ThreadPriority.AboveNormal;
			PacketQueueThread.IsBackground = true;
			PacketQueueThread.Start();
		}
		
		void PacketQueueThreadStart()
		{
			while(true) {
				TabletPacketEventArgs e = Dequeue();
				//Logger.Instance.Log("packet: X="+e.AbsoluteX+" Y="+e.AbsoluteY+" Press="+e.NormalPressure);
				this.tabletSoundRender.SetTabletX(e.AbsoluteX);
				this.tabletSoundRender.SetTabletY(e.AbsoluteY);
				this.tabletSoundRender.SetTabletPressure(e.NormalPressure);	
			}
		}	
		
		void HandleActivate(object sender, System.EventArgs e)
		{
			this.WindowState = FormWindowState.Maximized;
		}
		
		void HandleMove(object sender, System.EventArgs e)
		{
			this.Invalidate();
		}
		
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == Keys.Escape) {
				this.Close();
				return true;
			}
			return false;
		}
		
		protected override void OnPaint(PaintEventArgs e) {
			base.OnPaint(e);
			System.Drawing.Font myFont = new System.Drawing.Font("Verdana", 10);
			System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(Color.LightGray);
			System.Drawing.Pen myPen = new System.Drawing.Pen(Color.Gray);
			for (int note = 0; note < 20000; note++) {
				double y = Screen.PrimaryScreen.Bounds.Height -
					((this.tabletSoundRender.NoteToScaledFrequency(note) - this.tabletSoundRender.FrequencyMin)
					* Screen.PrimaryScreen.Bounds.Height
					/ (this.tabletSoundRender.FrequencyMax - this.tabletSoundRender.FrequencyMin));
				PointF relPos = this.PointToClient(new System.Drawing.Point(0,(int)Math.Round(y)));
				e.Graphics.DrawLine(myPen,0,relPos.Y,this.Width,relPos.Y);
				if (relPos.Y<0) break;
			}
			e.Graphics.DrawString("Press ESC to close this window",myFont,myBrush,10,10);
		}
		
		protected override void Log(string logline) {
			Logger.Instance.Log(logline);
		}
	}
}
