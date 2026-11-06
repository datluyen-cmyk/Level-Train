using SuperScrollView;

namespace Rotaterings.UI.RankList;

public interface IRankSettleAnimatorContext
{
	UIRankListView View { get; }

	RankListDataBuilder DataBuilder { get; }

	int SettleTotalCount { get; set; }

	int GetSelfDataIndex();

	int GetSettleLoopItemCount();

	int SettleDataIndexToLoopIndex(int dataIndex);

	RankItem GetSettleRankItem(LoopListViewItem2 listItem, int loopIndex, RankItemData entry);

	void RefreshTopRankItems();

	void OnSettleAnimationCompleted();
}
