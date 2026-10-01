using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterEquipmentInventory")]
public class CharacterEquipmentInventory : ScriptableObject
{
	[SerializeField] private ItemTable itemTable = null;

	[Header("[장착 중인 아이템]")]
	[Header("[(0) 바디 / (1) 머리 / (2) 눈 / (3) 얼굴 / (4) 악세사리 / (5) 소품]")]
	[Space]
	public string[] equippedItems = new string[6];

	public IEnumerable<PropType> GetEquippedPropType()
	{
		if (equippedItems == null || equippedItems.Any() == false)
			yield break;

		int index = 5;

		if (equippedItems.Length <= index)
			yield break;

		string itemCode = equippedItems[index];

		var propType = itemTable.GetItemPropType(itemCode);
		if (propType == PropType.TwinDagger_L || propType == PropType.TwinDagger_R)
		{
			yield return PropType.TwinDagger_L;
			yield return PropType.TwinDagger_R;
		}
		else
		{
			yield return propType;
		}
	}

	public CharacterType GetCurrentPartsCharacterType(CharacterPartsType partsType)
	{
		int index = -1;

		if (partsType == CharacterPartsType.Accessory)
			index = 4;

		if (partsType == CharacterPartsType.Face)
			index = 3;

		if (partsType == CharacterPartsType.RightEye || partsType == CharacterPartsType.Eye)
			index = 2;

		if (partsType == CharacterPartsType.Hair || partsType == CharacterPartsType.FrontHair)
			index = 1;

		if (partsType == CharacterPartsType.Body)
			index = 0;

		return GetEquippedBodyType(index);
	}

	private CharacterType GetEquippedBodyType(int index)
	{
		if (index < 0 || index >= equippedItems.Length)
			return CharacterType.Max;

		string itemCode = equippedItems[index];
		return itemTable.GetPartsCharacterType(itemCode);
	}
}
