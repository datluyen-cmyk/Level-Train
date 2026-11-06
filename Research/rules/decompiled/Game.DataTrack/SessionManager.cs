using System;
using UnityEngine;

namespace Game.DataTrack;

public sealed class SessionManager
{
	private static SessionManager instance;

	public const float SessionDurationSeconds = 1800f;

	private const string Tag = "[SessionManager] ";

	private float sessionStartTime;

	private bool isInitialized;

	private int sessionTimerId = -1;

	private int currentSessionIndex;

	public static SessionManager Instance => instance ?? (instance = new SessionManager());

	public int CurrentSessionIndex
	{
		get
		{
			Initialize();
			return currentSessionIndex;
		}
	}

	public bool IsFirstSession => CurrentSessionIndex == 1;

	private SessionManager()
	{
	}

	public void Initialize()
	{
		if (!isInitialized)
		{
			sessionStartTime = Time.realtimeSinceStartup;
			currentSessionIndex = 1;
			isInitialized = true;
			sessionTimerId = GameApp.Timer.Repeat(1800f, (Action)StartNextSession);
		}
	}

	public float GetElapsedSeconds()
	{
		Initialize();
		return Time.realtimeSinceStartup - sessionStartTime;
	}

	public void Stop()
	{
		if (sessionTimerId >= 0)
		{
			GameApp.Timer.Kill(sessionTimerId);
			sessionTimerId = -1;
		}
		isInitialized = false;
		currentSessionIndex = 0;
	}

	private void StartNextSession()
	{
		sessionStartTime = Time.realtimeSinceStartup;
		currentSessionIndex++;
	}
}
