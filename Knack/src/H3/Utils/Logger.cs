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
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace H3.Utils
{
	/// <summary>
	/// A singleton logger class.
	/// </summary>
	public class Logger
	{
		#region Loggers
		
		public interface ILogger {
			void Log(string logText);
		}
		
		/// <summary>
		/// A null logger class.
		/// </summary>
		private class NullLogger : ILogger
		{
			public void Log(string logText)
			{
				// It's right, this one should do nothing!
			}
		}
		
		/// <summary>
		/// A full logger class.
		/// </summary>
		private class FullLogger : IDisposable, ILogger
		{
			private class LogConsole : Form {
		
				RichTextBox logBox;
		
				public LogConsole() : base() 
				{
					this.Width = 600;
					this.Height = 100;
					this.Text = "Log window";
					this.ControlBox = true;
					this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
					this.TopMost = true;
					this.ShowInTaskbar = false;
					this.Opacity = 0.8;
					this.StartPosition = FormStartPosition.Manual;
					this.Location = new System.Drawing.Point(
						Screen.PrimaryScreen.Bounds.Right - this.Width - 10,
						Screen.PrimaryScreen.Bounds.Bottom - this.Height - 50);
					logBox = new RichTextBox();
					this.Controls.Add(logBox);
					logBox.Dock = DockStyle.Fill;
					logBox.BackColor = System.Drawing.Color.Black;
					logBox.ReadOnly = true;
					logBox.ForeColor = System.Drawing.Color.White;
					logBox.Font = new System.Drawing.Font(System.Drawing.FontFamily.GenericMonospace,
					                                      8,System.Drawing.FontStyle.Regular);
				}
				
				public void WriteLogLine(string logText) 
				{
					logBox.AppendText(logText+"\n");
					logBox.ScrollToCaret();
					if (logBox.Visible) logBox.Focus();
				}
			}
			
	    	public FullLogger()
	    	{
	    		logFilePath = System.IO.Path.GetDirectoryName(
	      			System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase );
	    		if (logFilePath.StartsWith("file:\\")) {
	    			logFilePath = logFilePath.Substring(6);
	    		}
	    		if (logFilePath.StartsWith("file://")) {
	    			logFilePath = logFilePath.Substring(7);
	    		}
	    		Console.WriteLine("logfile: "+logFilePath+Path.DirectorySeparatorChar+logFileName);
	    		StartLog();
	    	}
	    	
	    	LogConsole logConsole;
	    	bool disposed = false;
	    	const string logFileName = "log.txt";
	    	string logFilePath = "";
	    	StreamWriter logFile;
	    	
	    	~FullLogger() 
	    	{
	    		Dispose(false);
	    	}
	    	
	    	public void Dispose()
	    	{
	    		Dispose(true);
	     		GC.SuppressFinalize(this);
	    	}
	    	
	    	protected virtual void Dispose(bool disposing)
	  	 	{
				if(!this.disposed)
	      		{
	         		if(disposing)
	         		{
	            		// Dispose managed resources.
	         		}
	         		// Release unmanaged resources. 
					Log("Log closed");
					logFile.Flush();
	    			logFile.Close();
	      		}
	      		disposed = true;         
	   		}
	 
	    	void StartLog() 
	    	{
	    		logConsole = new LogConsole();
	    		logConsole.Show();
	    		//Console.WriteLine("file: "+logFilePath+Path.DirectorySeparatorChar+logFileName);
	    		logFile = File.AppendText(logFilePath+Path.DirectorySeparatorChar+logFileName);
	    		logFile.WriteLine();
	    		string logStartLine = "* "
	              +System.DateTime.Now.ToShortDateString()
	              +" - "
	              +System.DateTime.Now.ToShortTimeString()
	              +" * Log started for "
	              +Assembly.GetExecutingAssembly().GetName().Name.ToString()
	              +" v."
	              +Assembly.GetExecutingAssembly().GetName().Version.ToString();
				
	    		#if DEBUG
					logStartLine += " (debug build)";
				#else 
					logStartLine += " (release build)";
				#endif
				
				WriteLogLine(logStartLine);
	    	}
	    	
	    	public void Log(string logText) {
				WriteLogLine(System.DateTime.Now.ToShortTimeString()+" - "+logText);
	    	}
	    	
	    	protected void WriteLogLine(string logText) {
	    		logFile.WriteLine(logText);
	    		logConsole.WriteLogLine(logText);
	    		logFile.Flush();
	    	}
	    	
		}
		
		#endregion
		
		#region Singleton handling
		
		static readonly ILogger instance =
		#if DEBUG
    		new FullLogger();
		#else
			new NullLogger();
		#endif

    	// Explicit static constructor to tell C# compiler
    	// not to mark type as beforefieldinit
    	static Logger()
    	{ }
    	
    	public static ILogger Instance {
        	get { return instance; }
    	}
    	
    	#endregion
	}
	
}

