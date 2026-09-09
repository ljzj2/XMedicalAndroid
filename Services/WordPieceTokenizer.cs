using System.Text.RegularExpressions;

namespace XMedicalAndroid.Services;

public class WordPieceTokenizer
{
    private readonly Dictionary<string, int> _vocab;
    private readonly int _unkId;
    private readonly int _clsId;
    private readonly int _sepId;
    private readonly int _padId;
    private readonly Regex _tokenPattern = new(@"\w+|[^\s\w]+", RegexOptions.Compiled);

    public WordPieceTokenizer(Dictionary<string, int> vocab)
    {
        _vocab = vocab ?? throw new ArgumentNullException(nameof(vocab));
        _unkId = GetIdOrThrow("[UNK]");
        _clsId = GetIdOrFallback("[CLS]");
        _sepId = GetIdOrFallback("[SEP]");
        _padId = GetIdOrFallback("[PAD]");
    }
    private int GetIdOrThrow(string token)
    {
        if (_vocab.TryGetValue(token, out var id)) return id;
        throw new Exception($"Required token {token} not found in vocab");
    }

    private int GetIdOrFallback(string token)
    {
        if (_vocab.TryGetValue(token, out var id)) return id;
        return _unkId;
    }

    // Basic tokenization: splits text to word/punct tokens and lowercases (uncased model).
    private IEnumerable<string> BasicTokenize(string text)
    {
        if (string.IsNullOrEmpty(text)) yield break;
        text = text.ToLowerInvariant(); // uncased model
        var matches = _tokenPattern.Matches(text);
        foreach (Match m in matches) yield return m.Value;
    }

    // WordPiece greedy longest-match algorithm
    private IEnumerable<string> WordPieceTokenizePiece(string token)
    {
        int start = 0;
        int len = token.Length;
        var subTokens = new List<string>();
        while (start < len)
        {
            int end = len;
            string curSubstr = null;
            while (start < end)
            {
                string substr = token.Substring(start, end - start);
                // use '##' prefix for continuation pieces when checking vocab (BERT WordPiece)
                string check = start == 0 ? substr : "##" + substr;
                if (_vocab.ContainsKey(check))
                {
                    curSubstr = check;
                    break;
                }
                end--;
            }

            if (curSubstr == null)
            {
                // unknown token -> map to [UNK]
                subTokens.Add("[UNK]");
                break;
            }
            subTokens.Add(curSubstr);
            start = end;
        }
        return subTokens;
    }

    // Public: converts a single string into input ids and attention mask padded/truncated to maxLen
    public (long[] inputIds, long[] attentionMask) Encode(string text, int maxLen)
    {
        var tokens = new List<string>();
        // add CLS if available
        if (_vocab.ContainsKey("[CLS]")) tokens.Add("[CLS]");

        foreach (var word in BasicTokenize(text))
        {
            foreach (var sub in WordPieceTokenizePiece(word))
            {
                tokens.Add(sub);
            }
        }

        // add SEP
        if (_vocab.ContainsKey("[SEP]")) tokens.Add("[SEP]");

        // Convert tokens -> ids, build mask, pad/truncate to maxLen
        var ids = new long[maxLen];
        var mask = new long[maxLen];
        int i = 0;
        foreach (var t in tokens)
        {
            if (i >= maxLen) break;
            ids[i] = _vocab.ContainsKey(t) ? _vocab[t] : _unkId;
            mask[i] = 1;
            i++;
        }
        // pad remaining with pad id (or 0)
        for (; i < maxLen; i++)
        {
            ids[i] = _padId; // often 0, but use vocab's PAD id if present
            mask[i] = 0;
        }

        return (ids, mask);
    }

    // Helper: load vocab from vocab.txt (one token per line) -> token -> id (line index)
    public static Dictionary<string, int> LoadVocabFromFile(string path)
    {
        var dict = new Dictionary<string, int>();
        int idx = 0;
        foreach (var line in File.ReadLines(path))
        {
            var token = line.Trim();
            if (token == "") continue;
            if (!dict.ContainsKey(token))
            {
                dict[token] = idx;
                idx++;
            }
        }
        return dict;
    }
}