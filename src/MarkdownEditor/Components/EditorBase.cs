using System;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MarkdownEditor.Components
{
    public class EditorBase : ComponentBase, IDisposable
    {
        [Inject]
        private IJSRuntime _jsRuntime { get; set; }
        [Inject]
        private ILogger<EditorBase> _logger { get; set; }
        protected string MarkdownText { get; set; }

        private IObservable<long> _timerSource;
        private IDisposable _timerSubscription;
        string _inputText;

        public EditorBase()
        {
            MarkdownText = "Hello World.";
        }
        public void Dispose()
        {
            // 手抜きDispose
            _timerSubscription?.Dispose();
        }

        // OnAfterRenderAsyncもある
        protected override void OnAfterRender(bool firstRender)
        {
            // 描画毎に呼ばれるので、最初の1回目のみとする.
            if (!firstRender) return;

            // Rxで1秒毎にMarkdownをHtmlへ変換
            _timerSource = Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(500));
            _timerSubscription = _timerSource.Subscribe(async _ => await ConvertMarkdownToHtml());
        }

        public void HandleOnInput(ChangeEventArgs e)
        {
            // oninputイベントハンドラ
            // TextAreaに入力した文字列を取得
            _inputText = e?.Value?.ToString();
        }

        private async Task ConvertMarkdownToHtml()
        {
            if (string.IsNullOrEmpty(_inputText)) return;
            try
            {
                // JavaScriptのConvertToHtml関数でMarkdownをHtml化
                MarkdownText = await _jsRuntime.InvokeAsync<string>("ConvertMarkdownToHtml", _inputText);
                // ComponentBaseを継承したクラスから画面の更新する際はInvokeAsyncを使用する
                await InvokeAsync(() => StateHasChanged());
            }
            catch (System.Exception exp)
            {
                _logger.LogError(exp.Message);
            }
        }
    }
}