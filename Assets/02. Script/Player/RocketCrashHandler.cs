using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class RocketCrashHandler : MonoBehaviour
{
    [Header("Detect")]
    [SerializeField] private string obstacleTag = "Obstacle";

    [Header("FX")]
    [SerializeField] private ParticleSystem crashFxPrefab;
    [SerializeField] private Transform fxParent;              // 비워두면 월드에 생성
    [SerializeField] private bool stopChildParticles = true;  // 엔진 파티클 등 멈추기

    [Header("Rocket Disable")]
    [SerializeField] private bool disableRocketController = true;
    [SerializeField] private bool disableColliderOnCrash = true;
    [SerializeField] private bool hideSpriteOnCrash = true;

    [Header("Cleanup")]
    [SerializeField] private bool destroyRocket = false;
    [SerializeField] private float destroyDelay = 0.5f;

    private bool crashed;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (crashed) return;
        if (!other.CompareTag(obstacleTag)) return;

        Crash();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (crashed) return;
        if (!collision.collider.CompareTag(obstacleTag)) return;

        Crash();
    }

    private void Crash()
    {
        crashed = true;

        // 1) 로켓 움직임 멈추기
        if (disableRocketController)
        {
            var controller = GetComponent<RocketOrbitController>();
            if (controller != null) controller.enabled = false;
        }

        // 2) 콜라이더 꺼서 추가 충돌 방지
        if (disableColliderOnCrash)
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }

        // 3) 로켓 파티클(엔진 등) 멈추기
        if (stopChildParticles)
        {
            var particles = GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in particles)
            {
                if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        // 4) 로켓 스프라이트 숨기기(원하면)
        if (hideSpriteOnCrash)
        {
            var renderers = GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var r in renderers)
            {
                if (r != null) r.enabled = false;
            }
        }

        // 5) 크래쉬 파티클 생성
        SpawnCrashFx();

        // 6) 게임 오버 호출 (너가 게임오버에 크래쉬 넣어놨다 했으니 이거면 끝)
        GameManager.Instance?.GameOver();

        // 7) 로켓 제거(선택)
        if (destroyRocket)
            Destroy(gameObject, destroyDelay);
    }

    private void SpawnCrashFx()
    {
        if (crashFxPrefab == null) return;

        ParticleSystem fx = Instantiate(
            crashFxPrefab,
            transform.position,
            Quaternion.identity,
            fxParent
        );

        fx.Play();

        // 파티클 자동 제거 (대략적으로 duration + startLifetime 최대치)
        var main = fx.main;
        float maxLife = main.duration + main.startLifetime.constantMax;
        Destroy(fx.gameObject, maxLife);
    }
}
