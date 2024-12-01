using SchoolSportsApp.Services;

namespace SchoolSportsApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell(); // Указываем AppShell как главный контейнер
    }
}