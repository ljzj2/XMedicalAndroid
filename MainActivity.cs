using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
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
                //tokenizer = new Services.WordPieceTokenizer(vocab);

                //var (inputIds, attentionMask) = tokenizer.Encode("This is a sample", _maxLen);
                tokenizer = BertTokenizer.Create(vocabPath);

                var encodings = tokenizer.EncodeToIds("This is a sample");

                var path = Path.Combine(CacheDir.AbsolutePath, "allmini.onnx");
                using (var s1 = Assets.Open("Models/model.onnx"))
                {
                    using (var fs1 = File.Create(path))
                    {
                        s1.CopyTo(fs1);
                    }
                }
                _session = new InferenceSession(path);

                var inputIds = new long[_maxLen];
                long[] attentionMask = new long[_maxLen];

                var idTensor = new DenseTensor<long>([1, _maxLen]);
                for (int i = 0; i < encodings.Count; i++)
                {
                    inputIds[i] = encodings[i];
                    idTensor[0, i] = 1;
                }

                var inputs = new[]
                {
                    NamedOnnxValue.CreateFromTensor("input_ids",idTensor)
                };

                var results = _session.Run(inputs);
            }
        }

        //TiktokenTokenizer.CreateForModel("");
        //var _tokenizer = new Tokenizer();
    }

    private InferenceSession? _session;
    private int _maxLen = 128;
    private Tokenizer tokenizer;


}