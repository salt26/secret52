using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Prototype.NetworkLobby
{
    public class LobbyHelpPanel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public LobbyManager lobbyManager;

        public RectTransform prevHelpPanel;
        public RectTransform nextHelpPanel;

        private float x;

        public void OnClickPrev()
        {
            lobbyManager.ChangeTo(prevHelpPanel);
            lobbyManager.backDelegate = lobbyManager.SimpleBackClbk;
        }

        public void OnClickNext()
        {
            lobbyManager.ChangeTo(nextHelpPanel);
            lobbyManager.backDelegate = lobbyManager.SimpleBackClbk;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            x = eventData.position.x;
        }

        public void OnDrag(PointerEventData eventData)
        {
            
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.position.x <= x - (Screen.width / 5))
            {
                OnClickNext();
            }
            else if (eventData.position.x >= x + (Screen.width / 5))
            {
                OnClickPrev();
            }
        }
    }
}
