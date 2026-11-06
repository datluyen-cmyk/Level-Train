using UnityEngine;

namespace Basis.Tool;

[ExecuteAlways]
[RequireComponent(typeof(Animation))]
public class AnimationPlayerTool : MonoBehaviour
{
	[Tooltip("要播放的动画片段名称（需在 Animation 组件中已注册）")]
	[SerializeField]
	private string clipName = "";

	[Tooltip("播放模式：StopAll 停止其他动画，StopAll 同理；具体见 PlayMode 枚举")]
	[SerializeField]
	private PlayMode playMode = (PlayMode)4;

	[Tooltip("Enable 时自动播放一次")]
	[SerializeField]
	private bool playOnEnable;

	private Animation _animation;

	private void Awake()
	{
		_animation = ((Component)this).GetComponent<Animation>();
	}

	private void OnEnable()
	{
		if (playOnEnable)
		{
			Play();
		}
	}

	[ContextMenu("播放动画")]
	public void Play()
	{
		Play(clipName);
	}

	[ContextMenu("停止动画")]
	public void Stop()
	{
		if ((Object)(object)_animation == (Object)null)
		{
			_animation = ((Component)this).GetComponent<Animation>();
		}
		Animation animation = _animation;
		if (animation != null)
		{
			animation.Stop();
		}
	}

	public void Play(string clip)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if (!string.IsNullOrEmpty(clip))
		{
			if ((Object)(object)_animation == (Object)null)
			{
				_animation = ((Component)this).GetComponent<Animation>();
			}
			if (!((Object)(object)_animation == (Object)null) && !((Object)(object)_animation.GetClip(clip) == (Object)null))
			{
				_animation.Rewind(clip);
				_animation.Play(clip, playMode);
			}
		}
	}
}
