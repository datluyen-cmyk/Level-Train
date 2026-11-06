using System.Collections.Generic;
using UnityEngine;

namespace XingYun.RingGame;

public sealed class RingSegmentMeshBuilder
{
	private readonly List<Vector3> _verts = new List<Vector3>(256);

	private readonly List<Vector3> _normals = new List<Vector3>(256);

	private readonly List<Vector4> _tangents = new List<Vector4>(256);

	private readonly List<Color32> _colors = new List<Color32>(256);

	private readonly List<Vector2> _uv0 = new List<Vector2>(256);

	private readonly List<Vector2> _uv1 = new List<Vector2>(256);

	private readonly List<Vector2> _uv2 = new List<Vector2>(256);

	private readonly List<int> _tris = new List<int>(512);

	public int VertexCount => _verts.Count;

	public void SetOutlineMask(int vertexIndex, float mask)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		_uv2[vertexIndex] = new Vector2(mask, _uv2[vertexIndex].y);
	}

	public void AddVertex(Vector3 pos, Vector3 normal, Vector4 tangent, Color32 color, Vector2 uv0, Vector2 uv1, float outlineMask = 1f)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		_verts.Add(pos);
		_normals.Add(normal);
		_tangents.Add(tangent);
		_colors.Add(color);
		_uv0.Add(uv0);
		_uv1.Add(uv1);
		_uv2.Add(new Vector2(outlineMask, 0f));
	}

	public void AddTriangle(int a, int b, int c)
	{
		_tris.Add(a);
		_tris.Add(b);
		_tris.Add(c);
	}

	public void Begin()
	{
		_verts.Clear();
		_normals.Clear();
		_tangents.Clear();
		_colors.Clear();
		_uv0.Clear();
		_uv1.Clear();
		_uv2.Clear();
		_tris.Clear();
	}

	public void End(Mesh mesh)
	{
		if (!((Object)(object)mesh == (Object)null))
		{
			mesh.Clear();
			mesh.SetVertices(_verts);
			mesh.SetNormals(_normals);
			mesh.SetTangents(_tangents);
			mesh.SetColors(_colors);
			mesh.SetUVs(0, _uv0);
			mesh.SetUVs(1, _uv1);
			mesh.SetUVs(2, _uv2);
			mesh.SetTriangles(_tris, 0);
			mesh.RecalculateBounds();
		}
	}

	public void AppendArc(Vector3 center, float radius, float startAngle, float endAngle, RingSegmentBuildSettings settings, bool closedLoop, float zOffset = 0f, Color32? vertexColor = null, float uvSpan = 0f)
	{
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.Max(1, settings.arcSubdivisions);
		int num2 = Mathf.Clamp(settings.widthSubdivisions, 0, 8) + 2;
		float num3 = Mathf.Max(0f, settings.width) * 0.5f;
		float num4 = endAngle - startAngle;
		float num5 = Mathf.Abs(num4) * radius;
		float num6 = settings.styleIndex + 1;
		float num7 = ((uvSpan > 0f) ? uvSpan : num5);
		float num8 = (settings.useHeadTail ? 0f : 100f);
		float num9 = (settings.useHeadTail ? num5 : 99999f);
		int count = _verts.Count;
		Color32 item = (Color32)(((_003F?)vertexColor) ?? new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
		Vector3 val = default(Vector3);
		Vector3 val2 = default(Vector3);
		Vector4 item2 = default(Vector4);
		for (int i = 0; i <= num; i++)
		{
			float num10 = (float)i / (float)num;
			float num11 = Mathf.Lerp(startAngle, endAngle, num10);
			float num12 = Mathf.Cos(num11);
			float num13 = Mathf.Sin(num11);
			((Vector3)(ref val))._002Ector(num12, num13, 0f);
			((Vector3)(ref val2))._002Ector(0f - num13, num12, 0f);
			if (num4 < 0f)
			{
				val2 = -val2;
			}
			float num14 = num10 * num7;
			((Vector4)(ref item2))._002Ector(val2.x, val2.y, val2.z, -1f);
			for (int j = 0; j < num2; j++)
			{
				float num15 = (float)j / (float)(num2 - 1);
				float num16 = num3 - num15 * (2f * num3);
				Vector3 item3;
				if (closedLoop && i == num)
				{
					item3 = _verts[count + j];
				}
				else
				{
					item3 = center + val * (radius + num16);
					item3.z += zOffset;
				}
				_verts.Add(item3);
				_normals.Add(Vector3.forward);
				_tangents.Add(item2);
				_colors.Add(item);
				_uv0.Add(new Vector2(num14 + num8, 1f - num15));
				_uv1.Add(new Vector2(num9, num6));
				_uv2.Add(new Vector2(1f, 0f));
			}
		}
		int num17 = num + 1;
		for (int k = 0; k < num17 - 1; k++)
		{
			int num18 = count + k * num2;
			int num19 = count + (k + 1) * num2;
			for (int l = 0; l < num2 - 1; l++)
			{
				int item4 = num18 + l;
				int item5 = num18 + l + 1;
				int item6 = num19 + l;
				int item7 = num19 + l + 1;
				_tris.Add(item4);
				_tris.Add(item5);
				_tris.Add(item6);
				_tris.Add(item5);
				_tris.Add(item7);
				_tris.Add(item6);
			}
		}
	}

	public void AppendStraightRibbon(Vector3 origin, Vector3 direction, Vector3 up, float width, float totalLength, float headLength, bool hasHead, int styleIndex, float zOffset = 0f, float referenceWidth = 0f, Color32? vertexColor = null)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		float num = width * 0.5f;
		float num2 = styleIndex + 1;
		Vector3 val = Vector3.Cross(up, direction);
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		Vector4 item = default(Vector4);
		((Vector4)(ref item))._002Ector(direction.x, direction.y, direction.z, -1f);
		Color32 item2 = (Color32)(((_003F?)vertexColor) ?? new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
		float num3 = ((referenceWidth > 0f && width > 0f) ? (referenceWidth / width) : 1f);
		float num4 = totalLength * num3;
		float num5 = headLength * num3;
		float num6;
		float num7;
		if (hasHead)
		{
			num6 = 100f;
			num7 = num6 + num4;
		}
		else
		{
			num6 = 100f;
			num7 = 99999f;
		}
		float[] array;
		float[] array2;
		if (hasHead && headLength > 0f && headLength < totalLength)
		{
			array = new float[3]
			{
				0f,
				totalLength - headLength,
				totalLength
			};
			array2 = new float[3]
			{
				0f,
				num4 - num5,
				num4
			};
		}
		else
		{
			array = new float[2] { 0f, totalLength };
			array2 = new float[2] { 0f, num4 };
		}
		int count = _verts.Count;
		for (int i = 0; i < array.Length; i++)
		{
			float num8 = array[i];
			float num9 = array2[i];
			Vector3 val2 = origin + direction * num8;
			Vector3 item3 = val2 + normalized * num;
			Vector3 item4 = val2 - normalized * num;
			item3.z += zOffset;
			item4.z += zOffset;
			_verts.Add(item3);
			_verts.Add(item4);
			_normals.Add(Vector3.forward);
			_tangents.Add(item);
			_colors.Add(item2);
			_uv0.Add(new Vector2(num9 + num6, 1f));
			_uv1.Add(new Vector2(num7, num2));
			_uv2.Add(new Vector2(1f, 0f));
			_normals.Add(Vector3.forward);
			_tangents.Add(item);
			_colors.Add(item2);
			_uv0.Add(new Vector2(num9 + num6, 0f));
			_uv1.Add(new Vector2(num7, num2));
			_uv2.Add(new Vector2(1f, 0f));
		}
		for (int j = 0; j < array.Length - 1; j++)
		{
			int item5 = count + j * 2;
			int item6 = count + j * 2 + 1;
			int item7 = count + (j + 1) * 2;
			int item8 = count + (j + 1) * 2 + 1;
			_tris.Add(item5);
			_tris.Add(item6);
			_tris.Add(item7);
			_tris.Add(item6);
			_tris.Add(item8);
			_tris.Add(item7);
		}
	}

	public void AppendStraightRibbonWithCaps(Vector3 bodyOrigin, Vector3 direction, Vector3 up, float width, float bodyLength, float capLength, float matTailLength, float matHeadLength, int styleIndex, float zOffset = 0f, float referenceWidth = 0f, Color32? vertexColor = null)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		float num = width * 0.5f;
		float num2 = styleIndex + 1;
		Vector3 val = Vector3.Cross(up, direction);
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		Vector4 item = default(Vector4);
		((Vector4)(ref item))._002Ector(direction.x, direction.y, direction.z, -1f);
		Color32 item2 = (Color32)(((_003F?)vertexColor) ?? new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
		float num3 = ((referenceWidth > 0f && width > 0f) ? (referenceWidth / width) : 1f);
		float num4 = bodyLength * num3;
		float num5 = matTailLength + num4 + matHeadLength;
		float num6 = capLength + bodyLength + capLength;
		float[] array = new float[4]
		{
			0f,
			capLength,
			capLength + bodyLength,
			num6
		};
		float[] array2 = new float[4]
		{
			0f,
			matTailLength,
			matTailLength + num4,
			num5
		};
		Vector3 val2 = bodyOrigin - direction * capLength;
		int count = _verts.Count;
		for (int i = 0; i < array.Length; i++)
		{
			float num7 = array[i];
			float num8 = array2[i];
			Vector3 val3 = val2 + direction * num7;
			Vector3 item3 = val3 + normalized * num;
			Vector3 item4 = val3 - normalized * num;
			item3.z += zOffset;
			item4.z += zOffset;
			_verts.Add(item3);
			_verts.Add(item4);
			_normals.Add(Vector3.forward);
			_tangents.Add(item);
			_colors.Add(item2);
			_uv0.Add(new Vector2(num8, 1f));
			_uv1.Add(new Vector2(num5, num2));
			_uv2.Add(new Vector2(1f, 0f));
			_normals.Add(Vector3.forward);
			_tangents.Add(item);
			_colors.Add(item2);
			_uv0.Add(new Vector2(num8, 0f));
			_uv1.Add(new Vector2(num5, num2));
			_uv2.Add(new Vector2(1f, 0f));
		}
		for (int j = 0; j < array.Length - 1; j++)
		{
			int item5 = count + j * 2;
			int item6 = count + j * 2 + 1;
			int item7 = count + (j + 1) * 2;
			int item8 = count + (j + 1) * 2 + 1;
			_tris.Add(item5);
			_tris.Add(item6);
			_tris.Add(item7);
			_tris.Add(item6);
			_tris.Add(item8);
			_tris.Add(item7);
		}
	}

	public void AppendAxisAlignedQuad(Vector3 anchorPos, Vector3 outwardDir, float width, float length, float uvXStart, float uvXEnd, float visibleLength, int styleIndex, float zOffset = 0f, Color32? vertexColor = null)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		float num = width * 0.5f;
		float num2 = styleIndex + 1;
		Color32 item = (Color32)(((_003F?)vertexColor) ?? new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
		Vector4 item2 = default(Vector4);
		((Vector4)(ref item2))._002Ector(outwardDir.x, outwardDir.y, outwardDir.z, -1f);
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(0f - outwardDir.y, outwardDir.x, 0f);
		Vector3 val2 = anchorPos + outwardDir * length;
		Vector3 item3 = anchorPos - val * num;
		Vector3 item4 = anchorPos + val * num;
		Vector3 item5 = val2 + val * num;
		Vector3 item6 = val2 - val * num;
		item3.z += zOffset;
		item4.z += zOffset;
		item5.z += zOffset;
		item6.z += zOffset;
		int count = _verts.Count;
		_verts.Add(item3);
		_normals.Add(Vector3.forward);
		_tangents.Add(item2);
		_colors.Add(item);
		_uv0.Add(new Vector2(uvXStart, 0f));
		_uv1.Add(new Vector2(visibleLength, num2));
		_uv2.Add(new Vector2(1f, 0f));
		_verts.Add(item4);
		_normals.Add(Vector3.forward);
		_tangents.Add(item2);
		_colors.Add(item);
		_uv0.Add(new Vector2(uvXStart, 1f));
		_uv1.Add(new Vector2(visibleLength, num2));
		_uv2.Add(new Vector2(1f, 0f));
		_verts.Add(item5);
		_normals.Add(Vector3.forward);
		_tangents.Add(item2);
		_colors.Add(item);
		_uv0.Add(new Vector2(uvXEnd, 1f));
		_uv1.Add(new Vector2(visibleLength, num2));
		_uv2.Add(new Vector2(1f, 0f));
		_verts.Add(item6);
		_normals.Add(Vector3.forward);
		_tangents.Add(item2);
		_colors.Add(item);
		_uv0.Add(new Vector2(uvXEnd, 0f));
		_uv1.Add(new Vector2(visibleLength, num2));
		_uv2.Add(new Vector2(1f, 0f));
		_tris.Add(count);
		_tris.Add(count + 1);
		_tris.Add(count + 2);
		_tris.Add(count);
		_tris.Add(count + 2);
		_tris.Add(count + 3);
	}

	public void AppendExtraCrossRibbon(Vector3 origin, Vector3 direction, Vector3 up, float width, float length, int styleIndex, float zOffset = 0f, float referenceWidth = 0f, Color32? vertexColor = null)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		float num = width * 0.5f;
		float num2 = styleIndex + 1;
		Vector3 val = Vector3.Cross(up, direction);
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		Vector4 item = default(Vector4);
		((Vector4)(ref item))._002Ector(direction.x, direction.y, direction.z, -1f);
		Color32 item2 = (Color32)(((_003F?)vertexColor) ?? new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
		float num3 = ((referenceWidth > 0f && width > 0f) ? (referenceWidth / width) : 1f);
		float num4 = length * num3;
		float num5 = num4;
		float num6 = 0f - num2;
		int count = _verts.Count;
		Vector3 item3 = origin + normalized * num;
		Vector3 item4 = origin - normalized * num;
		item3.z += zOffset;
		item4.z += zOffset;
		_verts.Add(item3);
		_normals.Add(Vector3.forward);
		_tangents.Add(item);
		_colors.Add(item2);
		_uv0.Add(new Vector2(0f, 1f));
		_uv1.Add(new Vector2(num5, num6));
		_uv2.Add(new Vector2(1f, 0f));
		_verts.Add(item4);
		_normals.Add(Vector3.forward);
		_tangents.Add(item);
		_colors.Add(item2);
		_uv0.Add(new Vector2(0f, 0f));
		_uv1.Add(new Vector2(num5, num6));
		_uv2.Add(new Vector2(1f, 0f));
		Vector3 val2 = origin + direction * length;
		Vector3 item5 = val2 + normalized * num;
		Vector3 item6 = val2 - normalized * num;
		item5.z += zOffset;
		item6.z += zOffset;
		_verts.Add(item5);
		_normals.Add(Vector3.forward);
		_tangents.Add(item);
		_colors.Add(item2);
		_uv0.Add(new Vector2(num4, 1f));
		_uv1.Add(new Vector2(num5, num6));
		_uv2.Add(new Vector2(1f, 0f));
		_verts.Add(item6);
		_normals.Add(Vector3.forward);
		_tangents.Add(item);
		_colors.Add(item2);
		_uv0.Add(new Vector2(num4, 0f));
		_uv1.Add(new Vector2(num5, num6));
		_uv2.Add(new Vector2(1f, 0f));
		_tris.Add(count);
		_tris.Add(count + 1);
		_tris.Add(count + 2);
		_tris.Add(count + 1);
		_tris.Add(count + 3);
		_tris.Add(count + 2);
	}

	public void Build(Vector3 center, float radius, float startAngle, float endAngle, RingSegmentBuildSettings settings, Mesh mesh, bool closedLoop = false)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Begin();
		AppendArc(center, radius, startAngle, endAngle, settings, closedLoop);
		End(mesh);
	}
}
