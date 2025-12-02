using Microsoft.EntityFrameworkCore;
using ProductAdminPortal.Data;
using ProductAdminPortal.DTOs;
using ProductAdminPortal.Models.Domain;
using System.Text.Json;

namespace ProductAdminPortal.Services;

public class PricingService : IPricingService
{
    private readonly ProductAdminDbContext _context;

    public PricingService(ProductAdminDbContext context)
    {
        _context = context;
    }

    public async Task<PricingCalculationResponse> CalculatePricingAsync(PricingCalculationRequest request, CancellationToken cancellationToken = default)
    {
        var subscription = await _context.CustomerSubscriptions
            .Include(cs => cs.SubscriptionPlan)
                .ThenInclude(sp => sp.PricingRules)
                .ThenInclude(pr => pr.PricingTiers)
            .Include(cs => cs.SubscriptionPlan)
                .ThenInclude(sp => sp.PricingRules)
                .ThenInclude(pr => pr.ComplexityMultipliers)
            .FirstOrDefaultAsync(cs => cs.Id == request.CustomerSubscriptionId, cancellationToken);

        if (subscription == null)
            throw new KeyNotFoundException($"Customer subscription {request.CustomerSubscriptionId} not found");

        var lineItems = new List<LineItemResponse>();
        decimal subtotal = 0;

        // Add base subscription fee
        var baseFee = subscription.SubscriptionPlan.Price;
        lineItems.Add(new LineItemResponse(
            $"{subscription.SubscriptionPlan.Name} - Base Fee",
            1,
            baseFee,
            baseFee,
            new { Type = "base_fee" }
        ));
        subtotal += baseFee;

        // Group usage by entity
        var usageByEntity = request.UsageRecords.GroupBy(ur => ur.EntityId);

        foreach (var entityGroup in usageByEntity)
        {
            var entity = await _context.Entities
                .FirstOrDefaultAsync(e => e.Id == entityGroup.Key, cancellationToken);

            if (entity == null) continue;

            // Get pricing rules for this entity
            var pricingRule = subscription.SubscriptionPlan.PricingRules
                .FirstOrDefault(pr => pr.RuleType == "tiered" && 
                                     pr.Configuration != null && 
                                     pr.Configuration.Contains(entity.Id.ToString()));

            if (pricingRule?.PricingTiers != null && pricingRule.PricingTiers.Any())
            {
                // Apply tiered pricing with complexity multipliers
                var complexityGroups = entityGroup.GroupBy(ur => ur.Complexity ?? "low");

                foreach (var complexityGroup in complexityGroups)
                {
                    var complexity = complexityGroup.Key;
                    var totalUnits = complexityGroup.Sum(ur => ur.Units);

                    var multiplier = GetComplexityMultiplier(pricingRule, complexity);
                    var amount = CalculateTieredPrice(pricingRule.PricingTiers.OrderBy(t => t.MinUnits).ToList(), totalUnits, multiplier);

                    if (amount > 0)
                    {
                        lineItems.Add(new LineItemResponse(
                            $"{entity.Name} ({complexity} complexity)",
                            totalUnits,
                            amount / totalUnits,
                            amount,
                            new { EntityId = entity.Id, Complexity = complexity, Multiplier = multiplier }
                        ));
                        subtotal += amount;
                    }
                }
            }
            else
            {
                // Simple per-unit pricing
                var totalUnits = entityGroup.Sum(ur => ur.Units);
                var basePricing = string.IsNullOrEmpty(entity.BasePricing)
                    ? null
                    : JsonSerializer.Deserialize<Dictionary<string, object>>(entity.BasePricing);

                if (basePricing != null && basePricing.TryGetValue("unit_price", out var unitPriceObj))
                {
                    var unitPrice = Convert.ToDecimal(unitPriceObj.ToString());
                    var amount = totalUnits * unitPrice;

                    lineItems.Add(new LineItemResponse(
                        entity.Name,
                        totalUnits,
                        unitPrice,
                        amount,
                        new { EntityId = entity.Id, PricingType = "per_unit" }
                    ));
                    subtotal += amount;
                }
            }
        }

        // Calculate tax (18% GST)
        var taxAmount = Math.Round(subtotal * 0.18m, 2);
        var totalAmount = subtotal + taxAmount;

        return new PricingCalculationResponse(
            request.CustomerSubscriptionId,
            request.BillingPeriodStart,
            request.BillingPeriodEnd,
            subtotal,
            taxAmount,
            0, // No discount applied
            totalAmount,
            lineItems
        );
    }

