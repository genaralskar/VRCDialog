
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace genaralskar.VRC.Dialog
{
    /// <summary>
    /// Hold information in individual Dialog segments including the dialog and dialog options.
    /// </summary>
    public class Dialog : UdonSharpBehaviour
    {
        public string nodeName;
        public DataDictionary lineNameDict = new DataDictionary();
        public DataDictionary lineTextDict = new DataDictionary();

        [Header("Dialog Options")]
        public DataDictionary optionNodeConnection = new DataDictionary();
        public DataDictionary optionVarChecks = new DataDictionary();

        //[Header("Write node visit")]
        //public bool writeNodeVisitToVars = false;
        //[Tooltip("Name of the variable to store in the dialog vars script. '_visted' will always be added automatically to this.")]
        //public string varNameToWrite;


        /// <summary>
        /// Returns the dialog string at given index
        /// </summary>
        /// <param name="lineNumber"></param>
        /// <returns></returns>
        public string GetDialog(int lineNumber)
        {
            if (lineNumber >= lineTextDict.Count)
                return "";

			DataList keys = lineTextDict.GetKeys();


            return lineTextDict[keys[lineNumber]].String;
        }

        /// <summary>
        /// Gets the name for the given dialog line number
        /// </summary>
        /// <param name="lineNumber"></param>
        /// <returns></returns>
        public string GetDialogName(int lineNumber)
		{
            if (lineNumber >= lineTextDict.Count)
                return "";

            DataList keys = lineNameDict.GetKeys();

            return lineNameDict[keys[lineNumber]].String;
		}

        /// <summary>
        /// Returns list of all dialog options
        /// </summary>
        /// <returns></returns>
        public DataDictionary GetDialogOptions()
        {
            return optionNodeConnection;
        }

        /// <summary>
        /// Returns dictionary of conditionals (variable checks) for the node
        /// </summary>
        /// <returns></returns>
        public DataDictionary GetDialogVarChecks()
		{
            return optionVarChecks;
		}
    }

}
