using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.DropDowns;
using Nalam360Enterprise.UI.Core.Security;
using Nalam360Enterprise.UI.Core.AI.Services;
using Nalam360Enterprise.UI.Core.AI.Models;
using System.Collections.Generic;

namespace Nalam360Enterprise.UI.Components.AI;

public partial class N360IntelligentSearch<TItem>
{
    private SfAutoComplete<string, SearchResultItem<TItem>>? _autoComplete;
    private bool _showAdvancedPanel;

    #region Injected Services
    
    [Inject] private IAIService? AIService { get; set; }
    [Inject] private IAIComplianceService? ComplianceService { get; set; }
    [Inject] private IMLModelService? MLModelService { get; set; }
    
    #endregion

    #region Parameters

    [Parameter] public string Placeholder { get; set; } = "Search with AI...";
    [Parameter] public List<TItem>? DataSource { get; set; }
    [Parameter] public Func<TItem, string>? DisplayExpression { get; set; }
    [Parameter] public Func<TItem, string>? SearchExpression { get; set; }
    [Parameter] public Func<TItem, string>? DescriptionExpression { get; set; }
    [Parameter] public Func<TItem, string>? TypeExpression { get; set; }

    // AI Features
    [Parameter] public bool EnableAISearch { get; set; } = true;
    [Parameter] public bool EnableSemanticSearch { get; set; } = true;
    [Parameter] public bool EnableFuzzyMatch { get; set; } = true;
    [Parameter] public bool SearchInContent { get; set; } = true;
    [Parameter] public bool ShowRelevanceScore { get; set; } = true;
    [Parameter] public bool ShowRecentSearches { get; set; } = true;
    [Parameter] public bool ShowAdvancedOptions { get; set; } = true;
    [Parameter] public int MaxResults { get; set; } = 10;
    [Parameter] public double MinRelevanceScore { get; set; } = 0.3;
    
    // AI Integration
    [Parameter] public bool UseRealAI { get; set; } = false;
    [Parameter] public string? AIModelEndpoint { get; set; }
    [Parameter] public string? AIApiKey { get; set; }
    [Parameter] public bool EnablePHIDetection { get; set; } = true;
    [Parameter] public string? UserId { get; set; }

    // RBAC (N360 pattern)
    [Parameter] public string? RequiredPermission { get; set; }
    [Parameter] public bool HideIfNoPermission { get; set; }
    [Parameter] public bool EnableAudit { get; set; }
    [Parameter] public string AuditResource { get; set; } = "IntelligentSearch";
    [Parameter] public string AuditAction { get; set; } = "Search";

    // Styling
    [Parameter] public string CssClass { get; set; } = string.Empty;
    [Parameter] public string Style { get; set; } = string.Empty;
    [Parameter] public bool IsRtl { get; set; }

    // Callbacks
    [Parameter] public EventCallback<SearchResultItem<TItem>> OnResultSelected { get; set; }
    [Parameter] public EventCallback<List<SearchResultItem<TItem>>> OnSearchComplete { get; set; }

    #endregion

    #region State

    private List<SearchResultItem<TItem>> SearchResults { get; set; } = new();
    private List<string> RecentSearches { get; set; } = new();
    private string? CurrentQuery { get; set; }

