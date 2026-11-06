using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IGameplayManager
{
	GameplayMode Mode { get; }

	bool IsLevelCompleted { get; }

	bool HasUnsolvedGameplayObjects { get; }

	GameObject CurrentLevelObject { get; }

	bool IsLoadingLevel { get; }

	UniTaskVoid RunLevelEnterAsync(int level);

	bool TryPrepareForGmVictory();

	void GoToNextLevel();

	void ReplayCurrentLevel();

	void ExitToHome();
}
