using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Game.DataTrack;
using RotateRings.GamePlay.Events;
using RotateRings.Model;
using RotateRings.UI.WeakGuide;
using RotateRings.WeakGuide;
using RotateRings.WeakGuide.StepHandles;
using UnityEngine;
using XingYun.Core.Singletons;
using XingYun.Event;
using XingYun.UI.Controller;
using XingYun.UI.Core;

public class WeakGuideManager : MonoBehaviour
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CInstallForLevelAsync_003Ed__20 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<WeakGuideManager> _003C_003Et__builder;

		public GameplayManager gameplayManager;

		public CancellationToken ct;

		public IWeakGuideSetup setup;

		private GameObject _003ClevelRoot_003E5__2;

		private GameLevelInfo _003ClevelInfo_003E5__3;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			WeakGuideManager result;
			try
			{
				if (num == 0)
				{
					goto IL_009d;
				}
				WeakGuideInstallContext context = default(WeakGuideInstallContext);
				if ((Object)(object)gameplayManager == (Object)null || ct.IsCancellationRequested)
				{
					result = null;
				}
				else if (setup == null)
				{
					result = null;
				}
				else
				{
					_003ClevelRoot_003E5__2 = gameplayManager.CurrentLevelObject;
					if (!((Object)(object)_003ClevelRoot_003E5__2 == (Object)null) && !ct.IsCancellationRequested && _003ClevelRoot_003E5__2.TryGetComponent<GameLevelInfo>(ref _003ClevelInfo_003E5__3))
					{
						context = new WeakGuideInstallContext(_003ClevelRoot_003E5__2, _003ClevelInfo_003E5__3, gameplayManager);
						goto IL_009d;
					}
					result = null;
				}
				goto end_IL_0007;
				IL_009d:
				try
				{
					Awaiter val2;
					if (num != 0)
					{
						UniTask val = setup.InstallAsync(context, ct);
						val2 = ((UniTask)(ref val)).GetAwaiter();
						if (!((Awaiter)(ref val2)).IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val2;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CInstallForLevelAsync_003Ed__20>(ref val2, ref this);
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
				finally
				{
					if (num < 0 && (Object)(object)_003ClevelInfo_003E5__3 != (Object)null && (Object)(object)_003ClevelInfo_003E5__3.weakGuideManager != (Object)null)
					{
						_003ClevelInfo_003E5__3.weakGuideManager.isReadyForGesture = true;
					}
				}
				if (ct.IsCancellationRequested || (Object)(object)_003ClevelInfo_003E5__3 == (Object)null || (Object)(object)_003ClevelRoot_003E5__2 == (Object)null || (Object)(object)gameplayManager == (Object)null || (Object)(object)gameplayManager.CurrentLevelObject != (Object)(object)_003ClevelRoot_003E5__2)
				{
					CleanupInstalledManager(_003ClevelInfo_003E5__3);
					result = null;
				}
				else
				{
					WeakGuideManager weakGuideManager = _003ClevelInfo_003E5__3.weakGuideManager;
					result = ((!((Object)(object)weakGuideManager == (Object)null)) ? weakGuideManager : null);
				}
				end_IL_0007:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003ClevelRoot_003E5__2 = null;
				_003ClevelInfo_003E5__3 = null;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003ClevelRoot_003E5__2 = null;
			_003ClevelInfo_003E5__3 = null;
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

	private const string TAG = "[WeakGuide][Manager] ";

	private const float GuideIdleReshowDelaySeconds = 3f;

	[SerializeField]
	private List<GuideStep> guideSteps;

	[SerializeField]
	private Material highlightRingMaterial;

	private int currentIndex;

	private WeakGuideState state;

	private bool isReadyForGesture;

	private int reshowGuideTimerId = -1;

	private readonly Dictionary<string, int> stepNameToIndex = new Dictionary<string, int>();

	public Material HighlightRingMaterial => highlightRingMaterial;

	public WeakGuideState State => state;

	public int CurrentIndex => currentIndex;

	public bool IsCompleted => state == WeakGuideState.Completed;

	public bool IsRunning
	{
		get
		{
			if (state != WeakGuideState.WaitingForGesture)
			{
				return state == WeakGuideState.WaitingForRingSolved;
			}
			return true;
		}
	}

	public void ApplyHighlightRingCloneScale(GameObject clone, GameObject source)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)clone == (Object)null) && !((Object)(object)source == (Object)null))
		{
			Transform transform = clone.transform;
			Transform transform2 = source.transform;
			_ = transform.localScale;
			_ = transform2.localScale;
			_ = transform.lossyScale;
			Vector3 lossyScale = transform2.lossyScale;
			_ = transform.position;
			_ = transform2.position;
			if ((Object)(object)transform.parent != (Object)null)
			{
				_ = ((Object)transform.parent).name;
			}
			Camera gameplayCamera = GameplayCameraManager.GameplayCamera;
			if ((Object)(object)gameplayCamera != (Object)null)
			{
				_ = gameplayCamera.orthographicSize;
			}
			_ = GameUIScaleByCameraOrtho.DefaultReferenceOrthoSize;
			GameUIScaleByCameraOrtho.GetScaleFactor(GameUIScaleByCameraOrtho.BiggerItemScaleFactor > 1f);
			transform.localScale = Vector3.one;
			Vector3 localScale = transform.localScale;
			Vector3 lossyScale2 = transform.lossyScale;
			Vector3.Distance(localScale, Vector3.one);
			_ = 1E-05f;
			Vector3.Distance(lossyScale2, lossyScale);
			_ = 0.001f;
		}
	}

	[AsyncStateMachine(typeof(_003CInstallForLevelAsync_003Ed__20))]
	public static UniTask<WeakGuideManager> InstallForLevelAsync(GameplayManager gameplayManager, int currentLevel, CancellationToken ct, IWeakGuideSetup setup = null)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CInstallForLevelAsync_003Ed__20 _003CInstallForLevelAsync_003Ed__ = default(_003CInstallForLevelAsync_003Ed__20);
		_003CInstallForLevelAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<WeakGuideManager>.Create();
		_003CInstallForLevelAsync_003Ed__.gameplayManager = gameplayManager;
		_003CInstallForLevelAsync_003Ed__.ct = ct;
		_003CInstallForLevelAsync_003Ed__.setup = setup;
		_003CInstallForLevelAsync_003Ed__._003C_003E1__state = -1;
		_003CInstallForLevelAsync_003Ed__._003C_003Et__builder.Start<_003CInstallForLevelAsync_003Ed__20>(ref _003CInstallForLevelAsync_003Ed__);
		return _003CInstallForLevelAsync_003Ed__._003C_003Et__builder.Task;
	}

	private static void CleanupInstalledManager(GameLevelInfo levelInfo)
	{
		if (!((Object)(object)levelInfo == (Object)null) && !((Object)(object)levelInfo.weakGuideManager == (Object)null))
		{
			WeakGuideManager weakGuideManager = levelInfo.weakGuideManager;
			levelInfo.weakGuideManager = null;
			Object.Destroy((Object)(object)((Component)weakGuideManager).gameObject);
		}
	}

	public static bool HasWeakGuideSetupForLevel(int level)
	{
		return level > 0;
	}

	public void SetHighlightRingMaterial(Material material)
	{
		highlightRingMaterial = material;
	}

	private void Awake()
	{
		if (guideSteps != null)
		{
			for (int i = 0; i < guideSteps.Count; i++)
			{
				guideSteps[i]?.StopSequence();
			}
		}
	}

	private void OnEnable()
	{
		Singleton<EventManager>.Instance.Listen<GameplayGestureEvent>((Action<GameplayGestureEvent>)OnGestureEvent);
		Singleton<EventManager>.Instance.Listen<RingSolvedEvent>((Action<RingSolvedEvent>)OnRingSolvedEvent);
		Singleton<EventManager>.Instance.Listen<GameplayPointerReleasedEvent>((Action<GameplayPointerReleasedEvent>)OnPlayerReleasedEvent);
	}

	private void OnDisable()
	{
		CancelGuideSequence();
		Singleton<EventManager>.Instance.Unlisten<GameplayGestureEvent>((Action<GameplayGestureEvent>)OnGestureEvent);
		Singleton<EventManager>.Instance.Unlisten<RingSolvedEvent>((Action<RingSolvedEvent>)OnRingSolvedEvent);
		Singleton<EventManager>.Instance.Unlisten<GameplayPointerReleasedEvent>((Action<GameplayPointerReleasedEvent>)OnPlayerReleasedEvent);
		KillReshowTimer();
	}

	private void OnDestroy()
	{
		CancelGuideSequence();
		KillReshowTimer();
		if (GameApp.UI.IsOpen("WeakGuideView"))
		{
			GameApp.UI.CloseView("WeakGuideView", (object)null, false);
		}
	}

	public void InjectSteps(List<GuideStep> steps)
	{
		if (steps != null && state == WeakGuideState.Idle)
		{
			guideSteps = new List<GuideStep>(steps);
			for (int i = 0; i < guideSteps.Count; i++)
			{
				guideSteps[i]?.StopSequence();
			}
			currentIndex = 0;
			BuildStepNameIndex();
		}
	}

	public void InjectMotion(int stepIndex, GuidMotion motion)
	{
		if (!((Object)(object)motion == (Object)null) && state == WeakGuideState.Idle && guideSteps != null && stepIndex >= 0 && stepIndex < guideSteps.Count)
		{
			guideSteps[stepIndex]?.Append(motion);
		}
	}

	public void ReplaceStep(int stepIndex, GuideStep newStep)
	{
		if (newStep != null && state == WeakGuideState.Idle && guideSteps != null && stepIndex >= 0 && stepIndex < guideSteps.Count)
		{
			guideSteps[stepIndex] = newStep;
		}
	}

	public GuideStep GetGuideStep(int stepIndex)
	{
		return GetStep(stepIndex);
	}

	public void StartGuideSequence(int stepIndex)
	{
		GuideStep step = GetStep(stepIndex);
		if (step != null && step.HasAnyMotion)
		{
			step.StartSequence();
		}
	}

	public bool StartGuideByName(string stepName)
	{
		if (string.IsNullOrEmpty(stepName))
		{
			return false;
		}
		if (!stepNameToIndex.TryGetValue(stepName, out var value))
		{
			return false;
		}
		GuideStep step = GetStep(value);
		if (step == null || !step.HasAnyMotion)
		{
			return false;
		}
		currentIndex = value;
		StartGuideSequence(value);
		return true;
	}

	private void BuildStepNameIndex()
	{
		stepNameToIndex.Clear();
		if (guideSteps == null)
		{
			return;
		}
		for (int i = 0; i < guideSteps.Count; i++)
		{
			string text = guideSteps[i]?.StepName;
			if (!string.IsNullOrEmpty(text) && !stepNameToIndex.TryGetValue(text, out var _))
			{
				stepNameToIndex[text] = i;
			}
		}
	}

	public void CancelGuideSequence()
	{
		GetCurrentStep()?.StopSequence();
	}

	public void StartGuide()
	{
		if (state != WeakGuideState.Idle)
		{
			KillReshowTimer();
		}
		else if (guideSteps != null && guideSteps.Count != 0)
		{
			if (!GameApp.UI.IsOpen("WeakGuideView"))
			{
				GameApp.UI.Open("WeakGuideView", (UILayer)2, (object)null, (Action)null);
			}
			Singleton<EventManager>.Instance.Send<WeakGuideStartedEvent>(new WeakGuideStartedEvent());
			StartCurrentStep();
		}
	}

	public void OnGesture()
	{
		if (!isReadyForGesture)
		{
			return;
		}
		GuideStep currentStep = GetCurrentStep();
		if (currentStep != null && currentStep.HasAnyMotion)
		{
			CancelGuideSequence();
			HideGuideView();
			MoveToWaitingForRingSolved();
			KillReshowTimer();
		}
		else if (state == WeakGuideState.WaitingForGesture)
		{
			GuideStep currentStep2 = GetCurrentStep();
			if (currentStep2 == null || !currentStep2.HasAnyMotion)
			{
				MoveToWaitingForRingSolved();
				KillReshowTimer();
				return;
			}
			currentStep2.FinishAll();
			HideGuideView();
			MoveToWaitingForRingSolved();
			KillReshowTimer();
		}
		else if (state == WeakGuideState.WaitingForRingSolved)
		{
			KillReshowTimer();
		}
	}

	public void OnPlayerReleased()
	{
		if (state == WeakGuideState.WaitingForRingSolved)
		{
			StartReshowTimer();
		}
	}

	public void OnRingSolved()
	{
		CancelGuideSequence();
		KillReshowTimer();
		if (state == WeakGuideState.WaitingForRingSolved)
		{
			TryReportCurrentTutorialComplete();
			currentIndex++;
			if (currentIndex >= guideSteps.Count)
			{
				CompleteGuide();
			}
			else
			{
				StartCurrentStep();
			}
		}
	}

	public void FinishGuideOnLevelEnd()
	{
		KillReshowTimer();
		CancelGuideSequence();
		if (state != WeakGuideState.Completed)
		{
			state = WeakGuideState.Completed;
		}
		if (GameApp.UI.IsOpen("WeakGuideView"))
		{
			GameApp.UI.CloseView("WeakGuideView", (object)null, false);
		}
	}

	public void ResetGuide()
	{
		CancelGuideSequence();
		KillReshowTimer();
		if (guideSteps != null)
		{
			for (int i = 0; i < guideSteps.Count; i++)
			{
				guideSteps[i]?.StopSequence();
			}
		}
		currentIndex = 0;
		state = WeakGuideState.Idle;
	}

	private void OnGestureEvent(GameplayGestureEvent evt)
	{
		if (!TryFinishByHandle(WeakGuideStepTrigger.PlayerPressed, evt))
		{
			OnGesture();
		}
	}

	private void OnRingSolvedEvent(RingSolvedEvent evt)
	{
		if (!TryFinishByHandle(WeakGuideStepTrigger.RingSolved, evt))
		{
			OnRingSolved();
		}
	}

	private void OnPlayerReleasedEvent(GameplayPointerReleasedEvent evt)
	{
		if (!TryFinishByHandle(WeakGuideStepTrigger.PlayerReleased, evt))
		{
			OnPlayerReleased();
		}
	}

	private bool TryFinishByHandle(WeakGuideStepTrigger trigger, object args)
	{
		GuideStep currentStep = GetCurrentStep();
		IWeakGuideStepHandle weakGuideStepHandle = currentStep?.Handle;
		if (weakGuideStepHandle == null)
		{
			return false;
		}
		if (!weakGuideStepHandle.TryFinishStep(trigger, args))
		{
			return false;
		}
		if (currentStep != null)
		{
			weakGuideStepHandle.OnStepFinish(this, currentStep);
		}
		AdvanceToNextStep();
		return true;
	}

	private void AdvanceToNextStep()
	{
		CancelGuideSequence();
		KillReshowTimer();
		if (state == WeakGuideState.WaitingForRingSolved || state == WeakGuideState.WaitingForGesture)
		{
			TryReportCurrentTutorialComplete();
			currentIndex++;
			if (currentIndex >= guideSteps.Count)
			{
				CompleteGuide();
			}
			else
			{
				StartCurrentStep();
			}
		}
	}

	private void StartCurrentStep()
	{
		GuideStep currentStep = GetCurrentStep();
		if (currentStep == null || !currentStep.HasAnyMotion)
		{
			MoveToWaitingForRingSolved();
			return;
		}
		currentStep.Handle?.OnStepStart(this, currentStep);
		state = WeakGuideState.WaitingForGesture;
		ShowGuideView();
		StartGuideSequence(currentIndex);
	}

	private void MoveToWaitingForRingSolved()
	{
		state = WeakGuideState.WaitingForRingSolved;
	}

	private void CompleteGuide()
	{
		state = WeakGuideState.Completed;
		KillReshowTimer();
		Singleton<EventManager>.Instance.Send<WeakGuideCompletedEvent>(new WeakGuideCompletedEvent());
	}

	private GuideStep GetStep(int stepIndex)
	{
		if (guideSteps == null || stepIndex < 0 || stepIndex >= guideSteps.Count)
		{
			return null;
		}
		return guideSteps[stepIndex];
	}

	private GuideStep GetCurrentStep()
	{
		if (guideSteps == null || currentIndex < 0 || currentIndex >= guideSteps.Count)
		{
			return null;
		}
		return guideSteps[currentIndex];
	}

	private void ShowGuideView()
	{
		GameApp.UI.IsOpen("WeakGuideView");
		if (!GameApp.UI.IsOpen("WeakGuideView"))
		{
			GameApp.UI.Open("WeakGuideView", (UILayer)2, (object)null, (Action)null);
		}
		GuideStep currentStep = GetCurrentStep();
		if (currentStep != null)
		{
			IController val = default(IController);
			if (GameApp.UI.IsOpen("WeakGuideView", ref val) && val is UIWeakGuideController uIWeakGuideController)
			{
				uIWeakGuideController.SetGuideText(currentStep.GuideTextKey);
				uIWeakGuideController.PlayOpenAnimation();
			}
			TryReportCurrentTutorialOpen();
		}
	}

	private void HideGuideView()
	{
		IController val = default(IController);
		if (!GameApp.UI.IsOpen("WeakGuideView", ref val) || !(val is UIWeakGuideController uIWeakGuideController))
		{
			return;
		}
		uIWeakGuideController.PlayCloseAnimation();
		float closeAnimationDuration = uIWeakGuideController.GetCloseAnimationDuration();
		if (closeAnimationDuration > 0f)
		{
			GameApp.Timer.Delay((Object)(object)this, closeAnimationDuration, (Action)delegate
			{
				if (GameApp.UI.IsOpen("WeakGuideView"))
				{
					GameApp.UI.CloseView("WeakGuideView", (object)null, false);
				}
			});
		}
		else
		{
			GameApp.UI.CloseView("WeakGuideView", (object)null, false);
		}
	}

	private void StartReshowTimer()
	{
		KillReshowTimer();
		reshowGuideTimerId = GameApp.Timer.Delay((Object)(object)this, 3f, (Action)OnReshowTimerElapsed);
	}

	private void KillReshowTimer()
	{
		if (reshowGuideTimerId >= 0)
		{
			GameApp.Timer.Kill(reshowGuideTimerId);
			reshowGuideTimerId = -1;
		}
	}

	private void OnReshowTimerElapsed()
	{
		reshowGuideTimerId = -1;
		if (state == WeakGuideState.WaitingForRingSolved)
		{
			GuideStep currentStep = GetCurrentStep();
			if (currentStep != null && currentStep.HasAnyMotion)
			{
				WeakGuideSetupHelpers.RefreshGuideRotations(this);
				currentStep.RestartSequence();
				state = WeakGuideState.WaitingForGesture;
				ShowGuideView();
			}
		}
	}

	private void TryReportCurrentTutorialOpen()
	{
		if (IsFirstLevel() && currentIndex >= 0)
		{
			int num = currentIndex + 1;
			RRDataTrack.Instance.ReportTutorial(num, 0, $"新手引导-步骤{num}-打开");
		}
	}

	private void TryReportCurrentTutorialComplete()
	{
		if (IsFirstLevel() && currentIndex >= 0)
		{
			int num = currentIndex + 1;
			RRDataTrack.Instance.ReportTutorial(num, 0, $"新手引导-步骤{num}-完成");
		}
	}

	private static bool IsFirstLevel()
	{
		LevelModelProxy levelModelProxy = GameApp.Proxy.Get<LevelModelProxy>();
		if (levelModelProxy == null)
		{
			return false;
		}
		return levelModelProxy.Level == 1;
	}
}
