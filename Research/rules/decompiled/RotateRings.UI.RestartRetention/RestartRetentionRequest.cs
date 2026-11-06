using RotateRings.Model;
using RotateRings.RestartRetention;
using UnityEngine;
using XingYun.UI.Controller;

namespace RotateRings.UI.RestartRetention;

public sealed class RestartRetentionRequest
{
	public IGameplayManager Manager;

	public GameObject LevelObject;

	public GameMode GameMode;

	public int Level;

	public int Attempt;

	public string SettingsLocation;

	public IController SettingsController;

	public RestartRetentionDecision Decision;

	public bool Preview;

	public bool Registered;

	public bool Handled;

	public bool Closing;
}
