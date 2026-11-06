using System;
using System.Collections.Generic;
using UnityEngine;
using XingYun.Rendering;

namespace XingYun.RingGame;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public sealed class RingSegmentController : MonoBehaviour
{
	private RingSegmentBuildSettings _buildSettings = RingSegmentBuildSettings.Default;

	[SerializeField]
	private Material _sharedMaterial;

	[SerializeField]
	private float _radius = 2f;

	[SerializeField]
	private float _width = 1f;

	[Tooltip("Gap opening angle in degrees. 0 = full ring (no gap).")]
	[Range(0f, 360f)]
	[SerializeField]
	private float _gapAngleDeg = 60f;

	[Tooltip("Direction the gap faces, in degrees.")]
	[Range(0f, 360f)]
	[SerializeField]
	private float _gapDirectionDeg;

	[Tooltip("0 = full arc (original). 1 = entire tail/head replaced by flat quad.")]
	[SerializeField]
	[Range(0f, 1f)]
	private float _capRatio;

	[SerializeField]
	private int _styleIndex;

	[SerializeField]
	private int _arcSubdivisions = 32;

	[Header("Vertex Color")]
	[SerializeField]
	private Color _vertexColor = Color.white;

	[Header("Attachments")]
	[SerializeField]
	private List<RingAttachment> _attachments = new List<RingAttachment>();

	[Header("Extra")]
	[SerializeField]
	private bool _extraEnabled;

	[Header("Runtime Mesh")]
	[Tooltip("勾选后 Awake/OnEnable 复用预制体已烘焙的 Mesh，跳过首次 Rebuild。运行时 SetZLayerOffsets / SetCrossBarZ 直接平移对应部件的顶点 Z，不再重建 mesh。")]
	[SerializeField]
	private bool _meshBaked;

	[Tooltip("BindSharedMaterial / SharedMaterial 换材质时是否重建 mesh（按 BodyTile 等重算 UV）。默认关：烘焙关卡换风格只换材质。非烘焙且换风格需重算 UV 的模板（如 PhysicArrow/PhysicCircle）请在 prefab 上勾选。")]
	[SerializeField]
	private bool _rebuildMeshOnSharedMaterialChange;

	[Header("Z Layer Offsets")]
	[SerializeField]
	private float _ringZ;

	[SerializeField]
	private float _stemZ = 0.001f;

	[SerializeField]
	private float _crossBarZ = -0.002f;

	private const float ZPlaneEpsilon = 1E-06f;

	private MeshFilter _filter;

	private MeshRenderer _renderer;

	private Mesh _mesh;

	private readonly RingSegmentMeshBuilder _builder = new RingSegmentMeshBuilder();

	private readonly List<Vector3> _zShiftScratch = new List<Vector3>(512);

	private Material _runtimeMat;

	private Material _runtimeSource;

	private static readonly int IdTailLength = Shader.PropertyToID("_TailLength");

	private static readonly int IdHeadLength = Shader.PropertyToID("_HeadLength");

	private bool _isFirstEnableSkipped;

	public RingSegmentBuildSettings BuildSettings => _buildSettings;

	public Material SharedMaterial
	{
		get
		{
			return _sharedMaterial;
		}
		set
		{
			if ((Object)(object)value == (Object)null)
			{
				_sharedMaterial = null;
			}
			else
			{
				BindSharedMaterial(value);
			}
		}
	}

	public float CapRatio
	{
		get
		{
			return _capRatio;
		}
		set
		{
			_capRatio = Mathf.Clamp01(value);
			Rebuild();
		}
	}

	public int StyleIndex
	{
		get
		{
			return _styleIndex;
		}
		set
		{
			_styleIndex = value;
			Rebuild();
		}
	}

	public bool UseHeadTail => _gapAngleDeg > 0.01f;

	public float GapAngleDeg => _gapAngleDeg;

	public float GapDirectionDeg => _gapDirectionDeg;

	public float CenterRadius => _radius;

	public float BandWidth => _width;

	public Color VertexColor => _vertexColor;

