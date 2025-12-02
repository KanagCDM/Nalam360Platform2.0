# Pricing Calculation Engine - Complete Implementation

## Overview

This document provides the complete pricing calculation algorithm with a fully worked numeric example, including step-by-step calculations and unit tests.

---

## Pricing Calculation Example

### Scenario Setup

**Subscription Plan:** Gold Plan
- **Base Fee:** ₹1,000/month
- **Billing Cycle:** Monthly
- **Effective Period:** November 2025

**Entity:** Patient Registration (entity-001)
- **Pricing Model:** Tiered with complexity multipliers
- **Included in Base:** 1,000 transactions

**Tiered Pricing Structure:**
```
Tier 0: 0 - 1,000 transactions = ₹0.00/txn (included in base fee)
Tier 1: 1,001 - 10,000 transactions = ₹0.10/txn
Tier 2: >10,000 transactions = ₹0.07/txn
```

**Complexity Multipliers:**
```
Low = 1.0x
Medium = 2.0x
High = 4.0x
```

**Customer Usage for November 2025:**
- Total Transactions: 12,500
  - Low Complexity: 8,000 transactions
  - Medium Complexity: 3,000 transactions
  - High Complexity: 1,500 transactions

---

## Step-by-Step Calculation

### Step 1: Categorize Usage by Tier and Complexity

We need to distribute 12,500 total transactions across tiers:

**Tier 0 (0-1,000): Included in base fee**
- First 1,000 transactions are free regardless of complexity
- We'll allocate from low complexity first:
  - Low: 1,000 transactions @ ₹0.00 = ₹0.00

**Remaining after Tier 0:**
- Low: 8,000 - 1,000 = 7,000 remaining
- Medium: 3,000 remaining
- High: 1,500 remaining
- **Total remaining:** 11,500 transactions

**Tier 1 (1,001-10,000): ₹0.10/txn base**
- This tier can hold 9,000 transactions (10,000 - 1,000)
- Allocate from remaining:
  - Low: 7,000 transactions @ ₹0.10 × 1.0 = ₹700.00
  - Medium: 2,000 transactions @ ₹0.10 × 2.0 = ₹400.00
  - **Subtotal for Tier 1:** 9,000 transactions

**Remaining after Tier 1:**
- Low: 0 (all used)
- Medium: 3,000 - 2,000 = 1,000 remaining
- High: 1,500 remaining
- **Total remaining:** 2,500 transactions

**Tier 2 (>10,000): ₹0.07/txn base**
- Unlimited tier
- Allocate remaining:
  - Medium: 1,000 transactions @ ₹0.07 × 2.0 = ₹140.00
  - High: 1,500 transactions @ ₹0.07 × 4.0 = ₹420.00
  - **Subtotal for Tier 2:** 2,500 transactions

---

### Step 2: Calculate Line Items

| Line Item | Quantity | Unit Price | Total |
|-----------|----------|------------|-------|
| **Base Fee** | 1 | ₹1,000.00 | ₹1,000.00 |
| **Patient Registration - Low (Tier 0)** | 1,000 | ₹0.00 | ₹0.00 |
| **Patient Registration - Low (Tier 1)** | 7,000 | ₹0.10 | ₹700.00 |
| **Patient Registration - Medium (Tier 1)** | 2,000 | ₹0.20 | ₹400.00 |
| **Patient Registration - Medium (Tier 2)** | 1,000 | ₹0.14 | ₹140.00 |
| **Patient Registration - High (Tier 2)** | 1,500 | ₹0.28 | ₹420.00 |
| **Subtotal** | | | ₹2,660.00 |
| **Tax (18% GST)** | | | ₹478.80 |
| **Discount** | | | ₹0.00 |
| **Total Amount** | | | ₹3,138.80 |

---

### Step 3: Detailed Breakdown

#### Base Fee
```
Description: Gold Plan - Monthly Base Fee
Quantity: 1
Unit Price: ₹1,000.00
Total: ₹1,000.00
```

#### Usage Charges

