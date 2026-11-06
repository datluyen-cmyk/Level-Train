namespace Example.Cfg;

public enum LevelMatchType
{
	Fixed = 1,
	Range,
	Multiple,
	NotMultiple,
	AfterMultiple,
	Greater,
	FailOnChange,
	LastFailed,
	WinRate10LevelGreater,
	WinRate10GameGreater,
	Less
}
