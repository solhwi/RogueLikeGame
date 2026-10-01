using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.IO;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Linq;
using UnityEditor;

[CreateAssetMenu(fileName = "CharacterResourceSystem")]
public class CharacterResourceSystem : ScriptableObject
{
	[SerializeField] private List<AssetReferenceT<CharacterResourceContainer>> dataContainers = new List<AssetReferenceT<CharacterResourceContainer>>();
	[SerializeField] private List<AssetLabelReference> labelRefs = new List<AssetLabelReference>();

	private Dictionary<string, AsyncOperationHandle> cachedObjectHandleDictionary = new Dictionary<string, AsyncOperationHandle>();

	/// <summary>
	/// 메가바이트 단위로 돌려준다.
	/// </summary>
	private IEnumerator GetDownLoadSize(AssetLabelReference labelRef, System.Action<float> onCompleted)
	{
		var handle = Addressables.GetDownloadSizeAsync(labelRef);

		while (handle.IsDone == false)
		{
			yield return null;
		}

		onCompleted?.Invoke(handle.Result / 1024f / 1024f);
	}

	private IEnumerator DownLoadAssets(AssetLabelReference labelRef, System.Action<float> onProgress, System.Action onCompleted = null)
	{
		var handle = Addressables.DownloadDependenciesAsync(labelRef);

		while (handle.IsDone == false)
		{
			yield return null;
			onProgress?.Invoke(handle.PercentComplete);
		}

		onCompleted?.Invoke();
	}

	public IEnumerator DownLoadAssets(System.Action<float> onProgress = null)
	{
		yield return DownLoadAssets(labelRefs.FirstOrDefault(), onProgress);
	}

	public IEnumerator GetDownLoadSize(System.Action<float> onCompleted = null)
	{
		yield return GetDownLoadSize(labelRefs.FirstOrDefault(), onCompleted);
	}

	public IEnumerator PreLoadAssets()
	{
		foreach (var container in dataContainers)
		{
			CharacterResourceContainer result = null;

			yield return LoadAsync<CharacterResourceContainer>(container, (c) =>
			{
				result = c;
			});

			yield return result.Preload();
		}
	}

	public void ReleaseAllCache()
	{
		foreach (var handle in cachedObjectHandleDictionary.Values)
		{
			Addressables.Release(handle);
		}

		cachedObjectHandleDictionary.Clear();
	}

	/// <summary>
	/// 동기식 로드. 프리로드가 필요한 에셋은 반드시 PreLoadAssets를 통해 미리 로드되어야 한다.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public T LoadData<T>() where T : CharacterResourceContainer
	{
		foreach (var containerRef in dataContainers)
		{
			var c = Load<T>(containerRef);
			if (c == null)
				continue;

			if (c.GetType() != typeof(T))
				continue;

			return c as T;
		}

		return null;
	}


	public T Load<T>(AssetReference reference) where T : UnityEngine.Object
	{
		return Load<T>(reference.AssetGUID);
	}

	public T Load<T>(string path) where T : UnityEngine.Object
	{
#if UNITY_EDITOR
		if (Application.isPlaying == false)
		{
			return AssetDatabase.LoadAssetAtPath<T>(path);
		}
#endif
		if (cachedObjectHandleDictionary.ContainsKey(path) == false)
		{
			Debug.LogError($"{path}에 프리로드가 필요합니다.");
			return null;
		}

		return GetResult<T>(cachedObjectHandleDictionary[path].Result);
	}

	public IEnumerator LoadAsync<T>(AssetReference reference, System.Action<T> onCompleted = null) where T : UnityEngine.Object
	{
		yield return LoadAsync(reference.AssetGUID, onCompleted);
	}

	public IEnumerator LoadAsync<T>(string path, System.Action<T> onCompleted = null) where T : UnityEngine.Object
	{
		if (cachedObjectHandleDictionary.TryGetValue(path, out var handle))
		{
			var r = GetResult<T>(handle.Result);
			onCompleted?.Invoke(r);
			yield break;
		}

		var asyncOperation = LoadAssetAsync<T>(path);
		while (asyncOperation.IsDone == false)
		{
			yield return null;
		}

		cachedObjectHandleDictionary[path] = asyncOperation;

		var result = GetResult<T>(asyncOperation.Result);
		onCompleted?.Invoke(result);
	}

	private AsyncOperationHandle LoadAssetAsync<T>(string path)
	{
		if (typeof(T).IsSubclassOf(typeof(Component)))
		{
			return Addressables.LoadAssetAsync<GameObject>(path);
		}
		else
		{
			return Addressables.LoadAssetAsync<T>(path);
		}
	}

	private T GetResult<T>(object result) where T : UnityEngine.Object
	{
		if (result is T r)
		{
			return r;
		}
		else if (result is GameObject g)
		{
			return g.GetComponent<T>();
		}

		return null;
	}
}
