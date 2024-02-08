
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace genaralskar.VRC.Dialog
{
	/// <summary>
	/// Handles displaying and running through different dialogs.
	/// </summary>
	public class DialogManager : UdonSharpBehaviour
	{
		[Header("Dialog Variables")]
		public DialogVariables diaVars;
		[Tooltip("Will add a variable to diaVars with \"{nameOfNode}_visted,1\" when a dialog node is seen")]
		public bool writeAllNodeVists;

		[Header("UI")]
		public GameObject dialogCanvas;
		public RectTransform rebuildRect;
		public TextMeshProUGUI dialogTMP;
		public DialogButton continueButton;
		public DialogButton[] dialogOptionButtons;

		[Space]
		[Tooltip("Options for Dynamically sizing UI. The TextMesh and main Panel must have a ContentSizeFitter component to work.")]
		public bool dynamicUI = false;
		public ContentSizeFitter sizeFitterText;
		public ContentSizeFitter sizeFitterBox;
		[Tooltip("If set to true, the dialog canvas will move to whoever is talking in a line, if GameObjects with matching names are found. Otherwise will hover around the player.\n" +
			"If false, the canvas will not move at all.")]
		public bool movingCanvas = true;

		[Header("Run at start")]
		public bool runAtStart = false;
		//public Dialog startDialog;
		public DialogParser parser;
		public string startDialogTitle;

		public DataDictionary nameNodeDict = new DataDictionary();

		private bool runningDialog = false;
		private Dialog currentDialog;
		private int currentDialogNodeLine;
		private string dialogString;
		private DataDictionary currentOptionsNodeConnectionsDict;
		private DataDictionary currentOptionVariableCompareDict;
		private DataDictionary filteredOptions;
		private DataList filteredKeys;

		private DataDictionary nameGameObjectDict = new DataDictionary();

		private GameObject currentLineGameObject;
		private float yEulerDesiredRot;
		private Quaternion targetRot;
		private Vector3 targetPos;

		private void Start()
		{
			EndDialog();

			sizeFitterText.enabled = dynamicUI;
			sizeFitterBox.enabled = dynamicUI;

			// parse dialog
			nameNodeDict = parser.Parse(parser.input);
			// get gameobjects by name
			nameGameObjectDict = parser.GetNameGameObjectDict();

			if (runAtStart)
			{
				StartDialogByNodeName(startDialogTitle);
			}
		}

		private void Update()
		{
			if (movingCanvas)
				UpdateDialogCanvasPosition();
		}

		public void StartDialogByNodeName(string nodeName)
		{
			if (nameNodeDict.TryGetValue(nodeName, out DataToken node))
			{
				Dialog d = (Dialog)node.Reference;
				StartDialog(d);
			}
			else
			{
				Debug.LogWarning("Could not find dialog node with name: " + nodeName);
			}
		}

		/// <summary>
		/// Starts displaying a specified dialog
		/// </summary>
		/// <param name="dialog">Dialog to start displying</param>
		public void StartDialog(Dialog dialog)
		{
			if (runningDialog)
			{
				EndDialog();
			}

			dialogCanvas.SetActive(true);

			currentDialog = dialog;
			currentDialogNodeLine = 0;
			currentOptionsNodeConnectionsDict = new DataDictionary();
			currentOptionVariableCompareDict = new DataDictionary();
			filteredOptions = new DataDictionary();
			runningDialog = true;

			ContinueDialog();
		}

		private void ContinueDialog()
		{
			string lineName = currentDialog.GetDialogName(currentDialogNodeLine);
			SetupCurrentLineGameObject(lineName);

			// get current dialog
			dialogString = currentDialog.GetDialog(currentDialogNodeLine++);
			// process the string to replace {} with inline variables
			dialogString = GetInlineVariables(dialogString);


			// write node visit to vars
			if (writeAllNodeVists)
			{
				string name = currentDialog.name;
				diaVars.SetVar($"{name}_visted", true);
			}

			if (dialogString.StartsWith("<<"))
			{
				// process command!
				ProcessCommand(dialogString);
				ContinueDialog();
				return;
			}

			if (dialogString == "")
			{
				EndDialog();

			}
			// if the next dialog is empty and the dialog has options, display them
			else if (currentDialog.GetDialog(currentDialogNodeLine) == "")
			{
				currentOptionsNodeConnectionsDict = currentDialog.GetDialogOptions();
				currentOptionVariableCompareDict = currentDialog.GetDialogVarChecks();
				if (currentOptionsNodeConnectionsDict.GetKeys().Count > 0)
				{
					FilterOptions();
				}
				else
				{
					DisplayContinueButton(false);
				}
			}
			else
			{
				DisplayContinueButton(true);
			}

			DisplayText();

		}

		/// <summary>
		/// Used by external buttons to continue or select a dialog option.
		/// </summary>
		/// <param name="index">Index of option to select. -1 continues dialog</param>
		public void SelectOption(int index)
		{
			// -1 is continue
			if (index == -1)
				ContinueDialog();
			else
			{
				runningDialog = false;
				StartDialogByNodeName(filteredOptions[filteredKeys[index]].String);
			}
		}

		/// <summary>
		/// Immedielty ends the current dialog
		/// </summary>
		public void EndDialog()
		{
			// disable stuff
			dialogCanvas.SetActive(false);
		}

		private void DisplayText()
		{
			// set option1 button to Continue
			dialogTMP.text = dialogString;

			if (dynamicUI)
			{
				sizeFitterText.SetLayoutVertical();
				sizeFitterBox.SetLayoutVertical();
			}
		}

		private void FilterOptions()
		{
			filteredOptions = new DataDictionary();
			DataList currentOptionKeys = currentOptionsNodeConnectionsDict.GetKeys();

			int optionsCounter = 0;
			for (; optionsCounter < currentOptionKeys.Count; optionsCounter++)
			{
				string key = currentOptionKeys[optionsCounter].String;

				string optionValue = currentOptionsNodeConnectionsDict[key].String;

				// check if there's a var check on this option!
				bool varCheckSuccessful = true;
				if (currentOptionVariableCompareDict.TryGetValue(key, out DataToken value))
				{
					// if we have no dialog var then just show every option
					if (diaVars != null)
						varCheckSuccessful = diaVars.CheckVarString(value.String);
				}

				if (optionValue != "" && varCheckSuccessful)
				{
					filteredOptions.Add(key, currentOptionsNodeConnectionsDict[key]);
				}
			}

			filteredKeys = filteredOptions.GetKeys();


			if (optionsCounter == 0)
			{
				DisplayContinueButton(true);
			}
			else
			{

				DisplayOptionButtons();
			}
		}

		private void DisplayOptionButtons()
		{
			continueButton.gameObject.SetActive(false);
			// loop through filtered options, only displaying up to how many option buttons we have
			// TODO: Add auto pagination?
			int i = 0;
			for (; i < filteredOptions.Count && i < dialogOptionButtons.Length; i++)
			{
				dialogOptionButtons[i].gameObject.SetActive(true);
				// if dialog option text is not setup or blank, override with a default
				string optionText = filteredKeys[i].String;
				string nodeConnection = filteredOptions[filteredKeys[i]].String;

				// use node name if there is no option text
				if (optionText == "")
					optionText = nodeConnection;

				dialogOptionButtons[i].SetText(optionText);
			}
			// hide all further options
			for (; i < dialogOptionButtons.Length; i++)
			{
				dialogOptionButtons[i].gameObject.SetActive(false);
			}
		}

		private void DisplayContinueButton(bool continueDialog)
		{
			for (int i = 0; i < dialogOptionButtons.Length; i++)
			{
				dialogOptionButtons[i].gameObject.SetActive(false);
			}

			continueButton.gameObject.SetActive(true);
			if (continueDialog)
			{
				continueButton.SetText("Continue");
			}
			else
			{
				continueButton.SetText("End");
			}
		}

		private string GetInlineVariables(string line)
		{
			if (!diaVars) return line;
			if (line == "") return line;

			string newLine = "";
			int charIndex = 0;
			while (charIndex < line.Length && (int)line[charIndex] != 13)
			{
				if (line[charIndex] == '{')
				{
					// move past the {
					charIndex++;
					string varName = "";
					while (line[charIndex] != '}')
					{
						varName += line[charIndex++];
					}

					DataToken varValue = diaVars.GetVar(varName).String;
					newLine += varValue.String;
					charIndex++; // move past the }
				}
				else
				{
					newLine += line[charIndex++];
				}

			}
			return newLine;
		}

		/// <summary>
		/// Finds and sets currentLineGameObject based on given name
		/// </summary>
		/// <param name="name">Name of the GameObject to find</param>
		private void SetupCurrentLineGameObject(string name)
		{
			if (name.Length == 0)
			{
				currentLineGameObject = null;
				return;
			}

			// check if we have a gameobject for that name registered
			if (nameGameObjectDict.TryGetValue(name, out DataToken value))
			{
				currentLineGameObject = (GameObject)value.Reference;
			}
			else
			{
				currentLineGameObject = null;
			}
		}

		/// <summary>
		/// Given a dialog line that is a command, read it and do the associated command
		/// </summary>
		/// <param name="line">Line of dialog that is a command</param>
		private void ProcessCommand(string line)
		{
			if (!line.StartsWith("<<"))
			{
				Debug.LogError("Trying to process a command with inproper syntax! Skipping:\n" + line);
				return;
			}

			// start at 2 to skip the <<
			int charIndex = 2;
			string command = "";
			while (line[charIndex] != ' ')
			{
				command += line[charIndex++];
			}
			charIndex++; // for the space at the end
			if (command == "set" && diaVars != null)
			{
				string varName = "";

				while (line[charIndex] != ' ')
				{
					varName += line[charIndex++];
				}

				charIndex++; // for the space at the end

				string varValue = "";
				// while in bounds of the string, if we reach a space seperator break, or if we reach >> break
				while (!IsEndOfCommandLine(line, charIndex))
				{
					varValue += line[charIndex++];
				}

				diaVars.SetVar(varName, varValue);
			}
			// check this is the right command for deleting a variable
			else if (command == "remove" && diaVars != null)
			{
				string varName = "";

				while (!IsEndOfCommandLine(line, charIndex))
				{
					varName += line[charIndex++];
				}

				diaVars.RemoveVar(varName);
			}
			else if (command == "sendCustomEvent" || command == "SendCustomEvent")
			{
				string objectName = "";

				while (line[charIndex] != ' ')
				{
					objectName += line[charIndex++];
				}
				charIndex++; // for the space inbetween

				string eventName = "";
				while (!IsEndOfCommandLine(line, charIndex))
				{
					eventName += line[charIndex++];
				}

				// find object with name and send the event, if it has
				if(nameGameObjectDict.TryGetValue(objectName, out DataToken obj))
				{
					GameObject go = (GameObject)obj.Reference;

					// UdonBehavior gets both node and sharp behaviors
					UdonBehaviour[] nodeTargets = go.GetComponents<UdonBehaviour>();
					foreach(var target in nodeTargets)
					{
						target.SendCustomEvent(eventName);
					}
				}
			}
			else if (command == "sendCustomNetworkEvent" || command == "SendCustomNetworkEvent")
			{
				string objectName = "";

				while (line[charIndex] != ' ')
				{
					objectName += line[charIndex++];
				}
				charIndex++; // for the space inbetween

				string targetTypeString = "";
				while (line[charIndex] != ' ')
				{
					targetTypeString += line[charIndex++];
				}
				if(targetTypeString != "All" && targetTypeString != "Owner")
				{
					Debug.LogError($"TargetType for sendCustomNetworkEvent is incorrect! Supported type are All and Owner. Currently set to {targetTypeString}");
				}
				charIndex++;

				string eventName = "";
				while (!IsEndOfCommandLine(line, charIndex))
				{
					eventName += line[charIndex++];
				}

				// find object with name and send the event, if it has
				if (nameGameObjectDict.TryGetValue(objectName, out DataToken obj))
				{
					GameObject go = (GameObject)obj.Reference;
					NetworkEventTarget targetType = targetTypeString == "All" ? NetworkEventTarget.All : NetworkEventTarget.Owner;

					// UdonBehavior gets both node and sharp behaviors
					UdonBehaviour[] nodeTargets = go.GetComponents<UdonBehaviour>();
					foreach (var target in nodeTargets)
					{
						target.SendCustomNetworkEvent(targetType, eventName);
					}
				}
			}
		}

		private bool IsEndOfCommandLine(string line, int charIndex)
		{
			return !(charIndex < line.Length && line[charIndex] != ' ' && !(line[charIndex] == '>' && line[charIndex + 1] == '>'));
		}


		private void UpdateDialogCanvasPosition()
		{
			Vector3 playerPos = Networking.LocalPlayer.GetPosition();

			if (currentLineGameObject != null)
			{
				// hover around gameobject
				targetPos = currentLineGameObject.transform.position;
				targetPos.y = playerPos.y;

				Vector3 dir = (playerPos - targetPos).normalized;
				targetPos += dir * 1f;

			}
			else
			{
				// hover around player
				// check if player rot when opening the dialog has moved.
				Vector3 facingDir = (Networking.LocalPlayer.GetRotation() * Vector3.forward).normalized;
				Vector3 newPos = playerPos + facingDir * 2f;

				if ((targetPos - playerPos).sqrMagnitude < 3.1f || (newPos - targetPos).sqrMagnitude > 1f)
					targetPos = Vector3.Lerp(targetPos, newPos, Time.deltaTime * 10);
			}

			transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 10);

			RotateDialogCanvasToFacePlayer();
		}

		private void RotateDialogCanvasToFacePlayer()
		{
			float playerYRot = Networking.LocalPlayer.GetRotation().eulerAngles.y;

			if (Mathf.Abs(yEulerDesiredRot - playerYRot) > 30)
			{

			}
			// get direction to player
			Vector3 dir = Networking.LocalPlayer.GetPosition() - transform.position;
			dir.y = 0;
			dir = -dir.normalized;

			yEulerDesiredRot = playerYRot;

			// get quaterion in that direction
			targetRot = Quaternion.LookRotation(dir, Vector3.up);


			// lerp in that direction
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10);
		}
	}
}
