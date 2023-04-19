﻿using RazorLight.Razor;

namespace MacroMail.Service.Template;

public class InMemoryRazorLightProject : RazorLightProject
{
    public override Task<RazorLightProjectItem> GetItemAsync(string templateKey)
    {
        return Task.FromResult<RazorLightProjectItem>(new TextSourceRazorProjectItem(templateKey, templateKey));
    }

    public override Task<IEnumerable<RazorLightProjectItem>> GetImportsAsync(string templateKey)
    {
        return Task.FromResult<IEnumerable<RazorLightProjectItem>>(new List<RazorLightProjectItem>());
    }
}