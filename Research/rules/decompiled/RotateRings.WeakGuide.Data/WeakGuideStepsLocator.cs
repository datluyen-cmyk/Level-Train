using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;

namespace RotateRings.WeakGuide.Data;

public sealed class WeakGuideStepsLocator
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CEnsureLoadedAsync_003Ed__10 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public int levelId;

		public CancellationToken ct;

		private Awaiter<GuideStepsAsset> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				if (num == 0)
				{
					goto IL_0037;
				}
				string locationForLevel = default(string);
				if (levelId > 0 && !ct.IsCancellationRequested)
				{
					locationForLevel = GetLocationForLevel(levelId);
					goto IL_0037;
				}
				goto end_IL_0007;
				IL_0037:
				GuideStepsAsset result;
				try
				{
					Awaiter<GuideStepsAsset> val;
					if (num != 0)
					{
						val = GameApp.Res.LoadAssetAsync<GuideStepsAsset>(locationForLevel, (string)null).GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<GuideStepsAsset>, _003CEnsureLoadedAsync_003Ed__10>(ref val, ref this);
							return;
						}
					}
					else
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<GuideStepsAsset>);
						num = (_003C_003E1__state = -1);
					}
					result = val.GetResult();
				}
				catch (Exception ex) when (!IsLocationInvalidException(ex))
				{
					goto end_IL_0007;
				}
				if (!((Object)(object)result == (Object)null))
				{
					Default.Register(result);
				}
				end_IL_0007:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	private const string TAG = "[WeakGuide][Locator] ";

	public const string StepsAssetLocationPrefix = "WeakGuideSteps_Level";

	private readonly Dictionary<int, Dictionary<string, GuideStepsAsset.Entry>> byLevel = new Dictionary<int, Dictionary<string, GuideStepsAsset.Entry>>(16);

	private static WeakGuideStepsLocator cached;

	public static WeakGuideStepsLocator Default
	{
		get
		{
			if (cached != null)
			{
				return cached;
			}
			cached = new WeakGuideStepsLocator();
			cached.LoadFromEditor();
			return cached;
		}
	}

	public static void ResetDefault()
	{
		cached = null;
	}

	public GuideStepsAsset.Entry FindEntry(int levelId, string guideIndex = null)
	{
		if (byLevel.TryGetValue(levelId, out var value))
		{
			string key = (string.IsNullOrEmpty(guideIndex) ? "default" : guideIndex);
			if (value.TryGetValue(key, out var value2) && !string.IsNullOrWhiteSpace(value2.StepHandlesJson))
			{
				return value2;
			}
		}
		return null;
	}

	public void Register(GuideStepsAsset asset)
	{
		if ((Object)(object)asset == (Object)null || asset.Entries == null)
		{
			return;
		}
		for (int i = 0; i < asset.Entries.Count; i++)
		{
			GuideStepsAsset.Entry entry = asset.Entries[i];
			if (entry != null)
			{
				string key = (string.IsNullOrEmpty(entry.GuideIndex) ? "default" : entry.GuideIndex);
				if (!byLevel.TryGetValue(entry.LevelId, out var value))
				{
					value = new Dictionary<string, GuideStepsAsset.Entry>();
					byLevel[entry.LevelId] = value;
				}
				value[key] = entry;
			}
		}
	}

	public void Clear()
	{
		byLevel.Clear();
	}

	[AsyncStateMachine(typeof(_003CEnsureLoadedAsync_003Ed__10))]
	public static UniTask EnsureLoadedAsync(int levelId, CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CEnsureLoadedAsync_003Ed__10 _003CEnsureLoadedAsync_003Ed__ = default(_003CEnsureLoadedAsync_003Ed__10);
		_003CEnsureLoadedAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CEnsureLoadedAsync_003Ed__.levelId = levelId;
		_003CEnsureLoadedAsync_003Ed__.ct = ct;
		_003CEnsureLoadedAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CEnsureLoadedAsync_003Ed__._003C_003Et__builder)).Start<_003CEnsureLoadedAsync_003Ed__10>(ref _003CEnsureLoadedAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CEnsureLoadedAsync_003Ed__._003C_003Et__builder)).Task;
	}

	public static string GetLocationForLevel(int levelId)
	{
		return "WeakGuideSteps_Level" + levelId;
	}

	private static bool IsLocationInvalidException(Exception ex)
	{
		if (ex == null)
		{
			return false;
		}
		string message = ex.Message;
		if (string.IsNullOrEmpty(message))
		{
			return false;
		}
		return message.IndexOf("location is invalid", StringComparison.OrdinalIgnoreCase) >= 0;
	}

	private void LoadFromEditor()
	{
	}
}
