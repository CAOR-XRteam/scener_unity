// data.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace scener.scene
{
    public static class SceneUtils
    {
        // List of to-keep scene objects
        public static List<Type> filter = new List<Type>
        {
            //---------------------------

            typeof(MeshRenderer),
            typeof(Light),

            //---------------------------
        };

        public static bool HasChildren(GameObject obj)
        {
            //Does current gameobject has exportable childs (congrat !)
            //---------------------------

            foreach (Transform child in obj.transform)
            {
                GameObject child_obj = child.gameObject;
                if (IsOk(child_obj) || HasChildren(child_obj))
                {
                    return true;
                }
            }

            //---------------------------
            return false;
        }

        public static bool IsOk(GameObject obj)
        {
            //Does current gameobject is in to_keep type list
            //---------------------------

            foreach (var type in filter)
            {
                if (obj.GetComponent(type) != null)
                    return true;
            }

            //---------------------------
            return false;
        }
    }
}
