using Project.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Manager
{
    public class SceneManagementManager : Singleton<SceneManagementManager>
    {

        private List<LevelLoadingData> levelsLoading;
        private List<string> currentlyLoadedScenes;

        public override void Awake()
        {
            base.Awake();
            levelsLoading = new List<LevelLoadingData>();
            currentlyLoadedScenes = new List<string>();
        }

        public void Update()
        {
            for(int i = levelsLoading.Count-1; i >=0 ; i--)
            {
                if(levelsLoading[i] == null)
                {
                    levelsLoading.RemoveAt(i);
                    continue;
                }

                if (levelsLoading[i].ao.isDone)
                {
                    levelsLoading[i].ao.allowSceneActivation = true; //Make sur that the scene while fully loaded gets turn on for playe{r
                    levelsLoading[i].onLevelLoaded.Invoke(levelsLoading[i].sceneName);
                    currentlyLoadedScenes.Add(levelsLoading[i].sceneName);
                    levelsLoading.RemoveAt(i);
                    //Hide loading screen
                    //ApplicationManager.Instance.hideLoadingScreen()
                }
            }
        }

        public void LoadLevel(string levelName,Action<string> onLevelLoaded, bool isShowingLoadingScreen=false)
        {
            bool isLevelLoaded = currentlyLoadedScenes.Any(x => x == levelName);
            if (isLevelLoaded)
            {
                Debug.LogFormat("Current level ({0})", levelName);
                return;
            }
            LevelLoadingData lld = new LevelLoadingData(SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive),levelName,onLevelLoaded);
            levelsLoading.Add(lld);


            if (isShowingLoadingScreen)
            {
                //TURN ON LEVEL SCREEN
                //ApplicationManager.Instance.showLoadingScreen()
            }

        }

        public void UnloadLevel(string levelName)
        {
            foreach(string item in currentlyLoadedScenes)
            {
                if(item == levelName)
                {
                    SceneManager.UnloadSceneAsync(levelName);
                    currentlyLoadedScenes.Remove(item);
                    return;
                }
            }

            Debug.LogErrorFormat("Failed to load level {0}, most likely was never loaded or already unload",levelName);
        }
    }

    [Serializable]
    public class LevelLoadingData
    {
        public AsyncOperation ao;
        public string sceneName;
        public Action<string> onLevelLoaded;

        public LevelLoadingData(AsyncOperation _ao,string _name,Action<string> _action)
        {
            ao = _ao;
            sceneName = _name;
            onLevelLoaded = _action;
        }

    }

    public static class SceneList
    {
        public const string MAIN_MENU = "MainMenu";
        public const string LEVEL = "Level";
        public const string ONLINE = "Online";

    }
}

