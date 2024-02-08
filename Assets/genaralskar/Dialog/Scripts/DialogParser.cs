
using System.Collections.Generic;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace genaralskar.VRC.Dialog
{
	public class DialogParser : UdonSharpBehaviour
	{
		[Multiline]
		public string input;

		public GameObject DialogNode;

		//private DataDictionary nameNodesDict = new DataDictionary();

		// dictionary for tying character names to gameobjects in the scene
		private DataDictionary nameGameObjectDict = new DataDictionary();
		private bool nameGameObjectDictPopulated = false;
		private DataList gameObjectsToFindByName = new DataList();

		public DataDictionary Parse(string input)
		{
			DataDictionary nameNodesDict = new DataDictionary();

			//split at new lines so we get array of lines to loop through
			string[] lines = input.Split('\n');

			// look for a line staring with "title:"
			int lineIndex = 0;
			while (lineIndex < lines.Length)
			{
				if (lines[lineIndex].StartsWith("title:"))
				{
					if (!lines[lineIndex + 1].StartsWith("---"))
					{
						Debug.LogError("ERROR Parsing script. Format not correct, line after title: should start with `---`. line " + lineIndex);
						break;
					}

					// store the title!
					string title = lines[lineIndex].Substring(7);
					// remove any lingering carrage returns
					title = title.Replace("\r", "");

					// create a new node!
					GameObject nodeGO = Instantiate(DialogNode, transform);
					nodeGO.name = title;
					Dialog node = nodeGO.GetComponent<Dialog>();

					// add to dictionary to reference later!
					nameNodesDict.Add(title, node);

					// place to save all the lines of dialog
					DataDictionary lineTextDict = new DataDictionary();
					DataDictionary lineNamesDict = new DataDictionary();
					DataDictionary optionNodeDict = new DataDictionary();
					DataDictionary optionVarCheck = new DataDictionary();

					lineIndex++; // ---
					lineIndex++; // first line with dialog


					// loop through lines until we get to ===, then exit node population
					// i is line number
					while (!lines[lineIndex].StartsWith("==="))
					{
						bool firstSpaceFound = false;
						int charIndex = 0;
						string line = lines[lineIndex];
						string name = "";
						string text = "";
						// go through the line to parse whatever we need, like << commands and things
						while (charIndex < line.Length)
						{
							char c = line[charIndex];

							//=============== -> 
							// if the line starts with '-> ', its an option!
							// will have to gather a list of options that are one after another to have the multiple options to select
							// TODO: anytime we end our options list, go into creating a new node? if it jumps somewhere it will be a new node, and if it continues in the same node, just have it link to a new one starting after the -> 's
							if (charIndex == 0 && c == '-' && line[charIndex + 1] == '>' && line[charIndex + 2] == ' ')
							{
								// check if the next line starts with ->. if it doesn't, then we're done creating this node.
								charIndex += 3;

								// gather the string text
								string optionText = "";
								string conditionalText = "";
								while (charIndex < line.Length)
								{
									// THERES A CONDITIONAL ON THIS THAR VAR WAAAT!!
									// `<<if `
									//if (line[charIndex] == '<' && line[charIndex + 1] == '<' && line[charIndex + 2] == 'i' && line[charIndex + 3] == 'f' && line[charIndex + 4] == ' ')
									if (DoesCharStartWithString(line, "<<if ", charIndex))
									{
										charIndex += 5;

										// add a var check to this option!
										//while (!(line[charIndex] == '>' && line[charIndex + 1] == '>'))
										while (!DoesCharStartWithString(line, ">>", charIndex))
										{
											// <<if $var is "name">>
											// <<if $var is true>>
											// <<if $var is 10>>
											// <<if $var < 14>>
											// if starts with ", its a string. `true` or `false` is bool. anything else is a number.
											conditionalText += line[charIndex];
											charIndex++;
										}
										charIndex += 2;
									}
									else
									{
										optionText += line[charIndex];
										charIndex++;
									}
								}

								string optionNodeName = GetNodeNameFromJumpCommand(lines[lineIndex + 1]);
								if (optionNodeName != "") lineIndex++;

								// save option text and conditional!
								if (optionText != "")
								{
									optionNodeDict.Add(optionText, optionNodeName);
									if (conditionalText != "")
									{
										optionVarCheck.Add(optionText, conditionalText);
									}
								}
							}




							// ====================== tab
							// if it's tabbed, its a new node, kinda... or it jumps to a new node.
							// probably dont support nexted options right now. if they want that they can make a new node!
							// or maybe recursive node creation?
							else if (c == '\t')
							{

							}





							// ======================= { inline variable
							// we're trying to display a variable here!
							//else if (c == '{')
							//{
							//	string varName = "";
							//	charIndex++;
							//	while (!(line[charIndex] == '}'))
							//	{
							//		// loop through variable to get its name
							//		varName += line[charIndex];
							//		charIndex++;
							//	}
							//	// TODO: Find variable name, and insert it here. just putting varName here for testing
							//	text += varName;
							//}

							// first space!
							else if (!firstSpaceFound && c == ' ')
							{
								firstSpaceFound = true;
								text += ' ';
							}

							// first : before any spaces! indicates a name!
							else if (!firstSpaceFound && c == ':')
							{
								// setup previous part of string as name!
								name = line.Substring(0, charIndex); // charIndex - 1 maybe to get rid of the space
																	 // reset text since we don't want to add the name to it
								text = "";

								// add the name to our list of names to find later
								if (!gameObjectsToFindByName.Contains(name))
								{
									gameObjectsToFindByName.Add(name);
								}
							}




							//============================= << command!
							// COMMAND
							// store commands as lines of dialog, and process them while the dialog is running.
							// unless maybe we wanna get a list of gameobjects that have commands tied?
							else if (DoesCharStartWithString(line, "<<", charIndex))
							{
								// START OF A COMMAND!
								// if, elseif, endif
								// jump
								// need to check if its at start of line for command/main if statement, or if its at the end of an option line.

								charIndex += 2;
								string command = "";
								bool commandFound = false;
								bool commandProcessed = false;

								while (!DoesCharStartWithString(line, ">>", charIndex))
								{
									if(!commandFound)
									{
										while (line[charIndex] != ' ')
										{
											command += line[charIndex++];
										}
										commandFound = true;
									}

									// check if the command is something that targets a gameobject. if it does we need to store that gameobject!
									if (!commandProcessed && commandFound && (command == "sendCustomEvent" || command == "SendCustomEvent" || command == "sendCustomNetworkEvent" || command == "SendCustomNetworkEvent"))
									{
										string objectName = "";
										charIndex++;

										while (line[charIndex] != ' ')
										{
											objectName += line[charIndex++];
										}
										if (objectName.Length != 0 || !gameObjectsToFindByName.Contains(objectName))
										{
											gameObjectsToFindByName.Add(objectName);
										}

										command += " " + objectName;

										commandProcessed = true;
									}

									command += line[charIndex++];
								}

								//commands are saved in the line and parsed and executed in the DialogManager
								charIndex += 2;
								text += "<<" + command + ">>";
							}

							// no special command, just add the char to the text line
							else
							{
								text += c;
							}
							charIndex++;
						}
						if (text != "")
						{
							// finished parsing the line. save it somewhere
							lineTextDict.Add(lineIndex, text);
							lineNamesDict.Add(lineIndex, name);
							//if(name != "")
							//{
							//}
						}

						// parse the next line.
						lineIndex++;
					}
					// done with all lines in the node!
					// add all the saved data to the node info!
					node.lineNameDict = lineNamesDict;
					node.lineTextDict = lineTextDict;

					// add options
					node.optionNodeConnection = optionNodeDict;
					node.optionVarChecks = optionVarCheck;

					// on to the next node!
				}
				lineIndex++;
				// this is the if line starts with title: check. basically if it doesnt we just ignore everything until something starts with title:
			}
			// done with all lines in the text file! hurray!

			// prepopulate gameobject finders
			GetNameGameObjectDict();

			return nameNodesDict;
		}

		public DataDictionary GetNameGameObjectDict()
		{

			if (nameGameObjectDictPopulated)
				return nameGameObjectDict;

			for (int i = 0; i < gameObjectsToFindByName.Count; i++)
			{
				string nameToFind = gameObjectsToFindByName[i].String;
				GameObject go = GameObject.Find(nameToFind);

				if (go != null && !nameGameObjectDict.ContainsKey(nameToFind))
				{
					nameGameObjectDict.Add(nameToFind, go);
				}
			}

			nameGameObjectDictPopulated = true;

			return nameGameObjectDict;
		}

		private string GetNodeNameFromJumpCommand(string line)
		{
			string nodeName = "";

			int charIndex = 0;

			while (charIndex < line.Length)
			{
				// command found `<<`
				if (DoesCharStartWithString(line, "<<jump", charIndex))
				{
					charIndex += 7;
					// get the name until end of line or ` >>`
					while (charIndex < line.Length && !DoesCharStartWithString(line, ">>", charIndex))
					{
						nodeName += line[charIndex];
						charIndex++;
					}
				}
				charIndex++;
			}

			return nodeName;
		}

		private bool DoesCharStartWithString(string input, string toCheck, int charIndex)
		{
			for (int i = 0; i < toCheck.Length; i++)
			{
				if (input[charIndex + i] != toCheck[i])
				{
					return false;
				}
			}

			return true;
		}

		private void ParseNewNode(string[] lines)
		{

		}
	}
}