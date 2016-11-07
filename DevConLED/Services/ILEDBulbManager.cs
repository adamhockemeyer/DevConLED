using System;
using Acr.Ble;

namespace DevConLED
{
	public interface ILEDBulbManager
	{
		bool CanInterface(IGattCharacteristic characteristic);

		byte[] PowerOn();

		byte[] PowerOff();

		byte[] SetRGBValue(int red, int green, int blue);

		byte[] CurrentStatus();


	}
}
