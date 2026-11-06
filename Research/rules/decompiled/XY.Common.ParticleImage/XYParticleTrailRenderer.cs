using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace XY.Common.ParticleImage;

[AddComponentMenu("UI/XY Particle Image/XY Trail Renderer")]
public class XYParticleTrailRenderer : MaskableGraphic
{
	private XYParticleImage _particle;

	private Mesh _trailMesh;

	private MeshDataArray _trailMeshDataArray;

	private MeshData _trailMeshData;

	private int offset;

	private int trisOffset;

	private int trisCount;

	private bool _meshDataPrepared;

	public Mesh trailMesh
	{
		get
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Expected O, but got Unknown
			if ((Object)(object)_trailMesh == (Object)null)
			{
				_trailMesh = new Mesh();
				_trailMesh.MarkDynamic();
			}
			return _trailMesh;
		}
	}

	public XYParticleImage particle
	{
		get
		{
			return _particle;
		}
		set
		{
			_particle = value;
		}
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
	}

	protected override void UpdateGeometry()
	{
	}

	public void PrepareMeshData(int vertexCount, int particleCount)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		if (vertexCount <= 0 || particleCount <= 0)
		{
			_meshDataPrepared = false;
			trisCount = 0;
			return;
		}
		trisCount = Mathf.Max(0, (vertexCount - particleCount) * 6);
		_trailMeshDataArray = Mesh.AllocateWritableMeshData(1);
		_trailMeshData = ((MeshDataArray)(ref _trailMeshDataArray))[0];
		((MeshData)(ref _trailMeshData)).SetVertexBufferParams(vertexCount * 2, (VertexAttributeDescriptor[])(object)new VertexAttributeDescriptor[2]
		{
			new VertexAttributeDescriptor((VertexAttribute)0, (VertexAttributeFormat)0, 3, 0),
			new VertexAttributeDescriptor((VertexAttribute)3, (VertexAttributeFormat)0, 4, 1)
		});
		if (trisCount > 0)
		{
			((MeshData)(ref _trailMeshData)).SetIndexBufferParams(trisCount, (IndexFormat)0);
		}
		offset = 0;
		trisOffset = 0;
		_meshDataPrepared = true;
	}

	public void UpdateMeshData(NativeArray<Vector3> points, NativeArray<int> tris, NativeArray<Color> cols)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		if (!_meshDataPrepared || points.Length == 0)
		{
			return;
		}
		NativeArray<Vector3> vertexData = ((MeshData)(ref _trailMeshData)).GetVertexData<Vector3>(0);
		NativeArray<Color> vertexData2 = ((MeshData)(ref _trailMeshData)).GetVertexData<Color>(1);
		for (int i = 0; i < points.Length; i++)
		{
			vertexData[i + offset] = points[i];
		}
		for (int j = 0; j < cols.Length; j++)
		{
			vertexData2[j + offset] = cols[j];
		}
		if (tris.Length > 0)
		{
			NativeArray<ushort> indexData = ((MeshData)(ref _trailMeshData)).GetIndexData<ushort>();
			for (int k = 0; k < tris.Length; k++)
			{
				indexData[k + trisOffset] = (ushort)(tris[k] + offset);
			}
		}
		offset += points.Length;
		trisOffset += tris.Length;
	}

	public void SetMeshData()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (_meshDataPrepared)
		{
			SetMeshData(_trailMeshDataArray, _trailMeshData, trisCount);
			_meshDataPrepared = false;
		}
	}

	public void SetMeshData(MeshDataArray meshDataArray, MeshData meshData, int triCount)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		((MeshData)(ref meshData)).subMeshCount = 1;
		((MeshData)(ref meshData)).SetSubMesh(0, new SubMeshDescriptor(0, Mathf.Max(0, triCount), (MeshTopology)0), (MeshUpdateFlags)0);
		Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, trailMesh, (MeshUpdateFlags)8);
		trailMesh.RecalculateBounds();
		((Graphic)this).canvasRenderer.SetMesh(trailMesh);
		((Graphic)this).SetMaterialDirty();
	}

	public void Clear()
	{
		_meshDataPrepared = false;
		trisCount = 0;
		offset = 0;
		trisOffset = 0;
		trailMesh.Clear();
		((Graphic)this).canvasRenderer.SetMesh(trailMesh);
		((Graphic)this).SetMaterialDirty();
	}

	protected override void OnDestroy()
	{
		if ((Object)(object)_trailMesh != (Object)null)
		{
			if (Application.isPlaying)
			{
				Object.Destroy((Object)(object)_trailMesh);
			}
			else
			{
				Object.DestroyImmediate((Object)(object)_trailMesh);
			}
			_trailMesh = null;
		}
		((Graphic)this).OnDestroy();
	}
}
