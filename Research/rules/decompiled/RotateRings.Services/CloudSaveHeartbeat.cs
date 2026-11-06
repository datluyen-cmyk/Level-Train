using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using XingYun.Account;

namespace RotateRings.Services;

public sealed class CloudSaveHeartbeat : MonoBehaviour
{
	private const string Tag = "[CloudSaveHeartbeat]";

	private const string HealthPath = "/health";

	private const int HeartbeatIntervalSeconds = 20;

	private const int HeartbeatTimeoutSeconds = 3;

	private AccountComponent account;

	private string healthUrl;

	private Coroutine heartbeatCoroutine;

	private WaitForSecondsRealtime heartbeatInterval;

	public void Initialize(AccountComponent accountComponent)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		account = accountComponent;
		healthUrl = account.BaseUrl.TrimEnd('/') + "/health";
		if (heartbeatCoroutine == null)
		{
			heartbeatInterval = new WaitForSecondsRealtime(20f);
			heartbeatCoroutine = ((MonoBehaviour)this).StartCoroutine(SendHeartbeatRoutine());
		}
	}

	private IEnumerator SendHeartbeatRoutine()
	{
		while (true)
		{
			if (!CanSendHeartbeat())
			{
				yield return heartbeatInterval;
				continue;
			}
			UnityWebRequest request = UnityWebRequest.Get(healthUrl);
			try
			{
				request.timeout = 3;
				request.SetRequestHeader("Authorization", "Bearer " + account.SessionToken);
				yield return request.SendWebRequest();
				Result result = request.result;
				if ((int)result == 2)
				{
				}
			}
			finally
			{
				((IDisposable)request)?.Dispose();
			}
			yield return heartbeatInterval;
		}
	}

	private bool CanSendHeartbeat()
	{
		if ((Object)(object)account != (Object)null && account.IsInitialized)
		{
			return account.IsLoggedIn;
		}
		return false;
	}
}
