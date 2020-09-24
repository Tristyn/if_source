using System;

public static class Events
{
    public static Action<Machine> machineCreated;
    public static Action<Machine> machineDeleted;
}
