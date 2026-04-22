namespace Watchfolioo.Views;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
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

        if (string.IsNullOrWhiteSpace(username))
        {
            RegUser.BorderThickness = new Thickness(2);
            RegUser.BorderBrush = Brushes.Red;
            RegErrorMessage.Text = "Введіть логін";
            RegErrorMessage.IsVisible = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            RegPass.BorderThickness = new Thickness(2);
            RegPass.BorderBrush = Brushes.Red;
            RegErrorMessage.Text = "Введіть пароль";
            RegErrorMessage.IsVisible = true;
            return;
        }

        RegUser.BorderThickness = new Thickness(0);
        RegPass.BorderThickness = new Thickness(0);

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
        
        if (string.IsNullOrWhiteSpace(LoginUser.Text))
        {
            LoginUser.BorderThickness = new Thickness(2);
            LoginUser.BorderBrush = Brushes.Red;
            ErrorMessage.Text = "Введіть логін";
            ErrorMessage.IsVisible = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(LoginPass.Text))
        {
            LoginPass.BorderThickness = new Thickness(2);
            LoginPass.BorderBrush = Brushes.Red;
            ErrorMessage.Text = "Введіть пароль";
            ErrorMessage.IsVisible = true;
            return;
        }

       
        LoginUser.BorderThickness = new Thickness(0);
        LoginPass.BorderThickness = new Thickness(0);

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
                LoginUser.BorderThickness = new Thickness(2);
                LoginUser.BorderBrush = Brushes.Red;
                LoginPass.BorderThickness = new Thickness(2);
                LoginPass.BorderBrush = Brushes.Red;
                ErrorMessage.Text = "Невірний логін або пароль";
                ErrorMessage.IsVisible = true;
            }
        }
        else
        {
            ErrorMessage.Text = "Спочатку зареєструйтесь";
            ErrorMessage.IsVisible = true;
        }
    }
}