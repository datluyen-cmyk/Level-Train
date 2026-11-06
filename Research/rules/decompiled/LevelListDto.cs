using System;

[Serializable]
public sealed class LevelListDto
{
	public string source_bank_sha256;

	public int level_count;

	public LevelDto[] levels;
}
