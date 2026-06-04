using System.Diagnostics;

namespace core.Services;

public class Mpv : IMpvService
{
    private static string _mpv_options = "--input-ipc-server=/tmp/mpvsocket --fs";

    public async Task<string> StartMpv(string url)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(url, nameof(url));
        var uri = new Uri(url);

        if (uri.Host == "rezka.ag")
            url = await GetLinksRezka(url);
        using Process mpv = new Process();
        //mpv.StartInfo.UseShellExecute = true;
        mpv.StartInfo.FileName = "mpv";
        mpv.StartInfo.Arguments = $"{url} {_mpv_options}";
        //mpv.StartInfo.RedirectStandardOutput = true;
        mpv.Start();
        await mpv.WaitForExitAsync();
        //mpv.Kill();
        string output = await mpv.StandardOutput.ReadToEndAsync();

        return output;

    }

    private async Task<string> GetLinksRezka(string url)
    {
        var args = new string[]{
            "./ScrapeScript/parser.py",
            //" -- ",
            $"{url}",
            "-v",
            "51",
            "-q",
            "720p"
        };
        string output;
        using (Process scraper = new Process())
        {

            scraper.StartInfo.FileName = "python3";
            foreach (string arg in args)
                scraper.StartInfo.ArgumentList.Add(arg);
            scraper.StartInfo.RedirectStandardOutput = true;
            scraper.Start();
            await scraper.WaitForExitAsync();
            output = await scraper.StandardOutput.ReadLineAsync() ?? "";
            if (output.Equals("--playlist=-"))
                output = await scraper.StandardOutput.ReadToEndAsync();
        }
        return output;
    }
}
