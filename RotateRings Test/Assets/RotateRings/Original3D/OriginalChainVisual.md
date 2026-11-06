# Original chain visual

The chain helper reads the original Spine 4.2.43 binary skeleton, atlas page, and atlas text from `Resources/Original/ChainVisual`. It creates the four source orientations and plays the native idle, shake, break, and available appear clips through `SkeletonAnimation`.

The source skeleton includes `rr_suolian_shu_appear`, but no appear clip for mid, low, or high. `PlayChainAppear` reports `false` for those directions and leaves them in their source idle pose. The original break and shake calls are one-shot animations; break segments are recycled after the source clip duration.

The project uses the official Spine Unity 4.2.121 and Spine C# 4.2.0 packages from commit `5cb78e77d3711ddcd80c7bd043bb11cf70fcc88`. Both dependencies are pinned in `Packages/manifest.json` and `Packages/packages-lock.json`. The Spine Runtimes license notice is included at `Assets/ThirdPartyNotices/Spine-Runtimes-LICENSE.txt`.

`OriginalChainVisualAssets.CreateOrUpdateChainVisual` retains the owner transform, ring geometry, lock count, counter-rotation, board flip compensation, source scale, and 15 degree edge inset. Use `PlayChainAppear`, `PlayChainShake`, and `PlayChainBreak` on the segment/root when the matching source events occur.
