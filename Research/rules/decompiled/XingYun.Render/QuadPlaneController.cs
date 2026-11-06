using UnityEngine;

namespace XingYun.Render;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public sealed class QuadPlaneController : MonoBehaviour
{
	public enum PlaneOrientation
	{
		XY,
		XZ
	}

	[Header("Size")]
	[Tooltip("面片宽度（世界单位）")]
	[SerializeField]
	public float width = 1f;

	[Tooltip("面片高度（世界单位）")]
	[SerializeField]
	private float height = 1f;

	[Header("Appearance")]
	[SerializeField]
	private Material sharedMaterial;

	[Tooltip("贴图色调，与顶点色相乘")]
	[SerializeField]
	private Color vertexColor = Color.white;

	[Header("Build")]
	[Tooltip("OnEnable 时自动构建")]
	[SerializeField]
	private bool buildOnEnable = true;

	[Tooltip("面片朝向轴，XY = 朝相机（默认），XZ = 水平平铺")]
	[SerializeField]
	private PlaneOrientation orientation;

	private MeshFilter meshFilter;

	private MeshRenderer meshRenderer;

	private Mesh mesh;

	private Material _styleMaterial;

	public float Width => width;

	public float Height => height;

	private void Awake()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		meshFilter = ((Component)this).GetComponent<MeshFilter>();
		meshRenderer = ((Component)this).GetComponent<MeshRenderer>();
		mesh = new Mesh
		{
			name = "QuadPlane"
		};
		meshFilter.sharedMesh = mesh;
	}

	private void OnEnable()
	{
		if (buildOnEnable)
		{
			Build();
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)mesh != (Object)null)
		{
			if (Application.isPlaying)
			{
				Object.Destroy((Object)(object)mesh);
			}
			else
			{
				Object.DestroyImmediate((Object)(object)mesh);
			}
		}
	}

	public void SetupWidthAndHeight(float newWidth, float newHeight)
	{
		width = Mathf.Max(0.001f, newWidth);
		height = Mathf.Max(0.001f, newHeight);
		Build();
	}

	public void Build()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Invalid comparison between Unknown and I4
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)mesh == (Object)null)
		{
			mesh = new Mesh
			{
				name = "QuadPlane"
			};
		}
		if ((Object)(object)meshFilter == (Object)null)
		{
			meshFilter = ((Component)this).GetComponent<MeshFilter>();
		}
		if ((Object)(object)meshRenderer == (Object)null)
		{
			meshRenderer = ((Component)this).GetComponent<MeshRenderer>();
		}
		mesh.Clear();
		float num = width * 0.5f;
		float num2 = height * 0.5f;
		Vector3[] vertices = (Vector3[])(object)((orientation != PlaneOrientation.XZ) ? new Vector3[4]
		{
			new Vector3(0f - num, 0f - num2, 0f),
			new Vector3(num, 0f - num2, 0f),
			new Vector3(0f - num, num2, 0f),
			new Vector3(num, num2, 0f)
		} : new Vector3[4]
		{
			new Vector3(0f - num, 0f, 0f - num2),
			new Vector3(num, 0f, 0f - num2),
			new Vector3(0f - num, 0f, num2),
			new Vector3(num, 0f, num2)
		});
		Vector2[] uv = (Vector2[])(object)new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 1f)
		};
		int[] triangles = new int[6] { 0, 2, 1, 1, 2, 3 };
		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.triangles = triangles;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		meshFilter.sharedMesh = mesh;
		Color32 val = Color32.op_Implicit(((int)QualitySettings.activeColorSpace == 1) ? ((Color)(ref vertexColor)).linear : vertexColor);
		mesh.colors32 = (Color32[])(object)new Color32[4] { val, val, val, val };
		ApplyMaterial();
	}

	public void BindSharedMaterial(Material material)
	{
		if (!((Object)(object)material == (Object)null))
		{
			_styleMaterial = material;
			if ((Object)(object)meshRenderer == (Object)null)
			{
				meshRenderer = ((Component)this).GetComponent<MeshRenderer>();
			}
			if ((Object)(object)meshRenderer != (Object)null)
			{
				((Renderer)meshRenderer).sharedMaterial = material;
			}
		}
	}

	private void ApplyMaterial()
	{
		if ((Object)(object)meshRenderer == (Object)null)
		{
			meshRenderer = ((Component)this).GetComponent<MeshRenderer>();
		}
		if (!((Object)(object)meshRenderer == (Object)null))
		{
			Material val = (((Object)(object)_styleMaterial != (Object)null) ? _styleMaterial : sharedMaterial);
			if ((Object)(object)val != (Object)null)
			{
				((Renderer)meshRenderer).sharedMaterial = val;
			}
		}
	}
}
