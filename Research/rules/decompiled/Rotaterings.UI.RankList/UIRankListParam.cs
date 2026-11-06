using System;

namespace Rotaterings.UI.RankList;

public class UIRankListParam
{
	public ERankContent Content;

	public Action OnClosed;

	public bool Prewarm;

	public Action<bool> OnPrewarmReady;
}
