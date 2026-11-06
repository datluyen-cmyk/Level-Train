using RotateRings.Prop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Extensions;

namespace RotateRings.UI.Common;

public class ShowCoin : MonoBehaviour
{
	public Image imgAddIcon;

	public TMP_Text txtCoinNum;

	public Button btnAddCoin;

	public Image imgCoin;

	public GameObject effFlashing;

	public void OnRender(EShowCoinFrom from, int initShowCoinCount = -1)
	{
		if (!((Component)this).gameObject.activeSelf)
		{
			((Component)this).gameObject.SetActive(true);
		}
		if ((Object)(object)imgAddIcon != (Object)null)
		{
			((Component)imgAddIcon).gameObject.SetActive(false);
		}
		if ((Object)(object)btnAddCoin != (Object)null)
		{
			((Behaviour)btnAddCoin).enabled = false;
		}
		int coinNum = ((initShowCoinCount == -1) ? GameApp.Proxy.Get<PropModelProxy>().GetPropCount(EPropType.Coin) : initShowCoinCount);
		SetCoinNum(coinNum);
	}

	public void SetCoinNum(int coinNum)
	{
		if ((Object)(object)txtCoinNum != (Object)null)
		{
			TMPTextExtensions.SetLocalizedTextValue(txtCoinNum, coinNum.ToString());
		}
	}

	public void OnRelease()
	{
	}
}
