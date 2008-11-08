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
using System.Threading;
using System.Windows.Forms;
#if PLATFORM_WIN32  
	using Microsoft.DirectX.DirectSound;
#endif

using H3.Utils;

namespace H3.Sound.Wave
{
	/// <summary>
	/// A managed DirectX 9 sound player
	/// </summary>
	public class SoundOut : IDisposable
	{
		int samplesPerSecond = 44100;
		int channels = 2;
		int bitsPerSample = 16;
		int bufferBlockBytes = 3200;
		int bufferBytes = 3200 * 8;  // This must be a multiple of bufferBlockBytes !
		int sleepTime = 5;
		Device soundDevice;
		SecondaryBuffer soundBuffer;
		BufferDescription soundBufferDescription;
		WaveFormat soundWaveFormat = new WaveFormat();
		System.Timers.Timer soundTimer;
		FileStream fs;
	    BinaryWriter w;
		bool outputToFile = false;
		int completedPos = 0;
		StreamingType streamingType = StreamingType.Thread;
		short[] blockData;
		float[] floatLeftChannel;
		float[] floatRightChannel;
		bool disposed = false;
		
		public enum StreamingType { Thread, Timer }
		
		public SoundOut()
		{
			samplesPerSecond = Settings.Instance.GetInt(
				"/Settings/Output/Sound/General/SamplesPerSecond");
			bitsPerSample = Settings.Instance.GetInt(
				"/Settings/Output/Sound/General/BitsPerSample");
			channels = Settings.Instance.GetInt(
				"/Settings/Output/Sound/General/Channels");
			bufferBlockBytes = Utils.Settings.Instance.GetInt(
			    "/Settings/Output/Sound/DirectSound/BufferSamples")
				*(bitsPerSample/8)*channels;
			bufferBytes = Settings.Instance.GetInt(
				"/Settings/Output/Sound/DirectSound/Buffers")
				*bufferBlockBytes;
			sleepTime = Settings.Instance.GetInt(
				"/Settings/Output/Sound/DirectSound/ThreadSleepTime");
			
			soundDevice = new Device();
			soundDevice.SetCooperativeLevel(new Form(), CooperativeLevel.Normal);  
			blockData = new short[bufferBlockBytes];
			floatLeftChannel = new float[bufferBlockBytes/2];
			floatRightChannel = new float[bufferBlockBytes/2];
			
			soundWaveFormat.SamplesPerSecond = samplesPerSecond;
			soundWaveFormat.Channels = (short) channels;
			soundWaveFormat.BitsPerSample = (short) bitsPerSample;
			soundWaveFormat.BlockAlign = (short)(soundWaveFormat.Channels * (soundWaveFormat.BitsPerSample / 8));
			soundWaveFormat.AverageBytesPerSecond = soundWaveFormat.BlockAlign * soundWaveFormat.SamplesPerSecond;
			soundWaveFormat.FormatTag = WaveFormatTag.Pcm;
			System.Console.WriteLine(soundWaveFormat.ToString());
			soundBufferDescription = new BufferDescription();
			soundBufferDescription.GlobalFocus = true;
			soundBufferDescription.LocateInSoftware = true;
			soundBufferDescription.BufferBytes = bufferBytes;
			soundBufferDescription.CanGetCurrentPosition = true;
			soundBufferDescription.ControlVolume = true;  
			soundBufferDescription.Format = soundWaveFormat;
			
			soundBuffer = new SecondaryBuffer(soundBufferDescription,soundDevice);
			bufferBytes = soundBuffer.Caps.BufferBytes;
			
			if (streamingType == StreamingType.Timer) {
				soundTimer = new System.Timers.Timer(sleepTime);
				soundTimer.Enabled = false;
				soundTimer.Elapsed += new System.Timers.ElapsedEventHandler(SoundTimerElapsed);
			}
			
			blockData.Initialize();
			for (int i = 0; i<bufferBytes/bufferBlockBytes; i++)  {
				soundBuffer.Write(blockData.Length*i, blockData, LockFlag.EntireBuffer);  
			}
			
		}
		
		~SoundOut()
		{
			Dispose(false);
		}
		
		public void Dispose()
   		{
			
			if (streamingType == StreamingType.Timer) soundTimer.Dispose();
     	 	Dispose(true);
     		GC.SuppressFinalize(this);
  		}
		
