using PsISEProjectExplorer.Commands;
using PsISEProjectExplorer.UI.ViewModel;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PsISEProjectExplorer.UI.Helpers
{
    [Component]
    public class DragDropHandler
    {

        public Point DragStartPoint { get; set; }

        private TreeViewModel TreeViewModel { get; set; }


        private CommandExecutor CommandExecutor { get; set; }

        public DragDropHandler(TreeViewModel treeViewModel, CommandExecutor commandExecutor)
        {
            this.TreeViewModel = treeViewModel;
            this.CommandExecutor = commandExecutor;
        }

        public void ClearDragStartPoint()
        {
            this.DragStartPoint = new Point(0.0, 0.0);
        }

        public void HandleMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var selectedItem = this.TreeViewModel.SelectedItem;
                if ((selectedItem != null && selectedItem.IsBeingEdited))
                {
                    return;
                }
                if (this.IsDragStartPointEmpty())
                {
                    return;
                }

                var mousePos = e.GetPosition(null);
                var diff = this.DragStartPoint - mousePos;

                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance
                    || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    var treeView = sender as TreeView;
                    if (treeView == null)
                    {
                        return;
                    }

                    var treeViewItem = treeView.FindItemFromSource((DependencyObject)e.OriginalSource);
                    if (treeViewItem == null)
                    {
                        return;
                    }

                    var item = treeView.SelectedItem as TreeViewEntryItemModel;
                    if (item == null)
                    {
                        return;
                    }

                    var dragData = new DataObject(item);
                    DragDrop.DoDragDrop(treeViewItem, dragData, DragDropEffects.Move);
                }
            }
            else if (!IsDragStartPointEmpty())
            {
                ClearDragStartPoint();
            }
        }

        public void HandleDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TreeViewEntryItemModel)))
            {
                var item = e.Data.GetData(typeof(TreeViewEntryItemModel)) as TreeViewEntryItemModel;
                var treeView = sender as TreeView;

                if (treeView == null || item == null)
                {
                    return;
                }

                var treeViewItem = treeView.FindItemFromSource((DependencyObject)e.OriginalSource);

                TreeViewEntryItemModel dropTarget = null;

                if (treeViewItem != null)
                {
                    dropTarget = treeViewItem.Header as TreeViewEntryItemModel;
                    if (dropTarget == null)
                    {
                        return;
                    }
                }

                if (item != dropTarget)
                {
                    this.CommandExecutor.ExecuteWithParam<MoveItemCommand, Tuple<TreeViewEntryItemModel, TreeViewEntryItemModel>>(Tuple.Create(item, dropTarget));
                }
            }
            this.ClearDragStartPoint();
        }

        public void HandleDragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(TreeViewEntryItemModel)))
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private Boolean IsDragStartPointEmpty()
        {
            return this.DragStartPoint.X == 0.0 && this.DragStartPoint.Y == 0.0;
        }

    }
}
