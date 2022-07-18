using System;
using BlazingQuartzUI.Services;
using Microsoft.AspNetCore.Components;

namespace BlazingQuartzUI.Shared;

public partial class MainLayout
{
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private LayoutService LayoutService { get; set; } = null!;

    private string GetActiveClass(BasePage page)
    {
        return page == LayoutService.GetDocsBasePage(NavigationManager.Uri) ? "nav-item light-blue darken-1 mx-1 px-3" : "nav-item mx-1 px-3";
    }
}



