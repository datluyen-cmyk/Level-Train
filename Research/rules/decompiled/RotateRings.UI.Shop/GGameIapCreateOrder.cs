using System;
using XingYun.Track;

namespace RotateRings.UI.Shop;

[Serializable]
public class GGameIapCreateOrder : BaseTrackConfig
{
	public const string EventName = "g_game_iap_create_order";

	public string sku;

	public string sku_type;

	public string label;

	public double price;

	public string currency_code;

	public string history_paid_user;

	public string game_order_id;

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
		label = null;
		price = 0.0;
		currency_code = null;
		history_paid_user = null;
		game_order_id = null;
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
