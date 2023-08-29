using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using AvalonDock.Themes;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using Newtonsoft.Json.Linq;
using Stalker_Studio.Common;

namespace Stalker_Studio.ViewModel
{
    class TextFileViewModel : FileViewModel, IDisposable
    {
        TextDocument _document = new TextDocument();
        IHighlightingDefinition _highlightingDefinition = null;
        int _currentPosition = 0;
        int _selectionStart = 0;
        int _selectionLength = 0;
        bool _showAllSymbols = false;

        ToggleRelayCommand _showAllSymbolsCommand = null;

        public TextFileViewModel(TextFileModel file) : base(file)
        {
            if (file is ObjectTextFileModel)
                _document.Text = (file as ObjectTextFileModel).Serialize();
            else 
                _document.Text = file.Text;
            Initialize();
        }
        public TextFileViewModel(string fullName) : base(new TextFileModel(fullName))
        {
            Initialize();
        }
        public TextFileViewModel() : base(new TextFileModel())
        {
            Initialize();
        }

        public void Dispose()
        {
            Initialize();
            _document.TextChanged -= Document_TextChanged;
        }
        /// <summary>
        /// Документ для редактирования через TextEditor
        /// </summary>
        public TextDocument Document 
        {
            get { return _document; }
            set { _document = value; }
        }
        /// <summary>
        /// Подсветка синтаксиса
        /// </summary>
        public IHighlightingDefinition HighlightingDefinition
        {
            get 
            {
                if (_highlightingDefinition == null)
                {
                    var hlManager = HighlightingManager.Instance;
                    _highlightingDefinition = hlManager.GetDefinitionByExtension(_file.Extension);
                }
                return _highlightingDefinition; 
            }
            set 
            {
                _highlightingDefinition = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Текущая позиция каретки в редакторе
        /// </summary>
        public int CurrentPosition 
        {
            get 
            {
                if (_mainControl is TextEditor)
                    return (_mainControl as TextEditor).CaretOffset;
                return _currentPosition;
            }
            set
            {
                if (_mainControl is TextEditor)
                {
                    TextEditor editor = (_mainControl as TextEditor);
                    editor.CaretOffset = value;
                    editor.SelectionStart = value;
                    editor.TextArea.Caret.BringCaretToView();
                    editor.TextArea.Caret.Show();
                }
                _currentPosition = value;
                OnPropertyChanged();
            }
        }

        public ToggleRelayCommand ShowAllSymbolsCommand
        {
            get
            {
                if (_showAllSymbolsCommand == null)
                {
                    _showAllSymbolsCommand = new ToggleRelayCommand(this, (p) => OnShowAllSymbols(p), (p) => CanShowAllSymbols(p),
                    "Показывать\\скрывать спец. символы и пробелы", "icon_Paragraph",
                    nameof(ShowAllSymbols),
                    "Скрыть спец. символы",
                    "Показать все символы");
                }
                return _showAllSymbolsCommand;
            }
        }

        public bool ShowAllSymbols 
        { 
            get => _showAllSymbols;
            set 
            { 
                _showAllSymbols = value;
                (_mainControl as TextEditor).Options.ShowBoxForControlCharacters = _showAllSymbols;
                (_mainControl as TextEditor).Options.ShowEndOfLine = _showAllSymbols;
                (_mainControl as TextEditor).Options.ShowSpaces = _showAllSymbols;
                (_mainControl as TextEditor).Options.ShowTabs = _showAllSymbols;
                OnPropertyChanged();
            }
        }

        public int SelectionStart 
        { 
            get => _selectionStart;
            set
            {
                _selectionStart = value;
                if (_mainControl != null)
                    (_mainControl as TextEditor).SelectionStart = _selectionStart;
                OnPropertyChanged();
            }
        }
        public int SelectionLength { 
            get => _selectionLength;
            set
            {
                _selectionLength = value;
                if (_mainControl != null)
                    (_mainControl as TextEditor).SelectionLength = _selectionLength;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Выделяет строку по индексу
        /// </summary>
        public void SelectLine(int lineIndex)
        {
            if (_mainControl is TextEditor)
            {
                TextEditor editor = (_mainControl as TextEditor);
                editor.SelectionStart = editor.CaretOffset;
                editor.SelectionLength = editor.Document.Lines[lineIndex].Length - 2;
            }
        }
        /// <summary>
        /// Выделяет элемент в тексте
        /// </summary>
        public void SelectNode(TextNode node)
        {
            SelectionStart = node.Offset;
            SelectionLength = node.Length;
        }

        private void Initialize()
        {
            _commands.Add(ShowAllSymbolsCommand);
            _document.TextChanged += Document_TextChanged;
        }

        private bool CanShowAllSymbols(object p)
        {
            return true;
        }

        private void OnShowAllSymbols(object p)
        {
            ShowAllSymbols = !ShowAllSymbols;
        }

        private void Document_TextChanged(object sender, EventArgs e)
        {
            (File as TextFileModel).Text = _document.Text;
        }

        protected override void OnSetMainControl() 
        {
            TextEditor editor = (_mainControl as TextEditor);

            editor.SelectionStart = _selectionStart;
            editor.SelectionLength = _selectionLength;

            if (_currentPosition != 0)
            {
                editor.Focus();
                editor.CaretOffset = _currentPosition;
            }
            else if (_selectionLength != 0)
            {
                editor.Focus();
                editor.CaretOffset = _selectionStart;
            }
                
            editor.Loaded += TextFileViewModel_Loaded;
            editor.Options.ShowBoxForControlCharacters = _showAllSymbols;
            editor.Options.ShowEndOfLine = _showAllSymbols;
            editor.Options.ShowSpaces = _showAllSymbols;
            editor.Options.ShowTabs = _showAllSymbols;
        }

        private void TextFileViewModel_Loaded(object sender, EventArgs e)
        {
            TextEditor editor = (sender as TextEditor);
            editor.Encoding = (_file as TextFileModel).Encoding;
            if (_currentPosition != 0 && _selectionLength == 0)
            {
                editor.SelectionLength = editor.Document.Lines[editor.TextArea.Caret.Line - 1].Length;
            }

            editor.TextArea.Caret.BringCaretToView();
            editor.TextArea.Caret.Show();
        }
    }
}
