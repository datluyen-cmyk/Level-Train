using System;
using Cysharp.Threading.Tasks;
using RotateRings;
using RotateRings.GamePlay.Services;
using RotateRings.Halloween;
using RotateRings.UI.Level;
using UniLog;
using UnityEngine;

namespace RotateRing.GamePlay;

public static class GameplaySceneLoader
{
	private const string TAG = "[GameplaySceneLoader]";

	public static bool IsSceneReady => Gameplay.Active != null;

	public static void PrepareSceneConfig()
	{
		GameConst.IsCreatePhysic = true;
	}

	public static void LoadSceneAsync(Action onSceneReady = null, Action onSceneFailed = null)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		PrepareSceneConfig();
		GameObject val = GameApp.Res.LoadAsset<GameObject>("GameScene", (string)null);
		if ((Object)(object)val == (Object)null)
		{
			Log.LogError("XYFW-UIManager", "加载Prefab失败: GameScene");
			onSceneFailed?.Invoke();
			return;
		}
		GameObject val2 = GameObject.Find("Main Camera");
		if ((Object)(object)val2 != (Object)null)
		{
			val2.gameObject.SetActive(false);
		}
		GameObject obj = Object.Instantiate<GameObject>(val, (Transform)null, false);
		obj.transform.position = Vector3.zero;
		obj.transform.localScale = Vector3.one;
		((Object)obj.gameObject).name = "GameScene";
		if (Gameplay.Active is GameplayManager gameplayManager)
		{
			gameplayManager.GetGameService<RotateRingBackgroundAdaptation>()?.RefreshThemeMaterial();
			gameplayManager.GetGameService<HalloweenGameBackgroundService>()?.SyncBackgroundForLevelEntry();
		}
		onSceneReady?.Invoke();
	}

	public static void LoadLevel(int level)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		IGameplayManager active = Gameplay.Active;
		if (active == null)
		{
			Log.LogError("[GameplaySceneLoader]", "LoadLevel 失败: 场景未就绪, level={0}", new object[1] { level });
			EnterLevelCommand.ReleaseEnterGate();
		}
		else
		{
			UniTaskVoid val = active.RunLevelEnterAsync(level);
			((UniTaskVoid)(ref val)).Forget();
		}
	}
}
