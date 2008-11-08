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
using System.Reflection;
using System.Xml;
using System.ComponentModel;

using H3.Utils;

namespace H3.Sound
{
	/// <summary>
	/// KNKfile Load and Save
	/// </summary>
	public class KnackFile
	{
		ArrayList soundBlocks;
		
		public struct SoundBlockData
		{
			public SoundBlock SoundBlock;
			public ArrayList MidiInputs,SoundInputs;
			public int X,Y;
			
			public SoundBlockData(SoundBlock soundBlock, int x, int y)
			{
				this.MidiInputs = new ArrayList(1);
				this.SoundInputs = new ArrayList(2);
				this.SoundBlock = soundBlock;
				this.X = x;
				this.Y = y;
			}
		}
		
		public KnackFile()
		{
			soundBlocks = new ArrayList(20);
		}
		
		public KnackFile(string filename) : this()
		{
			Load(filename);
		}
		
		public ICollection SoundBlockList {
			get { return this.soundBlocks; }
		}
		
		public void Clear()
		{
			soundBlocks.Clear();
		}
		
		public void Add(SoundBlockData soundBlockData)
		{
			soundBlocks.Add(soundBlockData);
		}
		
		public void Add(SoundBlock soundBlock, int x, int y)
		{
			Add(new SoundBlockData(soundBlock,x,y));
		}
		
		public void Load(string filename)
		{
			XmlDocument xmldoc = new XmlDocument();
			
			xmldoc.Load(filename);
			Clear();
			XmlNodeList xmlSoundBlocks = xmldoc.SelectNodes("descendant::SoundBlocks/SoundBlock");
			foreach(XmlNode xmlSB in xmlSoundBlocks) {
				SoundBlock srSoundBlock;
				string srClass = "",srName = "";
				int srX,srY;
				foreach(XmlNode xmlAttr in xmlSB.Attributes) {
					if (xmlAttr.Name.Equals("class")) {
					    srClass = xmlAttr.Value;
					}
					if (xmlAttr.Name.Equals("name")) {
					    srName = xmlAttr.Value;
					}
				}
				
				//HACK TO LOAD pre-v.0.6 files
				if ((srClass == "H3.Sound.RandomMidiRender")
				   || (srClass == "H3.Sound.MidiInRender")
				   || (srClass == "H3.Sound.MidiFilterRender")
				   || (srClass == "H3.Sound.NullMidiRender")) {
					string oldClass = srClass;
					srClass = srClass.Replace("H3.Sound.","H3.Sound.Midi.MidiRender.");
					Logger.Instance.Log("pre v0.6 class "+oldClass+" converted to "+srClass);
				} else if (srClass.StartsWith("H3.Sound.") &&
				    (!srClass.StartsWith("H3.Sound.Midi.MidiRender.")) &&
				    (!srClass.StartsWith("H3.Sound.Wave.SoundRender."))) {
				    string oldClass = srClass;
				 	srClass = srClass.Replace("H3.Sound.","H3.Sound.Wave.SoundRender.");
				 	Logger.Instance.Log("pre v0.6 class "+oldClass+" converted to "+srClass);
				}
				//HACK END
				    
				srX = int.Parse(xmlSB.SelectSingleNode("descendant::Location/@x").Value);
				srY = int.Parse(xmlSB.SelectSingleNode("descendant::Location/@y").Value);
				try {
					srSoundBlock = (SoundBlock) System.Activator.CreateInstance(null,srClass).Unwrap();
				} catch (Exception) {
					Logger.Instance.Log("ERROR: Unknown class: "+srClass);
					continue;
				}
				srSoundBlock.Name = srName;
				Type classType = srSoundBlock.GetType();
				PropertyInfo[] pInfos = classType.GetProperties( );
				XmlNode xmlSettings = xmlSB.SelectSingleNode("Settings");
				if (xmlSettings != null)
					foreach(XmlNode xmlProperty in xmlSettings.ChildNodes) {
						PropertyInfo foundPInfo = null;
						foreach(PropertyInfo pInfo in pInfos)
					    {
							if (pInfo.Name.Equals(xmlProperty.Name)) {
								foundPInfo = pInfo;
								//Logger.Instance.Log("Setting: "+pInfo);
					    		break;
							}
					    }
						if (foundPInfo == null) {
							Logger.Instance.Log("WARNING: \""+xmlProperty.Name+"\" not found!");
						} else {
							object[] param = new object[1];
					        Type propertyType = foundPInfo.GetValue(srSoundBlock,new object[0]).GetType();
					        TypeConverter tc = TypeDescriptor.GetConverter(propertyType);
					        if (tc.CanConvertFrom(typeof(string))) {
					        	param[0] = tc.ConvertFromInvariantString(xmlProperty.InnerXml);
					        }
					        else {
					        	Logger.Instance.Log("ERROR: can't deserialize \""+xmlProperty.Name+"\"");
					        }
					        MethodInfo method = foundPInfo.GetSetMethod();
					        if (method != null) method.Invoke(srSoundBlock,param);
					        else Logger.Instance.Log("loading error: can't deserialize property "+xmlProperty.Name);
						}
					}
				SoundBlockData sbData = new SoundBlockData(srSoundBlock,srX,srY);
				XmlNode xmlSoundInputs = xmlSB.SelectSingleNode("SoundInputs");
				if (xmlSoundInputs != null)
					foreach(XmlNode xmlSoundInput in xmlSoundInputs.ChildNodes) {
						int id = int.Parse(xmlSoundInput.SelectSingleNode("@id").Value);
					    string val = xmlSoundInput.SelectSingleNode("@name").Value;
					    //Console.WriteLine("SoundInput id: "+id+" name: "+val);
					    sbData.SoundInputs.Insert(id,val);
					}
				XmlNode xmlMidiInputs = xmlSB.SelectSingleNode("MidiInputs");
				if (xmlMidiInputs != null)
					foreach(XmlNode xmlMidiInput in xmlMidiInputs.ChildNodes) {
						int id = int.Parse(xmlMidiInput.SelectSingleNode("@id").Value);
					    string val = xmlMidiInput.SelectSingleNode("@name").Value;
					    //Console.WriteLine("MidiInput id: "+id+" name: "+val);
						sbData.MidiInputs.Insert(id,val);
					}
				
				Add(sbData);
			}
		}
		
