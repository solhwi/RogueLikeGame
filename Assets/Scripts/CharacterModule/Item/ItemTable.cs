using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "ItemTable")]
public partial class ItemTable : ScriptableObject
{
	[Serializable]
	public class PartsItemData
	{
		public string key;
		public CharacterPartsType partsType;
		public CharacterType characterType;
	}

	[System.Serializable]
	public class PartsItemDataDictionary : SerializableDictionary<string, PartsItemData> {}
	public PartsItemDataDictionary partsItemDataDictionary = new PartsItemDataDictionary();

	[Serializable]
	public class PropItemData
	{
		public string key;
		public PropType propType;
	}

	[System.Serializable]
	public class PropItemDataDictionary : SerializableDictionary<string, PropItemData> {}
	public PropItemDataDictionary propItemDataDictionary = new PropItemDataDictionary();

}
