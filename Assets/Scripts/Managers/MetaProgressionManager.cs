using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using RogueLike.Core;

namespace RogueLike.Managers
{
    [Serializable]
    public class SaveData
    {
        public int gold;
        public List<string> unlockedChapterIds = new List<string>();
    }

    // Persists what survives between runs (currency, chapter unlocks) to a
    // JSON file.
    public class MetaProgressionManager : Singleton<MetaProgressionManager>
    {
        private const string SaveFileName = "save.json";

        public int Gold { get; private set; }

        private SaveData data = new SaveData();
        private string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        protected override void Awake()
        {
            base.Awake();
            Load();
        }

        public void AddGold(int amount)
        {
            Gold += amount;
            data.gold = Gold;
            Save();
        }

        public bool SpendGold(int amount)
        {
            if (Gold < amount)
            {
                return false;
            }

            Gold -= amount;
            data.gold = Gold;
            Save();
            return true;
        }

        private void Save()
        {
            File.WriteAllText(SavePath, JsonUtility.ToJson(data));
        }

        private void Load()
        {
            if (!File.Exists(SavePath))
            {
                data = new SaveData();
                Gold = 0;
                return;
            }

            data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
            Gold = data.gold;
        }
    }
}
