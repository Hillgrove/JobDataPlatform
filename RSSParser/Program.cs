
using HtmlAgilityPack;
using System.ServiceModel.Syndication;
using System.Xml;

var url = "https://www.jobindex.dk/jobsoegning.rss?geoareaid=15182&subid=1";

using var httpClient = new HttpClient();
var stream = await httpClient.GetStreamAsync(url);

using var reader = XmlReader.Create(stream);
var feed = SyndicationFeed.Load(reader);

foreach (var item in feed.Items)
{
    //Console.WriteLine($"Title: {item.Title.Text}");

    var htmlDoc = new HtmlDocument();
    htmlDoc.LoadHtml(item.Summary.Text);

    var locationNode = htmlDoc.DocumentNode.SelectSingleNode("//*[contains(@class, 'jix_robotjob--area')]");
    var jobindexJobLink = item.Links[0].Uri;

    var jobHtml = await httpClient.GetStringAsync(jobindexJobLink);
    var jobDoc = new HtmlDocument();
    jobDoc.LoadHtml(jobHtml);

    var node = jobDoc.DocumentNode.SelectSingleNode(
        "//a[contains(@class, 'seejobdesktop') or contains(@class, 'seejobmobil')]");
       
    node ??= jobDoc.DocumentNode.SelectSingleNode(
        "//a[normalize-space(text())='Se jobbet']");

    var externalJobLink = node?.GetAttributeValue("href", null);

    Console.WriteLine("Lokation: " + locationNode?.InnerText.Trim());
    Console.WriteLine($"Link: {jobindexJobLink}");
    Console.WriteLine("Ekstern ansøgnings-URL: " + externalJobLink);



    //Console.WriteLine(new string('-', 50));
}