	public void BindSharedMaterial(Material material)
	{
		if ((Object)(object)material == (Object)null)
		{
			return;
		}
		if (!_rebuildMeshOnSharedMaterialChange)
		{
			_sharedMaterial = material;
			if ((Object)(object)_renderer == (Object)null)
			{
				_renderer = ((Component)this).GetComponent<MeshRenderer>();
			}
			AssignMaterial();
			ApplyCapLengthsFromBuildWidth();
			return;
		}
		Material sharedMaterial = _sharedMaterial;
		bool flag = (Object)(object)sharedMaterial != (Object)(object)material;
		if (!flag && (Object)(object)sharedMaterial != (Object)null)
		{
			flag = !Mathf.Approximately(GetMaterialUvFloat(sharedMaterial, "_BodyTileLength"), GetMaterialUvFloat(material, "_BodyTileLength")) || !Mathf.Approximately(GetMaterialUvFloat(sharedMaterial, "_TailLength"), GetMaterialUvFloat(material, "_TailLength")) || !Mathf.Approximately(GetMaterialUvFloat(sharedMaterial, "_HeadLength"), GetMaterialUvFloat(material, "_HeadLength"));
		}
		_sharedMaterial = material;
		if ((Object)(object)_renderer == (Object)null)
		{
			_renderer = ((Component)this).GetComponent<MeshRenderer>();
		}
		AssignMaterial();
		ApplyCapLengthsFromBuildWidth();
		if (flag)
		{
			Rebuild();
		}
	}

	public void ApplyCapLengthsFromBuildWidth()
	{
		if (!Application.isPlaying || (Object)(object)_sharedMaterial == (Object)null || _width <= 0f)
		{
			return;
		}
		if ((Object)(object)_renderer == (Object)null)
		{
			_renderer = ((Component)this).GetComponent<MeshRenderer>();
		}
		if (SharedMaterialCapLengthsMatchBuildWidth(_sharedMaterial))
		{
			AssignMaterial();
			StyleLineUnderlayProxy component = ((Component)this).GetComponent<StyleLineUnderlayProxy>();
			if (component != null)
			{
				component.Sync();
			}
			return;
		}
		AssignMaterial();
		if (!((Object)(object)_runtimeMat == (Object)null))
		{
			float width = _width;
			if (_runtimeMat.HasProperty(IdTailLength))
			{
				_runtimeMat.SetFloat(IdTailLength, width);
			}
			if (_runtimeMat.HasProperty(IdHeadLength))
			{
				_runtimeMat.SetFloat(IdHeadLength, width);
			}
			StyleLineUnderlayProxy component2 = ((Component)this).GetComponent<StyleLineUnderlayProxy>();
			if (component2 != null)
			{
				component2.Sync();
			}
		}
	}

	private bool SharedMaterialCapLengthsMatchBuildWidth(Material material)
	{
		if ((Object)(object)material == (Object)null || _width <= 0f)
		{
			return true;
		}
		bool flag = material.HasProperty(IdTailLength);
		bool flag2 = material.HasProperty(IdHeadLength);
		if (!flag && !flag2)
		{
			return true;
		}
		float num = (flag ? material.GetFloat(IdTailLength) : _width);
		float num2 = (flag2 ? material.GetFloat(IdHeadLength) : _width);
		if (Mathf.Approximately(num, _width))
		{
			return Mathf.Approximately(num2, _width);
		}
		return false;
	}

