using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core.Preview;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Mis_Listas.Controls;
using Mis_Listas.Models;
using Mis_Listas.Services;

namespace Mis_Listas
{
    public sealed partial class MainPage : Page
    {
        public List<cList> lists = new List<cList>();
        public ResourceLoader textos;

        public MainPage()
        {
            InitializeComponent();

            this.textos = new ResourceLoader();

            ApplicationView.PreferredLaunchViewSize = new Size(600, 750);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += OnCloseRequest;

            Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            lists = await StorageService.LoadDataAsync();
            PopulateListSelector();

            if (lists.Count > 0)
            {
                slctList.SelectedIndex = 0;
            }
            else
            {
                UpdateAppTitle(textos.GetString("noListas"));
                LoadTasks(-1);
            }
        }

        public async Task SaveData()
        {
            await StorageService.SaveDataAsync(lists);
        }

        private void PopulateListSelector()
        {
            slctList.SelectionChanged -= slctList_SelectionChanged;
            slctList.Items.Clear();

            if (lists.Count == 0)
            {
                ComboBoxItem defaultItem = new ComboBoxItem
                {
                    Name = "itemXdef",
                    Content = textos.GetString("noListas")
                };
                slctList.Items.Add(defaultItem);
                slctList.SelectedIndex = 0;
            }
            else
            {
                foreach (cList list in lists)
                {
                    ComboBoxItem item = new ComboBoxItem
                    {
                        Content = string.IsNullOrEmpty(list.name) ? list.text : list.name,
                        Tag = list.id
                    };
                    slctList.Items.Add(item);
                }
            }

            slctList.SelectionChanged += slctList_SelectionChanged;
        }

        private void slctList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = slctList.SelectedIndex;
            if (index >= 0 && index < lists.Count)
            {
                cList currentList = lists[index];
                string listTitle = string.IsNullOrEmpty(currentList.name) ? currentList.text : currentList.name;
                UpdateAppTitle(listTitle);
                ApplyListColor(currentList.color);
                LoadTasks(index);
            }
            else
            {
                UpdateAppTitle(textos.GetString("noListas"));
                LoadTasks(-1);
            }
        }

        private void UpdateAppTitle(string title)
        {
            ApplicationView appView = ApplicationView.GetForCurrentView();
            appView.Title = title;
        }

        private void ApplyListColor(string colorHexOrName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(colorHexOrName))
                {
                    MainGrid.Background = new SolidColorBrush(Colors.Orange);
                    return;
                }

