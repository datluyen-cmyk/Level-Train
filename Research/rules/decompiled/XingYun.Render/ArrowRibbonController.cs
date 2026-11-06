using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using XingYun.Rendering;

namespace XingYun.Render;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public sealed class ArrowRibbonController : MonoBehaviour
{
	public enum State
	{
		Idle,
		Sliding,
		Done
	}

	private bool _isFirstEnableSkipped;

	[Header("References")]
	[Tooltip("Shared material using HsApp/Effect/ArrowRibbon.")]
	[SerializeField]
	private Material _sharedMaterial;

	[Header("Path (Tail -> Head, local space)")]
	[SerializeField]
	private List<Vector3> _pathPoints = new List<Vector3>();

	[Header("Build Settings")]
	[SerializeField]
	private ArrowRibbonBuildSettings _buildSettings = ArrowRibbonBuildSettings.Default;

	[Header("Slide")]
	[Tooltip("Slide speed in local path units per second.")]
	[SerializeField]
	private float _speed = 6f;

	[Tooltip("Build the ribbon automatically in OnEnable.")]
	[SerializeField]
	private bool _buildOnEnable = true;

	[Header("Vertex Color")]
	[SerializeField]
	private Color _vertexColor = Color.white;

	[Header("Debug")]
	[Tooltip("When enabled, the controller does NOT write _WindowStart to the material. You can then adjust it freely on the shared material.")]
	[SerializeField]
	private bool _debugMode;

	[Header("Runtime Mesh")]
	[Tooltip("勾选后 Awake/OnEnable 复用预制体已烘焙的 Mesh，跳过首次 Build。运行时调用 Setup/SetPath 等仍会正常重建。")]
	[SerializeField]
	private bool _meshBaked;

	[Tooltip("BindSharedMaterial 换材质时是否重建 mesh（按 BodyTile 等重算 UV）。默认关：烘焙关卡换风格只换材质。非烘焙模板（PhysicArrow 等）请在 prefab 上勾选。")]
	[SerializeField]
	private bool _rebuildMeshOnSharedMaterialChange;

	[Header("Decorations (optional)")]
	[SerializeField]
	private List<ArrowRibbonDecoration> _decorations = new List<ArrowRibbonDecoration>();

	[Header("Events")]
	[Tooltip("Fired exactly once when the tail has fully left the guide.")]
	public ArrowRibbonEvent OnCompleted = new ArrowRibbonEvent();

	public Func<bool> BlockedPredicate;

	public Action<ArrowRibbonController> OnRecycle;

	private static readonly int IdWindowStart = Shader.PropertyToID("_WindowStart");

	private static readonly int IdTailLength = Shader.PropertyToID("_TailLength");

	private static readonly int IdHeadLength = Shader.PropertyToID("_HeadLength");

	private MeshFilter _filter;

	private MeshRenderer _renderer;

	private Mesh _mesh;

	private Material _instanceMat;

	private Material _instanceSource;

	private readonly ArrowPathMeshBuilder _builder = new ArrowPathMeshBuilder();

	private ArrowGuidePath _guide;

	private float _windowStart;

	private float _lastAppliedWindowStart = float.MinValue;

	private State _state;

	public ArrowRibbonBuildSettings BuildSettings => _buildSettings;

	public Color VertexColor => _vertexColor;

	public State CurrentState => _state;

	public ArrowGuidePath Guide => _guide;

	public float WindowStart => _windowStart;

	public float VisibleLength
	{
		get
		{
			if (_guide == null)
			{
				return 0f;
			}
			return _guide.OriginalLength;
		}
	}

	public float ExitGuideLength
	{
		get
		{
			if (_guide == null)
			{
				return 0f;
			}
			return _guide.ExitGuideLength;
		}
	}

	public Material SharedMaterial => _sharedMaterial;

	public void BindSharedMaterial(Material material)
	{
		if ((Object)(object)material == (Object)null)
		{
			return;
		}
		if (!_rebuildMeshOnSharedMaterialChange)
		{
			_sharedMaterial = material;
			ReleaseInstanceMaterial();
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
			flag = !Mathf.Approximately(GetMaterialUvFloat(sharedMaterial, "_TailLength"), GetMaterialUvFloat(material, "_TailLength")) || !Mathf.Approximately(GetMaterialUvFloat(sharedMaterial, "_HeadLength"), GetMaterialUvFloat(material, "_HeadLength"));
		}
		_sharedMaterial = material;
		ReleaseInstanceMaterial();
		if ((Object)(object)_renderer == (Object)null)
		{
			_renderer = ((Component)this).GetComponent<MeshRenderer>();
		}
		AssignMaterial();
		ApplyCapLengthsFromBuildWidth();
		if (flag && _pathPoints != null && _pathPoints.Count >= 2)
		{
			Build();
		}
	}

