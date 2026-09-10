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
        using (var s = Assets.Open("Models/vocab.txt"))
        {
            using (var fs = File.Create(vocabPath))
            {
                s.CopyTo(fs);

                fs.Dispose();

                var vocab = Services.WordPieceTokenizer.LoadVocabFromFile(vocabPath);
                var tokenizer = new Services.WordPieceTokenizer(vocab);

                var (inputIds, attentionMask) = tokenizer.Encode("This is a sample", _maxLen);

                var path = Path.Combine(CacheDir.AbsolutePath, "allmini.onnx");
                using (var s1 = Assets.Open("Models/model.onnx"))
                {
                    using (var fs1 = File.Create(path))
                    {
                        s1.CopyTo(fs1);
                    }
                }

                _session = new InferenceSession(path);
            }
        }

        //TiktokenTokenizer.CreateForModel("");
        //var _tokenizer = new Tokenizer();
    }

    private InferenceSession? _session;
    private int _maxLen = 128;


}