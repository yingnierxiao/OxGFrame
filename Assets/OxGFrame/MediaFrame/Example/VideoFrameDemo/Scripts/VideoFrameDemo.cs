﻿using OxGFrame.MediaFrame;
using UnityEngine;
using UnityEngine.UI;

public static class Video
{
    // If use prefix "res#" will load from resource else will from bundle
    private const string _prefix = "res#";

    // Paths
    private static readonly string _videoPath = $"{_prefix}Video/";

    public static readonly string VideoCamExample = $"{_videoPath}video_cam_Example";
    public static readonly string VideoRtExample = $"{_videoPath}video_rt_Example";
}

public class VideoFrameDemo : MonoBehaviour
{
    private void Awake()
    {
        // If Init instance can more efficiency
        MediaFrames.VideoFrame.InitInstance();
    }

    public RawImage rawImage = null;

    #region Video cast to 【Camera】
    public async void PlayVideoCamera()
    {
        // if render mode is Camera just play directly
        await MediaFrames.VideoFrame.Play(Video.VideoCamExample);
    }

    public void StopVideoCamera()
    {
        MediaFrames.VideoFrame.Stop(Video.VideoCamExample);
    }

    public void StopVideoWithDestoryCamera()
    {
        MediaFrames.VideoFrame.Stop(Video.VideoCamExample, false, true);
    }

    public void PauseVideoCamera()
    {
        MediaFrames.VideoFrame.Pause(Video.VideoCamExample);
    }
    #endregion

    #region Video cast to 【RenderTexture】
    public async void PlayVideoRenderTexture()
    {
        var video = await MediaFrames.VideoFrame.Play(Video.VideoRtExample);

        // Get Video
        if (video != null)
        {
            // Make sure rawImage is enabled
            this.rawImage.enabled = true;
            // GetTargetRenderTexture and assign to rawImage.texture
            this.rawImage.texture = video.GetTargetRenderTexture();
            // Set EndEvent handler (if video play end can clear rawImage.texture)
            video.SetEndEvent(() =>
            {
                this.rawImage.texture = null;
                this.rawImage.enabled = false;
            });
        }
    }

    public void StopVideoRenderTexture()
    {
        MediaFrames.VideoFrame.Stop(Video.VideoRtExample);
    }

    public void StopVideoWithDestoryRenderTexture()
    {
        /*
         * [if Video is not checked OnDestroyAndUnload, can use ForceUnload to stop and unload]
         * 
         * MediaFrames.VideoFrame.ForceUnload(Video.VideoRtExample);
         */

        MediaFrames.VideoFrame.Stop(Video.VideoRtExample, false, true);
    }

    public void PauseVideoRenderTexture()
    {
        MediaFrames.VideoFrame.Pause(Video.VideoRtExample);
    }
    #endregion

    #region Control All Video
    public void ResumeAll()
    {
        MediaFrames.VideoFrame.ResumeAll();
    }

    public void StopAll()
    {
        MediaFrames.VideoFrame.StopAll();
    }

    public void StopAllWithDestroy()
    {
        MediaFrames.VideoFrame.StopAll(false, true);
    }

    public void PauseAll()
    {
        MediaFrames.VideoFrame.PauseAll();
    }
    #endregion
}
