using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TrustSync.Application;
using TrustSync.Application.Security;
using TrustSync.Desktop.Services;
using TrustSync.Desktop.ViewModels.Auth;
using TrustSync.Desktop.ViewModels.Pages;
using TrustSync.Desktop.ViewModels.Shell;
using TrustSync.Desktop.Views.Auth;
using TrustSync.Infrastructure;
using TrustSync.Infrastructure.Persistence;

namespace TrustSync.Desktop;

public partial class App : Avalonia.Application
{
    private IHost? _host;
    private ISessionService? _sessionService;
    private ReminderBackgroundService? _reminderService;
    private MainWindow? _mainWindow;
    private bool _isAuthInProgress;

    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        SetupGlobalExceptionHandling();

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddApplicationServices();
                services.AddInfrastructureServices();

                // Auth ViewModels
                services.AddTransient<LoginViewModel>();
                services.AddTransient<FirstRunSetupViewModel>();
                services.AddTransient<LockScreenViewModel>();

                // Navigation
                services.AddSingleton<NavigationService>();

                // Shell
                services.AddSingleton<MainViewModel>();

                // Page ViewModels
                services.AddTransient<DashboardViewModel>();
                services.AddTransient<IncomeViewModel>();
                services.AddTransient<ExpensesViewModel>();
                services.AddTransient<CompaniesViewModel>();
                services.AddTransient<ProjectsViewModel>();
                services.AddTransient<DeductionsViewModel>();
                services.AddTransient<SavingsViewModel>();
                services.AddTransient<ReportsViewModel>();
                services.AddTransient<BackupViewModel>();
                services.AddTransient<RemindersViewModel>();
                services.AddTransient<SettingsViewModel>();

