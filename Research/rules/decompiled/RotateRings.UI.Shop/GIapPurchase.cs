using System;
using XingYun.Track;

namespace RotateRings.UI.Shop;

[Serializable]
public class GIapPurchase : BaseTrackConfig
{
	public const string EventName = "g_iap_purchase";

	public string sku;

	public string sku_type;

	public string result;

	public string label;

	public double price;

	public string history_paid_user;

	public string game_order_id;

	public string order_id;

	public double item_count;

	public int level;

	public int mode_num;

	public int mode_num_all;

	public int game_mode;

	public int enter_path;

	public int coin_num;

	public int hint_num;

	public int undo_num;

	public int wildcard_num;

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
		sku = null;
		sku_type = null;
		result = null;
		label = null;
		price = 0.0;
		history_paid_user = null;
		game_order_id = null;
		order_id = null;
		item_count = 0.0;
		level = 0;
		mode_num = 0;
		mode_num_all = 0;
		game_mode = 0;
		enter_path = 0;
		coin_num = 0;
		hint_num = 0;
		undo_num = 0;
		wildcard_num = 0;
	}
}
