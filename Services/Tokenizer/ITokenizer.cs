namespace XMedicalAndroid.Services.Tokenizer;

public interface ITokenizer
{
    IEnumerable<Token> Tokenize(string text);
    IEnumerable<EncodedToken> Encode(int sequenceLength, string text);

}
