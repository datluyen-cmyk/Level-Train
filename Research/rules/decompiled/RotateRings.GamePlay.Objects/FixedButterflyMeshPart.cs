using UnityEngine;

namespace RotateRings.GamePlay.Objects;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public sealed class FixedButterflyMeshPart : MonoBehaviour
{
	public enum PartRole
	{
		Bridge,
		SlotCatch
	}

	private const float MinNaturalLength = 0.0001f;

	private const int SliceColumnCount = 4;

	private const int SliceRowCount = 2;

	[SerializeField]
	private PartRole role;

	[SerializeField]
	private float quadWidth = 0.5f;

	[SerializeField]
	private float quadHeight = 0.5f;

	[SerializeField]
	private Vector3 localCenter;

	[SerializeField]
	private float zOffset;

	[SerializeField]
	private Material sharedMaterial;

	[SerializeField]
	private Color vertexColor = Color.white;

	[SerializeField]
	private int bridgeFacingLocalXSign;

	[SerializeField]
	[Min(0f)]
	private float capStartLength = 0.0875f;

	[SerializeField]
	[Min(0f)]
	private float capEndLength = 0.0875f;

	[Header("Baked Mesh")]
	[SerializeField]
	private bool _meshBaked;

	private MeshFilter meshFilter;

	private MeshRenderer meshRenderer;

	private Mesh mesh;

	public PartRole Role => role;

	public bool MeshBaked => _meshBaked;

	public float QuadWidth => quadWidth;

	public float NaturalLength
	{
		get
		{
			if (role != PartRole.Bridge)
			{
				return 0f;
			}
			return ResolveNaturalLength();
		}
	}

	private void Awake()
	{
		meshFilter = ((Component)this).GetComponent<MeshFilter>();
		meshRenderer = ((Component)this).GetComponent<MeshRenderer>();
		if (_meshBaked && (Object)(object)meshFilter != (Object)null && (Object)(object)meshFilter.sharedMesh != (Object)null)
		{
			mesh = meshFilter.sharedMesh;
		}
	}

	internal bool IsRenderableMeshMissing()
	{
		if ((Object)(object)meshFilter == (Object)null)
		{
			meshFilter = ((Component)this).GetComponent<MeshFilter>();
		}
		Mesh val = (((Object)(object)meshFilter != (Object)null) ? meshFilter.sharedMesh : null);
		if (!((Object)(object)val == (Object)null))
		{
			return val.vertexCount == 0;
		}
		return true;
	}

	private void OnDestroy()
	{
		if (!((Object)(object)mesh == (Object)null) && !_meshBaked)
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

	public void SetRole(PartRole partRole)
	{
		role = partRole;
	}

	public void SetBridgeFacingLocalX(int sign)
	{
		bridgeFacingLocalXSign = ((sign < 0) ? (-1) : ((sign > 0) ? 1 : 0));
	}

	public void SetSharedMaterial(Material material)
	{
		sharedMaterial = material;
		if ((Object)(object)meshRenderer == (Object)null)
		{
			meshRenderer = ((Component)this).GetComponent<MeshRenderer>();
		}
		if ((Object)(object)meshRenderer != (Object)null && (Object)(object)sharedMaterial != (Object)null)
		{
			((Renderer)meshRenderer).sharedMaterial = sharedMaterial;
		}
	}

	public void ConfigureQuad(float width, float height, Vector3 center, float z = 0f)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		quadWidth = Mathf.Max(0.001f, width);
		quadHeight = Mathf.Max(0.001f, height);
		localCenter = center;
		zOffset = z;
	}

	public void ApplyVertexColor(Color color)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Invalid comparison between Unknown and I4
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		if (color == default(Color))
		{
			return;
		}
		vertexColor = color;
		if ((Object)(object)meshFilter == (Object)null)
		{
			meshFilter = ((Component)this).GetComponent<MeshFilter>();
		}
		Mesh val = (((Object)(object)meshFilter != (Object)null) ? meshFilter.sharedMesh : mesh);
		if (!((Object)(object)val == (Object)null) && val.vertexCount != 0)
		{
			Color32 val2 = (((int)QualitySettings.activeColorSpace == 1) ? Color32.op_Implicit(((Color)(ref color)).linear) : Color32.op_Implicit(color));
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

	public void Rebuild()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		if ((Object)(object)this == (Object)null)
		{
			return;
		}
		if ((Object)(object)meshFilter == (Object)null)
		{
			meshFilter = ((Component)this).GetComponent<MeshFilter>();
		}
		if ((Object)(object)meshRenderer == (Object)null)
		{
			meshRenderer = ((Component)this).GetComponent<MeshRenderer>();
		}
		if ((Object)(object)mesh == (Object)null)
		{
			mesh = new Mesh
			{
				name = "FixedButterflyPart"
			};
		}
		else if (_meshBaked)
		{
			mesh = Object.Instantiate<Mesh>(mesh);
			((Object)mesh).name = "FixedButterflyPart";
			_meshBaked = false;
			if ((Object)(object)meshFilter != (Object)null)
			{
				meshFilter.sharedMesh = mesh;
			}
		}
		BuildQuadMesh(mesh);
		meshFilter.sharedMesh = mesh;
		if ((Object)(object)sharedMaterial != (Object)null && (Object)(object)meshRenderer != (Object)null)
		{
			((Renderer)meshRenderer).sharedMaterial = sharedMaterial;
		}
	}

