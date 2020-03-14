﻿using Capstone.Common;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls;
using Capstone.Models;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Capstone
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RemindersPage : Page
    {
        private readonly List<Reminder> Reminders;
        public RemindersPage()
        {
            this.InitializeComponent();
            this.Reminders = this.GetRemindersFromDatabase();
            this.PopulateScreenWithReminders();
        }

        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            UIUtils.GoToMainPage(this);
        }

        private void AddNew_Click(object sender, RoutedEventArgs e)
        {
            var NewReminder = new Reminder();
            this.Frame.Navigate(typeof(RemindersFormPage), NewReminder);
        }

        private List<Reminder> GetRemindersFromDatabase()
        {
            List<Reminder> Reminders = new List<Reminder>();
            // TODO database stuff
            return Reminders;
        }

        private void PopulateScreenWithReminders()
        {
            this.Reminders.ForEach(this.AddReminderToScreen);
        }

        private void AddReminderToScreen(Reminder ReminderToAdd)
        {
            // each Reminder is wrapped in a relative panel
            RelativePanel ReminderPanel = new RelativePanel();
            ReminderPanel.Margin = new Thickness(5, 0, 5, 5);
            var borderBrush = new SolidColorBrush(Windows.UI.Colors.Gray);
            ReminderPanel.BorderBrush = borderBrush;
            ReminderPanel.BorderThickness = new Thickness(1);
            // create the text blocks for the title and date
            var ReminderTitleBlock = this.CreateReminderTitleBlock(ReminderToAdd);
            var ReminderDateBlock = this.CreateReminderDateBlock(ReminderToAdd);
            var ReminderDescriptionBlock = this.CreateDescriptionBlock(ReminderToAdd);
            var deleteButton = this.CreateDeleteButton(ReminderToAdd);
            var editButton = this.CreateEditButton(ReminderToAdd);
            // add the blocks and buttons
            ReminderPanel.Children.Add(ReminderTitleBlock);
            ReminderPanel.Children.Add(ReminderDateBlock);
            ReminderPanel.Children.Add(ReminderDescriptionBlock);
            ReminderPanel.Children.Add(deleteButton);
            ReminderPanel.Children.Add(editButton);
            // relatively place the edit text block
            RelativePanel.SetRightOf(ReminderDateBlock, ReminderTitleBlock);
            RelativePanel.SetAlignRightWithPanel(ReminderDateBlock, true);
            // relatively place the description block
            RelativePanel.SetBelow(ReminderDescriptionBlock, ReminderTitleBlock);
            // relatively place the delete button
            RelativePanel.SetAlignBottomWithPanel(deleteButton, true);
            RelativePanel.SetAlignLeftWithPanel(deleteButton, true);
            RelativePanel.SetBelow(deleteButton, ReminderDateBlock);
            RelativePanel.SetLeftOf(deleteButton, editButton);
            // relatively place the edit button
            RelativePanel.SetAlignBottomWithPanel(editButton, true);
            RelativePanel.SetAlignRightWithPanel(editButton, true);
            RelativePanel.SetBelow(editButton, ReminderDateBlock);
            this.VariableGrid.Children.Add(ReminderPanel);
        }

        private TextBlock CreateReminderTitleBlock(Reminder ReminderToAdd)
        {
            var ReminderTitleBlock = new TextBlock();
            ReminderTitleBlock.Text = ReminderToAdd.Title;
            ReminderTitleBlock.Margin = new Thickness(10);
            ReminderTitleBlock.TextWrapping = TextWrapping.Wrap;
            ReminderTitleBlock.MaxLines = 2;
            return ReminderTitleBlock;
        }

        private TextBlock CreateReminderDateBlock(Reminder ReminderToAdd)
        {
            var ReminderDateBlock = new TextBlock();
            ReminderDateBlock.Text = ReminderToAdd.ActivateDateAndTime.ToString("g");
            ReminderDateBlock.Margin = new Thickness(10);
            ReminderDateBlock.TextAlignment = TextAlignment.Right;
            return ReminderDateBlock;
        }

        private Button CreateDeleteButton(Reminder ReminderToAdd)
        {
            Button deleteButton = new Button();
            deleteButton.Click += (sender, eventArgs) => this.DeleteReminder(ReminderToAdd);
            deleteButton.Content = "Delete";
            deleteButton.Width = 150;
            deleteButton.Margin = new Thickness(10, 10, 0, 0);
            return deleteButton;
        }

        private Button CreateEditButton(Reminder ReminderToAdd)
        {
            Button editButton = new Button();
            editButton.Click += (sender, eventArgs) => this.editReminder(ReminderToAdd);
            editButton.Content = "Edit";
            editButton.Width = 150;
            editButton.Margin = new Thickness(0, 10, 10, 0);
            return editButton;
        }

        private TextBlock CreateDescriptionBlock(Reminder ReminderToAdd)
        {
            var DescriptionBlock = new TextBlock();
            DescriptionBlock.TextWrapping = TextWrapping.Wrap;
            DescriptionBlock.MaxLines = 4;
            DescriptionBlock.Margin = new Thickness(10, 0, 10, 0);
            DescriptionBlock.Text = ReminderToAdd.Description;
            return DescriptionBlock;
        }

        private void DeleteReminder(Reminder ReminderToDelete)
        {
            // TODO
        }

        private void editReminder(Reminder ReminderToEdit)
        {
            this.Frame.Navigate(typeof(RemindersFormPage), ReminderToEdit);
        }
    }
}