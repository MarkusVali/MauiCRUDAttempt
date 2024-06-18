using MauiCRUDAttempt.ViewModels;

namespace MauiCRUDAttempt
{
    public partial class CRUDPage : ContentPage
    {
        private readonly WishlistViewModel _wishlistViewModel;

        public CRUDPage(WishlistViewModel wishlistViewModel)
        {
            InitializeComponent();
            BindingContext = wishlistViewModel;
            _wishlistViewModel = wishlistViewModel;
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            await _wishlistViewModel.LoadWishlistsAsync();
        }
    }
}