namespace Game.Common.Services;

public interface IActivityStep
{
	int Priority { get; }

	bool ShouldRun();

	void Execute();
}