	private void BuildQuadMesh(Mesh target)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		Color32 vc = (((int)QualitySettings.activeColorSpace == 1) ? Color32.op_Implicit(((Color)(ref vertexColor)).linear) : Color32.op_Implicit(vertexColor));
		target.Clear();
		float num = ((role == PartRole.Bridge) ? ResolveNaturalLength() : 0f);
		if (num > 0.0001f)
		{
			BuildSlicedQuadMesh(target, vc, num);
		}
		else
		{
			BuildPlainQuadMesh(target, vc);
		}
		target.RecalculateBounds();
		target.RecalculateNormals();
	}

	private float ResolveNaturalLength()
	{
		Texture val = (((Object)(object)sharedMaterial != (Object)null) ? sharedMaterial.mainTexture : null);
		if ((Object)(object)val == (Object)null || val.width <= 0 || val.height <= 0)
		{
			return 0f;
		}
		return quadHeight * ((float)val.height / (float)val.width);
	}

	private void ResolveCapLengths(float length, float natural, out float capStartLen, out float capEndLen)
	{
		float num = capStartLength + capEndLength;
		if (num <= 0f)
		{
			capStartLen = 0f;
			capEndLen = 0f;
			return;
		}
		if (length >= num)
		{
			capStartLen = capStartLength;
			capEndLen = capEndLength;
		}
		else
		{
			float num2 = capStartLength / num;
			capStartLen = length * num2;
			capEndLen = length - capStartLen;
		}
		float num3 = capStartLen + capEndLen;
		float num4 = natural * 0.999f;
		if (num3 > num4)
		{
			float num5 = num4 / num3;
			capStartLen *= num5;
			capEndLen *= num5;
		}
	}

	private void BuildPlainQuadMesh(Mesh target, Color32 vc)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		float num = quadWidth * 0.5f;
		float num2 = quadHeight * 0.5f;
		Vector3 val = localCenter;
		float num3 = zOffset;
		Vector3[] vertices = (Vector3[])(object)new Vector3[4]
		{
			new Vector3(val.x - num, val.y - num2, num3),
			new Vector3(val.x + num, val.y - num2, num3),
			new Vector3(val.x - num, val.y + num2, num3),
			new Vector3(val.x + num, val.y + num2, num3)
		};
		Vector2[] uv = (Vector2[])(object)new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 0f),
			new Vector2(1f, 1f)
		};
		target.vertices = vertices;
		target.uv = uv;
		target.triangles = new int[6] { 0, 2, 1, 1, 2, 3 };
		target.colors32 = (Color32[])(object)new Color32[4] { vc, vc, vc, vc };
	}

	private void BuildSlicedQuadMesh(Mesh target, Color32 vc, float natural)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		float num = quadHeight * 0.5f;
		Vector3 val = localCenter;
		float num2 = zOffset;
		float num3 = quadWidth;
		float num4 = val.x - num3 * 0.5f;
		ResolveCapLengths(num3, natural, out var capStartLen, out var capEndLen);
		float[] array = new float[4]
		{
			0f,
			capStartLen,
			num3 - capEndLen,
			num3
		};
		float[] array2 = new float[4]
		{
			0f,
			capStartLen / natural,
			1f - capEndLen / natural,
			1f
		};
		int num5 = 8;
		Vector3[] array3 = (Vector3[])(object)new Vector3[num5];
		Vector2[] array4 = (Vector2[])(object)new Vector2[num5];
		for (int i = 0; i < 2; i++)
		{
			float num6 = i;
			float num7 = val.y + ((i == 0) ? (0f - num) : num);
			for (int j = 0; j < 4; j++)
			{
				int num8 = i * 4 + j;
				array3[num8] = new Vector3(num4 + array[j], num7, num2);
				array4[num8] = new Vector2(num6, array2[j]);
			}
		}
		int[] array5 = new int[18];
		int num9 = 0;
		for (int k = 0; k < 3; k++)
		{
			int num10 = k;
			int num11 = k + 1;
			int num12 = 4 + k;
			int num13 = 4 + k + 1;
			array5[num9++] = num10;
			array5[num9++] = num12;
			array5[num9++] = num11;
			array5[num9++] = num11;
			array5[num9++] = num12;
			array5[num9++] = num13;
		}
		Color32[] array6 = (Color32[])(object)new Color32[num5];
		for (int l = 0; l < num5; l++)
		{
			array6[l] = vc;
		}
		target.vertices = array3;
		target.uv = array4;
		target.triangles = array5;
		target.colors32 = array6;
	}
}
