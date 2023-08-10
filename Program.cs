using System;
using System.Diagnostics.Metrics;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using HtmlAgilityPack;

class Program
{
    private const string szxPATH = "C:\\Users\\danco\\OneDrive\\Documents\\Inv\\Jobs.txt";
    private const int nxMINUTE = 60000;
    static void Main()
    {
        Program program = new Program();
        program.Begin();
    }

    private void Begin()
    {
        List<string> val = new List<string>();
        Prs pr = new Prs();
       

        while (true)
        {
            pr.lines = ReadExistingValues();

            pr.sWriter = new StreamWriter(szxPATH);

            WriteToFileIf(pr);

            pr.sWriter.Flush();

            Thread.Sleep(nxMINUTE);
        }
    }

    private void WriteToFileIf(Prs pr)
    {
        bool bScanPage = true;

        try
        {
            while (bScanPage)
            {
                HtmlNodeCollection parentNode = FetchNodes(pr);

                if (parentNode == null)
                {
                    Console.WriteLine("No data found.");
                    break;
                }

                pr.parentNodeCount = parentNode.Count;

                foreach (HtmlNode node in parentNode)
                {
                    pr.node = node;

                    bScanPage = LogEntryIf(pr);

                    if (bScanPage == false)
                        break;
                }
                pr.pageCounter++;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private bool LogEntryIf(Prs pr)
    {
        bool bScanPage = true;

        if (pr.node?.InnerText.Contains("KenyaClosed") == false)
        {
            if (pr.node.InnerText.Contains("Software")
                || pr.node.InnerText.Contains("Developer")
                || pr.node.InnerText.Contains("Web")
                || pr.node.InnerText.Contains("Engineer"))
            {
                string entry = pr.counter + $".  (Page:{pr.pageCounter})    " + pr.node.InnerText;

                if (pr.lines?.Contains(entry) == false)
                {

                    if (pr.lines.Count > 0)
                    {
                        Console.Write($" [New entry on Page {pr.pageCounter}] ");

                        pr.sWriter?.WriteLine("[New]");
                        pr.lines.Add(entry);
                    }

                    Console.Write($"{pr.pageCounter}.{pr.counter}...");

                    pr.sWriter?.WriteLine(entry);
                    pr.counter++;
                }

            }
        }
        else
        {
            bScanPage = false;
            Console.WriteLine("...[202]");
        }


        return bScanPage;
    }

    private HtmlNodeCollection FetchNodes(Prs pr)
    {
        HtmlNodeCollection parentNode = null;

        string url = $"https://www.fuzu.com/kenya/job?filters[term]=Software%20Developer&page={pr.pageCounter}";

        HtmlWeb web = new HtmlWeb();

        Console.Write($"[101]...");

        HtmlDocument doc = web.Load(url);

        parentNode = doc.DocumentNode.SelectNodes("//a[@class='Card__StyledDiv-sc-uckied-0 logKwD b2c-card clickable  ']");

        return parentNode;

    }

    private List<string> ReadExistingValues()
    {
        List<string> collection = new List<string>();

        try
        {
            using (StreamReader reader = new StreamReader(szxPATH))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    collection.Add(line);
                }
            }
        }
        catch (Exception)
        {
            return collection;
        }

        return collection;
    }
}

public class Prs
{
    public int counter { get; set; } = 1;
    public int pageCounter { get; set; } = 1;
    public int parentNodeCount { get; set; }
    public bool isValueAdded { get; set; } = false;
    public List<string>? lines { get; set; }
    public StreamWriter? sWriter { get; set; }
    public HtmlNode? node { get; set; }
}
