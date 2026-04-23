namespace Watchfolioo.Views;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
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

        if (string.IsNullOrWhiteSpace(username) || username.Length < 4 || username.Length > 8)
        {
            RegUser.BorderThickness = new Thickness(2);
            RegUser.BorderBrush = Brushes.Red;
            RegErrorMessage.Text = "Логін має бути від 4 до 8 символів";
            RegErrorMessage.IsVisible = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(password) || password.Length < 4 || password.Length > 8)
        {
            RegPass.BorderThickness = new Thickness(2);
            RegPass.BorderBrush = Brushes.Red;
            RegErrorMessage.Text = "Пароль має бути від 4 до 8 символів";
            RegErrorMessage.IsVisible = true;
            return;
        }

        RegUser.BorderThickness = new Thickness(0);
        RegPass.BorderThickness = new Thickness(0);

        List<User> users = new List<User>();
        if (File.Exists("user_data.json"))
        {
            try
            {
                string json = File.ReadAllText("user_data.json");
                users = JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
            }
            catch { users = new List<User>(); }
        }

        if (users.Any(u => u.Username == username))
        {
            RegErrorMessage.Text = "Користувач вже існує";
            RegErrorMessage.IsVisible = true;
            return;
        }

        users.Add(new User { Username = username, Password = password });
        File.WriteAllText("user_data.json", JsonSerializer.Serialize(users));

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
            List<User> users = new List<User>();
            try
            {
                string json = File.ReadAllText("user_data.json");
                users = JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
            }
            catch { users = new List<User>(); }

            var user = users.FirstOrDefault(u => u.Username == LoginUser.Text && u.Password == LoginPass.Text);

            if (user != null)
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