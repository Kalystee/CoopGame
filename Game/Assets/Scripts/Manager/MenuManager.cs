using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Project.Utility;
using UnityEngine.UI;
using SocketIO;
using Project.Networking;

namespace Project.Manager
{
    public class MenuManager : Singleton<MenuManager>
    {
        [SerializeField]
        private Button queueButton;

        private SocketIOComponent socketReference;
        public SocketIOComponent SocketReference {
            get
            {
                return socketReference = (socketReference == null) ? FindObjectOfType<NetworkClient>(): socketReference;
            }
        }

        public void Start()
        {
            queueButton.interactable = false;
            SceneManagementManager.Instance.LoadLevel(SceneList.ONLINE,(levelName)=> {
                queueButton.interactable = true;
            });
        } 

        public void OnQueue()
        {
            SocketReference.Emit("joinGame");
        }

        public void QuitGame()
        {
            Application.Quit();
        }

    }
}

