using UnlockRing.Core.Runtime;
using UnlockRing.Serialization;

namespace UnlockRing.IntegrationSample;

public sealed record LoadedPlayableLevel(GameSession Session, PlayableLevelPackLevel PlayableLevelPackLevel);
