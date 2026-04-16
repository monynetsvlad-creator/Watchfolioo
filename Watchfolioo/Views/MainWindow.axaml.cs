namespace Watchfolioo.Views;

using Avalonia.Controls;
using Avalonia.Interactivity;
using System.IO;
using System.Text.Json;
using Watchfolioo.Models;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public void ShowRegister_OnClick(object? sender, RoutedEventArgs e)
    {
        LoginForm.IsVisible = false;
        RegistrationForm.IsVisible = true;
        ErrorMessage.IsVisible = false;
        RegErrorMessage.IsVisible = false;
    }

    public void ShowLogin_OnClick(object? sender, RoutedEventArgs e)
    {
        RegistrationForm.IsVisible = false;
        LoginForm.IsVisible = true;
    }

    public void RegisterAccount_OnClick(object? sender, RoutedEventArgs e)
    {
        string username = RegUser.Text ?? "";
        string password = RegPass.Text ?? "";
        RegErrorMessage.IsVisible = false;

        if (File.Exists("user_data.json"))
        {
            string existingJson = File.ReadAllText("user_data.json");
            var existingUser = JsonSerializer.Deserialize<User>(existingJson);
            if (existingUser != null && existingUser.Username == username)
            {
                RegErrorMessage.IsVisible = true;
                return;
            }
        }

        var newUser = new User { Username = username, Password = password };
        File.WriteAllText("user_data.json", JsonSerializer.Serialize(newUser));

        RegUser.Text = "";
        RegPass.Text = "";
        ShowLogin_OnClick(this, new RoutedEventArgs());
    }

    public void Login_OnClick(object? sender, RoutedEventArgs e)
    {
        if (File.Exists("user_data.json"))
        {
            var savedUser = JsonSerializer.Deserialize<User>(File.ReadAllText("user_data.json"));
            if (savedUser != null && LoginUser.Text == savedUser.Username && LoginPass.Text == savedUser.Password)
            {
               
                var catalog = new CatalogWindow();
                catalog.Show();
                this.Close(); 
            }
            else 
            {
                ErrorMessage.Text = "Невірний логін або пароль";
                ErrorMessage.IsVisible = true;
            }
        }
    }
}