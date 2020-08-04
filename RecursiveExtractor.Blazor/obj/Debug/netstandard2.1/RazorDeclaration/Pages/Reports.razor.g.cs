#pragma checksum "/Users/gabe/Documents/GitHub/RecursiveExtractor/RecursiveExtractor.Blazor/Pages/Reports.razor" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "69872461fdcb30119ea7e0a7ee73d36b97c76b09"
// <auto-generated/>
#pragma warning disable 1591
#pragma warning disable 0414
#pragma warning disable 0649
#pragma warning disable 0169

namespace RecursiveExtractor.Blazor.Pages
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
#nullable restore
#line 1 "/Users/gabe/Documents/GitHub/RecursiveExtractor/RecursiveExtractor.Blazor/_Imports.razor"
using System.Net.Http;

#line default
#line hidden
#nullable disable
#nullable restore
#line 2 "/Users/gabe/Documents/GitHub/RecursiveExtractor/RecursiveExtractor.Blazor/_Imports.razor"
using System.Net.Http.Json;

#line default
#line hidden
#nullable disable
#nullable restore
#line 3 "/Users/gabe/Documents/GitHub/RecursiveExtractor/RecursiveExtractor.Blazor/_Imports.razor"
using Microsoft.AspNetCore.Components.Forms;

#line default
#line hidden
#nullable disable
#nullable restore
#line 4 "/Users/gabe/Documents/GitHub/RecursiveExtractor/RecursiveExtractor.Blazor/_Imports.razor"
using Microsoft.AspNetCore.Components.Routing;

#line default
#line hidden
#nullable disable
#nullable restore
#line 5 "/Users/gabe/Documents/GitHub/RecursiveExtractor/RecursiveExtractor.Blazor/_Imports.razor"
using Microsoft.AspNetCore.Components.Web;

#line default
#line hidden
#nullable disable
#nullable restore
#line 6 "/Users/gabe/Documents/GitHub/RecursiveExtractor/RecursiveExtractor.Blazor/_Imports.razor"
using Microsoft.AspNetCore.Components.WebAssembly.Http;

#line default
#line hidden
#nullable disable
#nullable restore
#line 7 "/Users/gabe/Documents/GitHub/RecursiveExtractor/RecursiveExtractor.Blazor/_Imports.razor"
using Microsoft.JSInterop;

#line default
#line hidden
#nullable disable
#nullable restore
#line 3 "/Users/gabe/Documents/GitHub/RecursiveExtractor/RecursiveExtractor.Blazor/Pages/Reports.razor"
using Microsoft.CodeAnalysis.Sarif;

#line default
#line hidden
#nullable disable
#nullable restore
#line 4 "/Users/gabe/Documents/GitHub/RecursiveExtractor/RecursiveExtractor.Blazor/Pages/Reports.razor"
using System.Text;

#line default
#line hidden
#nullable disable
#nullable restore
#line 5 "/Users/gabe/Documents/GitHub/RecursiveExtractor/RecursiveExtractor.Blazor/Pages/Reports.razor"
using MoreLinq;