	public void ApplyCapLengthsFromBuildWidth()
	{
		if (!Application.isPlaying || _debugMode || (Object)(object)_sharedMaterial == (Object)null)
		{
			return;
		}
		float width = _buildSettings.width;
		if (width <= 0f)
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
		if (!((Object)(object)_instanceMat == (Object)null))
		{
			if (_instanceMat.HasProperty(IdTailLength))
			{
				_instanceMat.SetFloat(IdTailLength, width);
			}
			if (_instanceMat.HasProperty(IdHeadLength))
			{
				_instanceMat.SetFloat(IdHeadLength, width);
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
		float width = _buildSettings.width;
		if ((Object)(object)material == (Object)null || width <= 0f)
		{
			return true;
		}
		bool flag = material.HasProperty(IdTailLength);
		bool flag2 = material.HasProperty(IdHeadLength);
		if (!flag && !flag2)
		{
			return true;
		}
		float num = (flag ? material.GetFloat(IdTailLength) : width);
		float num2 = (flag2 ? material.GetFloat(IdHeadLength) : width);
		if (Mathf.Approximately(num, width))
		{
			return Mathf.Approximately(num2, width);
		}
		return false;
	}

	private void EnsureRuntimeMaterialInstance()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		if (Application.isPlaying && !_debugMode && !((Object)(object)_sharedMaterial == (Object)null) && (!((Object)(object)_instanceMat != (Object)null) || !((Object)(object)_instanceSource == (Object)(object)_sharedMaterial)))
		{
			if ((Object)(object)_instanceMat != (Object)null)
			{
				ReleaseInstanceMaterial();
			}
			_instanceMat = new Material(_sharedMaterial)
			{
				name = ((Object)_sharedMaterial).name + " (Arrow)"
			};
			((Object)_instanceMat).hideFlags = (HideFlags)61;
			_instanceSource = _sharedMaterial;
		}
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
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Invalid comparison between Unknown and I4
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		if (color == default(Color))
		{
			return;
		}
		_vertexColor = color;
		if (_state == State.Sliding)
		{
			return;
		}
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
				name = "ArrowRibbon"
			};
			_mesh.MarkDynamic();
			_filter.sharedMesh = _mesh;
			AssignMaterial();
			ApplyCapLengthsFromBuildWidth();
		}
	}

	private void OnEnable()
	{
		if (_isFirstEnableSkipped)
		{
			_isFirstEnableSkipped = false;
		}
		else if (_buildOnEnable && _pathPoints != null && _pathPoints.Count >= 2)
		{
			Build();
		}
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
				Object.DestroyImmediate((Object)(object)_mesh);
			}
		}
		ReleaseInstanceMaterial();
	}

	public void Setup(IList<Vector3> pointsTailToHead, ArrowRibbonBuildSettings settings, Color vertexColor = default(Color))
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		_buildSettings = settings;
		if (vertexColor != default(Color))
		{
			_vertexColor = vertexColor;
		}
		SetPath(pointsTailToHead);
	}

	public bool TryGetCenterlinePathLength(out float length)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		length = 0f;
		if (_pathPoints == null || _pathPoints.Count < 2)
		{
			return false;
		}
		float num = 0f;
		for (int i = 1; i < _pathPoints.Count; i++)
		{
			num += Vector3.Distance(_pathPoints[i - 1], _pathPoints[i]);
		}
		length = num;
		return num > 0.0001f;
	}

	public void SetPath(IList<Vector3> pointsTailToHead)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		_pathPoints.Clear();
		for (int i = 0; i < pointsTailToHead.Count; i++)
		{
			_pathPoints.Add(pointsTailToHead[i]);
		}
		Build();
	}

	public void Build()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Invalid comparison between Unknown and I4
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		if (Application.isPlaying && _meshBaked)
		{
			return;
		}
		if ((Object)(object)_mesh == (Object)null)
		{
			_mesh = new Mesh
			{
				name = "ArrowRibbon"
			};
			_mesh.MarkDynamic();
		}
		else if (_meshBaked)
		{
			_mesh = Object.Instantiate<Mesh>(_mesh);
			((Object)_mesh).name = "ArrowRibbon";
			_mesh.MarkDynamic();
			_meshBaked = false;
			if ((Object)(object)_filter == (Object)null)
			{
				_filter = ((Component)this).GetComponent<MeshFilter>();
			}
			_filter.sharedMesh = _mesh;
		}
		if ((Object)(object)_filter == (Object)null)
		{
			_filter = ((Component)this).GetComponent<MeshFilter>();
		}
		if ((Object)(object)_renderer == (Object)null)
		{
			_renderer = ((Component)this).GetComponent<MeshRenderer>();
		}
		bool uploadTBN = (Object)(object)_sharedMaterial != (Object)null && (_sharedMaterial.IsKeywordEnabled("_NORMALMAP_ON") || _sharedMaterial.IsKeywordEnabled("_MATCAP_ON") || _sharedMaterial.IsKeywordEnabled("_SIMPLELIGHT_ON"));
		Color32 value = (((int)QualitySettings.activeColorSpace == 1) ? Color32.op_Implicit(((Color)(ref _vertexColor)).linear) : Color32.op_Implicit(_vertexColor));
		_guide = _builder.Build(_pathPoints, _buildSettings, _mesh, _guide, uploadTBN, _sharedMaterial, value);
		_filter.sharedMesh = _mesh;
		AssignMaterial();
		_windowStart = 0f;
		_lastAppliedWindowStart = float.MinValue;
		_state = State.Idle;
		if (Application.isPlaying)
		{
			ApplyCapLengthsFromBuildWidth();
			ApplyWindowStart();
		}
		InitDecorations();
		UpdateDecorations();
	}

	public bool TryStartSlide()
	{
		if (_state != State.Idle)
		{
			return false;
		}
		if (BlockedPredicate != null && BlockedPredicate())
		{
			return false;
		}
		_state = State.Sliding;
		return true;
	}

	public void StartSlide()
	{
		if (_state == State.Idle)
		{
			_state = State.Sliding;
		}
	}

	public void ResetForReuse()
	{
		_windowStart = 0f;
		_lastAppliedWindowStart = float.MinValue;
		_state = State.Idle;
		if (_pathPoints != null && _pathPoints.Count >= 2)
		{
			Build();
		}
	}

	private void Update()
	{
		if (_state == State.Sliding)
		{
			_windowStart += _speed * Time.deltaTime;
			float num = ((_guide != null) ? _guide.ExitGuideLength : 0f);
			if (_windowStart >= num)
			{
				_windowStart = num;
				ApplyWindowStart();
				UpdateDecorations();
				Complete();
			}
			else
			{
				ApplyWindowStart();
				UpdateDecorations();
			}
		}
	}

	private void Complete()
	{
		_state = State.Done;
		((UnityEvent<ArrowRibbonController>)OnCompleted)?.Invoke(this);
		OnRecycle?.Invoke(this);
	}

	private void AssignMaterial()
	{
		if ((Object)(object)_renderer == (Object)null || (Object)(object)_sharedMaterial == (Object)null)
		{
			return;
		}
		if (!Application.isPlaying || _debugMode)
		{
			((Renderer)_renderer).sharedMaterial = _sharedMaterial;
			return;
		}
		if ((Object)(object)_instanceSource != (Object)(object)_sharedMaterial && (Object)(object)_instanceMat != (Object)null)
		{
			ReleaseInstanceMaterial();
		}
		if ((Object)(object)_instanceMat == (Object)null && SharedMaterialCapLengthsMatchBuildWidth(_sharedMaterial))
		{
			((Renderer)_renderer).sharedMaterial = _sharedMaterial;
			((Renderer)_renderer).SetPropertyBlock((MaterialPropertyBlock)null);
		}
		else
		{
			EnsureRuntimeMaterialInstance();
			((Renderer)_renderer).sharedMaterial = _instanceMat;
			((Renderer)_renderer).SetPropertyBlock((MaterialPropertyBlock)null);
		}
	}

	private void ReleaseInstanceMaterial()
	{
		if (!((Object)(object)_instanceMat == (Object)null))
		{
			if (Application.isPlaying)
			{
				Object.Destroy((Object)(object)_instanceMat);
			}
			else
			{
				Object.DestroyImmediate((Object)(object)_instanceMat);
			}
			_instanceMat = null;
			_instanceSource = null;
		}
	}

	private void ApplyWindowStart()
	{
		if (!_debugMode && Application.isPlaying && !Mathf.Approximately(_windowStart, _lastAppliedWindowStart))
		{
			_lastAppliedWindowStart = _windowStart;
			EnsureRuntimeMaterialInstance();
			AssignMaterial();
			if ((Object)(object)_instanceMat != (Object)null)
			{
				_instanceMat.SetFloat(IdWindowStart, _windowStart);
			}
		}
	}

	private void InitDecorations()
	{
		if (_decorations == null)
		{
			return;
		}
		for (int i = 0; i < _decorations.Count; i++)
		{
			if ((Object)(object)_decorations[i] != (Object)null)
			{
				_decorations[i].Init(_guide);
			}
		}
	}

	private void UpdateDecorations()
	{
		if (_decorations == null || _guide == null)
		{
			return;
		}
		for (int i = 0; i < _decorations.Count; i++)
		{
			if ((Object)(object)_decorations[i] != (Object)null)
			{
				_decorations[i].UpdateOnPath(_windowStart);
			}
		}
	}
}
