using RotateRings.GamePlay.Objects;
using UnityEngine;

public static class PhysicColliderService
{
	public static void DisableAllPhysicColliders(GameObject partGo)
	{
		if (!((Object)(object)partGo == (Object)null))
		{
			IGameplayPhysicColliders physicCollidersProvider = GetPhysicCollidersProvider(partGo);
			if (physicCollidersProvider != null)
			{
				physicCollidersProvider.DisableAllPhysicColliders();
			}
			else
			{
				SetAllCollider2DEnabled(partGo, enabled: false);
			}
		}
	}

	public static void EnableAllPhysicColliders(GameObject partGo)
	{
		if (!((Object)(object)partGo == (Object)null))
		{
			IGameplayPhysicColliders physicCollidersProvider = GetPhysicCollidersProvider(partGo);
			if (physicCollidersProvider != null)
			{
				physicCollidersProvider.EnableAllPhysicColliders();
			}
			else
			{
				SetAllCollider2DEnabled(partGo, enabled: true);
			}
		}
	}

	public static bool TryDisableAllPhysicColliders(IGameplayObject obj)
	{
		if (!(obj is IGameplayPhysicColliders gameplayPhysicColliders))
		{
			return false;
		}
		gameplayPhysicColliders.DisableAllPhysicColliders();
		return true;
	}

	public static bool TryEnableAllPhysicColliders(IGameplayObject obj)
	{
		if (!(obj is IGameplayPhysicColliders gameplayPhysicColliders))
		{
			return false;
		}
		gameplayPhysicColliders.EnableAllPhysicColliders();
		return true;
	}

	private static IGameplayPhysicColliders GetPhysicCollidersProvider(GameObject partGo)
	{
		RingObject component = partGo.GetComponent<RingObject>();
		if ((Object)(object)component != (Object)null)
		{
			return component;
		}
		PullObject component2 = partGo.GetComponent<PullObject>();
		if ((Object)(object)component2 != (Object)null)
		{
			return component2;
		}
		FixedObject component3 = partGo.GetComponent<FixedObject>();
		if ((Object)(object)component3 != (Object)null)
		{
			return component3;
		}
		RingPaletteLockGapObject component4 = partGo.GetComponent<RingPaletteLockGapObject>();
		if ((Object)(object)component4 != (Object)null)
		{
			return component4;
		}
		return null;
	}

	private static void SetAllCollider2DEnabled(GameObject partGo, bool enabled)
	{
		Collider2D[] componentsInChildren = partGo.GetComponentsInChildren<Collider2D>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if ((Object)(object)componentsInChildren[i] != (Object)null)
			{
				((Behaviour)componentsInChildren[i]).enabled = enabled;
			}
		}
	}
}
