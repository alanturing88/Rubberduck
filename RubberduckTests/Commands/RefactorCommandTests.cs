﻿using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rubberduck.Parsing;
using Rubberduck.Parsing.VBA;
using Rubberduck.UI;
using Rubberduck.UI.Command.Refactorings;
using Rubberduck.VBEditor;
using Rubberduck.VBEditor.Application;
using Rubberduck.VBEditor.Events;
using Rubberduck.VBEditor.SafeComWrappers;
using Rubberduck.VBEditor.SafeComWrappers.Abstract;
using RubberduckTests.Mocks;

namespace RubberduckTests.Commands
{
    [TestClass]
    public class RefactorCommandTests
    {
        [TestMethod]
        public void EncapsulateField_CanExecute_NullActiveCodePane()
        {
            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule("", out component);
            vbe.Setup(v => v.ActiveCodePane).Returns((ICodePane)null);
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var encapsulateFieldCommand = new RefactorEncapsulateFieldCommand(vbe.Object, parser.State, null);
            Assert.IsFalse(encapsulateFieldCommand.CanExecute(null));
        }

        [TestMethod]
        public void EncapsulateField_CanExecute_NonReadyState()
        {
            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule("", out component);
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }
            parser.State.SetStatusAndFireStateChanged(this, ParserState.ResolvedDeclarations);

            var encapsulateFieldCommand = new RefactorEncapsulateFieldCommand(vbe.Object, parser.State, null);
            Assert.IsFalse(encapsulateFieldCommand.CanExecute(null));
        }

        [TestMethod]
        public void EncapsulateField_CanExecute_LocalVariable()
        {
            var input =
@"Sub Foo()
    Dim d As Boolean
End Sub";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule(input, out component, new Selection(2, 9, 2, 9));
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var encapsulateFieldCommand = new RefactorEncapsulateFieldCommand(vbe.Object, parser.State, null);
            Assert.IsFalse(encapsulateFieldCommand.CanExecute(null));
        }

        [TestMethod]
        public void EncapsulateField_CanExecute_Proc()
        {
            var input =
@"Dim d As Boolean
Sub Foo()
End Sub";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule(input, out component, new Selection(2, 7, 2, 7));
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var encapsulateFieldCommand = new RefactorEncapsulateFieldCommand(vbe.Object, parser.State, null);
            Assert.IsFalse(encapsulateFieldCommand.CanExecute(null));
        }

        [TestMethod]
        public void EncapsulateField_CanExecute_Field()
        {
            var input =
@"Dim d As Boolean
Sub Foo()
End Sub";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule(input, out component, new Selection(1, 5, 1, 5));
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var encapsulateFieldCommand = new RefactorEncapsulateFieldCommand(vbe.Object, parser.State, null);
            Assert.IsTrue(encapsulateFieldCommand.CanExecute(null));
        }

        [TestMethod]
        public void ExtractInterface_CanExecute_NullActiveCodePane()
        {
            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule("", out component);
            vbe.Setup(v => v.ActiveCodePane).Returns((ICodePane)null);
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var extractInterfaceCommand = new RefactorExtractInterfaceCommand(vbe.Object, parser.State, null);
            Assert.IsFalse(extractInterfaceCommand.CanExecute(null));
        }

        [TestMethod]
        public void ExtractInterface_CanExecute_NonReadyState()
        {
            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleModule("", ComponentType.ClassModule, out component, new Selection());
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }
            parser.State.SetStatusAndFireStateChanged(this, ParserState.ResolvedDeclarations);

