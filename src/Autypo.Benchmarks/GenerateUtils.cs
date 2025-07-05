namespace Autypo.Benchmarks;
internal static class GenerateUtils
{
    public static string[] GenerateGuids(int n)
    {
        var items = new string[n];
        for (int i = 0; i < n; i++)
        {
            items[i] = string.Create(36, Guid.NewGuid(), static (buffer, state) =>
            {
                state.TryFormat(buffer, out int written);
                if (written != 36) throw new Exception();
                buffer[08] = ' ';
                buffer[13] = ' ';
                buffer[18] = ' ';
                buffer[23] = ' ';
            });
        }
        return items;
    }
}
