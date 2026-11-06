using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rotaterings.UI.RankList;

public class RankTopHeader : MonoBehaviour
{
	private const string MovingRankAnimName = "anim_rank_main_first_gp1";

	private const string UnchangedRankAnimName = "anim_rank_main_first_gp3";

	[SerializeField]
	private RankItem firstRankItem;

	[SerializeField]
	private RankItem secondRankItem;

	[SerializeField]
	private RankItem thirdRankItem;

	[SerializeField]
	private GameObject firstRankEffect;

	[SerializeField]
	private GameObject secondRankEffect;

	[SerializeField]
	private GameObject thirdRankEffect;

	[SerializeField]
	private Animation firstRankAni;

	[SerializeField]
	private Animation secondRankAni;

	[SerializeField]
	private Animation thirdRankAni;

	[SerializeField]
	private GameObject firstRankEffectBoom;

	[SerializeField]
	private GameObject secondRankEffectBoom;

	[SerializeField]
	private GameObject thirdRankEffectBoom;

	public RankItem FirstRankItem => firstRankItem;

	public RankItem SecondRankItem => secondRankItem;

	public RankItem ThirdRankItem => thirdRankItem;

	public void ShowRankEffect(int rank)
	{
		SetUnchangedRankEffectActive(rank, active: false);
		SetRankEffectActive(rank, active: true);
	}

	public void HideRankEffect(int rank)
	{
		ResetAnimationClip(GetRankAnimation(rank), "anim_rank_main_first_gp1");
		SetRankEffectActive(rank, active: false);
		SetUnchangedRankEffectActive(rank, active: false);
	}

	public void ShowUnchangedRankEffect(int rank)
	{
		SetRankEffectActive(rank, active: false);
		SetUnchangedRankEffectActive(rank, active: true);
	}

	public void HideUnchangedRankEffect(int rank)
	{
		ResetAnimationClip(GetRankAnimation(rank), "anim_rank_main_first_gp3");
		SetRankEffectActive(rank, active: false);
		SetUnchangedRankEffectActive(rank, active: false);
	}

	public void HideAllRankEffects()
	{
		ResetRankAnimation(firstRankAni);
		ResetRankAnimation(secondRankAni);
		ResetRankAnimation(thirdRankAni);
		SetRankEffectActive(1, active: false);
		SetRankEffectActive(2, active: false);
		SetRankEffectActive(3, active: false);
		SetUnchangedRankEffectActive(1, active: false);
		SetUnchangedRankEffectActive(2, active: false);
		SetUnchangedRankEffectActive(3, active: false);
	}

	public void SetRankAnimationAsLastSibling(int rank)
	{
		Animation rankAnimation = GetRankAnimation(rank);
		if ((Object)(object)rankAnimation != (Object)null)
		{
			((Component)rankAnimation).transform.SetAsLastSibling();
		}
	}

	public void PlayRankAnimation(int rank)
	{
		PlayExclusiveAnimation(rank, "anim_rank_main_first_gp1", "anim_rank_main_first_gp3");
	}

	public void PlayUnchangedRankAnimation(int rank)
	{
		PlayExclusiveAnimation(rank, "anim_rank_main_first_gp3", "anim_rank_main_first_gp1");
	}

	private void SetRankEffectActive(int rank, bool active)
	{
		GameObject rankEffect = GetRankEffect(rank);
		if (rankEffect != null)
		{
			rankEffect.SetActive(active);
		}
	}

	private void SetUnchangedRankEffectActive(int rank, bool active)
	{
		GameObject unchangedRankEffect = GetUnchangedRankEffect(rank);
		if (unchangedRankEffect != null)
		{
			unchangedRankEffect.SetActive(active);
		}
	}

	private GameObject GetRankEffect(int rank)
	{
		return (GameObject)(rank switch
		{
			1 => firstRankEffect, 
			2 => secondRankEffect, 
			3 => thirdRankEffect, 
			_ => null, 
		});
	}

	private GameObject GetUnchangedRankEffect(int rank)
	{
		return (GameObject)(rank switch
		{
			1 => firstRankEffectBoom, 
			2 => secondRankEffectBoom, 
			3 => thirdRankEffectBoom, 
			_ => null, 
		});
	}

	private Animation GetRankAnimation(int rank)
	{
		return (Animation)(rank switch
		{
			1 => firstRankAni, 
			2 => secondRankAni, 
			3 => thirdRankAni, 
			_ => null, 
		});
	}

	private void PlayExclusiveAnimation(int rank, string animationName, string otherAnimationName)
	{
		Animation rankAnimation = GetRankAnimation(rank);
		if (!((Object)(object)rankAnimation == (Object)null) && !((Object)(object)rankAnimation.GetClip(animationName) == (Object)null))
		{
			rankAnimation.Stop(otherAnimationName);
			SetRankEffectActive(rank, active: false);
			SetUnchangedRankEffectActive(rank, active: false);
			rankAnimation.Stop(animationName);
			rankAnimation.Rewind(animationName);
			rankAnimation.Play(animationName);
		}
	}

	private static void ResetRankAnimation(Animation animation)
	{
		if (!((Object)(object)animation == (Object)null))
		{
			ResetAnimationClip(animation, "anim_rank_main_first_gp1");
			ResetAnimationClip(animation, "anim_rank_main_first_gp3");
		}
	}

	private static void ResetAnimationClip(Animation animation, string animationName)
	{
		if (!((Object)(object)animation == (Object)null))
		{
			AnimationState val = animation[animationName];
			if (!((TrackedReference)(object)val == (TrackedReference)null))
			{
				animation.Stop(animationName);
				val.time = 0f;
				val.enabled = true;
				animation.Sample();
				val.enabled = false;
			}
		}
	}

	public void Render(IReadOnlyList<RankItemData> allEntries, ERankContent rankType, Action<int, Transform> onRewardBoxClicked, int collectIconSide = 0)
	{
		HideAllRankEffects();
		RenderSlot(firstRankItem, allEntries, 0, rankType, onRewardBoxClicked, collectIconSide);
		RenderSlot(secondRankItem, allEntries, 1, rankType, onRewardBoxClicked, collectIconSide);
		RenderSlot(thirdRankItem, allEntries, 2, rankType, onRewardBoxClicked, collectIconSide);
	}

	public RankItem GetRankItemForEntry(RankItemData entry)
	{
		if (entry == null || entry.RankIndex < 1 || entry.RankIndex > 3)
		{
			return null;
		}
		return entry.RankIndex switch
		{
			1 => firstRankItem, 
			2 => secondRankItem, 
			3 => thirdRankItem, 
			_ => null, 
		};
	}

	private static void RenderSlot(RankItem slot, IReadOnlyList<RankItemData> allEntries, int dataIndex, ERankContent rankType, Action<int, Transform> onRewardBoxClicked, int collectIconSide)
	{
		if (!((Object)(object)slot == (Object)null))
		{
			if (allEntries == null || dataIndex < 0 || dataIndex >= allEntries.Count)
			{
				((Component)slot).gameObject.SetActive(false);
				return;
			}
			((Component)slot).gameObject.SetActive(true);
			slot.SetSettleTopThree(value: true);
			slot.SetRankType(rankType);
			slot.InitializeCollectPosition(topHeader: true, collectIconSide);
			slot.Render(allEntries[dataIndex]);
			slot.OnRewardBoxClicked = onRewardBoxClicked;
		}
	}
}
