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

namespace H3.Sound.Midi.MidiRender
{
	public delegate void MidiMessageHandler(MidiMessage msg);
	
	public interface IMidiRender
	{
		event MidiMessageHandler OnMidiMessage;
		bool Editable { get; }
		void OnEdit();
	}
	
	public interface IMidiHandler
	{
		IMidiRender[] MidiInputs { get; set; }
		MidiMessageHandler MidiMessageHandler { get; }
	}
}
