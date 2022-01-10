
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
namespace genaralskar.VRC.Dialog
{
    /// <summary>
    /// Handles storing and modifiying local variables.
    /// </summary>
    public class DialogVariables : UdonSharpBehaviour
    {
        [Header("IMPORTANT: variable names and values cannot contain a ',' (comma) or ';' (semicolon). This may break setting vars.")]
        public string[] varNames = new string[0];
        public string[] varVals = new string[0];

        /// <summary>
        /// Add or Set a variable to a new value
        /// </summary>
        /// <param name="varName">Name of variable to modify</param>
        /// <param name="varVal">Value of variable</param>
        public void SetVar(string varName, string varVal)
        {
            for (int i = 0; i < varNames.Length; i++)
            {
                if(varNames[i] == varName)
                {
                    varVals[i] = varVal;
                    return;
                }
            }
            // if no var with varName is found, add a new variable
            AddVar(varName, varVal);
        }

        /// <summary>
        /// Add or set a variable to a new value
        /// </summary>
        /// <param name="nameValPair">string of name and value to add/set. syntax: "name,value"</param>
        public void SetNamValPair(string nameValPair)
        {
            string[] split = SplitString(nameValPair);
            if(split != null)
                SetVar(split[0], split[1]);
        }

        /// <summary>
        /// Returns the value of a given variable, or a blank string
        /// </summary>
        /// <param name="name">Name of the variable to get the value of</param>
        /// <returns>string value of variable</returns>
        public string GetVar(string name)
        {
            for(int i = 0; i < varNames.Length; i++)
            {
                if(varNames[i] == name)
                    return varVals[i];
            }
            return "";
        }

        // Reconstructs the arrays to make room for new vars
        // Rewrite this to allow for a "default" size, and filling an array before creating new "cells"
        private void AddVar(string varName, string varVal)
        {
            // create new arrays of one size larger
            string[] nameTemp = new string[varNames.Length + 1];
            string[] valTemp = new string[varVals.Length + 1];

            // populate the new arrays with old one 
            for (int i = 0; i < varNames.Length; i++)
            {
                nameTemp[i] = varNames[i];
                valTemp[i] = varVals[i];
            }

            // add new value to end of array
            nameTemp[nameTemp.Length-1] = varName;
            valTemp[valTemp.Length-1] = varVal;

            // update name array with new array
            varNames = nameTemp;
            varVals = valTemp;
        }

        /// <summary>
        /// Check if a variable equals the inputed value
        /// </summary>
        /// <param name="varName">Name of the variable</param>
        /// <param name="varVal">Value to check against</param>
        /// <returns>If the variable value matches the provided value</returns>
        public bool CheckVar(string varName, string varVal)
        {
            for (int i = 0; i < varNames.Length; i++)
            {
                if(varNames[i] == varName)
                {
                    return varVals[i] == varVal;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks a string of Name Value pairs and returns true if all are true
        /// </summary>
        /// <param name="nameValPair"></param>
        /// <returns>bool of if all provided valName pairs are true</returns>
        public bool CheckNameValPair(string nameValPair)
        {
            // Split given input into multiple nameValPairs seperated by a ';'
            string[] pairSplit = nameValPair.Split(';');
            string[] split;

            foreach(var s in pairSplit)
            {
                // split each pair with ',' then check their values. If the pair is too short, or the values don't match, return false
                split = s.Split(',');
                if(split.Length < 2)
                {
                    Debug.LogWarning($"Could not split nameValPair {nameValPair}. Possible invalid syntax. Returning false");
                    return false;
                }
                if(!CheckVar(split[0], split[1]))
                {
                    return false;
                }
            }

            // if all previous checks passed, return true.
            return true;
        }

        /// <summary>
        /// Remove a variable
        /// </summary>
        /// <param name="varName">Name of variable to remove</param>
        public void RemoveVar(string varName)
        {
            // create new array of one size smaller
            string[] nameTemp = new string[varNames.Length - 1];
            string[] valTemp = new string[varVals.Length + 1];
            // populate the new array, skipping the var to be removed
            int tempi = 0;
            for (int i = 0; i < varNames.Length; i++)
            {
                // populate with all vars except the one to be removed
                if(varNames[i] != varName)
                {
                    nameTemp[tempi] = varNames[i];
                    valTemp[tempi] = varVals[i];
                    tempi++;
                }
            }
            // update arrays with new arrays
            varNames = nameTemp;
            varVals = valTemp;
        }

        private string[] SplitString(string nameValPair)
        {
            string[] split = nameValPair.Split(',');
            if (split.Length != 2)
            {
                Debug.LogWarning($"Could not split nameValPair {nameValPair}. Possible invalid syntax");
                return null;
            }
            return split;
        }
    }
}
