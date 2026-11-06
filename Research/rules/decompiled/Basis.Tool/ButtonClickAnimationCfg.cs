using UnityEngine;

namespace Basis.Tool;

[CreateAssetMenu(fileName = "ButtonClickAnimationCfg", menuName = "UI/AnimationCfg/ButtonClick")]
public class ButtonClickAnimationCfg : ScriptableObject
{
	private static ButtonClickAnimationCfg instance;

	[Header("圆形按钮按钮点击效果配置")]
	public ButtonClickEffectConfig circleClickEffectConfig;

	[Header("长方形带外框按钮点击效果配置")]
	public ButtonClickEffectConfig rectangleFrameClickEffectConfig;

	[Header("正方形带外框按钮点击效果配置")]
	public ButtonClickEffectConfig squareFrameClickEffectConfig;

	public static ButtonClickAnimationCfg Instance
	{
		get
		{
			if ((Object)(object)instance == (Object)null)
			{
				instance = GameApp.Res.LoadAsset<ButtonClickAnimationCfg>("ButtonClickAnimationCfg", (string)null);
			}
			return instance;
		}
	}
}
