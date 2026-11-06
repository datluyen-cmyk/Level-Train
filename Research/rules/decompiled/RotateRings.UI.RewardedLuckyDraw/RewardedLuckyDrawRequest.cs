using System;
using Example.Cfg;
using XingYun.UI.Controller;

namespace RotateRings.UI.RewardedLuckyDraw;

public sealed class RewardedLuckyDrawRequest
{
	public int PresentationType;

	public RewardedLuckyDrawProbability[] Slots;

	public long Victory;

	public string UIPath;

	public bool Valid = true;

	public bool Registered;

	public bool Shown;

	public IController Controller;

	public bool AdInFlight;

	public bool OpeningClaim;

	public bool Completed;

	public Action OnCompleted;

	public Action OnHostCompleted;
}
