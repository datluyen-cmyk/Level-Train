namespace UnlockRing.Solver;

public sealed record DecisionLiteral
{
	public string DecisionId { get; }

	public string ChoiceId { get; }

	public string StableKey => DecisionId.Length + ":" + DecisionId + ChoiceId.Length + ":" + ChoiceId;

	public DecisionLiteral(string decisionId, string choiceId)
	{
		Compatibility.ThrowIfNullOrWhiteSpace(decisionId, "decisionId");
		Compatibility.ThrowIfNullOrWhiteSpace(choiceId, "choiceId");
		DecisionId = decisionId;
		ChoiceId = choiceId;
	}
}
