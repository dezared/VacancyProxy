using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System;
using System.Net;
using System.Text;

var mainurl = "https://xakep.ru";

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();

app.Use(async (context, next) =>
{
    var contextPath = context.Request.Path;

    var uri = new Uri(mainurl + contextPath);
    var client = new HttpClient();
    using var response = await client.GetAsync(uri);

    using var content = response.Content;
    var pageContent = await content.ReadAsStringAsync();

    var parser = new HtmlParser();
    var document = parser.ParseDocument(pageContent);

    foreach (var el in document.QuerySelectorAll("a"))
        ((IHtmlAnchorElement)el).Href = ((IHtmlAnchorElement)el).Href.Replace("xakep.ru", "localhost:44363");

    foreach (var el in document.QuerySelectorAll("a, span, p"))
    {
        var listOfStrings = el.TextContent.Split(" ");

        if (listOfStrings.Count() >= 2)
        {
            var returnString = new List<string>();
            foreach (var word in listOfStrings)
            {
                if (word.Length == 6)
                    returnString.Add(word + "™");
                else returnString.Add(word);
            }

            el.TextContent = string.Join(" ", returnString);
        }
        else
        {
            if (el.TextContent.Length == 6)
                el.TextContent += "™";
        }
    }

    var bytes = Encoding.UTF8.GetBytes(document.ToHtml());

    await context.Response.Body.WriteAsync(bytes);
    await next.Invoke();
});

app.Run();
