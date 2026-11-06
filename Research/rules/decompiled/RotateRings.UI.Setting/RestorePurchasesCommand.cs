using Cysharp.Threading.Tasks;
using RotateRings.UI.Shop;
using XingYun.Command;
using XingYun.Core.Singletons;
using XingYun.IAP.SDK;

namespace RotateRings.UI.Setting;

public class RestorePurchasesCommand : BaseCommand
{
	public override void Execute(object param = null)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		RestorePurchasesState.Begin();
		UniTaskVoid val = Singleton<StoreManager>.Instance.RestoreVIPEntitlement((IAPProductType)2);
		((UniTaskVoid)(ref val)).Forget();
	}
}
