using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Acr.Ble.Plugins;


namespace DevConLED
{
	public partial class BluetoothDevicesPage : ContentPage
	{

		public BluetoothDevicesPage()
		{
			InitializeComponent();

			this.BindingContext = new BluetoothDevicesViewModel();
		}

		async void Vm_NavigateToPage(object obj)
		{
			var page = obj as Page;

			var selected = BLEDeviceListView.SelectedItem as ScanResultViewModel;

			page.BindingContext = new DeviceViewModel(selected);

			await this.Navigation.PushAsync(page);
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			var vm = this.BindingContext as BluetoothDevicesViewModel;

			vm.NavigateToPage += Vm_NavigateToPage;
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();

			var vm = this.BindingContext as BluetoothDevicesViewModel;

			vm.NavigateToPage -= Vm_NavigateToPage;
		}
	}
}
