
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace genaralskar.VRC.Dialog
{
    /// <summary>
    /// Hold information in individual Dialog segments including the dialog and dialog options.
    /// </summary>
    public class Dialog : UdonSharpBehaviour
    {
        [Header("Dialog")]
        public string[] dialogs;

        [Header("Dialog Options")]
        public Dialog[] dialogOptions;
        public string[] dialogOptionText;
        public string[] varNameValPairs;

        [Header("Write node visit")]
        public bool writeNodeVisitToVars = false;
        [Tooltip("Name of the variable to store in the dialog vars script. '_visted' will always be added automatically to this.")]
        public string varNameToWrite;

        //[Header("Events, unimpelmented")]
        //public Button OnDialogStartEvent;
        //public Button OnDialogEndEvent;


        /// <summary>
        /// Returns the dialog string at given index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string GetDialog(int index)
        {
            if (index > dialogs.Length - 1)
                return "";
            return dialogs[index];
        }

        /// <summary>
        /// Returns list of all dialog options
        /// </summary>
        /// <returns></returns>
        public Dialog[] GetDialogOptions()
        {
            return dialogOptions;
        }
    }

}
