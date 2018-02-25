using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogPanelUI : MonoBehaviour {

    public static LogPanelUI logPanelUI;

    public GameObject logPanel;

    private bool isOpen;
    private PlayerControl player;

    void Awake()
    {
        logPanelUI = this;
        isOpen = false;
    }

    public void OpenPanel()
    {
        if (isOpen) return;
        isOpen = true;
        logPanel.SetActive(true);
    }

    public void ClosePanel()
    {
        if (!isOpen) return;
        isOpen = false;
        logPanel.SetActive(false);
    }

    public void TogglePanel()
    {
        if (isOpen) ClosePanel();
        else OpenPanel();
    }

    public bool GetIsOpen()
    {
        return isOpen;
    }

    public void SetLocalPlayer(PlayerControl pc)
    {
        if (pc != null) return;
        player = pc;
    }
}
