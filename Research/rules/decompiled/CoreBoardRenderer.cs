using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
using UniLog;
using UnityEngine;
using UnityEngine.Rendering;
using UnlockRing;
using UnlockRing.Core.Authoring;
using UnlockRing.Core.Domain;
using UnlockRing.Core.Geometry;
using UnlockRing.Core.Queries;
using UnlockRing.Core.Runtime;
using UnlockRing.Game.Gameplay;
using UnlockRing.Serialization;
using XingYun.Extensions;

public class CoreBoardRenderer : MonoBehaviour, IGameCameraSource
{
	private sealed class PartView
	{
		public GameObject GameObject;

		public Vector2 PosePosition;

		public float PoseRotationRad;
	}

	private const string TAG = "[UnlockRing][CoreBoardRenderer] ";

	private const float WorldMarginFactor = 0.01f;

	[Header("Rendering")]
	[Tooltip("Triangle = filled shapes (ShapeMeshBuilder). Ribbon = ArrowRibbon path-based ribbons (ShapeRibbonBuilder).")]
	public BoardRenderMode renderMode;

	[Tooltip("Ribbon width override (0 = auto from shape). Only used in Ribbon mode.")]
	public float ribbonWidthOverride;

	[Header("Debug")]
	[SerializeField]
	private bool drawBoundsGizmo = true;

	[Header("Status")]
	public TextMeshProUGUI statusText;

	private CoreGameController _controller;

	private bool _isSolved;

	private UnifiedLevelPackReadResult _levelPackReadResult;

	private Rect _worldBounds = new Rect(-100f, -100f, 200f, 200f);

	private Aabb? _debugCoreAabb;

	private Aabb? _debugCoreAabbSweep;

	private string _cameraLevelKey;

	private GameCameraAdaptation cameraAdaptation;

	private GameBackgroundAdaptation backgroundAdaptation;

	[Header("Background")]
	[SerializeField]
	private Transform background;

	private readonly Dictionary<PartId, List<CachedRenderFeature>> _renderCache = new Dictionary<PartId, List<CachedRenderFeature>>();

	private readonly Dictionary<PartId, PartView> _partViews = new Dictionary<PartId, PartView>();

	private readonly List<GameObject> _partObjects = new List<GameObject>();

	private readonly List<Transform> _boardPartTransformsCache = new List<Transform>();

	private readonly RingOperateHighlightService operateHighlightService = new RingOperateHighlightService();

	private readonly RingLockShakeService lockShakeService = new RingLockShakeService();

	private GameObject _root;

	internal PartId? ActivePart;

	internal GestureMotionDomain? ActiveDomain;

	internal double GestureAmount;

	internal double GestureCommitAmount;

	public Camera Cam;

	private Material _defaultMat;

	public CoreGameController Controller => _controller;

	public bool IsSolved => _isSolved;

	Camera IGameCameraSource.Cam => Cam;

	public GameObject BoardRoot => _root;

	public Rect WorldBounds => _worldBounds;

	public string CameraLevelKey => _cameraLevelKey;

	private float CoordScale
	{
		get
		{
			if (renderMode != BoardRenderMode.Ribbon)
			{
				return 1f;
			}
			return 1f / 24f;
		}
	}

	public event Action<string> StatusChanged;

	public event Action<string> SelectionChanged;

	public event Action<bool> VictoryChanged;

	public event Action<IReadOnlyList<PartId>> PlayerPartsRemoved;

	internal void RaiseStatus(string s)
	{
		this.StatusChanged?.Invoke(s);
		SetStatus(s);
	}

	internal void RaiseSelection(string s)
	{
		this.SelectionChanged?.Invoke(s);
	}

	internal void RaiseVictory(bool v)
	{
		this.VictoryChanged?.Invoke(v);
	}

