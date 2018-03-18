using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Prototype.NetworkLobby
{
    public class LobbyTopPanel : MonoBehaviour
    {
        public bool isInGame = false;
        public Text buttonText;

        protected bool isDisplayed = true;
        protected Image panelImage;

        void Start()
        {
            panelImage = GetComponent<Image>();
        }


        void Update()
        {
            if (!isInGame)
            {
                buttonText.text = "BACK";
                if (Input.GetKeyDown(KeyCode.Escape) && LobbyManager.s_Singleton.backButton.gameObject.activeInHierarchy)
                {
                    LobbyManager.s_Singleton.GoBackButton();
                }
                return;
            }

            if (isDisplayed)
            {
                buttonText.text = "QUIT";
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleVisibility(!isDisplayed);
            }
            /*
            if (isInGame && isDisplayed && Input.GetMouseButton(0) && Input.touchCount <= 1
                && Input.mousePosition.y < Screen.height - 100f)
            {
                ToggleVisibility(false);
            }
            */
        }

        public void ToggleVisibility(bool visible)
        {
            isDisplayed = visible;
            foreach (Transform t in transform)
            {
                t.gameObject.SetActive(isDisplayed);
            }

            if (panelImage != null)
            {
                panelImage.enabled = isDisplayed;
            }
        }
    }
}