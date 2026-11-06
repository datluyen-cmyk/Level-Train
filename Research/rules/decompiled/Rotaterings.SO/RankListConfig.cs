using UnityEngine;

namespace Rotaterings.SO;

[CreateAssetMenu(fileName = "RankListConfig", menuName = "UI/RankListConfig")]
public class RankListConfig : ScriptableObject
{
	private static RankListConfig _instance;

	[Header("列表 Item 依次出现的间隔（秒）")]
	public float itemShowInterval = 0.2f;

	[Space(15f)]
	[Header("初始时保持选中态的时长")]
	public float initSelectedDuration = 0.5f;

	[Header("初始箭头不移动时选中时长")]
	public float noArrowMoveDelay = 0.5f;

	[Space(15f)]
	[Header("箭头移动到目标位置的时间（秒")]
	public float arrowMoveDuration = 0.3f;

	[Header("延迟多久出现分数")]
	[Tooltip("箭头动画完成后，显示收集物图标前的等待时间（秒）")]
	public float delayShowScore = 0.2f;

	[Header("延迟多久出现星星")]
	public float delayShowStar = 0.2f;

	[Space(15f)]
	[Header("收集物图标飞行时长（秒）")]
	public float collectionFlyDuration = 0.6f;

	[Tooltip("飞行速度曲线（控制沿路径的快慢）")]
	public AnimationCurve CollectionFlyCurve = new AnimationCurve((Keyframe[])(object)new Keyframe[2]
	{
		new Keyframe(0f, 0f, 0f, 0.5f),
		new Keyframe(1f, 1f, 2.5f, 0f)
	});

	[Header("收集物相邻两次飞行的间隔（秒）")]
	public float collectionFlyInterval = 0.15f;

	[Header("弧线偏移强度（负值=向左凸，绝对值越大弧度越明显）")]
	public float collectionFlyArcStrength = -1.5f;

	[Space(15f)]
	[Header("列表滚动曲线")]
	public AnimationCurve listMoveCurve = new AnimationCurve((Keyframe[])(object)new Keyframe[2]
	{
		new Keyframe(0f, 0f, 0f, 2f),
		new Keyframe(1f, 1f, 0f, 0f)
	});

	[Space(10f)]
	[Header("结算爬排名 - 到位后下窜曲线")]
	public AnimationCurve settleGapAnimCurve = new AnimationCurve((Keyframe[])(object)new Keyframe[2]
	{
		new Keyframe(0f, 0f, 0f, 0f),
		new Keyframe(1f, 1f, 2f, 0f)
	});

	[Space(15f)]
	[Header("主页自己吸底 - 相对视口底边下移（像素）")]
	[Tooltip("半露出吸底判定线与吸底副本位置共用：在视口底边基础上再往下偏移")]
	public float mainSelfStickyBottomOffset = 10f;

	public static RankListConfig Instance
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)_instance))
			{
				_instance = GameApp.Res.LoadAsset<RankListConfig>("RankListConfig", (string)null);
			}
			return _instance;
		}
	}
}
