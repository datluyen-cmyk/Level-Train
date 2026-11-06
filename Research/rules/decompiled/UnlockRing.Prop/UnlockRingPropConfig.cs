using System;
using UnityEngine;
using XingYun.Utility;

namespace UnlockRing.Prop;

[CreateAssetMenu(fileName = "UnlockRingPropConfig", menuName = "Config/UnlockRingPropConfig")]
public class UnlockRingPropConfig : ScriptableObject
{
	private const string Tag = "[UnlockRingPropConfig] ";

	private const float DefaultHammerBreakDelaySeconds = 0.9f;

	private const float DefaultHammerHitSoundDelaySeconds = 0.35f;

	private const float DefaultRocketStartOffsetY = 150f;

	private const float DefaultRocketPrepareDuration = 0.5f;

	private const float DefaultRocketFlySpeed = 1000f;

	private const float DefaultRocketFlyDurationMax = 1.2f;

	private const float DefaultRocketNearFlyDurationMax = 1.5f;

	private const float DefaultRocketTakeoffOffsetY = 200f;

	private const float DefaultRocketTakeoffExtendOffsetY = 100f;

	private static readonly Vector2 DefaultRocketWaypoint1ProgressRange = new Vector2(0.3f, 0.4f);

	private static readonly Vector2 DefaultRocketWaypoint1OffsetRatioRange = new Vector2(0.2f, 0.35f);

	private static readonly Vector2 DefaultRocketWaypoint2ProgressRange = new Vector2(0.6f, 0.7f);

	private static readonly Vector2 DefaultRocketWaypoint2OffsetRatioRange = new Vector2(0.25f, 0.45f);

	private const float DefaultRocketNearThresholdY = 0.5f;

	private static readonly Vector2 DefaultRocketNearRightWaypointXRange = new Vector2(0.7f, 1f);

	private static readonly Vector2 DefaultRocketNearRightWaypointYRange = new Vector2(0.8f, 0.9f);

	private static readonly Vector2 DefaultRocketNearLeftWaypointXRange = new Vector2(0.2f, 0.4f);

	private static readonly Vector2 DefaultRocketNearLeftWaypointYRange = new Vector2(0.8f, 0.9f);

	private static readonly Vector2 DefaultRocketUpperP1XRange = new Vector2(0.25f, 0.75f);

	private const float DefaultRocketUpperP1YConst = 0.65f;

	private const float DefaultRocketUpperStartCtrlYOffset = 1f;

	private const float DefaultRocketUpperEndCtrlYOffset = 0f;

	private static readonly Vector2 DefaultRocketLowerP1XRange = new Vector2(0.25f, 0.75f);

	private const float DefaultRocketLowerP1YConst = 0.75f;

	private static readonly Vector2 DefaultRocketLowerP2XRange = new Vector2(0.2f, 0.8f);

	private static readonly Vector2 DefaultRocketLowerP2YJitterRange = new Vector2(-0.05f, 0.05f);

	private const float DefaultRocketLowerStartCtrlYOffset = 1f;

	private const float DefaultRocketLowerEndCtrlYOffset = 0f;

	private static readonly AnimationCurve DefaultRocketFlyEase = new AnimationCurve((Keyframe[])(object)new Keyframe[2]
	{
		new Keyframe(0f, 0f, 0f, 0f),
		new Keyframe(1f, 1f, 3f, 3f)
	});

	private static readonly AnimationCurve DefaultRocketScaleCurve = AnimationCurve.Linear(0f, 2f, 1f, 1f);

	private static UnlockRingPropConfig instance;

	[Header("锤子道具")]
	[Tooltip("锤子动画播放后，延迟多少秒执行 ForceRemove（环破碎）并关闭引导界面。\n需与锤子砸击特效节奏对齐；越大破碎越晚，越小破碎越早。\n推荐范围：0.5 ~ 1.2")]
	public float hammerBreakDelaySeconds = 0.9f;

	[Tooltip("锤子特效开始播放后，延迟多少秒播放砸击音效。\n独立于破碎延时，用于与砸击动画节奏对齐。\n推荐范围：0.2 ~ 0.6")]
	public float hammerHitSoundDelaySeconds = 0.35f;

	[Header("小礼炮-起点与预备(通用)")]
	[Tooltip("飞行起点Y坐标：局内道具 icon 正上方偏移位置。\n越大起点越偏上，推荐范围：100 ~ 200")]
	public float rocketStartOffsetY = 150f;

