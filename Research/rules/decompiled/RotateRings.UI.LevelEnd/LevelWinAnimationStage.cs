using UnityEngine;

namespace RotateRings.UI.LevelEnd;

public enum LevelWinAnimationStage
{
	[InspectorName("界面准备完成")]
	Ready,
	[InspectorName("整体入场")]
	RootAppear,
	[InspectorName("松鼠庆祝")]
	SquirrelOpen,
	[InspectorName("顶部栏出现")]
	TopBar,
	[InspectorName("整体与松鼠入场")]
	EntranceComplete,
	[InspectorName("分数显示/滚动")]
	ScoreRoll,
	[InspectorName("分数强调")]
	ScoreEmphasis,
	[InspectorName("鼓励词显示")]
	Encouragement,
	[InspectorName("IQ面板弹出")]
	IqAppear,
	[InspectorName("宝箱区域弹出")]
	ChestAppear,
	[InspectorName("IQ与宝箱弹出")]
	PanelsComplete,
	[InspectorName("IQ数值增长")]
	IqGrowth,
	[InspectorName("宝箱进度增长")]
	ChestGrowth,
	[InspectorName("下一关按钮出现")]
	NextButton
}
