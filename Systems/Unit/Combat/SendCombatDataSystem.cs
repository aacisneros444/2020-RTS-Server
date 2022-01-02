using Unity.Entities;

/// <summary>
/// This system sends target data and firing data to the client.
/// </summary>
public class SendCombatDataSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem commandBufferSystem;
    protected override void OnCreate()
    {
        commandBufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer commandBuffer = commandBufferSystem.CreateCommandBuffer();

        ComponentDataFromEntity<NetworkID> networkIDs = GetComponentDataFromEntity<NetworkID>(true);

        //Send target entity so client units can start aiming. //This is for weapons that are on the root entity.
        Entities.WithNone<RootEntity, LocalWeapon>().WithAll<SendTarget>().ForEach((Entity entity, int entityInQueryIndex,
            in HasTarget hasTarget) =>
        {
            ICommand command = new Command_SendTarget(networkIDs[entity].value, networkIDs[hasTarget.entity].value, 0);

            CommandProcessor.AddCommand(command, 0f);

            commandBuffer.RemoveComponent<SendTarget>(entity);

        }).WithoutBurst().Run();

        //Send target entity so client units can start aiming. //This is for weapons that are not on the root entity.
        Entities.WithAll<SendTarget>().ForEach((Entity entity, int entityInQueryIndex, in HasTarget hasTarget, in LocalWeapon localWeapon,
            in RootEntity rootEntity) => 
        {
            ICommand command = new Command_SendTarget(networkIDs[rootEntity.entity].value,
                networkIDs[hasTarget.entity].value, localWeapon.localID);

            CommandProcessor.AddCommand(command, 0f);

            commandBuffer.RemoveComponent<SendTarget>(entity);

        }).WithoutBurst().Run();

        //Send start firing tick and gun data to client (client then uses prediction to know when to fire weapons).
        //This is for weapons that are on the root entity
        Entities.WithNone<RootEntity, LocalWeapon>().ForEach((Entity entity, int entityInQueryIndex, in HasTarget hasTarget, in SendStartFiring sendStartFiring, in Gun gun,
            in NetworkID networkID) =>
        {
            ICommand command = new Command_SendStartFiring(networkID.value, 0,
                sendStartFiring.startFiringTick, sendStartFiring.roundsInTheMagazine);

            CommandProcessor.AddCommand(command, 0f);

            commandBuffer.RemoveComponent<SendStartFiring>(entity);

        }).WithoutBurst().Run();

        //Send start firing tick and gun data to client (client then uses prediction to know when to fire weapons).
        //This is for weapons that are not on the root entity.
        Entities.ForEach((Entity entity, int entityInQueryIndex, in HasTarget hasTarget, in LocalWeapon localWeapon,
            in SendStartFiring sendStartFiring, in Gun gun, in RootEntity rootEntity) =>
        {
            if (!HasComponent<GunC>(entity))
            {
                ICommand command = new Command_SendStartFiring(networkIDs[rootEntity.entity].value,
                    localWeapon.localID, sendStartFiring.startFiringTick, sendStartFiring.roundsInTheMagazine);

                CommandProcessor.AddCommand(command, 0f);
            }
            else
            {
                ICommand command = new Command_SendStartFiringC(networkIDs[rootEntity.entity].value,
                    localWeapon.localID, sendStartFiring.startFiringTick, sendStartFiring.roundsInTheMagazine,
                    sendStartFiring.roundsInTheMagazineC);

                CommandProcessor.AddCommand(command, 0f);
            }

            commandBuffer.RemoveComponent<SendStartFiring>(entity);

        }).WithoutBurst().Run();

    }

}
