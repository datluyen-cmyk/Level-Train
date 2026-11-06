using Script.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WordSolitaire.UI.Shop;

public class BuyItemView : MonoBehaviour
{
	public Image imgItem;

	public Image imgBgItem;

	public TMP_Text txtItemNumber;

	public int PropId { get; private set; }

	public void Initialize(int propId, int count, string spriteName)
	{
		PropId = propId;
		if ((Object)(object)txtItemNumber != (Object)null)
		{
			txtItemNumber.text = $"{count}";
		}
		if ((Object)(object)imgItem != (Object)null && !string.IsNullOrEmpty(spriteName))
		{
			imgItem.SetAtlasSprite("Common", spriteName);
		}
	}
}