	[Tooltip("起点预备展示时长（秒）：礼炮在起点停留一段时间后再开始飞行。\n推荐范围：0.2 ~ 0.8")]
	public float rocketPrepareDuration = 0.5f;

	[Header("小礼炮-飞行(通用)")]
	[Tooltip("飞行基本速度（位置点/秒）：飞行时间 = 路径长度 / 速度。\n模式1/模式2 共用，越大飞得越快，推荐范围：600 ~ 1500")]
	public float rocketFlySpeed = 1000f;

	[Tooltip("飞行缓动曲线：X 为归一化时间(0~1)、Y 为进度(0~1)。\n默认近似 InCubic（起始平缓、末端加速）；可自定义任意贝塞尔曲线。\nY 值可超出 0~1 制造回弹/过冲效果。")]
	public AnimationCurve rocketFlyEase = DefaultRocketFlyEase;

	[Tooltip("飞行期间缩放曲线。X 为归一化时间(0~1)、Y 直接为 scale 绝对值。\n默认 (0,2)→(1,1)：起飞时 scale=2，到达时 scale=1，线性递减。\n可自定义先慢后快/先快后慢/回弹等曲线，Y 值即该时刻的 scale。")]
	public AnimationCurve rocketScaleCurve = DefaultRocketScaleCurve;

	[Header("小礼炮-模式1(远处曲线)")]
	[Tooltip("模式1 起飞段：起点正上方偏移位置，礼炮先垂直上升到此点再转向目标。\n设为 0 表示无起飞段，路径降为 4 点。\n推荐范围：100 ~ 400")]
	public float rocketTakeoffOffsetY = 200f;

	[Tooltip("起飞延伸点偏移量：起飞点正上方再延伸一段，与起点、起飞点三点共线(x相同)，\n保证 CatmullRom 起飞段为垂直直线、起飞点切线也纯垂直。\n设为 0 时退化为不插延伸点(5点路径)，仅靠起飞点单点近似垂直。\n推荐范围：50 ~ 150，建议小于 rocketTakeoffOffsetY")]
	public float rocketTakeoffExtendOffsetY = 100f;

	[Tooltip("模式1 飞行时间上限（秒）：超出时按此上限截断，避免路径过长时飞行过慢。\n推荐范围：0.8 ~ 2.0")]
	public float rocketFlyDurationMax = 1.2f;

	[Tooltip("模式1 过渡点1 在起飞点→终点连线上的位置范围(x=min, y=max)。\nx、y 均取值(0.0-1.0)，0=起飞点、1=终点。\n运行时在此范围内随机取一个值作为该过渡点的连线位置。\n建议小于过渡点2的位置范围下限，推荐(0.3, 0.4)")]
	public Vector2 rocketWaypoint1ProgressRange = DefaultRocketWaypoint1ProgressRange;

	[Tooltip("模式1 过渡点1 垂直偏移比例范围(x=min, y=max)。\nx、y 均取值(0.0-1.0)，运行时在此范围内随机取一个值，\n乘以起飞点→终点连线长度得到实际偏移像素。\n越大拐弯幅度越大，推荐(0.2, 0.35)")]
	public Vector2 rocketWaypoint1OffsetRatioRange = DefaultRocketWaypoint1OffsetRatioRange;

	[Tooltip("模式1 过渡点2 在起飞点→终点连线上的位置范围(x=min, y=max)。\nx、y 均取值(0.0-1.0)，0=起飞点、1=终点。\n运行时在此范围内随机取一个值作为该过渡点的连线位置。\n建议大于过渡点1的位置范围上限，推荐(0.6, 0.7)")]
	public Vector2 rocketWaypoint2ProgressRange = DefaultRocketWaypoint2ProgressRange;

	[Tooltip("模式1 过渡点2 垂直偏移比例范围(x=min, y=max)。\nx、y 均取值(0.0-1.0)，运行时在此范围内随机取一个值，\n乘以起飞点→终点连线长度得到实际偏移像素。\n越大拐弯幅度越大，推荐(0.25, 0.45)")]
	public Vector2 rocketWaypoint2OffsetRatioRange = DefaultRocketWaypoint2OffsetRatioRange;

	[Header("小礼炮-模式2(近处曲线)")]
	[Tooltip("近处/远处判定阈值：目标在 RingRect 内的 y 百分比。\ny 百分比 < 此值时使用模式2(近处曲线)，否则使用模式1(远处曲线)。\n取值(0.0-1.0)，0=RingRect 底部、1=RingRect 顶部，推荐 0.5")]
	public float rocketNearThresholdY = 0.5f;

