using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using DarkRift;
using DarkRift.Server;

public static class CommandReceiver
{
    public static void MessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage())
        {
            if(message.Tag <= 10)
            {

                if (message.Tag == Tags.CreateEntityWithPosition)
                {
                    using (DarkRiftReader reader = message.GetReader())
                    {
                        ICommand command = new Command_CreateEntityWithPosition(reader.ReadUInt16(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                        CommandProcessor.AddCommand(command, 0);
                    }
                }

                if (message.Tag == Tags.LoginRequest)
                {
                    using (DarkRiftReader reader = message.GetReader())
                    {
                        ICommand command = new Command_ProcessLogin(reader.ReadUInt16());

                        CommandProcessor.AddCommand(command, 0);
                    }
                }

                if (message.Tag == Tags.ReceivedMoveOrder)
                {
                    using (DarkRiftReader reader = message.GetReader())
                    {
                        List<float> clickPosComponents = new List<float>();

                        clickPosComponents.Add(reader.ReadSingle());
                        clickPosComponents.Add(reader.ReadSingle());
                        clickPosComponents.Add(reader.ReadSingle());

                        List<ushort> unitNetworkIDs = new List<ushort>();

                        while (reader.Position < reader.Length)
                        {
                            unitNetworkIDs.Add(reader.ReadUInt16());
                        }

                        ICommand command = new Command_ClearOldStateData(unitNetworkIDs);

                        CommandProcessor.AddCommand(command, 0);

                        ICommand command0 = new Command_MoveOrder(clickPosComponents[0], clickPosComponents[1], clickPosComponents[2], unitNetworkIDs);

                        CommandProcessor.AddCommand(command0, 150);
                    }
                }

                return;
            }

            if(message.Tag <= 20)
            {
                if(message.Tag == Tags.BuildRequest)
                {
                    using (DarkRiftReader reader = message.GetReader())
                    {
                        ICommand command = new Command_BuildRequest(reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadSingle(),
                            reader.ReadSingle(), reader.ReadSingle(), reader.ReadByte(), reader.ReadInt16(), reader.ReadInt16(),
                            reader.ReadInt16());

                        CommandProcessor.AddCommand(command, 0);
                    }
                }

                if(message.Tag == Tags.ReceivedMoveOrderWithEndRotation)
                {
                    using (DarkRiftReader reader = message.GetReader())
                    {
                        List<float> clickPosComponents = new List<float>();

                        clickPosComponents.Add(reader.ReadSingle());
                        clickPosComponents.Add(reader.ReadSingle());
                        clickPosComponents.Add(reader.ReadSingle());

                        List<float> directionComponents = new List<float>();

                        directionComponents.Add(reader.ReadSingle());
                        directionComponents.Add(reader.ReadSingle());

                        List<ushort> unitNetworkIDs = new List<ushort>();

                        while (reader.Position < reader.Length)
                        {
                            unitNetworkIDs.Add(reader.ReadUInt16());
                        }

                        ICommand command = new Command_ClearOldStateData(unitNetworkIDs);

                        CommandProcessor.AddCommand(command, 0);

                        ICommand command0 = new Command_MoveOrderWithEndRotation(clickPosComponents[0], clickPosComponents[1], clickPosComponents[2],
                            directionComponents[0], directionComponents[1], unitNetworkIDs);

                        CommandProcessor.AddCommand(command0, 150);
                    }
                }

                if (message.Tag == Tags.ReceivedBuildOrder)
                {
                    using (DarkRiftReader reader = message.GetReader())
                    {
                        ushort buildingNetworkID = reader.ReadUInt16();

                        List<ushort> unitNetworkIDs = new List<ushort>();

                        while (reader.Position < reader.Length)
                        {
                            unitNetworkIDs.Add(reader.ReadUInt16());
                        }

                        ICommand command = new Command_ClearOldStateData(unitNetworkIDs);
                        CommandProcessor.AddCommand(command, 0);

                        ICommand command0 = new Command_BuildOrder(buildingNetworkID, unitNetworkIDs);
                        CommandProcessor.AddCommand(command0, 150);
                    }
                }

                if (message.Tag == Tags.ReceivedWorkOrder)
                {
                    using (DarkRiftReader reader = message.GetReader())
                    {
                        ushort buildingNetworkID = reader.ReadUInt16();

                        List<ushort> unitNetworkIDs = new List<ushort>();

                        while (reader.Position < reader.Length)
                        {
                            unitNetworkIDs.Add(reader.ReadUInt16());
                        }

                        ICommand command = new Command_ClearOldStateData(unitNetworkIDs);
                        CommandProcessor.AddCommand(command, 0);

                        ICommand command0 = new Command_WorkOrder(buildingNetworkID, unitNetworkIDs);
                        CommandProcessor.AddCommand(command0, 150);
                    }
                }

                if (message.Tag == Tags.EntityStateDataRequest)
                {
                    using (DarkRiftReader reader = message.GetReader())
                    {
                        ICommand command = new Command_AddSendEntityStateData(reader.ReadUInt16(), e.Client.ID);
                        CommandProcessor.AddCommand(command, 0);
                    }
                }

                if (message.Tag == Tags.CancelEntityStateDataRequest)
                {
                    using (DarkRiftReader reader = message.GetReader())
                    {
                        ICommand command = new Command_RemoveSendEntityStateData(reader.ReadUInt16());
                        CommandProcessor.AddCommand(command, 0);
                    }
                }

                return;
            }

            if (message.Tag == Tags.ReceivedProducerCreateRequest)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ICommand command = new Command_AddProducerQueueElementToProducer(reader.ReadUInt16(), reader.ReadUInt16());

                    CommandProcessor.AddCommand(command, 0);
                }
            }

            if (message.Tag == Tags.ReceivedResourceTransferRequest)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort unitNetworkID = reader.ReadUInt16();
                    int amount = reader.ReadInt32();

                    List<ushort> unitNetworkIDsToTransferTo = new List<ushort>();

                    while(reader.Position < reader.Length)
                    {
                        unitNetworkIDsToTransferTo.Add(reader.ReadUInt16());
                    }

                    ICommand command = new Command_ProcessResourceTransferRequest(unitNetworkID, amount, unitNetworkIDsToTransferTo);
                    CommandProcessor.AddCommand(command, 0);
                }
            }
        }
    }
}
