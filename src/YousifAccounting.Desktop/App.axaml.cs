using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using YousifAccounting.Application;
using YousifAccounting.Application.Security;
using YousifAccounting.Desktop.Services;
using YousifAccounting.Desktop.ViewModels.Auth;
using YousifAccounting.Desktop.ViewModels.Pages;
using YousifAccounting.Desktop.ViewModels.Shell;
using YousifAccounting.Desktop.Views.Auth;
using YousifAccounting.Infrastructure;

namespace YousifAccounting.Desktop;

public partial class App : Avalonia.Application
{
    private IHost? _host;
    private ISessionService? _sessionService;
    private MainWindow? _mainWindow;

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
            _sessionService.SessionEnded += OnSessionExpired;

            // Show main window
            _mainWindow = Services.GetRequiredService<MainWindow>();
            desktop.MainWindow = _mainWindow;
            desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
            _mainWindow.Show();

            desktop.ShutdownRequested += async (_, _) =>
            {
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

    private void OnSessionExpired(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(async () =>
        {
            if (_mainWindow is null) return;
            if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;

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
        });
    }
}
