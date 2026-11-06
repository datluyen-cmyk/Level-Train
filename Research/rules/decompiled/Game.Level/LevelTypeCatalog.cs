namespace Game.Level;

public static class LevelTypeCatalog
{
	public const int ELEMENT_BOMB = 1;

	public const int ELEMENT_CHAIN = 2;

	public const int ELEMENT_KEY_LOCK = 3;

	public const int PART_BOMB = 1;

	public const int PART_CAN_DIE = 2;

	public const int PART_CHAIN = 4;

	public const int PART_KEY_LOCK = 8;

	public const string TARGET_LEVEL_TYPE = "LevelType";

	public const string TARGET_LEVEL_ID = "LevelId";

	public const string TARGET_DA_WIN_LOSE = "DAWinLose";

	public const string TARGET_PARTS = "Parts";

	public const string SUFFIX_FIXED = "_Fixed";

	public const string SUFFIX_OFFSET = "_Offset";

	public const string KEY_PARTS_MUST_NOT_HAVE = "Parts_MustNotHave";

	public const string KEY_PARTS_MUST_NOT_HAVE_FIXED = "Parts_MustNotHave_Fixed";
}
