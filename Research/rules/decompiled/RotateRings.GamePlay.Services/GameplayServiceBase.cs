using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniLog;
using UnityEngine;

namespace RotateRings.GamePlay.Services;

public abstract class GameplayServiceBase<THost> : IGameplayService, IDisposable where THost : class, IGameplayServiceHost
{
	protected THost Host { get; }

	protected bool IsDisposed { get; private set; }

	protected CancellationToken DestroyToken
	{
		get
		{
			THost host = Host;
			THost obj = ((host is Component) ? host : null);
			if (obj == null)
			{
				return default(CancellationToken);
			}
			return UniTaskCancellationExtensions.GetCancellationTokenOnDestroy((Component)(object)obj);
		}
	}

	protected GameplayServiceBase(THost host)
	{
		Host = host ?? throw new ArgumentNullException("host");
		host.RegisterGameService(this);
	}

	public virtual void ClearLevel()
	{
	}

	public virtual void Dispose()
	{
		if (!IsDisposed)
		{
			IsDisposed = true;
			Host.UnregisterGameService(this);
		}
	}

	protected void ThrowIfDisposed()
	{
		if (IsDisposed)
		{
			Log.LogError("[GamePlay][Service]", "服务已释放仍被访问: {0}", new object[1] { GetType().Name });
		}
	}
}
