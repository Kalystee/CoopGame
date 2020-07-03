using Project.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Networking
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class NetworkTransform : MonoBehaviour
    {
        [SerializeField]
        [GreyOut]
        private Vector3 oldPosition;

        private NetworkIdentity networkIdentity;
        private Player player;

        private float stillCounter = 0;

        public void Start()
        {
            networkIdentity = GetComponent<NetworkIdentity>();
            oldPosition = transform.position;
            player = new Player();
            player.position = new Position();
            player.position.x = "";
            player.position.y = "";
            player.position.z = "";

            if (!networkIdentity.IsControlling())
            {
                enabled = false;
            }

        }
      
        public void Update()
        {
            if (networkIdentity.IsControlling())
            {
                if(oldPosition != transform.position)
                {
                    oldPosition = transform.position;
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
            //Update player information
            player.position.x = transform.position.x.TwoDecimals().ToString().Replace(",",".");
            player.position.y = transform.position.y.TwoDecimals().ToString().Replace(",", ".");
            player.position.z = transform.position.z.TwoDecimals().ToString().Replace(",", ".");

            networkIdentity.GetSocket().Emit("updatePosition", new JSONObject(JsonUtility.ToJson(player)));
        }
    }
}

