using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Newtonsoft.Json;
using Social_Media_Project.JsonClass;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Social_Media_Project
{
    public partial class MainWindow : Window
    {
        BrushConverter bc = new BrushConverter();
        HttpClient client = new HttpClient();
        private Users _user;
        private List<Posts> posts;
        public static string imageToSend;
        private byte[] _imageFile;
        private bool startedConversation = false;

        private int toIDMessage = 0;
        private string toNameMessage = "";
        private string toImageMessage = "";

        public MainWindow()
        {
            client.BaseAddress = new Uri("https://localhost:5001/api/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            InitializeComponent();

            getUser(WelcomeWindow._userID);

            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            checkIfNewMessage();
        }
        private async void checkIfNewMessage()
        {
            string response = await client.GetStringAsync("messages");
            if(response != null)
            {
                List<Messages> messages = JsonConvert.DeserializeObject<List<Messages>>(response);

                int counter = 0;
                for (int i = 0; i < messages.Count; i++)
                {
                    if (messages[i].fromID == _user.id && messages[i].toID == toIDMessage || messages[i].fromID == toIDMessage && messages[i].toID == _user.id) counter++;
                }
                if (counter != spChat.Children.OfType<StackPanel>().Count())
                {
                    if(tabMain.SelectedIndex != 1)
                    {
                        string textToAdd = counter - spChat.Children.Count == 1 ? "message" : "messages";
                        btnChats.Content = "CHAT (" + (counter - spChat.Children.Count) + " new " + textToAdd + ")";
                    }

                    populateMessage();
                    svChat.ScrollToEnd();
                }
            }
        }

        #region HEADER FUNCTIONS AND IMAGE SOURCE
        // BASIC FUNCTIONS
        private void spHeader_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            bool mouseIsDown = Mouse.LeftButton == MouseButtonState.Pressed;
            if (mouseIsDown) DragMove();
        }
        private void CloseApplicationFuncion(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
        private void CloseApplicationMouseEnter(object sender, MouseEventArgs e)
        {
            btnCloseApp.Background = Brushes.Red;
        }
        private void CloseApplicationMouseLeave(object sender, MouseEventArgs e)
        {
            btnCloseApp.Background = (Brush)bc.ConvertFrom("#FF303030");
        }
        private void imageSource(Image dynamicImage, string imageURL)
        {
            dynamicImage.Source = new BitmapImage(new Uri(imageURL, UriKind.Absolute), new RequestCachePolicy(RequestCacheLevel.BypassCache)) { CacheOption = BitmapCacheOption.OnLoad };
        }
        #endregion
        #region RELOCATOR
        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            tabMain.SelectedIndex = 0;
            getPosts(_user.id);
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e) 
        { 
            tabMain.SelectedIndex = 2;
            spSearchResult.Children.Clear();
            tbSearch.Text = "";
        }
        private void btnChats_Click(object sender, RoutedEventArgs e)
        {
            btnChats.Content = "CHAT";
            tabMain.SelectedIndex = 1;
            svChat.ScrollToEnd();
            populateMessage();
        }
        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            WelcomeWindow w = new WelcomeWindow();
            this.Close();
            w.Show();
        }
        #endregion

        #region API SEND REQUESTS AND FORM RESULT
        private async void getUser(int id)
        {
            string response = await client.GetStringAsync("users/" + id);
            _user = JsonConvert.DeserializeObject<Users>(response);

            tbFullName.Text = _user.name;
            tbUserTag.Text = "@" + _user.userTag;

            var imagePath = _user.imageURL;
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
            bitmap.EndInit();

            imgBrushProfile.ImageSource = bitmap;

            getPosts(WelcomeWindow._userID);
        } // LEFT PANEL INFO + GET POSTS
        private async void postFollowing(int _userID, int _followingID)
        {
            var response = await client.GetStringAsync("users");
            List<Users> people = JsonConvert.DeserializeObject<List<Users>>(response);
            Users person = null;
            for (int i = 0; i < people.Count; i++)
            {
                if (people[i].id == _followingID) person = people[i];
            }

            var listOfFollowing = await client.GetStringAsync(_user.id + "/following");
            List<Following> followingPerson = JsonConvert.DeserializeObject<List<Following>>(listOfFollowing);
            bool existsInDB = false;

            for (int i = 0; i < followingPerson.Count; i++) if (followingPerson[i].userID == _userID && followingPerson[i].followingID == _followingID) existsInDB = true;

            if (!existsInDB)
            {
                var following = new Following()
                {
                    id = 0,
                    userID = _userID,
                    followingID = _followingID,
                    name = person.name,
                    image = person.imageURL
                };
                await client.PostAsJsonAsync(_userID + "/following", following);
            }
        } // GET USERS AND CHECK IF USER CAN BE ADDED TO FOLLOWING
        private void btnViewProfile_Click(object sender, RoutedEventArgs e)
        {
            viewProfileUpdate();
            getUserInformation(_user.id);
            tabSettings.Visibility = Visibility.Visible;
        }
        private void btnMessagePerson(object sender, MouseButtonEventArgs e)
        {
            startedConversation = true;
            string[] infoToMessage = (sender as Button).Tag.ToString().Split("<divider>");
            populateMessages(Int32.Parse(infoToMessage[0]), infoToMessage[1], infoToMessage[2]);
        }

        private void populateMessages(int v1, string v2, string v3)
        {
            toIDMessage = v1;
            toNameMessage = v2;
            toImageMessage = v3;
            tabMain.SelectedIndex = 1;

            btnDeleteChat.Visibility = Visibility.Visible;
            populateMessage();
            svChat.ScrollToEnd();
        }
        private async void populateMessage()
        {
            spListOfRescentChats.Children.Clear();
            spChat.Children.Clear();
            string response = await client.GetStringAsync("messages");
            List<Messages> messages = JsonConvert.DeserializeObject<List<Messages>>(response);

            tbChattingWith.Text = toNameMessage;
            for (int j = messages.Count - 1; j >= 0; j--)
            {
                if (messages[j].fromID == _user.id || messages[j].toID == _user.id)
                {
                    bool checkIfDuplicate = false;
                    foreach (StackPanel sp in spListOfRescentChats.Children)
                    {
                        string[] infoToMessage = sp.Tag.ToString().Split("<divider>");

                        if (messages[j].fromID == _user.id) { if (infoToMessage[0] == messages[j].toID.ToString()) checkIfDuplicate = true; }
                        else if (messages[j].toID == _user.id) { if (infoToMessage[0] == messages[j].fromID.ToString()) checkIfDuplicate = true; }
                    }

                    if (!checkIfDuplicate)
                    {
                        StackPanel spMainLeft = new StackPanel()
                        {
                            Width = 100,
                            Margin = new Thickness(0, 10, 0, 0),
                            Cursor = Cursors.Hand
                        };
                        spMainLeft.PreviewMouseUp += new MouseButtonEventHandler(spOpenChat);
                        spListOfRescentChats.Children.Add(spMainLeft);
                        if (messages[j].fromID == _user.id) { spMainLeft.Tag = messages[j].toID + "<divider>" + messages[j].toName + "<divider>" + messages[j].toImage; }
                        else if(messages[j].toID == _user.id) spMainLeft.Tag = messages[j].fromID + "<divider>" + messages[j].fromName + "<divider>" + messages[j].fromImage;

                        Ellipse ell = new Ellipse() { HorizontalAlignment = HorizontalAlignment.Left, Width = 70, Height = 70, VerticalAlignment = VerticalAlignment.Center };
                        spMainLeft.Children.Add(ell);

                        ImageBrush dynamicImageLeft = new ImageBrush();
                        if (messages[j].fromID == _user.id) dynamicImageLeft.ImageSource = new BitmapImage(new Uri(messages[j].toImage, UriKind.Absolute), new RequestCachePolicy(RequestCacheLevel.BypassCache)) { CacheOption = BitmapCacheOption.OnLoad };
                        else if (messages[j].toID == _user.id) dynamicImageLeft.ImageSource = new BitmapImage(new Uri(messages[j].fromImage, UriKind.Absolute), new RequestCachePolicy(RequestCacheLevel.BypassCache)) { CacheOption = BitmapCacheOption.OnLoad };
                        ell.Fill = dynamicImageLeft;
                    }
                }
            }

            if (startedConversation)
            {
                DateTime lastDate = new DateTime();
                for (int i = 0; i < messages.Count; i++)
                {
                    TimeSpan span = messages[i].date.Subtract(lastDate);
                    if (span.TotalMinutes >= 90)
                    {
                        TextBlock tb = new TextBlock()
                        {
                            Width = 700,
                            Text = messages[i].date.ToString("MM/dd/yyyy hh:mm tt"),
                            Padding = new Thickness(15, 5, 15, 0),
                            TextAlignment = TextAlignment.Center,
                            Margin = new Thickness(0, 10, 0, 20),
                            Foreground = Brushes.Gray,
                            FontSize = 18
                        };
                        spChat.Children.Add(tb);
                    }
                    lastDate = messages[i].date;

                    if (messages[i].fromID == _user.id)
                    {
                        StackPanel spMain = new StackPanel()
                        {
                            Margin = new Thickness(0, 7, 10, 2),
                            Width = 700,
                        };
                        spChat.Children.Add(spMain);

                        StackPanel spContent = new StackPanel()
                        {
                            Tag = _user.imageURL + "<divider>" + _user.id,
                            Orientation = Orientation.Horizontal,
                            Width = 500,
                            HorizontalAlignment = HorizontalAlignment.Right,
                        };
                        spMain.Children.Add(spContent);

                        Border b = new Border()
                        {
                            CornerRadius = new CornerRadius(10),
                            Background = (Brush)bc.ConvertFrom("#FF1F1F1F"),
                            HorizontalAlignment = HorizontalAlignment.Right,
                        };
                        spContent.Children.Add(b);
                        TextBlock tb = new TextBlock()
                        {
                            Width = 400,
                            TextWrapping = TextWrapping.Wrap,
                            Text = messages[i].content,
                            Padding = new Thickness(15, 5, 15, 0),
                            FontSize = 18
                        };
                        b.Child = tb;

                        Ellipse ell = new Ellipse() { Width = 50, Height = 50, VerticalAlignment = VerticalAlignment.Bottom };
                        spContent.Children.Add(ell);

                        ImageBrush dynamicImageLeft = new ImageBrush();
                        dynamicImageLeft.ImageSource = new BitmapImage(new Uri(_user.imageURL, UriKind.Absolute), new RequestCachePolicy(RequestCacheLevel.BypassCache)) { CacheOption = BitmapCacheOption.OnLoad };
                        ell.Fill = dynamicImageLeft;

                    }
                    else if (messages[i].fromID == toIDMessage && messages[i].toID == _user.id)
                    {
                        StackPanel spMain = new StackPanel()
                        {
                            Margin = new Thickness(10, 7, 0, 2),
                            Width = 700,
                        };
                        spChat.Children.Add(spMain);

                        StackPanel spContent = new StackPanel()
                        {
                            Tag = toImageMessage + "<divider>" + messages[i].fromID,
                            Orientation = Orientation.Horizontal,
                            Width = 500,
                            HorizontalAlignment = HorizontalAlignment.Left,
                        };
                        spMain.Children.Add(spContent);

                        Ellipse ell = new Ellipse() { Width = 50, Height = 50, VerticalAlignment = VerticalAlignment.Bottom };
                        spContent.Children.Add(ell);

                        ImageBrush dynamicImageLeft = new ImageBrush();
                        dynamicImageLeft.ImageSource = new BitmapImage(new Uri(toImageMessage, UriKind.Absolute), new RequestCachePolicy(RequestCacheLevel.BypassCache)) { CacheOption = BitmapCacheOption.OnLoad };
                        ell.Fill = dynamicImageLeft;

                        Border b = new Border()
                        {
                            CornerRadius = new CornerRadius(10),
                            Background = (Brush)bc.ConvertFrom("#FF1F1F1F"),
                            HorizontalAlignment = HorizontalAlignment.Left,
                        };
                        spContent.Children.Add(b);
                        TextBlock tb = new TextBlock()
                        {
                            Width = 400,
                            TextWrapping = TextWrapping.Wrap,
                            Text = messages[i].content,
                            Padding = new Thickness(15, 5, 15, 0),
                            FontSize = 18
                        };
                        b.Child = tb;
                    }
                }
            }
        }

        private void spOpenChat(object sender, MouseButtonEventArgs e)
        {
            startedConversation = true;
            string[] infoToMessage = (sender as StackPanel).Tag.ToString().Split("<divider>");
            populateMessages(Int32.Parse(infoToMessage[0]), infoToMessage[1], infoToMessage[2]);
        }
        #endregion

        #region SEARCH
        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbSearch.Text.Length > 0) getUsersInDB(tbSearch.Text);
            else spSearchResult.Children.Clear();
        }
        private async void getUsersInDB(string username)
        {
            username = username.ToLower();
            spSearchResult.Children.Clear();

            string response = await client.GetStringAsync("users");
            List<Users> userFromList = JsonConvert.DeserializeObject<List<Users>>(response);

            string responseFollowing = await client.GetStringAsync(_user.id + "/following");
            List<Following> followingList = JsonConvert.DeserializeObject<List<Following>>(responseFollowing);

            for (int i = 0; i < userFromList.Count; i++)
            {
                if ((userFromList[i].name.ToLower().Contains(username) || userFromList[i].userTag.ToLower().Contains(username)) && userFromList[i].id != _user.id && userFromList[i].privateAccount == false)
                {
                    StackPanel spHeader = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(30, 5, 30, 0),
                        Background = (Brush)bc.ConvertFrom("#FF191919")
                    };
                    spSearchResult.Children.Add(spHeader);

                    Image dynamicImage = new Image
                    {
                        Width = 50,
                        Height = 50
                    };
                    imageSource(dynamicImage, userFromList[i].imageURL);
                    spHeader.Children.Add(dynamicImage);

                    TextBlock tbTitle = new TextBlock
                    {
                        Text = userFromList[i].name + " (@" + userFromList[i].userTag + ")",
                        Width = 555,
                        FontSize = 26,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(20, 0, 0, 0)
                    };
                    spHeader.Children.Add(tbTitle);

                    Button btnFollow = new Button
                    {
                        Tag = userFromList[i].id,
                        Width = 50,
                        Height = 50,
                        Background = (Brush)bc.ConvertFrom("#FF191919"),
                        Margin = new Thickness(0, 0, 10, 0)
                    };
                    spHeader.Children.Add(btnFollow);
                    btnFollow.PreviewMouseUp += new MouseButtonEventHandler(btnFollowPerson);

                    PackIcon icon = new PackIcon();
                    icon.Kind = PackIconKind.Add;
                    icon.Foreground = new SolidColorBrush(Colors.White);
                    icon.Height = 30;
                    icon.Width = 30;
                    icon.HorizontalAlignment = HorizontalAlignment.Center;
                    icon.VerticalAlignment = VerticalAlignment.Center;
                    btnFollow.Content = icon;

                    for (int j = 0; j < followingList.Count; j++)
                    {
                        if (followingList[j].followingID == userFromList[i].id) btnFollow.Visibility = Visibility.Hidden;
                    }

                    Button btnMessage = new Button
                    {
                        Tag = userFromList[i].id + "<divider>" + userFromList[i].name + "<divider>" + userFromList[i].imageURL,
                        Width = 50,
                        Height = 50,
                        Background = (Brush)bc.ConvertFrom("#FF191919")
                    };
                    spHeader.Children.Add(btnMessage);
                    btnMessage.PreviewMouseUp += new MouseButtonEventHandler(btnMessagePerson);

                    PackIcon icon2 = new PackIcon();
                    icon2.Kind = PackIconKind.Message;
                    icon2.Foreground = new SolidColorBrush(Colors.White);
                    icon2.Height = 20;
                    icon2.Width = 20;
                    icon2.HorizontalAlignment = HorizontalAlignment.Center;
                    icon2.VerticalAlignment = VerticalAlignment.Center;
                    btnMessage.Content = icon2;
                }
            }
        } // SEARCH DB FOR USERS THAT HAVE "TEXT" IN THE NAME OR TAG

        #endregion
        #region POSTS
        private void btnNewPost_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbPostContent.Text))
            {
                tbPostImageURL.IsEnabled = true;
                if (_imageFile.Length != 0)
                {
                    var person = new Posts()
                    {
                        id = 0,
                        userID = _user.id,
                        name = _user.name,
                        tag = _user.userTag,
                        userImage = _user.imageURL,
                        imageURL = string.Empty,
                        imageFile = _imageFile,
                        text = tbPostContent.Text,
                        date = DateTime.Now,
                    };
                    this.postPost(person, _user.id);
                }
                else
                {
                    var person = new Posts()
                    {
                        id = 0,
                        userID = _user.id,
                        name = _user.name,
                        tag = _user.userTag,
                        userImage = _user.imageURL,
                        imageURL = tbPostImageURL.Text,
                        imageFile = Array.Empty<byte>(),
                        text = tbPostContent.Text,
                        date = DateTime.Now,
                    };
                    this.postPost(person, _user.id);
                }
                tbPostContent.Text = string.Empty;
                tbPostImageURL.Text = string.Empty;
            }
        }
        private async void postPost(Posts post, int id)
        {
            await client.PostAsJsonAsync(id + "/posts", post);
            getPosts(_user.id);
        }
        private async void getPosts(int id)
        {
            spPosts.Children.Clear();
            string response = await client.GetStringAsync("posts");
            posts = JsonConvert.DeserializeObject<List<Posts>>(response);

            string responseFollowing = await client.GetStringAsync(id + "/following");
            List<Following> followingList = JsonConvert.DeserializeObject<List<Following>>(responseFollowing);
            
            for (int i = posts.Count - 1; i >= 0; i--)
            {
                for (int j = 0; j < followingList.Count; j++)
                {
                    if (posts[i].userID == followingList[j].followingID || posts[i].userID == _user.id)
                    {
                        StackPanel sBorder = new StackPanel
                        {
                            Margin = new Thickness(10, 0, 30, 10),
                        };
                        sBorder.SetResourceReference(StackPanel.BackgroundProperty, "PrimaryHueMidBrush");
                        spPosts.Children.Add(sBorder);

                        StackPanel spMain = new StackPanel()
                        {
                            Margin = new Thickness(3),
                            Background = (Brush)bc.ConvertFrom("#FF202020")
                        };
                        sBorder.Children.Add(spMain);

                        StackPanel spHeader = new StackPanel
                        {
                            Width = 690,
                            Orientation = Orientation.Horizontal,
                            Margin = new Thickness(0, 10, 0, 0),
                            Background = (Brush)bc.ConvertFrom("#FF252525")
                        };
                        spMain.Children.Add(spHeader);

                        Ellipse ell = new Ellipse()
                        {
                            Tag = posts[i].userID,
                            Cursor = Cursors.Hand,
                            HorizontalAlignment = HorizontalAlignment.Left, 
                            Width = 50, Height = 50,
                            VerticalAlignment = VerticalAlignment.Center 
                        };
                        ell.PreviewMouseUp += new MouseButtonEventHandler(imageProfilePerson);
                        spHeader.Children.Add(ell);

                        ImageBrush dynamicImageLeft = new ImageBrush();
                        dynamicImageLeft.ImageSource = new BitmapImage(new Uri(posts[i].userImage, UriKind.Absolute), new RequestCachePolicy(RequestCacheLevel.BypassCache)) { CacheOption = BitmapCacheOption.OnLoad };
                        ell.Fill = dynamicImageLeft;

                        TextBlock tbTitle = new TextBlock
                        {
                            Text = posts[i].name + " (@" + posts[i].tag + ")",
                            Cursor = Cursors.Hand,
                            Tag = posts[i].userID,
                            FontSize = 26,
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(20, 0, 0, 0)
                        };
                        tbTitle.PreviewMouseUp += new MouseButtonEventHandler(textProfilePerson);
                        spHeader.Children.Add(tbTitle);

                        if (!string.IsNullOrEmpty(posts[i].text))
                        {
                            TextBlock tbContent = new TextBlock
                            {
                                Text = posts[i].text,
                                TextWrapping = TextWrapping.Wrap,
                                FontSize = 20,
                                Padding = new Thickness(10, 20, 0, 20)
                            };
                            spMain.Children.Add(tbContent);
                        }
                        if (!string.IsNullOrEmpty(posts[i].imageURL))
                        {
                            Image dynamicImageContent = new Image
                            {
                                Tag = posts[i].imageURL,
                                Width = 715,
                            };
                            imageSource(dynamicImageContent, posts[i].imageURL);
                            spMain.Children.Add(dynamicImageContent);
                            dynamicImageContent.PreviewMouseUp += new MouseButtonEventHandler(showPictureFunction);
                        }
                        else if (posts[i].imageFile.Length != 0)
                        {
                            var image = new BitmapImage();
                            using (var mem = new MemoryStream(posts[i].imageFile))
                            {
                                mem.Position = 0;
                                image.BeginInit();
                                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                                image.CacheOption = BitmapCacheOption.OnLoad;
                                image.UriSource = null;
                                image.StreamSource = mem;
                                image.EndInit();
                            }
                            image.Freeze();

                            Image dynamicImageContent = new Image
                            {
                                Tag = posts[i].imageFile,
                                Width = 715,
                            };
                            dynamicImageContent.Source = image;

                            spMain.Children.Add(dynamicImageContent);
                            dynamicImageContent.PreviewMouseUp += new MouseButtonEventHandler(showPictureFileFunction);
                        }
                        break;
                    }
                }
            }
        }

        private void showPictureFileFunction(object sender, MouseButtonEventArgs e)
        {
            ImageWindow w = new ImageWindow((byte[])(sender as Image).Tag);
            w.Show();
            w.Left = this.Left;
            w.Top = this.Top;
        }
        private void showPictureFunction(object sender, MouseButtonEventArgs e)
        {
            imageToSend = (sender as Image).Tag.ToString();
            ImageWindow w = new ImageWindow(imageToSend);
            w.Show();
            w.Left = this.Left;
            w.Top = this.Top;
        }

        private void btnUploadPicture_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                _imageFile = File.ReadAllBytes(openFileDialog.FileName);
                tbPostImageURL.IsEnabled = false;
                tbPostImageURL.Text = "< Uploaded Image >";
            }
        }
        private void btnResetPost_Click(object sender, RoutedEventArgs e)
        {
            tbPostContent.Text = string.Empty;
            tbPostImageURL.Text = string.Empty;
            tbPostImageURL.IsEnabled = true;
            _imageFile = Array.Empty<byte>();
        }
        #endregion
        #region PROFILE
        private void viewProfileUpdate()
        {
            tabMain.SelectedIndex = 3;
            tabMainProfile.SelectedIndex = 0;

            if (!string.IsNullOrEmpty(_user.imageURL))
            {
                var imagePath = _user.imageURL;

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.EndInit();
                imgUserImage.Source = bitmap;
            }

            if (!string.IsNullOrEmpty(_user.imageURLBG))
            {
                var imagePath = _user.imageURLBG;

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.EndInit();
                imgBgImage.Source = bitmap;
            }
            else imgBgImage.Source = null;

            tbProfileName.Text = _user.name;

            tbAboutBio.Text = "About: " + _user.bio + "\n";
            tbAboutLives.Text = "Lives in: " + _user.livesIn;
            tbAboutRelationship.Text = "Relationship: " + _user.relationship;
            tbAboutJoined.Text = "Joined: " + _user.joined.ToString("dd/MM/yyyy") + "\n\n\n";

            setupProfilePosts(_user.id);
        }

        //      POSTS
        private async void setupProfilePosts(int id)
        {
            spProfilePosts.Children.Clear();
            string response = await client.GetStringAsync(id + "/posts");
            posts = JsonConvert.DeserializeObject<List<Posts>>(response);

            for (int i = posts.Count - 1; i >= 0; i--)
            {
                StackPanel spMain = new StackPanel
                {
                    Margin = new Thickness(30, 10, 30, 0),
                    Background = (Brush)bc.ConvertFrom("#FF191919")
                };
                spProfilePosts.Children.Add(spMain);

                StackPanel spHeader = new StackPanel
                {
                    Orientation = Orientation.Horizontal
                };
                spMain.Children.Add(spHeader);

                Image dynamicImage = new Image
                {
                    Width = 50,
                    Height = 50,
                    Margin = new Thickness(20, 0, 0, 0),
                };
                imageSource(dynamicImage, posts[i].userImage);
                spHeader.Children.Add(dynamicImage);

                TextBlock tbTitle = new TextBlock
                {
                    Text = posts[i].name,
                    Width = 580,
                    FontSize = 26,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(20, 0, 0, 0)
                };
                spHeader.Children.Add(tbTitle);

                if(posts[i].userID == _user.id)
                {
                    Button btnDelete = new Button
                    {
                        Tag = posts[i].id,
                        Width = 50,
                        Height = 50,
                        Background = (Brush)bc.ConvertFrom("#FF191919")
                    };
                    spHeader.Children.Add(btnDelete);
                    btnDelete.PreviewMouseUp += new MouseButtonEventHandler(btnDeletePost);

                    PackIcon icon = new PackIcon();
                    icon.Kind = PackIconKind.Garbage;
                    icon.Foreground = new SolidColorBrush(Colors.White);
                    icon.Height = 30;
                    icon.Width = 30;
                    icon.HorizontalAlignment = HorizontalAlignment.Center;
                    icon.VerticalAlignment = VerticalAlignment.Center;
                    btnDelete.Content = icon;
                }

                StackPanel spContents = new StackPanel
                {
                    Margin = new Thickness(0, 20, 0, 0)
                };
                spMain.Children.Add(spContents);

                if (!string.IsNullOrEmpty(posts[i].text))
                {
                    TextBlock tbContent = new TextBlock
                    {
                        Text = posts[i].text,
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 20,
                        Padding = new Thickness(10, 20, 0, 20)
                    };
                    spContents.Children.Add(tbContent);
                }
                if (!string.IsNullOrEmpty(posts[i].imageURL))
                {
                    Image dynamicImageContent = new Image
                    {
                        Tag = posts[i].imageURL,
                        Width = 715,
                    };
                    imageSource(dynamicImageContent, posts[i].imageURL);
                    spContents.Children.Add(dynamicImageContent);
                    dynamicImageContent.PreviewMouseUp += new MouseButtonEventHandler(showPictureFunction);
                }
                else if (posts[i].imageFile.Length != 0)
                {
                    var image = new BitmapImage();
                    using (var mem = new MemoryStream(posts[i].imageFile))
                    {
                        mem.Position = 0;
                        image.BeginInit();
                        image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.UriSource = null;
                        image.StreamSource = mem;
                        image.EndInit();
                    }
                    image.Freeze();

                    Image dynamicImageContent = new Image
                    {
                        Tag = posts[i].imageFile,
                        Width = 715,
                    };
                    dynamicImageContent.Source = image;

                    spMain.Children.Add(dynamicImageContent);
                    dynamicImageContent.PreviewMouseUp += new MouseButtonEventHandler(showPictureFileFunction);
                }
            }
        }
        private void btnDeletePost(object sender, MouseButtonEventArgs e)
        {
            deletePostFunction((sender as Button).Tag);
        }
        private async void deletePostFunction(object tag)
        {
            await client.DeleteAsync(tag + "/posts");
            spPosts.Children.Clear();
            getPosts(_user.id);
            setupProfilePosts(_user.id);
        }

        //      SETTINGS
        private void tabSettings_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (tabMainProfile.SelectedIndex != 2)
            {
                tabMainProfile.SelectedIndex = 2;
                getUserInformation(_user.id);
                tbSettingsChangesSaved.Visibility = Visibility.Hidden;
                tbSettingsChangesError.Visibility = Visibility.Collapsed;
            }
        }
        private async void getUserInformation(int inspectProfileID)
        {
            Users currentUser = JsonConvert.DeserializeObject<Users>(await client.GetStringAsync("users/" + inspectProfileID));

            tbSettingsName.Text = currentUser.name;
            if (currentUser.imageURL != "https://i.imgur.com/Gtjeuu5.png") tbSettingsProfileImage.Text = currentUser.imageURL;
            else tbSettingsProfileImage.Text = "";

            tbSettingsBackgroundImage.Text = currentUser.imageURLBG;
            tbSettingsBio.Text = currentUser.bio;
            tbSettingsLivesIn.Text = currentUser.livesIn;

            if (currentUser.relationship.Equals("")) cbRelationship.SelectedIndex = -1;
            else if (currentUser.relationship.Equals("Single")) cbRelationship.SelectedIndex = 0;
            else cbRelationship.SelectedIndex = 1;

            if(currentUser.privateAccount) cbPrivateAccount.SelectedIndex = 0;
            else cbPrivateAccount.SelectedIndex = 1;
        }
        private void btnSettingsSaveChanges_Click(object sender, RoutedEventArgs e)
        {
            tbSettingsChangesError.Visibility = Visibility.Collapsed;
            tbSettingsChangesSaved.Visibility = Visibility.Collapsed;

            string password;
            string image;
            bool privateAcc;

            if (tbSettingsOldPassword.Password == _user.password && tbSettingsNewPassword.Password == tbSettingsNewRePassword.Password)
            {
                password = tbSettingsNewRePassword.Password;
                tbSettingsChangesError.Visibility = Visibility.Collapsed;
                tbSettingsChangesSaved.Visibility = Visibility.Visible;
            }
            else if (string.IsNullOrEmpty(tbSettingsOldPassword.Password) && string.IsNullOrEmpty(tbSettingsNewPassword.Password) && string.IsNullOrEmpty(tbSettingsNewRePassword.Password))
            {
                password = _user.password;
                tbSettingsChangesError.Visibility = Visibility.Collapsed;
                tbSettingsChangesSaved.Visibility = Visibility.Visible;
            }
            else
            {
                tbSettingsChangesError.Visibility = Visibility.Visible;
                tbSettingsChangesSaved.Visibility = Visibility.Collapsed;
                tbSettingsChangesError.Text = "Wrong password or passwords do not match!";
                password = _user.password;
            }

            if (tbSettingsProfileImage.Text != "https://i.imgur.com/Gtjeuu5.png" && !string.IsNullOrEmpty(tbSettingsProfileImage.Text)) image = tbSettingsProfileImage.Text;
            else image = "https://i.imgur.com/Gtjeuu5.png";

            privateAcc = cbPrivateAccount.SelectedIndex == 0 ? true : false;

            imgUserImage.Source = new BitmapImage(new Uri(image, UriKind.Absolute), new RequestCachePolicy(RequestCacheLevel.BypassCache)) { CacheOption = BitmapCacheOption.OnLoad };
            var userEdit = new Users()
            {
                id = _user.id,
                username = _user.username,
                password = password,
                email = _user.email,
                name = tbSettingsName.Text != null ? tbSettingsName.Text : _user.name,
                userTag = _user.userTag,
                imageURL = image,
                imageURLBG = tbSettingsBackgroundImage.Text != null ? tbSettingsBackgroundImage.Text : _user.imageURLBG,
                bio = tbSettingsBio.Text != null ? tbSettingsBio.Text : _user.bio,
                livesIn = tbSettingsLivesIn.Text != null ? tbSettingsLivesIn.Text : _user.livesIn,
                relationship = cbRelationship.Text != null ? cbRelationship.Text : _user.relationship,
                joined = _user.joined,
                privateAccount = privateAcc,
                dnd = false
            };

            updateUserProfile(userEdit);
            setupProfilePosts(_user.id);
        }
        private void themePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string color = "";

            if (themePicker.SelectedIndex == 0) { color = "Purple"; }
            else if (themePicker.SelectedIndex == 1) { color = "Indigo"; }
            else if (themePicker.SelectedIndex == 2) { color = "Blue"; }
            else if (themePicker.SelectedIndex == 3) { color = "LightBlue"; }
            else if (themePicker.SelectedIndex == 4) { color = "Cyan"; }
            else if (themePicker.SelectedIndex == 5) { color = "Teal"; }
            else if (themePicker.SelectedIndex == 6) { color = "Green"; }
            else if (themePicker.SelectedIndex == 7) { color = "LightGreen"; }
            else if (themePicker.SelectedIndex == 8) { color = "Red"; }
            else if (themePicker.SelectedIndex == 9) { color = "Pink"; }
            else if (themePicker.SelectedIndex == 10) { color = "Brown"; }
            else if (themePicker.SelectedIndex == 11) { color = "Orange"; }
            else if (themePicker.SelectedIndex == 12) { color = "Yellow"; }
            else if (themePicker.SelectedIndex == 13) { color = "Lime"; }

            if (themePicker.SelectedIndex >= 0 && themePicker.SelectedIndex <= 13)
            {
                Uri uri = new Uri($"pack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Primary/MaterialDesignColor." + color + ".xaml");
                Application.Current.Resources.MergedDictionaries.RemoveAt(2);
                Application.Current.Resources.MergedDictionaries.Insert(2, new ResourceDictionary() { Source = uri });

                Properties.Settings.Default.color = color;
                Properties.Settings.Default.Save();
            }
        }

        private async void updateUserProfile(Users userEdit)
        {
            await client.PutAsJsonAsync("users/" + _user.id, userEdit);
            getUser(_user.id);
            changeImageOnFollowing(userEdit.id, userEdit.imageURL);
        }
        private async void changeImageOnFollowing(int id, string imageURL)
        {
            var listOfFollowing = await client.GetStringAsync("following");
            List<Following> followingPerson = JsonConvert.DeserializeObject<List<Following>>(listOfFollowing);

            for(int i = 0; i < followingPerson.Count; i++)
            {
                if(followingPerson[i].followingID == id)
                {
                    var following = new Following()
                    {
                        id = followingPerson[i].id,
                        userID = followingPerson[i].userID,
                        followingID = followingPerson[i].followingID,
                        name = followingPerson[i].name,
                        image = imageURL
                    };
                    await client.PutAsJsonAsync("following", following);
                }
            }

            string response = await client.GetStringAsync("posts");
            List<Posts> imageUpdatePosts = JsonConvert.DeserializeObject<List<Posts>>(response);
            for (int i = 0; i < imageUpdatePosts.Count; i++)
            {
                if (imageUpdatePosts[i].userID == _user.id)
                {
                    var userPostsImageUpdate = new Posts()
                    {
                        id = imageUpdatePosts[i].id,
                        userID = imageUpdatePosts[i].userID,
                        name = imageUpdatePosts[i].name,
                        tag = imageUpdatePosts[i].tag,
                        userImage = imageURL,
                        imageURL = imageUpdatePosts[i].imageURL,
                        text = imageUpdatePosts[i].text,
                        date = imageUpdatePosts[i].date
                    };
                    await client.PutAsJsonAsync("posts", userPostsImageUpdate);
                }
            }

            string response2 = await client.GetStringAsync("messages");
            List<Messages> imageUpdateMessages = JsonConvert.DeserializeObject<List<Messages>>(response2);
            for (int i = 0; i < imageUpdateMessages.Count; i++)
            {
                if (imageUpdateMessages[i].fromID == _user.id)
                {
                    var userMessageImageUpdate = new Messages()
                    {
                        id = imageUpdateMessages[i].id,
                        fromID = imageUpdateMessages[i].fromID,
                        toID = imageUpdateMessages[i].toID,
                        toName = imageUpdateMessages[i].toName,
                        toImage = imageUpdateMessages[i].toImage,
                        fromName = imageUpdateMessages[i].fromName,
                        fromImage = imageURL,
                        content = imageUpdateMessages[i].content,
                        date = imageUpdateMessages[i].date
                    };
                    await client.PutAsJsonAsync("messages", userMessageImageUpdate);
                }
                else if (imageUpdateMessages[i].toID == _user.id)
                {
                    var userMessageImageUpdate = new Messages()
                    {
                        id = imageUpdateMessages[i].id,
                        fromID = imageUpdateMessages[i].fromID,
                        toID = imageUpdateMessages[i].toID,
                        toName = imageUpdateMessages[i].toName,
                        toImage = imageURL,
                        fromName = imageUpdateMessages[i].fromName,
                        fromImage = imageUpdateMessages[i].fromImage,
                        content = imageUpdateMessages[i].content,
                        date = imageUpdateMessages[i].date
                    };
                    await client.PutAsJsonAsync("messages", userMessageImageUpdate);
                }
            }
        }
        #endregion
        #region FOLLOWING
        private void btnFollowPerson(object sender, MouseButtonEventArgs e)
        {
            this.postFollowing(_user.id, Int32.Parse((sender as Button).Tag.ToString()));
            (sender as Button).Visibility = Visibility.Hidden;
        }
        private void btnFollowing_Click(object sender, RoutedEventArgs e)
        {
            tabMain.SelectedIndex = 4;
            spFollowingList.Children.Clear();
            populateFollowingList(_user.id);
        }

        private async void populateFollowingList(int _userID)
        {
            spFollowingList.Children.Clear();
            string response = await client.GetStringAsync(_userID + "/following");
            List<Following> followingList = JsonConvert.DeserializeObject<List<Following>>(response);

            for (int i = 0; i < followingList.Count; i++)
            {
                StackPanel spHeader = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(30, 5, 30, 0),
                    Background = (Brush)bc.ConvertFrom("#FF191919")
                };
                spFollowingList.Children.Add(spHeader);

                Image dynamicImage = new Image
                {
                    Width = 50,
                    Height = 50
                };
                imageSource(dynamicImage, followingList[i].image);
                spHeader.Children.Add(dynamicImage);

                TextBlock tbTitle = new TextBlock
                {
                    Text = followingList[i].name,
                    Width = 480,
                    FontSize = 26,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(20, 0, 0, 0)
                };
                spHeader.Children.Add(tbTitle);

                Button btnUnfollow = new Button
                {
                    Tag = followingList[i].id,
                    Width = 50,
                    Height = 50,
                    Background = (Brush)bc.ConvertFrom("#FF191919"),
                    Margin = new Thickness(0, 0, 10, 0)
                };
                spHeader.Children.Add(btnUnfollow);
                btnUnfollow.PreviewMouseUp += new MouseButtonEventHandler(btnUnfollowPerson);

                PackIcon icon = new PackIcon();
                icon.Kind = PackIconKind.Minus;
                icon.Foreground = new SolidColorBrush(Colors.White);
                icon.Height = 30;
                icon.Width = 30;
                icon.HorizontalAlignment = HorizontalAlignment.Center;
                icon.VerticalAlignment = VerticalAlignment.Center;
                btnUnfollow.Content = icon;

                Button btnProfile = new Button
                {
                    Tag = followingList[i].followingID,
                    Width = 50,
                    Height = 50,
                    Background = (Brush)bc.ConvertFrom("#FF191919"),
                    Margin = new Thickness(0, 0, 10, 0)
                };
                spHeader.Children.Add(btnProfile);
                btnProfile.PreviewMouseUp += new MouseButtonEventHandler(btnProfilePerson);

                PackIcon iconProfile = new PackIcon();
                iconProfile.Kind = PackIconKind.Person;
                iconProfile.Foreground = new SolidColorBrush(Colors.White);
                iconProfile.Height = 30;
                iconProfile.Width = 30;
                iconProfile.HorizontalAlignment = HorizontalAlignment.Center;
                iconProfile.VerticalAlignment = VerticalAlignment.Center;
                btnProfile.Content = iconProfile;

                Button btnMessage = new Button
                {
                    Tag = followingList[i].followingID + "<divider>" + followingList[i].name + "<divider>" + followingList[i].image,
                    Width = 50,
                    Height = 50,
                    Background = (Brush)bc.ConvertFrom("#FF191919")
                };
                spHeader.Children.Add(btnMessage);
                btnMessage.PreviewMouseUp += new MouseButtonEventHandler(btnMessagePerson);

                PackIcon icon2 = new PackIcon();
                icon2.Kind = PackIconKind.Message;
                icon2.Foreground = new SolidColorBrush(Colors.White);
                icon2.Height = 20;
                icon2.Width = 20;
                icon2.HorizontalAlignment = HorizontalAlignment.Center;
                icon2.VerticalAlignment = VerticalAlignment.Center;
                btnMessage.Content = icon2;
            }
        }

        private void btnProfilePerson(object sender, MouseButtonEventArgs e)
        {
            viewProfileUpdate((sender as Button).Tag.ToString());
        }
        private void textProfilePerson(object sender, MouseButtonEventArgs e)
        {
            viewProfileUpdate((sender as TextBlock).Tag.ToString());
        }
        private void imageProfilePerson(object sender, MouseButtonEventArgs e)
        {
            viewProfileUpdate((sender as Image).Tag.ToString());
        }
        private async void viewProfileUpdate(string ID)
        {
            if(_user.id.ToString() != ID) tabSettings.Visibility = Visibility.Collapsed;
            string response = await client.GetStringAsync("users/" + ID);
            Users viewProfileUser = JsonConvert.DeserializeObject<Users>(response);

            tabMain.SelectedIndex = 3;
            tabMainProfile.SelectedIndex = 0;

            if (!string.IsNullOrEmpty(viewProfileUser.imageURL))
            {
                var imagePath = viewProfileUser.imageURL;

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.EndInit();
                imgUserImage.Source = bitmap;
            }

            if (!string.IsNullOrEmpty(viewProfileUser.imageURLBG))
            {
                var imagePath = viewProfileUser.imageURLBG;

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.EndInit();
                imgBgImage.Source = bitmap;
            }
            else imgBgImage.Source = null;

            tbProfileName.Text = viewProfileUser.name;

            tbAboutBio.Text = "About: " + viewProfileUser.bio + "\n";
            tbAboutLives.Text = "Lives in: " + viewProfileUser.livesIn;
            tbAboutRelationship.Text = "Relationship: " + viewProfileUser.relationship;
            tbAboutJoined.Text = "Joined: " + viewProfileUser.joined.ToString("dd/MM/yyyy") + "\n\n\n";

            setupProfilePosts(viewProfileUser.id);
        }

        private async void btnUnfollowPerson(object sender, MouseButtonEventArgs e)
        {
            await client.DeleteAsync(Int32.Parse((sender as Button).Tag.ToString()) + "/following");
            populateFollowingList(_user.id);
        }
        #endregion
        #region MESSAGES

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if(toIDMessage != 0)
                {
                    Messages message = new Messages()
                    {
                        id = 0,
                        fromID = _user.id,
                        toID = toIDMessage,
                        content = tbChatMessage.Text,
                        date = DateTime.Now,
                        toName = toNameMessage,
                        toImage = toImageMessage,
                        fromName = _user.name,
                        fromImage = _user.imageURL
                    };
                    postNewPrivateMessage(message);
                    tbChatMessage.Text = string.Empty;
                }
            }
        }
        private async void postNewPrivateMessage(Messages message)
        {
            await client.PostAsJsonAsync(_user.id + "/messages", message);
            populateMessage();
            svChat.ScrollToEnd();
        }

        private void btnDeleteChat_Click(object sender, RoutedEventArgs e)
        {
            deleteChat();
        }
        private async void deleteChat()
        {
            await client.DeleteAsync(_user.id + "/" + toIDMessage + "/messages");
            toIDMessage = 0;
            toNameMessage = "";
            toImageMessage = "";
            populateMessage();
            btnDeleteChat.Visibility = Visibility.Collapsed;
    }
        #endregion


    }
}