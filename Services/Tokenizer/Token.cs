namespace XMedicalAndroid.Services.Tokenizer;

public class Token(string value, long segmentIndex, long vocabularyIndex)
{
    public string Value { get; set; } = value;
    public long VocabularyIndex { get; set; } = vocabularyIndex;
    public long SegmentIndex { get; set; } = segmentIndex;
}
