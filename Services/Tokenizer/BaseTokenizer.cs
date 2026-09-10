namespace XMedicalAndroid.Services.Tokenizer;

internal abstract class BaseTokenizer
{
    public abstract IEnumerable<string> Tokenize(string text);
    protected static IEnumerable<string> WhitespaceTokenize(string text)
    {
        string strippedText = text.Trim();
        if (string.IsNullOrEmpty(strippedText))
        {
            return [];
        }

        IEnumerable<string> tokens = text.Split(' ');
        return tokens;
    }
}
