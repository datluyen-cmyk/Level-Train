using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Example.Cfg;
using Game.Level;
using Hot.Model;
using RotateRings.Common;
using RotateRings.Model;
using UniLog;
using UnityEngine;
using XingYun.Experiment;

namespace RotateRing.GamePlay;

public static class LevelConfigService
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CGetCoreLevelJson_003Ed__31 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<string> _003C_003Et__builder;

		public int level;

		private Awaiter<TextAsset> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			string text;
			try
			{
				Awaiter<TextAsset> val;
				if (num != 0)
				{
					string coreLevelPath = GetCoreLevelPath(level);
					val = GameApp.Res.LoadAssetAsync<TextAsset>(coreLevelPath, (string)null).GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<TextAsset>, _003CGetCoreLevelJson_003Ed__31>(ref val, ref this);
						return;
					}
				}
				else
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<TextAsset>);
					num = (_003C_003E1__state = -1);
				}
				text = val.GetResult().text;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003Et__builder.SetResult(text);
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			_003C_003Et__builder.SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CLoadPhysicLevel_003Ed__30 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<GameObject> _003C_003Et__builder;

		public int level;

		private Awaiter<GameObject> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			GameObject result;
			try
			{
				Awaiter<GameObject> val;
				if (num != 0)
				{
					string physicsLevelPath = GetPhysicsLevelPath(level);
					val = GameApp.Res.LoadAssetAsync<GameObject>(physicsLevelPath, (string)null).GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<GameObject>, _003CLoadPhysicLevel_003Ed__30>(ref val, ref this);
						return;
					}
				}
				else
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<GameObject>);
					num = (_003C_003E1__state = -1);
				}
				result = val.GetResult();
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003Et__builder.SetResult(result);
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			_003C_003Et__builder.SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	private const string TAG = "[GamePlay][LevelConfigService] ";

	private static readonly Dictionary<GameMode, ILevelConfigMode> modesByGameMode = new Dictionary<GameMode, ILevelConfigMode>();

	private static bool isInitialized;

	private static readonly ILevelSelectService[] selectServices = new ILevelSelectService[1]
	{
		new RandomSelectLevelService()
	};

	private static readonly ILevelSelectService fallbackSelectService = new BoardLevelSelectService();

	private const string PHYSICS_LEVEL_PATH_PREFIX = "Level";

	public static int LevelGapTableCount => LevelGapDegExp.LevelGapTableCount;

	public static int MainLevelMax => GetCoreLevelCount();

	public static int CurrentEndlessSeason => 0;

	private static void Register<T>(T mode) where T : class, ILevelConfigMode
	{
		if (mode != null)
		{
			modesByGameMode[mode.Mode] = mode;
		}
	}

	private static T GetMode<T>() where T : class, ILevelConfigMode
	{
		foreach (ILevelConfigMode value in modesByGameMode.Values)
		{
			if (value is T result)
			{
				return result;
			}
		}
		throw new InvalidOperationException("未注册关卡配置模式: " + typeof(T).Name);
	}

	public static void Init()
	{
		if (!isInitialized)
		{
			isInitialized = true;
			MainLevelConfigMode mainLevelConfigMode = new MainLevelConfigMode();
			Register(mainLevelConfigMode);
			Register(new EndlessLevelConfigMode());
			Register(new HalloweenLevelConfigMode(mainLevelConfigMode));
			RandomLevelBootstrap.Init();
		}
	}

	public static int GetCoreLevelCount()
	{
		return GetMode<MainLevelConfigMode>().GetCoreLevelCount();
	}

	public static void SetCoreLevelCount(int count)
	{
		GetMode<MainLevelConfigMode>().SetCoreLevelCount(count);
	}

	public static int GetLevelId(int level, bool isCore = true)
	{
		GameMode currentGameMode = GameInfo.CurrentGameMode;
		int configLevelId = GetLevelConfigMode(currentGameMode).GetConfigLevelId(level);
		ILevelSelectService[] array = selectServices;
		foreach (ILevelSelectService levelSelectService in array)
		{
			if (levelSelectService.IsAvailable(currentGameMode, level))
			{
				return levelSelectService.SelectLevelId(currentGameMode, level, configLevelId, isCore);
			}
		}
		return fallbackSelectService.SelectLevelId(currentGameMode, level, configLevelId, isCore);
	}

	public static bool TryGetLevelIdByCoreLevelPath(string coreLevelPath, out int levelId)
	{
		return GetMode<MainLevelConfigMode>().TryGetLevelIdByCoreLevelPath(coreLevelPath, out levelId);
	}

	public static LevelConfig GetLevelConfig(GameMode gameMode, int levelId)
	{
		return GetLevelConfigMode(gameMode).GetLevelConfig(levelId);
	}

	private static LevelConfig GetLevelConfig(int levelId)
	{
		return GetLevelConfig(GameInfo.CurrentGameMode, levelId);
	}

	public static bool TryGetRandomLevelContent(int level, out int coreLevelPath, out int parts)
	{
		coreLevelPath = -1;
		parts = 0;
		if (GameInfo.CurrentGameMode != GameMode.Main)
		{
			return false;
		}
		return RandomSelectLevelService.TryGetMatchedContent(level, out coreLevelPath, out parts);
	}

	public static int ResolveNewElementIntroId(int level)
	{
		if (level <= 0)
		{
			return 0;
		}
		if (GameInfo.CurrentGameMode == GameMode.Main && level >= 1 && FeatureConfig.LevelRandom.ResolveEnable())
		{
			int num = RandomLevelTutorial.ResolveSpecialPositionElementId(level);
			if (num > 0)
			{
				return num;
			}
			if (RandomSelectLevelService.TryGetMatchedTeachElement(level, out var elementId))
			{
				return elementId;
			}
		}
		return ResolveBottomLineIntroElementId(level);
	}

	private static int ResolveBottomLineIntroElementId(int level)
	{
		try
		{
			List<NewElementConfig> list = GameApp.Config.GetTable<ExampleTables>()?.TbNewElementConfig?.DataList;
			if (list == null)
			{
				return 0;
			}
			string text = GetPhysicsLevelPathKey(level);
			if (!string.IsNullOrEmpty(text) && text.StartsWith("Level", StringComparison.Ordinal))
			{
				text = text.Substring("Level".Length);
			}
			if (!int.TryParse(text, out var result) || result <= 0)
			{
				return 0;
			}
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < list.Count; i++)
			{
				NewElementConfig newElementConfig = list[i];
				if (newElementConfig != null && TryGetIntroTriggerRule(newElementConfig.id, out var contentId, out var unlockLevel) && unlockLevel == level && contentId == result)
				{
					num2++;
					if (num == 0 || newElementConfig.id < num)
					{
						num = newElementConfig.id;
					}
				}
			}
			_ = 1;
			return num;
		}
		catch (Exception ex)
		{
			Log.LogError("[GamePlay][LevelConfigService] ", "按关卡读取新元素配置失败 level={0}, error={1}", new object[2] { level, ex.Message });
			return 0;
		}
	}

	private static bool TryGetIntroTriggerRule(int elementId, out int contentId, out int unlockLevel)
	{
		contentId = 0;
		unlockLevel = 0;
		NewElementIntroFunction newElementIntroFunction = FunctionDataCenter.Get<NewElementIntroFunction>();
		string text;
		switch (elementId)
		{
		case 1:
			text = newElementIntroFunction.BombConfig;
			break;
		case 2:
			text = newElementIntroFunction.ChainConfig;
			break;
		case 3:
			text = newElementIntroFunction.RingPaletteLockConfig;
			break;
		default:
			return false;
		}
		string[] array = text?.Split('|');
		if (array == null || array.Length != 2 || !int.TryParse(array[0], out contentId) || contentId <= 0 || !int.TryParse(array[1], out unlockLevel) || unlockLevel <= 0)
		{
			return false;
		}
		return true;
	}

	private static bool TryResolveLevelIdentity(int level, out int levelId, out int coreLevelPath, out LevelConfig config)
	{
		levelId = GetLevelId(level);
		if (TryGetRandomLevelContent(level, out coreLevelPath, out var _))
		{
			config = null;
			return true;
		}
		coreLevelPath = 0;
		config = GetLevelConfig(levelId);
		return false;
	}

	private static string GetCoreLevelPath(int level)
	{
		LevelConfigModeBase levelConfigMode = GetLevelConfigMode(GameInfo.CurrentGameMode);
		TryResolveLevelIdentity(level, out var levelId, out var coreLevelPath, out var config);
		if (coreLevelPath > 0)
		{
			return string.Format(levelConfigMode.CorePathFmt, coreLevelPath);
		}
		if (config != null && !string.IsNullOrEmpty(config.CoreLevelPath))
		{
			return string.Format(levelConfigMode.CorePathFmt, config.CoreLevelPath);
		}
		return string.Format(levelConfigMode.FallbackPathFmt, levelId);
	}

	public static int GetCoreLevelPathKey(int level)
	{
		TryResolveLevelIdentity(level, out var _, out var coreLevelPath, out var config);
		if (coreLevelPath > 0)
		{
			return coreLevelPath;
		}
		if (config != null && !string.IsNullOrEmpty(config.CoreLevelPath) && int.TryParse(config.CoreLevelPath, out var result))
		{
			return result;
		}
		return 0;
	}

	public static string GetPhysicsLevelPath(int level)
	{
		LevelConfigModeBase levelConfigMode = GetLevelConfigMode(GameInfo.CurrentGameMode);
		TryResolveLevelIdentity(level, out var levelId, out var coreLevelPath, out var config);
		if (coreLevelPath > 0)
		{
			return string.Format(levelConfigMode.PhysicsPathFmt, "Level" + coreLevelPath);
		}
		if (config != null && !string.IsNullOrEmpty(config.PhysicsLevelPath))
		{
			return string.Format(levelConfigMode.PhysicsPathFmt, config.PhysicsLevelPath);
		}
		Log.LogError("[GamePlay][LevelConfigService] ", "配置表未找到关卡 {0}，回退硬编码路径", new object[1] { levelId });
		return string.Format(levelConfigMode.FallbackPathFmt, levelId);
	}

	public static string GetPhysicsLevelPathKey(int level)
	{
		TryResolveLevelIdentity(level, out var _, out var coreLevelPath, out var config);
		if (coreLevelPath > 0)
		{
			return "Level" + coreLevelPath;
		}
		if (!string.IsNullOrEmpty(config?.PhysicsLevelPath))
		{
			return config.PhysicsLevelPath;
		}
		return null;
	}

	[AsyncStateMachine(typeof(_003CLoadPhysicLevel_003Ed__30))]
	public static UniTask<GameObject> LoadPhysicLevel(int level)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CLoadPhysicLevel_003Ed__30 _003CLoadPhysicLevel_003Ed__ = default(_003CLoadPhysicLevel_003Ed__30);
		_003CLoadPhysicLevel_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<GameObject>.Create();
		_003CLoadPhysicLevel_003Ed__.level = level;
		_003CLoadPhysicLevel_003Ed__._003C_003E1__state = -1;
		_003CLoadPhysicLevel_003Ed__._003C_003Et__builder.Start<_003CLoadPhysicLevel_003Ed__30>(ref _003CLoadPhysicLevel_003Ed__);
		return _003CLoadPhysicLevel_003Ed__._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CGetCoreLevelJson_003Ed__31))]
	public static UniTask<string> GetCoreLevelJson(int level)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CGetCoreLevelJson_003Ed__31 _003CGetCoreLevelJson_003Ed__ = default(_003CGetCoreLevelJson_003Ed__31);
		_003CGetCoreLevelJson_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<string>.Create();
		_003CGetCoreLevelJson_003Ed__.level = level;
		_003CGetCoreLevelJson_003Ed__._003C_003E1__state = -1;
		_003CGetCoreLevelJson_003Ed__._003C_003Et__builder.Start<_003CGetCoreLevelJson_003Ed__31>(ref _003CGetCoreLevelJson_003Ed__);
		return _003CGetCoreLevelJson_003Ed__._003C_003Et__builder.Task;
	}

	private static LevelConfigModeBase GetLevelConfigMode(GameMode gameMode)
	{
		if (modesByGameMode.TryGetValue(gameMode, out var value) && value is LevelConfigModeBase result)
		{
			return result;
		}
		return GetMode<MainLevelConfigMode>();
	}
}
