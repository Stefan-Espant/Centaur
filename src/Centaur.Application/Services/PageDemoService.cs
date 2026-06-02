using System.Text.Json;
using Centaur.Application.DTOs;
using Centaur.Application.Interfaces;

namespace Centaur.Application.Services;

public class PageDemoService(
    IPageRepository pageRepository,
    IBlockTypePresetService blockTypePresetService) : IPageDemoService
{
    public async Task EnsureDemoPageAsync(string tenantSchema)
    {
        await blockTypePresetService.EnsurePresetsAsync();

        var pages = await pageRepository.GetAllAsync(tenantSchema);
        if (pages.Any(page => page.Slug == "demo-landingspagina"))
            return;

        var now = DateTime.UtcNow;
        var page = new PageDto(
            Guid.NewGuid(),
            "Demo landingspagina",
            "demo-landingspagina",
            "Voorbeeldpagina met voorgedefinieerde blokken",
            JsonSerializer.SerializeToElement(new object[]
            {
                new
                {
                    _type = "section",
                    _id = Guid.NewGuid().ToString(),
                    name = "Hero sectie",
                    anchor = "hero",
                    variant = "brand",
                    width = "wide",
                    padding_top = 96,
                    padding_bottom = 72
                },
                new
                {
                    _type = "titel",
                    _id = Guid.NewGuid().ToString(),
                    kicker = "Centaur presets",
                    title = "Voorgemaakte blokken die je direct kunt gebruiken",
                    level = "h1",
                    align = "left"
                },
                new
                {
                    _type = "paragraph",
                    _id = Guid.NewGuid().ToString(),
                    title = "Sneller een pagina opbouwen",
                    content = "Deze voorbeeldpagina laat zien hoe de voorgedefinieerde blokken samenwerken in een echte opbouw. Je kunt elk blok direct aanpassen, dupliceren of vervangen in de pagebuilder.",
                    width = "default"
                },
                new
                {
                    _type = "button",
                    _id = Guid.NewGuid().ToString(),
                    label = "Start met bewerken",
                    href = "/pages",
                    style = "primary",
                    new_tab = false
                },
                new
                {
                    _type = "medium",
                    _id = Guid.NewGuid().ToString(),
                    src = "https://images.unsplash.com/photo-1516321318423-f06f85e504b3?auto=format&fit=crop&w=1200&q=80",
                    alt = "Team aan het werk",
                    caption = "Gebruik media voor hero beelden, cases of sfeer.",
                    ratio = "16:9"
                },
                new
                {
                    _type = "gallery",
                    _id = Guid.NewGuid().ToString(),
                    title = "Galerij",
                    items = new object[]
                    {
                        new { image_url = "https://images.unsplash.com/photo-1497366754035-f200968a6e72?auto=format&fit=crop&w=800&q=80", alt = "Studio", caption = "Studio" },
                        new { image_url = "https://images.unsplash.com/photo-1497366412874-3415097a27e7?auto=format&fit=crop&w=800&q=80", alt = "Werkplek", caption = "Werkplek" },
                        new { image_url = "https://images.unsplash.com/photo-1524758631624-e2822e304c36?auto=format&fit=crop&w=800&q=80", alt = "Interieur", caption = "Interieur" }
                    }
                },
                new
                {
                    _type = "reviews_block",
                    _id = Guid.NewGuid().ToString(),
                    title = "Wat klanten zeggen",
                    intro = "Gebruik reviewblokken om vertrouwen op te bouwen met echte feedback.",
                    reviews = new object[]
                    {
                        new { name = "Sanne", role = "Marketing lead", quote = "De pagebuilder voelt veel sneller nu de basisblokken al klaarstaan.", rating = 5 },
                        new { name = "David", role = "Founder", quote = "We kunnen eindelijk meerdere pagina's opzetten zonder steeds opnieuw te beginnen.", rating = 5 }
                    }
                },
                new
                {
                    _type = "form",
                    _id = Guid.NewGuid().ToString(),
                    title = "Contactformulier",
                    intro = "Ook formulieren kun je als presetblok klaarzetten voor hergebruik.",
                    submit_label = "Versturen",
                    recipient_email = "hello@example.com",
                    fields = new object[]
                    {
                        new { label = "Naam", name = "name", type = "text", placeholder = "Je naam", required = true },
                        new { label = "E-mail", name = "email", type = "email", placeholder = "jij@bedrijf.nl", required = true },
                        new { label = "Bericht", name = "message", type = "textarea", placeholder = "Waar kunnen we mee helpen?", required = true }
                    }
                }
            }),
            now,
            now,
            "published");

        await pageRepository.CreateAsync(tenantSchema, page);
    }
}
