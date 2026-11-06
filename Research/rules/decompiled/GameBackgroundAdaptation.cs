using System;
using UnityEngine;
using UnlockRing;

public sealed class GameBackgroundAdaptation : IDisposable
{
	private readonly Transform background;

	private readonly Camera camera;

	private Vector2 lastCameraXY;

	private bool hasApplied;

	private bool disposed;

	public GameBackgroundAdaptation(Transform background, Camera camera)
	{
		this.background = background;
		this.camera = camera;
		GameApp.Event.Listen<GameCameraAdaptationEndEvent>((Action<GameCameraAdaptationEndEvent>)OnCameraFramingApplied);
	}

	public void Dispose()
	{
		if (!disposed)
		{
			disposed = true;
			GameApp.Event.Unlisten<GameCameraAdaptationEndEvent>((Action<GameCameraAdaptationEndEvent>)OnCameraFramingApplied);
		}
	}

	private void Refresh()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)background == (Object)null) && !((Object)(object)camera == (Object)null))
		{
			Vector3 position = ((Component)camera).transform.position;
			background.position = new Vector3(position.x, position.y, background.position.z);
			lastCameraXY = new Vector2(position.x, position.y);
			hasApplied = true;
		}
	}

	private void TrySync()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)background == (Object)null) && !((Object)(object)camera == (Object)null))
		{
			Vector3 position = ((Component)camera).transform.position;
			Vector2 val = default(Vector2);
			((Vector2)(ref val))._002Ector(position.x, position.y);
			if (!hasApplied || val != lastCameraXY)
			{
				Refresh();
			}
		}
	}

	private void OnCameraFramingApplied(GameCameraAdaptationEndEvent evt)
	{
		if (!disposed)
		{
			TrySync();
		}
	}
}
