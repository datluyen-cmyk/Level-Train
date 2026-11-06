using UnityEngine;

public class DraggableObject : MonoBehaviour
{
	public enum DragMode
	{
		Instance,
		Velocity,
		Blocked
	}

	public bool holdOnCenterPos;

	public DragMode dragMode;

	public float dragMagnitude;

	public float moveThreshold;

	public float yOffset;

	protected bool beingHeld;

	protected Rigidbody2D rb;

	protected Vector3 holdPos;

	protected Camera mainCamera;

	public bool BeingHeld => beingHeld;

	public virtual void HandlePointerDown(GameplayPointerData data)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		beingHeld = true;
		if (holdOnCenterPos)
		{
			holdPos = Vector3.zero;
		}
		else if (!((Object)(object)mainCamera == (Object)null))
		{
			holdPos = data.WorldPosition - ((Component)this).transform.position;
		}
	}

	public virtual void HandlePointerDrag(GameplayPointerData data)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (beingHeld && !((Object)(object)mainCamera == (Object)null))
		{
			ApplyDragAtWorldPosition(data.WorldPosition);
		}
	}

	public virtual void HandlePointerUp(GameplayPointerData data)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		beingHeld = false;
		if (dragMode == DragMode.Velocity && (Object)(object)rb != (Object)null)
		{
			rb.linearVelocity = Vector2.zero;
		}
	}

	protected virtual void Start()
	{
		rb = ((Component)this).GetComponent<Rigidbody2D>();
		mainCamera = GameplayCameraManager.GameplayCamera;
	}

	protected void ApplyDragAtWorldPosition(Vector3 worldPosition)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		if (dragMode == DragMode.Velocity)
		{
			Vector3 position = ((Component)this).transform.position;
			float num = worldPosition.x - holdPos.x;
			float num2 = worldPosition.y - holdPos.y;
			float num3 = num - position.x;
			float num4 = num2 - position.y;
			float num5 = Mathf.Sqrt(num3 * num3 + num4 * num4);
			if (!((Object)(object)rb == (Object)null))
			{
				if (num5 >= moveThreshold)
				{
					rb.linearVelocity = new Vector2(num3 * dragMagnitude, num4 * dragMagnitude);
				}
				else
				{
					rb.linearVelocity = Vector2.zero;
				}
			}
		}
		else if (dragMode == DragMode.Instance)
		{
			Vector3 val = worldPosition - holdPos + new Vector3(0f, yOffset, 0f);
			((Component)this).transform.position = new Vector3(val.x, val.y, 0f);
		}
	}
}
