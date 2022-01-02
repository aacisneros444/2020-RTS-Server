using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandProcessor : MonoBehaviour
{
    private static Queue<ICommand> commandQueue;
    public static List<BufferedCommand> commandBuffer;

    private void Awake()
    {
        commandQueue = new Queue<ICommand>();
        commandBuffer = new List<BufferedCommand>();
    }

    public static void AddCommand(ICommand command, float bufferTime)
    {
        if(bufferTime == 0)
        {
            //commandQueue.Enqueue(command);
            command.Execute();
        }

        if(bufferTime > 0)
        {
            commandBuffer.Add(new BufferedCommand(command, (bufferTime / 1000)));
        }
    }

    private void Update()
    {
        //if (commandQueue.Count > 0)
        //{
        //    for (int i = 0; i < commandQueue.Count; i++)
        //    {
        //        commandQueue.Dequeue().Execute();
        //    }
        //}

        if(commandBuffer.Count > 0)
        {
            for(int i = 0; i < commandBuffer.Count; i++)
            {
                commandBuffer[i].bufferTime -= Time.deltaTime;
                if(commandBuffer[i].bufferTime <= 0)
                {
                    commandBuffer[i].command.Execute();
                    commandBuffer.RemoveAt(i);
                }
            }
        }
    }
}

public class BufferedCommand
{
    public ICommand command;
    public float bufferTime;

    public BufferedCommand(ICommand command, float bufferTime)
    {
        this.command = command;
        this.bufferTime = bufferTime;
    }
}
