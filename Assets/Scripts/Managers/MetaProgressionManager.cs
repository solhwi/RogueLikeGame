using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using RogueLike.Core;
using RogueLike.Items;

namespace RogueLike.Managers
{
    [Serializable]
    public class SaveData
    {
        public int gold;
        public List<string> ownedEquipmentIds = new List<string>();
        public List<int> ownedEquipmentRarities = new List<int>();
        public List<string> unlockedChapterIds = new List<string>();
    }

    // Persists what survives between runs (currency, owned gear, chapter
    // unlocks) to a JSON file.
    // Restoring EquipmentInstance objects from ownedEquipmentIds needs an
    // id -> EquipmentDefinition lookup (an EquipmentDatabase asset) that
    // doesn't exist yet — left as a follow-up.
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

        public void PersistEquipment(IReadOnlyList<EquipmentInstance> equipment)
        {
            data.ownedEquipmentIds.Clear();
            data.ownedEquipmentRarities.Clear();

            foreach (var item in equipment)
            {
                data.ownedEquipmentIds.Add(item.Definition.EquipmentId);
                data.ownedEquipmentRarities.Add((int)item.Rarity);
            }

            Save();
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
