﻿
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace genaralskar.VRC.Dialog
{
    /// <summary>
    /// Starts a dialog on interacting with the object
    /// </summary>
    public class DialogInteractStarter : UdonSharpBehaviour
    {
        public DialogManager dialogManager;
        public string startNode;
        public override void Interact()
        {
            dialogManager.StartDialogByNodeName(startNode);
        }
    }
}