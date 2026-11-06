using System;
using UnityEngine;

namespace XingYun.Rendering;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public sealed class RingStyleLineExtraQuad : MonoBehaviour
{
	public enum Slot
	{
		A,
		B,
		C
	}

	private const float V = 1f;

	[Tooltip("Which Extra rect to sample: A/B/C. Written to the mesh as uv2.y (rect id).")]
	[SerializeField]
	private Slot _slot;

	[Tooltip("Style row (1-based). Written as uv1.y = −StyleIndex.")]
	[SerializeField]
	[Min(1f)]
	private int _styleIndex = 1;

	[Tooltip("Vertex color written to the quad.")]
	[SerializeField]
	private Color _vertexColor = Color.white;

	[Tooltip("Push the quad outward from local origin by this radius (place it on a ring).")]
	[SerializeField]
	private float _ringRadius;

	[Tooltip("Angle around the ring in degrees (0 = up/+Y, 90 = right/+X).")]
	[SerializeField]
	private float _angleDeg;

	[Tooltip("共享材质源（与 MeshRenderer 一致）；Play 模式下会克隆为独立实例。")]
	[SerializeField]
	private Material _sharedMaterial;

	private MeshFilter _filter;

	private MeshRenderer _renderer;

	private Mesh _mesh;

	private Material _runtimeMat;

	private Material _runtimeSource;

	public Slot CurrentSlot => _slot;

	public int RectId => (int)(_slot + 1);

	public Material SharedMaterial => _sharedMaterial;

	private Material Mat
	{
		get
		{
			if ((Object)(object)_renderer == (Object)null)
			{
				_renderer = ((Component)this).GetComponent<MeshRenderer>();
			}
			if (!((Object)(object)_renderer != (Object)null))
			{
				return null;
			}
			return ((Renderer)_renderer).sharedMaterial;
		}
	}

	public bool MaterialExtraEnabled()
	{
		Material mat = Mat;
		if ((Object)(object)mat != (Object)null)
		{
			if (!mat.IsKeywordEnabled("_EXTRA_ON"))
			{
				if (mat.HasProperty("_ExtraEnable"))
				{
					return mat.GetFloat("_ExtraEnable") > 0.5f;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public bool StyleIndexFromMesh()
	{
		Material mat = Mat;
		if ((Object)(object)mat != (Object)null && mat.HasProperty("_StyleIndexFromMesh"))
		{
			return mat.GetFloat("_StyleIndexFromMesh") > 0.5f;
		}
		return false;
	}

	private void Awake()
	{
		_filter = ((Component)this).GetComponent<MeshFilter>();
		_renderer = ((Component)this).GetComponent<MeshRenderer>();
		if ((Object)(object)_sharedMaterial == (Object)null && (Object)(object)_renderer != (Object)null && (Object)(object)((Renderer)_renderer).sharedMaterial != (Object)null)
		{
			_sharedMaterial = ((Renderer)_renderer).sharedMaterial;
		}
	}

	private void OnEnable()
	{
		Rebuild();
	}

	private void OnDestroy()
	{
		if ((Object)(object)_mesh != (Object)null)
		{
			if (Application.isPlaying)
			{
				Object.Destroy((Object)(object)_mesh);
			}
			else
			{
				Object.DestroyImmediate((Object)(object)_mesh);
			}
			_mesh = null;
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

	public void ApplyVertexColor(Color color)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		_vertexColor = color;
		Rebuild();
	}

	public void SetRingRadius(float ringRadius)
	{
		_ringRadius = ringRadius;
		Rebuild();
	}

	public void BindSharedMaterial(Material material)
	{
		if (!((Object)(object)material == (Object)null))
		{
			_sharedMaterial = material;
			if ((Object)(object)_renderer == (Object)null)
			{
				_renderer = ((Component)this).GetComponent<MeshRenderer>();
			}
			AssignMaterial();
		}
	}

	public void Rebuild()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_filter == (Object)null)
		{
			_filter = ((Component)this).GetComponent<MeshFilter>();
		}
		if ((Object)(object)_renderer == (Object)null)
		{
			_renderer = ((Component)this).GetComponent<MeshRenderer>();
		}
		if ((Object)(object)_mesh == (Object)null)
		{
			_mesh = new Mesh
			{
				name = "RingStyleLineExtraQuad"
			};
			((Object)_mesh).hideFlags = (HideFlags)52;
		}
		Material val = (((Object)(object)_sharedMaterial != (Object)null) ? _sharedMaterial : Mat);
		float num = (((Object)(object)val != (Object)null && val.HasProperty("_WindowStart")) ? val.GetFloat("_WindowStart") : 0f);
		float num2 = _angleDeg * (MathF.PI / 180f);
		float c = Mathf.Cos(num2);
		float s = Mathf.Sin(num2);
		float ox = s * _ringRadius;
		float oy = c * _ringRadius;
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector(1f, (float)(-_styleIndex));
		Vector2 val3 = default(Vector2);
		((Vector2)(ref val3))._002Ector(1f, (float)RectId);
		Color vertexColor = _vertexColor;
		_mesh.Clear();
		_mesh.vertices = (Vector3[])(object)new Vector3[4]
		{
			P(-0.5f, -0.5f),
			P(-0.5f, 0.5f),
			P(0.5f, 0.5f),
			P(0.5f, -0.5f)
		};
		_mesh.uv = (Vector2[])(object)new Vector2[4]
		{
			new Vector2(num, 0f),
			new Vector2(num, 1f),
			new Vector2(num + 1f, 1f),
			new Vector2(num + 1f, 0f)
		};
		_mesh.uv2 = (Vector2[])(object)new Vector2[4] { val2, val2, val2, val2 };
		_mesh.uv3 = (Vector2[])(object)new Vector2[4] { val3, val3, val3, val3 };
		_mesh.colors = (Color[])(object)new Color[4] { vertexColor, vertexColor, vertexColor, vertexColor };
		_mesh.normals = (Vector3[])(object)new Vector3[4]
		{
			Vector3.forward,
			Vector3.forward,
			Vector3.forward,
			Vector3.forward
		};
		_mesh.triangles = new int[6] { 0, 1, 2, 0, 2, 3 };
		_mesh.RecalculateBounds();
		_filter.sharedMesh = _mesh;
		AssignMaterial();
		Vector3 P(float px, float py)
		{
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			return new Vector3(px * c + py * s + ox, (0f - px) * s + py * c + oy, 0f);
		}
	}

	private void AssignMaterial()
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
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
		if ((Object)(object)_runtimeMat == (Object)null || (Object)(object)_runtimeSource != (Object)(object)_sharedMaterial)
		{
			if ((Object)(object)_runtimeMat != (Object)null)
			{
				Object.Destroy((Object)(object)_runtimeMat);
			}
			_runtimeMat = new Material(_sharedMaterial)
			{
				name = ((Object)_sharedMaterial).name + " (LockHeart)"
			};
			((Object)_runtimeMat).hideFlags = (HideFlags)61;
			_runtimeSource = _sharedMaterial;
		}
		((Renderer)_renderer).sharedMaterial = _runtimeMat;
		((Renderer)_renderer).SetPropertyBlock((MaterialPropertyBlock)null);
	}
}
