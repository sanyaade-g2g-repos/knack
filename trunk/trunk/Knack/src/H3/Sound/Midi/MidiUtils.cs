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

namespace H3.Sound.Midi
{
	/// <summary>
	/// Description of MidiUtils.
	/// </summary>
	public class MidiUtils
	{
		const int mask7bits = 0x7F;
		const int setbit8 = 0x80;
			
		private MidiUtils()
		{ }
		
		public static bool IsStatusByte(byte b)
		{
			if ((b & setbit8) != 0) return true;
			return false;
		}
		
		public static int MessageDataBytesCount(byte status)
		{
			byte messageType = (byte)((int)status & 0xF0);
			
			if(status == 0) return 0;
			
			switch(messageType) {
				case 0x80: return 2; // note-off
				case 0x90: return 2; // note-on
				case 0xA0: return 2; // aftertouch
				case 0xB0: return 2; // controller
				case 0xC0: return 1; // program change
				case 0xD0: return 1; // channel pressure
				case 0xE0: return 2; // pitch wheel
				case 0xF0: 
					switch(status) {
						case 0xF1: return 1; // MTC quarter frame
						case 0xF2: return 2; // song position pointer
						case 0xF3: return 1; // song select
						case 0xF4: return 0; // tune request
						case 0xF8: return 0; // MIDI clock
						case 0xF9: return 0; // MIDI tick
						case 0xFA: return 0; // MIDI start
						case 0xFC: return 0; // MIDI stop
						case 0xFB: return 0; // MIDI continue
						case 0xFE: return 0; // active sense
						case 0xFF: return 0; // reset
					}
					break;
			}
			
			
			
			return -1;
		}
		
		public static bool IsVoiceCategoryStatus(byte status)
		{
			if ((status >= 0x80) && (status <= 0xEF)) return true;
			return false;
		}
		
		public static bool IsSystemCommonCategoryStatus(byte status)
		{
			if ((status >= 0xF0) && (status <= 0xF7)) return true;
			return false;
		}
		
		public static bool IsSystemRealtimeCategoryStatus(byte status)
		{
			if ((status >= 0xF8) && (status <= 0xFF)) return true;
			return false;
		}
		
		public static int ReadVariableLength(BinaryReader reader) 
		{	
			
			byte b;
			int value = 0;
				
			do {
				b = reader.ReadByte();
				value = (value << 7) | (int)(b & mask7bits);
			} while ((b & setbit8) != 0);
	
			return value;
		}
		
		public static void WriteVariableLength(BinaryWriter writer, int value)
		{
			byte[] vlbytes = new byte[4];
			int startfrom;
				
			vlbytes[0] = (byte) (((value >> 21) & mask7bits) | setbit8);
			vlbytes[1] = (byte) (((value >> 14) & mask7bits) | setbit8);
			vlbytes[2] = (byte) (((value >> 7) & mask7bits) | setbit8);
			vlbytes[3] = (byte) (value & mask7bits);
		
			for (startfrom = 0; startfrom <3; startfrom++) {
				if (vlbytes[startfrom] != setbit8) break;
			}
			
			for (int i= startfrom; i<4; i++) {
				writer.Write(vlbytes[i]);
			}
		}
	}
}
