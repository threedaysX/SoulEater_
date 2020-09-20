using UnityEngine;

public class TutorialTrapTrigger : MonoBehaviour
{
    public ProjectileSetting projectileSetting;
    public ProjectileDirectSetting projectileDirectSetting;
    public GameObject projectile;
    public float slowDownFactor;
    public float slowDownTime;

    private bool canSpawn = true;

    private void Start()
    {
        ProjectileDataInitializer ProjectileData = new ProjectileDataInitializer(projectileSetting);
        projectileDirectSetting.initialAngleArray = ProjectileData.GetInitialAngle();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && canSpawn)
        {
            TimeScaleController.Instance.DoSlowMotion(slowDownFactor, 0f, slowDownTime);
            ProjectileSpawner.Instance.InstantiateProjectile(projectile, new ProjectileState.StraightWithDirection(), projectileSetting, projectileDirectSetting);
            canSpawn = false;
        }
    }
}
