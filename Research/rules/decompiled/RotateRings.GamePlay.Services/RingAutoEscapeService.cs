using UnityEngine;
using XingYun.Experiment;

namespace RotateRings.GamePlay.Services;

public sealed class RingAutoEscapeService : GameplayServiceBase<GameplayManager>
{
	private const string TAG = "[GamePlay][AutoEscape] ";

	private const float FallbackDpi = 160f;

	private const float FallbackTriggerWorld = 0.5f;

	private const float EscapeTimeoutPadding = 0.3f;

	private const float MaxStepDistance = 0.025f;

	private PullObject escapingPull;

	private Vector2 escapeDirection;

	private float escapeStartSpeed;

	private float escapeEndSpeed;

	private float escapeDistance;

	private float escapeTimeout;

	private float traveled;

	private float elapsed;

	public RingAutoEscapeService(GameplayManager host)
		: base(host)
	{
	}

	public bool TryBegin(PullObject pull)
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		int num = ReadExperimentLevel();
		if (num <= 0)
		{
			return false;
		}
		RingAutoEscapeConfig.ApplyExperimentLevel(num);
		if ((Object)(object)pull == (Object)null || pull.IsSolved || (Object)(object)escapingPull != (Object)null)
		{
			return false;
		}
		if (pull.HasLiveSelfBomb())
		{
			return false;
		}
		RingObjectSolveService solveService = base.Host.SolveService;
		if (solveService != null && solveService.CanSolve(pull))
		{
			return false;
		}
		float gestureAxisOffsetFromStart = pull.GetGestureAxisOffsetFromStart();
		if (Mathf.Abs(gestureAxisOffsetFromStart) < GetTriggerWorldThreshold())
		{
			return false;
		}
		float num2 = pull.GetGestureDurationSeconds() * 1000f;
		if (RingAutoEscapeConfig.MaxTriggerGestureMilliseconds > 0f && num2 > RingAutoEscapeConfig.MaxTriggerGestureMilliseconds)
		{
			return false;
		}
		Vector2 lockedDragDirection = pull.GetLockedDragDirection();
		if (((Vector2)(ref lockedDragDirection)).sqrMagnitude < 1E-08f)
		{
			return false;
		}
		((Vector2)(ref lockedDragDirection)).Normalize();
		Vector2 direction = lockedDragDirection * Mathf.Sign(gestureAxisOffsetFromStart);
		if (!pull.TryPlanEscape(direction, out var num3))
		{
			return false;
		}
		escapingPull = pull;
		escapeDirection = direction;
		escapeDistance = num3;
		traveled = 0f;
		elapsed = 0f;
		float num4 = Mathf.Abs(pull.GetAxisSpeedAtRelease());
		float minEscapeSpeed = RingAutoEscapeConfig.MinEscapeSpeed;
		float maxEscapeSpeed = RingAutoEscapeConfig.MaxEscapeSpeed;
		escapeStartSpeed = Mathf.Clamp((num4 < minEscapeSpeed) ? minEscapeSpeed : num4, minEscapeSpeed, maxEscapeSpeed);
		escapeEndSpeed = maxEscapeSpeed;
		escapeTimeout = escapeDistance / Mathf.Max(minEscapeSpeed, 0.01f) + 0.3f;
		pull.BeginAutoEscape(this);
		return true;
	}

	public void TickFrame()
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)escapingPull == (Object)null)
		{
			return;
		}
		PullObject pullObject = escapingPull;
		if ((Object)(object)pullObject == (Object)null || pullObject.AutoEscapeOwner != this)
		{
			Abort(settle: false);
			return;
		}
		if (pullObject.IsSolved)
		{
			Finish();
			return;
		}
		float deltaTime = Time.deltaTime;
		elapsed += deltaTime;
		float num = Mathf.Clamp01(elapsed / RingAutoEscapeConfig.EscapeAccelerateSeconds);
		float num2 = Mathf.Min(Mathf.Lerp(escapeStartSpeed, escapeEndSpeed, num) * deltaTime, escapeDistance - traveled);
		float num3 = 0f;
		float num4 = num2;
		while (num4 > 1E-05f)
		{
			float distance = Mathf.Min(0.025f, num4);
			float num5 = pullObject.MoveEscapeStep(distance, escapeDirection);
			if (num5 <= 1E-05f)
			{
				break;
			}
			num3 += num5;
			num4 -= num5;
		}
		traveled += num3;
		pullObject.SyncEscapeContacts();
		if (HasEscaped(pullObject))
		{
			Finish();
		}
		else if (num3 < num2 - 0.0001f)
		{
			Abort(settle: true);
		}
		else if (traveled >= escapeDistance - 0.0001f || elapsed >= escapeTimeout)
		{
			Finish();
		}
	}

	private bool HasEscaped(PullObject pull)
	{
		if (!pull.IsSolved)
		{
			return base.Host.SolveService?.CanSolve(pull) ?? false;
		}
		return true;
	}

	public void Abort(bool settle)
	{
		PullObject pullObject = escapingPull;
		if (!((Object)(object)pullObject == (Object)null))
		{
			escapingPull = null;
			pullObject.EndAutoEscape();
			if (!settle)
			{
				pullObject.ClearDragSessionState();
			}
			else
			{
				SettleAndClose(pullObject);
			}
		}
	}

	private void Finish()
	{
		PullObject pullObject = escapingPull;
		if (!((Object)(object)pullObject == (Object)null))
		{
			escapingPull = null;
			pullObject.EndAutoEscape();
			SettleAndClose(pullObject);
		}
	}

	private static void SettleAndClose(PullObject pull)
	{
		pull.FinishReleaseSettlement();
		pull.ClearDragSessionState();
	}

	public override void ClearLevel()
	{
		if (!((Object)(object)escapingPull == (Object)null))
		{
			escapingPull.EndAutoEscape();
			escapingPull = null;
		}
	}

	public override void Dispose()
	{
		if (!base.IsDisposed)
		{
			ClearLevel();
			base.Dispose();
		}
	}

	private static int ReadExperimentLevel()
	{
		return FunctionDataCenter.Get<GameBord>()?.GameRingAutoEscape ?? 0;
	}

	private static float GetTriggerWorldThreshold()
	{
		Camera gameplayCamera = GameplayCameraManager.GameplayCamera;
		if ((Object)(object)gameplayCamera == (Object)null || !gameplayCamera.orthographic || Screen.height <= 0)
		{
			return 0.5f;
		}
		float num = ((Screen.dpi > 0f) ? Screen.dpi : 160f);
		float num2 = RingAutoEscapeConfig.MinTriggerMillimeters / 25.4f * num;
		float num3 = 2f * gameplayCamera.orthographicSize / (float)Screen.height;
		return num2 * num3;
	}
}