	private void EnsureRuntimeMaterialInstance()
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		if (!Application.isPlaying || (Object)(object)_sharedMaterial == (Object)null || ((Object)(object)_runtimeMat != (Object)null && (Object)(object)_runtimeSource == (Object)(object)_sharedMaterial))
		{
			return;
		}
		if ((Object)(object)_runtimeMat != (Object)null)
		{
			if (Application.isPlaying)
			{
				Object.Destroy((Object)(object)_runtimeMat);
			}
			else
			{
				Object.DestroyImmediate((Object)(object)_runtimeMat);
			}
			_runtimeMat = null;
			_runtimeSource = null;
		}
		_runtimeMat = new Material(_sharedMaterial)
		{
			name = ((Object)_sharedMaterial).name + " (Ring)"
		};
		((Object)_runtimeMat).hideFlags = (HideFlags)61;
		_runtimeSource = _sharedMaterial;
	}

	private static float GetMaterialUvFloat(Material mat, string propertyName)
	{
		if ((Object)(object)mat == (Object)null || !mat.HasProperty(propertyName))
		{
			return 0f;
		}
		return mat.GetFloat(propertyName);
	}

	public void ApplyVertexColor(Color color)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Invalid comparison between Unknown and I4
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		if (color == default(Color))
		{
			return;
		}
		_vertexColor = color;
		if ((Object)(object)_filter == (Object)null)
		{
			_filter = ((Component)this).GetComponent<MeshFilter>();
		}
		Mesh val = ResolveVertexColorMesh();
		if (!((Object)(object)val == (Object)null) && val.vertexCount != 0)
		{
			Color32 val2 = (((int)QualitySettings.activeColorSpace == 1) ? Color32.op_Implicit(((Color)(ref _vertexColor)).linear) : Color32.op_Implicit(_vertexColor));
			int vertexCount = val.vertexCount;
			Color32[] array = val.colors32;
			if (array == null || array.Length != vertexCount)
			{
				array = (Color32[])(object)new Color32[vertexCount];
			}
			for (int i = 0; i < vertexCount; i++)
			{
				array[i] = val2;
			}
			val.colors32 = array;
		}
	}

	private Mesh ResolveVertexColorMesh()
	{
		if ((Object)(object)_filter == (Object)null)
		{
			return _mesh;
		}
		if (Application.isEditor)
		{
			return _mesh = _filter.mesh;
		}
		return _filter.sharedMesh ?? _mesh;
	}

	public void Setup(float radius, float gapAngleDeg, float gapDirectionDeg, RingSegmentBuildSettings settings, Material sharedMat)
	{
		_radius = radius;
		_gapAngleDeg = gapAngleDeg;
		_gapDirectionDeg = gapDirectionDeg;
		_width = settings.width;
		_arcSubdivisions = settings.arcSubdivisions;
		_styleIndex = settings.styleIndex;
		_sharedMaterial = sharedMat;
		_buildSettings = settings;
		Rebuild();
	}

	public void Setup(float radius, float gapAngleDeg, float gapDirectionDeg, RingSegmentBuildSettings settings, Material sharedMat, Color vertexColor = default(Color))
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		_radius = radius;
		_gapAngleDeg = gapAngleDeg;
		_gapDirectionDeg = gapDirectionDeg;
		_width = settings.width;
		_arcSubdivisions = settings.arcSubdivisions;
		_styleIndex = settings.styleIndex;
		_sharedMaterial = sharedMat;
		if (vertexColor != default(Color))
		{
			_vertexColor = vertexColor;
		}
		Rebuild();
	}

	private void Awake()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected O, but got Unknown
		_filter = ((Component)this).GetComponent<MeshFilter>();
		_renderer = ((Component)this).GetComponent<MeshRenderer>();
		if (_meshBaked && (Object)(object)_filter != (Object)null && (Object)(object)_filter.sharedMesh != (Object)null)
		{
			_mesh = _filter.sharedMesh;
			AssignMaterial();
			ApplyCapLengthsFromBuildWidth();
			_isFirstEnableSkipped = true;
		}
		else
		{
			_mesh = new Mesh
			{
				name = "RingSegment"
			};
			_filter.sharedMesh = _mesh;
		}
	}

	public void ResetAttachments(List<RingAttachment> attachments)
	{
		_attachments = attachments;
	}

	private void OnEnable()
	{
		if (_isFirstEnableSkipped)
		{
			_isFirstEnableSkipped = false;
			return;
		}
		_capRatio = 0.5f;
		Rebuild();
	}

	private void OnDestroy()
	{
		if ((Object)(object)_mesh != (Object)null && !_meshBaked)
		{
			if (Application.isPlaying)
			{
				Object.Destroy((Object)(object)_mesh);
			}
			else
			{
				Object.DestroyImmediate((Object)(object)_mesh, false);
			}
		}
		if ((Object)(object)_runtimeMat != (Object)null)
		{
			if (Application.isPlaying)
			{
				Object.Destroy((Object)(object)_runtimeMat);
			}
			else
			{
				Object.DestroyImmediate((Object)(object)_runtimeMat);
			}
			_runtimeMat = null;
		}
	}

	public void Rebuild()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Invalid comparison between Unknown and I4
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_034b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0390: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_041f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0421: Unknown result type (might be due to invalid IL or missing references)
		//IL_0423: Unknown result type (might be due to invalid IL or missing references)
		//IL_0438: Unknown result type (might be due to invalid IL or missing references)
		//IL_0472: Unknown result type (might be due to invalid IL or missing references)
		//IL_047d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0482: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04db: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)this == (Object)null || (Application.isPlaying && _meshBaked))
		{
			return;
		}
		if ((Object)(object)_mesh == (Object)null)
		{
			_mesh = new Mesh
			{
				name = "RingSegment"
			};
		}
		else if (_meshBaked)
		{
			EnsureIndependentMesh();
		}
		if ((Object)(object)_filter == (Object)null)
		{
			_filter = ((Component)this).GetComponent<MeshFilter>();
		}
		if ((Object)(object)_renderer == (Object)null)
		{
			_renderer = ((Component)this).GetComponent<MeshRenderer>();
		}
		float num = Mathf.Clamp(_gapAngleDeg, 0f, 360f);
		float num2 = 360f - num;
		if (num2 < 0.01f)
		{
			_mesh.Clear();
			return;
		}
		bool flag = num > 0.01f;
		float num3 = num * 0.5f;
		float num4 = _gapDirectionDeg + num3;
		float num5 = num4 + num2;
		float num6 = num4 * (MathF.PI / 180f);
		float num7 = num5 * (MathF.PI / 180f);
		float num8 = num2 * (MathF.PI / 180f) * _radius;
		float num9 = 0f;
		float num10 = 0f;
		float num11 = 1f;
		if (Application.isPlaying && flag && _width > 0f)
		{
			num9 = _width;
			num10 = _width;
		}
		else if ((Object)(object)_sharedMaterial != (Object)null && flag)
		{
			if (_sharedMaterial.HasFloat("_TailLength"))
			{
				num9 = _sharedMaterial.GetFloat("_TailLength");
			}
			if (_sharedMaterial.HasFloat("_HeadLength"))
			{
				num10 = _sharedMaterial.GetFloat("_HeadLength");
			}
		}
		if ((Object)(object)_sharedMaterial != (Object)null && _sharedMaterial.HasFloat("_BodyTileLength"))
		{
			num11 = Mathf.Max(0.001f, _sharedMaterial.GetFloat("_BodyTileLength"));
		}
		float num12 = num8 - num9 - num10;
		float num14;
		if (num12 > 0.001f && num11 > 0.001f)
		{
			float num13 = (float)Mathf.Max(1, Mathf.RoundToInt(num12 / num11)) * num11;
			num14 = num9 + num13 + num10;
		}
		else
		{
			num14 = num8;
		}
		float num15 = ((_radius > 0.001f) ? (num9 / _radius) : 0f);
		float num16 = ((_radius > 0.001f) ? (num10 / _radius) : 0f);
		float num17 = (flag ? (num15 * _capRatio) : 0f);
		float num18 = (flag ? (num16 * _capRatio) : 0f);
		float num19 = num6 + num17;
		float num20 = num7 - num18;
		Color32 val = (((int)QualitySettings.activeColorSpace == 1) ? Color32.op_Implicit(((Color)(ref _vertexColor)).linear) : Color32.op_Implicit(_vertexColor));
		float styleIdx = _styleIndex + 1;
		int num21 = RingSegmentArcSubdivision.ComputeFullCircleArcSubdivisions(RingSegmentArcSubdivision.EstimateInnerRadiusCore(_radius, _width));
		_builder.Begin();
		AppendAttachmentStems(val);
		if (!flag)
		{
			int arcSubdivisions = RingSegmentArcSubdivision.ResolveClosedLoopArcDivisions(num21, _radius);
			RingSegmentBuildSettings settings = new RingSegmentBuildSettings
			{
				width = _width,
				widthSubdivisions = 0,
				arcSubdivisions = arcSubdivisions,
				styleIndex = _styleIndex,
				useHeadTail = false
			};
			_builder.AppendArc(Vector3.zero, _radius, num6, num7, settings, closedLoop: true, _ringZ, val, num14);
		}
		else
		{
			float num22 = num9 * _capRatio;
			float num23 = num14 - num10 * _capRatio;
			if (num20 > num19 + 0.0001f)
			{
				AppendArcPortion(num19, num20, num22, num23, num14, styleIdx, val, num21);
			}
			if (_capRatio > 0.001f && num9 > 0.001f)
			{
				float num24 = num19;
				Vector3 anchorPos = new Vector3(Mathf.Cos(num24), Mathf.Sin(num24), 0f) * _radius;
				Vector3 outward = default(Vector3);
				((Vector3)(ref outward))._002Ector(Mathf.Sin(num24), 0f - Mathf.Cos(num24), 0f);
				Vector3 radial = default(Vector3);
				((Vector3)(ref radial))._002Ector(Mathf.Cos(num24), Mathf.Sin(num24), 0f);
				float length = num17 * _radius;
				AppendCapQuad(anchorPos, outward, radial, _width, length, num22, 0f, num14, styleIdx, val);
			}
			if (_capRatio > 0.001f && num10 > 0.001f)
			{
				float num25 = num20;
				Vector3 anchorPos2 = new Vector3(Mathf.Cos(num25), Mathf.Sin(num25), 0f) * _radius;
				Vector3 outward2 = default(Vector3);
				((Vector3)(ref outward2))._002Ector(0f - Mathf.Sin(num25), Mathf.Cos(num25), 0f);
				Vector3 radial2 = default(Vector3);
				((Vector3)(ref radial2))._002Ector(Mathf.Cos(num25), Mathf.Sin(num25), 0f);
				float length2 = num18 * _radius;
				AppendCapQuad(anchorPos2, outward2, radial2, _width, length2, num23, num14, num14, styleIdx, val);
			}
		}
		AppendAttachmentCrossBars(val);
		_builder.End(_mesh);
		_filter.sharedMesh = _mesh;
		AssignMaterial();
		ApplyCapLengthsFromBuildWidth();
		if (!Application.isPlaying)
		{
			StyleLineUnderlayProxy component = ((Component)this).GetComponent<StyleLineUnderlayProxy>();
			if (component != null)
			{
				component.Sync();
			}
		}
	}

	private void AssignMaterial()
	{
		if ((Object)(object)_sharedMaterial == (Object)null)
		{
			return;
		}
		if ((Object)(object)_renderer == (Object)null)
		{
			_renderer = ((Component)this).GetComponent<MeshRenderer>();
		}
		if ((Object)(object)_renderer == (Object)null)
		{
			return;
		}
		if (!Application.isPlaying)
		{
			((Renderer)_renderer).sharedMaterial = _sharedMaterial;
			return;
		}
		if ((Object)(object)_runtimeSource != (Object)(object)_sharedMaterial && (Object)(object)_runtimeMat != (Object)null)
		{
			Object.Destroy((Object)(object)_runtimeMat);
			_runtimeMat = null;
			_runtimeSource = null;
		}
		if ((Object)(object)_runtimeMat == (Object)null && SharedMaterialCapLengthsMatchBuildWidth(_sharedMaterial))
		{
			((Renderer)_renderer).sharedMaterial = _sharedMaterial;
			((Renderer)_renderer).SetPropertyBlock((MaterialPropertyBlock)null);
		}
		else
		{
			EnsureRuntimeMaterialInstance();
			((Renderer)_renderer).sharedMaterial = _runtimeMat;
			((Renderer)_renderer).SetPropertyBlock((MaterialPropertyBlock)null);
		}
	}

	private void AppendAttachmentStems(Color32 vc)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		if (_attachments != null && _attachments.Count != 0)
		{
			float halfRingWidth = _width * 0.5f;
			Vector3 up = -Vector3.forward;
			for (int i = 0; i < _attachments.Count; i++)
			{
				RingAttachment att = _attachments[i];
				ComputeStemGeometry(att, halfRingWidth, out var stemOrigin, out var extendDir, out var stemTotalLength);
				bool flag = att.type == RingAttachment.AttachType.HeadedBar;
				float headLength = (flag ? att.crossWidth : 0f);
				_builder.AppendStraightRibbon(stemOrigin, extendDir, up, att.stemWidth, stemTotalLength, headLength, flag, att.styleIndex, _stemZ, _width, vc);
			}
		}
	}

	private void AppendAttachmentCrossBars(Color32 vc)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		if (_attachments == null || _attachments.Count == 0)
		{
			return;
		}
		float halfRingWidth = _width * 0.5f;
		Vector3 up = -Vector3.forward;
		Vector3 val = default(Vector3);
		for (int i = 0; i < _attachments.Count; i++)
		{
			RingAttachment att = _attachments[i];
			if (att.type != RingAttachment.AttachType.TBar)
			{
				continue;
			}
			float num = att.angleDeg * (MathF.PI / 180f);
			new Vector3(Mathf.Cos(num), Mathf.Sin(num), 0f);
			((Vector3)(ref val))._002Ector(0f - Mathf.Sin(num), Mathf.Cos(num), 0f);
			ComputeStemGeometry(att, halfRingWidth, out var stemOrigin, out var extendDir, out var stemTotalLength);
			Vector3 val2 = stemOrigin + extendDir * stemTotalLength;
			Vector3 bodyOrigin = val2 - val * (att.crossLength * 0.5f);
			if (_extraEnabled)
			{
				float num2 = att.crossLength * 2f;
				Vector3 bodyOrigin2 = val2 - val * (num2 * 0.5f);
				AppendSubdividedCrossBar(bodyOrigin2, val, up, att.crossWidth, num2, 0f, 0f, 0f, att.stemWidth, att.styleIndex, vc, att.isInnerMount, isExtra: true);
				continue;
			}
			float num3 = Mathf.Clamp01(att.crossCapLength);
			if (num3 > 0f && (Object)(object)_sharedMaterial != (Object)null)
			{
				float matTailLength = _sharedMaterial.GetFloat("_TailLength");
				float matHeadLength = _sharedMaterial.GetFloat("_HeadLength");
				float capLength = att.crossWidth * num3;
				AppendSubdividedCrossBar(bodyOrigin, val, up, att.crossWidth, att.crossLength, capLength, matTailLength, matHeadLength, att.stemWidth, att.styleIndex, vc, att.isInnerMount);
			}
			else
			{
				AppendSubdividedCrossBar(bodyOrigin, val, up, att.crossWidth, att.crossLength, 0f, 0f, 0f, att.stemWidth, att.styleIndex, vc, att.isInnerMount);
			}
		}
	}

	private void AppendSubdividedCrossBar(Vector3 bodyOrigin, Vector3 direction, Vector3 up, float width, float bodyLength, float capLength, float matTailLength, float matHeadLength, float stemWidth, int styleIndex, Color32 vc, bool isInnerMount = false, bool isExtra = false)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_031d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		float num = width * 0.5f;
		float num2 = stemWidth * 0.25f;
		float num3 = (isExtra ? (-(styleIndex + 1)) : (styleIndex + 1));
		Vector3 val = Vector3.Cross(up, direction);
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		Vector4 tangent = default(Vector4);
		((Vector4)(ref tangent))._002Ector(direction.x, direction.y, direction.z, -1f);
		float num4 = ((_width > 0f && width > 0f) ? (_width / width) : 1f);
		bool flag = capLength > 0.0001f;
		float num5 = bodyLength * num4;
		float num6;
		float[] array;
		float[] array2;
		if (flag)
		{
			num6 = matTailLength + num5 + matHeadLength;
			float num7 = capLength + bodyLength + capLength;
			float num8 = capLength + bodyLength * 0.5f;
			float num9 = num8 - num2;
			float num10 = num8 + num2;
			num9 = Mathf.Clamp(num9, capLength, capLength + bodyLength);
			num10 = Mathf.Clamp(num10, capLength, capLength + bodyLength);
			float num11 = matTailLength + (num9 - capLength) * num4;
			float num12 = matTailLength + (num10 - capLength) * num4;
			array = new float[6]
			{
				0f,
				capLength,
				num9,
				num10,
				capLength + bodyLength,
				num7
			};
			array2 = new float[6]
			{
				0f,
				matTailLength,
				num11,
				num12,
				matTailLength + num5,
				num6
			};
		}
		else
		{
			float num13;
			if (isExtra)
			{
				num6 = num5;
				num13 = 0f;
			}
			else
			{
				num6 = 99999f;
				num13 = 100f;
			}
			float num14 = bodyLength * 0.5f;
			float num15 = Mathf.Clamp(num14 - num2, 0f, bodyLength);
			float num16 = Mathf.Clamp(num14 + num2, 0f, bodyLength);
			array = new float[4] { 0f, num15, num16, bodyLength };
			array2 = new float[4]
			{
				num13,
				num13 + num15 * num4,
				num13 + num16 * num4,
				num13 + num5
			};
		}
		Vector3 val2 = (flag ? (bodyOrigin - direction * capLength) : bodyOrigin);
		float[] array3 = new float[3] { 1f, 0.5f, 0f };
		float[] array4 = new float[3]
		{
			num,
			0f,
			0f - num
		};
		int num17 = array3.Length;
		int num18 = array.Length;
		int num19 = ((!flag) ? 1 : 2);
		int num20 = num19 + 1;
		int num21 = ((!isInnerMount) ? (num17 - 2) : 0);
		for (int i = 0; i < num18 - 1; i++)
		{
			bool flag2 = i >= num19 && i < num20;
			float num22 = array[i];
			float num23 = array[i + 1];
			float num24 = array2[i];
			float num25 = array2[i + 1];
			Vector3 val3 = val2 + direction * num22;
			Vector3 val4 = val2 + direction * num23;
			for (int j = 0; j < num17 - 1; j++)
			{
				bool flag3 = j == num21;
				float outlineMask = ((flag2 && flag3) ? 0f : 1f);
				int vertexCount = _builder.VertexCount;
				for (int k = 0; k < 2; k++)
				{
					Vector3 val5 = ((k == 0) ? val3 : val4);
					float num26 = ((k == 0) ? num24 : num25);
					for (int l = j; l <= j + 1; l++)
					{
						Vector3 pos = val5 + normalized * array4[l];
						pos.z += _crossBarZ;
						_builder.AddVertex(pos, Vector3.forward, tangent, vc, new Vector2(num26, array3[l]), new Vector2(num6, num3), outlineMask);
					}
				}
				_builder.AddTriangle(vertexCount, vertexCount + 1, vertexCount + 2);
				_builder.AddTriangle(vertexCount + 1, vertexCount + 3, vertexCount + 2);
			}
		}
	}

	private void ComputeStemGeometry(RingAttachment att, float halfRingWidth, out Vector3 stemOrigin, out Vector3 extendDir, out float stemTotalLength)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		float num = att.angleDeg * (MathF.PI / 180f);
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(Mathf.Cos(num), Mathf.Sin(num), 0f);
		float num2 = Mathf.Max(0f, att.stemInsertOffset);
		stemTotalLength = att.stemLength + num2;
		if (att.isInnerMount)
		{
			stemOrigin = val * (_radius - halfRingWidth + num2);
			extendDir = -val;
		}
		else
		{
			stemOrigin = val * (_radius + halfRingWidth - num2);
			extendDir = val;
		}
	}

	public void SetColor(Color color)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_renderer == (Object)null)
		{
			_renderer = ((Component)this).GetComponent<MeshRenderer>();
		}
		EnsureRuntimeMaterialInstance();
		AssignMaterial();
		Material val = (((Object)(object)_renderer != (Object)null) ? ((Renderer)_renderer).sharedMaterial : null);
		if ((Object)(object)val != (Object)null && val.HasProperty("_Color"))
		{
			val.SetColor("_Color", color);
		}
	}

	private bool EnsureIndependentMesh()
	{
		if ((Object)(object)_filter == (Object)null)
		{
			_filter = ((Component)this).GetComponent<MeshFilter>();
		}
		if (!_meshBaked)
		{
			return (Object)(object)_mesh != (Object)null;
		}
		if ((Object)(object)_mesh == (Object)null)
		{
			return false;
		}
		_mesh = Object.Instantiate<Mesh>(_mesh);
		((Object)_mesh).name = "RingSegment";
		_meshBaked = false;
		if ((Object)(object)_filter != (Object)null)
		{
			_filter.sharedMesh = _mesh;
		}
		return true;
	}

	private bool TryShiftZLayerOffsets(float ringZ, float stemZ, float crossBarZ)
	{
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_mesh == (Object)null || _mesh.vertexCount == 0)
		{
			return false;
		}
		float ringZ2 = _ringZ;
		float stemZ2 = _stemZ;
		float crossBarZ2 = _crossBarZ;
		float num = ringZ - ringZ2;
		float num2 = stemZ - stemZ2;
		float num3 = crossBarZ - crossBarZ2;
		if ((Mathf.Abs(ringZ2 - stemZ2) < 1E-06f && Mathf.Abs(num - num2) > 1E-06f) || (Mathf.Abs(ringZ2 - crossBarZ2) < 1E-06f && Mathf.Abs(num - num3) > 1E-06f) || (Mathf.Abs(stemZ2 - crossBarZ2) < 1E-06f && Mathf.Abs(num2 - num3) > 1E-06f))
		{
			return false;
		}
		if (Mathf.Abs(num) < 1E-06f && Mathf.Abs(num2) < 1E-06f && Mathf.Abs(num3) < 1E-06f)
		{
			_ringZ = ringZ;
			_stemZ = stemZ;
			_crossBarZ = crossBarZ;
			return true;
		}
		if (!EnsureIndependentMesh() || !_mesh.isReadable)
		{
			return false;
		}
		_ringZ = ringZ;
		_stemZ = stemZ;
		_crossBarZ = crossBarZ;
		_zShiftScratch.Clear();
		_mesh.GetVertices(_zShiftScratch);
		int count = _zShiftScratch.Count;
		int num4 = 0;
		for (int i = 0; i < count; i++)
		{
			Vector3 val = _zShiftScratch[i];
			if (Mathf.Abs(val.z - ringZ2) < 1E-06f)
			{
				val.z += num;
				num4++;
			}
			else if (Mathf.Abs(val.z - stemZ2) < 1E-06f)
			{
				val.z += num2;
				num4++;
			}
			else if (Mathf.Abs(val.z - crossBarZ2) < 1E-06f)
			{
				val.z += num3;
				num4++;
			}
			_zShiftScratch[i] = val;
		}
		if (num4 == 0)
		{
			return false;
		}
		_mesh.SetVertices(_zShiftScratch);
		_mesh.RecalculateBounds();
		return true;
	}

	public void SetZLayerOffsets(float ringZ, float stemZ, float crossBarZ)
	{
		if (!TryShiftZLayerOffsets(ringZ, stemZ, crossBarZ))
		{
			_ringZ = ringZ;
			_stemZ = stemZ;
			_crossBarZ = crossBarZ;
			Rebuild();
		}
	}

	public void GetZLayerOffsets(out float ringZ, out float stemZ, out float crossBarZ)
	{
		ringZ = _ringZ;
		stemZ = _stemZ;
		crossBarZ = _crossBarZ;
	}

	public void SetCrossBarZ(float crossBarZ)
	{
		SetZLayerOffsets(_ringZ, _stemZ, crossBarZ);
	}

	private void AppendArcPortion(float startRad, float endRad, float uvStart, float uvEnd, float visibleLength, float styleIdx1, Color32 vc, int fullCircleArcSub)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		float num = endRad - startRad;
		int num2 = RingSegmentArcSubdivision.ResolveArcDivisionsForVisibleArc(fullCircleArcSub, Mathf.Abs(num) * 57.29578f, _radius);
		float num3 = _width * 0.5f;
		int vertexCount = _builder.VertexCount;
		Vector3 val2 = default(Vector3);
		Vector4 tangent = default(Vector4);
		for (int i = 0; i <= num2; i++)
		{
			float num4 = (float)i / (float)num2;
			float num5 = Mathf.Lerp(startRad, endRad, num4);
			float num6 = Mathf.Cos(num5);
			float num7 = Mathf.Sin(num5);
			Vector3 val = new Vector3(num6, num7, 0f);
			((Vector3)(ref val2))._002Ector(0f - num7, num6, 0f);
			if (num < 0f)
			{
				val2 = -val2;
			}
			float num8 = Mathf.Lerp(uvStart, uvEnd, num4);
			((Vector4)(ref tangent))._002Ector(val2.x, val2.y, val2.z, -1f);
			Vector3 pos = val * (_radius + num3);
			pos.z += _ringZ;
			_builder.AddVertex(pos, Vector3.forward, tangent, vc, new Vector2(num8, 1f), new Vector2(visibleLength, styleIdx1));
			Vector3 pos2 = val * (_radius - num3);
			pos2.z += _ringZ;
			_builder.AddVertex(pos2, Vector3.forward, tangent, vc, new Vector2(num8, 0f), new Vector2(visibleLength, styleIdx1));
		}
		for (int j = 0; j < num2; j++)
		{
			int a = vertexCount + j * 2;
			int num9 = vertexCount + j * 2 + 1;
			int c = vertexCount + (j + 1) * 2;
			int b = vertexCount + (j + 1) * 2 + 1;
			_builder.AddTriangle(a, num9, c);
			_builder.AddTriangle(num9, b, c);
		}
	}

	private void AppendCapQuad(Vector3 anchorPos, Vector3 outward, Vector3 radial, float width, float length, float uvXAnchor, float uvXTip, float visibleLength, float styleIdx1, Color32 vc)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		float num = width * 0.5f;
		Vector3 val = ((uvXTip >= uvXAnchor) ? outward : (-outward));
		Vector4 tangent = default(Vector4);
		((Vector4)(ref tangent))._002Ector(val.x, val.y, val.z, -1f);
		Vector3 val2 = anchorPos + outward * length;
		Vector3 pos = anchorPos - radial * num;
		Vector3 pos2 = anchorPos + radial * num;
		Vector3 pos3 = val2 - radial * num;
		Vector3 pos4 = val2 + radial * num;
		pos.z += _ringZ;
		pos2.z += _ringZ;
		pos3.z += _ringZ;
		pos4.z += _ringZ;
		int vertexCount = _builder.VertexCount;
		_builder.AddVertex(pos2, Vector3.forward, tangent, vc, new Vector2(uvXAnchor, 1f), new Vector2(visibleLength, styleIdx1));
		_builder.AddVertex(pos, Vector3.forward, tangent, vc, new Vector2(uvXAnchor, 0f), new Vector2(visibleLength, styleIdx1));
		_builder.AddVertex(pos4, Vector3.forward, tangent, vc, new Vector2(uvXTip, 1f), new Vector2(visibleLength, styleIdx1));
		_builder.AddVertex(pos3, Vector3.forward, tangent, vc, new Vector2(uvXTip, 0f), new Vector2(visibleLength, styleIdx1));
		if (outward.x * radial.y - outward.y * radial.x < 0f)
		{
			_builder.AddTriangle(vertexCount, vertexCount + 1, vertexCount + 2);
			_builder.AddTriangle(vertexCount + 1, vertexCount + 3, vertexCount + 2);
		}
		else
		{
			_builder.AddTriangle(vertexCount, vertexCount + 2, vertexCount + 1);
			_builder.AddTriangle(vertexCount + 1, vertexCount + 2, vertexCount + 3);
		}
	}
}
