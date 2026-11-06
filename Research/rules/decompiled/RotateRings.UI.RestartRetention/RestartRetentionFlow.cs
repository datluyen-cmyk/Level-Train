using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Hot.Model;
using RotateRings.Model;
using RotateRings.RestartRetention;
using RotateRings.UI.Setting;
using Rotaterings.Rank;
using UniLog;
using UnityEngine;
using XingYun.UI.Controller;
using XingYun.UI.Core;
using XingYun.Utility;

namespace RotateRings.UI.RestartRetention;

public static class RestartRetentionFlow
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003COpenRequestAsync_003Ed__4 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public RestartRetentionRequest request;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				try
				{
					Awaiter val2;
					if (num != 0)
					{
						UniTask val = GameApp.UI.OpenAsync("UIRestartRetention", (UILayer)2, (object)request, (Action)null);
						val2 = ((UniTask)(ref val)).GetAwaiter();
						if (!((Awaiter)(ref val2)).IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val2;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003COpenRequestAsync_003Ed__4>(ref val2, ref this);
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
					if (!request.Handled)
					{
						IController val3 = default(IController);
						if (!request.Registered || !GameApp.UI.IsOpen("UIRestartRetention", ref val3) || !(val3 is UIRestartRetentionController uIRestartRetentionController) || !uIRestartRetentionController.OwnsRequest(request))
						{
							Log.LogError("RestartRetention", "弹窗资源打开失败，保留当前关卡与设置页");
							Release(request);
						}
						else if (!request.Preview && !IsCurrent(request))
						{
							Cancel(request);
						}
					}
				}
				catch (Exception ex)
				{
					Log.LogError("RestartRetention", "打开挽留失败：{0}", new object[1] { ex.Message });
					try
					{
						IController val4 = default(IController);
						if (GameApp.UI.IsOpen("UIRestartRetention", ref val4) && val4 is UIRestartRetentionController uIRestartRetentionController2 && uIRestartRetentionController2.OwnsRequest(request))
						{
							GameApp.UI.CloseView("UIRestartRetention", (object)null, false);
						}
					}
					finally
					{
						if (num < 0)
						{
							Release(request);
						}
					}
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	private static RestartRetentionRequest pending;

	public static void ResetForSession()
	{
		pending = null;
	}

	public static void ReplayCurrentLevelExtend(string settingsLocation)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		if (pending != null || GameApp.UI.IsOpen("UIRestartRetention"))
		{
			return;
		}
		IGameplayManager active = Gameplay.Active;
		if (active == null || active.IsLevelCompleted)
		{
			IController controller = default(IController);
			if (GameApp.UI.IsOpen(settingsLocation, ref controller))
			{
				CloseSettings(settingsLocation, controller, null, null);
			}
			return;
		}
		RestartRetentionRequest restartRetentionRequest = CreateRequest(settingsLocation, preview: false);
		if (restartRetentionRequest.Decision.CanRestart)
		{
			pending = restartRetentionRequest;
			if (restartRetentionRequest.Decision.ShowPopup)
			{
				UniTaskExtensions.Forget(OpenRequestAsync(restartRetentionRequest));
			}
			else
			{
				Confirm(restartRetentionRequest);
			}
		}
	}

	public static RestartRetentionRequest CreateRequest(string settingsLocation, bool preview)
	{
		IGameplayManager active = Gameplay.Active;
		LevelModelProxy levelModelProxy = GameApp.Proxy.Get<LevelModelProxy>();
		RankModelData rankModelData = GameApp.Proxy.Get<RankModelProxy>()?.Data;
		GameObject val = active?.CurrentLevelObject;
		RestartRetentionContext context = new RestartRetentionContext
		{
			Streak = Math.Max(0, rankModelData?.MultiplierTier ?? 0),
			RankActive = (preview || (rankModelData != null && rankModelData.Period > 0 && rankModelData.EndTimeStamp > Time.GetCurrentTimestamp())),
			CanRestart = (active != null && levelModelProxy != null && (Object)(object)val != (Object)null && !active.IsLoadingLevel && !active.IsLevelCompleted),
			SupportedMode = (GameInfo.CurrentGameMode == GameMode.Main),
			ForcePreview = preview
		};
		IController settingsController = null;
		if (!string.IsNullOrEmpty(settingsLocation))
		{
			GameApp.UI.IsOpen(settingsLocation, ref settingsController);
		}
		return new RestartRetentionRequest
		{
			Manager = active,
			LevelObject = val,
			GameMode = GameInfo.CurrentGameMode,
			Level = (levelModelProxy?.Level ?? 0),
			Attempt = (levelModelProxy?.TryLevelNum ?? 0),
			SettingsLocation = settingsLocation,
			SettingsController = settingsController,
			Decision = RestartRetentionRules.Evaluate(context),
			Preview = preview
		};
	}

	[AsyncStateMachine(typeof(_003COpenRequestAsync_003Ed__4))]
	private static UniTask OpenRequestAsync(RestartRetentionRequest request)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003COpenRequestAsync_003Ed__4 _003COpenRequestAsync_003Ed__ = default(_003COpenRequestAsync_003Ed__4);
		_003COpenRequestAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003COpenRequestAsync_003Ed__.request = request;
		_003COpenRequestAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003COpenRequestAsync_003Ed__._003C_003Et__builder)).Start<_003COpenRequestAsync_003Ed__4>(ref _003COpenRequestAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003COpenRequestAsync_003Ed__._003C_003Et__builder)).Task;
	}

	public static void Confirm(RestartRetentionRequest request)
	{
		if (request == null || request.Handled || request.Closing)
		{
			return;
		}
		request.Closing = true;
		bool closeSettings = !request.Preview && request.Decision.CanRestart && IsCurrent(request);
		CloseViewsTogether(request, closeSettings, delegate
		{
			if (!request.Handled)
			{
				request.Handled = true;
				if (request.Registered)
				{
					GameApp.UI.CloseView("UIRestartRetention", (object)null, false);
				}
				bool num = !request.Preview && request.Decision.CanRestart && IsCurrent(request);
				Release(request);
				if (num)
				{
					request.Manager.ReplayCurrentLevel();
				}
			}
		});
	}

	public static void Cancel(RestartRetentionRequest request)
	{
		if (request == null || request.Handled || request.Closing)
		{
			return;
		}
		request.Closing = true;
		bool closeSettings = !request.Preview && IsCurrent(request) && !request.Decision.ReturnToSettings;
		CloseViewsTogether(request, closeSettings, delegate
		{
			if (!request.Handled)
			{
				request.Handled = true;
				if (request.Registered)
				{
					GameApp.UI.CloseView("UIRestartRetention", (object)null, false);
				}
				Release(request);
			}
		});
	}

	private static void CloseViewsTogether(RestartRetentionRequest request, bool closeSettings, Action onComplete)
	{
		bool popupCompleted = false;
		bool settingsCompleted = !closeSettings;
		Action completeWhenReady = delegate
		{
			if (!request.Handled && popupCompleted && settingsCompleted)
			{
				onComplete?.Invoke();
			}
		};
		ClosePopupWithAnimation(request, delegate
		{
			if (!popupCompleted)
			{
				popupCompleted = true;
				completeWhenReady();
			}
		});
		if (request.Handled || !closeSettings)
		{
			return;
		}
		CloseOriginalSettings(request, delegate
		{
			if (!settingsCompleted)
			{
				settingsCompleted = true;
				completeWhenReady();
			}
		}, delegate
		{
			Release(request);
			IController val = default(IController);
			if (GameApp.UI.IsOpen("UIRestartRetention", ref val) && val is UIRestartRetentionController uIRestartRetentionController && uIRestartRetentionController.OwnsRequest(request))
			{
				GameApp.UI.CloseView("UIRestartRetention", (object)null, false);
			}
		});
	}

	public static void Release(RestartRetentionRequest request)
	{
		if (request != null)
		{
			request.Handled = true;
			if (pending == request)
			{
				pending = null;
			}
		}
	}

	private static bool IsCurrent(RestartRetentionRequest request)
	{
		LevelModelProxy levelModelProxy = GameApp.Proxy.Get<LevelModelProxy>();
		IGameplayManager active = Gameplay.Active;
		if (active != null && active == request.Manager && (Object)(object)request.LevelObject != (Object)null && (Object)(object)active.CurrentLevelObject == (Object)(object)request.LevelObject && !active.IsLoadingLevel && !active.IsLevelCompleted && GameInfo.CurrentGameMode == request.GameMode && levelModelProxy != null && levelModelProxy.Level == request.Level)
		{
			return levelModelProxy.TryLevelNum == request.Attempt;
		}
		return false;
	}

	private static void ClosePopupWithAnimation(RestartRetentionRequest request, Action onComplete)
	{
		IController val = default(IController);
		if (!request.Registered)
		{
			onComplete?.Invoke();
		}
		else if (GameApp.UI.IsOpen("UIRestartRetention", ref val) && val is UIRestartRetentionController uIRestartRetentionController)
		{
			uIRestartRetentionController.PlayCloseAnimation(request, onComplete);
		}
		else
		{
			Release(request);
		}
	}

	private static void CloseOriginalSettings(RestartRetentionRequest request, Action onComplete, Action onInterrupted)
	{
		IController val = default(IController);
		if (!string.IsNullOrEmpty(request.SettingsLocation) && GameApp.UI.IsOpen(request.SettingsLocation, ref val) && val == request.SettingsController)
		{
			CloseSettings(request.SettingsLocation, val, onComplete, onInterrupted);
		}
		else
		{
			onComplete?.Invoke();
		}
	}

	private static void CloseSettings(string location, IController controller, Action onComplete, Action onInterrupted)
	{
		if (controller is UISettingInGameV2Controller uISettingInGameV2Controller)
		{
			uISettingInGameV2Controller.CloseWithAnimation(onComplete, onInterrupted);
			return;
		}
		GameApp.UI.CloseView(location, (object)null, false);
		onComplete?.Invoke();
	}
}
