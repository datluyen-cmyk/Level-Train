using System.Collections.Generic;
using RotateRings;
using RotateRings.Common;
using RotateRings.Services;
using UnityEngine;
using UnlockRing;
using Vibration;

public class BombRelative : MonoBehaviour
{
	private const string TAG = "[GamePlay][BombRelative] ";

	private const int OverlapBufferSize = 32;

	private const float BombContactClearance = 0.03f;

	[SerializeField]
	private Collider2D activeCollider;

	private readonly Collider2D[] overlapBuffer = (Collider2D[])(object)new Collider2D[32];

	private readonly List<IGameplayObject> explodeTargets = new List<IGameplayObject>(8);

	private ContactFilter2D contactFilter;

	private IGameplayObject cachedHostGameplayObject;

	private bool isExploding;

	private bool removeVisualDone;

	public Collider2D ActiveCollider => activeCollider;

	private bool IsSolved { get; set; }

	public bool IsSolvedForTrack => IsSolved;

	public bool IsStandalone => cachedHostGameplayObject == null;

	protected void Awake()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		contactFilter = ContactFilter2D.noFilter;
		contactFilter.useTriggers = false;
		if ((Object)(object)activeCollider != (Object)null)
		{
			((ContactFilter2D)(ref contactFilter)).SetLayerMask(LayerMask.op_Implicit(Physics2D.GetLayerCollisionMask(((Component)activeCollider).gameObject.layer)));
		}
		cachedHostGameplayObject = ResolveHostGameplayObject();
	}

	public void RemoveWithoutExplosion()
	{
		MarkRemovedWithoutExplosion();
		FinishRemoveWithoutExplosion();
	}

	public void MarkRemovedWithoutExplosion()
	{
		if (!isExploding && !IsSolved)
		{
			IsSolved = true;
			if ((Object)(object)activeCollider != (Object)null)
			{
				((Behaviour)activeCollider).enabled = false;
			}
		}
	}

	public void FinishRemoveWithoutExplosion()
	{
		if (!removeVisualDone)
		{
			removeVisualDone = true;
			GameApp.Event.Send<BombRemoveWithoutExplosionEvent>(new BombRemoveWithoutExplosionEvent
			{
				Bomb = this
			});
		}
	}

	public void Solve()
	{
		if (!IsSolved)
		{
			IsSolved = true;
			GameApp.Event.Send<BombExplodeStartEvent>(new BombExplodeStartEvent
			{
				Bomb = this
			});
			if ((Object)(object)activeCollider != (Object)null)
			{
				((Behaviour)activeCollider).enabled = false;
			}
			Mesh sourceMesh = null;
			MeshFilter val = ((Component)this).GetComponent<MeshFilter>() ?? ((Component)this).GetComponentInChildren<MeshFilter>();
			if ((Object)(object)val != (Object)null)
			{
				sourceMesh = val.sharedMesh;
			}
			DispatchRemoveEffect(sourceMesh, ((Component)this).transform, ((Component)this).gameObject);
		}
	}

	private void DispatchRemoveEffect(Mesh sourceMesh, Transform sourceTransform, GameObject partGo)
	{
		if ((Object)(object)sourceMesh == (Object)null)
		{
			if ((Object)(object)partGo != (Object)null && Application.isPlaying)
			{
				Object.Destroy((Object)(object)partGo);
			}
			return;
		}
		RingRemoveEffectEvent ringRemoveEffectEvent = new RingRemoveEffectEvent
		{
			SourceMesh = sourceMesh,
			SourceTransform = sourceTransform,
			PartGo = partGo,
			EffectParent = sourceTransform.parent,
			IsFromBomb = true
		};
		GameApp.Event.Send<RingRemoveEffectEvent>(ringRemoveEffectEvent);
		if (!ringRemoveEffectEvent.Handled && (Object)(object)partGo != (Object)null && Application.isPlaying)
		{
			Object.Destroy((Object)(object)partGo);
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (!((Object)(object)other == (Object)null))
		{
			TryExplode(other);
		}
	}

	public bool TryExplodeFromDragProbes(IReadOnlyList<Collider2D> dragProbes)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (dragProbes == null || dragProbes.Count == 0 || (Object)(object)activeCollider == (Object)null || !((Behaviour)activeCollider).enabled)
		{
			return false;
		}
		GameplayManager instance = SingletonComponent<GameplayManager>.Instance;
		if ((instance != null && instance.IsLevelEnterPlaying) || IsSolved || isExploding)
		{
			return false;
		}
		int num = activeCollider.Overlap(contactFilter, overlapBuffer);
		for (int i = 0; i < num && i < overlapBuffer.Length; i++)
		{
			Collider2D val = overlapBuffer[i];
			if (!((Object)(object)val == (Object)null) && ((Behaviour)val).enabled && ContainsCollider(dragProbes, val) && !IsSelfOrHostProbe(val))
			{
				return TryExplode(val);
			}
		}
		if (TryExplodeFromDragProbeDistance(dragProbes))
		{
			return true;
		}
		return TryExplodeFromExternalOverlaps();
	}

	public bool TryExplodeFromSelfContact(Collider2D target)
	{
		if ((Object)(object)target == (Object)null || (Object)(object)activeCollider == (Object)null || !((Behaviour)activeCollider).enabled)
		{
			return false;
		}
		if (IsSelfOrHostProbe(target))
		{
			return false;
		}
		return TryExplode(target);
	}

	private bool TryExplodeFromDragProbeDistance(IReadOnlyList<Collider2D> dragProbes)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < dragProbes.Count; i++)
		{
			Collider2D val = dragProbes[i];
			if (!((Object)(object)val == (Object)null) && ((Behaviour)val).enabled && !IsSelfOrHostProbe(val))
			{
				ColliderDistance2D val2 = Physics2D.Distance(val, activeCollider);
				if (((ColliderDistance2D)(ref val2)).isValid && !(((ColliderDistance2D)(ref val2)).distance > 0.03f))
				{
					return TryExplode(val);
				}
			}
		}
		return false;
	}

	private bool TryExplodeFromExternalOverlaps()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)activeCollider == (Object)null || !((Behaviour)activeCollider).enabled)
		{
			return false;
		}
		ContactFilter2D noFilter = ContactFilter2D.noFilter;
		noFilter.useTriggers = true;
		noFilter.useLayerMask = true;
		noFilter.layerMask = GameConst.OverlapAreaLayerMask;
		int num = activeCollider.Overlap(noFilter, overlapBuffer);
		for (int i = 0; i < num && i < overlapBuffer.Length; i++)
		{
			Collider2D val = overlapBuffer[i];
			if (!((Object)(object)val == (Object)null) && ((Behaviour)val).enabled && !IsSelfOrHostProbe(val) && TryExplode(val))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsSelfOrHostProbe(Collider2D probe)
	{
		if ((Object)(object)probe == (Object)null)
		{
			return false;
		}
		Transform transform = ((Component)probe).transform;
		if ((Object)(object)transform == (Object)(object)((Component)this).transform || transform.IsChildOf(((Component)this).transform))
		{
			return true;
		}
		IGameplayObject gameplayObject = cachedHostGameplayObject;
		Component val = (Component)((gameplayObject is Component) ? gameplayObject : null);
		if (val == null)
		{
			return false;
		}
		Transform transform2 = val.transform;
		if (!((Object)(object)transform == (Object)(object)transform2))
		{
			return transform.IsChildOf(transform2);
		}
		return true;
	}

	private static bool ContainsCollider(IReadOnlyList<Collider2D> colliders, Collider2D target)
	{
		for (int i = 0; i < colliders.Count; i++)
		{
			if ((Object)(object)colliders[i] == (Object)(object)target)
			{
				return true;
			}
		}
		return false;
	}

	private bool TryExplode(Collider2D triggerCollider)
	{
		GameplayManager instance = SingletonComponent<GameplayManager>.Instance;
		if ((instance != null && instance.IsLevelEnterPlaying) || IsSolved || isExploding)
		{
			return false;
		}
		CollectAllExplodeTargets(triggerCollider);
		if (explodeTargets.Count == 0)
		{
			return false;
		}
		isExploding = true;
		try
		{
			VibrateUtility.Vibrate(1104);
			IGameplayObject hostGameplayObject = GetHostGameplayObject();
			GameplayManager instance2 = SingletonComponent<GameplayManager>.Instance;
			instance2?.RingBoom?.RegisterBombExplosionObjects(hostGameplayObject, explodeTargets);
			instance2?.CompleteBombExplosion(delegate
			{
				for (int i = 0; i < explodeTargets.Count; i++)
				{
					SolveTarget(explodeTargets[i], this);
				}
				SolveHostIfAttached(this);
				Solve();
			});
			AudioService.PlaySound(AudioKeys.ItemBombBoom.Value);
			return true;
		}
		finally
		{
			isExploding = false;
		}
	}

	private void CollectAllExplodeTargets(Collider2D triggerCollider)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		explodeTargets.Clear();
		IGameplayObject hostGameplayObject = GetHostGameplayObject();
		if ((Object)(object)activeCollider != (Object)null)
		{
			int num = activeCollider.Overlap(contactFilter, overlapBuffer);
			for (int i = 0; i < num && i < overlapBuffer.Length; i++)
			{
				TryAddExplodeTarget(overlapBuffer[i], hostGameplayObject);
			}
			if (explodeTargets.Count == 0)
			{
				ContactFilter2D noFilter = ContactFilter2D.noFilter;
				num = activeCollider.Overlap(noFilter, overlapBuffer);
				for (int j = 0; j < num && j < overlapBuffer.Length; j++)
				{
					TryAddExplodeTarget(overlapBuffer[j], hostGameplayObject);
				}
			}
		}
		if ((Object)(object)triggerCollider != (Object)null)
		{
			TryAddExplodeTarget(triggerCollider, hostGameplayObject);
		}
	}

	private void TryAddExplodeTarget(Collider2D col, IGameplayObject host)
	{
		if ((Object)(object)col == (Object)null)
		{
			return;
		}
		IGameplayObject gameplayObject = ResolveExplodeTarget(((Component)col).gameObject);
		if (gameplayObject == null || gameplayObject.IsSolved || gameplayObject == host)
		{
			return;
		}
		for (int i = 0; i < explodeTargets.Count; i++)
		{
			if (explodeTargets[i] == gameplayObject)
			{
				return;
			}
		}
		explodeTargets.Add(gameplayObject);
	}

	private IGameplayObject ResolveExplodeTarget(GameObject go)
	{
		if ((Object)(object)go == (Object)null)
		{
			return null;
		}
		IGameplayObject component = go.GetComponent<IGameplayObject>();
		if (component is RingObject || component is PullObject || component is FixedObject)
		{
			return component;
		}
		return go.GetComponentInParent<IGameplayObject>();
	}

	public string GetBombId()
	{
		string text = (((Object)(object)((Component)this).gameObject != (Object)null) ? ((Object)((Component)this).gameObject).name : string.Empty);
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return "bomb";
	}

	private static void SolveTarget(IGameplayObject target, BombRelative sourceBomb)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (target == null || target.IsSolved)
		{
			return;
		}
		GameplaySolveReasonContext releaseContext = GameplaySolveReasonContext.Bomb();
		if (!(target is RingObject ringObject))
		{
			if (!(target is PullObject pullObject))
			{
				if (target is FixedObject fixedObject)
				{
					fixedObject.SolveFromExternalTrigger(sourceBomb, releaseContext);
				}
			}
			else
			{
				pullObject.Solve(sourceBomb, releaseContext);
			}
		}
		else
		{
			ringObject.Solve(Vector3.zero, sourceBomb, releaseContext);
		}
	}

	private void SolveHostIfAttached(BombRelative sourceBomb)
	{
		IGameplayObject hostGameplayObject = GetHostGameplayObject();
		if (hostGameplayObject != null && !hostGameplayObject.IsSolved)
		{
			SolveTarget(hostGameplayObject, sourceBomb);
		}
	}

	private IGameplayObject GetHostGameplayObject()
	{
		return cachedHostGameplayObject;
	}

	private IGameplayObject ResolveHostGameplayObject()
	{
		if ((Object)(object)((Component)this).transform.parent == (Object)null)
		{
			return null;
		}
		RingObject componentInParent = ((Component)this).GetComponentInParent<RingObject>();
		if ((Object)(object)componentInParent != (Object)null && (Object)(object)((Component)this).transform != (Object)(object)((Component)componentInParent).transform)
		{
			return componentInParent;
		}
		PullObject componentInParent2 = ((Component)this).GetComponentInParent<PullObject>();
		if ((Object)(object)componentInParent2 != (Object)null && (Object)(object)((Component)this).transform != (Object)(object)((Component)componentInParent2).transform)
		{
			return componentInParent2;
		}
		return null;
	}
}