#line default
#line hidden
#nullable disable
    [Microsoft.AspNetCore.Components.RouteAttribute("/reports")]
    [Microsoft.AspNetCore.Components.RouteAttribute("/reports/{RunGuid}")]
    public partial class Reports : Microsoft.AspNetCore.Components.ComponentBase
    {
        #pragma warning disable 1998
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
        {
        }
        #pragma warning restore 1998
#nullable restore
#line 59 "/Users/gabe/Documents/GitHub/RecursiveExtractor/RecursiveExtractor.Blazor/Pages/Reports.razor"
       
    [Parameter]
    public string RunGuid { get; set; } = string.Empty;
    private static string nl = Environment.NewLine;
    private string runId = "";
    private string SelectedRun { get; set; } = "";
    private int Occurrences = 0;
    private int FileCount = 0;
    private HashSet<string> TagList = new HashSet<string>();
    private HashSet<string> RuleList = new HashSet<string>();
    private HashSet<string> FindingLocations = new HashSet<string>();
    public string Status { get; set; } = "Idle";
    public IList<string> RunList = new List<string> { };
    public IDictionary<string, (string,int,string, FailureLevel)> Findings = new Dictionary<string, (string, int,string, FailureLevel)>();
    private Results results;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await ReadStorage();
            if (!string.IsNullOrEmpty(RunGuid))
            {
                SelectedRun = results.RunIdMap[RunGuid];
            }
            else
            {
                SelectedRun = results.RunIdMap.Values.Select(runId => (DateTime.Parse(runId), runId)).MaxBy(x => x.Item1).First().runId;
            }
            await GetResults();
        }
    }

    public async void ClearLocalStorage()
    {
        SelectedRun = "";
        Status = "Idle";
        RunList.Clear();
        Findings.Clear();
        await localStorage.ClearAsync();
        StateHasChanged();
    }

    private void OnSelected(string selection)
    {
        SelectedRun = selection;
        GetResults();
    }

    public async Task ReadStorage()
    {
        try
        {
            Status = $"Reading storage...{nl}";
            this.StateHasChanged();
            var res = await localStorage.GetItemAsync<Results>("DevSkimResults");
            if (res == null)
                res = new Results();
            results = res;
            RunList = res.RunIdMap.Values.ToList();

            Status = $"Found {RunList.Count} runs.{nl}";
            this.StateHasChanged();
        }
        catch (Exception e)
        {
            var message = e.Message;
            var stackTrace = e.StackTrace;
            var type = e.GetType();
            var name = type.Name;
            Console.WriteLine(e.Message);
        }
    }

    public async Task GetResults()
    {
        try
        {
            Occurrences = 0;
            FileCount = 0;
            TagList.Clear();
            RuleList.Clear();
            FindingLocations.Clear();
            Findings.Clear();

            runId = SelectedRun;

            if (String.IsNullOrWhiteSpace(runId))
            {
                Status = $"No run selected.{nl}";
                this.StateHasChanged();
                return;
            }

            Status = $"Reading results of Run ID: {runId}{nl}";
            this.StateHasChanged();

            var res = await localStorage.GetItemAsync<Results>("DevSkimResults");

            // Load sarif results
            var location = res.ResultLocations[runId];
            var sarif = await localStorage.GetItemAsync<string>(location);
            using var stream = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(sarif));
            var loadedSarif = SarifLog.Load(stream);
            var sarifResults = loadedSarif.Runs[0].Results;


            // Read results for summary
            if (sarifResults != null)
            {
                foreach (var result in sarifResults)
                {
                    foreach (var tag in result.Tags)
                    {
                        TagList.Add(tag);
                    }
                    Occurrences += result.OccurrenceCount;
                    RuleList.Add(result.RuleId);
                }
            }
            FileCount = res.FileLocations.Count;

            // Enumerate findings
            foreach (var loc in res.FileLocations[runId])
            {
                var rulesWhichApply = loadedSarif.Runs[0].Results.Where(x => x.Locations.Any(y => y.PhysicalLocation.Address.FullyQualifiedName == loc.Key));
                var severities = loadedSarif.Runs[0].Tool.Driver.Rules.Where(y => rulesWhichApply.Any(x => x.RuleId == y.Id));
                var severity = FailureLevel.Note;
                if (severities.Any())
                    severity = severities.Max(x => x.DefaultConfiguration.Level);
                if (rulesWhichApply.Count() is int count && count > 0)
                {
                    Findings.Add(loc.Key, (loc.Value, count, count == 1 ? "finding" : "findings", severity));
                }
            }
            Status += $"Finished reading results of Run ID: {SelectedRun}{nl}";
            this.StateHasChanged();
        }
        catch (Exception e)
        {
            var message = e.Message;
            var stackTrace = e.StackTrace;
            var type = e.GetType();
            var name = type.Name;
            Console.WriteLine(e.Message);
        }
    }

#line default
#line hidden
#nullable disable
        [global::Microsoft.AspNetCore.Components.InjectAttribute] private Blazored.LocalStorage.ILocalStorageService localStorage { get; set; }
    }
}
#pragma warning restore 1591