		protected virtual void Dispose(bool disposing)
  	 	{
			Console.WriteLine("SoundOut disposed");
      		if(!this.disposed)
      		{
         		if(disposing)
         		{
            		// Dispose managed resources.
					Stop();
         		}
         		// Release unmanaged resources. 
      		}
      		disposed = true;         
   		}
		
		private void SoundTimerElapsed(object sender, System.Timers.ElapsedEventArgs e) 
		{
			int playPos,writePos;
			bool tryagain;
			LockFlag lockFlag = LockFlag.None;

			if (blockData == null) return;
								
			do {
				playPos = soundBuffer.PlayPosition;
				writePos = soundBuffer.WritePosition;
				//System.Console.WriteLine("current: "+playPos+" write: "+writePos+" diff: "+(writePos-playPos)+" completed: "+ completedPos);
				
				tryagain = false;
				if (playPos > completedPos) {
					if ((playPos-completedPos) >= bufferBlockBytes*2) {
						Render(blockData);
						soundBuffer.Write(completedPos,blockData,lockFlag);
						completedPos += bufferBlockBytes*2;
						tryagain = true;
					}
				}
				else {
					if ((bufferBytes-completedPos) >= bufferBlockBytes*2) {
						Render(blockData);
						soundBuffer.Write(completedPos,blockData,lockFlag);
						completedPos += bufferBlockBytes*2;
						tryagain = true;
					}
				}
				if (completedPos >= bufferBytes) completedPos = 0;
			} while (tryagain);
		}
		
		public void Start() 
		{	
			soundBuffer.SetCurrentPosition(0);
			soundBuffer.Play(0, BufferPlayFlags.Looping);
			
			if (streamingType == StreamingType.Timer) {
				System.Threading.Thread.Sleep(1);
				soundTimer.Enabled = true;
			}
			
			if (streamingType == StreamingType.Thread) {
				Thread soundThread = new Thread(new ThreadStart(SoundThread));  
				soundThread.Name = "SoundThread";  
				soundThread.Priority = ThreadPriority.Highest;  
				soundThread.IsBackground = true;
				soundThread.Start();  
			}
			
			System.Console.WriteLine("SoundOut started");
			
			if (outputToFile) {
				fs = new FileStream("knack.raw", FileMode.CreateNew);
		        w = new BinaryWriter(fs);
			}
		}
		
		public void Stop() 
		{
			System.Console.WriteLine("SoundOut stopped");
			if (streamingType == StreamingType.Timer) {
				if (soundTimer!= null) soundTimer.Enabled = false;
			}
			if (soundBuffer != null) {
				if (soundBuffer.Status.Playing) soundBuffer.Stop();
			}
			if (outputToFile) {
				w.Close();
	        	fs.Close();
			}
		}
		
		private void SoundThread()  
		{  
			for (;;) {
				System.Threading.Thread.Sleep(sleepTime);
				SoundTimerElapsed(null,null);
			}
		}  
		
		/// <summary>
		/// Override this method to output a sound
		/// </summary>
		public virtual void Render(float[] leftChannel,float[] rightChannel)
		{ }
		
		private void Render(short[] interleavedChannel)
			{
				int channelSize = interleavedChannel.Length/2;
				
				float wave;
			
				for (int i=0; i<channelSize; i++) {
					floatLeftChannel[i] = 0.0f;
				}
				for (int i=0; i<channelSize; i++) {
					floatRightChannel[i] = 0.0f;
				}
				
				this.Render(floatLeftChannel,floatRightChannel);
			
				for (int i=0; i<channelSize; i++) {
					wave = floatLeftChannel[i];
					if (wave>1.0f) wave = 1.0f; else if (wave<-1.0f) wave = -1.0f;
					interleavedChannel[i*2] = (short) Math.Round(wave * 32767.0f);
					wave = floatRightChannel[i];
					if (wave>1.0f) wave = 1.0f; else if (wave<-1.0f) wave = -1.0f;
					interleavedChannel[i*2+1] = (short) Math.Round(wave * 32767.0f);
				}
				if (outputToFile) {
					if (w!=null)
					for (int i=0; i<interleavedChannel.Length; i++) {
						w.Write(interleavedChannel[i]);
					}
				}
			}
		
	}
}
