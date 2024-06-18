using MauiCRUDAttempt.Data;
using MauiCRUDAttempt.ViewModels;

namespace MauiCRUDAttempt
{
    public partial class MainPage : ContentPage
    {
        private readonly WishlistViewModel _wishlistViewModel;
        public MainPage()
        {
            InitializeComponent();
            var databaseContext = new DatabaseContext();
            _wishlistViewModel = new WishlistViewModel(databaseContext);
        }

        async void RedirectToCRUDPage(System.Object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new CRUDPage(_wishlistViewModel));
        }
    }
}