    public async Task<PricingSimulationResponse> SimulatePricingAsync(PricingSimulationRequest request, CancellationToken cancellationToken = default)
    {
        var subscriptionPlan = await _context.SubscriptionPlans
            .Include(sp => sp.PricingRules)
                .ThenInclude(pr => pr.PricingTiers)
            .Include(sp => sp.PricingRules)
                .ThenInclude(pr => pr.ComplexityMultipliers)
            .FirstOrDefaultAsync(sp => sp.Id == request.SubscriptionPlanId, cancellationToken);

        if (subscriptionPlan == null)
            throw new KeyNotFoundException($"Subscription plan {request.SubscriptionPlanId} not found");

        var lineItems = new List<LineItemResponse>();
        var basePrice = subscriptionPlan.Price;
        decimal usageCharges = 0;

        // Add base fee to breakdown
        lineItems.Add(new LineItemResponse(
            $"{subscriptionPlan.Name} - Base Fee",
            1,
            basePrice,
            basePrice,
            new { Type = "base_fee" }
        ));

        // Calculate usage charges (similar to actual calculation)
        var usageByEntity = request.UsageRecords.GroupBy(ur => ur.EntityId);

        foreach (var entityGroup in usageByEntity)
        {
            var entity = await _context.Entities
                .FirstOrDefaultAsync(e => e.Id == entityGroup.Key, cancellationToken);

            if (entity == null) continue;

            var pricingRule = subscriptionPlan.PricingRules
                .FirstOrDefault(pr => pr.RuleType == "tiered" && 
                                     pr.Configuration != null && 
                                     pr.Configuration.Contains(entity.Id.ToString()));

            if (pricingRule?.PricingTiers != null && pricingRule.PricingTiers.Any())
            {
                var complexityGroups = entityGroup.GroupBy(ur => ur.Complexity ?? "low");

                foreach (var complexityGroup in complexityGroups)
                {
                    var complexity = complexityGroup.Key;
                    var totalUnits = complexityGroup.Sum(ur => ur.Units);
                    var multiplier = GetComplexityMultiplier(pricingRule, complexity);
                    var amount = CalculateTieredPrice(pricingRule.PricingTiers.OrderBy(t => t.MinUnits).ToList(), totalUnits, multiplier);

                    if (amount > 0)
                    {
                        lineItems.Add(new LineItemResponse(
                            $"{entity.Name} ({complexity})",
                            totalUnits,
                            amount / totalUnits,
                            amount,
                            new { Complexity = complexity }
                        ));
                        usageCharges += amount;
                    }
                }
            }
        }

        var estimatedTotal = basePrice + usageCharges + Math.Round((basePrice + usageCharges) * 0.18m, 2);

        return new PricingSimulationResponse(
            request.SubscriptionPlanId,
            subscriptionPlan.Name,
            basePrice,
            usageCharges,
            estimatedTotal,
            lineItems
        );
    }

    private decimal CalculateTieredPrice(List<PricingTier> tiers, int totalUnits, decimal multiplier)
    {
        decimal total = 0;
        int remainingUnits = totalUnits;

        foreach (var tier in tiers)
        {
            if (remainingUnits <= 0) break;

            var tierStart = tier.MinUnits;
            var tierEnd = tier.MaxUnits ?? int.MaxValue;
            var unitsInTier = Math.Min(remainingUnits, tierEnd - tierStart + 1);

            if (unitsInTier > 0)
            {
                total += unitsInTier * tier.UnitPrice * multiplier;
                remainingUnits -= unitsInTier;
            }
        }

        return Math.Round(total, 2);
    }

    private decimal GetComplexityMultiplier(PricingRule pricingRule, string complexity)
    {
        var complexityMultiplier = pricingRule.ComplexityMultipliers
            ?.FirstOrDefault(cm => cm.ComplexityLevel.Equals(complexity, StringComparison.OrdinalIgnoreCase));

        return complexityMultiplier?.Multiplier ?? 1.0m;
    }
}
