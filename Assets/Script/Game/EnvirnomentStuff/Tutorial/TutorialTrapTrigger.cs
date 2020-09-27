using UnityEngine;

public class TutorialTrapTrigger : MonoBehaviour
{
    public ProjectileInitSetting projectileInitSetting;
    public ProjectileSetting projectileSetting;
    public GameObject projectile;
    public Transform spawnPoint;
    public float slowDownFactor;
    public float slowDownTime;

    private ProjectileState.StraightWithDirection projectileState = new ProjectileState.StraightWithDirection();
    private bool canSpawn = true;

    private void Start() {
        projectileInitSetting = new ProjectileInitSetting(spawnPoint.position, 30, 180, 180);
        projectileSetting = new ProjectileSetting { moveSpeed = 3, lifeTime = 3, elementType = ElementType.None, };
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && canSpawn)
        {
            TimeScaleController.Instance.DoSlowMotion(slowDownFactor, 0f, slowDownTime);
            ProjectileSpawner.Instance.InstantiateProjectile(projectile, projectileState, projectileInitSetting, projectileSetting);
            canSpawn = false;
        }
    }
}
