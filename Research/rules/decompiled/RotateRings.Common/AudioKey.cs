namespace RotateRings.Common;

public class AudioKey
{
	public string Value { get; }

	public AudioKey(string eventId)
	{
		Value = eventId;
	}

	public override string ToString()
	{
		return Value ?? string.Empty;
	}
}
