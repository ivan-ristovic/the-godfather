$client = new-object System.Net.WebClient
$client.DownloadFile("https://ci.appveyor.com/api/projects/ivan-ristovic/the-godfather/artifacts/TheGodfather.zip","TheGodfather.zip")
$client.DownloadFile("https://ci.appveyor.com/api/projects/ivan-ristovic/the-godfather/artifacts/TheGodfatherResources.zip","TheGodfatherResources.zip")