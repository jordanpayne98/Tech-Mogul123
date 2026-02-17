# Product Category ScriptableObjects

This folder contains product type definitions for different categories of products.

## What Goes Here

ScriptableObject assets defining product categories with development requirements and revenue potential.

### Phase 1 Product Categories

Create these three product category assets:

**SoftwareProduct.asset**
- Category Name: Software Product
- Description: Desktop or web-based software application
- Development Days: 60
- Quality Importance: 0.8 (very important)
- Revenue Range: $5,000 - $20,000 monthly
- Market Size Multiplier: 1.0x (medium)
- Skill Weights: 50% Dev, 30% Design, 20% Marketing

**HardwareProduct.asset**
- Category Name: Hardware Product
- Description: Physical device or electronic product
- Development Days: 90
- Quality Importance: 0.9 (critical)
- Revenue Range: $10,000 - $40,000 monthly
- Market Size Multiplier: 1.5x (large)
- Skill Weights: 60% Dev, 25% Design, 15% Marketing

**ServiceProduct.asset**
- Category Name: Service Product
- Description: Cloud service or subscription-based offering
- Development Days: 30
- Quality Importance: 0.6 (medium)
- Revenue Range: $3,000 - $12,000 monthly
- Market Size Multiplier: 0.8x (small but recurring)
- Skill Weights: 30% Dev, 30% Design, 40% Marketing

## How to Create

1. Create `ProductCategorySO.cs` script (see Data Design documentation)
2. Right-click in this folder → Create → TechMogul → Product Category
3. Name the asset (e.g., `SoftwareProduct`)
4. Configure values in Inspector

## Documentation

See [Data Design](/Pages/Data Design.md) for ProductCategorySO script structure and detailed examples.

## Usage

ProductSystem uses these categories when player starts new product development. The category determines development time, revenue potential, and required skills.

## Status

- [x] ProductCategorySO script created ✓
- [ ] SoftwareProduct category - Ready to create (Right-click → Create → TechMogul → Product Category)
- [ ] HardwareProduct category - Ready to create (Right-click → Create → TechMogul → Product Category)
- [ ] ServiceProduct category - Ready to create (Right-click → Create → TechMogul → Product Category)

See [ScriptableObject Assets Guide](/Pages/ScriptableObject Assets Guide.md) for detailed creation instructions.