	[Tooltip("模式2 飞行时间上限（秒）：路径更长(6-7点)，通常比模式1略大。\n推荐范围：1.0 ~ 2.5")]
	public float rocketNearFlyDurationMax = 1.5f;

	[Tooltip("模式2 右侧过渡点 x 百分比范围(x=min, y=max)，相对 RingRect 宽度。\n0=RingRect 左边、1=RingRect 右边，推荐(0.7, 1.0)")]
	public Vector2 rocketNearRightWaypointXRange = DefaultRocketNearRightWaypointXRange;

	[Tooltip("模式2 右侧过渡点 y 百分比范围(x=min, y=max)，相对 RingRect 高度。\n0=RingRect 底部、1=RingRect 顶部，推荐(0.8, 0.9)")]
	public Vector2 rocketNearRightWaypointYRange = DefaultRocketNearRightWaypointYRange;

	[Tooltip("模式2 左侧过渡点 x 百分比范围(x=min, y=max)，相对 RingRect 宽度。\n0=RingRect 左边、1=RingRect 右边，推荐(0.2, 0.4)")]
	public Vector2 rocketNearLeftWaypointXRange = DefaultRocketNearLeftWaypointXRange;

	[Tooltip("模式2 左侧过渡点 y 百分比范围(x=min, y=max)，相对 RingRect 高度。\n0=RingRect 底部、1=RingRect 顶部，推荐(0.8, 0.9)")]
	public Vector2 rocketNearLeftWaypointYRange = DefaultRocketNearLeftWaypointYRange;

	[Header("小礼炮-新曲线规则-上半部分(y>=阈值)")]
	[Tooltip("上半部分 P1 必经点 x 归一化范围(x=min, y=max)，相对 RingRect 宽度。\n0=RingRect 左边、1=RingRect 右边，取值(0.0-1.0)")]
	public Vector2 rocketUpperP1XRange = DefaultRocketUpperP1XRange;

	[Tooltip("上半部分 P1.y 计算常量。\ny(P1) = 0.5 * y(Pn) + 此值，控制上半部分曲线峰值高度，取值(0.0-1.0)，推荐 0.65")]
	public float rocketUpperP1YConst = 0.65f;

	[Header("小礼炮-新曲线规则-下半部分(y<阈值)")]
	[Tooltip("下半部分 P1 必经点 x 归一化范围(x=min, y=max)，相对 RingRect 宽度。\n0=RingRect 左边、1=RingRect 右边，取值(0.0-1.0)")]
	public Vector2 rocketLowerP1XRange = DefaultRocketLowerP1XRange;

	[Tooltip("下半部分 P1.y 计算常量。\ny(P1) = 0.5 * y(Pn) + 此值，控制下半部分曲线峰值高度，取值(0.0-1.0)，推荐 0.75")]
	public float rocketLowerP1YConst = 0.75f;

	[Tooltip("下半部分 P2 必经点 x 归一化范围(x=min, y=max)，相对 RingRect 宽度。\n0=RingRect 左边、1=RingRect 右边，取值(0.0-1.0)")]
	public Vector2 rocketLowerP2XRange = DefaultRocketLowerP2XRange;

	[Tooltip("下半部分 P2.y 扰动范围(x=min, y=max)。\ny(P2) = 0.5*(y(P1)+y(P3)) + 此范围内随机值，取值(-0.1-0.1)")]
	public Vector2 rocketLowerP2YJitterRange = DefaultRocketLowerP2YJitterRange;

	public static UnlockRingPropConfig Instance
	{
		get
		{
			if (Object.op_Implicit((Object)(object)instance))
			{
				return instance;
			}
			try
			{
				instance = GameApp.Res.LoadAsset<UnlockRingPropConfig>(Res.GetDefaultResPath("UnlockRingPropConfig"), (string)null);
			}
			catch (Exception)
			{
				instance = ScriptableObject.CreateInstance<UnlockRingPropConfig>();
			}
			return instance;
		}
	}

	public static void Reload()
	{
		instance = null;
	}

	public float GetHammerBreakDelaySeconds()
	{
		if (hammerBreakDelaySeconds <= 0f)
		{
			return 0.9f;
		}
		return hammerBreakDelaySeconds;
	}

	public float GetHammerHitSoundDelaySeconds()
	{
		if (hammerHitSoundDelaySeconds <= 0f)
		{
			return 0.35f;
		}
		return hammerHitSoundDelaySeconds;
	}

	public float GetRocketStartOffsetY()
	{
		return rocketStartOffsetY;
	}

	public float GetRocketPrepareDuration()
	{
		if (rocketPrepareDuration <= 0f)
		{
			return 0.5f;
		}
		return rocketPrepareDuration;
	}

