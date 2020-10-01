using Avalonia.Threading;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using WalletWasabi.Gui;
using WalletWasabi.Wallets;

namespace WalletWasabi.Fluent.ViewModels
{
	public class NavBarViewModel : ViewModelBase
	{
		private ObservableCollection<WalletViewModelBase> _items;
		private ObservableCollection<NavBarItemViewModel> _topItems;
		private ObservableCollection<NavBarItemViewModel> _bottomItems;
		private NavBarItemViewModel _selectedItem;
		private Dictionary<Wallet, WalletViewModelBase> _walletDictionary;
		private bool _anyWalletStarted;
		private bool _isBackButtonVisible;
		private IScreen _screen;
		private bool _isNavigating;

		public NavBarViewModel(IScreen screen, RoutingState router, WalletManager walletManager, UiConfig uiConfig)
		{
			_screen = screen;
			Router = router;
			_topItems = new ObservableCollection<NavBarItemViewModel>();
			_items = new ObservableCollection<WalletViewModelBase>();
			_bottomItems = new ObservableCollection<NavBarItemViewModel>();

			_walletDictionary = new Dictionary<Wallet, WalletViewModelBase>();

			SelectedItem = new HomePageViewModel(screen);
			_topItems.Add(_selectedItem);
			_bottomItems.Add(new AddWalletPageViewModel(screen));
			_bottomItems.Add(new SettingsPageViewModel(screen));

			Router.CurrentViewModel
				.OfType<NavBarItemViewModel>()
				.Subscribe(x=>
			{				
				if(_items.Contains(x) || _topItems.Contains(x) || _bottomItems.Contains(x))
				{
					if (!_isNavigating)
					{
						_isNavigating = true;
						SelectedItem = x;
						_isNavigating = false;
					}
				}
			});

			this.WhenAnyValue(x => x.SelectedItem)
				.OfType<IRoutableViewModel>()				
				.Subscribe(x =>
				{
					if (!_isNavigating)
					{
						_isNavigating = true;												
						Router.NavigateAndReset.Execute(x);
						_isNavigating = false;
					}
				});

			Observable.FromEventPattern(Router.NavigationStack, nameof(Router.NavigationStack.CollectionChanged))
				.ObserveOn(RxApp.MainThreadScheduler)
				.Subscribe(_ => IsBackButtonVisible = Router.NavigationStack.Count > 1);

			Observable
				.FromEventPattern<WalletState>(walletManager, nameof(WalletManager.WalletStateChanged))
				.ObserveOn(RxApp.MainThreadScheduler)
				.Subscribe(x =>
				{
					var wallet = x.Sender as Wallet;

					if (wallet is { } && _walletDictionary.ContainsKey(wallet))
					{
						if (wallet.State == WalletState.Stopping)
						{
							RemoveWallet(_walletDictionary[wallet]);
						}
						else if (_walletDictionary[wallet] is ClosedWalletViewModel cwvm && wallet.State == WalletState.Started)
						{
							OpenClosedWallet(walletManager, uiConfig, cwvm);
						}
					}

					AnyWalletStarted = Items.OfType<WalletViewModelBase>().Any(x => x.WalletState == WalletState.Started);
				});

			Observable
				.FromEventPattern<Wallet>(walletManager, nameof(WalletManager.WalletAdded))
				.Select(x => x.EventArgs)
				.Where(x => x is { })
				.ObserveOn(RxApp.MainThreadScheduler)
				.Subscribe(wallet =>
				{
					WalletViewModelBase vm = (wallet.State <= WalletState.Starting)
						? ClosedWalletViewModel.Create(screen, walletManager, wallet)
						: WalletViewModel.Create(screen, uiConfig, wallet);

					InsertWallet(vm);
				});			

			Dispatcher.UIThread.Post(() =>
			{
				LoadWallets(walletManager);
			});
		}

		public ReactiveCommand<Unit, IRoutableViewModel> GoNext { get; }

		public ReactiveCommand<Unit, Unit> GoBack => Router.NavigateBack;

		public bool AnyWalletStarted
		{
			get => _anyWalletStarted;
			set => this.RaiseAndSetIfChanged(ref _anyWalletStarted, value);
		}		

		public ObservableCollection<NavBarItemViewModel> TopItems
		{
			get { return _topItems; }
			set { this.RaiseAndSetIfChanged(ref _topItems, value); }
		}


		public ObservableCollection<WalletViewModelBase> Items
		{
			get { return _items; }
			set { this.RaiseAndSetIfChanged(ref _items, value); }
		}		

		public ObservableCollection<NavBarItemViewModel> BottomItems
		{
			get { return _bottomItems; }
			set { this.RaiseAndSetIfChanged(ref _bottomItems, value); }
		}

		public NavBarItemViewModel SelectedItem
		{
			get { return _selectedItem; }
			set
			{
				if (_selectedItem != value)
				{
					if (_selectedItem is { })
					{
						_selectedItem.IsSelected = false;
					}

					_selectedItem = null;

					this.RaisePropertyChanged();

					_selectedItem = value;

					this.RaisePropertyChanged();

					if (_selectedItem is { })
					{
						_selectedItem.IsSelected = true;
					}
				}
			}
		}

		public bool IsBackButtonVisible
		{
			get => _isBackButtonVisible;
			set => this.RaiseAndSetIfChanged(ref _isBackButtonVisible, value);
		}

		public RoutingState Router { get; }

		private void LoadWallets(WalletManager walletManager)
		{
			foreach (var wallet in walletManager.GetWallets())
			{
				InsertWallet(ClosedWalletViewModel.Create(_screen, walletManager, wallet));
			}
		}

		private void OpenClosedWallet(WalletManager walletManager, UiConfig uiConfig, ClosedWalletViewModel closedWalletViewModel)
		{
			var select = SelectedItem == closedWalletViewModel;

			RemoveWallet(closedWalletViewModel);

			var walletViewModel = OpenWallet(walletManager, uiConfig, closedWalletViewModel.Wallet);

			if (select)
			{
				SelectedItem = walletViewModel;
			}
		}

		private WalletViewModelBase OpenWallet(WalletManager walletManager, UiConfig uiConfig, Wallet wallet)
		{
			if (_items.OfType<WalletViewModel>().Any(x => x.Title == wallet.WalletName))
			{
				throw new Exception("Wallet already opened.");
			}

			var walletViewModel = WalletViewModel.Create(_screen, uiConfig, wallet);

			InsertWallet(walletViewModel);

			if (!walletManager.AnyWallet(x => x.State >= WalletState.Started && x != walletViewModel.Wallet))
			{
				walletViewModel.OpenWalletTabs();
			}

			walletViewModel.IsExpanded = true;

			return walletViewModel;
		}

		private void InsertWallet(WalletViewModelBase walletVM)
		{
			Items.InsertSorted(walletVM);
			_walletDictionary.Add(walletVM.Wallet, walletVM);
		}

		internal void RemoveWallet(WalletViewModelBase walletVM)
		{
			walletVM.Dispose();

			_items.Remove(walletVM);
			_walletDictionary.Remove(walletVM.Wallet);
		}
	}
}
