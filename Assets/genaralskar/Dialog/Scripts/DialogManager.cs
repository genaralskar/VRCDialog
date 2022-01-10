
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
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

        [Header("Run at start")]
        public bool runAtStart = false;
        public Dialog startDialog;

        private bool runningDialog = false;
        private Dialog currentDialog;
        private int currentDialogIndex;
        private string dialogString;
        private Dialog[] currentOptions;
        private Dialog[] filteredOptions;

        private void Start()
        {
            EndDialog();

            sizeFitterText.enabled = dynamicUI;
            sizeFitterBox.enabled = dynamicUI;

            if(runAtStart)
            {
                StartDialog(startDialog);
            }
        }

        /// <summary>
        /// Starts displaying a specified dialog
        /// </summary>
        /// <param name="dialog">Dialog to start displying</param>
        public void StartDialog(Dialog dialog)
        {
            if (dialog == null)
            {
                if(startDialog == null)
                {
                    EndDialog();
                    return;
                }
                dialog = startDialog;
            }

            if (runningDialog)
            {
                EndDialog();
            }

            dialogCanvas.SetActive(true);

            currentDialog = dialog;
            currentDialogIndex = 0;
            currentOptions = new Dialog[0];
            filteredOptions = new Dialog[0];
            runningDialog = true;

            ContinueDialog();

        }

        private void ContinueDialog()
        {
            dialogString = currentDialog.GetDialog(currentDialogIndex++);

            // write node visit to vars
            if(writeAllNodeVists || currentDialog.writeNodeVisitToVars)
            {
                string name = currentDialog.varNameToWrite;
                if(string.IsNullOrEmpty(name))
                {
                    name = currentDialog.name;
                }
                diaVars.SetNamValPair($"{name}_visted,1");
            }

            // if reached the end of the dialog, end it
            if (dialogString == "")
            {
                EndDialog();
            }
            // if the next dialog is empty and the dialog has options, display them
            else if (currentDialog.GetDialog(currentDialogIndex) == "")
            {
                // check if dialog as options
                currentOptions = currentDialog.GetDialogOptions();
                if (currentOptions.Length > 0)
                {
                    FilterOptions();
                }
                else
                {
                    DisplayContinue(true);
                }
            }
            else
            {
                DisplayContinue(false);
            }

            DisplayText();

        }

        /// <summary>
        /// Used by external buttons to continue or select a dialog option.
        /// </summary>
        /// <param name="index">Index of option to select. -1 continues dialog</param>
        public void SelectOption(int index)
        {
            if (index == -1)
                ContinueDialog();
            else
            {
                runningDialog = false;
                StartDialog(currentDialog.dialogOptions[index]);
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

            if(dynamicUI)
            {
                sizeFitterText.SetLayoutVertical();
                sizeFitterBox.SetLayoutVertical();
            }
        }

        private void FilterOptions()
        {
            filteredOptions = new Dialog[dialogOptionButtons.Length];
            int optionsCounter = 0;
            for (int i = 0; i < currentOptions.Length; i++)
            {
                // check if vars exist
                // check if var list is shorter than itteration
                // check if the current option has nameVal pairs to check
                // check if dialog vars is assigned
                // check if vars are blank
                if (currentDialog.varNameValPairs == null ||
                    currentDialog.varNameValPairs.Length <= i ||
                    string.IsNullOrEmpty(currentDialog.varNameValPairs[i]) ||
                    diaVars == null ||
                    diaVars.CheckNameValPair(currentDialog.varNameValPairs[i])
                    )
                {
                    // if one of the above is true, display the option
                    filteredOptions[i] = currentOptions[i];
                    optionsCounter++;
                }
            }

            if(optionsCounter == 0)
            {
                DisplayContinue(true);
            }
            else
            {
                DisplayOptions();
            }
        }

        private void DisplayOptions()
        {
            continueButton.gameObject.SetActive(false);
            for (int i = 0; i < dialogOptionButtons.Length; i++)
            {
                if (filteredOptions[i] != null)
                {
                    // display and update option
                    dialogOptionButtons[i].gameObject.SetActive(true);
                    // if dialog option text is not setup or blank, override with a default
                    if(currentDialog.dialogOptionText.Length <= i || string.IsNullOrEmpty(currentDialog.dialogOptionText[i]))
                    {
                        dialogOptionButtons[i].SetText(currentDialog.dialogOptions[i].name);
                    }
                    else
                    {
                        dialogOptionButtons[i].SetText(currentDialog.dialogOptionText[i]);
                    }
                }
                else
                {
                    // hide option
                    dialogOptionButtons[i].gameObject.SetActive(false);
                }
            }
        }

        private void DisplayContinue(bool end)
        {
            for (int i = 0; i < dialogOptionButtons.Length; i++)
            {
                dialogOptionButtons[i].gameObject.SetActive(false);
            }

            continueButton.gameObject.SetActive(true);
            if(end)
            {
                continueButton.SetText("End");
            }
            else
            {
                continueButton.SetText("Continue");
            }
        }
    }
}
