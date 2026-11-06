using UnityEngine;

[RequireComponent(typeof(Camera))]
public class GameplayCameraSettings : MonoBehaviour
{
	[SerializeField]
	private LayerMask inputLayerMask = LayerMask.op_Implicit(192);

	[SerializeField]
	private Camera cameraApply;

	public LayerMask InputLayerMask => inputLayerMask;

	private void Awake()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)cameraApply == (Object)null)
		{
			cameraApply = ((Component)this).GetComponent<Camera>();
		}
		if (((LayerMask)(ref inputLayerMask)).value == 0 || ((LayerMask)(ref inputLayerMask)).value == 1)
		{
			inputLayerMask = GameplayInputSettings.DefaultRingInputLayerMask;
		}
		GameplayInputSettings.InputLayerMask = inputLayerMask;
		if ((Object)(object)cameraApply != (Object)null)
		{
			GameplayCameraManager.GameplayCamera = cameraApply;
		}
	}
}
