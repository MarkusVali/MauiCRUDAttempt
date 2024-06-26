﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiCRUDAttempt.Data;
using MauiCRUDAttempt.Models;
using System.Collections.ObjectModel;

namespace MauiCRUDAttempt.ViewModels
{
    public partial class WishlistViewModel : ObservableObject
    {
        private readonly DatabaseContext _context;

        public WishlistViewModel(DatabaseContext context)
        {
            _context = context;
        }

        [ObservableProperty]
        private ObservableCollection<Wishlist> _wishlists = new();

        [ObservableProperty]
        private Wishlist _operatingWishlist = new();

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _busyText;

        public async Task LoadWishlistsAsync()
        {
            await ExecuteAsync(async () =>
            {
                var wishlists = await _context.GetAllAsync<Wishlist>();
                if (wishlists is not null && Wishlists.Any())
                {
                    Wishlists ??= new ObservableCollection<Wishlist>();

                    foreach (var wishlist in wishlists)
                    {
                        Wishlists.Add(wishlist);
                    }
                }
            }, "Fetching wishlists...");
        }

        private async Task ExecuteAsync(Func<Task> operation, string? busyText = null)
        {
            IsBusy = true;
            BusyText = busyText ?? "Processing...";
            try
            {
                await operation?.Invoke();
            }
            catch (Exception)
            {
                /*
                 * {System.TypeInitializationException: The type initializer for 'SQLite.SQLiteConnection' threw an exception.
                 ---> System.IO.FileNotFoundException: Could not load file or assembly 'SQLitePCLRaw.provider.dynamic_cdecl, Version=2.0.4.976, Culture=neutral, PublicKeyToken=b68184102cba0b3b' or one of its dependencies.
                File name: 'SQLitePCLRaw.provider.dynamic_cdecl, Version=2.0.4.976, Culture=neutral, PublicKeyToken=b68184102cba0b3b'
                   at SQLitePCL.Batteries_V2.Init()
                   at SQLite.SQLiteConnection..cctor()
                   --- End of inner exception stack trace ---
                   at SQLite.SQLiteConnectionWithLock..ctor(SQLiteConnectionString connectionString)
                   at SQLite.SQLiteConnectionPool.Entry..ctor(SQLiteConnectionString connectionString)
                   at SQLite.SQLiteConnectionPool.GetConnectionAndTransactionLock(SQLiteConnectionString connectionString, Object& transactionLock)
                   at SQLite.SQLiteConnectionPool.GetConnection(SQLiteConnectionString connectionString)
                   at SQLite.SQLiteAsyncConnection.GetConnection()
                   at SQLite.SQLiteAsyncConnection.<>c__DisplayClass33_0`1[[SQLite.CreateTableResult, SQLite-net, Version=1.8.116.0, Culture=neutral, PublicKeyToken=null]].<WriteAsync>b__0()
                   at System.Threading.Tasks.Task`1[[SQLite.CreateTableResult, SQLite-net, Version=1.8.116.0, Culture=neutral, PublicKeyToken=null]].InnerInvoke()
                   at System.Threading.Tasks.Task.<>c.<.cctor>b__273_0(Object obj)
                   at System.Threading.ExecutionContext.RunFromThreadPoolDispatchLoop(Thread threadPoolThread, ExecutionContext executionContext, ContextCallback callback, Object state)
                --- End of stack trace from previous location ---
                   at System.Threading.ExecutionContext.RunFromThreadPoolDispatchLoop(Thread threadPoolThread, ExecutionContext executionContext, ContextCallback callback, Object state)
                   at System.Threading.Tasks.Task.ExecuteWithThreadLocal(Task& currentTaskSlot, Thread threadPoolThread)
                --- End of stack trace from previous location ---
                   at MAUISql.Data.DatabaseContext.<CreateTableIfNotExists>d__6`1[[MAUISql.Models.Wishlist, MAUISql, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]].MoveNext() in D:\MAUI\MAUISql\MAUISql\Data\DatabaseContext.cs:line 18
                   at MAUISql.Data.DatabaseContext.<GetTableAsync>d__7`1[[MAUISql.Models.Wishlist, MAUISql, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]].MoveNext() in D:\MAUI\MAUISql\MAUISql\Data\DatabaseContext.cs:line 23
                   at MAUISql.Data.DatabaseContext.<GetAllAsync>d__8`1[[MAUISql.Models.Wishlist, MAUISql, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]].MoveNext() in D:\MAUI\MAUISql\MAUISql\Data\DatabaseContext.cs:line 29
                   at MAUISql.ViewModels.WishlistViewModel.<LoadwishlistsAsync>b__6_0() in D:\MAUI\MAUISql\MAUISql\ViewModels\WishlistViewModel.cs:line 34
                   at MAUISql.ViewModels.WishlistViewModel.ExecuteAsync(Func`1 operation, String busyText) in D:\MAUI\MAUISql\MAUISql\ViewModels\WishlistViewModel.cs:line 103}
                 */
                throw;
            }
            finally
            {
                IsBusy = false;
                BusyText = "Processing...";
            }
        }

        [ICommand]
        private void SetOperatingWishlist(Wishlist? wishlist) => OperatingWishlist = wishlist ?? new();

        [ICommand]
        private async Task SaveWishlistAsync()
        {
            if (OperatingWishlist is null)
                return;

            var (isValid, errorMessage) = OperatingWishlist.Validate();
            if (!isValid)
            {
                await Shell.Current.DisplayAlert("Validation Error", errorMessage, "Ok");
                return;
            }

            var busyText = OperatingWishlist.Id == 0 ? "Creating wishlist..." : "Updating wishlist...";
            await ExecuteAsync(async () =>
            {
                if (OperatingWishlist.Id == 0)
                {
                    await _context.AddItemAsync<Wishlist>(OperatingWishlist);
                    Wishlists.Add(OperatingWishlist);
                }
                else
                {
                    if (await _context.UpdateItemAsync<Wishlist>(OperatingWishlist))
                    {
                        var productCopy = OperatingWishlist.Clone();

                        var index = Wishlists.IndexOf(OperatingWishlist);
                        Wishlists.RemoveAt(index);

                        Wishlists.Insert(index, productCopy);
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Error", "Wishlist updating error", "Ok");
                        return;
                    }
                }
                SetOperatingWishlistCommand.Execute(new());
            }, busyText);
        }

        [ICommand]
        private async Task DeleteWishlistAsync(int id)
        {
            await ExecuteAsync(async () =>
            {
                if (await _context.DeleteItemByKeyAsync<Wishlist>(id))
                {
                    var wishlist = Wishlists.FirstOrDefault(p => p.Id == id);
                    Wishlists.Remove(wishlist);
                }
                else
                {
                    await Shell.Current.DisplayAlert("Delete Error", "Wishlist was not deleted", "Ok");
                }
            }, "Deleting wishlist...");
        }


    }
}
