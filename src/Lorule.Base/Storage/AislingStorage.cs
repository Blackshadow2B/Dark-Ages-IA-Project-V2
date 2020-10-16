﻿#region

using System;
using System.IO;
using Newtonsoft.Json;

#endregion

namespace Darkages.Storage
{
    public class AislingStorage : IStorage<Aisling>
    {
        public static string StoragePath = $@"{ServerContext.StoragePath}\aislings";

        static AislingStorage()
        {
            if (!Directory.Exists(StoragePath))
                Directory.CreateDirectory(StoragePath);
        }

        public string[] Files => Directory.GetFiles(StoragePath, "*.json", SearchOption.TopDirectoryOnly);

        public bool Saving { get; set; }

        public Aisling Load(string name)
        {
            var path = Path.Combine(StoragePath, $"{name.ToLower()}.json");

            if (!File.Exists(path))
                return null;

            using var s = File.OpenRead(path);
            using var f = new StreamReader(s);
            return JsonConvert.DeserializeObject<Aisling>(f.ReadToEnd(), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
        }

        public void Save(Aisling obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (ServerContext.Paused)
                return;

            if (ServerContext.Config.DontSavePlayers) return;

            try
            {
                var path = Path.Combine(StoragePath, $"{obj.Username.ToLower()}.json");

                var objString = JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                });

                File.WriteAllText(path, objString);
            }
            catch (Exception)
            {
                /* Ignore */
            }
        }
    }
}