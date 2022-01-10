
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace genaralskar.VRC.Dialog
{
    public class DialogSetVariable : UdonSharpBehaviour
    {
        public DialogVariables dialogVariables;
        [Tooltip("Variables paired with thier values, seperated by a ',' (comma). ie 'score,10', 'gotBlueKey,true'")]
        public string[] nameValuePairs;

        public void SetVars()
        {
            foreach(var v in nameValuePairs)
            {
                dialogVariables.SetNamValPair(v);
            }
        }

        public override void Interact()
        {
            SetVars();
        }
    }
}