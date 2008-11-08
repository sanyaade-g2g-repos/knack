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
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;

using WinTab32;

namespace TabletFormTest
{
	/// <summary>
	/// A PacketQueueTabletForm test : a simple painting application.
	/// </summary>
	public class TestPacketQueueForm : PacketQueueTabletForm
	{
		private System.Windows.Forms.Button buttonNew;
		private System.Windows.Forms.SaveFileDialog saveFileDialog;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBoxLog;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button buttonSaveAs;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.Panel panel4;
		private System.Windows.Forms.PictureBox pictureBox;
		
		Bitmap bitmap;
		Graphics graphics;
		SolidBrush myBrush = new SolidBrush(Color.FromArgb(128,23,56,78));
		Thread PacketQueueThread;
			
		public TestPacketQueueForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// Add constructor code after the InitializeComponent() call.
			//
			bitmap = new Bitmap(SystemInformation.PrimaryMonitorSize.Width,
			                    SystemInformation.PrimaryMonitorSize.Height,
			                    PixelFormat.Format32bppArgb);
			
			graphics = Graphics.FromImage(bitmap);
			graphics.FillRectangle(new SolidBrush(Color.White),0,0,bitmap.Width,bitmap.Height);
			graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			this.pictureBox.Image = bitmap;
			
			PacketQueueThread = new Thread(new ThreadStart(PacketQueueThreadStart));
			PacketQueueThread.Name = "PacketQueueThread";
			PacketQueueThread.Priority = ThreadPriority.AboveNormal;
			PacketQueueThread.IsBackground = true;
			PacketQueueThread.Start();
		}
		
