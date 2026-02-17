using System;
using UnityEngine;

namespace TechMogul.UI.Components
{
    [Serializable]
    public class DataTableColumn
    {
        public string id;
        public string title;
        public float width;
        public float minWidth;
        public float maxWidth;
        public TextAnchor alignment;
        public bool sortable;
        public bool resizable;
        public int order;
        public bool visible;
        
        public Func<object, string> formatter;
        public Func<object, object> sortValueGetter;
        
        public DataTableColumn(string id, string title, float width = 100f)
        {
            this.id = id;
            this.title = title;
            this.width = width;
            this.minWidth = 50f;
            this.maxWidth = 500f;
            this.alignment = TextAnchor.MiddleLeft;
            this.sortable = true;
            this.resizable = true;
            this.visible = true;
            this.order = 0;
        }
        
        public DataTableColumn SetAlignment(TextAnchor alignment)
        {
            this.alignment = alignment;
            return this;
        }
        
        public DataTableColumn SetMinWidth(float minWidth)
        {
            this.minWidth = minWidth;
            return this;
        }
        
        public DataTableColumn SetMaxWidth(float maxWidth)
        {
            this.maxWidth = maxWidth;
            return this;
        }
        
        public DataTableColumn SetSortable(bool sortable)
        {
            this.sortable = sortable;
            return this;
        }
        
        public DataTableColumn SetResizable(bool resizable)
        {
            this.resizable = resizable;
            return this;
        }
        
        public DataTableColumn SetFormatter(Func<object, string> formatter)
        {
            this.formatter = formatter;
            return this;
        }
        
        public DataTableColumn SetSortValueGetter(Func<object, object> sortValueGetter)
        {
            this.sortValueGetter = sortValueGetter;
            return this;
        }
    }
}