	public float GetRocketFlySpeed()
	{
		if (rocketFlySpeed <= 0f)
		{
			return 1000f;
		}
		return rocketFlySpeed;
	}

	public float GetRocketFlyDurationMax()
	{
		if (rocketFlyDurationMax <= 0f)
		{
			return 1.2f;
		}
		return rocketFlyDurationMax;
	}

	public float GetRocketNearFlyDurationMax()
	{
		if (rocketNearFlyDurationMax <= 0f)
		{
			return 1.5f;
		}
		return rocketNearFlyDurationMax;
	}

	public AnimationCurve GetRocketFlyEase()
	{
		if (rocketFlyEase == null || rocketFlyEase.length < 2)
		{
			return DefaultRocketFlyEase;
		}
		return rocketFlyEase;
	}

	public float GetRocketTakeoffOffsetY()
	{
		if (rocketTakeoffOffsetY < 0f)
		{
			return 200f;
		}
		return rocketTakeoffOffsetY;
	}

	public float GetRocketTakeoffExtendOffsetY()
	{
		if (rocketTakeoffExtendOffsetY < 0f)
		{
			return 100f;
		}
		return rocketTakeoffExtendOffsetY;
	}

	public Vector2 GetRocketWaypoint1ProgressRange()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return ValidateRange(rocketWaypoint1ProgressRange, DefaultRocketWaypoint1ProgressRange);
	}

	public Vector2 GetRocketWaypoint1OffsetRatioRange()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return ValidateRange(rocketWaypoint1OffsetRatioRange, DefaultRocketWaypoint1OffsetRatioRange);
	}

	public Vector2 GetRocketWaypoint2ProgressRange()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return ValidateRange(rocketWaypoint2ProgressRange, DefaultRocketWaypoint2ProgressRange);
	}

	public Vector2 GetRocketWaypoint2OffsetRatioRange()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return ValidateRange(rocketWaypoint2OffsetRatioRange, DefaultRocketWaypoint2OffsetRatioRange);
	}

	public float GetRocketNearThresholdY()
	{
		if (rocketNearThresholdY < 0f || rocketNearThresholdY > 1f)
		{
			return 0.5f;
		}
		return rocketNearThresholdY;
	}

	public Vector2 GetRocketNearRightWaypointXRange()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return ValidateRange(rocketNearRightWaypointXRange, DefaultRocketNearRightWaypointXRange);
	}

	public Vector2 GetRocketNearRightWaypointYRange()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return ValidateRange(rocketNearRightWaypointYRange, DefaultRocketNearRightWaypointYRange);
	}

	public Vector2 GetRocketNearLeftWaypointXRange()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return ValidateRange(rocketNearLeftWaypointXRange, DefaultRocketNearLeftWaypointXRange);
	}

	public Vector2 GetRocketNearLeftWaypointYRange()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return ValidateRange(rocketNearLeftWaypointYRange, DefaultRocketNearLeftWaypointYRange);
	}

	public Vector2 GetRocketUpperP1XRange()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return ValidateRange(rocketUpperP1XRange, DefaultRocketUpperP1XRange);
	}

	public float GetRocketUpperP1YConst()
	{
		if (rocketUpperP1YConst < 0f || rocketUpperP1YConst > 1f)
		{
			return 0.65f;
		}
		return rocketUpperP1YConst;
	}

	public Vector2 GetRocketLowerP1XRange()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return ValidateRange(rocketLowerP1XRange, DefaultRocketLowerP1XRange);
	}

	public float GetRocketLowerP1YConst()
	{
		if (rocketLowerP1YConst < 0f || rocketLowerP1YConst > 1f)
		{
			return 0.75f;
		}
		return rocketLowerP1YConst;
	}

	public Vector2 GetRocketLowerP2XRange()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return ValidateRange(rocketLowerP2XRange, DefaultRocketLowerP2XRange);
	}

	public Vector2 GetRocketLowerP2YJitterRange()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return rocketLowerP2YJitterRange;
	}

	private static Vector2 ValidateRange(Vector2 range, Vector2 defaultValue)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (range.x < 0f || range.x > 1f || range.y < 0f || range.y > 1f || range.x > range.y)
		{
			return defaultValue;
		}
		return range;
	}

	public AnimationCurve GetRocketScaleCurve()
	{
		if (rocketScaleCurve == null || rocketScaleCurve.length < 2)
		{
			return DefaultRocketScaleCurve;
		}
		return rocketScaleCurve;
	}
}
