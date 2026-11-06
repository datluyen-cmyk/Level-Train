using UnityEngine;

public class LineHelper : SingletonComponent<LineHelper>
{
	[SerializeField]
	protected LineRenderer lineRenderer;

	[SerializeField]
	protected float startWidth;

	[SerializeField]
	protected float endWidth;

	[SerializeField]
	protected float lineColorAlpha;

	private void Start()
	{
		if (!((Object)(object)lineRenderer == (Object)null))
		{
			lineRenderer.startWidth = startWidth;
			lineRenderer.endWidth = endWidth;
			ClearLine();
		}
	}

	public void DrawLine(Vector3 start, Vector3 end, Color color)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)lineRenderer == (Object)null))
		{
			lineRenderer.positionCount = 2;
			Color val = default(Color);
			((Color)(ref val))._002Ector(color.r, color.g, color.b, lineColorAlpha);
			lineRenderer.startColor = val;
			lineRenderer.endColor = val;
			lineRenderer.SetPosition(0, start);
			lineRenderer.SetPosition(1, end);
		}
	}

	public void ClearLine()
	{
		if (!((Object)(object)lineRenderer == (Object)null))
		{
			lineRenderer.positionCount = 0;
		}
	}
}
