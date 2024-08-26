using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using MazeDesktop.ViewModels;
using MazeDesktop.Views;

namespace MazeDesktop;

public partial class App : Application {
  public override void Initialize() {
    AvaloniaXamlLoader.Load(this);
  }

  public override void OnFrameworkInitializationCompleted() {
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
      var mainWindow = new MainWindow();
      mainWindow.DataContext = new MainViewModel(mainWindow);
      desktop.MainWindow = mainWindow;
    } else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform) {
      var mainView = new MainView();
      mainView.DataContext =
          new MainViewModel(null);  // или получаем ссылку на главное окно, если это применимо
      singleViewPlatform.MainView = mainView;
    }

    base.OnFrameworkInitializationCompleted();
  }
}
