using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniLog;
using UnityEngine;
using UnityEngine.Rendering;
using UnlockRing.Core.Authoring;
using UnlockRing.Core.Domain;
using UnlockRing.Core.Geometry;
using UnlockRing.Core.Runtime;
using UnlockRing.Game.Gameplay;
using UnlockRing.Serialization;
using XingYun.Extensions;

namespace UnlockRing;

public class RenderModeComparator : MonoBehaviour
{
	private const string TAG = "[UnlockRing][RenderModeComparator] ";

	private const float WorldMarginFactor = 0.06f;

	private const float TriangleAlpha = 0.5f;

	[Header("Camera")]
	[Tooltip("主相机（渲染 Triangle 层）。如果为空则自动创建。")]
	public Camera triangleCam;

	[Header("Data")]
	[Tooltip("PlayableLevelPack 二进制资源。")]
	public TextAsset levelPack;

	[Header("Display")]
	[Tooltip("状态文本（可选）")]
	public TextMeshProUGUI statusText;

	private readonly List<GameObject> _allObjects = new List<GameObject>();

	private GameObject _triangleRoot;

	private GameObject _ribbonRoot;

	private Camera _ribbonCam;

	private Rect _triangleBounds;

	private Rect _ribbonBounds;

	private static readonly LevelComponentFactory.CoordSpace TriangleSpace = new LevelComponentFactory.CoordSpace(1f);

	private static readonly LevelComponentFactory.CoordSpace RibbonSpace = new LevelComponentFactory.CoordSpace(1f / 24f);

	private void Start()
	{
		if ((Object)(object)levelPack == (Object)null)
		{
			SetStatus("未指定 levelPack (TextAsset)");
			return;
		}
		try
		{
			UnifiedLevelPackReadResult levelPackReadResult = UnifiedLevelPackReader.Read(levelPack.text);
			LoadAndRender(levelPackReadResult);
		}
		catch (Exception ex)
		{
			SetStatus("加载失败: " + ex.Message);
		}
	}

	public static LevelDefinition ReadLevelDefinition(UnifiedLevelPackReadResult levelPackReadResult)
	{
		if (levelPackReadResult.Drafts.Count > 0)
		{
			AuthoringCompilationResult val = EditableLevelDraftAuthoringAdapter.Compile(levelPackReadResult.Drafts[0], false);
			if (!val.IsSuccess)
			{
				AuthoringDiagnostic val2 = val.Diagnostics.FirstOrDefault();
				Log.LogError("Level", "Level compilation failed." + val2.Path + ": " + val2.Message);
				return null;
			}
			LevelDefinition val3 = EditableLevelDraftAuthoringAdapter.InferReleasableParts(val.Level);
			ValidationError[] array = (from error in val3.Validate()
				where (int)error.Severity == 0
				select error).ToArray();
			if (array.Length != 0)
			{
				ValidationError val4 = array[0];
				Log.LogError("Level", val4.Message);
				return null;
			}
			InitialValidationResult val5 = CoreRuntime.Initialize(val3);
			if (!val5.IsValid)
			{
				ValidationError val6 = val5.Errors.FirstOrDefault();
				Log.LogError("Level", "runtime.initialize.failed", new object[2]
				{
					"$",
					(val6 == null) ? "CoreRuntime.Initialize did not produce an exact playable state." : (val6.Path + ": " + val6.Message)
				});
				return null;
			}
			return val3;
		}
		return null;
	}

