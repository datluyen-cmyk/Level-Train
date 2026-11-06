using System.Collections.Generic;
using RotateRings.WeakGuide.Data;
using UnityEngine;

namespace RotateRings.WeakGuide.Setup;

[CreateAssetMenu(fileName = "WeakGuideGlobalConfig", menuName = "RotateRings/WeakGuide/Global Config")]
public sealed class WeakGuideGlobalConfig : ScriptableObject
{
	[Header("加载路径")]
	[Tooltip("编辑器 AssetDatabase 加载路径(默认 Assets/02_Res/Art/ScriptableObject/WeakGuide/WeakGuideGlobalConfig.asset)。Default 在编辑器下优先按 AssetDatabase 加载,保证编辑时立即看到最新字段值。")]
	public string EditorAssetPath = "Assets/02_Res/Art/ScriptableObject/WeakGuide/WeakGuideGlobalConfig.asset";

	[Tooltip("运行时资源 location(供 GameApp.Res.LoadAsset 同步加载)。需在 YooAssets 资源映射表中注册;通常与 EditorAssetPath 主文件名一致。")]
	public string RuntimeLocation = "WeakGuideGlobalConfig";

	[Header("主 WeakGuide prefab")]
	[Tooltip("主 WeakGuide prefab 路径(Assets/ 下绝对路径或 YooAssets location)。")]
	public string MainPrefabPath = "Assets/02_Res/Art/Level/LevelPrefab/WeakGuidManager.prefab";

	[Tooltip("资源加载组,缺省 'LevelSceneGroup'。")]
	public string ResGroup = "LevelSceneGroup";

	[Header("eff_guide_hand prefab")]
	[Tooltip("小手 UI prefab 路径(Assets/02_Res/Art/Effect/Prefab/eff_rr_gp1/eff_guide_hand.prefab)。")]
	public string HandPrefabPath = "Assets/02_Res/Art/Effect/Prefab/eff_rr_gp1/eff_guide_hand.prefab";

	[Header("高亮材质")]
	[Tooltip("副本高亮 RingExtraGlow.mat 路径(Assets/02_Res/Art/Level/LevelMaterials/RingExtraGlow.mat),缺省时尝试从 Resources 加载。")]
	public string HighlightMaterialPath = "Assets/02_Res/Art/Level/LevelMaterials/RingExtraGlow.mat";

	[Header("节点名(WeakGuide prefab 内)")]
	[Tooltip("世界空间小手节点名,WeakIdMenuManagerLevel1 prefab 内的 GuidHand1。")]
	public string WorldSpaceHandNodeName = "GuidHand1";

	[Tooltip("UI 实例化后挂到 WeakGuideView Canvas 的手部节点名(用于 Transform.Find 复用)。")]
	public string HandInstanceName = "GuideHand1";

	[Tooltip("SkeletonGraphic 子节点名(用于 SpinePlayer 定位)。")]
	public string HandSkeletonNodeName = "hand";

	[Header("引导 SO 白名单")]
	[Tooltip("各关卡引导步骤 SO 引用列表。运行时由 Default 懒加载时一次性灌入 WeakGuideStepsLocator,只有这些 SO 中包含的 (LevelId, GuideIndex) 才会触发新手引导;Locator 编辑器扫盘等其他来源的索引项会被清空,以本列表为权威。为空或全 null 时,所有关卡都不会触发引导。")]
	public List<GuideStepsAsset> StepAssets = new List<GuideStepsAsset>();

	internal const string FallbackTargetRingPhysicPartId = "ring_2";

	internal const string FallbackRing1AdjustmentPhysicPartId = "ring_1";

	private static WeakGuideGlobalConfig cachedDefault;

	private const string EditorAssetPathFallback = "Assets/02_Res/Art/ScriptableObject/WeakGuide/WeakGuideGlobalConfig.asset";

	private const string RuntimeLocationFallback = "WeakGuideGlobalConfig";

	public static WeakGuideGlobalConfig Default
	{
		get
		{
			if ((Object)(object)cachedDefault != (Object)null)
			{
				return cachedDefault;
			}
			WeakGuideGlobalConfig weakGuideGlobalConfig = null;
			if (!string.IsNullOrEmpty("WeakGuideGlobalConfig") && (Object)(object)weakGuideGlobalConfig == (Object)null)
			{
				weakGuideGlobalConfig = GameApp.Res.LoadAsset<WeakGuideGlobalConfig>("WeakGuideGlobalConfig", (string)null);
				_ = (Object)(object)weakGuideGlobalConfig == (Object)null;
			}
			if ((Object)(object)weakGuideGlobalConfig != (Object)null)
			{
				cachedDefault = weakGuideGlobalConfig;
				WeakGuideStepsLocator weakGuideStepsLocator = WeakGuideStepsLocator.Default;
				weakGuideStepsLocator.Clear();
				int num = 0;
				if (weakGuideGlobalConfig.StepAssets != null)
				{
					for (int i = 0; i < weakGuideGlobalConfig.StepAssets.Count; i++)
					{
						weakGuideStepsLocator.Register(weakGuideGlobalConfig.StepAssets[i]);
						num++;
					}
				}
				return cachedDefault;
			}
			cachedDefault = ScriptableObject.CreateInstance<WeakGuideGlobalConfig>();
			((Object)cachedDefault).name = "WeakGuideGlobalConfig (Fallback)";
			return cachedDefault;
		}
	}

	public static void ResetDefault()
	{
		cachedDefault = null;
	}
}
