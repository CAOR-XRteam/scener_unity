// data.cs
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace scener.scene
{
    public static class Json
    {
        // List of to-keep scene objects
        public static void write_on_file(object data, string path)
        {
            //---------------------------

            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(path, json);

            //---------------------------
        }
    }
}
