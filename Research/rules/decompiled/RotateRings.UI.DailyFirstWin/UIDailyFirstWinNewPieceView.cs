using System;
using Coffee.UIExtensions;
using Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XingYun.UI.Core;

namespace RotateRings.UI.DailyFirstWin;

public class UIDailyFirstWinNewPieceView : GameBaseView
{
	private const string OpenAnimName = "anim_dailyfirstwinnewpiece_open";

	private const string LoopAnimName = "anim_dailyfirstwinnewpiece_loop";

	private const string CloseAnimName = "anim_dailyfirstwinnewpiece_close";

	private const float PerFrameSec = 0.0166f;

	private int openCompleteTimerId = -1;

	private int closeTimerId = -1;

	private Material glowMaskRuntimeMaterial;

	private Material fameGlowLoopRuntimeMaterial;

	private Material sweepRuntimeMaterial;

	private Material frameRuntimeMaterial;

	public Animation anim;

	public Image imagePiece;

	public Button btnContinue;

	public TMP_Text textTitle;

	public Image glowMask;

	public Image fameGlowLoop;

	public UIParticle root;

	public ParticleSystem sweep;

	public ParticleSystem frame;

	public void ApplyPiece(string spriteName)
	{
		NewPieceEffectTexturesUtil.TryApplyPieceSprite(imagePiece, spriteName, setNativeSize: true, activateOnSuccess: true);
	}

	public void ApplyPieceDayTextures(int pieceDay)
	{
		if (NewPieceEffectTexturesUtil.TryLoadConfig(pieceDay, "UIDailyFirstWinNewPieceView", out var config))
		{
			Texture2D frameTexture = config.FrameTexture;
			Texture2D maskTexture = config.MaskTexture;
			if (!((Object)(object)frameTexture == (Object)null))
			{
				_ = (Object)(object)maskTexture == (Object)null;
			}
			if ((Object)(object)maskTexture != (Object)null)
			{
				NewPieceEffectTexturesUtil.ApplyImageEffectTexture(glowMask, maskTexture, ref glowMaskRuntimeMaterial);
				NewPieceEffectTexturesUtil.ApplyParticleMaskTexture(sweep, maskTexture, ref sweepRuntimeMaterial);
				NewPieceEffectTexturesUtil.RestartParticleAfterMaterialChange(sweep);
			}
			if ((Object)(object)frameTexture != (Object)null)
			{
				NewPieceEffectTexturesUtil.ApplyImageEffectTexture(fameGlowLoop, frameTexture, ref fameGlowLoopRuntimeMaterial);
				NewPieceEffectTexturesUtil.ApplyParticleMaskTexture(frame, frameTexture, ref frameRuntimeMaterial);
				NewPieceEffectTexturesUtil.RestartParticleAfterMaterialChange(frame);
			}
			RefreshEffectUIParticleRenderers();
		}
	}

	private void RefreshEffectUIParticleRenderers()
	{
		UIParticle obj = root;
		if (obj != null)
		{
			obj.RefreshParticles();
		}
	}

	public void PrepareOpenFirstFrame()
	{
		DailyFirstWinViewUtil.SampleAnimationFrame(anim, "anim_dailyfirstwinnewpiece_open", 0f);
	}

	public void PlayOpen()
	{
		KillOpenTimer();
		KillCloseTimer();
		AnimationClip val = (((Object)(object)anim != (Object)null) ? anim.GetClip("anim_dailyfirstwinnewpiece_open") : null);
		if (!((Object)(object)anim == (Object)null) && !((Object)(object)val == (Object)null))
		{
			anim.Rewind("anim_dailyfirstwinnewpiece_open");
			anim.Play("anim_dailyfirstwinnewpiece_open", (PlayMode)4);
			float num = val.length + 0.0166f;
			openCompleteTimerId = GameApp.Timer.Delay((Object)(object)((UIBaseView)this).NodeRect, num, (Action)delegate
			{
				openCompleteTimerId = -1;
				DailyFirstWinViewUtil.PlayLoopClip(anim, "anim_dailyfirstwinnewpiece_loop");
			});
		}
	}

	public void PlayClose(Action onComplete)
	{
		KillOpenTimer();
		KillCloseTimer();
		AnimationClip val = (((Object)(object)anim != (Object)null) ? anim.GetClip("anim_dailyfirstwinnewpiece_close") : null);
		if ((Object)(object)anim == (Object)null || (Object)(object)val == (Object)null)
		{
			onComplete?.Invoke();
			return;
		}
		anim.Stop();
		anim.Rewind("anim_dailyfirstwinnewpiece_close");
		anim.Play("anim_dailyfirstwinnewpiece_close", (PlayMode)4);
		float clipLength = val.length;
		closeTimerId = GameApp.Timer.Delay((Object)(object)((UIBaseView)this).NodeRect, clipLength, (Action)delegate
		{
			closeTimerId = -1;
			float time = Mathf.Max(clipLength - 0.0166f, 0f);
			DailyFirstWinViewUtil.SampleAnimationFrame(anim, "anim_dailyfirstwinnewpiece_close", time);
			onComplete?.Invoke();
		});
	}

	public override void Dispose()
	{
		KillOpenTimer();
		KillCloseTimer();
		ReleaseRuntimeMaterials();
		((UIBaseView)this).Dispose();
	}

	private void ReleaseRuntimeMaterials()
	{
		NewPieceEffectTexturesUtil.ReleaseRuntimeMaterial(ref glowMaskRuntimeMaterial, delegate
		{
			NewPieceEffectTexturesUtil.ClearImageEffectMaterial(glowMask);
		});
		NewPieceEffectTexturesUtil.ReleaseRuntimeMaterial(ref fameGlowLoopRuntimeMaterial, delegate
		{
			NewPieceEffectTexturesUtil.ClearImageEffectMaterial(fameGlowLoop);
		});
		NewPieceEffectTexturesUtil.ReleaseRuntimeMaterial(ref sweepRuntimeMaterial, delegate
		{
			NewPieceEffectTexturesUtil.ClearParticleMaterial(sweep);
		});
		NewPieceEffectTexturesUtil.ReleaseRuntimeMaterial(ref frameRuntimeMaterial, delegate
		{
			NewPieceEffectTexturesUtil.ClearParticleMaterial(frame);
		});
	}

	private void KillOpenTimer()
	{
		if (openCompleteTimerId >= 0)
		{
			GameApp.Timer.Kill(openCompleteTimerId);
			openCompleteTimerId = -1;
		}
	}

	private void KillCloseTimer()
	{
		if (closeTimerId >= 0)
		{
			GameApp.Timer.Kill(closeTimerId);
			closeTimerId = -1;
		}
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		anim = behaviour.Get<Animation>("anim");
		imagePiece = behaviour.Get<Image>("imagePiece");
		btnContinue = behaviour.Get<Button>("btnContinue");
		textTitle = behaviour.Get<TMP_Text>("textTitle");
		glowMask = behaviour.Get<Image>("glowMask");
		fameGlowLoop = behaviour.Get<Image>("fameGlowLoop");
		root = behaviour.Get<UIParticle>("root");
		sweep = behaviour.Get<ParticleSystem>("sweep");
		frame = behaviour.Get<ParticleSystem>("frame");
	}
}
