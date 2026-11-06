using System.Collections.Generic;
using RotateRings.GamePlay.Objects;
using UnityEngine;
using UnlockRing.Core.Domain;

namespace RotateRings.GamePlay.Services;

public sealed class RingObjectsService : GameplayServiceBase<GameplayManager>
{
	private readonly List<RingObject> rings = new List<RingObject>();

	private readonly List<PullObject> pulls = new List<PullObject>();

	private readonly List<FixedObject> fixedObjects = new List<FixedObject>();

	private readonly List<BombRelative> bombs = new List<BombRelative>();

	private readonly List<IGameplayObject> gameplayObjects = new List<IGameplayObject>();

	private readonly List<IGameplayObject> collectableObjects = new List<IGameplayObject>();

	private readonly List<IGameplayObject> lockedObjects = new List<IGameplayObject>();

	private readonly List<IGameplayObject> randomBombSelectableObjects = new List<IGameplayObject>();

	private readonly List<IGameplayObject> bombCarryingObjects = new List<IGameplayObject>();

	private readonly Dictionary<PartId, IGameplayObject> objectsByPartId = new Dictionary<PartId, IGameplayObject>();

	private readonly Dictionary<IGameplayObject, PartId> partIdsByObject = new Dictionary<IGameplayObject, PartId>();

	private readonly HashSet<PartId> activeUnsolvedPartIds = new HashSet<PartId>();

	public IReadOnlyList<RingObject> Rings => rings;

	public IReadOnlyList<PullObject> Pulls => pulls;

	public IReadOnlyList<FixedObject> FixedObjects => fixedObjects;

	public IReadOnlyList<BombRelative> Bombs => bombs;

	public IReadOnlyList<IGameplayObject> GameplayObjects => gameplayObjects;

	public IReadOnlyList<IGameplayObject> LockedObjects => lockedObjects;

	public IReadOnlyList<IGameplayObject> RandomBombSelectableObjects => randomBombSelectableObjects;

	public IReadOnlyList<IGameplayObject> BombCarryingObjects => bombCarryingObjects;

	public IReadOnlyList<IGameplayObject> CollectableObjects => collectableObjects;

	public int RingCount => rings.Count;

	public int PullCount => pulls.Count;

	public int FixedCount => fixedObjects.Count;

	public int BombCount => bombs.Count;

	public int TotalCount => gameplayObjects.Count;

	public bool HasUnsolvedGameplayObjects => CountUnsolved(gameplayObjects) > 0;

	public IReadOnlyDictionary<IGameplayObject, PartId> LegacyObjectPartIds => partIdsByObject;

	public RingObjectsService(GameplayManager host)
		: base(host)
	{
	}

	public void AddRuntimeObject(IGameplayObject gameplayObject)
	{
		if (gameplayObject == null)
		{
			return;
		}
		MonoBehaviour val = (MonoBehaviour)((gameplayObject is MonoBehaviour) ? gameplayObject : null);
		if (val == null || !((Object)(object)val == (Object)null))
		{
			if (!gameplayObjects.Contains(gameplayObject))
			{
				gameplayObjects.Add(gameplayObject);
				RingObjectsSaveDataServices.Register(gameplayObject);
			}
			GameplayPartData partData = gameplayObject.PartData;
			if (partData.CanCollect && !collectableObjects.Contains(gameplayObject))
			{
				collectableObjects.Add(gameplayObject);
			}
			if (partData.LockCount > 0 && !lockedObjects.Contains(gameplayObject))
			{
				lockedObjects.Add(gameplayObject);
			}
			if (partData.RandomBombCanSelect && !randomBombSelectableObjects.Contains(gameplayObject))
			{
				randomBombSelectableObjects.Add(gameplayObject);
			}
			if (partData.HasBomb && !bombCarryingObjects.Contains(gameplayObject))
			{
				bombCarryingObjects.Add(gameplayObject);
			}
		}
	}

