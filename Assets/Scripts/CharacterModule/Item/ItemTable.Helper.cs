
public partial class ItemTable
{
	public int Compare(string itemCode1, string itemCode2)
	{
		int order1 = -1;
		int order2 = -1;

		if (propItemDataDictionary.ContainsKey(itemCode1))
		{
			order1 = (int)CharacterPartsType.Max;
		}
		else if (partsItemDataDictionary.TryGetValue(itemCode1, out var itemData))
		{
			order1 = (int)itemData.partsType;
		}

		if (propItemDataDictionary.ContainsKey(itemCode2))
		{
			order2 = (int)CharacterPartsType.Max;
		}
		else if (partsItemDataDictionary.TryGetValue(itemCode2, out var itemData))
		{
			order2 = (int)itemData.partsType;
		}

		if (order1 == order2)
		{
			return 0;
		}
		else
		{
			return order1 > order2 ? 1 : -1;
		}
	}

	public bool IsValidItem(string itemCode)
	{
		if (itemCode == null || itemCode == string.Empty)
			return false;

		if (partsItemDataDictionary.ContainsKey(itemCode))
			return true;

		if (propItemDataDictionary.ContainsKey(itemCode))
			return true;

		return false;
	}

	public bool IsEquipmentItem(string itemCode)
	{
		bool b = IsPropItem(itemCode);
		b |= IsPartsItem(itemCode);

		return b;
	}

	public bool IsAvatarItem(string itemCode)
	{
		if (partsItemDataDictionary.TryGetValue(itemCode, out var partsItemData))
		{
			if (partsItemData.partsType == CharacterPartsType.Body)
				return true;

			if (partsItemData.partsType == CharacterPartsType.Accessory)
				return true;
		}
		else if (propItemDataDictionary.ContainsKey(itemCode)) 
		{
			return true;
		}

		return false;
	}

	public bool IsEnableEquipOffItem(string itemCode)
	{
		if (IsPropItem(itemCode) || IsAccessoryItem(itemCode))
		{
			return true;
		}

		return false;
	}

	public bool IsAccessoryItem(string itemCode)
	{
		if (partsItemDataDictionary.TryGetValue(itemCode, out var partsItemData))
		{
			if (partsItemData.partsType == CharacterPartsType.Accessory)
				return true;
		}

		return false;
	}

	public bool IsBeautyItem(string itemCode)
	{
		if (partsItemDataDictionary.TryGetValue(itemCode, out var itemData))
		{
			if (itemData.partsType == CharacterPartsType.Hair)
				return true;

			if (itemData.partsType == CharacterPartsType.Eye)
				return true;

			if (itemData.partsType == CharacterPartsType.FrontHair)
				return true;

			if (itemData.partsType == CharacterPartsType.RightEye)
				return true;

			if (itemData.partsType == CharacterPartsType.Face)
				return true;
		}

		return false;
	}

	public bool IsPropItem(string itemCode)
	{
		if (propItemDataDictionary.ContainsKey(itemCode))
			return true;

		return false;
	}

	public bool IsPartsItem(string itemCode)
	{
		if (partsItemDataDictionary.ContainsKey(itemCode))
			return true;

		return false;
	}

	public CharacterPartsType GetItemPartsType(string itemCode)
	{
		if (itemCode == null)
			return CharacterPartsType.Max;

		if (partsItemDataDictionary.TryGetValue(itemCode, out var data) == false)
			return CharacterPartsType.Max;

		return data.partsType;
	}

	public CharacterType GetPartsCharacterType(string itemCode)
	{
		if (itemCode == null)
			return CharacterType.Max;

		if (partsItemDataDictionary.TryGetValue(itemCode, out var data) == false)
			return CharacterType.Max;

		return data.characterType;
	}

	public PropType GetItemPropType(string itemCode)
	{
		if (itemCode == null)
			return PropType.Max;

		if (propItemDataDictionary.TryGetValue(itemCode, out var data) == false)
			return PropType.Max;

		return data.propType;
	}
}