using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Stalker_Studio
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            using (var stream = new System.IO.StringReader(Stalker_Studio.Properties.Resources.LtxHighlighting))
            {
                using (var reader = new System.Xml.XmlTextReader(stream))
                {
                    ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.RegisterHighlighting("LTX", new string[] { ".ltx" },
                        ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader,
                            ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance));
                }
            }
            using (var stream = new System.IO.StringReader(Stalker_Studio.Properties.Resources.LuaHighlighting))
            {
                using (var reader = new System.Xml.XmlTextReader(stream))
                {
                    ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.RegisterHighlighting("LUA", new string[] { ".lua", ".script" },
                        ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader,
                            ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance));
                }
            }
        }

        public static string[] Args;
        protected override void OnStartup(StartupEventArgs e)
        {
            Args = e.Args;
            for(int i = 0; i < Args.Length; i++)
            {
                Args[i] = Args[i].Replace("[]", " ");
            }
            
            base.OnStartup(e);
        }
    }
}
