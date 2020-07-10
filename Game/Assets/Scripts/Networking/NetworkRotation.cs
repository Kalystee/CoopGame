using Project.Player;
using Project.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Networking
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class NetworkRotation : MonoBehaviour
    {
        [Header("Referenced Values")]
        [SerializeField]
        [GreyOut]
        private float oldRotation;


        [Header("Class References")]
        [SerializeField]
        private PlayerManager playerManager;

        private NetworkIdentity networkIdentity;
        private PlayerRotation player;

        private float stillCounter = 0;

        public void Start()
        {
            networkIdentity = GetComponent<NetworkIdentity>();

            player = new PlayerRotation();
            player.rotation = "";
            if (!networkIdentity.IsControlling())
            {
                enabled = false;
            }
        }

        public void Update()
        {
            if (networkIdentity.IsControlling())
            {
                if(oldRotation != transform.localEulerAngles.y)
                {
                    oldRotation = transform.localEulerAngles.y;
                    stillCounter = 0;
                    SendData();
                }
                else
                {
                    stillCounter += Time.deltaTime;
                    if(stillCounter >= 1)
                    {
                        stillCounter = 0;
                        SendData();
                    }
                }
            }
        }

        private void SendData()
        {
            player.rotation = transform.localEulerAngles.y.TwoDecimals().ToString().Replace(",", ".");
            networkIdentity.GetSocket().Emit("updateRotation", new JSONObject(JsonUtility.ToJson(player)));
        }
    }
}

