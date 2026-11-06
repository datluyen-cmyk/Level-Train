using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UniLog;
using UnityEngine;
using XingYun.Aot;
using XingYun.Core.Singletons;
using XingYun.Res;

namespace RotateRings.Services;

public sealed class YooAssetsResourceProvider : IAudioForgeReleasableResourceProvider, IAudioForgeResourceProvider, IAudioForgeFullPathResourceProvider
{
	private const string DefaultOutputRoot = "Assets/";

	private const string DefaultSeedAudioDataPath = "GameResBuiltin/AudioForge/AudioData.json";

	private const string DefaultSeedAssetsRootPath = "GameResBuiltin/AudioForge/Assets";

	private const string DefaultActivationManifestPath = "GameResBuiltin/AudioForge/Activation/runtime_activation_manifest.json";

	private const string LegacyAudioRoot = "Assets/GameRes/Audio/";

	private readonly string outputRootLocation;

	private readonly string audioDataLocation;

	private readonly string seedAssetsRootLocation;

	private readonly string activationManifestLocation;

	private readonly string resGroup;

	private readonly Dictionary<string, string> retainedClipLocations = new Dictionary<string, string>(8);

	private const string AudioPersistGroupPrefix = "AudioForgePersist_";

	public string OutputRootLocation => outputRootLocation;

	public string AudioDataLocation => audioDataLocation;

	public string SeedAssetsRootLocation => seedAssetsRootLocation;

	public string ActivationManifestLocation => activationManifestLocation;

	public YooAssetsResourceProvider()
		: this("Assets/", "GameResBuiltin/AudioForge/AudioData.json", "GameResBuiltin/AudioForge/Assets", "GameResBuiltin/AudioForge/Activation/runtime_activation_manifest.json", Define.ResGroup)
	{
	}

	public YooAssetsResourceProvider(string audioDataLocation)
		: this(audioDataLocation, "Assets/GameRes/Audio/", Define.ResGroup)
	{
	}

	public YooAssetsResourceProvider(string audioDataLocation, string assetKeyPrefix, string resGroup)
	{
		outputRootLocation = NormalizeRootLocation(assetKeyPrefix);
		this.audioDataLocation = NormalizeRelativeLocation(audioDataLocation, "audioDataLocation");
		seedAssetsRootLocation = EnsureTrailingSlash(ResolveOutputLocation("Assets", "seedAssetsRootPath"));
		activationManifestLocation = ResolveOutputLocation("Activation/runtime_activation_manifest.json", "activationManifestPath");
		this.resGroup = (string.IsNullOrWhiteSpace(resGroup) ? Define.ResGroup : resGroup);
	}

	public YooAssetsResourceProvider(string outputRootLocation, string seedAudioDataPath, string seedAssetsRootPath, string activationManifestPath)
		: this(outputRootLocation, seedAudioDataPath, seedAssetsRootPath, activationManifestPath, Define.ResGroup)
	{
	}

	public YooAssetsResourceProvider(string outputRootLocation, string seedAudioDataPath, string seedAssetsRootPath, string activationManifestPath, string resGroup)
	{
		this.outputRootLocation = NormalizeRootLocation(outputRootLocation);
		audioDataLocation = ResolveOutputLocation(seedAudioDataPath, "seedAudioDataPath");
		seedAssetsRootLocation = EnsureTrailingSlash(ResolveOutputLocation(seedAssetsRootPath, "seedAssetsRootPath"));
		activationManifestLocation = ResolveOutputLocation(activationManifestPath, "activationManifestPath");
		this.resGroup = (string.IsNullOrWhiteSpace(resGroup) ? Define.ResGroup : resGroup);
	}

	public IEnumerator LoadAudioDataJson(Action<string> onLoaded)
	{
		TextAsset val = MonoSingleton<ResManager>.Instance.LoadAsset<TextAsset>(audioDataLocation, resGroup);
		if ((Object)(object)val == (Object)null)
		{
			Log.Error((object)("AudioForge AudioData TextAsset not found: " + audioDataLocation), Array.Empty<object>());
			onLoaded(null);
		}
		else
		{
			onLoaded(val.text);
		}
		yield break;
	}

	public string LoadAudioDataJsonSync()
	{
		TextAsset val = MonoSingleton<ResManager>.Instance.LoadAsset<TextAsset>(audioDataLocation, resGroup);
		if ((Object)(object)val == (Object)null)
		{
			Log.Error((object)("AudioForge AudioData TextAsset not found: " + audioDataLocation), Array.Empty<object>());
			return null;
		}
		return val.text;
	}

