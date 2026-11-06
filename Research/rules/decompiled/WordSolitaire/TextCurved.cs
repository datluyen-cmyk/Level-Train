using System;
using TMPro;
using UnityEngine;

namespace WordSolitaire;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(TMP_Text))]
public class TextCurved : MonoBehaviour
{
	[Tooltip("垂直向曲线 (X: 0-1 表示字符在文本左右边界的归一化位置, Y: 相对基线的偏移)")]
	public AnimationCurve vertexCurve = new AnimationCurve((Keyframe[])(object)new Keyframe[3]
	{
		new Keyframe(0f, 0f),
		new Keyframe(0.5f, 2f),
		new Keyframe(1f, 0f)
	});

	[Tooltip("曲线 Y 值缩放系数")]
	public float curveScale = 1f;

	[Tooltip("是否让字符沿曲线切线方向旋转")]
	public bool rotateToTangent = true;

	private TMP_Text textComponent;

	private bool isApplying;

	private void OnEnable()
	{
		if ((Object)(object)textComponent == (Object)null)
		{
			textComponent = ((Component)this).GetComponent<TMP_Text>();
		}
		TMPro_EventManager.TEXT_CHANGED_EVENT.Add((Action<Object>)OnTextChanged);
		ApplyCurve();
	}

	private void OnDisable()
	{
		TMPro_EventManager.TEXT_CHANGED_EVENT.Remove((Action<Object>)OnTextChanged);
		if ((Object)(object)textComponent != (Object)null)
		{
			textComponent.havePropertiesChanged = true;
			textComponent.ForceMeshUpdate(false, false);
		}
	}

	private void OnTextChanged(Object obj)
	{
		if (!isApplying && obj == (Object)(object)textComponent)
		{
			ApplyCurve();
		}
	}

	public void ApplyCurve()
	{
		if ((Object)(object)textComponent == (Object)null || isApplying)
		{
			return;
		}
		isApplying = true;
		try
		{
			ApplyCurveInternal();
		}
		finally
		{
			isApplying = false;
		}
	}

	private void ApplyCurveInternal()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		//IL_031d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		vertexCurve.preWrapMode = (WrapMode)1;
		vertexCurve.postWrapMode = (WrapMode)1;
		textComponent.havePropertiesChanged = true;
		textComponent.ForceMeshUpdate(false, false);
		TMP_TextInfo textInfo = textComponent.textInfo;
		int characterCount = textInfo.characterCount;
		if (characterCount == 0)
		{
			return;
		}
		Bounds bounds = textComponent.bounds;
		float x = ((Bounds)(ref bounds)).min.x;
		bounds = textComponent.bounds;
		float num = ((Bounds)(ref bounds)).max.x - x;
		if (num <= Mathf.Epsilon)
		{
			return;
		}
		Vector3 val3 = default(Vector3);
		for (int i = 0; i < characterCount; i++)
		{
			if (!textInfo.characterInfo[i].isVisible)
			{
				continue;
			}
			int vertexIndex = textInfo.characterInfo[i].vertexIndex;
			int materialReferenceIndex = textInfo.characterInfo[i].materialReferenceIndex;
			Vector3[] vertices = textInfo.meshInfo[materialReferenceIndex].vertices;
			Vector3 val = Vector2.op_Implicit(new Vector2((vertices[vertexIndex].x + vertices[vertexIndex + 2].x) * 0.5f, textInfo.characterInfo[i].baseLine));
			ref Vector3 reference = ref vertices[vertexIndex];
			reference -= val;
			ref Vector3 reference2 = ref vertices[vertexIndex + 1];
			reference2 -= val;
			ref Vector3 reference3 = ref vertices[vertexIndex + 2];
			reference3 -= val;
			ref Vector3 reference4 = ref vertices[vertexIndex + 3];
			reference4 -= val;
			float num2 = (val.x - x) / num;
			float num3 = num2 + 0.0001f;
			float num4 = vertexCurve.Evaluate(num2) * curveScale;
			float num5 = vertexCurve.Evaluate(num3) * curveScale;
			Quaternion val2 = Quaternion.identity;
			if (rotateToTangent)
			{
				((Vector3)(ref val3))._002Ector(1f, 0f, 0f);
				Vector3 val4 = new Vector3(num3 * num + x, num5) - new Vector3(val.x, num4);
				if (((Vector3)(ref val4)).sqrMagnitude > Mathf.Epsilon)
				{
					float num6 = Mathf.Acos(Vector3.Dot(val3, ((Vector3)(ref val4)).normalized)) * 57.29578f;
					float num7 = ((Vector3.Cross(val3, val4).z > 0f) ? num6 : (360f - num6));
					val2 = Quaternion.Euler(0f, 0f, num7);
				}
			}
			Matrix4x4 val5 = Matrix4x4.TRS(new Vector3(0f, num4, 0f), val2, Vector3.one);
			vertices[vertexIndex] = ((Matrix4x4)(ref val5)).MultiplyPoint3x4(vertices[vertexIndex]) + val;
			vertices[vertexIndex + 1] = ((Matrix4x4)(ref val5)).MultiplyPoint3x4(vertices[vertexIndex + 1]) + val;
			vertices[vertexIndex + 2] = ((Matrix4x4)(ref val5)).MultiplyPoint3x4(vertices[vertexIndex + 2]) + val;
			vertices[vertexIndex + 3] = ((Matrix4x4)(ref val5)).MultiplyPoint3x4(vertices[vertexIndex + 3]) + val;
		}
		textComponent.UpdateVertexData();
	}
}