**Low Complexity Transactions:**
```
Tier 0 (Included):
  Quantity: 1,000
  Unit Price: ₹0.00
  Calculation: 1,000 × ₹0.00 × 1.0 = ₹0.00

Tier 1 (1,001-10,000):
  Quantity: 7,000
  Base Price: ₹0.10
  Complexity Multiplier: 1.0
  Unit Price: ₹0.10 × 1.0 = ₹0.10
  Calculation: 7,000 × ₹0.10 = ₹700.00
  Total Low: ₹700.00
```

**Medium Complexity Transactions:**
```
Tier 1 (1,001-10,000):
  Quantity: 2,000
  Base Price: ₹0.10
  Complexity Multiplier: 2.0
  Unit Price: ₹0.10 × 2.0 = ₹0.20
  Calculation: 2,000 × ₹0.20 = ₹400.00

Tier 2 (>10,000):
  Quantity: 1,000
  Base Price: ₹0.07
  Complexity Multiplier: 2.0
  Unit Price: ₹0.07 × 2.0 = ₹0.14
  Calculation: 1,000 × ₹0.14 = ₹140.00
  Total Medium: ₹540.00
```

**High Complexity Transactions:**
```
Tier 2 (>10,000):
  Quantity: 1,500
  Base Price: ₹0.07
  Complexity Multiplier: 4.0
  Unit Price: ₹0.07 × 4.0 = ₹0.28
  Calculation: 1,500 × ₹0.28 = ₹420.00
  Total High: ₹420.00
```

#### Summary
```
Base Fee:              ₹1,000.00
Usage Charges:         ₹1,660.00
  - Low:                 ₹700.00
  - Medium:              ₹540.00
  - High:                ₹420.00
Subtotal:              ₹2,660.00
Tax (18%):               ₹478.80
Total:                 ₹3,138.80
```

---

## Complete TypeScript Implementation

