using System;
using Example.Cfg;
using Hot.Model;
using RotateRing.GamePlay;
using RotateRings.Model;
using UniLog;
using XingYun.UI.Core;

namespace RotateRings.UI.NewElementIntro;

public static class NewElementIntroService
{
	public const string UiPath = "UINewElementIntro";

	private const string Tag = "NewElementIntro";

	private static bool popupActive;

	private static bool replacingPopup;

	private static Action afterPopupClosed;

	public static bool TryOpenForLevel(int level)
	{
		if (GameInfo.CurrentGameMode != GameMode.Main || level <= 0)
		{
			return false;
		}
		NewElementConfig newElementConfig = FindFirstConfigForLevel(level);
		if (newElementConfig == null)
		{
			return false;
		}
		NewElementIntroProxy newElementIntroProxy = GameApp.Proxy.Get<NewElementIntroProxy>();
		if (newElementIntroProxy == null || newElementIntroProxy.HasIntroduced(newElementConfig.id))
		{
			return false;
		}
		return OpenConfig(newElementConfig, level, forcedByGm: false);
	}

	public static bool ForceOpenById(int elementId)
	{
		NewElementConfig newElementConfig = FindConfigById(elementId);
		if (newElementConfig == null)
		{
			return false;
		}
		int level = GameApp.Proxy.Get<LevelModelProxy>()?.Level ?? 0;
		if (IsPopupActive())
		{
			replacingPopup = true;
			try
			{
				GameApp.UI.CloseView("UINewElementIntro", (object)null, false);
			}
			finally
			{
				replacingPopup = false;
			}
		}
		return OpenConfig(newElementConfig, level, forcedByGm: true);
	}

	public static void RunAfterCurrentPopup(Action action)
	{
		if (action != null)
		{
			if (!IsPopupActive())
			{
				action();
			}
			else
			{
				afterPopupClosed = (Action)Delegate.Combine(afterPopupClosed, action);
			}
		}
	}

	public static void NotifyPopupRemoved()
	{
		popupActive = false;
		if (!replacingPopup)
		{
			InvokeAfterPopupClosed();
		}
	}

	private static bool OpenConfig(NewElementConfig config, int level, bool forcedByGm)
	{
		if (config == null || GameApp.UI == null)
		{
			return false;
		}
		if (IsPopupActive())
		{
			return false;
		}
		NewElementIntroOpenData newElementIntroOpenData = new NewElementIntroOpenData
		{
			ElementId = config.id,
			NameKey = config.Name,
			DescKey = config.Desc,
			Icon = config.Icon
		};
		popupActive = true;
		GameApp.UI.Open("UINewElementIntro", (UILayer)4, (object)newElementIntroOpenData, (Action)null);
		if (!GameApp.UI.IsOpen("UINewElementIntro"))
		{
			popupActive = false;
			Log.LogError("NewElementIntro", "打开新元素介绍失败 id={0}", new object[1] { config.id });
			InvokeAfterPopupClosed();
			return false;
		}
		return true;
	}

	private static bool IsPopupActive()
	{
		if (!popupActive)
		{
			if (GameApp.UI != null)
			{
				return GameApp.UI.IsOpen("UINewElementIntro");
			}
			return false;
		}
		return true;
	}

	private static NewElementConfig FindConfigById(int elementId)
	{
		try
		{
			return GameApp.Config.GetTable<ExampleTables>()?.TbNewElementConfig?.Get(elementId);
		}
		catch (Exception ex)
		{
			Log.LogError("NewElementIntro", "读取新元素配置失败 id={0}, error={1}", new object[2] { elementId, ex.Message });
			return null;
		}
	}

	private static NewElementConfig FindFirstConfigForLevel(int level)
	{
		int num = LevelConfigService.ResolveNewElementIntroId(level);
		if (num <= 0)
		{
			return null;
		}
		return FindConfigById(num);
	}

	private static void InvokeAfterPopupClosed()
	{
		Action action = afterPopupClosed;
		afterPopupClosed = null;
		action?.Invoke();
	}
}
