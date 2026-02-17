# UI Dialogs Directory

This directory contains all standalone dialog UXML files. Each dialog is in its own separate file, making it easy to edit in Unity's UI Builder.

## 📋 Dialog Files

### Employee System Dialogs
- **HireDialog.uxml** - Employee hiring dialog with role selection and candidate preview
- **EmployeeDetailDialog.uxml** - Employee details view with skills, morale, and fire button

### Product System Dialogs
- **StartProductDialog.uxml** - Start new product dialog with category and employee selection
- **ProductDetailDialog.uxml** - Product details view showing progress and revenue

### Contract System Dialogs
- **AcceptContractDialog.uxml** - Accept contract dialog with employee assignment
- **ContractDetailDialog.uxml** - Contract details view showing goals and progress

## ✏️ How to Edit Dialogs in UI Builder

1. **Open Unity Editor**
2. Navigate to `/Assets/UI/UIToolkit/UIDocuments/Dialogs/`
3. **Double-click any `.uxml` file** to open it in UI Builder
4. Edit the dialog layout, styling, and structure
5. **Save** the file (Ctrl+S / Cmd+S)
6. Changes will automatically apply to all panels using that dialog

## 🔗 How Dialogs Are Used

Dialogs use the **wrapper pattern** to ensure controllers can query their elements:

### Pattern Structure

**1. Dialog template file** (contains content only):
```xml
<!-- In /Dialogs/StartProductDialog.uxml -->
<ui:UXML>
    <ui:VisualElement class="dialog-box">
        <ui:VisualElement class="dialog-header">
            <ui:Label text="START NEW PRODUCT" class="dialog-title"/>
            <ui:Button name="close-dialog-btn" text="✕" class="close-btn"/>
        </ui:VisualElement>
        <ui:VisualElement class="dialog-content">
            <ui:TextField name="product-name-field"/>
            <!-- More fields... -->
        </ui:VisualElement>
    </ui:VisualElement>
</ui:UXML>
```

**2. Parent panel file** (provides named wrapper):
```xml
<!-- In ProductPanel.uxml -->
<ui:Template name="StartProductDialog" src=".../Dialogs/StartProductDialog.uxml"/>

<ui:VisualElement name="start-product-dialog" class="dialog-overlay hidden">
    <ui:Instance template="StartProductDialog"/>
</ui:VisualElement>
```

**3. Controller queries** (finds wrapper and children):
```csharp
// Query the named wrapper
var dialog = panel.Q<VisualElement>("start-product-dialog");

// Query elements inside the template
var nameField = dialog.Q<TextField>("product-name-field");  // ✅ Works!
```

### Why This Pattern?

**❌ Without wrapper (broken):**
```xml
<!-- Dialog template has name on root -->
<ui:VisualElement name="start-product-dialog">
    <!-- Content -->
</ui:VisualElement>

<!-- Panel just instantiates -->
<ui:Instance template="StartProductDialog"/>

<!-- Controller can't find it! -->
var dialog = panel.Q("start-product-dialog");  // ❌ Returns null
```

**✅ With wrapper (works):**
```xml
<!-- Dialog template has NO name on root -->
<ui:VisualElement class="dialog-box">
    <!-- Content -->
</ui:VisualElement>

<!-- Panel provides named wrapper -->
<ui:VisualElement name="start-product-dialog">
    <ui:Instance template="StartProductDialog"/>
</ui:VisualElement>

<!-- Controller finds it! -->
var dialog = panel.Q("start-product-dialog");  // ✅ Found!
```

## ✅ Benefits of This Structure

### 1. Easy Editing
- Each dialog can be opened and edited independently in UI Builder
- No need to navigate through nested elements in the parent panel

### 2. Reusability
- Dialogs can be reused across multiple panels if needed
- Changes to a dialog file automatically apply everywhere it's used

### 3. Organization
- Clear separation of concerns: Panels contain layout, Dialogs contain interactive UI
- Easy to find and modify specific dialogs

### 4. Version Control
- Each dialog is a separate file, making Git diffs cleaner
- Easier to track changes to individual dialogs

### 5. Performance
- Templates are loaded once and instantiated as needed
- No duplication of UXML code

## 🎨 Styling

All dialogs inherit styles from:
- **Common.uss** - Base styles for all UI elements
- **HireDialog.uss** - Specific styles for the hire dialog
- **ProductsContracts.uss** - Specific styles for product/contract dialogs

## 📝 Naming Conventions

Dialog files follow this pattern:
- **[SystemName][Purpose]Dialog.uxml**

Examples:
- `EmployeeDetailDialog.uxml`
- `StartProductDialog.uxml`
- `AcceptContractDialog.uxml`

## 🚀 Adding New Dialogs

To add a new dialog:

1. **Create a new `.uxml` file** in this directory
2. **Add required styles** at the top:
   ```xml
   <Style src="/Assets/UI/UIToolkit/Styles/Common.uss"/>
   ```
3. **Create the dialog structure**:
   ```xml
   <ui:VisualElement name="my-dialog" class="dialog-overlay hidden">
       <ui:VisualElement class="dialog-box">
           <!-- Dialog content here -->
       </ui:VisualElement>
   </ui:VisualElement>
   ```
4. **Reference it in the parent panel**:
   ```xml
   <ui:Template name="MyDialog" src=".../MyDialog.uxml"/>
   <ui:Instance template="MyDialog"/>
   ```
5. **Wire up the controller** to show/hide the dialog

## 🔍 Finding Dialog References

To find where a dialog is used:

1. Open the dialog `.uxml` file
2. Note the root element `name` attribute
3. Search for that name in C# controller scripts

Example:
- Dialog: `name="start-product-dialog"`
- Controller: Search for `"start-product-dialog"` in `ProductPanelController.cs`

&nbsp;
