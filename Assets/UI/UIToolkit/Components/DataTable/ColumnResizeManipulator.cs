using UnityEngine;
using UnityEngine.UIElements;

namespace TechMogul.UI.Components
{
    public class ColumnResizeManipulator : MouseManipulator
    {
        private Vector2 startMousePosition;
        private float startWidth;
        private bool isResizing;
        private bool isHolding;
        private VisualElement column;
        private DataTableColumn columnData;
        private System.Action<float> onResize;
        private long holdStartTime;
        private const long HOLD_DURATION_MS = 1000; // 1 second hold required
        
        public ColumnResizeManipulator(VisualElement column, DataTableColumn columnData, System.Action<float> onResize)
        {
            this.column = column;
            this.columnData = columnData;
            this.onResize = onResize;
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
            // Only start hold detection with left mouse button and NO modifiers
            if (evt.button == 0 && !evt.shiftKey && !evt.ctrlKey && !evt.altKey)
            {
                startMousePosition = evt.mousePosition;
                startWidth = column.resolvedStyle.width;
                isHolding = true;
                isResizing = false;
                holdStartTime = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                target.CaptureMouse();
                evt.StopPropagation();
            }
        }
        
        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (target.HasMouseCapture())
            {
                Vector2 delta = evt.mousePosition - startMousePosition;
                long currentTime = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                long holdDuration = currentTime - holdStartTime;
                
                // Check if user moved too much before hold completed
                float movementDistance = delta.magnitude;
                if (isHolding && !isResizing)
                {
                    // If moved more than 5px before hold completes, cancel
                    if (movementDistance > 5f && holdDuration < HOLD_DURATION_MS)
                    {
                        target.ReleaseMouse();
                        isHolding = false;
                        return;
                    }
                    
                    // If held long enough, start resizing
                    if (holdDuration >= HOLD_DURATION_MS)
                    {
                        isHolding = false;
                        isResizing = true;
                        // Visual feedback that resize is active
                        target.style.backgroundColor = new Color(0.2f, 0.6f, 0.85f, 0.1f);
                    }
                }
                
                if (isResizing)
                {
                    float newWidth = Mathf.Clamp(startWidth + delta.x, columnData.minWidth, columnData.maxWidth);
                    
                    columnData.width = newWidth;
                    onResize?.Invoke(newWidth);
                    
                    evt.StopPropagation();
                }
            }
        }
        
        private void OnMouseUp(MouseUpEvent evt)
        {
            if (target.HasMouseCapture() && evt.button == 0)
            {
                bool wasResizing = isResizing;
                isResizing = false;
                isHolding = false;
                target.ReleaseMouse();
                
                // Clear visual feedback
                target.style.backgroundColor = Color.clear;
                
                // If we were resizing, stop propagation so the click doesn't trigger sort
                if (wasResizing)
                {
                    evt.StopPropagation();
                }
                // Otherwise, let the click through for sorting
            }
        }
    }
}