```typescript
// types.ts
export interface ComplexityBreakdown {
  low: number;
  medium: number;
  high: number;
  critical?: number;
}

export interface PricingTier {
  minUnits: number;
  maxUnits: number | null; // null = unlimited
  unitPrice: number;
  flatFee?: number;
}

export interface ComplexityMultiplier {
  low: number;
  medium: number;
  high: number;
  critical?: number;
}

export interface EntityPricingConfig {
  entityId: string;
  entityName: string;
  includedUnits: number;
  tiers: PricingTier[];
  complexityMultipliers: ComplexityMultiplier;
}

export interface SubscriptionPlanConfig {
  planId: string;
  planName: string;
  baseFee: number;
  currencyCode: string;
  entities: EntityPricingConfig[];
}

export interface UsageInput {
  entityId: string;
  totalUnits: number;
  complexityBreakdown: ComplexityBreakdown;
}

export interface LineItem {
  description: string;
  entityId: string | null;
  itemType: 'base_fee' | 'usage' | 'tax' | 'discount';
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  metadata?: {
    tier?: string;
    complexity?: string;
    multiplier?: number;
  };
}

export interface InvoiceCalculation {
  subscriptionPlan: {
    id: string;
    name: string;
    baseFee: number;
  };
  lineItems: LineItem[];
  subtotal: number;
  taxAmount: number;
  discountAmount: number;
  totalAmount: number;
  currencyCode: string;
}

// pricing-engine.ts
export class PricingEngine {
  /**
   * Main calculation function
   * Calculates invoice for a subscription based on usage
   */
  calculateInvoice(
    subscription: SubscriptionPlanConfig,
    usage: UsageInput[],
    taxRate: number = 0.18, // 18% GST
    discountAmount: number = 0
  ): InvoiceCalculation {
    const lineItems: LineItem[] = [];

    // 1. Add base fee
    lineItems.push({
      description: `${subscription.planName} - Monthly Base Fee`,
      entityId: null,
      itemType: 'base_fee',
      quantity: 1,
      unitPrice: subscription.baseFee,
      totalPrice: subscription.baseFee,
    });

    // 2. Calculate usage charges for each entity
    for (const usageRecord of usage) {
      const entityConfig = subscription.entities.find(
        (e) => e.entityId === usageRecord.entityId
      );

      if (!entityConfig) {
        throw new Error(`Entity ${usageRecord.entityId} not found in subscription plan`);
      }

      const entityLineItems = this.calculateEntityUsage(
        entityConfig,
        usageRecord.totalUnits,
        usageRecord.complexityBreakdown
      );

      lineItems.push(...entityLineItems);
    }

    // 3. Calculate subtotal
    const subtotal = lineItems.reduce((sum, item) => sum + item.totalPrice, 0);

    // 4. Calculate tax
    const taxAmount = subtotal * taxRate;
    lineItems.push({
      description: `Tax (${taxRate * 100}% GST)`,
      entityId: null,
      itemType: 'tax',
      quantity: 1,
      unitPrice: taxAmount,
      totalPrice: taxAmount,
    });

    // 5. Apply discount
    if (discountAmount > 0) {
      lineItems.push({
        description: 'Discount',
        entityId: null,
        itemType: 'discount',
        quantity: 1,
        unitPrice: -discountAmount,
        totalPrice: -discountAmount,
      });
    }

    // 6. Calculate total
    const totalAmount = subtotal + taxAmount - discountAmount;

    return {
      subscriptionPlan: {
        id: subscription.planId,
        name: subscription.planName,
        baseFee: subscription.baseFee,
      },
      lineItems,
      subtotal,
      taxAmount,
      discountAmount,
      totalAmount,
      currencyCode: subscription.currencyCode,
    };
  }

  /**
   * Calculate usage charges for a single entity with tiered pricing and complexity
   */
  private calculateEntityUsage(
    entityConfig: EntityPricingConfig,
    totalUnits: number,
    complexityBreakdown: ComplexityBreakdown
  ): LineItem[] {
    const lineItems: LineItem[] = [];

    // Process each complexity level
    const complexityLevels: (keyof ComplexityBreakdown)[] = ['low', 'medium', 'high', 'critical'];
    let remainingUnitsToAllocate: Record<string, number> = { ...complexityBreakdown };

    // Sort tiers by minUnits
    const sortedTiers = [...entityConfig.tiers].sort((a, b) => a.minUnits - b.minUnits);

    // Track how many units we've allocated across all complexity levels
    let totalAllocated = 0;

    for (const tier of sortedTiers) {
      const tierCapacity = tier.maxUnits ? tier.maxUnits - tier.minUnits : Infinity;
      const unitsAvailableInTier = Math.min(
        tierCapacity,
        totalUnits - totalAllocated
      );

      if (unitsAvailableInTier <= 0) break;

      // Allocate units to this tier by complexity level
      for (const complexity of complexityLevels) {
        const unitsOfThisComplexity = remainingUnitsToAllocate[complexity] || 0;
        if (unitsOfThisComplexity === 0) continue;

        const unitsToAllocate = Math.min(unitsOfThisComplexity, unitsAvailableInTier - totalAllocated);
        if (unitsToAllocate <= 0) continue;

        const multiplier = entityConfig.complexityMultipliers[complexity] || 1.0;
        const unitPrice = tier.unitPrice * multiplier;
        const totalPrice = unitsToAllocate * unitPrice;

        const tierLabel = tier.maxUnits
          ? `${tier.minUnits + 1}-${tier.maxUnits}`
          : `>${tier.minUnits}`;

        lineItems.push({
          description: `${entityConfig.entityName} - ${complexity.charAt(0).toUpperCase() + complexity.slice(1)} Complexity (Tier: ${tierLabel})`,
          entityId: entityConfig.entityId,
          itemType: 'usage',
          quantity: unitsToAllocate,
          unitPrice: unitPrice,
          totalPrice: totalPrice,
          metadata: {
            tier: tierLabel,
            complexity: complexity,
            multiplier: multiplier,
          },
        });

        remainingUnitsToAllocate[complexity] -= unitsToAllocate;
        totalAllocated += unitsToAllocate;
      }
    }

    return lineItems;
  }

  /**
   * Simplified tier allocation algorithm
   */
  private allocateUnitsToTiers(
    totalUnits: number,
    tiers: PricingTier[]
  ): { tier: PricingTier; units: number }[] {
    const allocations: { tier: PricingTier; units: number }[] = [];
    let remaining = totalUnits;

    for (const tier of tiers) {
      const tierCapacity = tier.maxUnits ? tier.maxUnits - tier.minUnits : Infinity;
      const unitsInTier = Math.min(remaining, tierCapacity);

      if (unitsInTier > 0) {
        allocations.push({ tier, units: unitsInTier });
        remaining -= unitsInTier;
      }

      if (remaining === 0) break;
    }

    return allocations;
  }
}

// pricing-engine.test.ts
import { describe, it, expect } from '@jest/globals';
import { PricingEngine } from './pricing-engine';
import { SubscriptionPlanConfig, UsageInput } from './types';

describe('PricingEngine', () => {
  const engine = new PricingEngine();

  const goldPlanConfig: SubscriptionPlanConfig = {
    planId: 'plan-gold-001',
    planName: 'Gold Plan',
    baseFee: 1000.00,
    currencyCode: 'INR',
    entities: [
      {
        entityId: 'entity-001',
        entityName: 'Patient Registration',
        includedUnits: 1000,
        tiers: [
          { minUnits: 0, maxUnits: 1000, unitPrice: 0.00 },
          { minUnits: 1000, maxUnits: 10000, unitPrice: 0.10 },
          { minUnits: 10000, maxUnits: null, unitPrice: 0.07 },
        ],
        complexityMultipliers: {
          low: 1.0,
          medium: 2.0,
          high: 4.0,
        },
      },
    ],
  };

  it('should calculate invoice for Gold Plan example correctly', () => {
    const usage: UsageInput[] = [
      {
        entityId: 'entity-001',
        totalUnits: 12500,
        complexityBreakdown: {
          low: 8000,
          medium: 3000,
          high: 1500,
        },
      },
    ];

    const invoice = engine.calculateInvoice(goldPlanConfig, usage, 0.18, 0);

    // Expected calculations:
    // Base Fee: ₹1,000.00
    // Tier 0 (0-1,000): 1,000 low @ ₹0.00 = ₹0.00
    // Tier 1 (1,001-10,000): 
    //   - 7,000 low @ ₹0.10 = ₹700.00
    //   - 2,000 medium @ ₹0.20 = ₹400.00
    // Tier 2 (>10,000):
    //   - 1,000 medium @ ₹0.14 = ₹140.00
    //   - 1,500 high @ ₹0.28 = ₹420.00
    // Subtotal: ₹2,660.00
    // Tax (18%): ₹478.80
    // Total: ₹3,138.80

    expect(invoice.subscriptionPlan.baseFee).toBe(1000.00);
    expect(invoice.subtotal).toBe(2660.00);
    expect(invoice.taxAmount).toBeCloseTo(478.80, 2);
    expect(invoice.totalAmount).toBeCloseTo(3138.80, 2);
    expect(invoice.currencyCode).toBe('INR');

    // Verify line items
    const baseFeeItem = invoice.lineItems.find(item => item.itemType === 'base_fee');
    expect(baseFeeItem?.totalPrice).toBe(1000.00);

    const usageItems = invoice.lineItems.filter(item => item.itemType === 'usage');
    expect(usageItems.length).toBeGreaterThan(0);

    const totalUsageCharges = usageItems.reduce((sum, item) => sum + item.totalPrice, 0);
    expect(totalUsageCharges).toBe(1660.00);
  });

  it('should handle zero usage correctly', () => {
    const usage: UsageInput[] = [
      {
        entityId: 'entity-001',
        totalUnits: 500, // Below included limit
        complexityBreakdown: {
          low: 500,
          medium: 0,
          high: 0,
        },
      },
    ];

    const invoice = engine.calculateInvoice(goldPlanConfig, usage, 0.18, 0);

    // Only base fee and tax, no usage charges
    expect(invoice.subtotal).toBe(1000.00);
    expect(invoice.taxAmount).toBeCloseTo(180.00, 2);
    expect(invoice.totalAmount).toBeCloseTo(1180.00, 2);
  });

  it('should apply discounts correctly', () => {
    const usage: UsageInput[] = [
      {
        entityId: 'entity-001',
        totalUnits: 12500,
        complexityBreakdown: {
          low: 8000,
          medium: 3000,
          high: 1500,
        },
      },
    ];

    const discountAmount = 500.00;
    const invoice = engine.calculateInvoice(goldPlanConfig, usage, 0.18, discountAmount);

    expect(invoice.discountAmount).toBe(500.00);
    expect(invoice.totalAmount).toBeCloseTo(2638.80, 2); // 3138.80 - 500
  });

  it('should handle only high complexity usage', () => {
    const usage: UsageInput[] = [
      {
        entityId: 'entity-001',
        totalUnits: 5000,
        complexityBreakdown: {
          low: 0,
          medium: 0,
          high: 5000,
        },
      },
    ];

    const invoice = engine.calculateInvoice(goldPlanConfig, usage, 0.18, 0);

    // Tier 0: 1,000 high @ ₹0.00 = ₹0.00
    // Tier 1: 4,000 high @ ₹0.40 = ₹1,600.00
    // Total: ₹1,000 (base) + ₹1,600 (usage) = ₹2,600
    // Tax: ₹468
    // Total: ₹3,068

    expect(invoice.subtotal).toBe(2600.00);
    expect(invoice.totalAmount).toBeCloseTo(3068.00, 2);
  });
});
```

