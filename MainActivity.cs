using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.Tokenizers;

namespace XMedicalAndroid;

[Activity(Label = "@string/app_name", MainLauncher = true)]
public class MainActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Set our view from the "main" layout resource
        SetContentView(Resource.Layout.activity_main);

        Init();
    }

    void Init()
    {
        ActionBar?.Hide();


        if (CacheDir == null)
        {
            return;
        }
        if (Assets == null)
        {
            return;
        }

        var vocabPath = Path.Combine(CacheDir.AbsolutePath, "vocab.txt");
        using (var s = Assets.Open("vocab.txt"))
        {
            using (var fs = File.Create(vocabPath))
            {
                s.CopyTo(fs);

                var vocab = Services.WordPieceTokenizer.LoadVocabFromFile(vocabPath);
                var tokenizer = new Services.WordPieceTokenizer(vocab);

                var (inputIds, attentionMask) = tokenizer.Encode("This is a sample", _maxLen);
            }
        }

        //TiktokenTokenizer.CreateForModel("");
        //var _tokenizer = new Tokenizer();
    }

    private readonly InferenceSession _session = new InferenceSession("");
    private int _maxLen = 128;


}