using System;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Acr.Ble;
using Xamarin.Forms;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

namespace DevConLED
{
	public class BluetoothDevicesViewModel : AbstractViewModel
	{
		IDisposable scan;
		IDisposable connect;
		public ICommand ScanToggle { get; }

		[Reactive]
		public ScanResultViewModel SelectedDevice { get; set; }
		[Reactive]
		public string ScanText { get; set; }
		[Reactive]
		public string ScanningStatus { get; set; }
		[Reactive]
		public bool IsScanning { get; set; }
		[Reactive]
		public bool IsSupported { get; set; }
		[Reactive]
		public string NameFilter { get; set; }

		public ObservableCollection<ScanResultViewModel> Devices { get; }

		public override event Action<object> NavigateToPage;

		public BluetoothDevicesViewModel()
		{
			this.Devices = new ObservableCollection<ScanResultViewModel>();

			BleAdapter.Current.WhenStatusChanged().Subscribe(status =>
			{
				this.IsSupported = status == AdapterStatus.PoweredOn;
			});

			BleAdapter.Current.WhenScanningStatusChanged().Subscribe(scanning =>
			{
				IsScanning = scanning;

				ScanText = scanning ? "Stop Scan" : "Scan";

				ScanningStatus = scanning ? "Scanning..." : "Idle";
			});



			this.ScanToggle = ReactiveCommand.CreateAsyncTask(
				this.WhenAny(
					x => x.IsSupported,
					x => x.Value
				),
				x =>
				{
					if (this.IsScanning)
					{
						this.scan?.Dispose();
					}
					else
					{
						this.Devices.Clear();
						this.ScanText = "Stop Scan";

						this.scan = BleAdapter.Current
								.Scan()
								.Subscribe(this.OnScanResult);
					}
					return Task.FromResult<object>(null);
				}
			);

			// Navigate to the details page when an item is selected
			this.WhenAnyValue(x => x.SelectedDevice).Where(x => x != null).Subscribe(x =>
			{
				if (IsScanning)
				{
					ScanToggle.Execute(null);
				}

				NavigateToPage(new DevicePage());
			});


			this.WhenAnyValue(x => x.NameFilter).Subscribe(filter =>
			{
				Devices.Clear();
			});

		}



		void OnScanResult(IScanResult result)
		{
			Device.BeginInvokeOnMainThread(() =>
			{
				var dev = this.Devices.FirstOrDefault(x => x.Uuid.Equals(result.Device.Uuid));
				if (dev != null)
				{
					dev.TrySet(result);
				}
				else
				{
					if (string.IsNullOrEmpty(NameFilter) || (!string.IsNullOrEmpty(result.Device.Name) && result.Device.Name.ToLower().Contains(NameFilter.ToLower())))
					{
						dev = new ScanResultViewModel();
						dev.TrySet(result);

						this.Devices.Add(dev);
					}
				}
			});
		}



	}
}
