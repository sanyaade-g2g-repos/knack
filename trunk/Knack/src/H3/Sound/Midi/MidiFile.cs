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
using System.Collections;

namespace H3.Sound.Midi
{
	public struct MidiFileChunkHeader
	{
		char ID0,ID1,ID2,ID3;
		//TODO If we use .NET 2.0 we can use a fixed size array
		
	  	uint length; 
	  	
	  	public string ID {
	  		get { return "" + ID0 + ID1 + ID2 + ID3; }
	  		set { 
	  			if (value.Length > 0) ID0 = value[0];
	  			if (value.Length > 1) ID1 = value[1];
	  			if (value.Length > 2) ID2 = value[2];
	  			if (value.Length > 3) ID3 = value[3];
	  		}
	  	}
	  	
	  	public uint Length {
	  		get { return length; }
	  		set { length = value; }
	  	}
	  	
	  	private uint NetworkToHostOrder(uint nOrd) 
	  	{
	  		return ((nOrd & 0xFF) << 24)
	  			| ((nOrd & 0xFF00) << 8)
	  			| ((nOrd & 0xFF0000) >> 8)
	  			| (nOrd >> 24);
	  	}
	  	
	  	public void Read(BinaryReader reader)
	  	{
	  		ID0 = reader.ReadChar();
	  		ID1 = reader.ReadChar();
	  		ID2 = reader.ReadChar();
	  		ID3 = reader.ReadChar();
	  		length = NetworkToHostOrder(reader.ReadUInt32());
	  	}
	  	
	  	public void Write(BinaryWriter writer)
	  	{
	  		writer.Write(ID0);
	  		writer.Write(ID1);
	  		writer.Write(ID2);
	  		writer.Write(ID3);
	  		writer.Write(length);
	  	}
	}
	
	public class MidiFileChunk 
	{
		MidiFileChunkHeader header = new MidiFileChunkHeader();
		byte[] data = null;
		
		public MidiFileChunk()
		{ }
		
		public MidiFileChunk(MidiFileChunk copyFrom)
		{
			header = new MidiFileChunkHeader();
			header.ID = copyFrom.Header.ID;
			header.Length = copyFrom.Header.Length;
			this.data = new byte[copyFrom.Data.Length];
			Array.Copy(copyFrom.Data,data,copyFrom.Data.Length);
			checkChunk();
		}
		
		public virtual string ExpectedChunkID {
			get { return null; }
		}
		
		public MidiFileChunkHeader Header {
			get { return header; }
		}
		
		public byte[] Data {
			get { return data; }
		}
		
		private bool correctChunkID() 
		{
			if ((this.ExpectedChunkID == null) ||
			    (this.ExpectedChunkID == this.header.ID)) return true;
			return false;
		}
		
		public virtual bool correctChunkLength() 
		{
			return true;
		}
		
		private void checkChunk() 
		{
			if (!correctChunkLength()) throw new Exception("Wrong chunk header length: "+header.Length+", should be 6");
			if (!correctChunkID()) throw new Exception("Wrong MIDI chunk ID: \""+header.ID+"\", should be \""+ExpectedChunkID+"\"");
		}
		
		public void Read(BinaryReader reader)
		{
			header.Read(reader);
			data = reader.ReadBytes((int)header.Length);
			checkChunk();
		}
		
		public override string ToString() 
		{
			return "MidiFileChunk ID: \""+header.ID+"\" Length: \""+header.Length+"\"";
		}
	}
	
	public class MidiFileChunkMThd : MidiFileChunk
	{
		public override string ExpectedChunkID {
			get { return "MThd"; }
		}
		
		public short Format {
			get { return (short) ((Data[0]<<8) | Data[1]); }
		}
		
		public short NumTracks {
			get { return (short) ((Data[2]<<8) | Data[3]); }
		}
		
		public short Division {
			get { return (short) ((Data[4]<<8) | Data[5]); }
		}
		
		public MidiFileChunkMThd() : base()
		{ }
		
		public MidiFileChunkMThd(MidiFileChunk copyFrom) : base(copyFrom)
		{ }
		
		public override bool correctChunkLength() 
		{
			if (Header.Length == 6) return true;
			return false;
		}
	}
	
	public class MidiEvent 
	{
		const byte statusSYSEX = 0xF0;
		const byte statusMETA = 0xFF;
		int deltaTime;
		byte status;
		bool isRunningStatus;
		byte type;
		byte[] data;
		
		public MidiEvent(BinaryReader reader, byte currentStatus) {
			Load(reader,currentStatus);
		}
		
		public int DeltaTime {
			get { return deltaTime; }
		}
		
		public byte Status {
			get { return status; }
		}
		
		public byte[] Data {
			get { return data; }
		}
		
