
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace genaralskar.VRC.Dialog
{
    /// <summary>
    /// Handles selection buttons for interacting with DialogManager
    /// </summary>
    public class DialogButton : UdonSharpBehaviour
    {
        public DialogManager dialogManager;
        public int optionIndex;
        public Button button;
        public TextMeshProUGUI buttonText;

        /// <summary>
        /// Selects this button, like it was clicked on
        /// </summary>
        public void SelectButton()
        {
            dialogManager.SelectOption(optionIndex);
        }

        /// <summary>
        /// Sets the text of this button
        /// </summary>
        /// <param name="text"></param>
        public void SetText(string text)
        {
            buttonText.text = text;
        }
    }
}