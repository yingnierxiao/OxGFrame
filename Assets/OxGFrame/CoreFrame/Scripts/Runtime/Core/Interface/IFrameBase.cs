﻿using Cysharp.Threading.Tasks;

namespace OxGFrame.CoreFrame
{
    public interface IFrameBase
    {
        void OnInit();

        void InitFirst();

        UniTask PreInit();

        void Display(object obj);

        void Hide(bool disablePreClose);

        void OnRelease();

        void SetNames(string assetName);

        void SetGroupId(int id);

        void SetHidden(bool isHidden);
    }
}