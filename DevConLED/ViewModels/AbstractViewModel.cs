using System;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace DevConLED
{
	public abstract class AbstractViewModel : ReactiveObject
	{
		public abstract event Action<object> NavigateToPage;

		public AbstractViewModel()
		{
			
		}

	}
}
