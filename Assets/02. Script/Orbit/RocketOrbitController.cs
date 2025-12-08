using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketOrbitController : MonoBehaviour
{
    [Header("궤도 이동")]
    public float orbitSpeed = 90f;

    private bool isLaunched = false;
    private bool isOrbit = false;

    // 클릭시 isLaunched = true, isOrbit = false
    public void LaunchRocket()
    {
        isLaunched = true;
        isOrbit = false;
    }

    // 궤도 들어갈시 isLaunched = false, isOrbit = true
    public void EnterOrbit()
    {
        isLaunched = false;
        isOrbit = true;
    }

    // 궤도 회전 시 로켓 -> 원접선에 수평되게 로켓이. 회전


}