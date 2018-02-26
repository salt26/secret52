using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogPanelUI : MonoBehaviour {

    public static LogPanelUI logPanelUI;
    private static BattleManager bm;

    public GameObject logPanel;

    private bool isOpen;
    private PlayerControl player;

    void Awake()
    {
        logPanelUI = this;
        isOpen = false;
    }

    void FixedUpdate()
    {
        if (bm == null)
        {
            bm = BattleManager.bm;
            return;
        }
    }

    public void OpenPanel()
    {
        if (isOpen || bm == null || bm.GetTurnStep() <= 0) return;
        isOpen = true;
        logPanel.SetActive(true);
        if (StatPanelUI.statPanelUI != null && StatPanelUI.statPanelUI.GetIsOpen())
        {
            StatPanelUI.statPanelUI.ClosePanel();
        }
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
