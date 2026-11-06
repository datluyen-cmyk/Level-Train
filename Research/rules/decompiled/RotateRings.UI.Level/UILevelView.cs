using System;
using Game.UI;
using Hot.Model;
using RotateRings.Model;
using RotateRings.SpecialLevel;
using TMPro;
using UnityEngine;
using XingYun.Extensions;
using XingYun.UI.Core;

namespace RotateRings.UI.Level;

public class UILevelView : GameWidgetView
{
	private const string LevelLabelKey = "MainGame_Level";

	private const string EndlessLevelLabelKey = "Level_Round";

	public RectTransform Node;

	public TMP_Text TxtLevel;

	public TMP_Text TxtLevelNum;

	public void SetLevel(int level)
	{
		if ((Object)(object)Node != (Object)null)
		{
			((Component)Node).gameObject.SetActive(!IsSpecialLevelMode());
		}
		if ((Object)(object)TxtLevelNum != (Object)null)
		{
			TMPTextExtensions.SetLocalizedTextValue(TxtLevelNum, level.ToString());
		}
		RefreshLevelLabel();
	}

	public void RefreshLevelLabel()
	{
		if (!((Object)(object)TxtLevel == (Object)null))
		{
			bool flag = GameInfo.CurrentGameMode == GameMode.Endless;
			TMPTextExtensions.SetLocalizedText(TxtLevel, flag ? "Level_Round" : "MainGame_Level", Array.Empty<object>());
		}
	}

	private static bool IsSpecialLevelMode()
	{
		return SpecialLevelModule.IsSpecialLevelMode();
	}

	public override void Dispose()
	{
		base.Dispose();
	}

	public override void Inject(UIBehaviour behaviour)
	{
		base.Inject(behaviour);
		Node = behaviour.Get<RectTransform>("Node");
		TxtLevel = behaviour.Get<TMP_Text>("TxtLevel");
		TxtLevelNum = behaviour.Get<TMP_Text>("TxtLevelNum");
	}
}
