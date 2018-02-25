using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatPanelUI : MonoBehaviour {

    public static StatPanelUI statPanelUI;
    private static BattleManager bm;

    public GameObject statPanel;

    public Text cText;
    public Text cAtckText;
    public Text cAthrText;
    public Text cMntlText;
    public Text cExpText;
    public Image uAtckButton;
    public Image uAthrButton;
    public Image uMntlButton;
    public Text uAtckText;
    public Text uAthrText;
    public Text uMntlText;
    public Image redoButton;
    public Image redoBorder;
    public Text redoText;
    public Image confirmButton;
    public Image confirmBorder;
    public Text confirmText;

    public Text dAtckText;
    public Text dAthrText;
    public Text dMntlText;
    public Text dExpText;

    [HideInInspector] public int currentAttack;     // 확정 후 공격력 (PlayerControl에서 직접 받아옴)
    [HideInInspector] public int currentAuthority;  // 확정 후 권력 (PlayerControl에서 직접 받아옴)
    [HideInInspector] public int currentMentality;  // 확정 후 정신력 (PlayerControl에서 직접 받아옴)
    [HideInInspector] public int currentExperience; // 확정 후 경험치 (PlayerControl에서 직접 받아옴)

    private bool isOpen;
    private bool isContribTime;     // 능력치 분배 시간인가? (bm의 turnStep 기준)
    private bool isConfirmed;       // 바뀐 능력치를 확정하였는가? (isContribTime이 false이면 isConfirmed도 false)
    private bool canRedo;           // 되돌릴 변경사항이 있는가? (isContribTime이 false이면 이것도 false, isConfirmed가 true이면 이것은 false)
    private bool canUpAttack;
    private bool canUpAuthority;
    private bool canUpMentality;
    private PlayerControl player;

    void Awake()
    {
        statPanelUI = this;
        isOpen = false;
        isConfirmed = false;
        isContribTime = false;
        canRedo = false;
        canUpAttack = false;
        canUpAuthority = false;
        canUpMentality = false;
    }
	
	void FixedUpdate () {
        if (bm == null)
        {
            bm = BattleManager.bm;
            return;
        }

        if (isOpen && player != null)
        {
            cAtckText.text = currentAttack.ToString();
            cAthrText.text = currentAuthority.ToString();
            cMntlText.text = currentMentality.ToString();
            cExpText.text = currentExperience.ToString();
            dAtckText.text = player.GetStatAttack().ToString();
            dAthrText.text = player.GetStatAuthority().ToString();
            dMntlText.text = player.GetStatMentality().ToString();
            dExpText.text = player.GetExperience().ToString();
            uMntlText.text = "-" + (currentExperience + 1).ToString() + "\nExp";

            
        }

        // 능력치 분배 시간이 아니게 된 경우
        if (bm.GetTurnStep() != 13 && isContribTime)
        {
            isContribTime = false;
            isConfirmed = false;
            canRedo = false;
            canUpAttack = false;
            canUpAuthority = false;
            canUpMentality = false;
            cText.color = SetAlphaTo96(cText.color);
            cAtckText.color = SetAlphaTo96(cAtckText.color);
            cAthrText.color = SetAlphaTo96(cAthrText.color);
            cMntlText.color = SetAlphaTo96(cMntlText.color);
            cExpText.color = SetAlphaTo96(cExpText.color);
            uAtckButton.color = SetAlphaTo96(uAtckButton.color);
            uAthrButton.color = SetAlphaTo96(uAthrButton.color);
            uMntlButton.color = SetAlphaTo96(uMntlButton.color);
            uAtckText.color = SetAlphaTo96(uAtckText.color);
            uAthrText.color = SetAlphaTo96(uAthrText.color);
            uMntlText.color = SetAlphaTo96(uMntlText.color);
            redoButton.color = SetAlphaTo96(redoButton.color);
            redoBorder.color = SetAlphaTo96(redoBorder.color);
            redoText.color = SetAlphaTo96(redoText.color);
            confirmButton.color = SetAlphaTo96(confirmButton.color);
            confirmBorder.color = SetAlphaTo96(confirmBorder.color);
            confirmText.color = SetAlphaTo96(confirmText.color);
            uAtckButton.GetComponent<Button>().interactable = false;
            uAthrButton.GetComponent<Button>().interactable = false;
            uMntlButton.GetComponent<Button>().interactable = false;
            redoButton.GetComponent<Button>().interactable = false;
            confirmButton.GetComponent<Button>().interactable = false;
        }

        // 능력치 분배 시간이 된 경우
        if (bm.GetTurnStep() == 13 && !isContribTime)
        {
            isContribTime = true;
            isConfirmed = false;
            canRedo = false;
            cText.color = SetAlphaTo255(cText.color);
            cAtckText.color = SetAlphaTo255(cAtckText.color);
            cAthrText.color = SetAlphaTo255(cAthrText.color);
            cMntlText.color = SetAlphaTo255(cMntlText.color);
            cExpText.color = SetAlphaTo255(cExpText.color);
            redoButton.color = SetAlphaTo255(redoButton.color);
            redoBorder.color = SetAlphaTo255(redoBorder.color);
            redoText.color = SetAlphaTo255(redoText.color);
            confirmButton.color = SetAlphaTo255(confirmButton.color);
            confirmBorder.color = SetAlphaTo255(confirmBorder.color);
            confirmText.color = SetAlphaTo255(confirmText.color);
            redoButton.GetComponent<Button>().interactable = false;
            confirmButton.GetComponent<Button>().interactable = true;
        }

        if (isContribTime && !isConfirmed) { 
            if (canUpAttack && (currentAttack >= 99 || currentExperience < 5))
            {
                canUpAttack = false;
                uAtckButton.color = SetAlphaTo96(uAtckButton.color);
                uAtckText.color = SetAlphaTo96(uAtckText.color);
                uAtckButton.GetComponent<Button>().interactable = false;
            }
            if (!canUpAttack && currentAttack < 99 && currentExperience >= 5)
            {
                canUpAttack = true;
                uAtckButton.color = SetAlphaTo255(uAtckButton.color);
                uAtckText.color = SetAlphaTo255(uAtckText.color);
                uAtckButton.GetComponent<Button>().interactable = true;
            }

            if (canUpAuthority && (currentAuthority >= 99 || currentExperience < 5))
            {
                canUpAuthority = false;
                uAthrButton.color = SetAlphaTo96(uAthrButton.color);
                uAthrText.color = SetAlphaTo96(uAthrText.color);
                uAthrButton.GetComponent<Button>().interactable = false;
            }
            if (!canUpAuthority && currentAuthority < 99 && currentExperience >= 5)
            {
                canUpAuthority = true;
                uAthrButton.color = SetAlphaTo255(uAthrButton.color);
                uAthrText.color = SetAlphaTo255(uAthrText.color);
                uAthrButton.GetComponent<Button>().interactable = true;
            }

            if (canUpMentality && (currentMentality >= 99 || currentExperience < currentMentality + 1))
            {
                canUpMentality = false;
                uMntlButton.color = SetAlphaTo96(uMntlButton.color);
                uMntlText.color = SetAlphaTo96(uMntlText.color);
                uMntlButton.GetComponent<Button>().interactable = false;
            }
            if (!canUpMentality && currentMentality < 99 && currentExperience >= currentMentality + 1)
            {
                canUpMentality = true;
                uMntlButton.color = SetAlphaTo255(uMntlButton.color);
                uMntlText.color = SetAlphaTo255(uMntlText.color);
                uMntlButton.GetComponent<Button>().interactable = true;
            }
        }

    }

    public void OpenPanel()
    {
        if (isOpen) return;
        isOpen = true;
        statPanel.SetActive(true);
    }

    public void ClosePanel()
    {
        if (!isOpen) return;
        isOpen = false;
        statPanel.SetActive(false);
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
        if (pc == null) return;
        player = pc;
        Debug.Log("SetLocalPlayer " + player.GetName());
    }

    public void Confirm()
    {
        if (player == null || !isContribTime || isConfirmed) return;
        player.StatConfirm();
        isConfirmed = true;
        canRedo = false;
        canUpAttack = false;
        canUpAuthority = false;
        canUpMentality = false;

        uAtckButton.color = SetAlphaTo96(uAtckButton.color);
        uAthrButton.color = SetAlphaTo96(uAthrButton.color);
        uMntlButton.color = SetAlphaTo96(uMntlButton.color);
        uAtckText.color = SetAlphaTo96(uAtckText.color);
        uAthrText.color = SetAlphaTo96(uAthrText.color);
        uMntlText.color = SetAlphaTo96(uMntlText.color);
        redoButton.color = SetAlphaTo96(redoButton.color);
        redoBorder.color = SetAlphaTo96(redoBorder.color);
        redoText.color = SetAlphaTo96(redoText.color);
        confirmButton.color = SetAlphaTo96(confirmButton.color);
        confirmBorder.color = SetAlphaTo96(confirmBorder.color);
        confirmText.color = SetAlphaTo96(confirmText.color);
        uAtckButton.GetComponent<Button>().interactable = false;
        uAthrButton.GetComponent<Button>().interactable = false;
        uMntlButton.GetComponent<Button>().interactable = false;
        redoButton.GetComponent<Button>().interactable = false;
        confirmButton.GetComponent<Button>().interactable = false;
    }

    public void Redo()
    {
        if (player == null || !isContribTime || isConfirmed || !canRedo) return;
        canRedo = false;
        player.StatRedo();
        redoButton.color = SetAlphaTo96(redoButton.color);
        redoBorder.color = SetAlphaTo96(redoBorder.color);
        redoText.color = SetAlphaTo96(redoText.color);
        redoButton.GetComponent<Button>().interactable = false;
    }

    public void UpAttack()
    {
        if (player == null || !isContribTime || isConfirmed || !canUpAttack) return;
        player.StatAttackUp();
        if (!canRedo)
        {
            canRedo = true;
            redoButton.color = SetAlphaTo255(redoButton.color);
            redoBorder.color = SetAlphaTo255(redoBorder.color);
            redoText.color = SetAlphaTo255(redoText.color);
            redoButton.GetComponent<Button>().interactable = true;
        }
    }

    public void UpAuthority()
    {
        if (player == null || !isContribTime || isConfirmed || !canUpAuthority) return;
        player.StatAuthorityUp();
        if (!canRedo)
        {
            canRedo = true;
            redoButton.color = SetAlphaTo255(redoButton.color);
            redoBorder.color = SetAlphaTo255(redoBorder.color);
            redoText.color = SetAlphaTo255(redoText.color);
            redoButton.GetComponent<Button>().interactable = true;
        }
    }

    public void UpMentality()
    {
        if (player == null || !isContribTime || isConfirmed || !canUpMentality) return;
        player.StatMentalityUp();
        if (!canRedo)
        {
            canRedo = true;
            redoButton.color = SetAlphaTo255(redoButton.color);
            redoBorder.color = SetAlphaTo255(redoBorder.color);
            redoText.color = SetAlphaTo255(redoText.color);
            redoButton.GetComponent<Button>().interactable = true;
        }
    }

    private Color SetAlphaTo96(Color c)
    {
        return new Color(c.r, c.g, c.b, 0.376f);
    }

    private Color SetAlphaTo255(Color c)
    {
        return new Color(c.r, c.g, c.b, 1f);
    }
}
