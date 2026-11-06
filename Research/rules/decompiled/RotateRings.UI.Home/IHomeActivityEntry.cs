using System;
using Game.UI;
using UnityEngine;

namespace RotateRings.UI.Home;

public interface IHomeActivityEntry
{
	int Order { get; }

	HomeActivityEntrySlot Slot { get; }

	void Mount(GameBaseView host, RectTransform left, RectTransform right);

	void Unmount();

	void Bind();

	void Unbind();

	void Refresh();

	bool TryPlayIconSpineAnimation(Action onComplete);

	void StopIconSpineAnimation();
}