	public void CollectFromLevel(GameObject levelRoot)
	{
		ClearLevel();
		if ((Object)(object)levelRoot == (Object)null)
		{
			return;
		}
		MonoBehaviour[] componentsInChildren = levelRoot.GetComponentsInChildren<MonoBehaviour>(true);
		foreach (MonoBehaviour val in componentsInChildren)
		{
			if ((Object)(object)val == (Object)null || val is RingPaletteLockGapObject)
			{
				continue;
			}
			if (!(val is RingObject item))
			{
				if (!(val is PullObject item2))
				{
					if (!(val is FixedObject item3))
					{
						if (val is BombRelative item4)
						{
							bombs.Add(item4);
						}
					}
					else
					{
						fixedObjects.Add(item3);
					}
				}
				else
				{
					pulls.Add(item2);
				}
			}
			else
			{
				rings.Add(item);
			}
			if (val is IGameplayObject gameplayObject)
			{
				gameplayObjects.Add(gameplayObject);
				RingObjectsSaveDataServices.Register(gameplayObject);
				GameplayPartData partData = gameplayObject.PartData;
				if (partData.CanCollect)
				{
					collectableObjects.Add(gameplayObject);
				}
				if (partData.LockCount > 0)
				{
					lockedObjects.Add(gameplayObject);
				}
				if (partData.RandomBombCanSelect)
				{
					randomBombSelectableObjects.Add(gameplayObject);
				}
				if (partData.HasBomb)
				{
					bombCarryingObjects.Add(gameplayObject);
				}
			}
		}
	}

	public override void ClearLevel()
	{
		rings.Clear();
		pulls.Clear();
		fixedObjects.Clear();
		bombs.Clear();
		gameplayObjects.Clear();
		collectableObjects.Clear();
		lockedObjects.Clear();
		randomBombSelectableObjects.Clear();
		bombCarryingObjects.Clear();
		ClearPartMaps();
		activeUnsolvedPartIds.Clear();
		RingObjectsSaveDataServices.Clear();
		base.ClearLevel();
	}

	public void DestroyedReferences()
	{
		RemoveListInfos(rings);
		RemoveListInfos(pulls);
		RemoveListInfos(fixedObjects);
		RemoveListInfos(bombs);
		RemoveDestroyedObjects(gameplayObjects);
		RemoveDestroyedObjects(collectableObjects);
		RemoveDestroyedObjects(lockedObjects);
		RemoveDestroyedObjects(randomBombSelectableObjects);
		RemoveDestroyedObjects(bombCarryingObjects);
		RemovePartMaps();
	}

	private static void RemoveDestroyedObjects(List<IGameplayObject> list)
	{
		if (list == null)
		{
			return;
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			IGameplayObject gameplayObject = list[num];
			MonoBehaviour val = (MonoBehaviour)((gameplayObject is MonoBehaviour) ? gameplayObject : null);
			if (val == null || (Object)(object)val == (Object)null)
			{
				list.RemoveAt(num);
			}
		}
	}

	private void RemovePartMaps()
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		if (partIdsByObject.Count == 0)
		{
			return;
		}
		List<IGameplayObject> list = new List<IGameplayObject>();
		foreach (KeyValuePair<IGameplayObject, PartId> item in partIdsByObject)
		{
			IGameplayObject key = item.Key;
			MonoBehaviour val = (MonoBehaviour)((key is MonoBehaviour) ? key : null);
			if (val == null || (Object)(object)val == (Object)null)
			{
				list.Add(item.Key);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			IGameplayObject key2 = list[i];
			if (partIdsByObject.TryGetValue(key2, out var value))
			{
				objectsByPartId.Remove(value);
				activeUnsolvedPartIds.Remove(value);
			}
			partIdsByObject.Remove(key2);
		}
	}