		public void Load(BinaryReader reader, byte currentStatus) {
			int dataLength;
			
			deltaTime = MidiUtils.ReadVariableLength(reader);
			
			byte firstByte = reader.ReadByte();
			if (MidiUtils.IsStatusByte(firstByte)) {
				status = firstByte;
				isRunningStatus = false;
			} else {
				status = currentStatus;
				isRunningStatus = true;
			}
			
			switch(status) {
					
				case statusSYSEX:
					dataLength = MidiUtils.ReadVariableLength(reader);
					data = reader.ReadBytes(dataLength);
					break;
					
				case statusMETA:
					type = reader.ReadByte();
					dataLength = MidiUtils.ReadVariableLength(reader);
					data = reader.ReadBytes(dataLength);
					break;
					
				default:
					data = new byte[MidiUtils.MessageDataBytesCount(status)];
					if (isRunningStatus) {
						data[0] = firstByte;
						for(int i=1; i<data.Length; i++)
							data[i] = reader.ReadByte();
					} else {
						data = reader.ReadBytes(data.Length);
					}
					break;
			} 
		}
	}
	
	public class MidiFileChunkMTrk : MidiFileChunk
	{
		ArrayList midiEvents = new ArrayList();
		
		public MidiFileChunkMTrk() : base()
		{ }
		
		public MidiFileChunkMTrk(MidiFileChunk copyFrom) : base(copyFrom)
		{ 
			parseData();
		}
		
		public override string ExpectedChunkID {
			get { return "MTrk"; }
		}
		
		public ArrayList MidiEvents {
			get { return midiEvents; }
		}
		
		public override bool correctChunkLength() {
			return true;
		}
		
		private void parseData() 
		{
			byte currentStatus = 0;
			BinaryReader reader = new BinaryReader(new MemoryStream(this.Data));
			while (reader.BaseStream.Position<reader.BaseStream.Length) {
				MidiEvent readEvent = new MidiEvent(reader,currentStatus);
				midiEvents.Add(readEvent);
				if (MidiUtils.IsVoiceCategoryStatus(readEvent.Status))
					currentStatus = readEvent.Status;
				if (MidiUtils.IsSystemCommonCategoryStatus(readEvent.Status))
					currentStatus = 0;
			}
			
		}

	}
	
	/// <summary>
	/// Description of MidiFile.
	/// </summary>
	public class MidiFile
	{
		MidiFileChunkMThd chunkMThd;
		ArrayList chunkMTrk = new ArrayList();
	
		public MidiFile()
		{ }
		
		public MidiFileChunkMThd ChunkMThd {
			get { return this.chunkMThd; }
		}
		
		public ArrayList ChunkMTrk {
			get { return this.chunkMTrk; }
		}
		
		public MidiFile(string fileName) 
		{
			Load(fileName);
		}
		
		private void showMsg(string msg) 
		{
			System.Windows.Forms.MessageBox.Show(
				msg, 
				"Informazioni", 
				System.Windows.Forms.MessageBoxButtons.OK, 
				System.Windows.Forms.MessageBoxIcon.Asterisk, 
				System.Windows.Forms.MessageBoxDefaultButton.Button1);
		}
		
		public void Load(string fileName) 
		{
			MidiFileChunk tempChunk = new MidiFileChunk();
			BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open));
			chunkMThd =  null;
			
			string fileHeader = new string(reader.ReadChars(4));
			if (fileHeader == "RIFF") {
				reader.ReadBytes(8); // if it's a RIFF we skip the first 4+8 bytes
			} else {
				reader.BaseStream.Seek(0,SeekOrigin.Begin); // else we don't skip
			}
			
			while (reader.BaseStream.Position<=reader.BaseStream.Length-6) {
				tempChunk.Read(reader);
				//showMsg("Chunk ID: \""+tempChunk.Header.ID+"\" Length: "+tempChunk.Header.Length);
				if (tempChunk.Header.ID == "MThd") {
					chunkMThd = new MidiFileChunkMThd(tempChunk);
					break;
				}
			}
			
			if (chunkMThd == null) {
				showMsg("Error: MIDI MThd chunk not found in MIDI file");
			} else {
			
				while (reader.BaseStream.Position<=reader.BaseStream.Length-6) {
					tempChunk.Read(reader);
					//showMsg("Chunk ID: \""+tempChunk.Header.ID+"\" Length: "+tempChunk.Header.Length);
					if (tempChunk.Header.ID == "MTrk") {
						chunkMTrk.Add(new MidiFileChunkMTrk(tempChunk));
					}
				}
				
				if (chunkMTrk.Count != chunkMThd.NumTracks) {
					showMsg("Warning: wrong number of tracks in MIDI file");
				}
			}
			/*
			reader.Close();
			MidiFileChunkMTrk mtrk = (MidiFileChunkMTrk)chunkMTrk[2];
			showMsg(""+mtrk.MidiEvents.Count);
			*/
		}
	
	
		
	}
}
