using UnityEngine;
using UnityEngine.UIElements;

namespace TechMogul.UI.Components
{
    public class ColumnDragManipulator : MouseManipulator
    {
        private Vector2 startMousePosition;
        private bool isDragging;
        private VisualElement dragPreview;
        private System.Action<int, int> onReorder;
        private int columnIndex;
        private VisualElement headerContainer;
        
        public ColumnDragManipulator(int columnIndex, VisualElement headerContainer, System.Action<int, int> onReorder)
        {
            this.columnIndex = columnIndex;
            this.headerContainer = headerContainer;
            this.onReorder = onReorder;
        }
        
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
        }
        
        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        }
        
        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button == 0 && evt.shiftKey)
            {
                startMousePosition = evt.mousePosition;
                isDragging = true;
                
                dragPreview = new VisualElement();
                dragPreview.AddToClassList("column-drag-preview");
                dragPreview.style.position = Position.Absolute;
                dragPreview.style.left = evt.mousePosition.x;
                dragPreview.style.top = evt.mousePosition.y;
                
                var label = new Label(target.Q<Label>()?.text ?? "Column");
                label.style.color = new Color(0.2f, 0.6f, 0.85f);
                label.style.paddingLeft = 10;
                label.style.paddingRight = 10;
                label.style.paddingTop = 10;
                label.style.paddingBottom = 10;
                dragPreview.Add(label);
                
                target.panel.visualTree.Add(dragPreview);
                target.CaptureMouse();
                evt.StopPropagation();
            }
        }
        
        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (isDragging && dragPreview != null)
            {
                dragPreview.style.left = evt.mousePosition.x - 50;
                dragPreview.style.top = evt.mousePosition.y - 20;
                evt.StopPropagation();
            }
        }
        
        private void OnMouseUp(MouseUpEvent evt)
        {
            if (isDragging && evt.button == 0)
            {
                isDragging = false;
                
                if (dragPreview != null)
                {
                    dragPreview.RemoveFromHierarchy();
                    dragPreview = null;
                }
                
                int newIndex = GetColumnIndexAtPosition(evt.mousePosition);
                if (newIndex != -1 && newIndex != columnIndex)
                {
                    onReorder?.Invoke(columnIndex, newIndex);
                }
                
                target.ReleaseMouse();
                evt.StopPropagation();
            }
        }
        
        private int GetColumnIndexAtPosition(Vector2 position)
        {
            for (int i = 0; i < headerContainer.childCount; i++)
            {
                var child = headerContainer[i];
                if (child.worldBound.Contains(position))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
