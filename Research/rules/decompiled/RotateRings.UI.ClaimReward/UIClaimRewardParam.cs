using System;
using System.Collections.Generic;
using RotateRings.Ad;
using RotateRings.Prop;
using UnityEngine;

namespace RotateRings.UI.ClaimReward;

public class UIClaimRewardParam
{
	public Vector2 BoxStartWorldPosition;

	public string BoxAnimName;

	public int RewardId;

	public bool EnableBoxFlyFadeIn = true;

	public bool SkipBoxAnimation;

	public bool SkipMaskFadeIn;

	public Vector2 PropFlyTargetPos;

	public string BoxPrefabLocation;

	public string BoxSkeletonDataAssetPath;

	public string BoxSkinName;

	public string TitleTextKey;

	public int RankRewardRank;

	public bool IsRankReward;

	public bool HideBgMaskOnOpen;

	public Action OnCloseStarted;

	public Action OnBeforeClose;

	public Action OnBoxArrive;

	public Action OnFlyStart;

	public Action OnFirstPropArrive;

	public string RewardReasonId;

	public string RewardReasonDetail;

	public Dictionary<int, int> ExtraRewardItems;

	public Dictionary<int, int> RewardItems;

	public int GrantedMultiplier;

	public int ChestCycleIndex = -1;

	public string PendingRewardTransactionId;

	public bool EnableClaimX2;

	public bool UseContinueButton;

	public Func<bool> ClaimHandler;

	public bool IsFreeDoubleReward;

	public Func<int, bool, bool> ClaimMultiplierHandler;

	public Action OnClaimSucceeded;

	public Action OnOpen;

	public Action OnNoBoxOpenStarted;

	public Action OnClosed;

	public AdPopPosition AdPopPosition;

	public Action OnClaimClickTrack;

	public Action OnClaimX2ClickTrack;

	public Action<EPropType> OnAdEnterTrack;
}
