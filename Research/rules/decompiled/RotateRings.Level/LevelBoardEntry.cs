using System.IO;
using System.Text;
using XingYun.Level;

namespace RotateRings.Level;

public class LevelBoardEntry : BaseLevelExtensionEntry
{
	public int Index;

	public string Content;

	public override int Deserialize(BinaryReader indexReader, byte[] dataTable)
	{
		Index = indexReader.ReadInt32();
		int index = indexReader.ReadInt32();
		int count = indexReader.ReadInt32();
		Content = Encoding.UTF8.GetString(dataTable, index, count);
		return Index;
	}
}
