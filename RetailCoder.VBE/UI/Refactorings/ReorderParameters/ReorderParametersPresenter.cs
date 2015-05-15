﻿using Microsoft.Vbe.Interop;
using Rubberduck.Parsing;
using Rubberduck.Parsing.Grammar;
using Rubberduck.Parsing.Symbols;
using Rubberduck.VBEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;

namespace Rubberduck.UI.Refactorings.ReorderParameters
{
    class ReorderParametersPresenter
    {
        private readonly VBE _vbe;
        private readonly IReorderParametersView _view;
        private readonly Declarations _declarations;
        private readonly QualifiedSelection _selection;
        private readonly VBProjectParseResult _parseResult;

        public ReorderParametersPresenter(VBE vbe, IReorderParametersView view, VBProjectParseResult parseResult, QualifiedSelection selection)
        {
            _vbe = vbe;
            _view = view;
            _view.OkButtonClicked += OnOkButtonClicked;

            _parseResult = parseResult;
            _declarations = parseResult.Declarations;
            _selection = selection;
        }

        public void Show()
        {
            AcquireTarget(_selection);

            if (_view.Target != null && ValidDeclarationTypes.Contains(_view.Target.DeclarationType))
            {
                LoadParams();

                _view.InitializeParameterGrid();
                _view.ShowDialog();
            }
        }

        private void LoadParams()
        {
            var proc = (dynamic)_view.Target.Context;
            var argList = (VBAParser.ArgListContext)proc.argList();
            var args = argList.arg();

            int index = 0;
            foreach (var arg in args)
            {
                _view.Parameters.Add(new Parameter(arg.ambiguousIdentifier().GetText(), index++));
            }
        }

        private void OnOkButtonClicked(object sender, EventArgs e)
        {
            if (!Changes()) { return; }

            AdjustSignature();

            foreach (var reference in _view.Target.References)
            {
                // change value here
                // will create methods as needed
            }
        }

        private void AdjustSignature()
        {
            var proc = (dynamic)_view.Target.Context;
            var argList = (VBAParser.ArgListContext)proc.argList();
            var args = argList.arg();

            var lineNum = argList.GetSelection().LineCount;

            var module = _view.Target.QualifiedName.QualifiedModuleName.Component.CodeModule;
            var newContent = string.Empty;

            var index = 0;
            for (var i = 0; i < lineNum; i++)
            {
                var word = string.Empty;
                var line = module.get_Lines(argList.Start.Line + i, 1);
                
                foreach (var c in line)
                {
                    if (char.IsLetterOrDigit(c))
                    {
                        word += c;
                    }
                    else if (c == ' ')
                    {
                        try
                        {
                            var param = _view.Parameters.Where(p => p.Name == word).First().Name;

                            newContent += _view.Parameters[index++].Name + " ";
                        }
                        catch
                        {
                            newContent += word + " ";
                        }
                        word = string.Empty;
                    }
                    else
                    {
                        newContent += word + c;
                        word = string.Empty;
                    }
                }
                
                module.ReplaceLine(argList.Start.Line + i, newContent);
                newContent = string.Empty;
            }
            //module.ReplaceLine(argList.Start.Line, newContent);
            //module.DeleteLines(argList.Start.Line + 1, lineNum - 1);
        }

        private bool Changes()
        {
            for (int i = 0; i < _view.Parameters.Count; i++)
            {
                if (_view.Parameters[i].Index != i)
                {
                    return true;
                }
            }

            return false;
        }

        private static readonly DeclarationType[] ValidDeclarationTypes =
            {
                 DeclarationType.Event,
                 DeclarationType.Function,
                 DeclarationType.Procedure,
                 DeclarationType.PropertyGet,
                 DeclarationType.PropertyLet,
                 DeclarationType.PropertySet
            };

        private void AcquireTarget(QualifiedSelection selection)
        {
            var target = _declarations.Items
                .Where(item => !item.IsBuiltIn && ValidDeclarationTypes.Contains(item.DeclarationType))
                .FirstOrDefault(item => IsSelectedDeclaration(selection, item)
                                      || IsSelectedReference(selection, item));

            PromptIfTargetImplementsInterface(ref target);
            _view.Target = target;
        }

        private void PromptIfTargetImplementsInterface(ref Declaration target)
        {
            var declaration = target;
            var interfaceImplementation = _declarations.FindInterfaceImplementationMembers().SingleOrDefault(m => m.Equals(declaration));
            if (target == null || interfaceImplementation == null)
            {
                return;
            }

            var interfaceMember = _declarations.FindInterfaceMember(interfaceImplementation);
            var message = string.Format(RubberduckUI.RenamePresenter_TargetIsInterfaceMemberImplementation, target.IdentifierName, interfaceMember.ComponentName, interfaceMember.IdentifierName);

            var confirm = MessageBox.Show(message, RubberduckUI.RenameDialog_TitleText, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (confirm == DialogResult.No)
            {
                target = null;
                return;
            }

            target = interfaceMember;
        }

        private bool IsSelectedReference(QualifiedSelection selection, Declaration declaration)
        {
            return declaration.References.Any(r =>
                r.QualifiedModuleName == selection.QualifiedName &&
                r.Selection.ContainsFirstCharacter(selection.Selection));
        }

        private bool IsSelectedDeclaration(QualifiedSelection selection, Declaration declaration)
        {
            return declaration.QualifiedName.QualifiedModuleName == selection.QualifiedName
                   && (declaration.Selection.ContainsFirstCharacter(selection.Selection));
        }
    }
}
