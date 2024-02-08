
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
namespace genaralskar.VRC.Dialog
{
	/// <summary>
	/// Handles storing and modifiying local variables.
	/// </summary>
	public class DialogVariables : UdonSharpBehaviour
	{
		[Header(
			"Default variable syntax: variableName valueOfVariable\n" +
			"IMPORTANT: variable names and values cannot contain a\n" +
			"',' (comma) or ';' (semicolon) or ' ' (space) or `{` or `}` (curly braces)\n" +
			"This may break setting vars."
			)]
		public string[] defaults = new string[] { "defaultVarsEdited false" };
		[Tooltip("When the game starts, the player's name will be added under the variable `name`")]
		public bool initializePlayerName = true;

		private DataDictionary varNameValueDict = new DataDictionary();

		private void Awake()
		{
			foreach (string varNameValue in defaults)
			{
				SetNameValuePair(varNameValue);
			}

			if(initializePlayerName)
			{
				SetVar("name", Networking.LocalPlayer.displayName);
			}
		}

		#region // Public API \\

		/// <summary>
		/// Add or Set a variable to a new value
		/// </summary>
		/// <param name="varName">Name of variable to modify</param>
		/// <param name="varVal">Value of variable</param>
		public void SetVar(string varName, string varVal)
		{
			if (varName == "") return;

			// remove quotes if it has them
			if(varVal[0] == '\"' && varVal[varVal.Length - 1] == '\"')
			{
				varVal = varVal.Substring(1, varVal.Length - 2);
			}

			// check if we already have that var stored, if not add as a new one
			if (varNameValueDict.TryGetValue(varName, out DataToken value))
			{
				varNameValueDict[varName] = varVal;
			}
			else
			{
				varNameValueDict.Add(varName, varVal);
			}
		}

		/// <summary>
		/// Add or Set a variable to a new value
		/// </summary>
		/// <param name="varName">Name of variable to modify</param>
		/// <param name="varVal">Value of variable</param>
		public void SetVar(string varName, float varVal)
		{
			SetVar(varName, varVal.ToString());
		}

		/// <summary>
		/// Add or Set a variable to a new value
		/// </summary>
		/// <param name="varName">Name of variable to modify</param>
		/// <param name="varVal">Value of variable</param>
		public void SetVar(string varName, bool varVal)
		{
			SetVar(varName, varVal.ToString().ToLower());
		}

		/// <summary>
		/// Remove a variable
		/// </summary>
		/// <param name="varName">Name of variable to remove</param>
		public void RemoveVar(string varName)
		{
			if (varNameValueDict.ContainsKey(varName))
			{
				varNameValueDict.Remove(varName);
			}
		}

		/// <summary>
		/// Add or set a variable to a new value
		/// </summary>
		/// <param name="nameValPair">string of name and value to add/set. syntax: "name,value"</param>
		public void SetNameValuePair(string nameValPair)
		{
			string[] split = SplitString(nameValPair);
			if (split != null)
				SetVar(split[0], split[1]);
		}

		/// <summary>
		/// Returns the value of a given variable, or a blank string
		/// </summary>
		/// <param name="name">Name of the variable to get the value of</param>
		/// <returns>string value of variable</returns>
		public DataToken GetVar(string name)
		{
			if (varNameValueDict.TryGetValue(name, out var value))
			{
				return value;
			}
			else
			{
				return new DataToken("");
			}
		}

		/// <summary>
		/// Check if a variable equals the inputed value
		/// </summary>
		/// <param name="varName">Name of the variable</param>
		/// <param name="varVal">Value to check against</param>
		/// <returns>If the variable value matches the provided value</returns>
		public bool CheckVar(string varName, string comparitor, string varVal)
		{
			DataToken storedValue;
			if (!varNameValueDict.TryGetValue(varName, out storedValue))
			{
				// var is not in the variable storage. return false
				Debug.LogWarning("Trying to check the value of a var that doesn't exist! Returning true. Trying to check var: " + varName);
				return true;
			}
			//DataToken storedValue = vars[varName];

			// equals comparitor as its own since it can be used with all vars
			if (comparitor == "==" || comparitor == "is")
			{
				return varNameValueDict[varName].String == varVal;
			}

			if(comparitor == "!=" || comparitor == "isnot")
			{
				return varNameValueDict[varName].String != varVal;
			}

			// assuming all numbers after this, so need to check everything is actually a number
			if (storedValue.String.StartsWith("\"") || varVal.StartsWith("\""))
			{
				Debug.LogError($"Trying to compare a string var, {varName} and/or {varVal}, with number comparitors: > < <= =>. This does not work, returning false");
				return false;
			}
			if (storedValue.String == "true" || storedValue.String == "false" || varVal == "true" || varVal == "false")
			{
				Debug.LogError($"Trying to compare a bool var, {varName} and/or {varVal}, with number comparitors: > < <= =>. This does not work, returning false");
				return false;
			}

			// the floats of both vars
			float storedFloatVal;
			if (!float.TryParse(storedValue.String, out storedFloatVal))
			{
				Debug.LogError($"Error trying to parse var {varVal} into a float! returning false.");
				return false;
			}

			float floatCompare;
			if (!float.TryParse(varVal, out floatCompare))
			{
				Debug.LogError($"Error trying to parse var {varVal} into a float! returning false.");
				return false;
			}

			// we're all numbers now baby
			switch (comparitor)
			{
				case "<":
					return storedFloatVal < floatCompare;

				case ">":
					return storedFloatVal > floatCompare;

				case "<=":
					return storedFloatVal <= floatCompare;

				case ">=":
					return storedFloatVal >= floatCompare;

				default:
					return false;
			}
		}

		/// <summary>
		/// Does a var comparison with a given comparitor string. ie "varOne >= varTwo"
		/// </summary>
		/// <param name="comparitor">String containing three terms for a comparison, ie "varOne >= varTwo"</param>
		/// <returns>Value of comparison statement</returns>
		public bool CheckVarString(string comparitor)
		{
			string[] terms = comparitor.Split(' ');
			if(terms.Length < 3)
			{
				Debug.LogError($"Could not parse {comparitor} when trying to check its compare value");
				return false;
			}

			return CheckVar(terms[0], terms[1], terms[2]);
		}

		/// <summary>
		/// Returns JSON string of current variables
		/// </summary>
		/// <param name="jsonExportType">JsonExportType</param>
		/// <returns>JSON string of current variables</returns>
		public string GetVarsJSON(JsonExportType jsonExportType = JsonExportType.Beautify)
		{
			string jsonString = "";

			if (VRCJson.TrySerializeToJson(varNameValueDict, JsonExportType.Beautify, out DataToken json))
			{
				jsonString = json.String;
			}

			return jsonString;
		}

		#endregion \\ Public API //

		/// <summary>
		/// Checks and splits a given string to make sure it is a valid name value pair
		/// </summary>
		/// <param name="nameValPair"></param>
		/// <returns>string array split on ' '</returns>
		private string[] SplitString(string nameValPair)
		{
			string[] split = nameValPair.Split(' ');
			if (split.Length != 2)
			{
				Debug.LogWarning($"Could not split nameValPair {nameValPair}. Possible invalid syntax");
				return null;
			}
			return split;
		}
	}
}
