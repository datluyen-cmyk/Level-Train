using System;

namespace RotateRings.Ad.Orchestration;

public sealed class FrontAdCoordinator
{
	private sealed class FrontAdPending
	{
		public InterstitialTrigger Trigger;

		public RRAdContext Context;

		public Action<RRShowAdResult> Callback;
	}

	public readonly struct PendingRequest
	{
		public InterstitialTrigger Trigger { get; }

		public RRAdContext Context { get; }

		public Action<RRShowAdResult> Callback { get; }

		public PendingRequest(InterstitialTrigger trigger, RRAdContext context, Action<RRShowAdResult> callback)
		{
			Trigger = trigger;
			Context = context;
			Callback = callback;
		}
	}

	private const int PendingGateLevelReady = 1;

	private const int PendingGateEnterPresentationReady = 2;

	private FrontAdPending frontAdPending;

	private bool requireEnterPresentation;

	private int readyGateMask;

	public bool HasPending => frontAdPending != null;

	public bool ShouldDeferToEnterAnimation
	{
		get
		{
			if (frontAdPending != null)
			{
				return requireEnterPresentation;
			}
			return false;
		}
	}

	public void Queue(InterstitialTrigger trigger, RRAdContext context, Action<RRShowAdResult> callback, bool waitForEnterPresentation)
	{
		frontAdPending = new FrontAdPending
		{
			Trigger = trigger,
			Context = context,
			Callback = callback
		};
		requireEnterPresentation = waitForEnterPresentation;
		readyGateMask = 0;
	}

	public bool NotifyLevelReady(Func<PendingRequest, bool> consume)
	{
		readyGateMask |= 1;
		return TryConsumeInternal(consume, force: false);
	}

	public bool NotifyEnterPresentationReady(Func<PendingRequest, bool> consume)
	{
		readyGateMask |= 2;
		return TryConsumeInternal(consume, force: false);
	}

	public bool TryConsumeForce(Func<PendingRequest, bool> consume)
	{
		return TryConsumeInternal(consume, force: true);
	}

	public void Clear(string reason = null)
	{
		if (frontAdPending != null || readyGateMask != 0 || requireEnterPresentation)
		{
			Reset();
		}
	}

	private bool TryConsumeInternal(Func<PendingRequest, bool> consume, bool force)
	{
		if (frontAdPending == null)
		{
			return false;
		}
		if (!force)
		{
			int num = ((!requireEnterPresentation) ? 1 : 3);
			if ((readyGateMask & num) != num)
			{
				return false;
			}
		}
		PendingRequest arg = new PendingRequest(frontAdPending.Trigger, frontAdPending.Context, frontAdPending.Callback);
		Reset();
		return consume?.Invoke(arg) ?? false;
	}

	private void Reset()
	{
		frontAdPending = null;
		requireEnterPresentation = false;
		readyGateMask = 0;
	}
}