            var extractInterfaceCommand = new RefactorExtractInterfaceCommand(vbe.Object, parser.State, null);
            Assert.IsFalse(extractInterfaceCommand.CanExecute(null));
        }

        [TestMethod]
        public void ExtractInterface_CanExecute_NoMembers()
        {
            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleModule("Option Explicit", ComponentType.ClassModule, out component, new Selection());
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var extractInterfaceCommand = new RefactorExtractInterfaceCommand(vbe.Object, parser.State, null);
            Assert.IsFalse(extractInterfaceCommand.CanExecute(null));
        }

        [TestMethod]
        public void ExtractInterface_CanExecute_Proc_StdModule()
        {
            var input =
@"Sub foo()
End Sub";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule(input, out component);
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var extractInterfaceCommand = new RefactorExtractInterfaceCommand(vbe.Object, parser.State, null);
            Assert.IsFalse(extractInterfaceCommand.CanExecute(null));
        }

        [TestMethod]
        public void ExtractInterface_CanExecute_Field()
        {
            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleModule("Dim d As Boolean", ComponentType.ClassModule, out component, Selection.Home);
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var extractInterfaceCommand = new RefactorExtractInterfaceCommand(vbe.Object, parser.State, null);
            Assert.IsFalse(extractInterfaceCommand.CanExecute(null));
        }

        [TestMethod]
        public void CanExecuteNameCollision_ActiveCodePane_EmptyClass()
        {
            var input = @"
Sub Foo()
End Sub
";
            var builder = new MockVbeBuilder();
            var proj1 = builder.ProjectBuilder("TestProj1", ProjectProtection.Unprotected)
                               .AddComponent("Class1", ComponentType.ClassModule, input, Selection.Home)
                               .Build();
            var proj2 = builder.ProjectBuilder("TestProj2", ProjectProtection.Unprotected)
                               .AddComponent("Class1", ComponentType.ClassModule, string.Empty, Selection.Home)
                               .Build();

            var vbe = builder
                .AddProject(proj1)
                .AddProject(proj2)
                .Build();

            vbe.Object.ActiveCodePane = proj1.Object.VBComponents[0].CodeModule.CodePane;
            if (string.IsNullOrEmpty(vbe.Object.ActiveCodePane.CodeModule.Content()))
            {
                Assert.Inconclusive("The active code pane should be the one with the method stub.");
            }

            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }
            var extractInterfaceCommand = new RefactorExtractInterfaceCommand(vbe.Object, parser.State, null);
            Assert.IsTrue(extractInterfaceCommand.CanExecute(null));
        }

        [TestMethod]
        public void ExtractInterface_CanExecute_ClassWithMembers_SameNameAsClassWithMembers()
        {
            var input =
@"Sub foo()
End Sub";

            var builder = new MockVbeBuilder();
            var proj1 = builder.ProjectBuilder("TestProj1", ProjectProtection.Unprotected).AddComponent("Comp1", ComponentType.ClassModule, input, Selection.Home).Build();
            var proj2 = builder.ProjectBuilder("TestProj2", ProjectProtection.Unprotected).AddComponent("Comp1", ComponentType.ClassModule, "").Build();

            var vbe = builder
                .AddProject(proj1)
                .AddProject(proj2)
                .Build();

            vbe.Setup(s => s.ActiveCodePane).Returns(proj1.Object.VBComponents[0].CodeModule.CodePane);

            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var extractInterfaceCommand = new RefactorExtractInterfaceCommand(vbe.Object, parser.State, null);
            Assert.IsTrue(extractInterfaceCommand.CanExecute(null));
        }

        [TestMethod]
        public void ExtractInterface_CanExecute_Proc()
        {
            var input =
@"Sub foo()
End Sub";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleModule(input, ComponentType.ClassModule, out component, Selection.Home);
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var extractInterfaceCommand = new RefactorExtractInterfaceCommand(vbe.Object, parser.State, null);
            var canExecute = extractInterfaceCommand.CanExecute(null);
            Assert.IsTrue(canExecute);
        }

        [TestMethod]
        public void ExtractInterface_CanExecute_Function()
        {
            var input =
@"Function foo() As Integer
End Function";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleModule(input, ComponentType.ClassModule, out component, Selection.Home);
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var extractInterfaceCommand = new RefactorExtractInterfaceCommand(vbe.Object, parser.State, null);
            Assert.IsTrue(extractInterfaceCommand.CanExecute(null));
        }

        [TestMethod]
        public void ExtractInterface_CanExecute_PropertyGet()
        {
            var input =
@"Property Get foo() As Boolean
End Property";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleModule(input, ComponentType.ClassModule, out component, Selection.Home);
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var extractInterfaceCommand = new RefactorExtractInterfaceCommand(vbe.Object, parser.State, null);
            Assert.IsTrue(extractInterfaceCommand.CanExecute(null));
        }

        [TestMethod]
        public void ExtractInterface_CanExecute_PropertyLet()
        {
            var input =
@"Property Let foo(value)
End Property";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleModule(input, ComponentType.ClassModule, out component, Selection.Home);
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var extractInterfaceCommand = new RefactorExtractInterfaceCommand(vbe.Object, parser.State, null);
            Assert.IsTrue(extractInterfaceCommand.CanExecute(null));
        }

        [TestMethod]
        public void ExtractInterface_CanExecute_PropertySet()
        {
            var input =
@"Property Set foo(value)
End Property";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleModule(input, ComponentType.ClassModule, out component, Selection.Home);
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var extractInterfaceCommand = new RefactorExtractInterfaceCommand(vbe.Object, parser.State, null);
            Assert.IsTrue(extractInterfaceCommand.CanExecute(null));
        }

        [TestMethod]
        public void ImplementInterface_CanExecute_NullActiveCodePane()
        {
            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule("", out component);
            vbe.Setup(v => v.ActiveCodePane).Returns((ICodePane)null);
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var implementInterfaceCommand = new RefactorImplementInterfaceCommand(vbe.Object, parser.State, null);
            Assert.IsFalse(implementInterfaceCommand.CanExecute(null));
        }

        [TestMethod]
        public void ImplementInterface_CanExecute_NonReadyState()
        {
            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule("", out component);
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }
            parser.State.SetStatusAndFireStateChanged(this, ParserState.ResolvedDeclarations);

            var implementInterfaceCommand = new RefactorImplementInterfaceCommand(vbe.Object, parser.State, null);
            Assert.IsFalse(implementInterfaceCommand.CanExecute(null));
        }

        [TestMethod]
        public void ImplementInterface_CanExecute_ImplementsInterfaceNotSelected()
        {
            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleModule("", ComponentType.ClassModule, out component, new Selection());
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var implementInterfaceCommand = new RefactorImplementInterfaceCommand(vbe.Object, parser.State, null);
            Assert.IsFalse(implementInterfaceCommand.CanExecute(null));
        }

        [TestMethod]
        public void ImplementInterface_CanExecute_ImplementsInterfaceSelected()
        {
            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject", ProjectProtection.Unprotected)
                .AddComponent("IClass1", ComponentType.ClassModule, "")
                .AddComponent("Class1", ComponentType.ClassModule, "Implements IClass1", Selection.Home)
                .Build();

            var vbe = builder.AddProject(project).Build();
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var implementInterfaceCommand = new RefactorImplementInterfaceCommand(vbe.Object, parser.State, null);
            Assert.IsTrue(implementInterfaceCommand.CanExecute(null));
        }

        [TestMethod]
        public void IntroduceField_CanExecute_NullActiveCodePane()
        {
            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule("", out component);
            vbe.Setup(v => v.ActiveCodePane).Returns((ICodePane)null);
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var msgbox = new Mock<IMessageBox>();
            var introduceFieldCommand = new RefactorIntroduceFieldCommand(vbe.Object, parser.State, msgbox.Object);
            Assert.IsFalse(introduceFieldCommand.CanExecute(null));
        }

        [TestMethod]
        public void IntroduceField_CanExecute_NonReadyState()
        {
            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule("", out component);
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }
            parser.State.SetStatusAndFireStateChanged(this, ParserState.ResolvedDeclarations);

            var msgbox = new Mock<IMessageBox>();
            var introduceFieldCommand = new RefactorIntroduceFieldCommand(vbe.Object, parser.State, msgbox.Object);
            Assert.IsFalse(introduceFieldCommand.CanExecute(null));
        }

        [TestMethod]
        public void IntroduceField_CanExecute_Field()
        {
            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule("Dim d As Boolean", out component, Selection.Home);
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var msgbox = new Mock<IMessageBox>();
            var introduceFieldCommand = new RefactorIntroduceFieldCommand(vbe.Object, parser.State, msgbox.Object);
            Assert.IsFalse(introduceFieldCommand.CanExecute(null));
        }

        [TestMethod]
        public void IntroduceField_CanExecute_LocalVariable()
        {
            var input =
@"Property Get foo() As Boolean
    Dim d As Boolean
End Property";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule(input, out component, new Selection(2, 10, 2, 10));
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var msgbox = new Mock<IMessageBox>();
            var introduceFieldCommand = new RefactorIntroduceFieldCommand(vbe.Object, parser.State, msgbox.Object);
            Assert.IsTrue(introduceFieldCommand.CanExecute(null));
        }

        [TestMethod]
        public void IntroduceParameter_CanExecute_NullActiveCodePane()
        {
            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule("", out component);
            vbe.Setup(v => v.ActiveCodePane).Returns((ICodePane)null);
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var msgbox = new Mock<IMessageBox>();
            var introduceParameterCommand = new RefactorIntroduceParameterCommand(vbe.Object, parser.State, msgbox.Object);
            Assert.IsFalse(introduceParameterCommand.CanExecute(null));
        }

        [TestMethod]
        public void IntroduceParameter_CanExecute_NonReadyState()
        {
            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule("", out component);
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }
            parser.State.SetStatusAndFireStateChanged(this, ParserState.ResolvedDeclarations);

            var msgbox = new Mock<IMessageBox>();
            var introduceParameterCommand = new RefactorIntroduceParameterCommand(vbe.Object, parser.State, msgbox.Object);
            Assert.IsFalse(introduceParameterCommand.CanExecute(null));
        }

        [TestMethod]
        public void IntroduceParameter_CanExecute_Field()
        {
            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule("Dim d As Boolean", out component, Selection.Home);
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var msgbox = new Mock<IMessageBox>();
            var introduceParameterCommand = new RefactorIntroduceParameterCommand(vbe.Object, parser.State, msgbox.Object);
            Assert.IsFalse(introduceParameterCommand.CanExecute(null));
        }

        [TestMethod]
        public void IntroduceParameter_CanExecute_LocalVariable()
        {
            var input =
@"Property Get foo() As Boolean
    Dim d As Boolean
End Property";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule(input, out component, new Selection(2, 10, 2, 10));
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var msgbox = new Mock<IMessageBox>();
            var introduceParameterCommand = new RefactorIntroduceParameterCommand(vbe.Object, parser.State, msgbox.Object);
            Assert.IsTrue(introduceParameterCommand.CanExecute(null));
        }

        [TestMethod]
        public void MoveCloserToUsage_CanExecute_NullActiveCodePane()
        {
            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule("", out component);
            vbe.Setup(v => v.ActiveCodePane).Returns((ICodePane)null);
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var moveCloserToUsageCommand = new RefactorMoveCloserToUsageCommand(vbe.Object, parser.State, null);
            Assert.IsFalse(moveCloserToUsageCommand.CanExecute(null));
        }

        [TestMethod]
        public void MoveCloserToUsage_CanExecute_NonReadyState()
        {
            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule("", out component);
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }
            parser.State.SetStatusAndFireStateChanged(this, ParserState.ResolvedDeclarations);

            var moveCloserToUsageCommand = new RefactorMoveCloserToUsageCommand(vbe.Object, parser.State, null);
            Assert.IsFalse(moveCloserToUsageCommand.CanExecute(null));
        }

        [TestMethod]
        public void MoveCloserToUsage_CanExecute_Field_NoReferences()
        {
            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule("Dim d As Boolean", out component, Selection.Home);
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var moveCloserToUsageCommand = new RefactorMoveCloserToUsageCommand(vbe.Object, parser.State, null);
            Assert.IsFalse(moveCloserToUsageCommand.CanExecute(null));
        }

        [TestMethod]
        public void MoveCloserToUsage_CanExecute_LocalVariable_NoReferences()
        {
            var input =
@"Property Get foo() As Boolean
    Dim d As Boolean
End Property";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule(input, out component, new Selection(2, 10, 2, 10));
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var moveCloserToUsageCommand = new RefactorMoveCloserToUsageCommand(vbe.Object, parser.State, null);
            Assert.IsFalse(moveCloserToUsageCommand.CanExecute(null));
        }

        [TestMethod]
        public void MoveCloserToUsage_CanExecute_Const_NoReferences()
        {
            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule("Private Const const_abc = 0", out component, Selection.Home);
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var moveCloserToUsageCommand = new RefactorMoveCloserToUsageCommand(vbe.Object, parser.State, null);
            Assert.IsFalse(moveCloserToUsageCommand.CanExecute(null));
        }

        [TestMethod]
        public void MoveCloserToUsage_CanExecute_Field()
        {
            var input =
@"Dim d As Boolean
Sub Foo()
    d = True
End Sub";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule(input, out component, new Selection(1, 5, 1, 5));
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var moveCloserToUsageCommand = new RefactorMoveCloserToUsageCommand(vbe.Object, parser.State, null);
            Assert.IsTrue(moveCloserToUsageCommand.CanExecute(null));
        }

        [TestMethod]
        public void MoveCloserToUsage_CanExecute_LocalVariable()
        {
            var input =
@"Property Get foo() As Boolean
    Dim d As Boolean
    d = True
End Property";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule(input, out component, new Selection(2, 10, 2, 10));
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var moveCloserToUsageCommand = new RefactorMoveCloserToUsageCommand(vbe.Object, parser.State, null);
            Assert.IsTrue(moveCloserToUsageCommand.CanExecute(null));
        }

        [TestMethod]
        public void MoveCloserToUsage_CanExecute_Const()
        {
            var input =
@"Private Const const_abc = 0
Sub Foo()
    Dim d As Integer
    d = const_abc
End Sub";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule(input, out component, new Selection(1, 17, 1, 17));
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var moveCloserToUsageCommand = new RefactorMoveCloserToUsageCommand(vbe.Object, parser.State, null);
            Assert.IsTrue(moveCloserToUsageCommand.CanExecute(null));
        }

        [TestMethod]
        public void RemoveParameters_CanExecute_NullActiveCodePane()
        {
            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule("", out component);
            vbe.Setup(v => v.ActiveCodePane).Returns((ICodePane)null);
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var removeParametersCommand = new RefactorRemoveParametersCommand(vbe.Object, parser.State, null);
            Assert.IsFalse(removeParametersCommand.CanExecute(null));
        }

        [TestMethod]
        public void RemoveParameters_CanExecute_NonReadyState()
        {
            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule("", out component);
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }
            parser.State.SetStatusAndFireStateChanged(this, ParserState.ResolvedDeclarations);

            var removeParametersCommand = new RefactorRemoveParametersCommand(vbe.Object, parser.State, null);
            Assert.IsFalse(removeParametersCommand.CanExecute(null));
        }

        [TestMethod]
        public void RemoveParameters_CanExecute_Event_NoParams()
        {
            const string input =
@"Public Event Foo()";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule(input, out component, new Selection(1, 16, 1, 16));
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var removeParametersCommand = new RefactorRemoveParametersCommand(vbe.Object, parser.State, null);
            Assert.IsFalse(removeParametersCommand.CanExecute(null));
        }

        [TestMethod]
        public void RemoveParameters_CanExecute_Proc_NoParams()
        {
            var input =
@"Sub foo()
End Sub";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule(input, out component, new Selection(1, 6, 1, 6));
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var removeParametersCommand = new RefactorRemoveParametersCommand(vbe.Object, parser.State, null);
            Assert.IsFalse(removeParametersCommand.CanExecute(null));
        }

        [TestMethod]
        public void RemoveParameters_CanExecute_Function_NoParams()
        {
            var input =
@"Function foo() As Integer
End Function";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule(input, out component, new Selection(1, 11, 1, 11));
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var removeParametersCommand = new RefactorRemoveParametersCommand(vbe.Object, parser.State, null);
            Assert.IsFalse(removeParametersCommand.CanExecute(null));
        }

        [TestMethod]
        public void RemoveParameters_CanExecute_PropertyGet_NoParams()
        {
            var input =
@"Property Get foo() As Boolean
End Property";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule(input, out component, new Selection(1, 16, 1, 16));
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var removeParametersCommand = new RefactorRemoveParametersCommand(vbe.Object, parser.State, null);
            Assert.IsFalse(removeParametersCommand.CanExecute(null));
        }

        [TestMethod]
        public void RemoveParameters_CanExecute_PropertyLet_OneParam()
        {
            var input =
@"Property Let foo(value)
End Property";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule(input, out component, new Selection(1, 16, 1, 16));
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var removeParametersCommand = new RefactorRemoveParametersCommand(vbe.Object, parser.State, null);
            Assert.IsFalse(removeParametersCommand.CanExecute(null));
        }

        [TestMethod]
        public void RemoveParameters_CanExecute_PropertySet_OneParam()
        {
            var input =
@"Property Set foo(value)
End Property";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule(input, out component, new Selection(1, 16, 1, 16));
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var removeParametersCommand = new RefactorRemoveParametersCommand(vbe.Object, parser.State, null);
            Assert.IsFalse(removeParametersCommand.CanExecute(null));
        }

        [TestMethod]
        public void RemoveParameters_CanExecute_Event_OneParam()
        {
            const string input =
@"Public Event Foo(value)";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule(input, out component, new Selection(1, 16, 1, 16));
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var removeParametersCommand = new RefactorRemoveParametersCommand(vbe.Object, parser.State, null);
            Assert.IsTrue(removeParametersCommand.CanExecute(null));
        }

        [TestMethod]
        public void RemoveParameters_CanExecute_Proc_OneParam()
        {
            var input =
@"Sub foo(value)
End Sub";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule(input, out component, new Selection(1, 6, 1, 6));
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var removeParametersCommand = new RefactorRemoveParametersCommand(vbe.Object, parser.State, null);
            Assert.IsTrue(removeParametersCommand.CanExecute(null));
        }

        [TestMethod]
        public void RemoveParameters_CanExecute_Function_OneParam()
        {
            var input =
@"Function foo(value) As Integer
End Function";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule(input, out component, new Selection(1, 11, 1, 11));
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var removeParametersCommand = new RefactorRemoveParametersCommand(vbe.Object, parser.State, null);
            Assert.IsTrue(removeParametersCommand.CanExecute(null));
        }

        [TestMethod]
        public void RemoveParameters_CanExecute_PropertyGet_OneParam()
        {
            var input =
@"Property Get foo(value) As Boolean
End Property";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule(input, out component, new Selection(1, 16, 1, 16));
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var removeParametersCommand = new RefactorRemoveParametersCommand(vbe.Object, parser.State, null);
            Assert.IsTrue(removeParametersCommand.CanExecute(null));
        }

        [TestMethod]
        public void RemoveParameters_CanExecute_PropertyLet_TwoParams()
        {
            var input =
@"Property Let foo(value1, value2)
End Property";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule(input, out component, new Selection(1, 16, 1, 16));
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var removeParametersCommand = new RefactorRemoveParametersCommand(vbe.Object, parser.State, null);
            Assert.IsTrue(removeParametersCommand.CanExecute(null));
        }

        [TestMethod]
        public void RemoveParameters_CanExecute_PropertySet_TwoParams()
        {
            var input =
@"Property Set foo(value1, value2)
End Property";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule(input, out component, new Selection(1, 16, 1, 16));
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var removeParametersCommand = new RefactorRemoveParametersCommand(vbe.Object, parser.State, null);
            Assert.IsTrue(removeParametersCommand.CanExecute(null));
        }

        [TestMethod]
        public void ReorderParameters_CanExecute_NullActiveCodePane()
        {
            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule("", out component);
            vbe.Setup(v => v.ActiveCodePane).Returns((ICodePane)null);
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var reorderParametersCommand = new RefactorReorderParametersCommand(vbe.Object, parser.State, null);
            Assert.IsFalse(reorderParametersCommand.CanExecute(null));
        }

        [TestMethod]
        public void ReorderParameters_CanExecute_NonReadyState()
        {
            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule("", out component);
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }
            parser.State.SetStatusAndFireStateChanged(this, ParserState.ResolvedDeclarations);

            var reorderParametersCommand = new RefactorReorderParametersCommand(vbe.Object, parser.State, null);
            Assert.IsFalse(reorderParametersCommand.CanExecute(null));
        }

        [TestMethod]
        public void ReorderParameters_CanExecute_Event_OneParam()
        {
            const string input =
@"Public Event Foo(value)";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule(input, out component, new Selection(1, 16, 1, 16));
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var reorderParametersCommand = new RefactorReorderParametersCommand(vbe.Object, parser.State, null);
            Assert.IsFalse(reorderParametersCommand.CanExecute(null));
        }

        [TestMethod]
        public void ReorderParameters_CanExecute_Proc_OneParam()
        {
            var input =
@"Sub foo(value)
End Sub";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule(input, out component, new Selection(1, 6, 1, 6));
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var reorderParametersCommand = new RefactorReorderParametersCommand(vbe.Object, parser.State, null);
            Assert.IsFalse(reorderParametersCommand.CanExecute(null));
        }

        [TestMethod]
        public void ReorderParameters_CanExecute_Function_OneParam()
        {
            var input =
@"Function foo(value) As Integer
End Function";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule(input, out component, new Selection(1, 11, 1, 11));
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var reorderParametersCommand = new RefactorReorderParametersCommand(vbe.Object, parser.State, null);
            Assert.IsFalse(reorderParametersCommand.CanExecute(null));
        }

        [TestMethod]
        public void ReorderParameters_CanExecute_PropertyGet_OneParam()
        {
            var input =
@"Property Get foo(value) As Boolean
End Property";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule(input, out component, new Selection(1, 16, 1, 16));
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var reorderParametersCommand = new RefactorReorderParametersCommand(vbe.Object, parser.State, null);
            Assert.IsFalse(reorderParametersCommand.CanExecute(null));
        }

        [TestMethod]
        public void ReorderParameters_CanExecute_PropertyLet_TwoParams()
        {
            var input =
@"Property Let foo(value1, value2)
End Property";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule(input, out component, new Selection(1, 16, 1, 16));
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var reorderParametersCommand = new RefactorReorderParametersCommand(vbe.Object, parser.State, null);
            Assert.IsFalse(reorderParametersCommand.CanExecute(null));
        }

        [TestMethod]
        public void ReorderParameters_CanExecute_PropertySet_TwoParams()
        {
            var input =
@"Property Set foo(value1, value2)
End Property";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule(input, out component, new Selection(1, 16, 1, 16));
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var reorderParametersCommand = new RefactorReorderParametersCommand(vbe.Object, parser.State, null);
            Assert.IsFalse(reorderParametersCommand.CanExecute(null));
        }

        [TestMethod]
        public void ReorderParameters_CanExecute_Event_TwoParams()
        {
            const string input =
@"Public Event Foo(value1, value2)";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule(input, out component, new Selection(1, 16, 1, 16));
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var reorderParametersCommand = new RefactorReorderParametersCommand(vbe.Object, parser.State, null);
            Assert.IsTrue(reorderParametersCommand.CanExecute(null));
        }

        [TestMethod]
        public void ReorderParameters_CanExecute_Proc_TwoParams()
        {
            var input =
@"Sub foo(value1, value2)
End Sub";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule(input, out component, new Selection(1, 6, 1, 6));
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var reorderParametersCommand = new RefactorReorderParametersCommand(vbe.Object, parser.State, null);
            Assert.IsTrue(reorderParametersCommand.CanExecute(null));
        }

        [TestMethod]
        public void ReorderParameters_CanExecute_Function_TwoParams()
        {
            var input =
@"Function foo(value1, value2) As Integer
End Function";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule(input, out component, new Selection(1, 11, 1, 11));
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var reorderParametersCommand = new RefactorReorderParametersCommand(vbe.Object, parser.State, null);
            Assert.IsTrue(reorderParametersCommand.CanExecute(null));
        }

        [TestMethod]
        public void ReorderParameters_CanExecute_PropertyGet_TwoParams()
        {
            var input =
@"Property Get foo(value1, value2) As Boolean
End Property";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule(input, out component, new Selection(1, 16, 1, 16));
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var reorderParametersCommand = new RefactorReorderParametersCommand(vbe.Object, parser.State, null);
            Assert.IsTrue(reorderParametersCommand.CanExecute(null));
        }

        [TestMethod]
        public void ReorderParameters_CanExecute_PropertyLet_ThreeParams()
        {
            var input =
@"Property Let foo(value1, value2, value3)
End Property";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule(input, out component, new Selection(1, 16, 1, 16));
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var reorderParametersCommand = new RefactorReorderParametersCommand(vbe.Object, parser.State, null);
            Assert.IsTrue(reorderParametersCommand.CanExecute(null));
        }

        [TestMethod]
        public void ReorderParameters_CanExecute_PropertySet_ThreeParams()
        {
            var input =
@"Property Set foo(value1, value2, value3)
End Property";

            var builder = new MockVbeBuilder();
            IVBComponent component;
            var vbe = builder.BuildFromSingleStandardModule(input, out component, new Selection(1, 16, 1, 16));
            var mockHost = new Mock<IHostApplication>();
            mockHost.SetupAllProperties();
            var parser = MockParser.Create(vbe.Object, new RubberduckParserState(new Mock<ISinks>().Object));

            parser.Parse(new CancellationTokenSource());
            if (parser.State.Status >= ParserState.Error)
            {
                Assert.Inconclusive("Parser Error");
            }

            var reorderParametersCommand = new RefactorReorderParametersCommand(vbe.Object, parser.State, null);
            Assert.IsTrue(reorderParametersCommand.CanExecute(null));
        }
    }
}