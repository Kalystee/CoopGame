﻿using Project.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Gameplay
{
    public class CollisionDestroy : MonoBehaviour
    {
        [SerializeField]
        private NetworkIdentity networkIdentity;
        [SerializeField]
        private WhoActivateMe whoActivatedMe;

        public void OnCollisionEnter2D(Collision2D collision)
        {
            NetworkIdentity ni = collision.gameObject.GetComponent<NetworkIdentity>();
            if(ni == null || ni.GetId() != whoActivatedMe.GetActivator())
            {
                networkIdentity.GetSocket().Emit("collisionDestroy", new JSONObject(JsonUtility.ToJson(new IDData()
                {
                    id = networkIdentity.GetId()
                }
                )));
            }
        }
    }
}

