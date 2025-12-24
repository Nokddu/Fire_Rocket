using System;
using UnityEngine;

[Serializable]
public class GameStateConfig
{
    public GameStateId state;

    public bool instantBgm = true;
    public bool applyCursor = true;

    public float timeScale = 1f;

    // Playing 진입 시 동작 옵션들
    public bool resetReloading = true;
    public bool callSpawnSetting = false;
    public bool startStage = false;
    public bool setMissionAssassination = false;

    // PausePopup 내 로비 버튼 표시 여부(원하는 정책대로)
    public bool showLobbyButtonInPausePopup = false;
}
