﻿using Capstone.Actions;
using Capstone.Common;
using Capstone.Providers;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace Capstone
{
    /// <summary>
    /// The main page that is shown to the user when they open up the application
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private bool IsMenuExpanded = false;
        public MainPage()
        {
            this.InitializeComponent();
            // hide the main menu
            this.MenuColumn.Width = new GridLength(0);
            ActionRouter.SetUp();

            // DEBUG -- uncomment this code if you want to test out the weather service.
            //string command = "what's the weather?";
            //Func<string, Actions.Action> foundAction = ActionRouter.GetFunctionFromCommandString(command);
            //foundAction(command).PerformAction(this.media);
        }

        private void MenuButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!this.IsMenuExpanded)
            {
                // show the menu column
                this.MenuColumn.Width = new GridLength(320);
            }
            else
            {
                this.MenuColumn.Width = new GridLength(0);
            }
            this.IsMenuExpanded = !this.IsMenuExpanded;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO navigate to settings screen (this.Frame.Navigate(typeof(screenName)))
        }

        private void RemindersButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(RemindersPage));
        }

        private void AlarmsButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AlarmsPage));
        }

        private void VoiceNotesButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(VoiceMemosPage));
        }

        private void DataPrivacyButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(DataPrivacyTips));
        }

        private void LibrariesButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO navigate to settings screen (this.Frame.Navigate(typeof(screenName)))
        }
    }
}