	public string LoadActivationManifestJson()
	{
		TextAsset val = MonoSingleton<ResManager>.Instance.LoadAsset<TextAsset>(activationManifestLocation, resGroup);
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		return val.text;
	}

	public string LoadModuleJson(string moduleJsonPath)
	{
		if (string.IsNullOrWhiteSpace(moduleJsonPath))
		{
			return null;
		}
		string text = ResolveYooAssetLocation(moduleJsonPath);
		TextAsset val = MonoSingleton<ResManager>.Instance.LoadAsset<TextAsset>(text, resGroup);
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		return val.text;
	}

	public IEnumerator LoadClip(string assetKey, string runtimeAudioFormat, Action<AudioClip> onLoaded)
	{
		string text = ResolveYooAssetLocation(assetKey);
		AudioClip val = MonoSingleton<ResManager>.Instance.LoadAsset<AudioClip>(text, resGroup);
		if ((Object)(object)val == (Object)null)
		{
			Log.LogError("AudioForge", "LoadClip failed, AudioClip not found: " + text);
		}
		onLoaded(val);
		yield break;
	}

	public AudioClip LoadClipSync(string assetKey, string runtimeAudioFormat)
	{
		string text = ResolveYooAssetLocation(assetKey);
		AudioClip obj = MonoSingleton<ResManager>.Instance.LoadAsset<AudioClip>(text, resGroup);
		if ((Object)(object)obj == (Object)null)
		{
			Log.LogError("AudioForge", "LoadClipSync failed, AudioClip not found: " + text);
		}
		return obj;
	}

	public void RetainClip(string assetKey)
	{
		if (!string.IsNullOrEmpty(assetKey) && !retainedClipLocations.ContainsKey(assetKey))
		{
			string text = ResolveYooAssetLocation(assetKey);
			if ((Object)(object)MonoSingleton<ResManager>.Instance.LoadAsset<AudioClip>(text, "AudioForgePersist_" + assetKey) == (Object)null)
			{
				Log.LogError("AudioForge", "RetainClip failed, AudioClip not found: " + text);
			}
			else
			{
				retainedClipLocations[assetKey] = text;
			}
		}
	}

	public void ReleaseClip(string assetKey)
	{
		if (!string.IsNullOrEmpty(assetKey) && retainedClipLocations.ContainsKey(assetKey))
		{
			retainedClipLocations.Remove(assetKey);
			MonoSingleton<ResManager>.Instance.ReleaseGroup("AudioForgePersist_" + assetKey);
		}
	}

	private static string ResolveYooAssetLocation(string pathOrName)
	{
		if (string.IsNullOrWhiteSpace(pathOrName))
		{
			return pathOrName;
		}
		if (pathOrName.IndexOf('/') < 0 && pathOrName.IndexOf('\\') < 0)
		{
			return pathOrName;
		}
		return Path.GetFileNameWithoutExtension(pathOrName.Replace('\\', '/'));
	}

	private string ResolveOutputLocation(string relativeLocation, string parameterName)
	{
		string text = NormalizeRelativeLocation(relativeLocation, parameterName);
		if (text.StartsWith(outputRootLocation, StringComparison.OrdinalIgnoreCase))
		{
			return text;
		}
		return outputRootLocation + text;
	}

	private static string NormalizeRootLocation(string value)
	{
		return EnsureTrailingSlash(NormalizeRelativeLocation(value, "outputRootLocation"));
	}

	private static string NormalizeRelativeLocation(string value, string parameterName)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			throw new ArgumentException("AudioForge location is required.", parameterName);
		}
		string text = value.Trim().Replace('\\', '/');
		if (text.StartsWith("/", StringComparison.Ordinal) || text.Contains("://") || (text.Length >= 2 && char.IsLetter(text[0]) && text[1] == ':'))
		{
			throw new ArgumentException("AudioForge location must be a relative YooAssets location: " + value, parameterName);
		}
		List<string> list = new List<string>();
		string[] array = text.Split('/');
		foreach (string text2 in array)
		{
			if (!string.IsNullOrEmpty(text2) && !(text2 == "."))
			{
				if (text2 == "..")
				{
					throw new ArgumentException("AudioForge location cannot escape OutputRoot: " + value, parameterName);
				}
				list.Add(text2);
			}
		}
		if (list.Count == 0)
		{
			throw new ArgumentException("AudioForge location is required.", parameterName);
		}
		return string.Join("/", list);
	}

	private static string EnsureTrailingSlash(string value)
	{
		if (!value.EndsWith("/", StringComparison.Ordinal))
		{
			return value + "/";
		}
		return value;
	}
}
