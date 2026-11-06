using System.Collections.Generic;
using UnityEngine;

namespace XingYun.Render;

public sealed class ArrowPathMeshBuilder
{
	private const float Epsilon = 0.0001f;

	private readonly List<Vector3> _cleaned = new List<Vector3>(32);

	private readonly List<Vector3> _centerline = new List<Vector3>(128);

	private readonly List<Vector3> _verts = new List<Vector3>(256);

	private readonly List<Vector3> _normals = new List<Vector3>(256);

	private readonly List<Vector4> _tangents = new List<Vector4>(256);

	private readonly List<Color32> _colors = new List<Color32>(256);

	private readonly List<Vector2> _uv0 = new List<Vector2>(256);

	private readonly List<Vector2> _uv1 = new List<Vector2>(256);

	private readonly List<Vector2> _uv2 = new List<Vector2>(256);

	private readonly List<int> _tris = new List<int>(512);

	public ArrowGuidePath Build(IList<Vector3> inputTailToHead, ArrowRibbonBuildSettings s, Mesh mesh, ArrowGuidePath reuse = null, bool uploadTBN = true, Material sharedMaterial = null, Color32? vertexColor = null)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)mesh == (Object)null)
		{
			return null;
		}
		Vector3 forward = Vector3.forward;
		CleanPoints(inputTailToHead, forward);
		BuildRoundedCenterline(s, forward);
		if (_centerline.Count < 2)
		{
			mesh.Clear();
			ArrowGuidePath obj = reuse ?? new ArrowGuidePath();
			obj.Count = 0;
			obj.OriginalLength = 0f;
			obj.ExitGuideLength = 0f;
			obj.PlaneNormal = forward;
			return obj;
		}
		int num = _centerline.Count - 1;
		float num2 = 0f;
		for (int i = 1; i <= num; i++)
		{
			num2 += Vector3.Distance(_centerline[i], _centerline[i - 1]);
		}
		float visibleLength = num2;
		float num3 = Mathf.Max(0f, s.exitExtraLength);
		if (num3 > 0.0001f && _centerline.Count >= 2)
		{
			Vector3 val = _centerline[_centerline.Count - 1];
			Vector3 val2 = _centerline[_centerline.Count - 2];
			Vector3 val3 = val - val2;
			float magnitude = ((Vector3)(ref val3)).magnitude;
			if (magnitude > 0.0001f)
			{
				val3 /= magnitude;
				_centerline.Add(val + val3 * num3);
			}
		}
		int count = _centerline.Count;
		ArrowGuidePath arrowGuidePath = reuse ?? new ArrowGuidePath();
		EnsureGuideArrays(arrowGuidePath, count);
		arrowGuidePath.Count = count;
		arrowGuidePath.PlaneNormal = forward;
		float num4 = 0f;
		arrowGuidePath.Distances[0] = 0f;
		for (int j = 1; j < count; j++)
		{
			num4 += Vector3.Distance(_centerline[j], _centerline[j - 1]);
			arrowGuidePath.Distances[j] = num4;
		}
		arrowGuidePath.ExitGuideLength = num4;
		arrowGuidePath.OriginalLength = arrowGuidePath.Distances[Mathf.Clamp(num, 0, count - 1)];
		for (int k = 0; k < count; k++)
		{
			arrowGuidePath.Positions[k] = _centerline[k];
			arrowGuidePath.Tangents[k] = ComputeTangent(k, count);
		}
		Color32 val4 = (Color32)(((_003F?)vertexColor) ?? new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
		if (val4.a == 0 && val4.r == 0 && val4.g == 0 && val4.b == 0)
		{
			((Color32)(ref val4))._002Ector(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		}
		BuildRibbonMesh(arrowGuidePath, s, forward, mesh, uploadTBN, visibleLength, val4);
		return arrowGuidePath;
	}

	private void CleanPoints(IList<Vector3> input, Vector3 planeN)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		_cleaned.Clear();
		if (input == null)
		{
			return;
		}
		Vector3 val2;
		for (int i = 0; i < input.Count; i++)
		{
			Vector3 val = input[i];
			if (_cleaned.Count != 0)
			{
				val2 = val - _cleaned[_cleaned.Count - 1];
				if (!(((Vector3)(ref val2)).sqrMagnitude > 9.999999E-09f))
				{
					continue;
				}
			}
			_cleaned.Add(val);
		}
		if (_cleaned.Count < 3)
		{
			return;
		}
		int num = 1;
		int num2 = _cleaned.Count - 1;
		for (int j = 1; j < num2; j++)
		{
			Vector3 val3 = _cleaned[j] - _cleaned[num - 1];
			Vector3 val4 = _cleaned[j + 1] - _cleaned[j];
			if (!(((Vector3)(ref val3)).sqrMagnitude < 9.999999E-09f) && !(((Vector3)(ref val4)).sqrMagnitude < 9.999999E-09f))
			{
				((Vector3)(ref val3)).Normalize();
				((Vector3)(ref val4)).Normalize();
				val2 = Vector3.Cross(val3, val4);
				if (((Vector3)(ref val2)).magnitude >= 0.001f || Vector3.Dot(val3, val4) <= 0f)
				{
					_cleaned[num++] = _cleaned[j];
				}
			}
		}
		_cleaned[num++] = _cleaned[num2];
		_cleaned.RemoveRange(num, _cleaned.Count - num);
	}

	private void BuildRoundedCenterline(ArrowRibbonBuildSettings s, Vector3 planeN)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		_centerline.Clear();
		int count = _cleaned.Count;
		if (count == 0)
		{
			return;
		}
		if (count < 3)
		{
			for (int i = 0; i < count; i++)
			{
				_centerline.Add(_cleaned[i]);
			}
			return;
		}
		int num = Mathf.Max(1, s.cornerSegments);
		float num2 = Mathf.Max(0f, s.cornerRadius);
		_centerline.Add(_cleaned[0]);
		Vector3 val15;
		for (int j = 1; j < count - 1; j++)
		{
			Vector3 val = _cleaned[j - 1];
			Vector3 val2 = _cleaned[j];
			Vector3 val3 = _cleaned[j + 1];
			Vector3 val4 = val2 - val;
			Vector3 val5 = val3 - val2;
			float magnitude = ((Vector3)(ref val4)).magnitude;
			float magnitude2 = ((Vector3)(ref val5)).magnitude;
			if (magnitude < 0.0001f || magnitude2 < 0.0001f)
			{
				_centerline.Add(val2);
				continue;
			}
			Vector3 val6 = val4 / magnitude;
			Vector3 val7 = val5 / magnitude2;
			float num3 = Mathf.Acos(Mathf.Clamp(Vector3.Dot(-val6, val7), -1f, 1f));
			if (num3 < 0.02f || num3 > 3.1215928f)
			{
				_centerline.Add(val2);
				continue;
			}
			float num4 = num3 * 0.5f;
			float num5 = Mathf.Tan(num4);
			float num6 = Mathf.Sin(num4);
			float num7 = ((num5 > 0.0001f) ? (num2 / num5) : 0f);
			float num8 = 0.5f * Mathf.Min(magnitude, magnitude2);
			float num9 = Mathf.Min(num7, num8);
			float num10 = num9 * num5;
			if (num9 < 0.0001f || num10 < 0.0001f)
			{
				_centerline.Add(val2);
				continue;
			}
			Vector3 val8 = val2 - val6 * num9;
			Vector3 val9 = val2 + val7 * num9;
			Vector3 val10 = -val6 + val7;
			if (((Vector3)(ref val10)).sqrMagnitude < 9.999999E-09f)
			{
				_centerline.Add(val2);
				continue;
			}
			((Vector3)(ref val10)).Normalize();
			Vector3 val11 = val2 + val10 * (num10 / num6);
			Vector3 val12 = val8 - val11;
			Vector3 val13 = val9 - val11;
			for (int k = 0; k <= num; k++)
			{
				float num11 = (float)k / (float)num;
				Vector3 val14 = val11 + Vector3.Slerp(val12, val13, num11);
				val15 = val14 - _centerline[_centerline.Count - 1];
				if (((Vector3)(ref val15)).sqrMagnitude > 9.999999E-09f)
				{
					_centerline.Add(val14);
				}
			}
		}
		val15 = _cleaned[count - 1] - _centerline[_centerline.Count - 1];
		if (((Vector3)(ref val15)).sqrMagnitude > 9.999999E-09f)
		{
			_centerline.Add(_cleaned[count - 1]);
		}
	}

	private Vector3 ComputeTangent(int i, int count)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((i == 0) ? (_centerline[1] - _centerline[0]) : ((i != count - 1) ? (_centerline[i + 1] - _centerline[i - 1]) : (_centerline[count - 1] - _centerline[count - 2])));
		if (((Vector3)(ref val)).sqrMagnitude < 9.999999E-09f)
		{
			val = Vector3.right;
		}
		return ((Vector3)(ref val)).normalized;
	}

	private void BuildRibbonMesh(ArrowGuidePath path, ArrowRibbonBuildSettings s, Vector3 planeN, Mesh mesh, bool uploadTBN, float visibleLength, Color32 vertexColor)
	{
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		int count = path.Count;
		float num = Mathf.Max(0f, s.width) * 0.5f;
		float num2 = s.styleIndex + 1;
		int num3 = Mathf.Clamp(s.widthSubdivisions, 0, 8) + 2;
		_verts.Clear();
		_normals.Clear();
		_tangents.Clear();
		_colors.Clear();
		_uv0.Clear();
		_uv1.Clear();
		_uv2.Clear();
		_tris.Clear();
		float originalLength = path.OriginalLength;
		float num4 = ((originalLength > 0.0001f) ? (visibleLength / originalLength) : 1f);
		Vector4 item = default(Vector4);
		for (int i = 0; i < count; i++)
		{
			Vector3 val = path.Tangents[i];
			Vector3 val2 = Vector3.Cross(planeN, val);
			Vector3 normalized = ((Vector3)(ref val2)).normalized;
			Vector3 val3 = path.Positions[i];
			float num5 = path.Distances[i] * num4;
			((Vector4)(ref item))._002Ector(val.x, val.y, val.z, -1f);
			for (int j = 0; j < num3; j++)
			{
				float num6 = (float)j / (float)(num3 - 1);
				float num7 = num - num6 * (2f * num);
				_verts.Add(val3 + normalized * num7);
				if (uploadTBN)
				{
					_normals.Add(Vector3.forward);
					_tangents.Add(item);
				}
				_colors.Add(vertexColor);
				_uv0.Add(new Vector2(num5, num6));
				_uv1.Add(new Vector2(visibleLength, num2));
				_uv2.Add(new Vector2(1f, 0f));
			}
		}
		for (int k = 0; k < count - 1; k++)
		{
			int num8 = k * num3;
			int num9 = (k + 1) * num3;
			for (int l = 0; l < num3 - 1; l++)
			{
				int item2 = num8 + l;
				int item3 = num8 + l + 1;
				int item4 = num9 + l;
				int item5 = num9 + l + 1;
				_tris.Add(item2);
				_tris.Add(item4);
				_tris.Add(item3);
				_tris.Add(item3);
				_tris.Add(item4);
				_tris.Add(item5);
			}
		}
		mesh.Clear();
		mesh.SetVertices(_verts);
		if (uploadTBN)
		{
			mesh.SetNormals(_normals);
			mesh.SetTangents(_tangents);
		}
		mesh.SetColors(_colors);
		mesh.SetUVs(0, _uv0);
		mesh.SetUVs(1, _uv1);
		mesh.SetUVs(2, _uv2);
		mesh.SetTriangles(_tris, 0);
		mesh.RecalculateBounds();
	}

	private static void EnsureGuideArrays(ArrowGuidePath path, int count)
	{
		if (path.Positions == null || path.Positions.Length < count)
		{
			path.Positions = (Vector3[])(object)new Vector3[count];
			path.Tangents = (Vector3[])(object)new Vector3[count];
			path.Distances = new float[count];
		}
	}
}
