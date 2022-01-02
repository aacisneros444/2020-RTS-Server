using Unity.Entities;

/// <summary>
/// This system reloads all guns when they run out of ammunition (gun.roundsInTheMagazine == 0).
/// </summary>
[DisableAutoCreation]
public class ReloadSystem : SystemBase
{
    protected override void OnUpdate()
    {
        //Reload all normal guns.
        Entities.ForEach((Entity entity, ref Gun gun) =>
        {
            if(gun.roundsInMagazine == 0)
            {
                gun.reloadTicksTaken += 1;

                if (gun.reloadTicksTaken == gun.reloadTicks)
                {
                    gun.roundsInMagazine = gun.magazineSize;

                    gun.reloadTicksTaken = 0;
                    gun.ticksBetweenShotsTaken = gun.ticksBetweenShots;
                }
            }

        }).ScheduleParallel();

        //Reload all coaxial guns.
        Entities.ForEach((Entity entity, ref GunC gunC) =>
        {
            if (gunC.roundsInMagazine == 0)
            {
                gunC.reloadTicksTaken += 1;

                if (gunC.reloadTicksTaken == gunC.reloadTicks)
                {
                    gunC.roundsInMagazine = gunC.magazineSize;

                    gunC.reloadTicksTaken = 0;
                    gunC.ticksBetweenShotsTaken = gunC.ticksBetweenShots;
                }
            }

        }).ScheduleParallel();
    }
}
