using PsISEProjectExplorer.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace PsISEProjectExplorer.UI.Helpers
{
    // http://blog.quantumbitdesigns.com/2008/07/22/programmatically-selecting-an-item-in-a-treeview/
    public static class TreeViewHelper
    {
        /// <summary>
        /// Expands all children of a TreeView
        /// </summary>
        /// <param name="treeView">The TreeView whose children will be expanded</param>
        public static void ExpandAll(this TreeView treeView)
        {
            ExpandSubContainers(treeView);
        }

        /// <summary>
        /// Expands all children of a TreeView or TreeViewItem
        /// </summary>
        /// <param name="parentContainer">The TreeView or TreeViewItem containing the children to expand</param>
        private static void ExpandSubContainers(ItemsControl parentContainer)
        {
            foreach (Object item in parentContainer.Items)
            {
                var currentContainer = parentContainer.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (currentContainer != null && currentContainer.Items.Count > 0)
                {
                    //expand the item
                    currentContainer.IsExpanded = true;

                    //if the item's children are not generated, they must be expanded
                    if (currentContainer.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                    {
                        //store the event handler in a variable so we can remove it (in the handler itself)
                        EventHandler eh = null;
                        eh = delegate
                        {
                            //once the children have been generated, expand those children's children then remove the event handler
                            if (currentContainer.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                            {
                                ExpandSubContainers(currentContainer);
                                currentContainer.ItemContainerGenerator.StatusChanged -= eh;
                            }
                        };

                        currentContainer.ItemContainerGenerator.StatusChanged += eh;
                    }
                    else //otherwise the children have already been generated, so we can now expand those children
                    {
                        ExpandSubContainers(currentContainer);
                    }
                }
            }
        }

        /// <summary>
        /// Searches a TreeView for the provided object and selects it if found
        /// </summary>
        /// <param name="treeView">The TreeView containing the item</param>
        /// <param name="item">The item to search and select</param>
        public static void ExpandAndSelectItem(this TreeView treeView, TreeViewEntryItemModel item)
        {
            ExpandAndSelectItemContainer(treeView, item);
        }

        /// <summary>
        /// Searches a TreeView basing on source.
        /// </summary>
        /// <param name="treeView">treeView</param>
        /// <param name="source">Source of event</param>
        /// <returns>Found TreeViewItem or null</returns>
        public static TreeViewItem FindItemFromSource(this TreeView treeView, DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
            {
                source = VisualTreeHelper.GetParent(source);
            }
            return source as TreeViewItem;
        }

        /// <summary>
        /// Searches a TreeView and selectes it if found.
        /// </summary>
        /// <param name="treeView">treeView</param>
        /// <param name="source">Source of event</param>
        /// <returns>True if item has been selected</returns>
        public static bool SelectItemFromSource(this TreeView treeView, DependencyObject source)
        {
            TreeViewItem item = FindItemFromSource(treeView, source);
            if (item != null)
            {
                item.IsSelected = true;
                return true;
            }
            return false;
        }

        public static TreeViewItem GetSelectedTreeViewItem(this TreeView treeView)
        {
            object selectedItem = treeView.SelectedItem;
            foreach (Object item in treeView.Items)
            {
                var currentContainer = treeView.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;

                //if the data item matches the item we want to select, set the corresponding
                //TreeViewItem IsSelected to true
                if (item == selectedItem && currentContainer != null)
                {
                    return currentContainer;
                }
            }
            return null;
        }


        /// <summary>
        /// Finds the provided object in an ItemsControl's children and selects it
        /// </summary>
        /// <param name="parentContainer">The parent container whose children will be searched for the selected item</param>
        /// <param name="itemToSelect">The item to select</param>
        /// <returns>True if the item is found and selected, false otherwise</returns>
        private static bool ExpandAndSelectItemContainer(ItemsControl parentContainer, TreeViewEntryItemModel itemToSelect)
        {
            IList<TreeViewItem> applicableParents = new List<TreeViewItem>();
            //check all items at the current level
            foreach (TreeViewEntryItemModel item in parentContainer.Items)
            {
                if (itemToSelect.Path.StartsWith(item.Path))
                {
                    var currentContainer = parentContainer.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                    //if the data item matches the item we want to select, set the corresponding
                    //TreeViewItem IsSelected to true
                    if (item == itemToSelect && currentContainer != null)
                    {
                        currentContainer.IsSelected = true;
                        currentContainer.BringIntoView();

                        //the item was found
                        return true;
                    }
                    else
                    {
                        applicableParents.Add(currentContainer);
                    }
                }

                
            }

            //if we get to this point, the selected item was not found at the current level, so we must check the children
            foreach (TreeViewItem currentContainer in applicableParents)
            {
                //if children exist
                if (currentContainer != null && currentContainer.Items.Count > 0)
                {
                    //keep track of if the TreeViewItem was expanded or not
                    bool wasExpanded = currentContainer.IsExpanded;

                    //expand the current TreeViewItem so we can check its child TreeViewItems
                    currentContainer.IsExpanded = true;

                    //if the TreeViewItem child containers have not been generated, we must listen to
                    //the StatusChanged event until they are
                    if (currentContainer.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                    {
                        //store the event handler in a variable so we can remove it (in the handler itself)
                        EventHandler eh = null;
                        eh = delegate
                        {
                            if (currentContainer.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                            {
                                if (ExpandAndSelectItemContainer(currentContainer, itemToSelect) == false)
                                {
                                    //The assumption is that code executing in this EventHandler is the result of the parent not
                                    //being expanded since the containers were not generated.
                                    //since the itemToSelect was not found in the children, collapse the parent since it was previously collapsed
                                    currentContainer.IsExpanded = false;
                                }

                                //remove the StatusChanged event handler since we just handled it (we only needed it once)
                                currentContainer.ItemContainerGenerator.StatusChanged -= eh;
                            }
                        };
                        currentContainer.ItemContainerGenerator.StatusChanged += eh;
                    }
                    else //otherwise the containers have been generated, so look for item to select in the children
                    {
                        if (ExpandAndSelectItemContainer(currentContainer, itemToSelect) == false)
                        {
                            //restore the current TreeViewItem's expanded state
                            currentContainer.IsExpanded = wasExpanded;
                        }
                        else //otherwise the node was found and selected, so return true
                        {
                            return true;
                        }
                    }
                }
            }

            //no item was found
            return false;
        }
    }
}