                // Windows
                services.AddSingleton<MainWindow>();
            })
            .Build();

        Services = _host.Services;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Initialize database (migrate + seed)
            await Infrastructure.DependencyInjection.InitializeDatabaseAsync(Services);

            // Load saved theme
            await LoadThemeAsync();

            // Run auth flow
            var authenticated = await RunAuthFlowAsync();
            if (!authenticated)
            {
                desktop.Shutdown();
                return;
            }

            // Set up session monitoring
            _sessionService = Services.GetRequiredService<ISessionService>();
            _sessionService.SessionExpired += OnSessionExpired;

            // Show main window
            _mainWindow = Services.GetRequiredService<MainWindow>();
            desktop.MainWindow = _mainWindow;
            desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
            _mainWindow.Show();

            // Start reminder background service
            _reminderService = new ReminderBackgroundService(Services);
            _reminderService.NotificationFired += (_, notification) =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    // Show in-app notification via MainViewModel toast
                    var mainVm = Services.GetRequiredService<MainViewModel>();
                    mainVm.ShowReminderNotification(notification.Title);
                });
            };
            _reminderService.Start();

            // Setup system tray
            SetupTrayIcon(desktop);

            desktop.ShutdownRequested += async (_, _) =>
            {
                _reminderService?.Dispose();
                _sessionService?.EndSession();
                if (_host is not null)
                {
                    await _host.StopAsync();
                    _host.Dispose();
                }
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async Task<bool> RunAuthFlowAsync()
    {
        var authService = Services.GetRequiredService<IAuthenticationService>();
        var isFirstRun = await authService.IsFirstRunRequiredAsync();

        if (isFirstRun)
        {
            var setupVm = Services.GetRequiredService<FirstRunSetupViewModel>();
            var setupView = new FirstRunSetupView { DataContext = setupVm };

            var authWindow = new AuthWindow();
            var tcs = new TaskCompletionSource<bool>();
            setupVm.SetupCompleted += (_, _) => tcs.TrySetResult(true);
            authWindow.Closing += (_, _) => tcs.TrySetResult(false);

            authWindow.SetContent(setupView);
            authWindow.Show();

            var result = await tcs.Task;
            authWindow.Close();

            if (!result) return false;
        }

        // Show login
        {
            var loginVm = Services.GetRequiredService<LoginViewModel>();
            var loginView = new LoginView { DataContext = loginVm };

            var loginWindow = new AuthWindow();
            var tcs = new TaskCompletionSource<bool>();
            loginVm.LoginSucceeded += (_, _) => tcs.TrySetResult(true);
            loginWindow.Closing += (_, _) => tcs.TrySetResult(false);

            loginWindow.SetContent(loginView);
            loginWindow.Show();

            var result = await tcs.Task;
            loginWindow.Close();

            return result;
        }
    }

    private void SetupGlobalExceptionHandling()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            var ex = args.ExceptionObject as Exception;
            LogFatalError("AppDomain.UnhandledException", ex);
        };

        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            LogFatalError("TaskScheduler.UnobservedTaskException", args.Exception);
            args.SetObserved();
        };
    }

    private static void LogFatalError(string source, Exception? ex)
    {
        try
        {
            var logger = Services?.GetService<ILogger<App>>();
            logger?.LogCritical(ex, "Unhandled exception from {Source}", source);
        }
        catch
        {
            // Last resort - nothing we can do
        }
    }

    private void SetupTrayIcon(IClassicDesktopStyleApplicationLifetime desktop)
    {
        if (_mainWindow is null) return;

        var trayIcon = new TrayIcon
        {
            Icon = _mainWindow.Icon,
            ToolTipText = "TrustSync",
            IsVisible = true,
            Menu = new NativeMenu
            {
                new NativeMenuItem("Open TrustSync")
                {
                    Command = new CommunityToolkit.Mvvm.Input.RelayCommand(() =>
                    {
                        _mainWindow.Show();
                        _mainWindow.WindowState = WindowState.Normal;
                        _mainWindow.Activate();
                    })
                },
                new NativeMenuItemSeparator(),
                new NativeMenuItem("Quit")
                {
                    Command = new CommunityToolkit.Mvvm.Input.RelayCommand(() =>
                    {
                        _mainWindow.Tag = "quit";
                        desktop.Shutdown();
                    })
                }
            }
        };

        trayIcon.Clicked += (_, _) =>
        {
            _mainWindow.Show();
            _mainWindow.WindowState = WindowState.Normal;
            _mainWindow.Activate();
        };

        // Minimize to tray instead of closing
        _mainWindow.Closing += (_, e) =>
        {
            if (_mainWindow.Tag as string != "quit")
            {
                e.Cancel = true;
                _mainWindow.Hide();
            }
        };
    }

    private async Task LoadThemeAsync()
    {
        try
        {
            var db = Services.GetRequiredService<AppDbContext>();
            var setting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "ThemeMode");
            var mode = setting?.Value ?? "Light";
            ApplyTheme(mode);
        }
        catch { /* default to light */ }
    }

    public static void ApplyTheme(string mode)
    {
        var app = (App)Current!;
        app.RequestedThemeVariant = mode switch
        {
            "Dark" => ThemeVariant.Dark,
            "System" => ThemeVariant.Default,
            _ => ThemeVariant.Light
        };
    }

    private void OnSessionExpired(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(async () =>
        {
            if (_isAuthInProgress) return;
            if (_mainWindow is null) return;
            if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;
            _isAuthInProgress = true;

            // Hide main window
            _mainWindow.Hide();

            // Show full login screen
            var loginVm = Services.GetRequiredService<LoginViewModel>();
            var loginView = new LoginView { DataContext = loginVm };

            var loginWindow = new AuthWindow { Title = "Session Expired — Log In" };
            var tcs = new TaskCompletionSource<bool>();
            loginVm.LoginSucceeded += (_, _) => tcs.TrySetResult(true);
            loginWindow.Closing += (_, e) =>
            {
                if (!tcs.Task.IsCompleted)
                    e.Cancel = true;
            };

            loginWindow.SetContent(loginView);
            loginWindow.Show();

            var result = await tcs.Task;
            loginWindow.Close();

            // Show main window again
            _mainWindow.Show();
            _mainWindow.Activate();
            _isAuthInProgress = false;
        });
    }
}