    #endregion

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        if (!string.IsNullOrEmpty(RequiredPermission))
        {
            var hasPermission = await PermissionService.HasPermissionAsync(RequiredPermission);
            if (!hasPermission && HideIfNoPermission)
                return;
        }
    }

    private async Task OnFilteringHandler(FilteringEventArgs args)
    {
        CurrentQuery = args.Text;
        
        if (string.IsNullOrWhiteSpace(args.Text) || DataSource == null)
        {
            SearchResults = new();
            return;
        }

        // Track recent searches
        if (!RecentSearches.Contains(args.Text))
        {
            RecentSearches.Insert(0, args.Text);
            if (RecentSearches.Count > 10)
                RecentSearches.RemoveAt(10);
        }

        SearchResults = await PerformIntelligentSearch(args.Text);
        await OnSearchComplete.InvokeAsync(SearchResults);

        if (EnableAudit)
        {
            await AuditService.LogAsync(new AuditMetadata
            {
                Action = "IntelligentSearch",
                Resource = AuditResource,
                AdditionalData = new Dictionary<string, object>
                {
                    ["Query"] = args.Text,
                    ["ResultCount"] = SearchResults.Count
                }
            });
        }
    }

    private async Task<List<SearchResultItem<TItem>>> PerformIntelligentSearch(string query)
    {
        var results = new List<SearchResultItem<TItem>>();
        if (DataSource == null) return results;

        var normalizedQuery = query.ToLower().Trim();

        foreach (var item in DataSource)
        {
            var displayText = DisplayExpression?.Invoke(item) ?? item?.ToString() ?? "";
            var searchText = SearchExpression?.Invoke(item) ?? displayText;
            
            double score = 0;

            // Exact match
            if (searchText.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                score = 1.0;
            }
            // Semantic similarity
            else if (EnableSemanticSearch)
            {
                score = CalculateSemanticSimilarity(normalizedQuery, searchText.ToLower());
            }
            // Fuzzy matching
            else if (EnableFuzzyMatch)
            {
                score = CalculateFuzzyScore(normalizedQuery, searchText.ToLower());
            }

            if (score >= MinRelevanceScore)
            {
                results.Add(new SearchResultItem<TItem>
                {
                    Item = item,
                    DisplayText = displayText,
                    Description = DescriptionExpression?.Invoke(item),
                    ResultType = TypeExpression?.Invoke(item) ?? "General",
                    RelevanceScore = Math.Min(score, 1.0)
                });
            }
        }

        return results.OrderByDescending(r => r.RelevanceScore).Take(MaxResults).ToList();
    }

    private double CalculateSemanticSimilarity(string query, string text)
    {
        var queryWords = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var textWords = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var matchCount = queryWords.Count(qw => textWords.Any(tw => tw.Contains(qw) || qw.Contains(tw)));
        return queryWords.Length > 0 ? (double)matchCount / queryWords.Length : 0;
    }

    private double CalculateFuzzyScore(string query, string text)
    {
        var minDistance = text.Split(' ').Min(word => LevenshteinDistance(query, word));
        var maxLength = Math.Max(query.Length, text.Length);
        return maxLength > 0 ? 1.0 - ((double)minDistance / maxLength) : 0;
    }

    private int LevenshteinDistance(string s, string t)
    {
        if (string.IsNullOrEmpty(s)) return t?.Length ?? 0;
        if (string.IsNullOrEmpty(t)) return s.Length;

        var d = new int[s.Length + 1, t.Length + 1];
        for (int i = 0; i <= s.Length; i++) d[i, 0] = i;
        for (int j = 0; j <= t.Length; j++) d[0, j] = j;

        for (int i = 1; i <= s.Length; i++)
        {
            for (int j = 1; j <= t.Length; j++)
            {
                var cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
            }
        }
        return d[s.Length, t.Length];
    }

    private async Task OnValueChangeHandler(ChangeEventArgs<string, SearchResultItem<TItem>> args)
    {
        if (args.ItemData != null)
        {
            await OnResultSelected.InvokeAsync(args.ItemData);
        }
    }

    private async Task ApplyRecentSearch(string query)
    {
        CurrentQuery = query;
        if (_autoComplete != null)
        {
            await _autoComplete.ShowPopupAsync();
        }
    }

    private Dictionary<string, object> GetHtmlAttributes()
    {
        var attributes = new Dictionary<string, object>();
        if (IsRtl)
        {
            attributes["dir"] = "rtl";
        }
        return attributes;
    }

    private string GetTypeIcon(string? type) => type switch
    {
        "Patient" => "👤",
        "Appointment" => "📅",
        "Document" => "📄",
        "Prescription" => "💊",
        "Lab Result" => "🔬",
        _ => "📋"
    };
}

public class SearchResultItem<TItem>
{
    public TItem? Item { get; set; }
    public string? DisplayText { get; set; }
    public string? Description { get; set; }
    public string? ResultType { get; set; }
    public double RelevanceScore { get; set; }
}
