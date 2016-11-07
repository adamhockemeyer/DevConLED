using System;
using System.Drawing;
using System.Linq;
using Acr.Ble;

namespace DevConLED
{
	public class LEDBulbManager : ILEDBulbManager
	{
		const byte StaticColorConstant_0 = 0x56;
		const byte StaticColorConstant_4 = 0x00;
		const byte StaticColorConstant_5 = 0xF0;
		const byte StaticColorConstant_6 = 0xAA;

		private static LEDBulbManager current;

		private LEDBulbManager()
		{
		}

		public static LEDBulbManager Current
		{
			get
			{
				if (current == null)
				{
					current = new LEDBulbManager();
				}
				return current;
			}
		}

		public bool CanInterface(IGattCharacteristic characteristic)
		{
			var handles = GetHandleFromUUID(characteristic.Uuid);

			var write = handles.Equals("FFE9");

			var read = handles.Equals("FFE4");

			return write || read;
		}

		public byte[] PowerOff()
		{
			return new byte[] { 0xCC, 0x24, 0x33 };
		}

		public byte[] PowerOn()
		{
			return new byte[] { 0xCC, 0x23, 0x33 };
		}

		public byte[] SetRGBValue(int red, int green, int blue)
		{
			var color = Color.FromArgb(red, green, blue);

			return new byte[] { StaticColorConstant_0, color.R, color.G, color.B, StaticColorConstant_4, StaticColorConstant_5, StaticColorConstant_6 };
		}

		public byte[] CurrentStatus()
		{
			return new byte[] { 0xEF, 0x01, 0x77 };
		}

		string GetHandleFromUUID(Guid uuid)
		{
			if (uuid != null)
			{

				return BitConverter.ToString(uuid.ToByteArray().Take(2).Reverse().ToArray()).Replace("-","");
			}
			else
			{
				return "";
			}
		}

	}
}
