using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace XingYun.Render;

public sealed class ArrowRibbonDemo : MonoBehaviour
{
	[Tooltip("Arrows to drive. Auto-collected from the scene when empty.")]
	[SerializeField]
	private List<ArrowRibbonController> _arrows = new List<ArrowRibbonController>();

	[Tooltip("Simulate external blocking. While true, clicks do NOT move the arrows.")]
	public bool SimulateBlocked;

	[Tooltip("Seconds after completion before an arrow is re-armed for another run.")]
	[SerializeField]
	private float _respawnDelay = 1f;

	private void Start()
	{
		if (_arrows == null || _arrows.Count == 0)
		{
			_arrows = new List<ArrowRibbonController>();
			_arrows.AddRange(Object.FindObjectsByType<ArrowRibbonController>((FindObjectsSortMode)0));
		}
		for (int i = 0; i < _arrows.Count; i++)
		{
			ArrowRibbonController arrowRibbonController = _arrows[i];
			if (!((Object)(object)arrowRibbonController == (Object)null))
			{
				arrowRibbonController.BlockedPredicate = () => SimulateBlocked;
				((UnityEvent<ArrowRibbonController>)arrowRibbonController.OnCompleted).AddListener((UnityAction<ArrowRibbonController>)HandleCompleted);
			}
		}
	}

	private void Update()
	{
		if (!Input.GetMouseButtonDown(0))
		{
			return;
		}
		for (int i = 0; i < _arrows.Count; i++)
		{
			ArrowRibbonController arrowRibbonController = _arrows[i];
			if ((Object)(object)arrowRibbonController != (Object)null && arrowRibbonController.CurrentState == ArrowRibbonController.State.Idle)
			{
				arrowRibbonController.TryStartSlide();
			}
		}
	}

	private void HandleCompleted(ArrowRibbonController arrow)
	{
		((MonoBehaviour)this).StartCoroutine(Respawn(arrow));
	}

	private IEnumerator Respawn(ArrowRibbonController arrow)
	{
		yield return (object)new WaitForSeconds(_respawnDelay);
		if ((Object)(object)arrow != (Object)null)
		{
			arrow.ResetForReuse();
		}
	}
}
