using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StateUISet
{
    public GameStateId state;

    [Header("이 상태에서 켤 Canvas(GameObject)들")]
    public List<GameObject> canvasesToEnable = new();
}

public class GameStateController : MonoBehaviour
{
    [SerializeField] private List<GameObject> allCanvases = new();
    [SerializeField] private List<StateUISet> stateSets = new();
    [SerializeField] private GameStateId initialState = GameStateId.Lobby;

    public GameStateId CurrentState { get; private set; }

    private Dictionary<GameStateId, StateUISet> _map = new();

    private void Awake()
    {
        _map.Clear();
        foreach (var s in stateSets)
        {
            if (s == null) continue;
            if (_map.ContainsKey(s.state))
            {
                Debug.LogWarning($"[GameStateController] Duplicated config: {s.state}");
                continue;
            }
            _map.Add(s.state, s);
        }

        // 초기값 세팅(여기서는 UI 적용 안 함)
        CurrentState = initialState;
    }

    private void Start()
    {
        // 시작할 때는 무조건 한 번 UI 적용
        ApplyUIForCurrentState();
    }

    public void SetState(GameStateId next, bool force = false)
    {
        if (!force && CurrentState == next)
            return;

        CurrentState = next;
        ApplyUIForCurrentState();
    }

    /// "현재 상태" 기준으로 UI 적용 (이게 핵심)
    private void ApplyUIForCurrentState()
    {
        // 1) 전체 OFF
        for (int i = 0; i < allCanvases.Count; i++)
        {
            var c = allCanvases[i];
            if (c != null) c.SetActive(false);
        }

        // 2) 현재 상태에 등록된 UI만 ON
        if (!_map.TryGetValue(CurrentState, out var set))
        {
            Debug.LogWarning($"[GameStateController] No StateUISet for: {CurrentState}");
            return;
        }

        for (int i = 0; i < set.canvasesToEnable.Count; i++)
        {
            var go = set.canvasesToEnable[i];
            if (go != null) go.SetActive(true);
        }

        Debug.Log($"[GameStateController] ApplyUI => {CurrentState}");
    }
}