---

## C# Implementation (for .NET Backend)

```csharp
// Models/PricingModels.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProductAdminPortal.Models.Pricing
{
    public class ComplexityBreakdown
    {
        public int Low { get; set; }
        public int Medium { get; set; }
        public int High { get; set; }
        public int Critical { get; set; }
    }

    public class PricingTier
    {
        public int MinUnits { get; set; }
        public int? MaxUnits { get; set; } // null = unlimited
        public decimal UnitPrice { get; set; }
        public decimal FlatFee { get; set; }
    }

    public class ComplexityMultiplier
    {
        public decimal Low { get; set; } = 1.0m;
        public decimal Medium { get; set; } = 2.0m;
        public decimal High { get; set; } = 4.0m;
        public decimal Critical { get; set; } = 6.0m;
    }

    public class EntityPricingConfig
    {
        public Guid EntityId { get; set; }
        public string EntityName { get; set; }
        public int IncludedUnits { get; set; }
        public List<PricingTier> Tiers { get; set; }
        public ComplexityMultiplier ComplexityMultipliers { get; set; }
    }

    public class SubscriptionPlanConfig
    {
        public Guid PlanId { get; set; }
        public string PlanName { get; set; }
        public decimal BaseFee { get; set; }
        public string CurrencyCode { get; set; }
        public List<EntityPricingConfig> Entities { get; set; }
    }

    public class UsageInput
    {
        public Guid EntityId { get; set; }
        public int TotalUnits { get; set; }
        public ComplexityBreakdown ComplexityBreakdown { get; set; }
    }

    public class LineItem
    {
        public string Description { get; set; }
        public Guid? EntityId { get; set; }
        public string ItemType { get; set; } // base_fee, usage, tax, discount
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
    }

    public class InvoiceCalculation
    {
        public SubscriptionPlanSummary SubscriptionPlan { get; set; }
        public List<LineItem> LineItems { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string CurrencyCode { get; set; }
    }

    public class SubscriptionPlanSummary
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal BaseFee { get; set; }
    }
}

// Services/PricingEngine.cs
using ProductAdminPortal.Models.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProductAdminPortal.Services
{
    public class PricingEngine
    {
        /// <summary>
        /// Main calculation function
        /// Calculates invoice for a subscription based on usage
        /// </summary>
        public InvoiceCalculation CalculateInvoice(
            SubscriptionPlanConfig subscription,
            List<UsageInput> usage,
            decimal taxRate = 0.18m, // 18% GST
            decimal discountAmount = 0m)
        {
            var lineItems = new List<LineItem>();

            // 1. Add base fee
            lineItems.Add(new LineItem
            {
                Description = $"{subscription.PlanName} - Monthly Base Fee",
                EntityId = null,
                ItemType = "base_fee",
                Quantity = 1,
                UnitPrice = subscription.BaseFee,
                TotalPrice = subscription.BaseFee
            });

            // 2. Calculate usage charges for each entity
            foreach (var usageRecord in usage)
            {
                var entityConfig = subscription.Entities
                    .FirstOrDefault(e => e.EntityId == usageRecord.EntityId);

                if (entityConfig == null)
                    throw new ArgumentException($"Entity {usageRecord.EntityId} not found");

                var entityLineItems = CalculateEntityUsage(
                    entityConfig,
                    usageRecord.TotalUnits,
                    usageRecord.ComplexityBreakdown
                );

                lineItems.AddRange(entityLineItems);
            }

            // 3. Calculate subtotal
            var subtotal = lineItems.Sum(item => item.TotalPrice);

            // 4. Calculate tax
            var taxAmount = subtotal * taxRate;
            lineItems.Add(new LineItem
            {
                Description = $"Tax ({taxRate * 100:F0}% GST)",
                EntityId = null,
                ItemType = "tax",
                Quantity = 1,
                UnitPrice = taxAmount,
                TotalPrice = taxAmount
            });

            // 5. Apply discount
            if (discountAmount > 0)
            {
                lineItems.Add(new LineItem
                {
                    Description = "Discount",
                    EntityId = null,
                    ItemType = "discount",
                    Quantity = 1,
                    UnitPrice = -discountAmount,
                    TotalPrice = -discountAmount
                });
            }

            // 6. Calculate total
            var totalAmount = subtotal + taxAmount - discountAmount;

            return new InvoiceCalculation
            {
                SubscriptionPlan = new SubscriptionPlanSummary
                {
                    Id = subscription.PlanId,
                    Name = subscription.PlanName,
                    BaseFee = subscription.BaseFee
                },
                LineItems = lineItems,
                Subtotal = subtotal,
                TaxAmount = taxAmount,
                DiscountAmount = discountAmount,
                TotalAmount = totalAmount,
                CurrencyCode = subscription.CurrencyCode
            };
        }

        /// <summary>
        /// Calculate usage charges for a single entity with tiered pricing
        /// </summary>
        private List<LineItem> CalculateEntityUsage(
            EntityPricingConfig entityConfig,
            int totalUnits,
            ComplexityBreakdown complexityBreakdown)
        {
            var lineItems = new List<LineItem>();

            // Track remaining units by complexity
            var remainingUnits = new Dictionary<string, int>
            {
                ["low"] = complexityBreakdown.Low,
                ["medium"] = complexityBreakdown.Medium,
                ["high"] = complexityBreakdown.High,
                ["critical"] = complexityBreakdown.Critical
            };

            var sortedTiers = entityConfig.Tiers.OrderBy(t => t.MinUnits).ToList();
            var totalAllocated = 0;

            foreach (var tier in sortedTiers)
            {
                var tierCapacity = tier.MaxUnits.HasValue
                    ? tier.MaxUnits.Value - tier.MinUnits
                    : int.MaxValue;

                var complexities = new[] { "low", "medium", "high", "critical" };
                var multipliers = new Dictionary<string, decimal>
                {
                    ["low"] = entityConfig.ComplexityMultipliers.Low,
                    ["medium"] = entityConfig.ComplexityMultipliers.Medium,
                    ["high"] = entityConfig.ComplexityMultipliers.High,
                    ["critical"] = entityConfig.ComplexityMultipliers.Critical
                };

                foreach (var complexity in complexities)
                {
                    if (remainingUnits[complexity] == 0) continue;

                    var unitsToAllocate = Math.Min(
                        remainingUnits[complexity],
                        tierCapacity - totalAllocated
                    );

                    if (unitsToAllocate <= 0) continue;

                    var multiplier = multipliers[complexity];
                    var unitPrice = tier.UnitPrice * multiplier;
                    var totalPrice = unitsToAllocate * unitPrice;

                    var tierLabel = tier.MaxUnits.HasValue
                        ? $"{tier.MinUnits + 1}-{tier.MaxUnits.Value}"
                        : $">{tier.MinUnits}";

                    lineItems.Add(new LineItem
                    {
                        Description = $"{entityConfig.EntityName} - " +
                            $"{char.ToUpper(complexity[0]) + complexity.Substring(1)} " +
                            $"Complexity (Tier: {tierLabel})",
                        EntityId = entityConfig.EntityId,
                        ItemType = "usage",
                        Quantity = unitsToAllocate,
                        UnitPrice = unitPrice,
                        TotalPrice = totalPrice,
                        Metadata = new Dictionary<string, object>
                        {
                            ["tier"] = tierLabel,
                            ["complexity"] = complexity,
                            ["multiplier"] = multiplier
                        }
                    });

                    remainingUnits[complexity] -= unitsToAllocate;
                    totalAllocated += unitsToAllocate;
                }

                if (totalAllocated >= tierCapacity) break;
            }

            return lineItems;
        }
    }
}

// Tests/PricingEngineTests.cs
using Xunit;
using ProductAdminPortal.Services;
using ProductAdminPortal.Models.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProductAdminPortal.Tests
{
    public class PricingEngineTests
    {
        private readonly PricingEngine _engine = new PricingEngine();

        private SubscriptionPlanConfig GetGoldPlanConfig()
        {
            return new SubscriptionPlanConfig
            {
                PlanId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
                PlanName = "Gold Plan",
                BaseFee = 1000.00m,
                CurrencyCode = "INR",
                Entities = new List<EntityPricingConfig>
                {
                    new EntityPricingConfig
                    {
                        EntityId = Guid.Parse("entity-001"),
                        EntityName = "Patient Registration",
                        IncludedUnits = 1000,
                        Tiers = new List<PricingTier>
                        {
                            new PricingTier { MinUnits = 0, MaxUnits = 1000, UnitPrice = 0.00m },
                            new PricingTier { MinUnits = 1000, MaxUnits = 10000, UnitPrice = 0.10m },
                            new PricingTier { MinUnits = 10000, MaxUnits = null, UnitPrice = 0.07m }
                        },
                        ComplexityMultipliers = new ComplexityMultiplier
                        {
                            Low = 1.0m,
                            Medium = 2.0m,
                            High = 4.0m
                        }
                    }
                }
            };
        }

        [Fact]
        public void CalculateInvoice_GoldPlanExample_ReturnsCorrectTotal()
        {
            // Arrange
            var goldPlan = GetGoldPlanConfig();
            var usage = new List<UsageInput>
            {
                new UsageInput
                {
                    EntityId = Guid.Parse("entity-001"),
                    TotalUnits = 12500,
                    ComplexityBreakdown = new ComplexityBreakdown
                    {
                        Low = 8000,
                        Medium = 3000,
                        High = 1500
                    }
                }
            };

            // Act
            var invoice = _engine.CalculateInvoice(goldPlan, usage, 0.18m, 0m);

            // Assert
            Assert.Equal(1000.00m, invoice.SubscriptionPlan.BaseFee);
            Assert.Equal(2660.00m, invoice.Subtotal);
            Assert.Equal(478.80m, invoice.TaxAmount, 2);
            Assert.Equal(3138.80m, invoice.TotalAmount, 2);
            Assert.Equal("INR", invoice.CurrencyCode);

            var baseFee = invoice.LineItems.First(i => i.ItemType == "base_fee");
            Assert.Equal(1000.00m, baseFee.TotalPrice);

            var usageCharges = invoice.LineItems
                .Where(i => i.ItemType == "usage")
                .Sum(i => i.TotalPrice);
            Assert.Equal(1660.00m, usageCharges);
        }

        [Fact]
        public void CalculateInvoice_WithDiscount_AppliesCorrectly()
        {
            // Arrange
            var goldPlan = GetGoldPlanConfig();
            var usage = new List<UsageInput>
            {
                new UsageInput
                {
                    EntityId = Guid.Parse("entity-001"),
                    TotalUnits = 12500,
                    ComplexityBreakdown = new ComplexityBreakdown
                    {
                        Low = 8000,
                        Medium = 3000,
                        High = 1500
                    }
                }
            };

            // Act
            var invoice = _engine.CalculateInvoice(goldPlan, usage, 0.18m, 500.00m);

            // Assert
            Assert.Equal(500.00m, invoice.DiscountAmount);
            Assert.Equal(2638.80m, invoice.TotalAmount, 2);
        }
    }
}
```

---

## Summary

**Final Invoice for Gold Plan with 12,500 Patient Registration Transactions:**

| Component | Amount |
|-----------|--------|
| Base Fee | ₹1,000.00 |
| Usage Charges (Low) | ₹700.00 |
| Usage Charges (Medium) | ₹540.00 |
| Usage Charges (High) | ₹420.00 |
| **Subtotal** | **₹2,660.00** |
| Tax (18% GST) | ₹478.80 |
| **Total Amount** | **₹3,138.80** |

This implementation provides:
- ✅ Complete TypeScript and C# code
- ✅ Tiered pricing support
- ✅ Complexity-based multipliers
- ✅ Included units handling
- ✅ Tax calculation
- ✅ Discount application
- ✅ Comprehensive unit tests
- ✅ Step-by-step numeric example
