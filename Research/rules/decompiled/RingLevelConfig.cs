using System;
using UnityEngine;
using XingYun.Utility;

[CreateAssetMenu(fileName = "RingLevelConfig", menuName = "Config/RingLevelConfig")]
public class RingLevelConfig : ScriptableObject
{
	private const string TAG = "[RingLevelConfig] ";

	private static RingLevelConfig instance;

	[Tooltip("单个圆环从「起始态」到「落定态」的持续时间（秒）。\n越大：每个环动作越慢、整体更柔；越小：弹出越快、更利落。\n推荐范围：0.3 ~ 0.6")]
	public float waveDuration = 0.5f;

	[Tooltip("相邻两个圆环开始动画的基础时间间隔 a（秒）。RotateRing 入场时与 b/c 配合动态计算实际间隔。\n排序为从上到下，上方先动。\n越小：波纹更密、推进更快；越大：波纹更疏、依次感更强。\n推荐范围：0.02 ~ 0.08")]
	public float waveInterval = 0.03f;

	[Tooltip("全部元素错峰出场的最大总时长 b（秒）。RotateRing 入场时实际间隔 t = max(min(a, b/x), c)。\n元素多时压缩间隔，使错峰总时长趋近 b。\n推荐范围：0.5 ~ 2")]
	public float waveMaxEnterDuration = 1f;

	[Tooltip("相邻元素最小出场间隔 c（秒）。优先级高于 b，实际总时长可能超过 b。\n若不希望总时长超过 b，可将此值设为 0。\n推荐范围：0 ~ 0.02")]
	public float waveMinInterval = 0.01f;

	[Tooltip("入场动画开始时全部圆环的缩放比例，相对进关后的正常大小（1 = 原始大小）。\n未轮到播放的环保持此缩放；例如 0.72 表示等待中的环显示为 72% 大小。\n推荐范围：0.5 ~ 0.9")]
	[Range(0f, 1f)]
	public float initialAllRingsScaleFactor = 0.72f;

	[Tooltip("单个圆环动画开始时的缩放比例，相对进关后的正常大小（1 = 原始大小）。\n例如 0.72 表示从 72% 大小放大到 100%；越小起始越小、弹出感越强。\n推荐范围：0.5 ~ 0.9")]
	[Range(0f, 1f)]
	public float startScaleFactor = 0.72f;

	[Tooltip("单个圆环缩放节奏曲线。\n横轴：该环动画进度 0（刚开始）→ 1（结束）。\n纵轴：缩放插值 0 = 使用 startScaleFactor，1 = 正常大小。\n前段陡 = 快速弹出；末段缓 = 柔和落定；>1 可做过冲回弹。")]
	public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[Tooltip("整个抖动动画的总时长（秒）。\n越大振荡越慢；越小越急促。\n推荐范围：0.2 ~ 0.5")]
	public float blockedShakeDuration = 0.32f;

	[Tooltip("曲线纵轴峰值对应的最大旋转角度（度）。\n最终角度 = blockedShakeCurve.Evaluate(t) × blockedShakeStrength。\n推荐范围：4 ~ 12")]
	public float blockedShakeStrength = 8f;

	[Tooltip("衰减振荡曲线。\n横轴：动画进度 0（开始）→ 1（结束）。\n纵轴：振荡系数，可正可负；绘制逐渐衰减的波形，首尾应为 0。\n波峰越高、间距越密 = 振荡越强越频繁；末段趋近 0 = 柔和停止。")]
	public AnimationCurve blockedShakeCurve = CreateDefaultBlockedShakeCurve();

	[Tooltip("果冻挤压初始值（负值向内收缩）。\n推荐范围：-30 ~ -10")]
	public float jellyOvershoot = -20f;

	[Tooltip("果冻弹起峰值（AtlasPadding 像素）。\n推荐范围：3 ~ 10")]
	public float jellyPeak = 5f;

	[Tooltip("果冻动画总时长（秒）。\n推荐范围：1 ~ 3")]
	public float jellyDuration = 2f;

	[Tooltip("阻尼比（0 = 无阻尼，1 = 临界阻尼）。\n推荐范围：0.05 ~ 0.3")]
	[Range(0.01f, 2f)]
	public float jellyDamping = 0.1f;

	[Tooltip("振荡频率（越高摆动越快）。\n推荐范围：4 ~ 12")]
	public float jellyFrequency = 8f;

	[Tooltip("DOTween 进度缓动曲线。\n横轴：动画进度 0（开始）→ 1（结束）。\n纵轴： eased 进度 0 → 1；默认线性。")]
	public AnimationCurve jellyEaseCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[Tooltip("第二/三条锁链边缘向开口外侧扩展的固定偏移角（度），使锁链远离缺口边缘。\n推荐范围：5 ~ 25")]
	public float chainEdgeInsetDeg = 15f;

