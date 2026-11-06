namespace Game.Feature.Gameplay;

public interface IHookOnSettle
{
	int SettleOrder { get; }

	void Settle();
}