	public IReadOnlyList<Transform> GetAllBoardPartTransforms()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		_boardPartTransformsCache.Clear();
		if (_controller == null)
		{
			return _boardPartTransformsCache;
		}
		foreach (KeyValuePair<PartId, PartView> partView in _partViews)
		{
			if (_controller.State.Parts.TryGetValue(partView.Key, out var value) && !value.Released && !value.Removed && !((Object)(object)partView.Value.GameObject == (Object)null))
			{
				_boardPartTransformsCache.Add(partView.Value.GameObject.transform);
			}
		}
		return _boardPartTransformsCache;
	}

	private void Awake()
	{
		backgroundAdaptation = new GameBackgroundAdaptation(background, Cam);
		cameraAdaptation = new GameCameraAdaptation(this);
	}

	private void Start()
	{
		cameraAdaptation.Refresh();
	}

	private void OnDestroy()
	{
		backgroundAdaptation?.Dispose();
		backgroundAdaptation = null;
	}

	private void Update()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		cameraAdaptation.UpdateIfNeeded();
		if (ActivePart.HasValue && ActiveDomain != null && _partViews.TryGetValue(ActivePart.Value, out var value))
		{
			GestureMotionDomain activeDomain = ActiveDomain;
			double gestureAmount = GestureAmount;
			if ((int)activeDomain.MotionKind == 0)
			{
				float num = 0f - (float)((double)(value.PoseRotationRad * 57.29578f) + gestureAmount);
				value.GameObject.transform.rotation = Quaternion.Euler(0f, 0f, num);
				return;
			}
			Vector2D value2 = activeDomain.TranslationWorldAxis.Value;
			Vector2 val = default(Vector2);
			((Vector2)(ref val))._002Ector((float)((Vector2D)(ref value2)).X, (float)((Vector2D)(ref value2)).Y);
			Vector2 val2 = value.PosePosition + val * (float)gestureAmount;
			float coordScale = CoordScale;
			value.GameObject.transform.position = new Vector3(val2.x * coordScale, (0f - val2.y) * coordScale, 0f);
		}
	}

	public void LoadLevel(LevelDefinition levelDefinition, UnifiedLevelPackReadResult levelPackReadResult = null, string levelKey = null)
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		if (levelDefinition != null)
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			ClearGesture();
			_levelPackReadResult = levelPackReadResult;
			_cameraLevelKey = levelKey;
			try
			{
				_controller = new CoreGameController(levelDefinition);
			}
			catch (ArgumentException ex)
			{
				_controller = null;
				SetStatus("Core rejected: " + ex.Message);
				return;
			}
			stopwatch.Stop();
			_ = stopwatch.Elapsed.TotalMilliseconds;
			_isSolved = false;
			_worldBounds = new Rect(0f, 0f, 0f, 0f);
			stopwatch.Restart();
			RebuildRenderCache();
			stopwatch.Stop();
			_ = stopwatch.Elapsed.TotalMilliseconds;
			stopwatch.Restart();
			cameraAdaptation.BindFromUIGame();
			stopwatch.Stop();
			_ = stopwatch.Elapsed.TotalMilliseconds;
			((Component)this).GetComponent<SimpleGestureInput>()?.BeginDomainBuild();
		}
	}

	public void ReleaseLoadedLevel()
	{
		ClearGesture();
		ClearPartObjects();
		_renderCache.Clear();
		_controller = null;
		_isSolved = false;
		_levelPackReadResult = null;
		_cameraLevelKey = null;
		if ((Object)(object)_root != (Object)null)
		{
			Object.Destroy((Object)(object)_root);
			_root = null;
		}
	}

	[ContextMenu("Toggle Render Mode")]
	public void ToggleRenderMode()
	{
		renderMode = ((renderMode == BoardRenderMode.Triangle) ? BoardRenderMode.Ribbon : BoardRenderMode.Triangle);
		SetStatus($"Render mode: {renderMode}");
		RebuildRenderCache();
	}

	public void SetRenderMode(BoardRenderMode mode)
	{
		if (renderMode != mode)
		{
			renderMode = mode;
			SetStatus($"Render mode: {mode}");
			RebuildRenderCache();
		}
	}

	internal Vector3 CorePointToUnity(Vector2D corePoint, float zOffset = -0.01f)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		float coordScale = CoordScale;
		return new Vector3((float)((Vector2D)(ref corePoint)).X * coordScale, (0f - (float)((Vector2D)(ref corePoint)).Y) * coordScale, zOffset);
	}

	internal float PixelsPerWorld()
	{
		Camera cam = Cam;
		if ((Object)(object)cam == (Object)null || Screen.height == 0)
		{
			return 1f;
		}
		return (float)Screen.height / Mathf.Max(cam.orthographicSize * 2f, 1E-06f);
	}

	internal Vector2 ScreenToWorld(Vector2 screen)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		Camera cam = Cam;
		if ((Object)(object)cam == (Object)null)
		{
			return screen;
		}
		Ray val = cam.ScreenPointToRay(new Vector3(screen.x, screen.y, 0f));
		float z = ((Ray)(ref val)).direction.z;
		float num = (Mathf.Approximately(z, 0f) ? 0f : ((0f - ((Ray)(ref val)).origin.z) / z));
		Vector3 val2 = ((Ray)(ref val)).origin + ((Ray)(ref val)).direction * num;
		float num2 = ((CoordScale > 0f) ? (1f / CoordScale) : 1f);
		return new Vector2(val2.x * num2, (0f - val2.y) * num2);
	}

	internal double GetDefaultHitSlopWorld(double hitSlopPixels = 8.0)
	{
		return hitSlopPixels / (double)Math.Max(PixelsPerWorld(), 1E-06f);
	}

	internal PartId? HitTestWorld(Vector2 world)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (_controller == null)
		{
			return null;
		}
		double defaultHitSlopWorld = GetDefaultHitSlopWorld();
		return CoreHitTester.HitTest(_controller.Level, _controller.State, new Vector2D((double)world.x, (double)world.y), defaultHitSlopWorld);
	}

	internal bool ApplyForceRemoveResult(ForceRemoveResult result)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		if (result == null || (int)result.Status != 0 || _controller == null)
		{
			return false;
		}
		RemoveParts(result.Delta.ReleasedParts);
		RemoveParts(result.Delta.RemovedParts);
		_isSolved = CoreQueries.IsSolved(_controller.Level, _controller.State).IsSolved;
		RaiseVictory(_isSolved);
		return true;
	}

	internal void RebuildRenderCache()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		ClearPartObjects();
		_renderCache.Clear();
		if (_controller != null)
		{
			if ((Object)(object)_root == (Object)null)
			{
				_root = new GameObject("Board");
				_root.transform.SetParent((Transform)null, false);
				_root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
			}
			Aabb? val = null;
			Aabb? val2 = null;
			if (renderMode != BoardRenderMode.Ribbon || !(_levelPackReadResult != (UnifiedLevelPackReadResult)null))
			{
				(val, val2) = RebuildFromPartDefinitions();
			}
			else
			{
				(val, val2) = RebuildRibbonFromDrafts();
			}
			Aabb value = (Aabb)(((_003F?)val) ?? new Aabb(new Vector2(-100.0, -100.0), new Vector2(100.0, 100.0)));
			_debugCoreAabb = value;
			_debugCoreAabbSweep = val2;
			_worldBounds = BoardRenderUtils.WorldRectFromAabb(value, 0.01f, CoordScale);
			_isSolved = CoreQueries.IsSolved(_controller.Level, _controller.State).IsSolved;
		}
	}

	private static Aabb? UnionBounds(Aabb? current, Aabb candidate)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		Aabb value;
		if (!current.HasValue)
		{
			value = candidate;
		}
		else
		{
			Aabb value2 = current.Value;
			value = ((Aabb)(ref value2)).Union(candidate);
		}
		return value;
	}

	private (Aabb? bounds, Aabb? sweepBounds) RebuildFromPartDefinitions()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		Aabb? val = null;
		Aabb? val2 = null;
		foreach (PartDefinition part in _controller.Level.Parts)
		{
			if (_controller.State.Parts.TryGetValue(part.Id, out var value) && !value.Released && !value.Removed)
			{
				Aabb? partBounds;
				List<CachedRenderFeature> list = BoardRenderUtils.CollectFeatures(part, value, out partBounds);
				_renderCache[part.Id] = list;
				if (partBounds.HasValue)
				{
					BuildPartObject(part, value, list);
					Aabb candidate = BoardRenderUtils.ComputeRequiredBounds(part, value, list, partBounds.Value);
					val = UnionBounds(val, partBounds.Value);
					val2 = UnionBounds(val2, candidate);
				}
			}
		}
		return (bounds: val, sweepBounds: val2);
	}

	private (Aabb? bounds, Aabb? sweepBounds) RebuildRibbonFromDrafts()
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		UnifiedLevelPackReadResult levelPackReadResult = _levelPackReadResult;
		if (((levelPackReadResult != null) ? levelPackReadResult.Drafts : null) == null || _levelPackReadResult.Drafts.Count == 0)
		{
			return RebuildFromPartDefinitions();
		}
		EditableLevelDraft val = _levelPackReadResult.Drafts[0];
		LevelComponentFactory.CoordSpace cs = new LevelComponentFactory.CoordSpace(CoordScale);
		Aabb? val2 = null;
		Aabb? val3 = null;
		for (int i = 0; i < val.Parts.Count; i++)
		{
			EditablePartDraft val4 = val.Parts[i];
			PartId partId = new PartId(val4.Id);
			if (!_controller.State.Parts.TryGetValue(partId, out var value) || value.Released || value.Removed)
			{
				continue;
			}
			LevelComponentFactory.CreateResult createResult = LevelComponentFactory.CreatePart(val4, i, _root.transform, cs);
			if ((Object)(object)createResult.GameObject == (Object)null)
			{
				continue;
			}
			_partObjects.Add(createResult.GameObject);
			Dictionary<PartId, PartView> partViews = _partViews;
			PartId key = partId;
			PartView obj = new PartView
			{
				GameObject = createResult.GameObject
			};
			Pose2D pose = value.Pose;
			Vector2D position = ((Pose2D)(ref pose)).Position;
			float num = (float)((Vector2D)(ref position)).X;
			pose = value.Pose;
			position = ((Pose2D)(ref pose)).Position;
			obj.PosePosition = new Vector2(num, (float)((Vector2D)(ref position)).Y);
			pose = value.Pose;
			obj.PoseRotationRad = (float)((Pose2D)(ref pose)).RotationRadians;
			partViews[key] = obj;
			PartDefinition val5 = ((IEnumerable<PartDefinition>)_controller.Level.Parts).FirstOrDefault((Func<PartDefinition, bool>)delegate(PartDefinition p)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000a: Unknown result type (might be due to invalid IL or missing references)
				PartId id = p.Id;
				return ((PartId)(ref id)).Equals(partId);
			});
			if (val5 != null)
			{
				Aabb? partBounds;
				List<CachedRenderFeature> list = BoardRenderUtils.CollectFeatures(val5, value, out partBounds);
				_renderCache[partId] = list;
				if (partBounds.HasValue)
				{
					Aabb candidate = BoardRenderUtils.ComputeRequiredBounds(val5, value, list, partBounds.Value);
					val2 = UnionBounds(val2, partBounds.Value);
					val3 = UnionBounds(val3, candidate);
					continue;
				}
			}
			if (createResult.Bounds.HasValue)
			{
				val2 = UnionBounds(val2, createResult.Bounds.Value);
				val3 = UnionBounds(val3, createResult.Bounds.Value);
			}
		}
		return (bounds: val2, sweepBounds: val3);
	}

	internal void RemoveParts(IReadOnlyList<PartId> parts)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		if (parts == null || parts.Count == 0)
		{
			return;
		}
		List<PartId> list = new List<PartId>();
		foreach (PartId partId in parts)
		{
			if (_partViews.TryGetValue(partId, out var value))
			{
				CoreGameController controller = _controller;
				PartDefinition val = ((controller != null) ? ((IEnumerable<PartDefinition>)controller.Level.Parts).FirstOrDefault((Func<PartDefinition, bool>)delegate(PartDefinition part)
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					//IL_0006: Unknown result type (might be due to invalid IL or missing references)
					//IL_000a: Unknown result type (might be due to invalid IL or missing references)
					PartId id = part.Id;
					return ((PartId)(ref id)).Equals(partId);
				}) : null);
				if (val != null && BoardRenderUtils.IsPlayerPart(val))
				{
					list.Add(partId);
				}
				GameObject gameObject = value.GameObject;
				if ((Object)(object)gameObject != (Object)null)
				{
					_partObjects.Remove(gameObject);
					RemovePartWithEffect(gameObject);
				}
				operateHighlightService.Remove(gameObject);
				_partViews.Remove(partId);
				_renderCache.Remove(partId);
			}
		}
		if (list.Count > 0)
		{
			this.PlayerPartsRemoved?.Invoke(list);
		}
		if (_controller != null)
		{
			_isSolved = CoreQueries.IsSolved(_controller.Level, _controller.State).IsSolved;
		}
	}

	private void RemovePartWithEffect(GameObject partGo)
	{
		MeshFilter val = partGo.GetComponent<MeshFilter>() ?? partGo.GetComponentInChildren<MeshFilter>();
		Mesh val2 = (((Object)(object)val != (Object)null) ? val.sharedMesh : null);
		if ((Object)(object)val2 == (Object)null)
		{
			Object.Destroy((Object)(object)partGo);
			return;
		}
		MeshRenderer[] componentsInChildren = partGo.GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			((Renderer)componentsInChildren[i]).enabled = false;
		}
		RingRemoveEffectEvent ringRemoveEffectEvent = new RingRemoveEffectEvent
		{
			SourceMesh = val2,
			SourceTransform = partGo.transform,
			PartGo = partGo,
			EffectParent = (((Object)(object)_root != (Object)null) ? _root.transform : null)
		};
		GameApp.Event.Send<RingRemoveEffectEvent>(ringRemoveEffectEvent);
		if (!ringRemoveEffectEvent.Handled)
		{
			Object.Destroy((Object)(object)partGo);
		}
	}

	internal void SyncChangedParts(IReadOnlyList<PartId> changedParts)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		if (_controller == null || changedParts == null || changedParts.Count == 0)
		{
			return;
		}
		float coordScale = CoordScale;
		foreach (PartId changedPart in changedParts)
		{
			if (_controller.State.Parts.TryGetValue(changedPart, out var value) && !value.Released && !value.Removed && _partViews.TryGetValue(changedPart, out var value2))
			{
				PartView partView = value2;
				Pose2D pose = value.Pose;
				Vector2D position = ((Pose2D)(ref pose)).Position;
				float num = (float)((Vector2D)(ref position)).X;
				pose = value.Pose;
				position = ((Pose2D)(ref pose)).Position;
				partView.PosePosition = new Vector2(num, (float)((Vector2D)(ref position)).Y);
				PartView partView2 = value2;
				pose = value.Pose;
				partView2.PoseRotationRad = (float)((Pose2D)(ref pose)).RotationRadians;
				Transform transform = value2.GameObject.transform;
				pose = value.Pose;
				position = ((Pose2D)(ref pose)).Position;
				float num2 = (float)((Vector2D)(ref position)).X * coordScale;
				pose = value.Pose;
				position = ((Pose2D)(ref pose)).Position;
				transform.position = new Vector3(num2, (0f - (float)((Vector2D)(ref position)).Y) * coordScale, 0f);
				Transform transform2 = value2.GameObject.transform;
				pose = value.Pose;
				transform2.rotation = Quaternion.Euler(0f, 0f, 0f - (float)(((Pose2D)(ref pose)).RotationRadians * 57.295780181884766));
			}
		}
	}

	private void BuildPartObject(PartDefinition part, PartState state, List<CachedRenderFeature> features)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Expected O, but got Unknown
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Expected O, but got Unknown
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_031f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		Color val = (BoardRenderUtils.IsFixedConstraintPart(part) ? RingTheme.Fixed : PartPalette.For(part.AuthoredOrder, (!BoardRenderUtils.IsSliding(part)) ? 1 : 10));
		PartId id = part.Id;
		GameObject val2 = new GameObject("Part_" + ((PartId)(ref id)).Value);
		val2.transform.SetParent(_root.transform, false);
		Transform transform = val2.transform;
		Pose2D pose = state.Pose;
		Vector2D position = ((Pose2D)(ref pose)).Position;
		float num = (float)((Vector2D)(ref position)).X * CoordScale;
		pose = state.Pose;
		position = ((Pose2D)(ref pose)).Position;
		transform.position = new Vector3(num, (0f - (float)((Vector2D)(ref position)).Y) * CoordScale, 0f);
		Transform transform2 = val2.transform;
		pose = state.Pose;
		transform2.rotation = Quaternion.Euler(0f, 0f, 0f - (float)(((Pose2D)(ref pose)).RotationRadians * 57.295780181884766));
		pose = state.Pose;
		position = ((Pose2D)(ref pose)).Position;
		float num2 = (float)((Vector2D)(ref position)).X;
		pose = state.Pose;
		position = ((Pose2D)(ref pose)).Position;
		Vector2 val3 = default(Vector2);
		((Vector2)(ref val3))._002Ector(num2, (float)((Vector2D)(ref position)).Y);
		pose = state.Pose;
		float num3 = (float)((Pose2D)(ref pose)).RotationRadians;
		if (renderMode == BoardRenderMode.Ribbon)
		{
			List<GameObject> list = RibbonPartBuilder.BuildPartFeatures(part, state, features, val2.transform, val, CoordScale);
			_partObjects.Add(val2);
			foreach (GameObject item in list)
			{
				if ((Object)(object)item != (Object)null)
				{
					_partObjects.Add(item);
				}
			}
		}
		else
		{
			MeshFilter val4 = val2.AddComponent<MeshFilter>();
			MeshRenderer obj = val2.AddComponent<MeshRenderer>();
			Material val5 = new Material(GetDefaultMaterial());
			val5.color = val;
			((Renderer)obj).sharedMaterial = val5;
			((Renderer)obj).shadowCastingMode = (ShadowCastingMode)0;
			((Renderer)obj).receiveShadows = false;
			CombineInstance[] array = (CombineInstance[])(object)new CombineInstance[features.Count];
			int num4 = 0;
			for (int i = 0; i < features.Count; i++)
			{
				if (BoardRenderUtils.IsSliding(part))
				{
					FeatureId id2 = features[i].Feature.Id;
					if (((FeatureId)(ref id2)).Value.Equals("guide", StringComparison.Ordinal))
					{
						continue;
					}
				}
				Mesh val6 = ShapeMeshBuilder.Build(features[i].Shape);
				if (!((Object)(object)val6 == (Object)null))
				{
					BoardRenderUtils.BakeToLocal(val6, val3, num3);
					Color val7 = BoardRenderUtils.FeatureColor(features[i], val);
					Color[] array2 = (Color[])(object)new Color[val6.vertexCount];
					for (int j = 0; j < array2.Length; j++)
					{
						array2[j] = val7;
					}
					val6.SetColors(array2);
					((CombineInstance)(ref array[num4])).mesh = val6;
					num4++;
				}
			}
			Mesh val8 = new Mesh();
			id = part.Id;
			((Object)val8).name = ((PartId)(ref id)).Value;
			Mesh val9 = val8;
			if (num4 > 0)
			{
				Array.Resize(ref array, num4);
				val9.CombineMeshes(array, true, false, false);
			}
			val4.sharedMesh = val9;
			_partObjects.Add(val2);
		}
		_partViews[part.Id] = new PartView
		{
			GameObject = val2,
			PosePosition = val3,
			PoseRotationRad = num3
		};
	}

	private Material GetDefaultMaterial()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		if ((Object)(object)_defaultMat == (Object)null)
		{
			Shader val = Shader.Find("Sprites/Default") ?? Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default") ?? Shader.Find("Universal Render Pipeline/Unlit");
			_defaultMat = new Material(val)
			{
				color = Color.white
			};
		}
		return _defaultMat;
	}

	private void ClearPartObjects()
	{
		operateHighlightService.ClearAll();
		foreach (GameObject partObject in _partObjects)
		{
			if ((Object)(object)partObject != (Object)null)
			{
				Object.Destroy((Object)(object)partObject);
			}
		}
		_partObjects.Clear();
		_partViews.Clear();
		_boardPartTransformsCache.Clear();
	}

	internal bool TryGetPartGameObject(PartId partId, out GameObject gameObject)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (_partViews.TryGetValue(partId, out var value) && (Object)(object)value.GameObject != (Object)null)
		{
			gameObject = value.GameObject;
			return true;
		}
		gameObject = null;
		return false;
	}

	internal bool TryGetPartCenterWorldPosition(PartId partId, out Vector3 worldPosition)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		if (_partViews.TryGetValue(partId, out var value) && (Object)(object)value.GameObject != (Object)null)
		{
			worldPosition = value.GameObject.transform.position;
			if (_renderCache.TryGetValue(partId, out var value2) && value2.Count > 0)
			{
				Aabb? val = null;
				Aabb value4;
				for (int i = 0; i < value2.Count; i++)
				{
					Aabb value3;
					if (!val.HasValue)
					{
						value3 = value2[i].Shape.Bounds;
					}
					else
					{
						value4 = val.Value;
						value3 = ((Aabb)(ref value4)).Union(value2[i].Shape.Bounds);
					}
					val = value3;
				}
				if (val.HasValue)
				{
					value4 = val.Value;
					Vector2 min = ((Aabb)(ref value4)).Min;
					value4 = val.Value;
					Vector2 max = ((Aabb)(ref value4)).Max;
					worldPosition = new Vector3((float)((((Vector2)(ref min)).X + ((Vector2)(ref max)).X) * 0.5 * (double)CoordScale), 0f - (float)((((Vector2)(ref min)).Y + ((Vector2)(ref max)).Y) * 0.5 * (double)CoordScale), worldPosition.z);
				}
			}
			return true;
		}
		worldPosition = default(Vector3);
		return false;
	}

	internal bool TryGetPartWorldPosition(PartId partId, out Vector3 worldPosition)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (_partViews.TryGetValue(partId, out var value) && (Object)(object)value.GameObject != (Object)null)
		{
			worldPosition = value.GameObject.transform.position;
			return true;
		}
		worldPosition = default(Vector3);
		return false;
	}

	internal void PlayBlockedShake(PartId partId)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (_partViews.TryGetValue(partId, out var value) && !((Object)(object)value.GameObject == (Object)null))
		{
			lockShakeService.PlayLockShakeService(value.GameObject.transform, value.GameObject);
		}
	}

	internal void ClearGesture()
	{
		SetPartOperateHighlight(ActivePart, highlight: false);
		ActivePart = null;
		ActiveDomain = null;
		GestureAmount = 0.0;
		GestureCommitAmount = 0.0;
	}

	internal void ClearOperateHighlights()
	{
		operateHighlightService.ClearAll();
	}

	internal void SetPartOperateHighlight(PartId? partId, bool highlight)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		if (!partId.HasValue)
		{
			return;
		}
		if (!_partViews.TryGetValue(partId.Value, out var value) || (Object)(object)value.GameObject == (Object)null)
		{
			if (highlight)
			{
				Log.LogError("[UnlockRing][CoreBoardRenderer] ", "SetPartOperateHighlight 找不到部件 part={0}", new object[1] { partId.Value });
			}
		}
		else if (!operateHighlightService.TrySetHighlight(value.GameObject, highlight) && highlight)
		{
			Log.LogError("[UnlockRing][CoreBoardRenderer] ", "RingOperateHighlight 创建失败 part={0} go={1}", new object[2]
			{
				partId.Value,
				((Object)value.GameObject).name
			});
		}
	}

	internal void SetStatus(string msg)
	{
		if ((Object)(object)statusText != (Object)null)
		{
			TMPTextExtensions.SetLocalizedTextValue((TMP_Text)(object)statusText, msg);
		}
	}
}
