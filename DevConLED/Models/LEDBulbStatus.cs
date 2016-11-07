using System;
using System.Drawing;
using PropertyChanged;

namespace DevConLED
{
	[ImplementPropertyChanged]
	public class LEDBulbStatus
	{
		public bool PoweredOn { get; set; }
		public byte PoweredOnRaw { get; set; }

		public int Red { get; set; }
		public byte RedRaw { get; set; }

		public int Green { get; set; }
		public byte GreenRaw { get; set; }

		public int Blue { get; set; }
		public byte BlueRaw { get; set; }

		public int Speed { get; set; }
		public byte SpeedRaw { get; set; }


		public void TrySet(byte[] value)
		{
			if (value.Length > 2)
			{
				PoweredOnRaw = value[2];
				PoweredOn = PoweredOnRaw == 0x23;
			}

			if (value.Length > 5)
			{
				SpeedRaw = value[5];
				Speed = 32-SpeedRaw;
			}

			if (value.Length > 6)
			{
				RedRaw = value[6];
				Red = RedRaw;
			}

			if (value.Length > 7)
			{
				GreenRaw = value[7];
				Green = GreenRaw;
			}

			if (value.Length > 8)
			{
				BlueRaw = value[8];
				Blue = BlueRaw;
			}
		}

		public Color GetColor()
		{
			return Color.FromArgb(Red, Green, Blue);
		}
	}
}
