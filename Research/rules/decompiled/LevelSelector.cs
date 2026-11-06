using System;
using System.Diagnostics;
using UniLog;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnlockRing;
using UnlockRing.Core.Domain;
using UnlockRing.IntegrationSample;
using UnlockRing.Serialization;

public class LevelSelector : MonoBehaviour
{
	private const string TAG = "[UnlockRing][LevelSelector] ";

	[Header("References")]
	[Tooltip("关卡渲染器引用。不挂载在同一 GameObject 上也可。")]
	public CoreBoardRenderer boardRenderer;

	[Header("Data")]
	[Tooltip("PlayableLevelPack 二进制资源。")]
	public TextAsset ta;

	[Header("Navigation")]
	public Button btnNextLevel;

	public Button btnPrevLevel;

	[Tooltip("启用 A=上一关 D=下一关 S=重置 键盘导航")]
	public bool keyboardNavigation = true;

	private PlayableLevelPackUnityLoader _packLoader;

	private string[] _levelIds = Array.Empty<string>();

	private int _currentIndex;

	public int CurrentIndex
	{
		get
		{
			return _currentIndex;
		}
		set
		{
			if (_levelIds.Length != 0)
			{
				int num = (value % _levelIds.Length + _levelIds.Length) % _levelIds.Length;
				if (num != _currentIndex || boardRenderer.Controller == null)
				{
					LoadIndex(num);
				}
			}
		}
	}

	public int LevelCount => _levelIds.Length;

	private CoreBoardRenderer _renderer => boardRenderer;

	private void Start()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected O, but got Unknown
		if ((Object)(object)boardRenderer == (Object)null)
		{
			Log.LogError("[UnlockRing][LevelSelector] ", "boardRenderer not assigned.");
			return;
		}
		if ((Object)(object)btnNextLevel != (Object)null)
		{
			((UnityEvent)btnNextLevel.onClick).AddListener(new UnityAction(NextLevel));
		}
		if ((Object)(object)btnPrevLevel != (Object)null)
		{
			((UnityEvent)btnPrevLevel.onClick).AddListener(new UnityAction(PrevLevel));
		}
		LoadAllLevels();
	}

	private void Update()
	{
		if (keyboardNavigation)
		{
			if (Input.GetKeyDown((KeyCode)97))
			{
				PrevLevel();
			}
			if (Input.GetKeyDown((KeyCode)100))
			{
				NextLevel();
			}
			if (Input.GetKeyDown((KeyCode)115))
			{
				ResetLevel();
			}
		}
	}

	public void LoadAllLevels()
	{
		if ((Object)(object)ta == (Object)null)
		{
			Log.LogError("[UnlockRing][LevelSelector] ", "TextAsset (ta) not assigned.");
			_renderer.SetStatus("No level pack assigned.");
		}
		else
		{
			_currentIndex = 0;
			LoadIndex(_currentIndex);
		}
	}

	public void LoadIndex(int index)
	{
		if (index < 0)
		{
			return;
		}
		Stopwatch stopwatch = Stopwatch.StartNew();
		_currentIndex = index;
		try
		{
			UnifiedLevelPackReadResult levelPackReadResult = UnifiedLevelPackReader.Read(ta.text);
			LevelDefinition levelDefinition = RenderModeComparator.ReadLevelDefinition(levelPackReadResult);
			_renderer.LoadLevel(levelDefinition, levelPackReadResult);
			stopwatch.Stop();
			_renderer.SetStatus($"Level {index + 1}/{_levelIds.Length} — {index}");
		}
		catch (Exception ex)
		{
			stopwatch.Stop();
			Log.LogError("[UnlockRing][LevelSelector] ", "Failed to load level '{0}': {1}", new object[2] { index, ex.Message });
			_renderer.SetStatus($"Failed to load '{index}'.");
		}
	}

	public void NextLevel()
	{
		if (_levelIds.Length != 0)
		{
			LoadIndex((_currentIndex + 1) % _levelIds.Length);
		}
	}

	public void PrevLevel()
	{
		if (_levelIds.Length != 0)
		{
			LoadIndex((_currentIndex - 1 + _levelIds.Length) % _levelIds.Length);
		}
	}

	public void ResetLevel()
	{
		if (_levelIds.Length != 0)
		{
			LoadIndex(_currentIndex);
		}
	}
}
