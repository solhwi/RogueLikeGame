using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public enum CharacterType
{
	UnityChan = 0,
	Yuco = 1,
	Misaki = 2,
	Max = 3,
}

public enum CharacterPartsType
{
	Body = 0,
	Hair = 1,
	FrontHair = 2,
	Eye = 3, // 왼쪽 눈
	RightEye = 4,
	Face = 5,
	Accessory = 6,
	Max,
}

public enum PropType
{
	GreatSword = 0,
	TwinDagger_L = 1,
	TwinDagger_R = 2,
	Axe = 3,
	PickAx = 4,
	Shovel = 5,
	Umbrella = 6,
	Net = 7,
	Max,
}


[CreateAssetMenu(fileName = "CharacterAssetContainer")]
public class CharacterAssetContainer : CharacterResourceContainer
{
	[SerializeField] private CharacterResourceSystem resourceSystem = null;

	[Header("[0 : UnityChan, 1 : Yuco, 2 : Misaki]")]
	[Space]

	[SerializeField] private AssetReferenceGameObject[] characterBodyPrefabRefs = new AssetReferenceGameObject[(int)CharacterType.Max];

	[SerializeField] private AssetReferenceGameObject[] characterBackHairPrefabRefs = new AssetReferenceGameObject[(int)CharacterType.Max];

	[SerializeField] private AssetReferenceGameObject[] characterFrontHairPrefabRefs = new AssetReferenceGameObject[(int)CharacterType.Max];

	[SerializeField] private AssetReferenceGameObject[] characterHeadAccessoryPrefabRefs = new AssetReferenceGameObject[(int)CharacterType.Max];

	[Header("[0 : 대검 / 1, 2 : 쌍검 /  3 : 도끼 / 4 : 곡괭이 / 5 : 삽 / 6 : 우산 / 7 : 잠자리채]")]
	[Space]
	[SerializeField] private AssetReferenceGameObject[] characterPropPrefabRefs = new AssetReferenceGameObject[(int)CharacterType.Max];

	[SerializeField] private AssetReferenceGameObject characterLeftEyePrefabRef = null;

	[SerializeField] private AssetReferenceGameObject characterRightEyePrefabRef	= null;

	[SerializeField] private AssetReferenceGameObject characterFacePrefabRef = null;

	[SerializeField] private AssetReferenceT<Mesh>[] characterLeftEyeMeshRefs = new AssetReferenceT<Mesh>[(int)CharacterType.Max];

	[SerializeField] private AssetReferenceT<Mesh>[] characterRightEyeMeshRefs = new AssetReferenceT<Mesh>[(int)CharacterType.Max];

	[SerializeField] private AssetReferenceT<Mesh>[] characterFaceMeshRefs = new AssetReferenceT<Mesh>[(int)CharacterType.Max];

	[SerializeField] private AssetReferenceT<Material>[] characterEyeMaterialRefs = new AssetReferenceT<Material>[(int)CharacterType.Max];

	public override IEnumerator Preload()
	{
		foreach (var reference in characterBodyPrefabRefs)
		{
			yield return resourceSystem.LoadAsync<GameObject>(reference);
		}

		foreach (var reference in characterBackHairPrefabRefs)
		{
			yield return resourceSystem.LoadAsync<GameObject>(reference);
		}

		foreach (var reference in characterFrontHairPrefabRefs)
		{
			yield return resourceSystem.LoadAsync<GameObject>(reference);
		}

		foreach (var reference in characterHeadAccessoryPrefabRefs)
		{
			yield return resourceSystem.LoadAsync<GameObject>(reference);
		}

		foreach (var reference in characterPropPrefabRefs)
		{
			yield return resourceSystem.LoadAsync<GameObject>(reference);
		}

		yield return resourceSystem.LoadAsync<GameObject>(characterLeftEyePrefabRef);

		yield return resourceSystem.LoadAsync<GameObject>(characterRightEyePrefabRef);

		yield return resourceSystem.LoadAsync<GameObject>(characterFacePrefabRef);

		foreach (var reference in characterLeftEyeMeshRefs)
		{
			yield return resourceSystem.LoadAsync<Mesh>(reference);
		}

		foreach (var reference in characterRightEyeMeshRefs)
		{
			yield return resourceSystem.LoadAsync<Mesh>(reference);
		}

		foreach (var reference in characterFaceMeshRefs)
		{
			yield return resourceSystem.LoadAsync<Mesh>(reference);
		}

		foreach (var reference in characterEyeMaterialRefs)
		{
			yield return resourceSystem.LoadAsync<Material>(reference);
		}
	}

	public GameObject GetPartsObject(CharacterType characterType, CharacterPartsType meshType)
	{
		string assetGuid = string.Empty;

		switch (meshType)
		{
			case CharacterPartsType.Body:
				assetGuid = characterBodyPrefabRefs[(int)characterType].AssetGUID;
				break;

			case CharacterPartsType.Hair:
				assetGuid = characterBackHairPrefabRefs[(int)characterType].AssetGUID;
				break;

			case CharacterPartsType.FrontHair:
				assetGuid = characterFrontHairPrefabRefs[(int)characterType].AssetGUID;
				break;

			case CharacterPartsType.Eye:
				assetGuid = characterLeftEyePrefabRef.AssetGUID;
				break;

			case CharacterPartsType.RightEye:
				assetGuid = characterRightEyePrefabRef.AssetGUID;
				break;

			case CharacterPartsType.Face:
				assetGuid = characterFacePrefabRef.AssetGUID;
				break;

			case CharacterPartsType.Accessory:
				assetGuid = characterHeadAccessoryPrefabRefs[(int)characterType].AssetGUID;
				break;
		}

		return resourceSystem.Load<GameObject>(assetGuid);
	}

	public GameObject GetPropObject(PropType propType)
	{
		string assetGuid = characterPropPrefabRefs[(int)propType].AssetGUID;
		return resourceSystem.Load<GameObject>(assetGuid);
	}

	public Mesh GetMesh(CharacterType characterType, CharacterPartsType partsType)
	{
		string assetGuid = string.Empty;

		switch (partsType)
		{
			case CharacterPartsType.Eye:
				assetGuid = characterLeftEyeMeshRefs[(int)characterType].AssetGUID; 
				break;

			case CharacterPartsType.RightEye:
				assetGuid = characterRightEyeMeshRefs[(int)characterType].AssetGUID;
				break;

			case CharacterPartsType.Face:
				assetGuid = characterFaceMeshRefs[(int)characterType].AssetGUID;
				break;
		}

		return resourceSystem.Load<Mesh>(assetGuid);
	}

	public Material GetMaterial(CharacterType characterType, CharacterPartsType partsType)
	{
		string assetGuid = string.Empty;

		switch (partsType)
		{
			case CharacterPartsType.Eye:
			case CharacterPartsType.RightEye:
				assetGuid = characterEyeMaterialRefs[(int)characterType].AssetGUID;
				break;
		}

		return resourceSystem.Load<Material>(assetGuid);
	}
}
