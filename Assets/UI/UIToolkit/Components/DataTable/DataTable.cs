using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace TechMogul.UI.Components
{
    public class DataTable
    {
        private VisualElement root;
        private ScrollView dataContainer;
        private List<DataTableColumn> columns;
        private List<object> data;
        
        private string currentSortColumn;
        private bool sortAscending = true;
        
        private Dictionary<string, VisualElement> columnHeaders = new Dictionary<string, VisualElement>();
        
        // Callback for when a row is clicked
        public System.Action<object> OnRowClicked;
        
        public DataTable(VisualElement root)
        {
            this.root = root;
            this.columns = new List<DataTableColumn>();
            this.data = new List<object>();
            
            Initialize();
        }
        
        private void Initialize()
        {
            root.Clear();
            
            // Single container for both header and data
            dataContainer = new ScrollView(ScrollViewMode.Vertical);
            dataContainer.AddToClassList("data-table-body");
            dataContainer.style.flexGrow = 1;
            
            root.Add(dataContainer);
        }
        
        public DataTable AddColumn(DataTableColumn column)
        {
            column.order = columns.Count;
            columns.Add(column);
            return this;
        }
        
        public DataTable SetColumns(List<DataTableColumn> columns)
        {
            this.columns = columns;
            for (int i = 0; i < columns.Count; i++)
            {
                columns[i].order = i;
            }
            return this;
        }
        
        public void Build()
        {
            Refresh();
        }
        
        private void BuildHeaders()
        {
            // Create header as a special row
            var headerRow = new VisualElement();
            headerRow.AddToClassList("data-table-header-row");
            headerRow.style.flexDirection = FlexDirection.Row;
            headerRow.style.backgroundColor = new Color(0.12f, 0.12f, 0.14f);
            headerRow.style.paddingTop = 14;
            headerRow.style.paddingBottom = 14;
            headerRow.style.borderBottomWidth = 2;
            headerRow.style.borderBottomColor = new Color(0.2f, 0.6f, 0.85f);
            headerRow.style.marginTop = 0;
            headerRow.style.marginBottom = 0;
            
            var sortedColumns = columns.OrderBy(c => c.order).Where(c => c.visible).ToList();
            
            for (int i = 0; i < sortedColumns.Count; i++)
            {
                var column = sortedColumns[i];
                var headerCell = CreateHeaderCell(column, i);
                headerRow.Add(headerCell);
            }
            
            // Insert header row at the top of the data container
            if (dataContainer.childCount > 0)
            {
                dataContainer.Insert(0, headerRow);
            }
            else
            {
                dataContainer.Add(headerRow);
            }
        }
        
        private VisualElement CreateHeaderCell(DataTableColumn column, int index)
        {
            // Use a Label (not Button) for perfect alignment with data cells
            var cell = new Label();
            cell.text = column.title;
            cell.AddToClassList("data-table-header-cell");
            cell.style.width = column.width;
            cell.style.minWidth = column.minWidth;
            cell.style.maxWidth = column.maxWidth;
            cell.style.flexShrink = 0;
            cell.style.paddingLeft = 8;
            cell.style.paddingRight = 8;
            cell.style.unityTextAlign = column.alignment;
            cell.style.color = new Color(0.59f, 0.59f, 0.63f);
            cell.style.fontSize = 12;
            cell.style.unityFontStyleAndWeight = FontStyle.Bold;
            
            // Add right border
            cell.style.borderRightWidth = 1;
            cell.style.borderRightColor = new Color(1f, 1f, 1f, 0.1f);
            
            if (column.sortable && currentSortColumn == column.id)
            {
                cell.text += sortAscending ? " ▼" : " ▲";
            }
            
            // Add click handler for sorting (without using Button)
            if (column.sortable)
            {
                cell.RegisterCallback<ClickEvent>(evt => OnHeaderClick(column));
                
                // Add hover effect
                cell.RegisterCallback<MouseEnterEvent>(evt =>
                {
                    cell.style.backgroundColor = new Color(0.2f, 0.6f, 0.85f, 0.1f);
                });
                
                cell.RegisterCallback<MouseLeaveEvent>(evt =>
                {
                    cell.style.backgroundColor = Color.clear;
                });
            }
            
            columnHeaders[column.id] = cell;
            
            // Add resize manipulator directly to the label (hold for 1 second, then drag)
            if (column.resizable)
            {
                var resizeManipulator = new ColumnResizeManipulator(cell, column, (newWidth) =>
                {
                    cell.style.width = newWidth;
                    RefreshRowWidths();
                });
                cell.AddManipulator(resizeManipulator);
            }
            
            return cell;
        }
        
        private void OnHeaderClick(DataTableColumn column)
        {
            if (!column.sortable) return;
            
            if (currentSortColumn == column.id)
            {
                sortAscending = !sortAscending;
            }
            else
            {
                currentSortColumn = column.id;
                sortAscending = true;
            }
            
            Refresh();
        }
        
        public void SetData<T>(List<T> data)
        {
            this.data = data.Cast<object>().ToList();
            Refresh();
        }
        
        public void Refresh()
        {
            dataContainer.Clear();
            columnHeaders.Clear();
            
            // Add header row first
            BuildHeaders();
            
            var sortedData = GetSortedData();
            
            // Add data rows
            foreach (var item in sortedData)
            {
                var row = CreateRow(item);
                dataContainer.Add(row);
            }
        }
        
        private List<object> GetSortedData()
        {
            if (string.IsNullOrEmpty(currentSortColumn))
                return data;
            
            var column = columns.FirstOrDefault(c => c.id == currentSortColumn);
            if (column == null || column.sortValueGetter == null)
                return data;
            
            var sorted = sortAscending
                ? data.OrderBy(item => column.sortValueGetter(item))
                : data.OrderByDescending(item => column.sortValueGetter(item));
            
            return sorted.ToList();
        }
        
        private VisualElement CreateRow(object item)
        {
            var row = new VisualElement();
            row.AddToClassList("data-table-row");
            row.style.flexDirection = FlexDirection.Row;
            row.style.paddingLeft = 0;
            row.style.paddingRight = 0;
            row.style.paddingTop = 12;
            row.style.paddingBottom = 12;
            row.style.marginTop = 2;
            row.style.marginBottom = 2;
            row.style.backgroundColor = new Color(1, 1, 1, 0.02f);
            row.style.borderTopLeftRadius = 4;
            row.style.borderTopRightRadius = 4;
            row.style.borderBottomLeftRadius = 4;
            row.style.borderBottomRightRadius = 4;
            row.style.borderLeftWidth = 1;
            row.style.borderRightWidth = 1;
            row.style.borderTopWidth = 1;
            row.style.borderBottomWidth = 1;
            row.style.borderLeftColor = Color.clear;
            row.style.borderRightColor = Color.clear;
            row.style.borderTopColor = Color.clear;
            row.style.borderBottomColor = Color.clear;
            
            row.RegisterCallback<MouseEnterEvent>(evt =>
            {
                row.style.backgroundColor = new Color(0.2f, 0.6f, 0.85f, 0.1f);
                row.style.borderLeftColor = new Color(0.2f, 0.6f, 0.85f, 0.3f);
                row.style.borderRightColor = new Color(0.2f, 0.6f, 0.85f, 0.3f);
                row.style.borderTopColor = new Color(0.2f, 0.6f, 0.85f, 0.3f);
                row.style.borderBottomColor = new Color(0.2f, 0.6f, 0.85f, 0.3f);
            });
            
            row.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                row.style.backgroundColor = new Color(1, 1, 1, 0.02f);
                row.style.borderLeftColor = Color.clear;
                row.style.borderRightColor = Color.clear;
                row.style.borderTopColor = Color.clear;
                row.style.borderBottomColor = Color.clear;
            });
            
            // Add click handler for row selection
            row.RegisterCallback<ClickEvent>(evt =>
            {
                OnRowClicked?.Invoke(item);
            });
            
            var sortedColumns = columns.OrderBy(c => c.order).Where(c => c.visible).ToList();
            
            foreach (var column in sortedColumns)
            {
                var cell = CreateCell(column, item);
                row.Add(cell);
            }
            
            return row;
        }
        
        private VisualElement CreateCell(DataTableColumn column, object item)
        {
            var cell = new Label();
            cell.style.width = column.width;
            cell.style.minWidth = column.minWidth;
            cell.style.maxWidth = column.maxWidth;
            cell.style.flexShrink = 0;
            cell.style.unityTextAlign = column.alignment;
            cell.style.fontSize = 14;
            cell.style.color = new Color(0.86f, 0.86f, 0.9f);
            cell.style.overflow = Overflow.Hidden;
            cell.style.paddingLeft = 8;
            cell.style.paddingRight = 8;
            
            // Add right border to match header
            cell.style.borderRightWidth = 1;
            cell.style.borderRightColor = new Color(1f, 1f, 1f, 0.1f);
            
            if (column.formatter != null)
            {
                cell.text = column.formatter(item);
            }
            else if (column.sortValueGetter != null)
            {
                var value = column.sortValueGetter(item);
                cell.text = value?.ToString() ?? "";
            }
            else
            {
                cell.text = "";
            }
            
            return cell;
        }
        
        private void RefreshRowWidths()
        {
            foreach (var row in dataContainer.Children())
            {
                int cellIndex = 0;
                var sortedColumns = columns.OrderBy(c => c.order).Where(c => c.visible).ToList();
                
                foreach (var cell in row.Children())
                {
                    if (cellIndex < sortedColumns.Count)
                    {
                        var column = sortedColumns[cellIndex];
                        cell.style.width = column.width;
                        cellIndex++;
                    }
                }
            }
        }
        
        public void SaveColumnState(string key)
        {
            var state = new ColumnState
            {
                columns = columns.Select(c => new ColumnStateData
                {
                    id = c.id,
                    width = c.width,
                    order = c.order,
                    visible = c.visible
                }).ToList()
            };
            
            string json = JsonUtility.ToJson(state);
            PlayerPrefs.SetString($"DataTable_{key}", json);
            PlayerPrefs.Save();
        }
        
        public void LoadColumnState(string key)
        {
            string json = PlayerPrefs.GetString($"DataTable_{key}", "");
            if (string.IsNullOrEmpty(json)) return;
            
            try
            {
                var state = JsonUtility.FromJson<ColumnState>(json);
                
                foreach (var stateData in state.columns)
                {
                    var column = columns.FirstOrDefault(c => c.id == stateData.id);
                    if (column != null)
                    {
                        column.width = stateData.width;
                        column.order = stateData.order;
                        column.visible = stateData.visible;
                    }
                }
                
                BuildHeaders();
                Refresh();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to load column state: {e.Message}");
            }
        }
    }
    
    [Serializable]
    public class ColumnState
    {
        public List<ColumnStateData> columns;
    }
    
    [Serializable]
    public class ColumnStateData
    {
        public string id;
        public float width;
        public int order;
        public bool visible;
    }
}
