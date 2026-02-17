# ProductSystem

This folder contains product development, quality calculation, and revenue generation logic.

## What Goes Here

### Core Scripts
- `ProductSystem.cs` - Main system managing product development and revenue
- `ProductData.cs` - Product data structure (progress, quality, revenue, etc.)
- `ProductEvents.cs` - All product-related events

### UI Subfolder
- `UI/ProductPanel.cs` - Main product list panel
- `UI/NewProductDialog.cs` - Dialog for starting new products
- `UI/ProductDetailView.cs` - Detailed product information
- `UI/ProductCard.cs` - Individual product display

## Documentation

See [Product System](/Pages/Systems/Product System.md) for full implementation details.

## Dependencies

- TimeSystem (for daily progress and monthly revenue)
- EmployeeSystem (for employee productivity)
- EventBus (for event communication)
- ProductCategory ScriptableObjects (in `/Assets/Data/ProductCategories/`)

## Status

- [ ] Not yet implemented
