using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Game.Feature.Ad;
using Game.Feature.Gameplay;
using Game.Feature.UI;
using UnityEngine;
using XingYun.Core.Singletons;

namespace Game.Feature.Demo;

public class FeatureTestRunner : MonoBehaviour
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CRunSequentialAsync_003Ed__16 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public CancellationToken ct;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				DemoMatchData data = default(DemoMatchData);
				if (num != 0)
				{
					data = new DemoMatchData
					{
						ClearedCount = 10,
						TotalCount = 10,
						ComboCount = 5,
						ClearedLines = 3
					};
				}
				try
				{
					Awaiter val2;
					if (num != 0)
					{
						UniTask val = Singleton<GameplayFeatureManager>.Instance.OnMatchMapSequentialAsync(data, ct);
						val2 = ((UniTask)(ref val)).GetAwaiter();
						if (!((Awaiter)(ref val2)).IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val2;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRunSequentialAsync_003Ed__16>(ref val2, ref this);
							return;
						}
					}
					else
					{
						val2 = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter);
						num = (_003C_003E1__state = -1);
					}
					((Awaiter)(ref val2)).GetResult();
				}
				catch (OperationCanceledException)
				{
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	private Vector2 scrollPos;

	private GUIStyle headerStyle;

	private GUIStyle btnStyle;

	private GUIStyle toggleBtnStyle;

	private bool stylesInited;

	private CancellationTokenSource sequentialCts;

	private bool showPanel;

	private bool isQuitting;

	private const float BtnWidth = 500f;

	private const float BtnHeight = 80f;

	private const float Gap = 8f;

	private const float SectionGap = 24f;

	private const float ToggleBtnWidth = 200f;

	private const float ToggleBtnHeight = 70f;

	private void InitStyles()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		if (!stylesInited)
		{
			stylesInited = true;
			GUIStyle val = new GUIStyle(GUI.skin.label)
			{
				fontSize = 36,
				fontStyle = (FontStyle)1
			};
			val.normal.textColor = Color.yellow;
			headerStyle = val;
			btnStyle = new GUIStyle(GUI.skin.button)
			{
				fontSize = 30
			};
			toggleBtnStyle = new GUIStyle(GUI.skin.button)
			{
				fontSize = 28,
				fontStyle = (FontStyle)1
			};
		}
	}

	private void OnGUI()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0368: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_045d: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_051b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0555: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0644: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d5: Unknown result type (might be due to invalid IL or missing references)
		if (isQuitting)
		{
			return;
		}
		InitStyles();
		if (GUI.Button(new Rect((float)Screen.width - 200f - 10f, 10f, 200f, 70f), showPanel ? "关闭面板" : "Feature", toggleBtnStyle))
		{
			showPanel = !showPanel;
		}
		if (showPanel)
		{
			float num = 540f;
			float num2 = (float)Screen.width - num - 10f;
			float num3 = 2400f;
			scrollPos = GUI.BeginScrollView(new Rect(num2, 90f, num, (float)Screen.height - 70f - 30f), scrollPos, new Rect(0f, 0f, 510f, num3));
			float num4 = 10f;
			GUI.Label(new Rect(10f, num4, 500f, 40f), "=== Gameplay ===", headerStyle);
			num4 += 45f;
			if (GUI.Button(new Rect(10f, num4, 500f, 80f), "关卡开始 (FireAll)", btnStyle))
			{
				DemoLevelData demoLevelData = new DemoLevelData();
				demoLevelData.Level = 1;
				demoLevelData.IsDailyChallenge = false;
				demoLevelData.MapData = new int[3, 3]
				{
					{ 1, 0, 1 },
					{ 0, 1, 0 },
					{ 1, 1, 1 }
				};
				DemoLevelData data = demoLevelData;
				Singleton<GameplayFeatureManager>.Instance.OnLevelStart(data);
			}
			num4 += 88f;
			if (GUI.Button(new Rect(10f, num4, 500f, 80f), "消除成功 (FireAll)", btnStyle))
			{
				DemoMatchData data2 = new DemoMatchData
				{
					ClearedCount = 10,
					TotalCount = 10,
					ComboCount = 5,
					ClearedLines = 3
				};
				Singleton<GameplayFeatureManager>.Instance.OnMatchMap(data2);
			}
			num4 += 88f;
			if (GUI.Button(new Rect(10f, num4, 500f, 80f), "消除排队 (Sequential 协程)", btnStyle))
			{
				DemoMatchData data3 = new DemoMatchData
				{
					ClearedCount = 10,
					TotalCount = 10,
					ComboCount = 5,
					ClearedLines = 3
				};
				((MonoBehaviour)this).StartCoroutine(Singleton<GameplayFeatureManager>.Instance.OnMatchMapSequential(data3));
			}
			num4 += 88f;
			if (GUI.Button(new Rect(10f, num4, 500f, 80f), "消除排队 (Async UniTask)", btnStyle))
			{
				sequentialCts?.Cancel();
				sequentialCts = new CancellationTokenSource();
				UniTaskVoid val = RunSequentialAsync(sequentialCts.Token);
				((UniTaskVoid)(ref val)).Forget();
			}
			num4 += 88f;
			if (GUI.Button(new Rect(10f, num4, 500f, 80f), "取消 Async Sequential", btnStyle) && sequentialCts != null)
			{
				sequentialCts.Cancel();
			}
			num4 += 88f;
			if (GUI.Button(new Rect(10f, num4, 500f, 80f), "万能牌验证 Id=1001 (FireFirst)", btnStyle))
			{
				DemoCard card = new DemoCard
				{
					Id = 1001,
					Suit = "hearts",
					Value = 5
				};
				DemoSlotColumn column = new DemoSlotColumn
				{
					RequiredSuit = "spades",
					RequiredValue = 3
				};
				Singleton<GameplayFeatureManager>.Instance.OnValidatePlacement(card, column);
			}
			num4 += 88f;
			if (GUI.Button(new Rect(10f, num4, 500f, 80f), "普通牌验证 Id=999 (FireFirst)", btnStyle))
			{
				DemoCard card2 = new DemoCard
				{
					Id = 999,
					Suit = "hearts",
					Value = 5
				};
				DemoSlotColumn column2 = new DemoSlotColumn
				{
					RequiredSuit = "spades",
					RequiredValue = 3
				};
				Singleton<GameplayFeatureManager>.Instance.OnValidatePlacement(card2, column2);
			}
			num4 += 88f;
			if (GUI.Button(new Rect(10f, num4, 500f, 80f), "万能牌放置 (FireAll)", btnStyle))
			{
				DemoCard card3 = new DemoCard
				{
					Id = 1001,
					Suit = "hearts",
					Value = 5
				};
				DemoSlotColumn column3 = new DemoSlotColumn
				{
					RequiredSuit = "hearts",
					RequiredValue = 5
				};
				Singleton<GameplayFeatureManager>.Instance.OnCardPlaced(card3, column3);
			}
			num4 += 88f;
			if (GUI.Button(new Rect(10f, num4, 500f, 80f), "Tile 放置 (FireAll)", btnStyle))
			{
				DemoTilePutInfo info = new DemoTilePutInfo
				{
					TileId = 42,
					X = 3,
					Y = 5
				};
				Singleton<GameplayFeatureManager>.Instance.OnTilePut(info);
			}
			num4 += 88f;
			if (GUI.Button(new Rect(10f, num4, 500f, 80f), "自定义优先级结算 (GetHooks)", btnStyle))
			{
				Singleton<GameplayFeatureManager>.Instance.OnSettleCustomOrder();
			}
			num4 += 104f;
			GUI.Label(new Rect(10f, num4, 500f, 40f), "=== Ad ===", headerStyle);
			num4 += 45f;
			if (GUI.Button(new Rect(10f, num4, 500f, 80f), "关卡结束插屏 (FireFirst)", btnStyle))
			{
				Singleton<AdFeatureManager>.Instance.OnLevelEnd();
			}
			num4 += 88f;
			if (GUI.Button(new Rect(10f, num4, 500f, 80f), "激励视频请求 (FireFirst)", btnStyle))
			{
				Singleton<AdFeatureManager>.Instance.OnRewardVideoRequest("hint", delegate
				{
				});
			}
			num4 += 104f;
			GUI.Label(new Rect(10f, num4, 500f, 40f), "=== UI ===", headerStyle);
			num4 += 45f;
			if (GUI.Button(new Rect(10f, num4, 500f, 80f), "结算显示 Rank=3 Streak=5 (FireAll)", btnStyle))
			{
				DemoGameResultData data4 = new DemoGameResultData
				{
					Score = 1000,
					Rank = 3,
					WinStreak = 5
				};
				DemoResultDisplayConfig display = new DemoResultDisplayConfig();
				Singleton<UIFeatureManager>.Instance.OnResultShow(data4, display);
			}
			num4 += 88f;
			if (GUI.Button(new Rect(10f, num4, 500f, 80f), "结算显示 Rank=8 Streak=1 (FireAll)", btnStyle))
			{
				DemoGameResultData data5 = new DemoGameResultData
				{
					Score = 500,
					Rank = 8,
					WinStreak = 1
				};
				DemoResultDisplayConfig display2 = new DemoResultDisplayConfig();
				Singleton<UIFeatureManager>.Instance.OnResultShow(data5, display2);
			}
			num4 += 104f;
			GUI.Label(new Rect(10f, num4, 500f, 40f), "=== System ===", headerStyle);
			num4 += 45f;
			if (GUI.Button(new Rect(10f, num4, 500f, 80f), "Refresh 全部领域", btnStyle))
			{
				FeatureBootstrap.Refresh();
			}
			GUI.EndScrollView();
		}
	}

	[AsyncStateMachine(typeof(_003CRunSequentialAsync_003Ed__16))]
	private UniTaskVoid RunSequentialAsync(CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CRunSequentialAsync_003Ed__16 _003CRunSequentialAsync_003Ed__ = default(_003CRunSequentialAsync_003Ed__16);
		_003CRunSequentialAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CRunSequentialAsync_003Ed__.ct = ct;
		_003CRunSequentialAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CRunSequentialAsync_003Ed__._003C_003Et__builder)).Start<_003CRunSequentialAsync_003Ed__16>(ref _003CRunSequentialAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CRunSequentialAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private void OnApplicationQuit()
	{
		isQuitting = true;
	}

	private void OnDestroy()
	{
		if (!isQuitting)
		{
			sequentialCts?.Cancel();
			sequentialCts?.Dispose();
		}
	}
}
