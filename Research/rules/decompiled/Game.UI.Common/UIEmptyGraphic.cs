using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Common;

[RequireComponent(typeof(CanvasRenderer))]
public class UIEmptyGraphic : Graphic
{
	private static readonly Color32 TransparentColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte)0);

	protected override void OnEnable()
	{
		((Graphic)this).OnEnable();
		((Graphic)this).canvasRenderer.cullTransparentMesh = false;
	}

	public override bool Raycast(Vector2 sp, Camera eventCamera)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (!((Behaviour)this).isActiveAndEnabled || !((Graphic)this).raycastTarget)
		{
			return false;
		}
		return RectTransformUtility.RectangleContainsScreenPoint(((Graphic)this).rectTransform, sp, eventCamera, ((Graphic)this).raycastPadding);
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		vh.Clear();
		Rect pixelAdjustedRect = ((Graphic)this).GetPixelAdjustedRect();
		UIVertex simpleVert = UIVertex.simpleVert;
		simpleVert.color = TransparentColor;
		simpleVert.position = new Vector3(((Rect)(ref pixelAdjustedRect)).xMin, ((Rect)(ref pixelAdjustedRect)).yMin);
		vh.AddVert(simpleVert);
		simpleVert.position = new Vector3(((Rect)(ref pixelAdjustedRect)).xMin, ((Rect)(ref pixelAdjustedRect)).yMax);
		vh.AddVert(simpleVert);
		simpleVert.position = new Vector3(((Rect)(ref pixelAdjustedRect)).xMax, ((Rect)(ref pixelAdjustedRect)).yMax);
		vh.AddVert(simpleVert);
		simpleVert.position = new Vector3(((Rect)(ref pixelAdjustedRect)).xMax, ((Rect)(ref pixelAdjustedRect)).yMin);
		vh.AddVert(simpleVert);
		vh.AddTriangle(0, 1, 2);
		vh.AddTriangle(2, 3, 0);
	}
}
