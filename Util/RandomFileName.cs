namespace PrinterApp.Util
{
    public static class RandomFileName
    {
        public static string GenerateFileName() => $"Arquivo_{Guid.NewGuid().ToString()[..6]}.txt";
    }
}