	[Tooltip("多条锁链 break 动画之间的间隔时长（秒）：同环或不同环同时断裂时逐条错峰播放，避免视觉/音效叠加。\n推荐范围：0.1 ~ 0.4")]
	public float chainBreakIntervalSeconds = 0.2f;

	[Tooltip("锁链断裂前统一空拍时长（秒）：解环成功后空拍再断链；炸弹引爆时先等爆炸尾声、再叠加此空拍。\n推荐范围：0.2 ~ 0.6")]
	public float chainBreakStartDelaySeconds = 0.4f;

	[Tooltip("多个带锁环同时解环时，环与环之间 break 动画与碎环的错峰间隔最小值（秒）：同一波次内按环顺序逐环延后，避免多个环同帧断裂/破碎。\n推荐范围：0.1 ~ 0.4")]
	public float chainRingBreakIntervalSecondsMin = 0.2f;

	[Tooltip("多个带锁环同时解环时，环与环之间 break 动画与碎环的错峰间隔最大值（秒）：实际间隔在 min 与 max 之间随机取值。\n推荐范围：0.2 ~ 0.6")]
	public float chainRingBreakIntervalSecondsMax = 0.4f;

	[Tooltip("锁头打开动画期间，剩余锁数量文本（txtNum）的透明度曲线。\n横轴：打开动画进度 0（开始）→ 1（结束）。\n纵轴：txtNum 透明度 1 = 不透明，0 = 完全透明；默认 1→0 平滑淡出。\n若不需要淡出，可将曲线整体拉平到 1。")]
	public AnimationCurve lockOpenNumFadeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

	[Tooltip("钥匙解环后，多段飞向被锁环时相邻两段起飞的时间间隔（秒）。\n推荐范围：0.1 ~ 0.25")]
	public float paletteLockKeyUnlockFlyStartIntervalSeconds = 0.15f;

	[Tooltip("单段飞粒子播放时长估算之外的超时余量（秒），超时后强制完成解锁。\n推荐范围：0.5 ~ 2")]
	public float paletteLockKeyUnlockFlyTimeoutPaddingSeconds = 1f;

	[Tooltip("彩色锁钥匙飞到后，宿主环断链/碎环较其灰色缺口补块(碎缺口)碎裂再延后的间隔（秒）。\n推荐范围：0.1 ~ 0.35")]
	public float paletteLockCascadeSolveDelaySeconds = 0.2f;

	public static RingLevelConfig Instance
	{
		get
		{
			if (Object.op_Implicit((Object)(object)instance))
			{
				return instance;
			}
			try
			{
				instance = GameApp.Res.LoadAsset<RingLevelConfig>(Res.GetDefaultResPath("RingLevelConfig"), (string)null);
			}
			catch (Exception)
			{
				instance = ScriptableObject.CreateInstance<RingLevelConfig>();
			}
			return instance;
		}
	}

	public static void Reload()
	{
		instance = null;
	}

	public AnimationCurve GetJellyEase()
	{
		if (jellyEaseCurve == null || jellyEaseCurve.length < 2)
		{
			return AnimationCurve.Linear(0f, 0f, 1f, 1f);
		}
		return jellyEaseCurve;
	}

	public static AnimationCurve CreateDefaultBlockedShakeCurve()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Expected O, but got Unknown
		return new AnimationCurve((Keyframe[])(object)new Keyframe[7]
		{
			new Keyframe(0f, 0f),
			new Keyframe(0.12f, 1f),
			new Keyframe(0.24f, -0.65f),
			new Keyframe(0.38f, 0.38f),
			new Keyframe(0.52f, -0.18f),
			new Keyframe(0.68f, 0.06f),
			new Keyframe(1f, 0f)
		});
	}

	public float GetTotalDuration(int ringCount)
	{
		if (ringCount <= 0)
		{
			return 0f;
		}
		return (float)(ringCount - 1) * waveInterval + waveDuration;
	}

	public void ApplyInitialAllRingsScale(Transform ring, Vector3 originalScale)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)ring == (Object)null))
		{
			ring.localScale = originalScale * initialAllRingsScaleFactor;
		}
	}

	public void ApplyRingWaveScale(Transform ring, Vector3 originalScale, float t)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)ring == (Object)null))
		{
			float num = scaleCurve.Evaluate(t);
			Vector3 val = originalScale * startScaleFactor;
			ring.localScale = Vector3.LerpUnclamped(val, originalScale, num);
		}
	}
}
