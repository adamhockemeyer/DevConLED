using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.Ble;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Xamarin.Forms;
using System.Linq;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace DevConLED
{
	public class DeviceViewModel : AbstractViewModel
	{
		IDisposable conn;
		IDevice device;
		IDisposable readRssiTimer;

		public override event Action<object> NavigateToPage;

		[Reactive]
		public string Name { get; set; }
		[Reactive]
		public string ConnectText { get; set; } = "Connect";
		[Reactive]
		public Guid Uuid { get; set; }
		[Reactive]
		public int Rssi { get; set; }
		[Reactive]
		public ConnectionStatus Status { get; set; } = ConnectionStatus.Disconnected;
		[Reactive]
		public bool IsConnected { get; set; }
		[Reactive]
		public string PowerButtonImg { get; set; } = "power-button-off.png";

		public ICommand ConnectionToggle { get; }
		public ICommand PowerToggle { get; }
		public ICommand QueryCurrentStatus { get; }
		public ICommand SetRGBValue { get; }
		public ICommand ColorClickedCommand { get; }

		[Reactive]
		public LEDBulbStatus BulbStatus { get; set; }

		[Reactive]
		IGattCharacteristic WriteCharacteristic { get; set; }

		[Reactive]
		IGattCharacteristic ReadCharacteristic { get; set; }

		public DeviceViewModel(ScanResultViewModel result)
		{
			this.device = result.Device;

			Name = result.Name;
			Uuid = result.Uuid;

			BulbStatus = new LEDBulbStatus();

			ColorClickedCommand = new Command<Button>(( obj) => 
			{
				BulbStatus.Red = (int)(obj.BackgroundColor.R * 255);
				BulbStatus.Green = (int)(obj.BackgroundColor.G * 255);
				BulbStatus.Blue = (int)(obj.BackgroundColor.B * 255);
			});

			this.ConnectionToggle = ReactiveCommand.CreateAsyncTask(
				this.WhenAny(
					x => x.Status,
					x => x.Value != ConnectionStatus.Disconnecting
				),
				x =>
				{
					if (this.conn == null)
					{
						this.conn = this.device.CreateConnection().Subscribe();
					}
					else
					{
						this.conn?.Dispose();
						this.conn = null;
					}
					return Task.FromResult(Unit.Default);
				}
			);

			this.QueryCurrentStatus = ReactiveCommand.CreateAsyncTask(
				this.WhenAny(
					x => x.WriteCharacteristic,
					x => x.ReadCharacteristic,
					(write, read) => write.Value.CanWrite() && read.Value.CanNotify()
				),
				 x =>
				 {
					 Debug.WriteLine($"Getting current status...");

					 WriteCharacteristic.WriteWithoutResponse(LEDBulbManager.Current.CurrentStatus());

					 return Task.FromResult(Unit.Default);
				 }
			);

			this.WhenAnyValue(x => x.Status, x => x.WriteCharacteristic, x => x.ReadCharacteristic, (status, write, read) => new { status, write, read }).Where(x => x.status == ConnectionStatus.Connected && x.write != null && x.read != null).Subscribe(connected =>
				  {
					  QueryCurrentStatus.Execute(null);
				  });



			var canPowerToggle = this.WhenAny(x => x.IsConnected, x => x.WriteCharacteristic, (x, w) => x.Value == true && w != null);

			this.PowerToggle = ReactiveCommand.CreateAsyncTask(canPowerToggle,
				x =>
				{

					WriteCharacteristic.WriteWithoutResponse(BulbStatus.PoweredOn ? LEDBulbManager.Current.PowerOff() : LEDBulbManager.Current.PowerOn());

					QueryCurrentStatus.Execute(null);
					return Task.FromResult(Unit.Default);
				});

			this.SetRGBValue = ReactiveCommand.CreateAsyncTask(x => 
			{
				WriteCharacteristic.WriteWithoutResponse(LEDBulbManager.Current.SetRGBValue(BulbStatus.Red, BulbStatus.Green, BulbStatus.Blue));

				return Task.FromResult(Unit.Default);
			});

			BulbStatus.WhenAnyValue(x => x.Red, x => x.Green, x => x.Blue).Subscribe(x => 
			{
				SetRGBValue.Execute(null);
			});

			this.device
				.WhenNameUpdated()
				.Subscribe(x => this.Name = this.device.Name);

			this.device
				.WhenStatusChanged()
				.Subscribe(x => Device.BeginInvokeOnMainThread(() =>
			   {
				   this.Status = x;

				   switch (x)
				   {
					   case ConnectionStatus.Disconnecting:
					   case ConnectionStatus.Connecting:
						   this.IsConnected = false;
						   this.ConnectText = x.ToString();
						   break;

					   case ConnectionStatus.Disconnected:
						   this.IsConnected = false;
						   this.ConnectText = "Connect";
						   this.readRssiTimer?.Dispose();
						   break;

					   case ConnectionStatus.Connected:
						   this.IsConnected = true;
						   this.ConnectText = "Disconnect";
						   this.readRssiTimer = this.device
							   .WhenRssiUpdated()
							   .Subscribe(rssi => this.Rssi = rssi);
						   break;
				   }
			   }));

			this.device.WhenAnyCharacteristicDiscovered()
				.Subscribe((IGattCharacteristic obj) =>
			{
				Debug.WriteLine($"+ Characteristic Discovered: {obj.Uuid} {obj.Properties}");

				if (LEDBulbManager.Current.CanInterface(obj))
				{
					Debug.WriteLine($"++ CanInterface");

					if (obj.Properties == CharacteristicProperties.WriteNoResponse)
					{
						WriteCharacteristic = obj;
					}
					else if (obj.Properties == CharacteristicProperties.Notify)
					{
						ReadCharacteristic = obj;
					}
				}
				//Characteristics.Add(obj);
			});

			this.WhenAnyValue(x => x.ReadCharacteristic).Where(x => x != null).Subscribe(x => 
			{ 
				ReadCharacteristic.SubscribeToNotifications().Subscribe(notification =>
			   {
				   Debug.WriteLine($"+ SubscribeToNotifications: {BitConverter.ToString(notification)}");

				   BulbStatus = BulbStatus ?? new LEDBulbStatus();
				   BulbStatus.TrySet(notification);

				   if (BulbStatus.PoweredOn)
				   {
					   PowerButtonImg = "power-button-on.png";
				   }
				   else
				   {
					   PowerButtonImg = "power-button-off.png";
				   }
			   });
			});



		}


	}
}
