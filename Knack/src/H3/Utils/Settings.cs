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
using System.Xml;

namespace H3.Utils
{
	/// <summary>
	/// A singleton settings class.
	/// </summary>
	public sealed class Settings
	{
		#region Singleton handling
    	static readonly Settings instance = new Settings();

    	// Explicit static constructor to tell C# compiler
    	// not to mark type as beforefieldinit
    	static Settings()
    	{ }
    	
    	Settings()
    	{
    		settingsFilePath = System.IO.Path.GetDirectoryName(
      			System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase );
    		//Console.WriteLine("settingsfile: "+settingsFilePath+"\\"+settingsFileName);
    		LoadSettings();
    	}
    	
    	public static Settings Instance {
        	get { return instance; }
    	}
    	#endregion
    	
    	const string settingsFileName = "settings.xml";
    	string settingsFilePath = "";
    	XmlDocument xmlSettings = new XmlDocument();
    	
    	void LoadSettings()
    	{
    		xmlSettings.Load(settingsFilePath+Path.DirectorySeparatorChar+settingsFileName);
    	}
    	/*
    	void SaveSettings()
    	{
    		xmlSettings.Save(settingsFilePath+Path.DirectorySeparatorChar+settingsFileName);
    	}
    	*/
    	public string GetString(string xpath) {
    		try {
    			XmlNode node = xmlSettings.SelectSingleNode(xpath);
    			return node.InnerText;
    		} catch (System.Xml.XPath.XPathException e) {
    			Console.WriteLine(e.Message);
    			throw new Exception("Node missing in the "
    			                    +settingsFilePath+Path.DirectorySeparatorChar
    			                    +settingsFileName+" file: "+xpath);
    		}
    	}
    	
    	public int GetInt(string xpath) {
    		return int.Parse(GetString(xpath));
    	}
    	
    	public short GetShort(string xpath) {
    		return (short) GetInt(xpath);
    	}
	}
}