	public void LoadAndRender(UnifiedLevelPackReadResult levelPackReadResult)
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		if (!(levelPackReadResult == (UnifiedLevelPackReadResult)null))
		{
			ClearAll();
			LevelDefinition level = ReadLevelDefinition(levelPackReadResult);
			BoardState state;
			try
			{
				CoreGameController coreGameController = new CoreGameController(level);
				level = coreGameController.Level;
				state = coreGameController.State;
			}
			catch (ArgumentException ex)
			{
				SetStatus("Core rejected: " + ex.Message);
				return;
			}
			EnsureRoots();
			EnsureCameras();
			BuildLayer(level, state, _triangleRoot.transform, BoardRenderMode.Triangle, TriangleSpace, out _triangleBounds);
			BuildLayerByLevelPackReadResult(levelPackReadResult, state, _ribbonRoot.transform, BoardRenderMode.Ribbon, RibbonSpace, out _ribbonBounds);
			_ribbonRoot.transform.localScale = Vector3.one * RibbonSpace.InverseScale;
			FrameCameras();
			SetStatus("Overlay 对比渲染就绪 — Ribbon 底层 + Triangle 半透明叠加");
		}
	}

	private void EnsureRoots()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_triangleRoot == (Object)null)
		{
			_triangleRoot = new GameObject("Comparator_TriangleRoot");
			_triangleRoot.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
		}
		if ((Object)(object)_ribbonRoot == (Object)null)
		{
			_ribbonRoot = new GameObject("Comparator_RibbonRoot");
			_ribbonRoot.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
		}
	}

	private void EnsureCameras()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Expected O, but got Unknown
		if ((Object)(object)triangleCam == (Object)null)
		{
			GameObject val = new GameObject("Comparator_TriangleCam");
			triangleCam = val.AddComponent<Camera>();
		}
		triangleCam.orthographic = true;
		triangleCam.clearFlags = (CameraClearFlags)2;
		triangleCam.backgroundColor = RingTheme.Surface;
		triangleCam.nearClipPlane = -100f;
		triangleCam.farClipPlane = 100f;
		triangleCam.depth = 1f;
		if ((Object)(object)_ribbonCam == (Object)null)
		{
			GameObject val2 = new GameObject("Comparator_RibbonCam");
			_ribbonCam = val2.AddComponent<Camera>();
		}
		_ribbonCam.orthographic = true;
		_ribbonCam.clearFlags = (CameraClearFlags)2;
		_ribbonCam.backgroundColor = new Color(0.85f, 0.88f, 0.92f, 1f);
		_ribbonCam.nearClipPlane = -100f;
		_ribbonCam.farClipPlane = 100f;
		_ribbonCam.depth = 0f;
	}

	private void BuildLayerByLevelPackReadResult(UnifiedLevelPackReadResult levelPackReadResult, BoardState state, Transform parent, BoardRenderMode mode, LevelComponentFactory.CoordSpace cs, out Rect worldBounds)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		worldBounds = default(Rect);
		if (levelPackReadResult.Drafts.Count <= 0)
		{
			return;
		}
		EditableLevelDraft val = levelPackReadResult.Drafts[0];
		Aabb? val2 = null;
		for (int i = 0; i < val.Parts.Count; i++)
		{
			LevelComponentFactory.CreateResult createResult = LevelComponentFactory.CreatePart(val.Parts[i], i, parent, cs);
			if ((Object)(object)createResult.GameObject != (Object)null)
			{
				_allObjects.Add(createResult.GameObject);
			}
			if (createResult.Bounds.HasValue)
			{
				Aabb value;
				if (!val2.HasValue)
				{
					value = createResult.Bounds.Value;
				}
				else
				{
					Aabb value2 = val2.Value;
					value = ((Aabb)(ref value2)).Union(createResult.Bounds.Value);
				}
				val2 = value;
			}
		}
		Aabb value3 = (Aabb)(((_003F?)val2) ?? new Aabb(new Vector2(-100.0, -100.0), new Vector2(100.0, 100.0)));
		worldBounds = BoardRenderUtils.WorldRectFromAabb(value3, 0.06f, cs.Scale);
	}

	private void BuildLayer(LevelDefinition level, BoardState state, Transform parent, BoardRenderMode mode, LevelComponentFactory.CoordSpace cs, out Rect worldBounds)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Expected O, but got Unknown
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		Aabb? val = null;
		Vector2 posePos = default(Vector2);
		foreach (PartDefinition part in level.Parts)
		{
			if (!state.Parts.TryGetValue(part.Id, out var value) || value.Released || value.Removed)
			{
				continue;
			}
			Aabb? partBounds;
			List<CachedRenderFeature> features = BoardRenderUtils.CollectFeatures(part, value, out partBounds);
			if (!partBounds.HasValue)
			{
				continue;
			}
			Color baseColor = (BoardRenderUtils.IsFixedConstraintPart(part) ? RingTheme.Fixed : PartPalette.For(part.AuthoredOrder, (!BoardRenderUtils.IsSliding(part)) ? 1 : 10));
			Pose2D pose = value.Pose;
			Vector2D position = ((Pose2D)(ref pose)).Position;
			float num = (float)((Vector2D)(ref position)).X;
			pose = value.Pose;
			position = ((Pose2D)(ref pose)).Position;
			((Vector2)(ref posePos))._002Ector(num, (float)((Vector2D)(ref position)).Y);
			pose = value.Pose;
			float num2 = (float)((Pose2D)(ref pose)).RotationRadians;
			PartId id = part.Id;
			GameObject val2 = new GameObject("Part_" + ((PartId)(ref id)).Value);
			val2.transform.SetParent(parent, false);
			Transform transform = val2.transform;
			pose = value.Pose;
			position = ((Pose2D)(ref pose)).Position;
			double x = ((Vector2D)(ref position)).X;
			pose = value.Pose;
			position = ((Pose2D)(ref pose)).Position;
			transform.position = Vector2.op_Implicit(cs.ToUnityPosition(x, ((Vector2D)(ref position)).Y));
			val2.transform.rotation = Quaternion.Euler(0f, 0f, (0f - num2) * 57.29578f);
			if (mode == BoardRenderMode.Ribbon)
			{
				List<GameObject> list = RibbonPartBuilder.BuildPartFeatures(part, value, features, val2.transform, baseColor, cs.Scale);
				_allObjects.Add(val2);
				foreach (GameObject item in list)
				{
					if ((Object)(object)item != (Object)null)
					{
						_allObjects.Add(item);
					}
				}
			}
			else
			{
				BuildTriangleMesh(val2, part, features, posePos, num2, baseColor);
				_allObjects.Add(val2);
			}
			Aabb val3 = BoardRenderUtils.ComputeRequiredBounds(part, value, features, partBounds.Value);
			Aabb value2;
			if (!val.HasValue)
			{
				value2 = val3;
			}
			else
			{
				Aabb value3 = val.Value;
				value2 = ((Aabb)(ref value3)).Union(val3);
			}
			val = value2;
		}
		Aabb value4 = (Aabb)(((_003F?)val) ?? new Aabb(new Vector2(-100.0, -100.0), new Vector2(100.0, 100.0)));
		worldBounds = BoardRenderUtils.WorldRectFromAabb(value4, 0.06f, cs.Scale);
	}

	private void BuildTriangleMesh(GameObject go, PartDefinition part, List<CachedRenderFeature> features, Vector2 posePos, float poseRotRad, Color baseColor)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Expected O, but got Unknown
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		MeshFilter val = go.AddComponent<MeshFilter>();
		MeshRenderer obj = go.AddComponent<MeshRenderer>();
		Material val2 = new Material(Shader.Find("Sprites/Default") ?? Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default") ?? Shader.Find("Universal Render Pipeline/Unlit"));
		val2.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.5f);
		((Renderer)obj).sharedMaterial = val2;
		((Renderer)obj).shadowCastingMode = (ShadowCastingMode)0;
		((Renderer)obj).receiveShadows = false;
		CombineInstance[] array = (CombineInstance[])(object)new CombineInstance[features.Count];
		int num = 0;
		for (int i = 0; i < features.Count; i++)
		{
			if (BoardRenderUtils.IsSliding(part))
			{
				FeatureId id = features[i].Feature.Id;
				if (((FeatureId)(ref id)).Value.Equals("guide", StringComparison.Ordinal))
				{
					continue;
				}
			}
			Mesh val3 = ShapeMeshBuilder.Build(features[i].Shape);
			if (!((Object)(object)val3 == (Object)null))
			{
				BoardRenderUtils.BakeToLocal(val3, posePos, poseRotRad);
				Color val4 = BoardRenderUtils.FeatureColor(features[i], baseColor);
				val4.a = 0.5f;
				Color[] array2 = (Color[])(object)new Color[val3.vertexCount];
				for (int j = 0; j < array2.Length; j++)
				{
					array2[j] = val4;
				}
				val3.SetColors(array2);
				((CombineInstance)(ref array[num])).mesh = val3;
				num++;
			}
		}
		Mesh val5 = new Mesh();
		PartId id2 = part.Id;
		((Object)val5).name = ((PartId)(ref id2)).Value;
		Mesh val6 = val5;
		if (num > 0)
		{
			Array.Resize(ref array, num);
			val6.CombineMeshes(array, true, false, false);
		}
		val.sharedMesh = val6;
	}

	private void FrameCameras()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		float aspect = ((Screen.width > 0 && Screen.height > 0) ? ((float)Screen.width / (float)Screen.height) : 1.7777778f);
		FrameCamera(triangleCam, _triangleBounds, aspect);
		FrameCamera(_ribbonCam, _ribbonBounds, aspect);
	}

	private static void FrameCamera(Camera cam, Rect bounds, float aspect)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)cam == (Object)null))
		{
			((Component)cam).transform.position = new Vector3(((Rect)(ref bounds)).center.x, ((Rect)(ref bounds)).center.y, -10f);
			cam.orthographicSize = BoardRenderUtils.OrthoSizeFor(bounds, aspect);
		}
	}

	public void ClearAll()
	{
		foreach (GameObject allObject in _allObjects)
		{
			if ((Object)(object)allObject != (Object)null)
			{
				Object.Destroy((Object)(object)allObject);
			}
		}
		_allObjects.Clear();
	}

	private void OnDestroy()
	{
		ClearAll();
		if ((Object)(object)_ribbonCam != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)_ribbonCam).gameObject);
		}
	}

	private void SetStatus(string msg)
	{
		if ((Object)(object)statusText != (Object)null)
		{
			TMPTextExtensions.SetLocalizedTextValue((TMP_Text)(object)statusText, msg);
		}
	}
}
