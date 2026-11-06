using RotateRings.Common;
using UnityEngine;
using XingYun.GM;

namespace RotateRings.IAP;

public static class IAPDebugSetting
{
	private const string Tag = "IAPDebugSetting";

	private const string KeyUseTestSku = "RR_IAPDebug_UseTestSku";

	private const string KeyConsumableShopItemId = "RR_IAPDebug_ConsumableShopItemId";

	private const string KeyNonConsumableShopItemId = "RR_IAPDebug_NonConsumableShopItemId";

	private const string KeyReorderShopItemId = "RR_IAPDebug_ReorderShopItemId";

	private const string KeyMockBuyMode = "RR_IAPDebug_MockBuyMode";

	private const string KeyShopToggle = "RR_IAPDebug_ShopToggle";

	private const string KeyShopShowEntry = "RR_GM_ShopFunction_ShowEntry";

	private const string LegacyTestStoreIdPrefKey = "ShopUseTestStoreId";

	public const int DefaultConsumableShopItemId = 1001;

	public const int DefaultNonConsumableShopItemId = 1002;

	public const int DefaultReorderShopItemId = 2005;

	public static bool IsEditorActive => false;

	public static bool UseTestSku
	{
		get
		{
			if (!IsEditorActive)
			{
				return false;
			}
			if (GMConfig.HasKey("RR_IAPDebug_UseTestSku"))
			{
				return GMConfig.GetInt("RR_IAPDebug_UseTestSku", 0) == 1;
			}
			return PlayerPrefs.GetInt("ShopUseTestStoreId", 0) == 1;
		}
		set
		{
			if (IsEditorActive)
			{
				GMConfig.SetInt("RR_IAPDebug_UseTestSku", value ? 1 : 0);
			}
		}
	}

	public static int ConsumableShopItemId
	{
		get
		{
			return GetPositiveInt("RR_IAPDebug_ConsumableShopItemId", 1001);
		}
		set
		{
			SetPositiveInt("RR_IAPDebug_ConsumableShopItemId", value, 1001);
		}
	}

	public static int NonConsumableShopItemId
	{
		get
		{
			return GetPositiveInt("RR_IAPDebug_NonConsumableShopItemId", 1002);
		}
		set
		{
			SetPositiveInt("RR_IAPDebug_NonConsumableShopItemId", value, 1002);
		}
	}

	public static int ReorderShopItemId
	{
		get
		{
			return GetPositiveInt("RR_IAPDebug_ReorderShopItemId", 2005);
		}
		set
		{
			SetPositiveInt("RR_IAPDebug_ReorderShopItemId", value, 2005);
		}
	}

	public static EIapMockBuyMode MockBuyMode
	{
		get
		{
			if (!IsEditorActive)
			{
				return EIapMockBuyMode.Off;
			}
			int num = GMConfig.GetInt("RR_IAPDebug_MockBuyMode", 0);
			if (num < 0 || num > 2)
			{
				return EIapMockBuyMode.Off;
			}
			return (EIapMockBuyMode)num;
		}
		set
		{
			if (IsEditorActive)
			{
				GMConfig.SetInt("RR_IAPDebug_MockBuyMode", (int)value);
			}
		}
	}

	public static int ShopShowEntry
	{
		get
		{
			if (IsEditorActive && GMConfig.HasKey("RR_GM_ShopFunction_ShowEntry"))
			{
				return Mathf.Clamp(GMConfig.GetInt("RR_GM_ShopFunction_ShowEntry", 0), 0, 4);
			}
			return FeatureConfig.ShopFunction?.ShowEntry ?? 0;
		}
		set
		{
			if (IsEditorActive)
			{
				int num = Mathf.Clamp(value, 0, 4);
				GMConfig.SetInt("RR_GM_ShopFunction_ShowEntry", num);
				if (FeatureConfig.ShopFunction != null)
				{
					FeatureConfig.ShopFunction.ShowEntry = num;
				}
			}
		}
	}

	public static int ShopToggle
	{
		get
		{
			if (IsEditorActive && GMConfig.HasKey("RR_IAPDebug_ShopToggle"))
			{
				return Mathf.Clamp(GMConfig.GetInt("RR_IAPDebug_ShopToggle", 0), 0, 1);
			}
			return FeatureConfig.ShopFunction?.ShopToggle ?? 0;
		}
		set
		{
			if (IsEditorActive)
			{
				int num = Mathf.Clamp(value, 0, 1);
				GMConfig.SetInt("RR_IAPDebug_ShopToggle", num);
				if (FeatureConfig.ShopFunction != null)
				{
					FeatureConfig.ShopFunction.ShopToggle = num;
				}
			}
		}
	}

	public static void ResetAll()
	{
		if (IsEditorActive)
		{
			GMConfig.DeleteKey("RR_IAPDebug_UseTestSku");
			GMConfig.DeleteKey("RR_IAPDebug_ConsumableShopItemId");
			GMConfig.DeleteKey("RR_IAPDebug_NonConsumableShopItemId");
			GMConfig.DeleteKey("RR_IAPDebug_ReorderShopItemId");
			GMConfig.DeleteKey("RR_IAPDebug_MockBuyMode");
			GMConfig.DeleteKey("RR_IAPDebug_ShopToggle");
			GMConfig.DeleteKey("RR_GM_ShopFunction_ShowEntry");
			PlayerPrefs.DeleteKey("ShopUseTestStoreId");
			PlayerPrefs.Save();
		}
	}

	private static int GetPositiveInt(string key, int defaultValue)
	{
		if (!IsEditorActive)
		{
			return defaultValue;
		}
		int num = GMConfig.GetInt(key, defaultValue);
		if (num <= 0)
		{
			return defaultValue;
		}
		return num;
	}

	private static void SetPositiveInt(string key, int value, int defaultValue)
	{
		if (IsEditorActive)
		{
			GMConfig.SetInt(key, (value > 0) ? value : defaultValue);
		}
	}
}
