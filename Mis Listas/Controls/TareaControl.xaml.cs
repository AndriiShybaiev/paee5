using System;
using Windows.System;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Mis_Listas.Models;

namespace Mis_Listas.Controls
{
    public class deleteTaskEventArgs : EventArgs
    {
        public int list { get; set; }
        public int task { get; set; }
        public cTask? taskItem { get; set; }

        public deleteTaskEventArgs(int l, int t, cTask? item = null) : base()
        {
            list = l;
            task = t;
            taskItem = item;
        }
    }

    public class checkTaskEventArgs : EventArgs
    {
        public int list { get; set; }
        public int task { get; set; }
        public cTask? taskItem { get; set; }

        public checkTaskEventArgs(int l, int t, cTask? item = null) : base()
        {
            list = l;
            task = t;
            taskItem = item;
        }
    }

    public class editTaskEventArgs : EventArgs
    {
        public int list { get; set; }
        public int task { get; set; }
        public string newText { get; set; }

        public editTaskEventArgs(int l, int t, string text) : base()
        {
            list = l;
            task = t;
            newText = text;
        }
    }

    public sealed partial class TareaControl : UserControl
    {
        public int list { get; set; }
        public int task { get; set; }
        public cTask? TaskModel { get; private set; }

        public event EventHandler<deleteTaskEventArgs>? deleteTask;
        public event EventHandler<checkTaskEventArgs>? checkTask;
        public event EventHandler<editTaskEventArgs>? editTask;

        public TareaControl()
        {
            InitializeComponent();
        }

        public TareaControl(string text, int list = 0, int task = 0, cTask? taskModel = null)
        {
            InitializeComponent();
            this.list = list;
            this.task = task;
            this.TaskModel = taskModel;

            setText(text);
            if (taskModel != null)
            {
                UpdateVisuals(taskModel);
            }
        }

        public void SetTask(cTask taskModel, int listIndex, int taskIndex)
        {
            this.list = listIndex;
            this.task = taskIndex;
            this.TaskModel = taskModel;
            setText(taskModel.text);
            UpdateVisuals(taskModel);
        }

        public void setText(string text)
        {
            lblTask.Text = text;
            txtTaskEdit.Text = text;
        }

        public void UpdateVisuals(cTask taskModel)
        {
            bool isFinished = taskModel.finished;
            bool isOverdue = taskModel.IsOverdue;

            if (isFinished)
            {
                lblTask.TextDecorations = TextDecorations.Strikethrough;
                lblTask.Foreground = new SolidColorBrush(Colors.DarkGray);
                pnlContainer.Background = new SolidColorBrush(Color.FromArgb(255, 240, 240, 240));
                pnlContainer.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
                btnCheckTask.Opacity = 0.4;
            }
            else
            {
                lblTask.TextDecorations = TextDecorations.None;
                lblTask.Foreground = new SolidColorBrush(Color.FromArgb(255, 30, 30, 30));
                pnlContainer.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 224));
                pnlContainer.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 224, 224, 208));
                btnCheckTask.Opacity = 1.0;
            }

            if (taskModel.expired.HasValue)
            {
                pnlDueDate.Visibility = Visibility.Visible;
                lblDueDate.Text = $"Vence: {taskModel.expired.Value:dd/MM/yyyy}";

                if (isOverdue)
                {
                    badgeDueDate.Background = new SolidColorBrush(Color.FromArgb(255, 255, 205, 210));
                    lblDueDate.Foreground = new SolidColorBrush(Color.FromArgb(255, 198, 40, 40));
                    lblDueDate.FontWeight = FontWeights.Bold;
                    pnlContainer.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 229, 115, 115));
                }
                else
                {
                    badgeDueDate.Background = new SolidColorBrush(Color.FromArgb(255, 238, 238, 238));
                    lblDueDate.Foreground = new SolidColorBrush(Color.FromArgb(255, 100, 100, 100));
                    lblDueDate.FontWeight = FontWeights.Normal;
                }
            }
            else
            {
                pnlDueDate.Visibility = Visibility.Collapsed;
            }
        }

        private void btnDeleteTask_Click(object sender, RoutedEventArgs e)
        {
            deleteTask?.Invoke(this, new deleteTaskEventArgs(list, task, TaskModel));
        }

        private void btnCheckTask_Click(object sender, RoutedEventArgs e)
        {
            checkTask?.Invoke(this, new checkTaskEventArgs(list, task, TaskModel));
        }

        private void lblTask_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // Begin inline editing
            lblTask.Visibility = Visibility.Collapsed;
            txtTaskEdit.Visibility = Visibility.Visible;
            txtTaskEdit.Text = lblTask.Text;
            txtTaskEdit.SelectAll();
            txtTaskEdit.Focus(FocusState.Programmatic);
        }

        private void FinishEdit()
        {
            if (txtTaskEdit.Visibility == Visibility.Visible)
            {
                string newText = txtTaskEdit.Text.Trim();
                if (!string.IsNullOrEmpty(newText))
                {
                    lblTask.Text = newText;
                    if (TaskModel != null)
                    {
                        TaskModel.text = newText;
                    }
                    editTask?.Invoke(this, new editTaskEventArgs(list, task, newText));
                }
                txtTaskEdit.Visibility = Visibility.Collapsed;
                lblTask.Visibility = Visibility.Visible;
            }
        }

        private void txtTaskEdit_LostFocus(object sender, RoutedEventArgs e)
        {
            FinishEdit();
        }

        private void txtTaskEdit_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                FinishEdit();
                e.Handled = true;
            }
            else if (e.Key == VirtualKey.Escape)
            {
                txtTaskEdit.Visibility = Visibility.Collapsed;
                lblTask.Visibility = Visibility.Visible;
                e.Handled = true;
            }
        }
    }
}
