using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Project.Scriptable
{
    [CreateAssetMenu(fileName = "Server Object", menuName = "ScriptableObjects/Server Object", order=3)]
    public class ServerObject : ScriptableObject
    {
        public List<ServerObjectData> objects;

        public ServerObjectData GetObjectByName(string name)
        {
            return objects.SingleOrDefault(x => x.name == name);
        }
    }

    [Serializable]
    public class ServerObjectData
    {
        public string name = "New Object";
        public GameObject prefab;

    }
}

