using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;
using System;
using Project.Utility;
using Project.Entities.Player;
using Project.Scriptable;
using Project.Gameplay;
using System.Globalization;
using Project.Manager;
using Project.Entities.Enemy;

namespace Project.Networking
{
    public class NetworkClient : SocketIOComponent
    {

        public const float SERVER_UPDATE_TIME = 10;

        public static Action<SocketIOEvent> OnGameStateChange = (e) =>{};

        [Header("Network Client")]
        [SerializeField]
        private Transform networkContainer;

        [SerializeField]
        private GameObject playerPrefab;
        [SerializeField]
        private ServerObject serverSpawnables;

        private Dictionary<string, NetworkIdentity> serverObjects;

        public static string ClientID { get; private set; }

        public override void Start()
        {
            base.Start();
            Initialize();
            SetupEvents();
        }

        public override void Update()
        {
            base.Update();
        }

        private void Initialize()
        {
            serverObjects = new Dictionary<string, NetworkIdentity>();
        }

        private void SetupEvents()
        {
            On("open", (e) =>
             {
                 Debug.Log("Connected to server");
             });

            On("register", (e) =>
             {
                 ClientID = e.data["id"].ToString().RemoveQuotes();
                 Debug.LogFormat("My client ID ({0})", ClientID);
             });

            On("spawn", (e) =>
             {
                 string id = e.data["id"].ToString().RemoveQuotes();
                 GameObject go = Instantiate(playerPrefab,networkContainer);
                 go.name = string.Format("Player ({0})", id);
                 NetworkIdentity ni = go.GetComponent<NetworkIdentity>();
                 ni.SetCotrollerID(id);
                 ni.SetSocketReference(this);
                 serverObjects.Add(id, ni);

                 //Enabling Camera
                 if (ni.IsControlling())
                 {
                     go.transform.Find("Third Person Camera").gameObject.SetActive(true);
                 }
                 Debug.LogFormat("I have spawned");

             });

            On("disconnected", (e) =>
             {
                 string id = e.data["id"].ToString().RemoveQuotes();

                 GameObject go = serverObjects[id].gameObject;
                 Destroy(go);
                 serverObjects.Remove(id);
                 Debug.LogFormat("Disconnected");
             });

            On("updatePosition", (e) =>
            {
                
                string id = e.data["id"].ToString().RemoveQuotes();
                float x = float.Parse(e.data["position"]["x"].str, CultureInfo.InvariantCulture.NumberFormat);
                float y = float.Parse(e.data["position"]["y"].str, CultureInfo.InvariantCulture.NumberFormat);
                float z = float.Parse(e.data["position"]["z"].str, CultureInfo.InvariantCulture.NumberFormat);

                NetworkIdentity ni = serverObjects[id];
                ni.transform.position = new Vector3(x, y, z);

            });

            On("updateRotation", (e) =>
            {
                string id = e.data["id"].ToString().RemoveQuotes();

                float rotation = float.Parse(e.data["rotation"].str, CultureInfo.InvariantCulture.NumberFormat);

                NetworkIdentity ni = serverObjects[id];
                ni.transform.localEulerAngles = new Vector3(0, rotation,0);

            });

            On("updateAI", (e) => {
                string id = e.data["id"].ToString().RemoveQuotes();

                float x = float.Parse(e.data["position"]["x"].str, CultureInfo.InvariantCulture.NumberFormat);
                float y = float.Parse(e.data["position"]["y"].str, CultureInfo.InvariantCulture.NumberFormat);
                float z = float.Parse(e.data["position"]["z"].str, CultureInfo.InvariantCulture.NumberFormat);

                float rotation = float.Parse(e.data["rotation"].str, CultureInfo.InvariantCulture.NumberFormat);

                NetworkIdentity ni = serverObjects[id];

                if (ni.gameObject.activeInHierarchy)
                {
                    StartCoroutine(AIPositionSmoothing(ni.transform, new Vector3(x, y, z)));
                    ni.GetComponent<EnemyManager>().SetRotation(rotation);
                }
            });

            On("serverSpawn", (e) =>
            {
                string name = e.data["name"].str;
                string id = e.data["id"].ToString().RemoveQuotes();
                float x = float.Parse(e.data["position"]["x"].ToString(), CultureInfo.InvariantCulture.NumberFormat);
                float y = float.Parse(e.data["position"]["y"].ToString(), CultureInfo.InvariantCulture.NumberFormat);
                float z = float.Parse(e.data["position"]["z"].ToString(), CultureInfo.InvariantCulture.NumberFormat);

                Debug.LogFormat("Server wants us to spawn a '{0}'", name);
               
                if (!serverObjects.ContainsKey(id))
                {
                    ServerObjectData sod = serverSpawnables.GetObjectByName(name);
                    GameObject spawnedObject = Instantiate(sod.prefab, networkContainer);
                    spawnedObject.transform.position = new Vector3(x,y,z);
                    NetworkIdentity ni = spawnedObject.GetComponent<NetworkIdentity>();
                    ni.SetCotrollerID(id);
                    ni.SetSocketReference(this);
                    Debug.Log(spawnedObject.transform.position);



                    //if bullet apply direction as well
                    //       if(name == "Bullet")
                    //        {
                    //            float directionX = float.Parse(e.data["direction"]["x"].str, CultureInfo.InvariantCulture.NumberFormat);
                    //            float directionY = float.Parse(e.data["direction"]["y"].str, CultureInfo.InvariantCulture.NumberFormat);
                    //            string activator = e.data["activator"].ToString().RemoveQuotes();
                    //            float speed = float.Parse(e.data["speed"].str, CultureInfo.InvariantCulture.NumberFormat);

                    //            float rot = Mathf.Atan2(directionY, directionX) * Mathf.Rad2Deg;
                    //            Vector3 currentRotation = new Vector3(0, 0, rot - 90);
                    //            spawnedObject.transform.rotation = Quaternion.Euler(currentRotation);

                    //            WhoActivateMe whoActivateMe = spawnedObject.GetComponent<WhoActivateMe>();
                    //            whoActivateMe.SetActivator(activator);

                    //            Projectile projectile = spawnedObject.GetComponent<Projectile>();
                    //            projectile.Direction = new Vector2(directionX, directionY);
                    //            projectile.Speed = speed;
                    //        }

                    serverObjects.Add(id, ni);

                }

            });

            On("serverUnspawn", (e) =>
            {
                string id = e.data["id"].ToString().RemoveQuotes();
                NetworkIdentity ni = serverObjects[id];
                serverObjects.Remove(id);
                DestroyImmediate(ni.gameObject);
            });

            On("playerDied", (e) =>
            {
                string id = e.data["id"].ToString().RemoveQuotes();
                NetworkIdentity ni = serverObjects[id];
                ni.gameObject.SetActive(false);
            });

           

            On("loadGame", (e) =>
            {
                Debug.Log("Switching to game");
                SceneManagementManager.Instance.LoadLevel(SceneList.LEVEL,(levelName) =>
                {
                    SceneManagementManager.Instance.UnloadLevel(SceneList.MAIN_MENU);
                });
            });

            On("lobbyUpdate", (e) =>
            {
                OnGameStateChange.Invoke(e);   //I prefer use Invoke to make difference between Action and function
            });
        }

        public void AttempToJoinLobby()
        {
            Emit("joinGame");
        }

        private IEnumerator AIPositionSmoothing(Transform aiTransform, Vector3 goalPosition)
        {
            float count = 0.1f; //In sync with server update
            float currentTime = 0.0f;
            Vector3 startPosition = aiTransform.position;

            while (currentTime < count)
            {
                currentTime += Time.deltaTime;

                if (currentTime < count)
                {
                    aiTransform.position = Vector3.Lerp(startPosition, goalPosition, currentTime / count);
                }

                yield return new WaitForEndOfFrame();

                if (aiTransform == null)
                {
                    currentTime = count;
                    yield return null;
                }
            }

            yield return null;
        }

    }

    [Serializable]
    public class Player
    {
        public string id;
        public string username;
        public Position position;
    }

    [Serializable]
    public class Position
    {
        public string x;
        public string y;
        public string z;
    }

    [Serializable]
    public class PlayerRotation
    {
        public string rotation;

    }

    [Serializable]
    public class CombatData
    {
        public string initiatorId;
        public string targetId;
        public string ammount;
    }

    [Serializable]
    public class IDData
    {
        public string id;
    }
}

