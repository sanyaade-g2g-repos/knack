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

using H3.Sound.Midi;
using NUnit.Framework;

namespace H3.Sound.Tests
{
	[TestFixture]
	public class TestMidiFile
	{
		[Test]
		public void TestLoad()
		{
			MidiFile mf = new MidiFile("town.mid");
			Assert.AreEqual(mf.ChunkMThd.Header.ID,"MThd","The first chunk ID should be MThd");
			Assert.AreEqual(mf.ChunkMThd.Header.Length,6,"The first chunk Length should be 6");
			Assert.AreEqual(mf.ChunkMThd.Format,1,"The Midi File Format for this file should be 1");
			Assert.AreEqual(mf.ChunkMThd.NumTracks,12,"The number of tracks for this file should be 12");
			Assert.AreEqual(mf.ChunkMThd.Division,384,"The Division for this file should be 384");
		
			mf = new MidiFile("h3-9settembre-v2.mid");
			Assert.AreEqual(mf.ChunkMThd.Header.ID,"MThd","The first chunk ID should be MThd");
			Assert.AreEqual(mf.ChunkMThd.Header.Length,6,"The first chunk Length should be 6");
			Assert.AreEqual(mf.ChunkMThd.Format,1,"The Midi File Format for this file should be 1");
			Assert.AreEqual(mf.ChunkMThd.NumTracks,4,"The number of tracks for this file should be 12");
			Assert.AreEqual(mf.ChunkMThd.Division,15360,"The Division for this file should be 384");
		
			//Assert.Fail("");
		}
	}
	
	[TestFixture]
	public class TestMidiUtils
	{
		[Test]
		public void TestWriteVariableLength()
		{
			MemoryStream mStream = new MemoryStream();
			BinaryWriter bWriter = new BinaryWriter(mStream);
			BinaryReader bReader = new BinaryReader(mStream);
			
			MidiUtils.WriteVariableLength(bWriter,0x40);
			mStream.Seek(0,SeekOrigin.Begin);
			Assert.AreEqual(mStream.Length,1);
			Assert.AreEqual(bReader.ReadByte(),0x40);
			
			mStream = new MemoryStream();
			bWriter = new BinaryWriter(mStream);
			bReader = new BinaryReader(mStream);
			
			MidiUtils.WriteVariableLength(bWriter,0x2000);
			mStream.Seek(0,SeekOrigin.Begin);
			Assert.AreEqual(mStream.Length,2);
			Assert.AreEqual(bReader.ReadByte(),0xC0);
			Assert.AreEqual(bReader.ReadByte(),0x00);
			
			mStream = new MemoryStream();
			bWriter = new BinaryWriter(mStream);
			bReader = new BinaryReader(mStream);
			
			MidiUtils.WriteVariableLength(bWriter,0x186A0);
			mStream.Seek(0,SeekOrigin.Begin);
			Assert.AreEqual(mStream.Length,3);
			Assert.AreEqual(bReader.ReadByte(),0x86);
			Assert.AreEqual(bReader.ReadByte(),0x8D);
			Assert.AreEqual(bReader.ReadByte(),0x20);
			
			mStream = new MemoryStream();
			bWriter = new BinaryWriter(mStream);
			bReader = new BinaryReader(mStream);
			
			MidiUtils.WriteVariableLength(bWriter,0x1111111);
			mStream.Seek(0,SeekOrigin.Begin);
			Assert.AreEqual(mStream.Length,4);
			Assert.AreEqual(bReader.ReadByte(),0x88);
			Assert.AreEqual(bReader.ReadByte(),0xC4);
			Assert.AreEqual(bReader.ReadByte(),0xA2);
			Assert.AreEqual(bReader.ReadByte(),0x11);
		}
		
		public void TestReadVariableLength()
		{
			MemoryStream mStream = new MemoryStream();
			BinaryWriter bWriter = new BinaryWriter(mStream);
			BinaryReader bReader = new BinaryReader(mStream);
			
			bWriter.Write((byte)0x40);
			
			mStream.Seek(0,SeekOrigin.Begin);
			int readVal = MidiUtils.ReadVariableLength(bReader);
			Assert.AreEqual(mStream.Position,1);
			Assert.AreEqual(readVal,0x40);
			
			mStream = new MemoryStream();
			bWriter = new BinaryWriter(mStream);
			bReader = new BinaryReader(mStream);
			
			bWriter.Write((byte)0xC0);
			bWriter.Write((byte)0x00);
			
			mStream.Seek(0,SeekOrigin.Begin);
			readVal = MidiUtils.ReadVariableLength(bReader);
			Assert.AreEqual(mStream.Position,2);
			Assert.AreEqual(readVal,0x2000);
			
			mStream = new MemoryStream();
			bWriter = new BinaryWriter(mStream);
			bReader = new BinaryReader(mStream);
			
			bWriter.Write((byte)0x86);
			bWriter.Write((byte)0x8D);
			bWriter.Write((byte)0x20);
			
			mStream.Seek(0,SeekOrigin.Begin);
			readVal = MidiUtils.ReadVariableLength(bReader);
			Assert.AreEqual(mStream.Position,3);
			Assert.AreEqual(readVal,0x186A0);
			
			mStream = new MemoryStream();
			bWriter = new BinaryWriter(mStream);
			bReader = new BinaryReader(mStream);
			
			bWriter.Write((byte)0x88);
			bWriter.Write((byte)0xC4);
			bWriter.Write((byte)0xA2);
			bWriter.Write((byte)0x11);
			
			mStream.Seek(0,SeekOrigin.Begin);
			readVal = MidiUtils.ReadVariableLength(bReader);
			Assert.AreEqual(mStream.Position,4);
			Assert.AreEqual(readVal,0x1111111);
		}
	}
}
