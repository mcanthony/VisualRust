﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel.Composition;
using System.Linq.Expressions;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Antlr4.Runtime;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Microsoft.VisualStudio.Text.Formatting;

namespace VisualRust
{
    [Export(typeof(ISmartIndentProvider))]
    [ContentType("rust")]
    public class VisualRustSmartIndentProvider : ISmartIndentProvider
    {
        public ISmartIndent CreateSmartIndent(ITextView textView)
        {
            return new VisualRustSmartIndent(textView);
        }
    }

    // TODO: this indenter should take comments into consideration
    // TODO: this indenter should take tabs into consideration
    class VisualRustSmartIndent : ISmartIndent
    {
        private ITextView _textView;
        internal VisualRustSmartIndent(ITextView textView)
        {
            _textView = textView;
        }

        int? ISmartIndent.GetDesiredIndentation(ITextSnapshotLine currentSnapshotLine)
        {
            var textView = _textView;
            var textSnapshot = textView.TextSnapshot;
            var caret = textView.Caret;
            var caretPosition = caret.Position.BufferPosition.Position;

            var indentStep = _textView.Options.GetIndentSize();

            var caretLine = textSnapshot.GetLineFromPosition(caretPosition);
            var lineReminder = new Span(caretPosition, caretLine.End - caretPosition);

            var textToCaret = textSnapshot.GetText(0, caretPosition);
            var tokens = Utils.LexString(textToCaret);

            var indentStepsCount = 0;
            foreach (var token in tokens)
            {
                // "{"
                if (token.Type == RustLexer.RustLexer.LBRACE)
                {
                    indentStepsCount++;
                }

                // "}"
                if (token.Type == RustLexer.RustLexer.RBRACE && indentStepsCount > 0)
                {
                    indentStepsCount--;
                }

                // "("
                if (token.Type == RustLexer.RustLexer.LPAREN)
                {
                    indentStepsCount++;
                }

                // ")"
                if (token.Type == RustLexer.RustLexer.RPAREN && indentStepsCount > 0)
                {
                    indentStepsCount--;
                }
            }

            var closeBraceAfterCaret = false;
            foreach (var ch in textSnapshot.GetText(lineReminder))
            {
                if (ch == '}' || ch == ')')
                {
                    closeBraceAfterCaret = true;
                    break;
                }

                if (ch != ' ')
                {
                    break;
                }
            }

            var indention = indentStepsCount * indentStep;
            if (closeBraceAfterCaret)
            {
                indention -= indentStep;
            }


            return indention;
        }

        public void Dispose() { }
    }
}