                if (colorHexOrName.StartsWith("#"))
                {
                    string hex = colorHexOrName.Substring(1);
                    if (hex.Length == 6)
                    {
                        byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
                        byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
                        byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
                        MainGrid.Background = new SolidColorBrush(Color.FromArgb(255, r, g, b));
                        return;
                    }
                    else if (hex.Length == 8)
                    {
                        byte a = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
                        byte r = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
                        byte g = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
                        byte b = byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber);
                        MainGrid.Background = new SolidColorBrush(Color.FromArgb(a, r, g, b));
                        return;
                    }
                }

                // Named colors fallback
                switch (colorHexOrName.ToLowerInvariant())
                {
                    case "blue":
                        MainGrid.Background = new SolidColorBrush(Colors.DodgerBlue);
                        break;
                    case "green":
                        MainGrid.Background = new SolidColorBrush(Colors.MediumSeaGreen);
                        break;
                    case "red":
                        MainGrid.Background = new SolidColorBrush(Colors.IndianRed);
                        break;
                    case "purple":
                        MainGrid.Background = new SolidColorBrush(Colors.MediumPurple);
                        break;
                    case "teal":
                        MainGrid.Background = new SolidColorBrush(Colors.Teal);
                        break;
                    case "orange":
                    default:
                        MainGrid.Background = new SolidColorBrush(Colors.Orange);
                        break;
                }
            }
            catch
            {
                MainGrid.Background = new SolidColorBrush(Colors.Orange);
            }
        }

        public void LoadTasks(int listIndex)
        {
            pnlList.Children.Clear();

            if (listIndex < 0 || listIndex >= lists.Count)
            {
                lblXdef.Visibility = Visibility.Visible;
                return;
            }

            cList list = lists[listIndex];
            if (list.tasks == null || list.tasks.Count == 0)
            {
                lblXdef.Visibility = Visibility.Visible;
                return;
            }

            // Filtering
            string filterTag = (slctFilter?.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "pending";
            IEnumerable<cTask> filteredTasks = list.tasks;

            switch (filterTag)
            {
                case "pending":
                    filteredTasks = filteredTasks.Where(t => t.visible && !t.finished);
                    break;
                case "finished":
                    filteredTasks = filteredTasks.Where(t => t.visible && t.finished);
                    break;
                case "deleted":
                    filteredTasks = filteredTasks.Where(t => !t.visible);
                    break;
                case "all":
                default:
                    filteredTasks = filteredTasks.Where(t => t.visible);
                    break;
            }

            // Sorting
            string sortTag = (slctSort?.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "created";
            switch (sortTag)
            {
                case "name":
                    filteredTasks = filteredTasks.OrderBy(t => t.text);
                    break;
                case "expired":
                    filteredTasks = filteredTasks.OrderBy(t => t.expired.HasValue ? t.expired.Value : DateTime.MaxValue);
                    break;
                case "created":
                default:
                    filteredTasks = filteredTasks.OrderBy(t => t.date);
                    break;
            }

            var taskList = filteredTasks.ToList();

            if (taskList.Count == 0)
            {
                lblXdef.Visibility = Visibility.Visible;
            }
            else
            {
                lblXdef.Visibility = Visibility.Collapsed;

                foreach (cTask task in taskList)
                {
                    int originalTaskIndex = list.tasks.IndexOf(task);
                    TareaControl ctrl = new TareaControl(task.text, listIndex, originalTaskIndex, task);
                    ctrl.deleteTask += TaskControl_DeleteTask;
                    ctrl.checkTask += TaskControl_CheckTask;
                    ctrl.editTask += TaskControl_EditTask;

                    pnlList.Children.Add(ctrl);
                }
            }
        }

        #region Task Events & Actions

        private async void TaskControl_DeleteTask(object? sender, deleteTaskEventArgs e)
        {
            if (e.list < 0 || e.list >= lists.Count) return;
            cList list = lists[e.list];
            if (e.task < 0 || e.task >= list.tasks.Count) return;

            cTask task = list.tasks[e.task];

            string filterTag = (slctFilter?.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "pending";

            // If already soft-deleted, ask for permanent deletion or restore
            if (!task.visible)
            {
                ContentDialog permDeleteDialog = new ContentDialog
                {
                    Title = textos.GetString("txtAtencion"),
                    Content = $"¿Desea restaurar la tarea '{task.text}' o eliminarla permanentemente?",
                    PrimaryButtonText = "Restaurar",
                    SecondaryButtonText = "Eliminar",
                    CloseButtonText = textos.GetString("txtNo")
                };

                var permRes = await permDeleteDialog.ShowAsync();
                if (permRes == ContentDialogResult.Primary)
                {
                    task.visible = true;
                    await SaveData();
                    LoadTasks(e.list);
                }
                else if (permRes == ContentDialogResult.Secondary)
                {
                    list.tasks.RemoveAt(e.task);
                    await SaveData();
                    LoadTasks(e.list);
                }
                return;
            }

            ContentDialog msgBox = new ContentDialog
            {
                Title = textos.GetString("txtAtencion"),
                Content = string.Format(textos.GetString("txtSeguroBorrarTarea"), task.text),
                PrimaryButtonText = textos.GetString("txtSi"),
                CloseButtonText = textos.GetString("txtNo")
            };

            var res = await msgBox.ShowAsync();
            if (res == ContentDialogResult.Primary)
            {
                // Soft delete (moves to bin)
                task.visible = false;
                await SaveData();
                LoadTasks(e.list);
            }
        }

        private async void TaskControl_CheckTask(object? sender, checkTaskEventArgs e)
        {
            if (e.list < 0 || e.list >= lists.Count) return;
            cList list = lists[e.list];
            if (e.task < 0 || e.task >= list.tasks.Count) return;

            cTask task = list.tasks[e.task];
            task.finished = !task.finished;

            await SaveData();
            LoadTasks(e.list);
        }

        private async void TaskControl_EditTask(object? sender, editTaskEventArgs e)
        {
            await SaveData();
        }

        #endregion

        #region Flyouts & List CRUD

        private void btnNuevaLista_Click(object sender, RoutedEventArgs e)
        {
            tituloNuevaLista.Text = textos.GetString("tituloNuevaLista");
            lblNuevaLista.Text = textos.GetString("lblNuevaLista");
            txt_btnAdd_newList.Text = textos.GetString("btnAdd");
            txt_btnCancel_newList.Text = textos.GetString("btnCancel");
            txt_newList_name.Text = "";
            txt_newList_name.PlaceholderText = textos.GetString("ttpNuevaLista");
            txt_newList_name.PlaceholderForeground = new SolidColorBrush(Colors.LightGray);
        }

        private async void btnAdd_newList_Click(object sender, RoutedEventArgs e)
        {
            string listName = txt_newList_name.Text.Trim();
            if (!string.IsNullOrEmpty(listName))
            {
                flyNuevaLista.Hide();

                Color selColor = colorPickerLista.Color;
                string hexColor = $"#{selColor.R:X2}{selColor.G:X2}{selColor.B:X2}";

                cList newList = new cList
                {
                    id = lists.Count > 0 ? lists.Max(l => l.id) + 1 : 1,
                    name = listName,
                    text = listName,
                    color = hexColor,
                    visible = true,
                    date = DateTime.Now,
                    tasks = new List<cTask>()
                };

                lists.Add(newList);
                await SaveData();

                PopulateListSelector();
                slctList.SelectedIndex = lists.Count - 1;
            }
            else
            {
                txt_newList_name.PlaceholderText = textos.GetString("errNuevaLista");
                txt_newList_name.PlaceholderForeground = new SolidColorBrush(Colors.Red);
            }
        }

        private void btnCancel_newList_Click(object sender, RoutedEventArgs e)
        {
            flyNuevaLista.Hide();
        }

        private async void btnDeleteList_Click(object sender, RoutedEventArgs e)
        {
            int index = slctList.SelectedIndex;
            if (index < 0 || index >= lists.Count) return;

            cList listToDelete = lists[index];
            string listName = string.IsNullOrEmpty(listToDelete.name) ? listToDelete.text : listToDelete.name;

            ContentDialog msgBox = new ContentDialog
            {
                Title = textos.GetString("txtAtencion"),
                Content = string.Format(textos.GetString("txtSeguroBorrarLista"), listName),
                PrimaryButtonText = textos.GetString("txtSi"),
                CloseButtonText = textos.GetString("txtNo")
            };

            var res = await msgBox.ShowAsync();
            if (res == ContentDialogResult.Primary)
            {
                lists.RemoveAt(index);
                await SaveData();

                PopulateListSelector();
                if (lists.Count > 0)
                {
                    slctList.SelectedIndex = Math.Min(index, lists.Count - 1);
                }
                else
                {
                    slctList.SelectedIndex = -1;
                    UpdateAppTitle(textos.GetString("noListas"));
                    LoadTasks(-1);
                }
            }
        }

        private void btnNuevaTarea_Click(object sender, RoutedEventArgs e)
        {
            if (slctList.SelectedIndex < 0 || slctList.SelectedIndex >= lists.Count)
            {
                flyAddTask.Hide();
                return;
            }

            tituloNuevaTarea.Text = textos.GetString("tituloNuevaTarea");
            lblNuevaTarea.Text = textos.GetString("lblNuevaTarea");
            txt_btnAdd_newTask.Text = textos.GetString("btnAdd");
            txt_btnCancel_newTask.Text = textos.GetString("btnCancel");
            txt_newTask_name.Text = "";
            txt_newTask_name.PlaceholderText = textos.GetString("ttpNuevaTarea");
            txt_newTask_name.PlaceholderForeground = new SolidColorBrush(Colors.LightGray);
            chkEnableDueDate.IsChecked = false;
            dp_newTask_expired.Date = DateTimeOffset.Now.AddDays(1);
            dp_newTask_expired.IsEnabled = false;
        }

        private void chkEnableDueDate_Checked(object sender, RoutedEventArgs e)
        {
            dp_newTask_expired.IsEnabled = true;
        }

        private void chkEnableDueDate_Unchecked(object sender, RoutedEventArgs e)
        {
            dp_newTask_expired.IsEnabled = false;
        }

        private async void btnAdd_newTask_Click(object sender, RoutedEventArgs e)
        {
            int listIndex = slctList.SelectedIndex;
            if (listIndex < 0 || listIndex >= lists.Count) return;

            string taskName = txt_newTask_name.Text.Trim();
            if (!string.IsNullOrEmpty(taskName))
            {
                flyAddTask.Hide();

                DateTime? dueDate = null;
                if (chkEnableDueDate.IsChecked == true)
                {
                    dueDate = dp_newTask_expired.Date.DateTime;
                }

                cTask newTask = new cTask
                {
                    text = taskName,
                    name = taskName,
                    date = DateTime.Now,
                    expired = dueDate,
                    finished = false,
                    visible = true
                };

                lists[listIndex].tasks.Add(newTask);
                await SaveData();

                LoadTasks(listIndex);
            }
            else
            {
                txt_newTask_name.PlaceholderText = textos.GetString("errNuevaTarea");
                txt_newTask_name.PlaceholderForeground = new SolidColorBrush(Colors.Red);
            }
        }

        private void btnCancel_newTask_Click(object sender, RoutedEventArgs e)
        {
            flyAddTask.Hide();
        }

        #endregion

        #region Filters & Sorting

        private void slctFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadTasks(slctList.SelectedIndex);
        }

        private void slctSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadTasks(slctList.SelectedIndex);
        }

        #endregion

        #region Window Close Confirmation

        private async void OnCloseRequest(object? sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            var deferral = e.GetDeferral();

            ContentDialog dialog = new ContentDialog
            {
                Title = textos.GetString("txtAtencion"),
                Content = textos.GetString("txtSeguroCerrar"),
                PrimaryButtonText = textos.GetString("txtSi"),
                CloseButtonText = textos.GetString("txtNo")
            };

            var res = await dialog.ShowAsync();
            if (res == ContentDialogResult.Primary)
            {
                await SaveData();
                Application.Current.Exit();
            }
            else
            {
                e.Handled = true;
            }

            deferral.Complete();
        }

        #endregion
    }
}