		private bool IsPropertyToSave(string propertyName) 
		{
			string[] prName = { "MidiInputs", "SoundInputs", "Playing", "Name", "MidiMessageHandler", "Icon", "Editable" };
			
			foreach(string name in prName) {
				if (propertyName.Equals(name)) return false;
			}
			return true;
		}
		
		public void Save(string filename)
		{
			XmlDocument xmldoc = new XmlDocument();
			XmlNode xmlnode;
			XmlElement xmlroot,xmlsoundblocks,xmlsblock,xmlelem,xmlelem2;
			XmlAttribute xmlattr;
			XmlText xmltext;
			bool hasMidiInputs,hasSoundInputs,hasSettings;
	
   			xmlnode = xmldoc.CreateNode(XmlNodeType.XmlDeclaration,"","");
   			xmldoc.AppendChild(xmlnode);
   			xmlroot = xmldoc.CreateElement("Knack");
			xmlattr = xmldoc.CreateAttribute("version");
			xmltext = xmldoc.CreateTextNode(Assembly.GetExecutingAssembly().GetName().Version.ToString());
			xmlattr.AppendChild(xmltext);
			xmlroot.Attributes.Append(xmlattr);
			xmldoc.AppendChild(xmlroot);
			xmlsoundblocks = xmldoc.CreateElement("SoundBlocks");
			xmlroot.AppendChild(xmlsoundblocks);
			
			foreach (SoundBlockData sbData in soundBlocks) {
				xmlsblock = xmldoc.CreateElement("SoundBlock");
				xmlsoundblocks.AppendChild(xmlsblock);
				xmlattr = xmldoc.CreateAttribute("class");
				xmlsblock.Attributes.Append(xmlattr);
				xmltext = xmldoc.CreateTextNode(sbData.SoundBlock.GetType().ToString());
				xmlattr.AppendChild(xmltext);
				xmlattr = xmldoc.CreateAttribute("name");
				xmlsblock.Attributes.Append(xmlattr);
				xmltext = xmldoc.CreateTextNode(sbData.SoundBlock.Name);
				xmlattr.AppendChild(xmltext);
				
				xmlelem = xmldoc.CreateElement("Location");
				xmlsblock.AppendChild(xmlelem);
				xmlattr = xmldoc.CreateAttribute("x");
				xmltext = xmldoc.CreateTextNode(sbData.X.ToString());
				xmlattr.AppendChild(xmltext);
				xmlelem.Attributes.Append(xmlattr);
				xmlattr = xmldoc.CreateAttribute("y");
				xmltext = xmldoc.CreateTextNode(sbData.Y.ToString());
				xmlattr.AppendChild(xmltext);
				
				xmlelem.Attributes.Append(xmlattr);
				
				xmlelem = xmldoc.CreateElement("MidiInputs");
				hasMidiInputs = false;
				for(int i=0; i<sbData.SoundBlock.MidiInputs.Length; i++) {
					if ((sbData.SoundBlock.MidiInputs[i] != null)
					    && (sbData.SoundBlock.MidiInputs[i] is SoundBlock)) {
						SoundBlock sb = (SoundBlock) sbData.SoundBlock.MidiInputs[i];
						if (sb.Name.Length>0) {
							hasMidiInputs = true;
							xmlelem2 = xmldoc.CreateElement("Input");
							xmlattr = xmldoc.CreateAttribute("id");
							xmltext = xmldoc.CreateTextNode(i.ToString());
							xmlattr.AppendChild(xmltext);
							xmlelem2.Attributes.Append(xmlattr);
							xmlattr = xmldoc.CreateAttribute("name");
							xmlelem2.Attributes.Append(xmlattr);
							xmltext = xmldoc.CreateTextNode(sb.Name);
							xmlattr.AppendChild(xmltext);
							xmlelem.AppendChild(xmlelem2);
						}
					}
				}
				if (hasMidiInputs) xmlsblock.AppendChild(xmlelem);
				
				xmlelem = xmldoc.CreateElement("SoundInputs");
				hasSoundInputs = false;
				for(int i=0; i<sbData.SoundBlock.SoundInputs.Length; i++) {
					if ((sbData.SoundBlock.SoundInputs[i] != null)
					    && (sbData.SoundBlock.SoundInputs[i] is SoundBlock)) {
						SoundBlock sb = (SoundBlock) sbData.SoundBlock.SoundInputs[i];
						if ((!(sb is Wave.SoundRender.NullSoundRender)) && (sb.Name.Length>0)) {
							hasSoundInputs = true;
							xmlelem2 = xmldoc.CreateElement("Input");
							xmlattr = xmldoc.CreateAttribute("id");
							xmltext = xmldoc.CreateTextNode(i.ToString());
							xmlattr.AppendChild(xmltext);
							xmlelem2.Attributes.Append(xmlattr);
							xmlattr = xmldoc.CreateAttribute("name");
							xmlelem2.Attributes.Append(xmlattr);
							xmltext = xmldoc.CreateTextNode(sb.Name);
							xmlattr.AppendChild(xmltext);
							xmlelem.AppendChild(xmlelem2);
						}
					}
				}
				if (hasSoundInputs) xmlsblock.AppendChild(xmlelem);
				
				xmlelem = xmldoc.CreateElement("Settings");
				Type sbtype = sbData.SoundBlock.GetType();
				PropertyInfo[] pi = sbtype.GetProperties();
				hasSettings = false;
				foreach(PropertyInfo property in pi)
			    {
					if (IsPropertyToSave(property.Name)) {
						hasSettings = true;
						xmlelem2 = xmldoc.CreateElement(property.Name);
						object propertyValue = property.GetValue(sbData.SoundBlock,null);
						string temp="";
		  				TypeConverter tc=TypeDescriptor.GetConverter(propertyValue.GetType());
			          	if (tc.CanConvertTo(typeof(string))) {
			          		temp = (string) tc.ConvertToInvariantString(propertyValue);
			          	}
			          	else {
			          		Console.WriteLine("ERROR: can't serialize \""+property.Name+"\"");	          
				        }
						xmltext = xmldoc.CreateTextNode(temp);
						xmlelem2.AppendChild(xmltext);
						xmlelem.AppendChild(xmlelem2);
					}
			    }
				if (hasSettings) xmlsblock.AppendChild(xmlelem);
			}
			
			try {
				xmldoc.Save(filename); 
			}
			catch (Exception e) {
				Console.WriteLine(e.Message);
			}
		}
	}
}