	public PartId RegisterPartId(IGameplayObject gameplayObject)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		gameplayObject.TryGetPartId(out var partId);
		partIdsByObject[gameplayObject] = partId;
		objectsByPartId[partId] = gameplayObject;
		return partId;
	}

	public bool TryGetPartId(IGameplayObject gameplayObject, out PartId partId)
	{
		return partIdsByObject.TryGetValue(gameplayObject, out partId);
	}

	public bool TryGetPartObject(PartId partId, out IGameplayObject gameplayObject)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return objectsByPartId.TryGetValue(partId, out gameplayObject);
	}

	public bool TryGetRing(PartId partId, out RingObject ring)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		ring = null;
		if (!TryGetPartObject(partId, out var gameplayObject))
		{
			return false;
		}
		ring = gameplayObject as RingObject;
		return (Object)(object)ring != (Object)null;
	}

	public bool TryGetPull(PartId partId, out PullObject pull)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		pull = null;
		if (!TryGetPartObject(partId, out var gameplayObject))
		{
			return false;
		}
		pull = gameplayObject as PullObject;
		return (Object)(object)pull != (Object)null;
	}

	public bool TryGetFixed(PartId partId, out FixedObject fixedObj)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		fixedObj = null;
		if (!TryGetPartObject(partId, out var gameplayObject))
		{
			return false;
		}
		fixedObj = gameplayObject as FixedObject;
		return (Object)(object)fixedObj != (Object)null;
	}

	public void RegisterGuidePartIdBinding(PartId guidePartId, IGameplayObject gameplayObject)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (gameplayObject != null && !string.IsNullOrEmpty(((PartId)(ref guidePartId)).Value))
		{
			objectsByPartId[guidePartId] = gameplayObject;
		}
	}

	public void ClearPartMaps()
	{
		objectsByPartId.Clear();
		partIdsByObject.Clear();
	}

	public void CollectActiveUnsolvedPartIds()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		activeUnsolvedPartIds.Clear();
		for (int i = 0; i < gameplayObjects.Count; i++)
		{
			IGameplayObject gameplayObject = gameplayObjects[i];
			if (gameplayObject != null && !gameplayObject.IsSolved && TryGetPartId(gameplayObject, out var partId))
			{
				activeUnsolvedPartIds.Add(partId);
			}
		}
	}

	public bool IsActiveUnsolvedPartId(PartId partId)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return activeUnsolvedPartIds.Contains(partId);
	}

	public void ClearActiveUnsolvedPartIds()
	{
		activeUnsolvedPartIds.Clear();
	}

	public void CountAssemblyStats(bool includeSolved, out int ringNum, out int stickNum, out int bombNum, out int totalNum)
	{
		ringNum = CountObjects(rings, includeSolved);
		stickNum = CountObjects(pulls, includeSolved);
		bombNum = CountBombs(includeSolved);
		totalNum = ringNum + stickNum + bombNum;
	}

	public void CountRemainingAssemblyStats(out int ringNum, out int stickNum, out int bombNum, out int totalNum)
	{
		ringNum = CountUnsolved(rings);
		stickNum = CountUnsolved(pulls);
		bombNum = CountUnsolvedBombs();
		totalNum = ringNum + stickNum + bombNum;
	}

	public int CountUnsolvedRings()
	{
		return CountUnsolved(rings);
	}

	public int CountUnsolvedPulls()
	{
		return CountUnsolved(pulls);
	}

	public int CountUnsolvedFixedObjects()
	{
		return CountUnsolved(fixedObjects);
	}

	private int CountObjects<T>(IReadOnlyList<T> objects, bool includeSolved) where T : IGameplayObject
	{
		if (objects == null)
		{
			return 0;
		}
		if (includeSolved)
		{
			return objects.Count;
		}
		return CountUnsolved(objects);
	}

	private int CountBombs(bool includeSolved)
	{
		if (includeSolved)
		{
			return bombs.Count;
		}
		return CountUnsolvedBombs();
	}

	private int CountUnsolvedBombs()
	{
		int num = 0;
		for (int i = 0; i < bombs.Count; i++)
		{
			BombRelative bombRelative = bombs[i];
			if ((Object)(object)bombRelative != (Object)null && !bombRelative.IsSolvedForTrack)
			{
				num++;
			}
		}
		return num;
	}

	private void RemoveListInfos<T>(List<T> list) where T : MonoBehaviour
	{
		if (list == null)
		{
			return;
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if ((Object)(object)list[num] == (Object)null)
			{
				list.RemoveAt(num);
			}
		}
	}

	private int CountUnsolved<T>(IReadOnlyList<T> objects) where T : IGameplayObject
	{
		int num = 0;
		for (int i = 0; i < objects.Count; i++)
		{
			if (objects[i] != null && !objects[i].IsSolved)
			{
				num++;
			}
		}
		return num;
	}
}