		#region Windows Forms Designer generated code
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent() {
			this.pictureBox = new System.Windows.Forms.PictureBox();
			this.panel4 = new System.Windows.Forms.Panel();
			this.panel3 = new System.Windows.Forms.Panel();
			this.buttonSaveAs = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.panel2 = new System.Windows.Forms.Panel();
			this.textBoxLog = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.buttonNew = new System.Windows.Forms.Button();
			this.panel4.SuspendLayout();
			this.panel3.SuspendLayout();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// pictureBox
			// 
			this.pictureBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.pictureBox.Cursor = System.Windows.Forms.Cursors.Cross;
			this.pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pictureBox.Location = new System.Drawing.Point(0, 0);
			this.pictureBox.Name = "pictureBox";
			this.pictureBox.Size = new System.Drawing.Size(462, 281);
			this.pictureBox.TabIndex = 1;
			this.pictureBox.TabStop = false;
			// 
			// panel4
			// 
			this.panel4.Controls.Add(this.buttonSaveAs);
			this.panel4.Controls.Add(this.buttonNew);
			this.panel4.Dock = System.Windows.Forms.DockStyle.Right;
			this.panel4.Location = new System.Drawing.Point(398, 0);
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(64, 281);
			this.panel4.TabIndex = 2;
			// 
			// panel3
			// 
			this.panel3.Controls.Add(this.panel4);
			this.panel3.Controls.Add(this.pictureBox);
			this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel3.Location = new System.Drawing.Point(5, 5);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(462, 281);
			this.panel3.TabIndex = 2;
			// 
			// buttonSaveAs
			// 
			this.buttonSaveAs.BackColor = System.Drawing.SystemColors.ControlLight;
			this.buttonSaveAs.Cursor = System.Windows.Forms.Cursors.Hand;
			this.buttonSaveAs.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonSaveAs.Font = new System.Drawing.Font("Tahoma", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.World);
			this.buttonSaveAs.Location = new System.Drawing.Point(8, 64);
			this.buttonSaveAs.Name = "buttonSaveAs";
			this.buttonSaveAs.Size = new System.Drawing.Size(56, 56);
			this.buttonSaveAs.TabIndex = 1;
			this.buttonSaveAs.Text = "Save As...";
			this.buttonSaveAs.Click += new System.EventHandler(this.ButtonSaveAsClick);
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.panel2);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.DockPadding.All = 4;
			this.panel1.Location = new System.Drawing.Point(5, 286);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(462, 72);
			this.panel1.TabIndex = 1;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.textBoxLog);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel2.Location = new System.Drawing.Point(4, 20);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(454, 48);
			this.panel2.TabIndex = 4;
			// 
			// textBoxLog
			// 
			this.textBoxLog.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBoxLog.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.World);
			this.textBoxLog.Location = new System.Drawing.Point(0, 0);
			this.textBoxLog.Multiline = true;
			this.textBoxLog.Name = "textBoxLog";
			this.textBoxLog.ReadOnly = true;
			this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textBoxLog.Size = new System.Drawing.Size(454, 48);
			this.textBoxLog.TabIndex = 3;
			this.textBoxLog.Text = "";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Dock = System.Windows.Forms.DockStyle.Top;
			this.label1.Location = new System.Drawing.Point(4, 4);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(92, 16);
			this.label1.TabIndex = 3;
			this.label1.Text = "Tablet debug log:";
			// 
			// saveFileDialog
			// 
			this.saveFileDialog.DefaultExt = "jpg";
			this.saveFileDialog.Filter = "JPEG files|*.jpg;*.jpeg|PNG files|*.png|All files|*.*";
			// 
			// buttonNew
			// 
			this.buttonNew.BackColor = System.Drawing.SystemColors.ControlLight;
			this.buttonNew.Cursor = System.Windows.Forms.Cursors.Hand;
			this.buttonNew.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonNew.Font = new System.Drawing.Font("Tahoma", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.World);
			this.buttonNew.Location = new System.Drawing.Point(8, 0);
			this.buttonNew.Name = "buttonNew";
			this.buttonNew.Size = new System.Drawing.Size(56, 56);
			this.buttonNew.TabIndex = 0;
			this.buttonNew.Text = "Clear All";
			this.buttonNew.Click += new System.EventHandler(this.ButtonNewClick);
			// 
			// TestForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(472, 363);
			this.Controls.Add(this.panel3);
			this.Controls.Add(this.panel1);
			this.DockPadding.All = 5;
			this.Name = "TestForm";
			this.Text = "WinTab32 Test by h3";
			this.panel4.ResumeLayout(false);
			this.panel3.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion
		
		void PaintDot(float x, float y, float pressure)
		{
			float brushSize = pressure * 4.0f;
			
			myBrush.Color = Color.FromArgb((int)(pressure * 150.0f) + 10,
			                               myBrush.Color.R,
			                               myBrush.Color.G,
			                               myBrush.Color.B);
			
			Rectangle rect = new Rectangle((int)(x - brushSize * 0.5f),
			                               (int)(y - brushSize * 0.5f),
			                               (int)brushSize + 2, 
			                               (int)brushSize + 2);
			//graphics.FillRectangle(new SolidBrush(Color.Red),rect);
			lock(graphics) {
			graphics.FillEllipse(myBrush, 
			                     x - brushSize * 0.5f,
			                     y - brushSize * 0.5f, 
			                     brushSize, 
			                     brushSize);
			}
			this.pictureBox.Invalidate(rect);
		}
		
		protected override void Log(string logtext) 
		{
			if (textBoxLog != null)
				lock(textBoxLog) {
					textBoxLog.AppendText(logtext+"\r\n");
				}
		}
		
		private delegate void PaintDotDelegate(float x, float y, float pressure);
		
		void PacketQueueThreadStart()
		{
			while(true) {
				TabletPacketEventArgs e = Dequeue();
				if (e.NormalPressure > 0) {
					PointF relPos = e.GetRelativePosition(this.pictureBox);
					//PaintDot(relPos.X,relPos.Y,e.NormalPressure);
					BeginInvoke(new PaintDotDelegate(PaintDot), new object[] {relPos.X,relPos.Y,e.NormalPressure});
				}
			}
		}
		
		void ButtonNewClick(object sender, System.EventArgs e)
		{
			graphics.FillRectangle(new SolidBrush(Color.White),0,0,bitmap.Width,bitmap.Height);
			this.pictureBox.Invalidate();
		}
		
		void ButtonSaveAsClick(object sender, System.EventArgs e)
		{
			if (this.saveFileDialog.ShowDialog() == DialogResult.OK) {
				this.pictureBox.Image.Save(this.saveFileDialog.FileName);
			}
		}
		
	}
}

