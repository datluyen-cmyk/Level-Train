using Spine.Unity;
using UnityEngine;

namespace HsTileMahJong;

[ExecuteAlways]
[AddComponentMenu("UI/Comment/Spine/AutoPlaySpineAnimation")]
public class AutoPlaySpineAnimation : MonoBehaviour
{
	public SkeletonGraphic skeletonGraphic;

	public bool loop;

	public float timeScale = 1f;

	public bool autoPlayOnEnable = true;

	private void OnEnable()
	{
		if (autoPlayOnEnable)
		{
			PlaySpine();
		}
	}

	public void PlaySpine()
	{
		if ((Object)(object)skeletonGraphic == (Object)null)
		{
			skeletonGraphic = ((Component)this).GetComponent<SkeletonGraphic>();
		}
		if (!((Object)(object)skeletonGraphic == (Object)null))
		{
			skeletonGraphic.AnimationState.SetAnimation(0, skeletonGraphic.startingAnimation, loop);
			skeletonGraphic.AnimationState.TimeScale = timeScale;
		}
	}

	public void StopSpine()
	{
		if ((Object)(object)skeletonGraphic == (Object)null)
		{
			skeletonGraphic = ((Component)this).GetComponent<SkeletonGraphic>();
		}
		if (!((Object)(object)skeletonGraphic == (Object)null) && skeletonGraphic.AnimationState != null)
		{
			skeletonGraphic.AnimationState.ClearTracks();
		}
	}
}
