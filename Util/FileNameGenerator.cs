namespace PrinterApp.Util
{
    public static class FileNameGenerator
    {
        public static string Generate() => $"Arquivo_{Guid.NewGuid().ToString()[..6]}.txt";
    }
}
