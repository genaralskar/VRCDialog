
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace genaralskar.VRC.Dialog
{
    public class DialogSetVariable : UdonSharpBehaviour
    {
        public DialogVariables dialogVariables;
        [Tooltip("Variables paired with thier values, seperated by a ' ' (space). ie 'score 10', 'gotBlueKey true'")]
        public string[] nameValuePairs;

        public void SetVars()
        {
            foreach(var v in nameValuePairs)
            {
                dialogVariables.SetNameValuePair(v);
            }
        }

        public override void Interact()
        {
            SetVars();
        }
    }
}