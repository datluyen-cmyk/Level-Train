using System.Collections.Generic;
using UnityEngine;

public class RingRelative : MonoBehaviour, IObjectEffectedByLayerArea
{
	[SerializeField]
	protected string shapeId;

	[SerializeField]
	private string featureId;

	[SerializeField]
	protected Overlap2DHandler overlap2DHandler;

	[SerializeField]
	protected Collider2D selfCollider;

	[SerializeField]
	protected Collider2D[] blockColliders;

	[SerializeField]
	protected LayerMaskHelper[] extendLayerMaskHelpers;

	protected List<ILayerMaskHelper> layerMaskHelpers;

	private IGameplayObject cachedOwner;

	private bool ownerResolved;

	public string FeatureId
	{
		get
		{
			return featureId;
		}
		set
		{
			featureId = value;
		}
	}

	public Collider2D[] BlockColliders => blockColliders;

	public bool IsSolved { get; protected set; }

	public IGameplayObject GetOwnerGameplayObject()
	{
		if (!ownerResolved)
		{
			RingObject componentInParent = ((Component)this).GetComponentInParent<RingObject>();
			IGameplayObject gameplayObject;
			if (!((Object)(object)componentInParent != (Object)null))
			{
				IGameplayObject componentInParent2 = ((Component)this).GetComponentInParent<PullObject>();
				gameplayObject = componentInParent2;
			}
			else
			{
				IGameplayObject componentInParent2 = componentInParent;
				gameplayObject = componentInParent2;
			}
			cachedOwner = gameplayObject;
			ownerResolved = true;
		}
		return cachedOwner;
	}

	public virtual int GetOverlapColliders(Collider2D[] listOverlap)
	{
		return overlap2DHandler.GetOverlapColliders(listOverlap);
	}

	public virtual int GetOverlapCount()
	{
		return overlap2DHandler.GetOverlapCount();
	}

	public virtual bool IsBlockedByRing()
	{
		return GetOverlapCount() > 0;
	}

	public bool ContainsPhysicalBlockCollider(Collider2D col)
	{
		if ((Object)(object)col == (Object)null || blockColliders == null)
		{
			return false;
		}
		for (int i = 0; i < blockColliders.Length; i++)
		{
			if ((Object)(object)blockColliders[i] == (Object)(object)col)
			{
				return true;
			}
		}
		return false;
	}

	public void CollectSelfColliderIds(HashSet<int> output)
	{
		if (output != null && !((Object)(object)selfCollider == (Object)null))
		{
			output.Add(((Object)selfCollider).GetInstanceID());
		}
	}

	public virtual void DoFade()
	{
		if (blockColliders == null)
		{
			return;
		}
		Collider2D[] array = blockColliders;
		for (int i = 0; i < array.Length; i++)
		{
			Behaviour val = (Behaviour)(object)array[i];
			if ((Object)(object)val != (Object)null)
			{
				val.enabled = false;
			}
		}
	}

	public virtual void Solve()
	{
		if (!IsSolved)
		{
			IsSolved = true;
			DoFade();
		}
	}

	public void SetBlockedByLayer(LayerArea layerArea, bool isBlocked)
	{
		LayerArea componentInParent = ((Component)this).GetComponentInParent<LayerArea>();
		if ((Object)(object)layerArea != (Object)(object)componentInParent || layerMaskHelpers == null)
		{
			return;
		}
		layerMaskHelpers.ForEach(delegate(ILayerMaskHelper h)
		{
			if (isBlocked)
			{
				h.SetLayer(0);
			}
			else
			{
				h.ResetToOriginalLayer();
			}
		});
	}

	protected virtual void Awake()
	{
		SetupLayerMaskHelpers();
	}

	protected void SetupLayerMaskHelpers()
	{
		layerMaskHelpers = new List<ILayerMaskHelper>();
		if (overlap2DHandler != null)
		{
			layerMaskHelpers.Add(overlap2DHandler);
		}
		LayerMaskHelper component = ((Component)((Component)this).transform).GetComponent<LayerMaskHelper>();
		if ((Object)(object)component != (Object)null)
		{
			layerMaskHelpers.Add(component);
		}
		layerMaskHelpers.AddRange(((Component)((Component)this).transform).GetComponentsInChildren<LayerMaskHelper>());
		if (extendLayerMaskHelpers != null)
		{
			LayerMaskHelper[] array = extendLayerMaskHelpers;
			for (int i = 0; i < array.Length; i++)
			{
				layerMaskHelpers.Add(array[i]);
			}
		}
	}

	protected virtual void OnDestroy()
	{
	}
}
