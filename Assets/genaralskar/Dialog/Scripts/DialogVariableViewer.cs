
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using genaralskar.VRC.Dialog;
using TMPro;
using VRC.SDK3.Data;

public class DialogVariableViewer : UdonSharpBehaviour
{
	public TextMeshProUGUI text;
	public DialogVariables vars;

	public void DisplayVars()
	{
		string json = vars.GetVarsJSON();

		text.text = json;
	}
}
