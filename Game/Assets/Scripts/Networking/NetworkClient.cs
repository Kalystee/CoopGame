using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;
using System;
using Project.Utility;
using Project.Player;
using Project.Scriptable;
using Project.Gameplay;
using System.Globalization;
using Project.Manager;

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
            setupEvents();
        }

        public override void Update()
        {
            base.Update();
        }

        private void Initialize()
        {
            serverObjects = new Dictionary<string, NetworkIdentity>();
        }

        private void setupEvents()
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

                NetworkIdentity ni = serverObjects[id];
                ni.transform.position = new Vector3(x, y, 0);

            });

            On("updateRotation", (e) =>
            {
                string id = e.data["id"].ToString().RemoveQuotes();
                float tankRotation = float.Parse(e.data["tankRotation"].str, CultureInfo.InvariantCulture.NumberFormat);
                float barrelRotation = float.Parse(e.data["barrelRotation"].str, CultureInfo.InvariantCulture.NumberFormat);

                NetworkIdentity ni = serverObjects[id];
                ni.transform.localEulerAngles = new Vector3(0, 0, tankRotation);
                ni.GetComponent<PlayerManager>().SetRotation(barrelRotation);

            });

            On("serverSpawn", (e) =>
            {
                string name = e.data["name"].str;
                string id = e.data["id"].ToString().RemoveQuotes();
               
                float x = float.Parse(e.data["position"]["x"].ToString().RemoveQuotes(), CultureInfo.InvariantCulture.NumberFormat);
                float y = float.Parse(e.data["position"]["y"].ToString().RemoveQuotes(), CultureInfo.InvariantCulture.NumberFormat);
                Debug.LogFormat("Server wants us to spawn a '{0}'", name);

                if (!serverObjects.ContainsKey(id))
                {
                    ServerObjectData sod = serverSpawnables.GetObjectByName(name);
                    GameObject spawnedObject = Instantiate(sod.prefab, networkContainer);
                    spawnedObject.transform.position = new Vector3(x, y, 0);
                    NetworkIdentity ni = spawnedObject.GetComponent<NetworkIdentity>();
                    ni.SetCotrollerID(id);
                    ni.SetSocketReference(this);

                    //if bullet apply direction as well
                    if(name == "Bullet")
                    {
                        float directionX = float.Parse(e.data["direction"]["x"].str, CultureInfo.InvariantCulture.NumberFormat);
                        float directionY = float.Parse(e.data["direction"]["y"].str, CultureInfo.InvariantCulture.NumberFormat);
                        string activator = e.data["activator"].ToString().RemoveQuotes();
                        float speed = float.Parse(e.data["speed"].str, CultureInfo.InvariantCulture.NumberFormat);

                        float rot = Mathf.Atan2(directionY, directionX) * Mathf.Rad2Deg;
                        Vector3 currentRotation = new Vector3(0, 0, rot - 90);
                        spawnedObject.transform.rotation = Quaternion.Euler(currentRotation);

                        WhoActivateMe whoActivateMe = spawnedObject.GetComponent<WhoActivateMe>();
                        whoActivateMe.SetActivator(activator);

                        Projectile projectile = spawnedObject.GetComponent<Projectile>();
                        projectile.Direction = new Vector2(directionX, directionY);
                        projectile.Speed = speed;
                    }

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

            On("playerRespawn", (e) =>
            {
                string id = e.data["id"].ToString().RemoveQuotes();
                float x = e.data["position"]["x"].f;
                float y = e.data["position"]["y"].f;
                NetworkIdentity ni = serverObjects[id];
                ni.transform.position = new Vector3(x, y, 0);
                ni.gameObject.SetActive(true);

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
    }

    [Serializable]
    public class PlayerRotation
    {
        public string tankRotation;
        public string barrelRotation;
    }

    [Serializable]
    public class BulletData
    {
        public string id;
        public string activator;
        public Position position;
        public Position direction;

        public BulletData(string id="", string activator="")
        {
            this.position = new Position();
            this.direction = new Position();
            this.id = id;
            this.activator = activator;
        }
    }

    [Serializable]
    public class IDData
    {
        public string id;
    }
}

