using System;

[Serializable]
public struct MachineEfficiency
{
	const int TIME_WINDOW = 8;
	/// <summary>
	/// efficiency from 0.0 to 1.0
	/// </summary>
	[ReadOnly]
	public float efficiency;
	[ReadOnly]
	public bool operating;
	private BitArray32 ticks;
	private int tickIndex;
	
	public void Tick(bool operating)
	{
		this.operating = operating;
		ticks[tickIndex] = operating;
		tickIndex = (tickIndex + 1) % TIME_WINDOW;
		efficiency = ticks.PopCount() / (float)TIME_WINDOW;
	}
}