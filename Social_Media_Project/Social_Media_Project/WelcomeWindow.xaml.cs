using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Social_Media_Project
{
    public partial class WelcomeWindow : Window
    {
        HttpClient client = new HttpClient();
        BrushConverter bc = new BrushConverter();
        public static int _userID;

        public WelcomeWindow()
        {
            client.BaseAddress = new Uri("https://localhost:7227/api/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            InitializeComponent();
        }

        private void mainWindow_MouseDown(object sender, MouseButtonEventArgs e) 
        {
            bool mouseIsDown = Mouse.LeftButton == MouseButtonState.Pressed;
            if( mouseIsDown ) DragMove();
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            tbIncorrect.Visibility = Visibility.Hidden;
            var response = await client.GetStringAsync("users");
            var people = JsonConvert.DeserializeObject<List<Users>>(response);

            string username = tbUsernameLogin.Text;
            string password = tbPasswordLogin.Text; 

            if(username != null || password != null)
            {
                for(int i = 0; i < people.Count; i++)
                {
                    if (username == people[i].username && password == people[i].password)
                    {
                        getUser(people[i]);
                        
                        tbIncorrect.Visibility = Visibility.Hidden;
                        break;
                    }
                    else tbIncorrect.Visibility = Visibility.Visible;
                }
            }
        }

        private async void getUser(Users user)
        {
            string response = await client.GetStringAsync("users/" + user.id);
            Users person = JsonConvert.DeserializeObject<Users>(response);
            _userID = person.id;

            MainWindow w = new MainWindow();
            this.Close();
            w.Show();
        }

        private async void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            tbIncorrectRegister.Text = "";
            tbIncorrectRegister.Foreground = new SolidColorBrush(Colors.Red);

            var response = await client.GetStringAsync("users");
            List<Users> people = JsonConvert.DeserializeObject<List<Users>>(response);

            if (tbNameRegister.Text != null || tbUsernameRegister.Text != null || tbPasswordRegister.Text != null ||
                tbPasswordReRegister.Text != null || tbEmailRegister.Text != null)
            {
                if (tbPasswordRegister.Text == tbPasswordReRegister.Text)
                {
                    bool personExists = false;
                    for (int i = 0; i < people.Count; i++)
                    {
                        if (people[i].email == tbEmailRegister.Text)
                        {
                            personExists = true;
                            tbIncorrectRegister.Text = "Email is already registered!";
                        }
                        if (people[i].username == tbUsernameRegister.Text)
                        {
                            personExists = true;
                            tbIncorrectRegister.Text = "Username is already taken!";
                        }
                    }

                    if (!personExists)
                    {
                        var user = new Users()
                        {
                            username = tbUsernameRegister.Text,
                            password = tbPasswordRegister.Text,
                            email = tbEmailRegister.Text,
                            name = tbNameRegister.Text,
                            userTag = tbUsernameRegister.Text
                        };
                        tbNameRegister.Text = string.Empty;
                        tbUsernameRegister.Text = string.Empty;
                        tbPasswordRegister.Text = string.Empty;
                        tbPasswordReRegister.Text = string.Empty;
                        tbEmailRegister.Text = string.Empty;

                        this.postUsers(user);

                        tbIncorrectRegister.Foreground = new SolidColorBrush(Colors.Green);
                        tbIncorrectRegister.Text = "Account registered!";
                    }
                }
                else
                {
                    tbIncorrectRegister.Text = "Passwords do not match!";
                }
            }
        }
        private async void postUsers(Users user)
        {
            await client.PostAsJsonAsync("users", user);
        }

        // LOGIN ANIMATION
        private void spLogin_MouseEnter(object sender, MouseEventArgs e)
        {
            if(spLogin.Width == 300 || spLogin.Width == 50) 
            {
                DoubleAnimation ani = new DoubleAnimation(1, 0.7, TimeSpan.FromSeconds(0.1));
                spLogin.BeginAnimation(OpacityProperty, ani);
            }
        }
        private void spLogin_MouseLeave(object sender, MouseEventArgs e)
        {
            DoubleAnimation ani = new DoubleAnimation(spLogin.Opacity, 1, TimeSpan.FromSeconds(0.1));
            spLogin.BeginAnimation(OpacityProperty, ani);
        }
        private void spRegister_MouseEnter(object sender, MouseEventArgs e)
        {
            if(spRegister.Width == 300 || spRegister.Width == 50)
            {
                DoubleAnimation ani = new DoubleAnimation(1, 0.7, TimeSpan.FromSeconds(0.1));
                spRegister.BeginAnimation(OpacityProperty, ani);
            }
        }
        private void spRegister_MouseLeave(object sender, MouseEventArgs e)
        {
            DoubleAnimation ani = new DoubleAnimation(spRegister.Opacity, 1, TimeSpan.FromSeconds(0.1));
            spRegister.BeginAnimation(OpacityProperty, ani);
        }

        private void spRegister_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            spRegister.Width = 550;
            spLogin.Width = 600 - spRegister.Width;

            DoubleAnimation aniRegisterOpacity = new DoubleAnimation(spRegisterPage.Opacity, 1, TimeSpan.FromSeconds(0.5));
            spRegisterPage.BeginAnimation(OpacityProperty, aniRegisterOpacity);

            DoubleAnimation aniLoginOpacity = new DoubleAnimation(spLoginPage.Opacity, 0, TimeSpan.FromSeconds(0.1));
            spLoginPage.BeginAnimation(OpacityProperty, aniLoginOpacity);
            
            tbRegister.Margin = new Thickness(0, 20, 0 ,0);
            tbLogin.Margin = new Thickness(0, 150, 0, 0);

            tbLoginHidden.Visibility = Visibility.Visible;
            tbLogin.Visibility = Visibility.Collapsed;
            tbRegisterHidden.Visibility = Visibility.Collapsed;
            tbRegister.Visibility = Visibility.Visible;

            spRegister.Width = spRegister.Width + 1;
            DoubleAnimation aniOpacity = new DoubleAnimation(spRegister.Opacity, 1, TimeSpan.FromSeconds(0.1));
            spRegister.BeginAnimation(OpacityProperty, aniOpacity);
        }
        private void spLogin_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            spLogin.Width = 550;
            spRegister.Width = 600 - spLogin.Width;

            DoubleAnimation aniRegisterOpacity = new DoubleAnimation(spRegisterPage.Opacity, 0, TimeSpan.FromSeconds(0.1));
            spRegisterPage.BeginAnimation(OpacityProperty, aniRegisterOpacity);

            DoubleAnimation aniLoginOpacity = new DoubleAnimation(spLoginPage.Opacity, 1, TimeSpan.FromSeconds(0.5));
            spLoginPage.BeginAnimation(OpacityProperty, aniLoginOpacity);

            tbRegister.Margin = new Thickness(0, 150, 0, 0);
            tbLogin.Margin = new Thickness(0, 20, 0, 0);

            tbLoginHidden.Visibility = Visibility.Collapsed;
            tbLogin.Visibility = Visibility.Visible;
            tbRegisterHidden.Visibility = Visibility.Visible;
            tbRegister.Visibility = Visibility.Collapsed;

            spLogin.Width = spLogin.Width + 1;
            DoubleAnimation aniOpacity = new DoubleAnimation(spLogin.Opacity, 1, TimeSpan.FromSeconds(0.1));
            spLogin.BeginAnimation(OpacityProperty, aniOpacity);
        }

        // HEADER PANEL AND EXIT BUTTON
        private void spHeader_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            bool mouseIsDown = Mouse.LeftButton == MouseButtonState.Pressed;
            if (mouseIsDown) DragMove();
        }
        private void CloseApplicationFuncion(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Close();
        }
        private void CloseApplicationMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            btnCloseApp.Background = Brushes.Red;
        }
        private void CloseApplicationMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            btnCloseApp.Background = (Brush)bc.ConvertFrom("#FF303030");
        }
    }
}
