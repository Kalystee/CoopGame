using Project.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Manager
{
    public class ApplicationManager : Singleton<ApplicationManager>
    {
        // Start is called before the first frame update
        void Start()
        {
            Debug.Log("MainMenu");
            SceneManagementManager.Instance.LoadLevel(SceneList.MAIN_MENU,(levelName) => { });
        }
    }
}

