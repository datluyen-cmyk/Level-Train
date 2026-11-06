using System;
using System.Collections.Generic;
using Example.Cfg;
using RotateRings.Common;
using RotateRings.UI.Setting;
using UniLog;
using UnityEngine;

namespace RotateRings.Services;

public static class AudioService
{
	private const string Tag = "Audio";

	private const string BusSfx = "SFX";

	private const string BusUi = "UI";

	private const string BusBgm = "BGM";

	private const string BusVO = "VO";

	private const string AMB = "AMB";

	private const ulong PostedSoundHandleMask = 9223372036854775808uL;

	private const string AudioForgeOutputRoot = "Assets/";

	private const string AudioForgeSeedAudioDataPath = "02_Res/Art/Audio/AudioData.json";

	private const string AudioForgeSeedAssetsRootPath = "02_Res/Art/Audio/Assets";

	private const string AudioForgeActivationManifestPath = "02_Res/Art/Audio/Activation/runtime_activation_manifest.json";

	private static int bgmPauseForAdCount;

	private static int soundPauseForAdCount;

	private static List<string> batchedActivateVariantIds;

	private static List<string> batchedDeactivateVariantIds;

	public static bool IsBgmSuppressedForAd => bgmPauseForAdCount > 0;

	public static void Init()
	{
		InitAudioForge();
		InitAudioGroup();
		EnsureAudioListener();
		GameApp.Command.Execute<AudioExptInitCommand>((object)null);
	}

	public static void EnsureAudioListener()
	{
		AudioForgeRuntime instance = AudioForgeRuntime.Instance;
		if (!((Object)(object)instance == (Object)null) && (Object)(object)((Component)instance).gameObject.GetComponent<AudioListener>() == (Object)null)
		{
			((Component)instance).gameObject.AddComponent<AudioListener>();
		}
	}

	private static float LinearToDb(float volume)
	{
		if (volume <= 0f)
		{
			return -80f;
		}
		return 20f * Mathf.Log10(Mathf.Clamp01(volume));
	}

	public static void ApplyConfigVolumeOffsets()
	{
		AudioForgeRuntime instance = AudioForgeRuntime.Instance;
		if ((Object)(object)instance == (Object)null)
		{
			return;
		}
		foreach (KeyValuePair<string, AudioConfig> item in GameApp.Config.GetConfig<ExampleTables, TbAudioConfig>((Func<ExampleTables, TbAudioConfig>)((ExampleTables tb) => tb.TbAudioConfig)).DataMap)
		{
			float num = LinearToDb(item.Value.Volume);
			instance.SetUnityEventVolumeOffsetDb(item.Key, num);
		}
	}

	public static ulong PlayUiSound(string audioName)
	{
		return PlayUiSound(audioName, 1f);
	}

	public static ulong PlayUiSound(string audioName, float volume)
	{
		return PlayUiSound(audioName, volume, null);
	}

	public static ulong PlayUiSound(string audioName, float volume, string resGroup)
	{
		return PostUiSoundInternal(audioName, volume);
	}

	public static ulong PlayUiSoundSync(string audioName)
	{
		return PlayUiSoundSync(audioName, 1f);
	}

	public static ulong PlayUiSoundSync(string audioName, float volume)
	{
		return PlayUiSoundSync(audioName, volume, null);
	}

	public static ulong PlayUiSoundSync(string audioName, float volume, string resGroup)
	{
		return PostUiSoundInternal(audioName, volume);
	}

	public static ulong PlaySound(string audioName)
	{
		return PlaySound(audioName, 1f);
	}

	public static ulong PlaySound(string audioName, float volume)
	{
		return PlaySound(audioName, volume, null);
	}

	public static ulong PlaySound(string audioName, float volume, string resGroup)
	{
		return PlaySoundInternal(audioName, volume, useSync: false);
	}

	public static ulong PlaySoundSync(string audioName)
	{
		return PlaySoundSync(audioName, 1f);
	}

	public static ulong PlaySoundSync(string audioName, float volume)
	{
		return PlaySoundSync(audioName, volume, null);
	}

	public static ulong PlaySoundSync(string audioName, float volume, string resGroup)
	{
		return PlaySoundInternal(audioName, volume, useSync: true);
	}

	private static ulong PlaySoundInternal(string audioName, float volume, bool useSync)
	{
		return TryPlayEvent(audioName, volume, useSync, "PlaySound");
	}

	private static ulong TryPlayEvent(string audioName, float volume, bool useSync, string logPrefix)
	{
		AudioForgeRuntime instance = AudioForgeRuntime.Instance;
		if (string.IsNullOrEmpty(audioName))
		{
			Log.LogError("Audio", logPrefix + " 跳过: audioName 为空");
			return 0uL;
		}
		if ((Object)(object)instance == (Object)null || !instance.IsReady)
		{
			return 0uL;
		}
		float num = (Mathf.Approximately(volume, 1f) ? 0f : LinearToDb(volume));
		if (!useSync)
		{
			return instance.Play(audioName, (AudioSource)null, num);
		}
		return instance.PlaySync(audioName, (AudioSource)null, num);
	}

	private static ulong PostUiSoundInternal(string audioName, float volume)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		using (new ProfilerScope("AudioService.PostUiSoundInternal"))
		{
			if (string.IsNullOrEmpty(audioName))
			{
				Log.LogError("Audio", "PlayUiSound 跳过: audioName 为空");
				return 0uL;
			}
			AudioForgeRuntime instance = AudioForgeRuntime.Instance;
			if ((Object)(object)instance == (Object)null || !instance.IsReady)
			{
				return 0uL;
			}
			float num = (Mathf.Approximately(volume, 1f) ? 0f : LinearToDb(volume));
			AudioForgeTriggerPayload val = new AudioForgeTriggerPayload(1f, 0, num);
			AudioForgePostReceipt val2 = instance.PostEvent(new AudioForgeEventHandle(audioName), (AudioForgeEmitterHandle)null, ref val);
			return ((AudioForgePostReceipt)(ref val2)).IsAccepted ? EncodePostHandle(val2.Handle) : 0;
		}
	}

	private static ulong EncodePostHandle(AudioForgePostHandle handle)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (!((AudioForgePostHandle)(ref handle)).IsValid)
		{
			return 0uL;
		}
		return (ulong)(long.MinValue | handle.Value);
	}

	private static bool TryDecodePostHandle(ulong soundHandle, out AudioForgePostHandle postHandle)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if ((soundHandle & 0x8000000000000000uL) == 0L)
		{
			postHandle = default(AudioForgePostHandle);
			return false;
		}
		long num = (long)(soundHandle & 0x7FFFFFFFFFFFFFFFL);
		postHandle = new AudioForgePostHandle(num);
		return ((AudioForgePostHandle)(ref postHandle)).IsValid;
	}

	public static void StopSound(string audioName)
	{
		AudioForgeRuntime instance = AudioForgeRuntime.Instance;
		if (!((Object)(object)instance == (Object)null) && instance.IsReady)
		{
			instance.StopEvent(audioName);
		}
	}

	public static bool IsEventPlaying(string audioName)
	{
		if (string.IsNullOrEmpty(audioName))
		{
			return false;
		}
		AudioForgeRuntime instance = AudioForgeRuntime.Instance;
		if ((Object)(object)instance == (Object)null || !instance.IsReady)
		{
			return false;
		}
		return instance.GetActiveVoiceCount(audioName) > 0;
	}

	public static bool StopSound(ulong guid)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		AudioForgeRuntime instance = AudioForgeRuntime.Instance;
		if ((Object)(object)instance == (Object)null || !instance.IsReady || guid == 0L)
		{
			return false;
		}
		if (TryDecodePostHandle(guid, out var postHandle))
		{
			return instance.StopPost(postHandle);
		}
		return instance.StopVoice(guid);
	}

	public static ulong PlayLoopIfNotPlaying(string audioName)
	{
		return PlayLoopIfNotPlayingInternal(audioName, useSync: false);
	}

	public static ulong PlayLoopIfNotPlayingSync(string audioName)
	{
		return PlayLoopIfNotPlayingInternal(audioName, useSync: true);
	}

	private static ulong PlayLoopIfNotPlayingInternal(string audioName, bool useSync)
	{
		if (string.IsNullOrEmpty(audioName))
		{
			Log.LogError("Audio", "PlayLoopIfNotPlaying 跳过: audioName 为空");
			return 0uL;
		}
		AudioForgeRuntime instance = AudioForgeRuntime.Instance;
		if ((Object)(object)instance == (Object)null || !instance.IsReady)
		{
			return 0uL;
		}
		if (instance.GetActiveVoiceCount(audioName) > 0)
		{
			return 0uL;
		}
		if (useSync)
		{
			return instance.PlaySync(audioName, (AudioSource)null, 0f);
		}
		return instance.Play(audioName, (AudioSource)null, 0f);
	}

	public static void SetSoundMuted(bool muted)
	{
		AudioForgeRuntime instance = AudioForgeRuntime.Instance;
		if ((Object)(object)instance != (Object)null && instance.IsReady)
		{
			instance.SetBusMuted("SFX", muted);
			instance.SetBusMuted("UI", muted);
			instance.SetBusMuted("VO", muted);
			instance.SetBusMuted("AMB", muted);
		}
		GameApp.Audio.SoundVolume = (muted ? 0f : 1f);
	}

	public static void SetMusicMuted(bool muted)
	{
		AudioForgeRuntime instance = AudioForgeRuntime.Instance;
		if ((Object)(object)instance != (Object)null && instance.IsReady)
		{
			instance.SetBusMuted("BGM", muted);
		}
		GameApp.Audio.MusicVolume = (muted ? 0f : 1f);
	}

	public static void SetSoundVolume(float linearVolume)
	{
		AudioForgeRuntime instance = AudioForgeRuntime.Instance;
		if ((Object)(object)instance != (Object)null && instance.IsReady)
		{
			instance.SetBusVolume("SFX", linearVolume);
			instance.SetBusVolume("UI", linearVolume);
		}
		GameApp.Audio.SoundVolume = linearVolume;
	}

	public static void SetMusicVolume(float linearVolume)
	{
		AudioForgeRuntime instance = AudioForgeRuntime.Instance;
		if ((Object)(object)instance != (Object)null && instance.IsReady)
		{
			instance.SetBusVolume("BGM", linearVolume);
		}
		GameApp.Audio.MusicVolume = linearVolume;
	}

	public static void PauseBgmForAd()
	{
		if (bgmPauseForAdCount == 0 && IsUserMusicOn())
		{
			BgmController.StopAllWsgBgm();
			SetMusicMuted(muted: true);
		}
		bgmPauseForAdCount++;
	}

	public static void ResumeBgmFromAd()
	{
		if (bgmPauseForAdCount <= 0)
		{
			bgmPauseForAdCount = 0;
			return;
		}
		bgmPauseForAdCount--;
		if (bgmPauseForAdCount == 0 && IsUserMusicOn())
		{
			SetMusicMuted(muted: false);
			BgmController.RefreshAfterMusicToggle();
		}
	}

	public static void PauseSoundForAd()
	{
		if (soundPauseForAdCount == 0 && IsUserSoundOn())
		{
			SetSoundMuted(muted: true);
		}
		soundPauseForAdCount++;
	}

	public static void ResumeSoundFromAd()
	{
		if (soundPauseForAdCount <= 0)
		{
			soundPauseForAdCount = 0;
			return;
		}
		soundPauseForAdCount--;
		if (soundPauseForAdCount == 0 && IsUserSoundOn())
		{
			SetSoundMuted(muted: false);
		}
	}

	private static bool IsUserMusicOn()
	{
		return GameApp.Proxy.Get<SettingProxy>().Data.IsMusicOn;
	}

	private static bool IsUserSoundOn()
	{
		return GameApp.Proxy.Get<SettingProxy>().Data.IsSoundOn;
	}

	private static void InitAudioForge()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		AudioForgeLog.SetSink((IAudioForgeLogSink)(object)new UniLogAudioForgeSink());
		AudioForgeLog.MinLevel = (AudioForgeLogLevel)2;
		if ((Object)(object)AudioForgeRuntime.Instance != (Object)null)
		{
			AudioForgeRuntime.Instance.SetResourceProvider((IAudioForgeResourceProvider)(object)CreateAudioForgeResourceProvider());
			AudioForgeRuntime.Instance.InitializeSync();
			ApplyConfigVolumeOffsets();
			ApplyAudioVolume();
			return;
		}
		GameObject val = new GameObject("[AudioForgeRuntime]");
		Object.DontDestroyOnLoad((Object)val);
		val.AddComponent<AudioForgeRuntime>();
		AudioForgeRuntime instance = AudioForgeRuntime.Instance;
		instance.SetResourceProvider((IAudioForgeResourceProvider)(object)CreateAudioForgeResourceProvider());
		instance.InitializeSync();
		ApplyConfigVolumeOffsets();
		ApplyAudioVolume();
	}

	private static YooAssetsResourceProvider CreateAudioForgeResourceProvider()
	{
		return new YooAssetsResourceProvider("Assets/", "02_Res/Art/Audio/AudioData.json", "02_Res/Art/Audio/Assets", "02_Res/Art/Audio/Activation/runtime_activation_manifest.json");
	}

	public static void ApplyAudioVolume()
	{
		SettingProxy settingProxy = GameApp.Proxy.Get<SettingProxy>();
		SetSoundMuted(!settingProxy.Data.IsSoundOn);
		SetMusicMuted(!settingProxy.Data.IsMusicOn);
	}

	private static void InitAudioGroup()
	{
		List<AudioGroupConfig> list = GameApp.Config.GetTable<ExampleTables>().TbAudioGroupConfig.DataList.FindAll((AudioGroupConfig item) => item.AudioGroupID == 1);
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		for (int num = 0; num < list.Count; num++)
		{
			AudioGroupConfig audioGroupConfig = list[num];
			dictionary[audioGroupConfig.OrgAudio] = audioGroupConfig.OverrideAudio;
		}
		AudioKeys.ApplyOverrides(dictionary);
	}

	public static void BeginBatchExperiments()
	{
		batchedActivateVariantIds = new List<string>();
		batchedDeactivateVariantIds = new List<string>();
	}

	public static void CommitExperiments()
	{
		if (batchedActivateVariantIds != null)
		{
			List<string> list = batchedActivateVariantIds;
			List<string> list2 = batchedDeactivateVariantIds;
			batchedActivateVariantIds = null;
			batchedDeactivateVariantIds = null;
			if (list2.Count > 0)
			{
				DeactivateExperiments(list2);
			}
			if (list.Count > 0)
			{
				ActivateExperiments(list);
			}
		}
	}

	public static void ActivateExperiment(string variantId)
	{
		if (batchedActivateVariantIds != null)
		{
			batchedActivateVariantIds.Add(variantId);
			return;
		}
		AudioForgeRuntime instance = AudioForgeRuntime.Instance;
		if ((Object)(object)instance == (Object)null || !instance.IsReady)
		{
			Log.LogError("Audio", "ActivateExperiment 失败: AudioForgeRuntime 未就绪");
		}
		else
		{
			instance.ActivateExperiment(variantId);
		}
	}

	public static void ActivateExperiments(List<string> variantIds)
	{
		AudioForgeRuntime instance = AudioForgeRuntime.Instance;
		if ((Object)(object)instance == (Object)null || !instance.IsReady)
		{
			Log.LogError("Audio", "ActivateExperiments 失败: AudioForgeRuntime 未就绪");
		}
		else
		{
			instance.ActivateExperiments(variantIds);
		}
	}

	public static void DeactivateExperiment(string variantId)
	{
		if (batchedDeactivateVariantIds != null)
		{
			batchedDeactivateVariantIds.Add(variantId);
			return;
		}
		AudioForgeRuntime instance = AudioForgeRuntime.Instance;
		if ((Object)(object)instance == (Object)null || !instance.IsReady)
		{
			Log.LogError("Audio", "DeactivateExperiment 失败: AudioForgeRuntime 未就绪");
		}
		else
		{
			instance.DeactivateExperiment(variantId);
		}
	}

	public static void DeactivateExperiments(List<string> variantIds)
	{
		AudioForgeRuntime instance = AudioForgeRuntime.Instance;
		if ((Object)(object)instance == (Object)null || !instance.IsReady)
		{
			Log.LogError("Audio", "DeactivateExperiments 失败: AudioForgeRuntime 未就绪");
		}
		else
		{
			instance.DeactivateExperiments(variantIds);
		}
	}

	public static void DeactivateAllExperiments()
	{
		AudioForgeRuntime instance = AudioForgeRuntime.Instance;
		if ((Object)(object)instance == (Object)null || !instance.IsReady)
		{
			Log.LogError("Audio", "DeactivateAllExperiments 失败: AudioForgeRuntime 未就绪");
		}
		else
		{
			instance.DeactivateAllExperiments();
		}
	}
}
